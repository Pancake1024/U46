using System;
using UnityEngine;

[AddComponentMenu("SBS/Racing/VehicleRaceData")]
public class VehicleRaceData : MonoBehaviour
{
    #region Protected members
    protected VehicleTrackData trackData;
    protected int position;
    protected int lap;
    protected int boxLap;
    protected float beforeRaceStartTime;
    protected float lastLapTime;
    protected float currLapTime;
    protected float totalTime;
    protected bool raceStarted;
    protected bool raceEnded;
    protected int flCounter;
    #endregion

    #region Public properties
    public int Position
    {
        get
        {
            return position;
        }
        set
        {
            position = value;
        }
    }

    public int Lap
    {
        get
        {
            return lap;
        }
    }

    public int FinishLineCounter
    {
        get
        {
            return flCounter;
        }
        set
        {
            flCounter = value;
        }
    }

    public int BoxLap
    {
        get
        {
            return boxLap;
        }
        set
        {
            boxLap = value;
        }
    }

    public float LastLapTime
    {
        get
        {
            return lastLapTime;
        }
    }

    public float CurrentLapTime
    {
        get
        {
            return currLapTime;
        }
    }

    public float RaceTime
    {
        get
        {
            return totalTime + currLapTime;
        }
        set 
        {
            totalTime = value;
        }
    }

    public bool RaceStarted
    {
        get
        {
            return raceStarted;
        }
        set
        {
            raceStarted = value;
        }
    }

    public bool RaceEnded
    {
        get
        {
            return raceEnded;
        }
        set
        {
            raceEnded = value;
        }
    }

    public float RaceTrackPosition
    {
        get
        {
            if (trackData==null)
                trackData = gameObject.GetComponent<VehicleTrackData>();
            /*
            Debug.Log("RacingManager.Instance.track " + RacingManager.Instance.track + " Length " + RacingManager.Instance.track.Length );
            Debug.Log(" trackData " + trackData);
            Debug.Log(" TrackPosition " + trackData.TrackPosition);
            */
            
//          return ((lap - 1) * RacingGame.Instance.Track.Length) + trackData.TrackPosition; // Mathf.Max(lap - 1, 0)
            return ((flCounter - 1) * RacingManager.Instance.track.Length) + trackData.TrackPosition; // Mathf.Max(lap - 1, 0)
        }
    }
    #endregion

    #region Public methods
    public void UpdatePosition()
    {
        int prevPos = position;

        position = 1;
        foreach (VehicleRaceData component in RacingManager.Instance.vehicles)
        {
            if (this == component)
                continue;

            if (!raceEnded && component.RaceTrackPosition >= this.RaceTrackPosition)
            {
                ++position;
                continue;
            }

            if (raceEnded && component.raceEnded && (component.beforeRaceStartTime + component.RaceTime) < (beforeRaceStartTime + this.RaceTime))
            {
                ++position;
                continue;
            }
        }

        if (prevPos != position)
            gameObject.SendMessage("OnRacePosChanged", this, SendMessageOptions.DontRequireReceiver);
    }
    #endregion

    #region Unity callbacks
    void Start()
    {
        trackData = gameObject.GetComponent<VehicleTrackData>();
        //Debug.Log("START OF VEHICLERACEDATA " + trackData);

        position = 1;
        lap = 0;
        beforeRaceStartTime = 0.0f;
        lastLapTime = 0.0f;
        currLapTime = 0.0f;
        totalTime = 0.0f;
        raceStarted = false;
        raceEnded = false;
        flCounter = 0;

        int numLaps = RacingManager.Instance.laps;
        boxLap = UnityEngine.Random.Range(numLaps / 2 - 1, numLaps / 2 + 2);
    }

    void Update()
    {
        if (!raceEnded && !raceStarted)
            beforeRaceStartTime += TimeManager.Instance.MasterSource.DeltaTime;

        if (!raceEnded && raceStarted)//lap > 0)
            currLapTime += TimeManager.Instance.MasterSource.DeltaTime;
    }

    void LateUpdate()
    {
        this.UpdatePosition();
    }
    #endregion

    #region Messages
    void OnCheckline(ChecklinesManager.Message msg)
    {       
        RacingManager.Instance.CheckFinishLine(this, msg, ref flCounter);
    }

    void OnNextLap(int numLaps)
    {
        if (raceEnded)
            return;

        if (0 == lap)
            raceStarted = true;

        lastLapTime = currLapTime;
        currLapTime = 0.0f;

        totalTime += lastLapTime;

        raceEnded = (numLaps < ++lap); // ==
        //Debug.Log("next lap" + lap);
        gameObject.SendMessage("OnRaceLapChanged", this, SendMessageOptions.DontRequireReceiver);
    }

    void ForceRaceEnd(VehicleRaceData reference)
    {
        if (raceEnded)
            return;

        lastLapTime = 0.0f;
        currLapTime = 0.0f;

        float timeRef = reference.RaceTime;
        float totRaceLen = RacingManager.Instance.track.Length * RacingManager.Instance.laps;
        totalTime = timeRef + timeRef * (totRaceLen - this.RaceTrackPosition) / totRaceLen;

        raceEnded = true;
    }

    void ResetRace()
    {
        this.Start();
    }
    #endregion
}
