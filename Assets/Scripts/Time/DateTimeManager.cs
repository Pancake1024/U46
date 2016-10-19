using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using SBS.Core;
using System.Globalization;
using SBS_MiniJSON;
using McSocialApiUtils;

public class DateTimeManager : Manager<DateTimeManager>
{
	public static long TickCount
	{
		get
		{
			return (long)Mathf.Round(Time.realtimeSinceStartup * 1000.0f);
		}
	}

	#region Singleton Instance
    static public DateTimeManager Instance
    {
        get
        {
            return Manager<DateTimeManager>.Get();
        }
    }
    #endregion

    #region Public members
    public string requestURL = "http://api.miniclippt.com/v1/timestamp";    // "http://www.silentbaystudios.com/test/services/getTime.php";
    public long trustThreshold = 300; // default 5 minutes
    public bool resetDateOnRequestFail = false;
    #endregion

    #region Protected members
    protected DateTime UTCEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    protected const long ticksPerSecond = 1000;

    protected bool inRequestDateTime = false;
    protected bool stopRequestFlag = false;

    protected DateTime lastReqDateTime;
    protected bool lastReqSuccessful;

    protected long lastReqTicks;
    protected float halfRtt = 0.0f;
    #endregion

    #region Coroutines
    IEnumerator PrintDate()
    {
        DateTime now = DateTime.UtcNow;
        bool valid = false;
        long deltaSeconds;

        while (true)
        {
            yield return new WaitForSeconds(15.0f);

            this.GetDeltaSeconds(ref now, ref valid, out deltaSeconds);

            //Debug.Log("offset: " + (DateTime.UtcNow - now).TotalSeconds + ", valid: " + valid + ", deltaSeconds: " + deltaSeconds);
        }
    }

