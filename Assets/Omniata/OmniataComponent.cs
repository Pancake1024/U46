using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{
    /**
     * The Unity-facing class of Omniata SDK.
     * OmniataComponent is the only MonoBehaviour in the SDK
     * and the only integration point between a Unity application
     * and the SDK.
     * 
     * Note event sending is asynchronous, i.e. Track and other
     * methods that send events actually just add the event to a queue,
     * which is later flushed to the server.
     */
    public class OmniataComponent : MonoBehaviour
    {
        public bool ProcessEvents { get; set; }

        private const string TAG = "OmniataComponent";

        internal int WaitTime { get; set; }

        private const bool USE_SSL = false;

        private bool isRunning { get; set; }

        private Omniata OmniataInstance { get ; set; }

        void Awake()//Start()
        {
            WaitTime = 1;
            ProcessEvents = true;
            OmniataInstance = new Omniata(this, USE_SSL);
        }

        /*
         * No need to be called for each frame, Run() is used instead
         * to more precisely control yielding.
        /*
        void Update()
        {
        }
        */  

        internal IEnumerator Run()
        {
            // TODO: Need a way to stop the loop at some point(?)
            while (true)
            {
                if (ProcessEvents)
                    OmniataInstance.ProcessEvents();
                yield return new WaitForSeconds(WaitTime);
            }
        }

        /**
         * Calls apiKey, userID, false, true, new DefaultReachability()
         */
        public void Initialize(string apiKey, string userID)
        {
            Initialize(apiKey, userID, false, true, new DefaultReachability());
        }

        /**
         * Sets up the given parameters and starts the background sending 
         * of events (if not started before). If called multiple times
         * sets the parameters, but doesn't do anything with the background
         * sending since that's already running.
         * 
         * Needs to be called before any other methods are called.
         */
        public void Initialize(string apiKey, string userID, bool debug, bool automaticParametersEnabled, Reachability reachability)
        {
            OmniataInstance.ApiKey = apiKey;
            OmniataInstance.UserID = userID;
            OmniataInstance.Debug = debug;
            OmniataInstance.AutomaticParametersEnabled = automaticParametersEnabled;
            OmniataInstance.Reachability = reachability;
            OmniataInstance.Initialized = true;

            if (!isRunning)
            {
                StartCoroutine(Run());
                isRunning = true;
            }
            Log.Debug(TAG, "initialized. ApiKey: " + OmniataInstance.ApiKey + ", UserID: " + OmniataInstance.UserID + ", Debug: " + OmniataInstance.Debug + ", AutomaticParametersEnabled: " + OmniataInstance.AutomaticParametersEnabled);
        }

        /**
         * Sets the LogLevel. The default is DEBUG.
         */
        public void LogLevel(LogLevel logLevel)
        {
            Log.LogLevel = logLevel;
        }

        /**
         * Track sends an event to Omniata.
         */
        public void Track(string eventType)
        {
            Track(eventType, new Dictionary<string, string>());
        }

        /**
         * Track sends an event with parameters to Omniata.
         */
        public void Track(string eventType, Dictionary<string, string> parameters)
        {
            OmniataInstance.Track(eventType, parameters);
        }

        /**
         * TrackLoad sends an om_load-event to Omniata.
         */
        public void TrackLoad()
        {
            TrackLoad(new Dictionary<string, string>());
        }
        
        /**
         * TrackLoad sends an om_load-event with parameters to Omniata.
         */
        public void TrackLoad(Dictionary<string, string> parameters)
        {
            // Copy not to mess up the originals
            Dictionary<string, string> parametersCopy = new Dictionary<string, string>(parameters);
            OmniataInstance.AddAutomaticParameters(parametersCopy);

            Track(Omniata.EVENT_TYPE_OM_LOAD, parametersCopy);
        }

        /**
         * TrackRevenue sends an om_revenue-event to Omniata.
         */
        public void TrackRevenue(decimal total, string currencyCode)
        {
            TrackRevenue(total, currencyCode, new Dictionary<string, string>());
        }
        
        /**
         * TrackRevenue sends an om_revenue-event with parameters to Omniata.
         */
        public void TrackRevenue(decimal total, string currencyCode, Dictionary<string, string> parameters)
        {
            // Copy not to mess up the originals
            Dictionary<string, string> parametersCopy = new Dictionary<string, string>(parameters);
            
            parametersCopy.Add(Omniata.EVENT_PARAM_TOTAL, Utils.DecimalToString(total));
            parametersCopy.Add(Omniata.EVENT_PARAM_CURRENCY_CODE, currencyCode);

            Track(Omniata.EVENT_TYPE_OM_REVENUE, parametersCopy);
        }

        /**
         * Fetches content for this user from a specific channel.
         */
        public void Channel(int channelId, NetworkResponseHandler networkResponseHandler)
        {
            OmniataInstance.Channel(channelId, networkResponseHandler);
        }

        /*
         * Helper methods so that other classes don't need to be MonoBehaviours -
         * UnityEngine.StartCoroutine etc can only be called from a MonoBehaviour.
         */
        internal void StartCoroutine(Coroutine coroutine)
        {
            StartCoroutine(coroutine);
        }

        internal string PersistentDataPath()
        {
            return Application.persistentDataPath;
        }

        /*
         * Native code wrapping.
         */
        internal string GetOsVersion()
        {
            return SystemInfo.operatingSystem;
        }
      
        internal string GetDevice()
        {
            return SystemInfo.deviceModel;
        }
    }
}


#endif
