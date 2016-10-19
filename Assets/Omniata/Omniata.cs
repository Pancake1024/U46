using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Threading;

#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{
    public class Omniata
    {
        private const string TAG = "Omniata";

        private const string SDK_VERSION = "unity-1.2.1";

        // Seconds to timeout the HTTP (TCP) connection
        public const int TCP_TIMEOUT = 120;

        // Max time to wait between event retries.
        private const int MAX_WAIT_TIME = 64;
        // Minimum time between events.
        private const int MIN_WAIT_TIME = 1;
        // Maximum number of times to retry event sending in case the sending fails.
        private const int MAX_RETRIES = 50;

        // Event parameter names constants
        public const string EVENT_PARAM_API_KEY = "api_key";
        public const string EVENT_PARAM_CURRENCY_CODE = "currency_code";
        public const string EVENT_PARAM_EVENT_TYPE = "om_event_type";
        public const string EVENT_PARAM_TOTAL = "total";
        public const string EVENT_PARAM_UID = "uid";
        public const string EVENT_PARAM_OM_DELTA = "om_delta";
        public const string EVENT_PARAM_OM_DEVICE = "om_device";
        public const string EVENT_PARAM_OM_PLATFORM = "om_platform";
        public const string EVENT_PARAM_OM_OS_VERSION = "om_os_version";
        public const string EVENT_PARAM_OM_SDK_VERSION = "om_sdk_version";
		public const string EVENT_PARAM_OM_RETRY = "om_retry";
		public const string EVENT_PARAM_OM_DISCARDED = "om_discarded";

        // Event type constants
        public const string EVENT_TYPE_OM_LOAD = "om_load";
        public const string EVENT_TYPE_OM_REVENUE = "om_revenue";
        public const string PLATFORM_ANDROID = "android";
        public const string PLATFORM_IOS = "ios";
        public const string PLATFORM_WP8 = "wp8";

        // The user-defined configuration
        internal string ApiKey { get; set; }
        internal string UserID { get; set; }
        internal bool Debug { get; set; }
        internal bool AutomaticParametersEnabled { get; set; }
        internal Reachability Reachability { get; set; }

        internal bool Initialized { get; set; }
        internal bool UseSSL { get; set; }

        private Queue EventQueue { get; set; }
        private Network Network { get; set; }

        private OmniataComponent OmniataComponent { get; set; }

        private bool Sending { get; set; }

		private int Discarded { get; set; }

        private QueueElement fetchedElement = null;
        //private Thread fetchThread = null;
        private ManualResetEvent fetchEvent = new ManualResetEvent(false);
        private bool fetchStarted = false;

        public Omniata(OmniataComponent omniataComponent, bool useSSL)
        {
            UseSSL = useSSL;
            OmniataComponent = omniataComponent;

            EventQueue = new Queue(omniataComponent.PersistentDataPath());
            Network = new Network(omniataComponent);

            Sending = false;

			Discarded = EventQueue.Storage.ReadDiscarded();
        }

        public void ProcessEvents()
        {
            if (Sending || !Reachability.Reachable())
            {
                return;
            }

            if (!fetchStarted)//null == fetchThread)
            {
                fetchEvent.Reset();
                ThreadPool.QueueUserWorkItem((o) =>
                //fetchThread = new Thread(() =>
                {
                    lock (EventQueue)
                    {
                        if (EventQueue.Count() == 0)
                        {
                            fetchEvent.Set();
                            return;
                        }
                        fetchedElement = EventQueue.Peek();
                    }
                    fetchEvent.Set();
                });
                //fetchThread.Start();
                fetchStarted = true;
            }
            else if (fetchEvent.WaitOne(0))//fetchThread.Join(0))
            {
                if (fetchedElement != null)
                {
                    Log.Debug(TAG, "Sending " + fetchedElement.EventType + " event in the queue");
                    SendToEventAPI(fetchedElement, new EventAPINetworkResponseHandler(this, fetchedElement));
                }
                fetchedElement = null;
                //fetchThread = null;
                fetchStarted = false;
            }
/*
            lock (EventQueue)
            {
                if (EventQueue.Count() == 0)
                {
                    return;
                }
                Log.Debug(TAG, "Sending, " + EventQueue.Count() + " event(s) in the queue");
            }

            // Network unreachable -> cannot send anything
            if (!Reachability.Reachable())
            {
                return;
            }

            // Guaranteed to return a value since count > 0
            QueueElement element = null;
            lock (EventQueue)
            {
                element = EventQueue.Peek();
            }

            SendToEventAPI(element, new EventAPINetworkResponseHandler(this, element));*/
        }

        public void Track(string eventType, Dictionary<string, string> parameters)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Uninitialized");
            }

            // Copy not to mess up the originals
            Dictionary<string, string> parametersCopy = new Dictionary<string, string>(parameters);

            parametersCopy.Add(EVENT_PARAM_EVENT_TYPE, eventType);
            parametersCopy.Add(EVENT_PARAM_API_KEY, ApiKey);
            parametersCopy.Add(EVENT_PARAM_UID, UserID);

            QueueElement element = new QueueElement(eventType, Utils.SecondsSinceEpoch(), 1, parametersCopy);
            //EventQueue.Put(element);
            /*new Thread(() => {
                lock (EventQueue)
                {
                    EventQueue.Put(element);
                }
            }).Start();*/
            ThreadPool.QueueUserWorkItem((o) =>
            {
                lock (EventQueue)
                {
                    EventQueue.Put(element);
                }
            });
        }

        public void Channel(int channelId, NetworkResponseHandler networkResponseHandler)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Uninitialized");
            }

            SendToChannelAPI(channelId, networkResponseHandler);
        }

        public void AddAutomaticParameters(Dictionary<string, string> parameters)
        {
            if (!AutomaticParametersEnabled/* || RunningInEditor()*/)
            {
                return;
            }

            // PlatformName. Note only a subset of Unity platforms is supported.
            RuntimePlatform platform = Application.platform;
            string platformName = "";
            if (platform == RuntimePlatform.Android)
            {
                platformName = PLATFORM_ANDROID;
            } else if (platform == RuntimePlatform.IPhonePlayer)
            {
                platformName = PLATFORM_IOS;
            } else if (platform == RuntimePlatform.WP8Player)
            {
                platformName = PLATFORM_WP8;
            } else
            {
                platformName = "unknown";
            }
            parameters.Add(EVENT_PARAM_OM_PLATFORM, platformName);

            parameters.Add(EVENT_PARAM_OM_DEVICE, OmniataComponent.GetDevice());
            parameters.Add(EVENT_PARAM_OM_OS_VERSION, OmniataComponent.GetOsVersion());

            parameters.Add(EVENT_PARAM_OM_SDK_VERSION, SDK_VERSION);
        }

        private bool RunningInEditor()
        {
            return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor;
        }
                        
        void SendToEventAPI(QueueElement element, NetworkResponseHandler networkResponseHandler)
        {
            string url = Utils.GetEventAPI(UseSSL, Debug) + "?" + element.WireFormat(Discarded);

            Sending = true;
            Network.Send(url, networkResponseHandler);
        }

        void SendToChannelAPI(int channelId, NetworkResponseHandler networkResponseHandler)
        {
            string url = Utils.GetChannelAPI(UseSSL, Debug) + "?" + "api_key=" + ApiKey + "&uid=" + WWW.EscapeURL(UserID) + "&channel_id=" + channelId;

            Network.Send(url, networkResponseHandler);
        }

        internal void EventSendSucceeded(QueueElement element)
        {
            // Event succeeded, reset WaitTime
            /*new Thread(() =>
            {
                lock (EventQueue)
                {
                    EventQueue.Take();
                    OmniataComponent.WaitTime = MIN_WAIT_TIME;

                    Sending = false;
                }
            }).Start();*/
            ThreadPool.QueueUserWorkItem((o) =>
            {
                lock (EventQueue)
                {
                    EventQueue.Take();
                    OmniataComponent.WaitTime = MIN_WAIT_TIME;

                    Sending = false;
                }
            });

            OmniataComponent.gameObject.SendMessage("OnOmniataEvent", element.EventType, SendMessageOptions.DontRequireReceiver);
        }

        internal void EventSendFailed(QueueElement element, int responseCode)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            //new Thread(() =>
            {
                lock (EventQueue)
                {
                    // +1 for retries
                    int retries = element.Retries + 1;

                    // 0 -> no response code received
                    if (responseCode != 0 && responseCode > 400)
                    {
                        // Only in this case consider retry

                        if (element.Retries > MAX_RETRIES)
                        {
                            // Retried too many times
                            EventQueue.Take();
                            OmniataComponent.WaitTime = MIN_WAIT_TIME;

                            Discarded++;
                            EventQueue.Storage.WriteDiscarded(Discarded);

                            Log.Error(TAG, "Discarding event after " + MAX_RETRIES + " retries");
                        }
                        else
                        {
                            // Retry
                            // Double (or gap) wait time
                            int waitTime = Math.Min(MAX_WAIT_TIME, OmniataComponent.WaitTime * 2);
                            OmniataComponent.WaitTime = waitTime;
                            Log.Debug(TAG, "Retrying event for the time: " + element.Retries + ", waitTime: " + waitTime);

                            // Update the retry count in the entry in the persistent queue
                            QueueElement elementFromQueue = EventQueue.Take();
                            elementFromQueue.Retries = retries;
                            EventQueue.Prepend(elementFromQueue);
                        }
                    }
                    else
                    {
                        EventQueue.Take();
                        OmniataComponent.WaitTime = MIN_WAIT_TIME;
                        Log.Error(TAG, "Discarding event after responseCode: " + responseCode);
                    }

                    Sending = false;
                }
            }//).Start();
            );
        }
    }
}

#endif