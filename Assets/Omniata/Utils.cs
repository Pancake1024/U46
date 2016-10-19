using System;
using System.Globalization;
using UnityEngine;

namespace omniata
{
	public class Utils
	{
		private static string API = "api.omniata.com";
		private static string TEST_API = "api-test.omniata.com";

		private static String GetProtocol(Boolean useSSL) {
			return useSSL ? "https://" : "http://";
		}
	
		public static String GetEventAPI(Boolean useSSL, Boolean debug)
		{
			if (debug) {
				return GetProtocol(useSSL) + TEST_API + "/event";
			} else {
				return GetProtocol(useSSL) + API + "/event";
			}
		}

        public static String GetChannelAPI(Boolean useSSL, Boolean debug)
        {
            if (debug) {
                return GetProtocol(useSSL) + /*TEST_API*/API + "/channel";
            } else {
                return GetProtocol(useSSL) + API + "/channel";
            }
        }     

        public static String DoubleToIntegerString(double value)
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }

        public static String DecimalToString(decimal value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        public static Double SecondsSinceEpoch() 
        {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
	}
}

