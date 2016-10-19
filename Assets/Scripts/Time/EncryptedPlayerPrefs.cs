using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using System.Globalization;

public class EncryptedPlayerPrefs
{
    private static BlowFishCS.BlowFish bfForKey = new BlowFishCS.BlowFish("76BB0032D45882BF");
    private static string keyString = "3ee29abd3025d013";
    private static BlowFishCS.BlowFish bfForData = null;
    private static Dictionary<string, string> encodedKeys = new Dictionary<string, string>();

    //converts a single hex character to it's decimal value
    private static byte GetHex(char x)
    {
        if (x <= '9' && x >= '0')
        {
            return (byte)(x - '0');
        }
        else if (x <= 'z' && x >= 'a')
        {
            return (byte)(x - 'a' + 10);
        }
        else if (x <= 'Z' && x >= 'A')
        {
            return (byte)(x - 'A' + 10);
        }
        return 0;
    }

    //converts a byte array to a hex string
    private static string BytesToHex(byte[] bytes)
    {
        StringBuilder s = new StringBuilder();
        foreach (byte b in bytes)
            s.Append(b.ToString("x2"));
        return s.ToString();
    }

    //converts a hex string to a byte array
    private static byte[] HexToBytes(string hex)
    {
        byte[] r = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length - 1; i += 2)
        {
            byte a = GetHex(hex[i]);
            byte b = GetHex(hex[i + 1]);
            r[i / 2] = (byte)(a * 16 + b);
        }
        return r;
    }

    //create a random key and saves it (encrypted with static key)
    private static void Initialize()
    {
        if (null == bfForData)
        {
            string key = PlayerPrefs.GetString(keyString, string.Empty);
            if (0 == key.Length)
            {
                System.Random rng = new System.Random();
                byte[] bytes = new byte[8];
                rng.NextBytes(bytes);
                byte[] encBytes = bfForKey.Encrypt_ECB(bytes);
                key = BytesToHex(encBytes);
                PlayerPrefs.SetString(keyString, key);
            }
            bfForData = new BlowFishCS.BlowFish(bfForKey.Decrypt_ECB(HexToBytes(key)));
        }
    }

    // returns encoded key (cached)
    private static string GetEncodedKey(string key)
    {
        string encKey = null;
        if (encodedKeys.TryGetValue(key, out encKey))
        {
            /*
            if (key.Equals("money"))
                Debug.Log("######################################### ########################  1GetEncodedKey key " + key + " -> " + encKey);
            */
            return encKey;
        }
        else
        {
            Initialize();
            encKey = bfForData.Encrypt_ECB(key);
            encodedKeys.Add(key, encKey);
            /*
            if (key.Equals("money"))
                Debug.Log("######################################### ########################  2GetEncodedKey key " + key + " -> " + encKey);
            */
            return encKey;
        }
    }

    public static void DeleteAll()
    {
        encodedKeys = new Dictionary<string, string>();
        bfForData = null;
        PlayerPrefs.DeleteAll();
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.DeleteKey(GetEncodedKey(key));
    }

    public static float GetFloat(string key, float defaultValue)
    {
        string encKey = GetEncodedKey(key);

        if (PlayerPrefs.HasKey(encKey))
        { // new data
            string input = PlayerPrefs.GetString(encKey, string.Empty);
            if (0 == input.Length)
                return defaultValue;
            byte[] output = bfForData.Decrypt_ECB(HexToBytes(input));
            return BitConverter.ToSingle(output, 0);
        }
        else if (PlayerPrefs.HasKey(key))
        { // old data
            float value = PlayerPrefs.GetFloat(key, defaultValue);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.SetString(encKey, BytesToHex(bfForData.Encrypt_ECB(BitConverter.GetBytes(value))));
            return value;
        }
        else
            return defaultValue;
    }

    public static int GetInt(string key, int defaultValue)
    {
        string encKey = GetEncodedKey(key);

        if (PlayerPrefs.HasKey(encKey))
        { // new data
            string input = PlayerPrefs.GetString(encKey, string.Empty);
            if (0 == input.Length)
                return defaultValue;
            byte[] output = bfForData.Decrypt_ECB(HexToBytes(input));
            return BitConverter.ToInt32(output, 0);
        }
        else if (PlayerPrefs.HasKey(key))
        { // old data
            int value = PlayerPrefs.GetInt(key, defaultValue);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.SetString(encKey, BytesToHex(bfForData.Encrypt_ECB(BitConverter.GetBytes(value))));
            return value;
        }
        else
            return defaultValue;
    }

    public static long GetLong(string key, long defaultValue)
    {
        string s = GetString(key, string.Empty);
        if (!string.IsNullOrEmpty(s))
        {
            long l;
            if (long.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out l))
                return l;
        }
        return defaultValue;
    }

    public static string GetString(string key, string defaultValue)
    {
        string encKey = GetEncodedKey(key);

        if (PlayerPrefs.HasKey(encKey))
        { // new data
            string input = PlayerPrefs.GetString(encKey, string.Empty);
            if (0 == input.Length)
                return defaultValue;
            return bfForData.Decrypt_ECB(input);
        }
        else if (PlayerPrefs.HasKey(key))
        { // old data
            string value = PlayerPrefs.GetString(key, defaultValue);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.SetString(encKey, bfForData.Encrypt_ECB(value));
            return value;
        }
        else
            return defaultValue;
    }

    public static DateTime GetUtcDate(string key)
    {
        string s = GetString(key, string.Empty);
        long l;
        if (!string.IsNullOrEmpty(s))
        {
            if (long.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out l))
                return new DateTime(l, DateTimeKind.Utc);
        }
        return new DateTime(0, DateTimeKind.Utc);
    }

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(GetEncodedKey(key));
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.SetString(GetEncodedKey(key), BytesToHex(bfForData.Encrypt_ECB(BitConverter.GetBytes(value))));
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.SetString(GetEncodedKey(key), BytesToHex(bfForData.Encrypt_ECB(BitConverter.GetBytes(value))));
    }

    public static void SetLong(string key, long value)
    {
        SetString(key, value.ToString(CultureInfo.InvariantCulture));
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.SetString(GetEncodedKey(key), bfForData.Encrypt_ECB(value));
    }

    public static void SetUtcDate(string key, DateTime dateTime)
    {
        EncryptedPlayerPrefs.SetString(key, dateTime.Ticks.ToString(CultureInfo.InvariantCulture));
    }
}
