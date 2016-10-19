
#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{
    public class EventAPINetworkResponseHandler : NetworkResponseHandler
    {
        private const string TAG = "EventAPINetworkResponseHandler";

        private Omniata Omniata { get; set; }
        private QueueElement QueueElement { get; set; }

        public EventAPINetworkResponseHandler(Omniata omniata, QueueElement queueElement)
        {
            Omniata = omniata;
            QueueElement = queueElement;
        }

        public void OnSuccess(string content, string url, int durationInMillis)
        {
            Log.Debug(TAG, "Success (in " + durationInMillis + "), url: " + url  + ", retries: " + QueueElement.Retries);

            Omniata.EventSendSucceeded(QueueElement);
        }

        /**
         * Called in case the request failed for some reason.
         */
        public void OnError(string message, int responseCode, string url, int durationInMillis)
        {
            Log.Error(TAG, "Event sending failed (in " + durationInMillis + "), message: " + message + ", responseCode: " + responseCode + ", url: " + url + ", retries: " + QueueElement.Retries);

            Omniata.EventSendFailed(QueueElement, responseCode);
        }
    }
}

#endif