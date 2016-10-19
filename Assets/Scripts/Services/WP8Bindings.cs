using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SBS.Miniclip
{
    public class WP8Bindings
    {
#if UNITY_WP8
        public const string RATE_IT_URL = "http://www.windowsphone.com/s?appId=48c10698-8953-4d5e-b979-2954cdf65f98";

        public class MoreGamesArgs : EventArgs
        {
            string url;

            public string Url { get { return url; } }

            public MoreGamesArgs(string url)
            {
                this.url = url;
            }
        }

        public class RateItEventArgs : EventArgs
        {
            string title;
            string text;
            string url;

            public string Title { get { return title; } }
            public string Text { get { return text; } }
            public string Url { get { return url; } }

            public RateItEventArgs(string title, string text, string rateItUrl)
            {
                this.title = title;
                this.text = text;
                this.url = rateItUrl;
            }
        }

        public class ExitEventArgs : EventArgs
        {
            string title;
            string text;

            public string Title { get { return title; } }
            public string Text { get { return text; } }

            public ExitEventArgs(string title, string text)
            {
                this.title = title;
                this.text = text;
            }
        }

        public delegate void EventHandler(EventArgs args);
        public static event EventHandler MoreGamesEvent = delegate { };
        public static event EventHandler RateItEvent = delegate { };
        public static event EventHandler ExitEvent = delegate { };

        private static bool storeLoaded = false;
        private static string versionNumber;

        public static string VersionNumber { get { return versionNumber; } }

        public static void OnMoreGames(string url)
        {
            MoreGamesEvent(new MoreGamesArgs(url));
        }

        public static void OnRateItShowPopup(string title, string text, string rateItUrl)
        {
            RateItEvent(new RateItEventArgs(title, text, rateItUrl));
        }

        public static void OnShowExitPopup(string title, string text)
        {
            ExitEvent(new ExitEventArgs(title, text));
        }

        public static void OnVersionNumberAvailable(string num)
        {
            versionNumber = num;
        }
#endif
    }
}
