using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace McSocialApiUtils
{
    public enum ErrorCode
    {
        Ok,
        InvalidToken,
        BadRequest,
        InvalidKey,
        TooManyTokens,
        BadBacon,
        /*UserNotFound,
        IdNotFound,
        RecipientNotFound,*/
        MaxQuotaExceeded,
        MessageNotFound,
        /*NotEligible,*/
        Undefined
    }

    public static class ErrorCodeUtils
    {

        public static ErrorCode ErrorStringToErrorCode(string errorString)
        {
            switch (errorString)
            {
                case "ok":
                    return ErrorCode.Ok;

                case "invalid_token":
                    return ErrorCode.InvalidToken;

                case "bad_request":
                    return ErrorCode.BadRequest;

                case "invalid_key":
                    return ErrorCode.InvalidKey;

                case "too_many_tokens":
                    return ErrorCode.TooManyTokens;

                case "bad_bacon":
                    return ErrorCode.BadBacon;

                /*case "user_not_found":
                    return ErrorCode.UserNotFound;

                case "id_not_found":
                    return ErrorCode.IdNotFound;

                case "recipient_not_found":
                    return ErrorCode.RecipientNotFound;*/

                case "max_quota_exceeded":
                    return ErrorCode.MaxQuotaExceeded;

                case "message_not_found":
                    return ErrorCode.MessageNotFound;

                /*// The following are missing from the documentation (ver 0.6)
                case "not_eligible":
                    return ErrorCode.NotEligible;*/

                default:
                    Debug.LogError("Undefined error code for string " + errorString);
                    return ErrorCode.Undefined;
            }
        }
    }

    public enum LoginType
    {
        Facebook,
        Google,
        Guest
    }

    public struct LoginData
    {
        string id;
        string name;
        string token;
        long expires;
        LoginType type;

        public string Id { get { return id; } }
        public string Name { get { return name; } }
        public string Token { get { return token; } }
        public long Expires { get { return expires; } }
        public LoginType Type { get { return type; } }

        public LoginData(string id, string name, string token, long expires, LoginType type)
        {
            this.id = id;
            this.name = name;
            this.token = token;
            this.expires = expires;
            this.type = type;
        }

        public override string ToString()
        {
            return "id: " + id + "\n" +
                   "name: " + name + "\n" +
                   "token: " + token + "\n" +
                   "expires: " + expires + "\n" + 
                   "type: " + type;
        }

        public void RefreshToken(string newToken)
        {
            token = newToken;
        }

        public static LoginData InvalidLogin()
        {
            return new LoginData(string.Empty, string.Empty, string.Empty, -1, LoginType.Guest);
        }

        public void SaveGuestToPlayerPrefs()
        {
            if (type != LoginType.Guest)
                return;

            EncryptedPlayerPrefs.SetString("Guest_Id", id);
            EncryptedPlayerPrefs.SetString("Guset_Name", name);
            EncryptedPlayerPrefs.SetString("Guest_Token", token);
            EncryptedPlayerPrefs.SetString("Guest_Expires", expires.ToString());
        }

        public static bool GuestLoginDataIsSaved()
        {
            return !string.IsNullOrEmpty(EncryptedPlayerPrefs.GetString("Guest_Token", null));
        }

        public static LoginData LoadPlayerPrefsGuest()
        {
            if (!GuestLoginDataIsSaved())
                return InvalidLogin();

            string guestToken = EncryptedPlayerPrefs.GetString("Guest_Token", null);
            string guestId = EncryptedPlayerPrefs.GetString("Guest_Id", null);
            string guestName = EncryptedPlayerPrefs.GetString("Guset_Name", null);
            long guestExpires;
            if (!long.TryParse(EncryptedPlayerPrefs.GetString("Guest_Expires", null), out guestExpires))
                guestExpires = -1;

            return new LoginData(guestId, guestName, guestToken, guestExpires, LoginType.Guest);
        }

        public static void ClearPlayerPrefsGuest()
        {
            EncryptedPlayerPrefs.DeleteKey("Guest_Id");
            EncryptedPlayerPrefs.DeleteKey("Guset_Name");
            EncryptedPlayerPrefs.DeleteKey("Guest_Token");
            EncryptedPlayerPrefs.DeleteKey("Guest_Expires");
        }
    }

    public struct FriendData
    {
        string id;
        LoginType loginType;
        string loginTypeId;
        string name;

        public string Id { get { return id; } }
        public LoginType Type { get { return loginType; } }
        public string LoginTypeId { get { return loginTypeId; } }
        public string Name { get { return name; } }

        public FriendData(string id, LoginType loginType, string loginTypeId, string name)
        {
            this.id = id;
            this.loginType = loginType;
            this.loginTypeId = loginTypeId;
            this.name = name;
        }

        public override string ToString()
        {
            return "id: " + id + "\n" +
                   "loginType: " + loginType.ToString() + "\n" +
                   "loginTypeId: " + loginTypeId + "\n" +
                   "name: " + name;
        }
    }

    public enum ScoreType
    {
        Latest,
        Weekly,
        Monthly
    }

    public enum ScoreFilter
    {
        Friends,
        Country,
        Global
    }

    public class ScoreData
    {
        string id;
        string name;
        string country;
        long score;
        long rank;
        LoginType loginType;
        string loginTypeId;

        public string Id { get { return id; } }
        public string Name { get { return name; } }
        public string Country { get { return country; } }
        public long Score { get { return score; } }
        public long Rank { get { return rank; } }
        public LoginType LoginType { get { return loginType; } }
        public string LoginTypeId { get { return loginTypeId; } }

        public ScoreData(string id, string name, string country, long score, long rank, LoginType loginType, string loginTypeId)
        {
            this.id = id;
            this.name = name;
            this.country = country;
            this.score = score;
            this.rank = rank;
            this.loginType = loginType;
            this.loginTypeId = loginTypeId;
        }

        public void SetRank(int newRank)
        {
            this.rank = newRank;
        }

        public override string ToString()
        {
            return "id: " + id + "\n" +
                   "name: " + name + "\n" +
                   "country: " + country + "\n" +
                   "score: " + score + "\n" +
                   "rank: " + rank + "\n" +
                   "loginType: " + loginType + "\n" +
                   "loginTypeId: " + loginTypeId;
        }
    }
    
    public struct DataToStore
    {
        string name;
        object value;
        bool isPublic;

        public string Name { get { return name; } }
        public object Value { get { return value; } }
        public bool IsPublic { get { return isPublic; } }

        public DataToStore(string name, object value, bool isPublic)
        {
            this.name = name;
            this.value = value;
            this.isPublic = isPublic;
        }

        public override string ToString()
        {
            return "name: " + name + "\n" +
                   "value: " + (value != null ? value.ToString() : "null_value") + "\n" +
                   "isPublic: " + isPublic;
        }
    }

    public class RetrievedData
    {
        string userId;
        List<DataToStore> data;

        public string UserId { get { return userId; } }
        public List<DataToStore> Data { get { return data; } }


        public RetrievedData(string userId, List<DataToStore> data)
        {
            this.userId = userId;
            this.data = data;
        }

        public override string ToString()
        {
            string dataString = string.Empty;
            for (int i = 0; i < data.Count; i++)
            {
                dataString += data[i].ToString();
                if (i < data.Count - 1)
                    dataString += "\n\n";
            }

            return "userId: " + userId + "\n\n" + dataString;
        }
    }

    public class ReceivedNotification
    {
        string messageId;
        string senderId;
        string senderName;
        long sendDate;
        string contentType;
        string payload;

        Sprite senderPicture;

        public string MessageId { get { return messageId; } }
        public string SenderId { get { return senderId; } }
        public string SenderName { get { return senderName; } }
        public long SendDate { get { return sendDate; } }
        public string ContentType { get { return contentType; } }
        public string Payload { get { return payload; } }
        public Sprite SenderPicture { get { return senderPicture; } }

        public ReceivedNotification(string messageId, string senderId, string senderName, long sendDate, string contentType, string payload)
        {
            this.messageId = messageId;
            this.senderId = senderId;
            this.senderName = senderName;
            this.sendDate = sendDate;
            this.contentType = contentType;
            this.payload = payload;
            this.senderPicture = null;
        }

        public void SetSenderName(string name)
        {
            senderName = name;
        }

        public void SetSenderPicture(Sprite picture)
        {
            senderPicture = picture;
        }

        public override string ToString()
        {
            return "messageId: " + messageId + "\n" +
                   "senderId: " + senderId + "\n" +
                   "senderName: " + senderName + "\n" +
                   "sendDate: " + sendDate + "\n" +
                   "contentType: " + contentType + "\n" +
                   "payload: " + payload;
        }
    }

    public class ScoreRequest
    {
        public bool showTopScores;
        public bool showFriends;
        public ScoreType scoresType;
        public long spread;
        public string leaderboard;
        public string country = "";
        
        public ScoreRequest(bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string leaderboard, string country)
        {
            this.showTopScores = showTopScores;
            this.showFriends = showFriends;
            this.scoresType = scoresType;
            this.spread = spread;
            this.leaderboard = leaderboard;
            this.country = country;
        }
    }
}