    IEnumerator RequestDateTime()
    {
        Asserts.Assert(!inRequestDateTime);
        inRequestDateTime = true;

        if (NetworkReachability.NotReachable == Application.internetReachability)
        {
            OnRequestFailure();
            inRequestDateTime = false;

            yield break;
        }

        long timestamp = TickCount;
        WWW req = new WWW(requestURL + "?nocache=" + Environment.TickCount);
        while (!req.isDone)
        {
            if (stopRequestFlag)
            {
                req.Dispose();
                
                inRequestDateTime = false;

                stopRequestFlag = false;

                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

        if (req.isDone && string.IsNullOrEmpty(req.error))
        {
            float rtt = (TickCount - timestamp) / (float)ticksPerSecond;
            halfRtt += (rtt * 0.5f - halfRtt) * 0.25f;

            /*long milliseconds;
            if (long.TryParse(req.text, NumberStyles.None, CultureInfo.InvariantCulture, out milliseconds))
            {
                lastReqDateTime = UTCEpoch.AddMilliseconds(milliseconds);
                lastReqSuccessful = true;
                lastReqTicks = TickCount;
            }*/

            Dictionary<string, object> responseDict = Json.Deserialize(req.text) as Dictionary<string, object>;
            ErrorCode errorCode = ErrorCodeUtils.ErrorStringToErrorCode((string)responseDict["error"]);
            if (errorCode == ErrorCode.Ok)
                OnRequestSuccess((long)responseDict["data"]);
            else
                OnRequestFailure();
        }
        else
            OnRequestFailure();

        inRequestDateTime = false;
        req.Dispose();
    }

    void OnRequestSuccess(long unixTimeStamp)
    {
        lastReqDateTime = UTCEpoch.AddSeconds(unixTimeStamp).ToUniversalTime();
        lastReqSuccessful = true;
		lastReqTicks = TickCount;

        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- DateTimeManager OnRequestSuccess");
		//Debug.Log ("--- --- DateTime - OnRequestSuccess - lastReqDateTime: " + lastReqDateTime + " - lastReqTicks: " + lastReqTicks);
    }

    void OnRequestFailure()
    {
        //Debug.Log ("--- --- DateTime - OnRequestFailure");
        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- DateTimeManager OnRequestFailure");
        if (resetDateOnRequestFail)
        {
            lastReqDateTime = DateTime.UtcNow;
            lastReqSuccessful = false;
            lastReqTicks = TickCount;
        }
    }

    IEnumerator RefreshDateTime(float deltaSeconds)
    {
        this.StartCoroutine(this.RequestDateTime());

        while (true)
        {
            yield return new WaitForSeconds(deltaSeconds);

            while (inRequestDateTime)
                yield return new WaitForSeconds(1.0f);

            this.StartCoroutine(this.RequestDateTime());
        }
    }

    IEnumerator ValidateDateTime(DateTime dateTime, float timeout, Action<bool> onValidationFinished)
    {
        float t = 0.0f;
        int counter = 0;
        while (t < timeout)
        {
            long timestamp = TickCount;

            yield return new WaitForSeconds(Mathf.Min(0.0f, 0.6f * (++counter) - t));

            if (inRequestDateTime)
            {
                while (inRequestDateTime)
                    yield return new WaitForEndOfFrame();
            }
            else
                yield return this.StartCoroutine(this.RequestDateTime());

            t += ((TickCount - timestamp) / (float)ticksPerSecond);
            timestamp = TickCount;

            if (lastReqSuccessful)
            {
                dateTime.AddSeconds(t);

                if (Mathf.Abs((float)(dateTime - lastReqDateTime).TotalSeconds) <= trustThreshold)
                    onValidationFinished(true);
                else
                    onValidationFinished(false);

                yield break;
            }

            t += ((TickCount - timestamp) / (float)ticksPerSecond);
        }

        onValidationFinished(false);
    }

    IEnumerator GetDateTimeAsync(DateTime dateTime, bool dateIsValid, Action<DateTime, bool, long> onFinished)
    {
        if (inRequestDateTime)
        {
            while (inRequestDateTime)
                yield return new WaitForEndOfFrame();
        }
        else
            yield return this.StartCoroutine(this.RequestDateTime());

        long deltaSeconds;
        bool isValid = this.GetDeltaSeconds(ref dateTime, ref dateIsValid, out deltaSeconds);
        onFinished(dateTime, isValid, deltaSeconds);
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        lastReqDateTime = DateTime.UtcNow;
        lastReqSuccessful = false;
		lastReqTicks = TickCount;

        this.StartCoroutine(this.RefreshDateTime(trustThreshold * 1.0f));

        //this.StartCoroutine(this.PrintDate());
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused && !inRequestDateTime)
            this.StartCoroutine(this.RequestDateTime());
    }
    #endregion

    #region Public methods
    public void GetDate(out DateTime now, out bool dateIsValid)
    {
        dateIsValid = lastReqSuccessful;

        long offsetSeconds = (((long)TickCount - lastReqTicks) / ticksPerSecond) - Mathf.RoundToInt(halfRtt);

        DateTime localUtcNow = DateTime.UtcNow,
                 netUtcNow   = lastReqDateTime.Add(TimeSpan.FromSeconds(offsetSeconds));
        if (Mathf.Abs((float)(localUtcNow - netUtcNow).TotalSeconds) <= trustThreshold)
            now = localUtcNow;
        else
            now = netUtcNow;
    }

    public bool GetDeltaSeconds(ref DateTime lastDate, ref bool lastDateWasValid, out long deltaSeconds)
    {
        bool wasDateValid = lastDateWasValid;

        /*
        if (inRequestDateTime)
        {
            stopRequestFlag = true;
            
            lastDateWasValid = false;

            deltaSeconds = (DateTime.UtcNow - lastDate).Seconds;
            lastDate = DateTime.UtcNow;

            return wasDateValid || lastDateWasValid;
        }*/

        lastDateWasValid = lastReqSuccessful;

        long offsetSeconds = (((long)TickCount - lastReqTicks) / ticksPerSecond) - Mathf.RoundToInt(halfRtt);

		//Debug.Log("--- --- DateTime - GetDeltaSeconds - offsetSeconds: " + offsetSeconds);

        DateTime localUtcNow = DateTime.UtcNow,
                 netUtcNow   = lastReqDateTime.Add(TimeSpan.FromSeconds(offsetSeconds));
        if (Mathf.Abs((float)(localUtcNow - netUtcNow).TotalSeconds) <= trustThreshold)
        {
            deltaSeconds = (long)(localUtcNow - lastDate).TotalSeconds;
            lastDate = localUtcNow;
        }
        else
        {
            deltaSeconds = (long)(lastReqDateTime - lastDate).TotalSeconds + offsetSeconds;
            lastDate = netUtcNow;
        }

        return wasDateValid || lastDateWasValid;
    }

    public void GetDeltaSecondsWithValidation(ref DateTime lastDate, ref bool lastDateWasValid, out long deltaSeconds, float timeout, Action<bool> onValidationFinished)
    {
        bool isValid = this.GetDeltaSeconds(ref lastDate, ref lastDateWasValid, out deltaSeconds);
        if (isValid)
            onValidationFinished(true);
        else
            this.StartCoroutine(this.ValidateDateTime(lastDate, timeout, onValidationFinished));
    }

    public void GetDeltaSecondsAsync(DateTime lastDate, bool lastDateWasValid, long deltaSeconds, Action<DateTime, bool, long> onFinished)
    {
        bool isValid = this.GetDeltaSeconds(ref lastDate, ref lastDateWasValid, out deltaSeconds);
        
        if (isValid)
            onFinished(lastDate, true, deltaSeconds);
        else
            this.StartCoroutine(this.GetDateTimeAsync(lastDate, lastDateWasValid, onFinished));
    }
    #endregion
}
