using System;
using UnityEngine;

#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{	
	public class Log : MonoBehaviour
	{
		private static LogLevel logLevel = LogLevel.DEBUG;

		public static LogLevel LogLevel 
		{ 
			get {
					return logLevel; 
			}
			set {
					logLevel = value;
			}
		}

		public static void Debug(string tag, string message)
		{
            DoLog (LogLevel.DEBUG, tag, message);
		}

        public static void Info(string tag, string message)
		{
            DoLog (LogLevel.INFO, tag, message);
		}

        public static void Warn(string tag, string message)
		{
            DoLog (LogLevel.WARN, tag, message);
		}

        public static void Error(string tag, string message)
		{
            DoLog (LogLevel.ERROR, tag, message);
		}

        public static void Fatal(string tag, string message)
		{
			DoLog (LogLevel.FATAL, tag, message);
		}

		private static void DoLog(LogLevel logLevel, String tag, String msg) { 
			if (logLevel >= Log.LogLevel) {
                // It seems that Unity won't *sometimes* log an identical log message.
                // DateTime is there to make the log entries more unique.
                msg = DateTime.Now + " Omniata-" + tag + "-" + logLevel + ": " + msg;
				if (logLevel == LogLevel.DEBUG || logLevel == LogLevel.INFO) {
                    UnityEngine.Debug.Log(msg);
                } else if (logLevel == LogLevel.WARN) {
                    UnityEngine.Debug.LogWarning(msg);
				} else {
                    UnityEngine.Debug.LogError(msg);
				}
			}
		}
	}
}

#endif