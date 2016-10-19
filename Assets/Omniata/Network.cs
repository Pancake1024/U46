using System;
using System.Collections;
using UnityEngine;

#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{
    public delegate void SuccessHandler(string text, int ms);
    public delegate void ErrorHandler(string message, int ms);

    public class Network
    {
        private const string TAG = "Network";

        private OmniataComponent OmniataComponent { get; set; }

        public Network(OmniataComponent omniataComponent)
        {
            OmniataComponent = omniataComponent;
        }

        public void Send(string url, NetworkResponseHandler networkResponseHandler)
        {
			Log.Debug(TAG, "sending, URL: " + url);
			HttpRequest httpRequest = new HttpRequest(OmniataComponent, url, Omniata.TCP_TIMEOUT, networkResponseHandler);
			httpRequest.Get();
		}
    }
}

#endif
