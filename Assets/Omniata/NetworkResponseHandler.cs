namespace omniata
{
    public interface NetworkResponseHandler
    {
        /**
         * Called in case the request succeeded.
         */
        void OnSuccess(string content, string url, int durationInMillis);

        /**
         * Called in case the request failed for some reason.
         */
        void OnError(string message, int responseCode, string url, int durationInMillis);
    }
}

