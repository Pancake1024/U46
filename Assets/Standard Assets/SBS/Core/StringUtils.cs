using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Core
{
	public class StringUtils
	{
        public static string MillisecondsToTime(float time, bool printCents)
        {
            time = SBSMath.Max(time, 0.0f);

            int cents = Mathf.FloorToInt(time * 100.0f) % 100,
                secs = Mathf.FloorToInt(time) % 60,
                mins = Mathf.FloorToInt(time / 60.0f);
#if UNITY_FLASH
            if (printCents)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", mins, secs, cents);
            else
                return string.Format("{0:D2}:{1:D2}", mins, secs);
#else
            if (printCents)
                return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}", mins, secs, cents);
            else
                return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}", mins, secs);
#endif
        }

        public static string MillisecondsToHours(float time, bool printSeconds)
        {
            time = SBSMath.Max(time, 0.0f);

            int secs = Mathf.FloorToInt(time) % 60,
                mins = Mathf.FloorToInt(time / 60.0f),
                hours = Mathf.FloorToInt(mins / 60.0f);
            mins = mins % 60;
#if UNITY_FLASH
            if (printSeconds)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, mins, secs);
            else
                return string.Format("{0:D2}:{1:D2}", hours, mins);
#else
            if (printSeconds)
                return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}", hours, mins, secs);
            else
                return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}", hours, mins);
#endif
        }

        public static string FormatGroupSeparator(int number, string separator)
		{
#if UNITY_FLASH
            return number.ToString();//FLASH ToDo
#else
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberDecimalDigits = 0;
            nfi.NumberGroupSeparator = separator;
            return number.ToString("n", nfi);
#endif
        }
    }
}
