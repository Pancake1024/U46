using System;
using UnityEngine;
using SBS.Math;

namespace SBS.Core
{
    public class TimeSource
    {
        #region Protected members
        protected float lastPauseTime = 0.0f;
        protected float totalPauseTime = 0.0f;
        protected float realTime = 0.0f;
        protected float totalTime = 0.0f;
        protected float deltaTime = 0.0f;
        protected float timeMultiplier = 1.0f;
        protected int pauseCounter = 0;
        protected float maxDeltaTime = 1.0f / 12.0f;
        protected bool lockPause = false;
        #endregion

        #region Public properties
        public float TotalTime
        {
            get
            {
                return totalTime;
            }
        }

        public float DeltaTime
        {
            get
            {
                return this.IsPaused ? 0.0f : deltaTime;
            }
        }

        public float TimeMultiplier
        {
            get
            {
                return this.IsPaused ? 0.0f : timeMultiplier;
            }
            set
            {
                timeMultiplier = SBSMath.Max(0.0f, value);

                if (this.IsMaster)
                    Time.timeScale = timeMultiplier;
            }
        }

        public bool IsPaused
        {
            get
            {
                return pauseCounter > 0;
            }
        }

        public bool IsMaster
        {
            get
            {
                return this == TimeManager.Instance.MasterSource;
            }
        }
        #endregion

        #region Virtual methods
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        #endregion

        #region Public methods
        public void Pause()
        {
            if (0 == pauseCounter && !lockPause)
            {
                lockPause = true;

                lastPauseTime = Time.realtimeSinceStartup;

                if (this.IsMaster)
                {
                    Time.timeScale = 0.0f;

                    TimeManager.Instance.OnGamePaused();
                }

                this.OnPause();

                lockPause = false;
            }

            ++pauseCounter;
        }

        public void Resume()
        {
            Asserts.Assert(pauseCounter >= 1);

            --pauseCounter;
            if (0 == pauseCounter)
            {
                totalPauseTime += (Time.realtimeSinceStartup - lastPauseTime);

                if (this.IsMaster)
                {
                    Time.timeScale = timeMultiplier;

                    TimeManager.Instance.OnGameResumed();
                }

                this.OnResume();
            }
        }

        public void Reset()
        {
            lastPauseTime = Time.realtimeSinceStartup;
            totalPauseTime = 0.0f;
            realTime = Time.realtimeSinceStartup;
            totalTime = 0.0f;
            deltaTime = 0.0f;
            timeMultiplier = 1.0f;
        }

        public void Update()
        {
            if (this.IsPaused)
                return;

            float prevRealTime = realTime;

            realTime = Time.realtimeSinceStartup - totalPauseTime;
            deltaTime = SBSMath.Min(maxDeltaTime, realTime - prevRealTime) * timeMultiplier;
            totalTime += deltaTime;
        }
        #endregion
    }
}
