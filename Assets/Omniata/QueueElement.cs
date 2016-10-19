using System;
using System.Collections.Generic;
using UnityEngine;

#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{
    public class QueueElement
    {
        private const string TAG = "QueueElement";
        private static string META = "meta";
        private static string DATA = "data";
        private const string META_EVENT_TYPE = "event_type";
        private const string META_CREATION = "creation";
        private const string META_RETRIES = "retries";

        // Metadata
        public string EventType { get; private set; }

        private double CreationSecondsSinceEpoch { get; set; }

        public int Retries { get; set; }

        // Data
        private Dictionary<string, string> Parameters { get; set; }

		// Other
		private OmniataComponent OmniataComponent { get; set; }

        public QueueElement(string eventType, double creationSecondsSinceEpoch, int retries, Dictionary<string, string> parameters)
        {
            EventType = eventType;
            CreationSecondsSinceEpoch = creationSecondsSinceEpoch;
            Retries = retries;
            Parameters = parameters;
        }

        public string WireFormat(int omDiscarded)
        {
            List<string> elements = new List<string>();
            foreach (KeyValuePair<string, string> entry in Parameters)
            {
                elements.Add(WWW.EscapeURL(entry.Key) + '=' + WWW.EscapeURL(entry.Value));
            }

            // om_delta
            if (CreationSecondsSinceEpoch != 0)
            {
                double omDelta = Utils.SecondsSinceEpoch() - CreationSecondsSinceEpoch;
                elements.Add(Omniata.EVENT_PARAM_OM_DELTA + "=" + Utils.DoubleToIntegerString(omDelta));
            }

			if (Retries >= 1) {
				elements.Add(Omniata.EVENT_PARAM_OM_RETRY + "=" + Retries);
			}

			if (EventType == "om_load") {
				elements.Add(Omniata.EVENT_PARAM_OM_DISCARDED + "=" + omDiscarded);
			}

            return string.Join("&", elements.ToArray());
        }

        public string Serialize()
        {
            string metaValue = WWW.EscapeURL(META_EVENT_TYPE + "=" + WWW.EscapeURL(EventType) + "&" + META_CREATION + "=" + Utils.DoubleToIntegerString(CreationSecondsSinceEpoch) + "&" + META_RETRIES + "=" + Retries);

            List<string> elements = new List<string>();
            foreach (KeyValuePair<string, string> entry in Parameters)
            {
                elements.Add(WWW.EscapeURL(entry.Key) + '=' + WWW.EscapeURL(entry.Value));
            }

            return META + '=' + metaValue + '&' + DATA + '=' + WWW.EscapeURL(string.Join("&", elements.ToArray()));
        }

        public static QueueElement Deserialize(string serialized)
        {
            // serialized: meta=xxx&data=yyy
            string[] tokens = serialized.Split('&');

            string eventType = "";
            double creationSecondsSinceEpoch = 0;
            int retries = 0;
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (string token in tokens)
            {
                string[] keyAndValueURLEncoded = token.Split('=');
                string key = WWW.UnEscapeURL(keyAndValueURLEncoded [0]); // either META or DATA
                string value = WWW.UnEscapeURL(keyAndValueURLEncoded [1]);

                if (key.CompareTo(META) == 0)
                {
                    // value: metadata as keyvalue pairs
                    string[] tokens2 = value.Split('&');
                    foreach (string token2 in tokens2)
                    {
                        string[] keyAndValueURLEncoded2 = token2.Split('=');
                        string key2 = WWW.UnEscapeURL(keyAndValueURLEncoded2 [0]);
                        string value2 = WWW.UnEscapeURL(keyAndValueURLEncoded2 [1]);
                       
                        if (key2.CompareTo(META_EVENT_TYPE) == 0)
                        {
                            eventType = value2;
                        } else if (key2.CompareTo(META_CREATION) == 0)
                        {
                            creationSecondsSinceEpoch = Convert.ToDouble(value2);
                        } else if (key2.CompareTo(META_RETRIES) == 0)
                        {
                            retries = Convert.ToInt32(value2);
                        } else
                        {
                            Log.Error(TAG, "Unknown meta key: " + key2);
                        }
                    }
                } else if (key.CompareTo(DATA) == 0)
                {
                    // value: metadata as keyvalue pairs
                    string[] tokens2 = value.Split('&');
                    foreach (string token2 in tokens2)
                    {
                        string[] keyAndValueURLEncoded2 = token2.Split('=');
                        string key2 = WWW.UnEscapeURL(keyAndValueURLEncoded2 [0]);
                        string value2 = WWW.UnEscapeURL(keyAndValueURLEncoded2 [1]);

                        parameters.Add(key2, value2);
                    }
                }
            }

            if (eventType.CompareTo("") == 0)
            {
                Log.Error(TAG, "eventType not found when deserializing, skipping event");
                return null;
            }

            return new QueueElement(eventType, creationSecondsSinceEpoch, retries, parameters);
        }
    }
}

#endif