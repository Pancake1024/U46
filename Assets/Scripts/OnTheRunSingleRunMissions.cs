using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Globalization;

[AddComponentMenu("OnTheRun/OnTheRunSingleRunMissions")]
public class OnTheRunSingleRunMissions : MonoBehaviour
{
#if UNITY_WEBPLAYER
    public static bool canBuyMissions = true;
#else
    public static bool canBuyMissions = false;
#endif

    public static OnTheRunSingleRunMissions Instance = null;

    protected List<OnTheRunEnvironment.Environments> forceSpecificTierFlags;
    protected List<OnTheRunGameplay.CarId> forceSpecificSpecialVehicleFlags;

    public bool[] lastMissionWasSpecial = { false, false, false, false };

    #region Mission types
    public enum MissionUnit
    {
        Meters = 0,
        Seconds,
        Times,
        Cars
    }

    public enum MissionType
    {
        None = -1,
        DriveForMeters = 0,
        ChangeLane,
        Jump,
        OverRedTruck,
        OverBlueTruck,
        OverYellowTruck,
        CheckpointPassed,
        CollectCoins,
        CollectShield,
        CollectBolt,
        CollectMagnet,
        CarSlipstream,
        BumpOnTraffic,
        MoveAwayTraffic,
        DestroyTraffic,
        DestroyCar1,
        DestroyCar2,
        DestroyCar3,
        DestroyCar4,
        DestroyCar5,
        DestroyCar6,
        DestroyCar7,
        DestroyBlackCar,
        DestroyBlackHelicopter,
        DestroyBlackVan,
        DestroyBlackPickup,
        DestroyBadGuyWithSuperpower,
        DestroyCarsWithSuperpower,
        DestroyCarsInTurbo,
        CollectBigCoins,
        UseTank,
        UseBigfoot,
        DestroyCarsWithTank,
        DestroyCarsWithBigfoot,
        CentralLaneMeters,
        LeftLaneMeters,
        RightLaneMeters,
        NeverSlipstream,
        OnAirFor,
        DestroyPolice,
        WrongDirection,
        ReachCombo,
        ReachCombo2,
        ReachCombo3,
        ReachCombo4,
        ReachCombo5,
        ReachCombo6,
        ReachCombo7,
        ReachCombo8,
        ReachCombo9,
        ReachCombo10,
        ReachCombo11,
        ReachCombo12,
        ReachCombo13,
        ReachCombo14,
        ReachCombo15,
        UseUFo,
        UseFiretruck,
        UsePlane,
        DestroyCarsWithFiretruck,
        DestroyCarsWithPlane,
        DestroyCarsWithUfo,
        CheckpointPassedInTurbo,
        DontChangeLane,
        StartTheGame,
        DriveForMetersMedium,
        DriveForMetersHard,
        AvoidBarriers,
        DestroyCarsInTurboWrongLane,
        DestroyCarsInWrongLaneWithFiretruck,
        DestroyCarsInWrongLaneWithPlane,
        DestroyCarsInWrongLaneWithUfo,
        DestroyCarsInWrongLaneWithBigfoot,
        DestroyCarsInWrongLaneWithTank,
        CentralLaneUSAFor,
        UseBigfootFor,
        UseTankFor,
        UseUFoFor,
        UseFiretruckFor,
        UsePlaneFor,
        NumTypes
    }   

    public enum MissionCategory
    {
        RunFor = 0,
        ChangeLane,
        Jump,
        GetOverRamp,
        OnAir,
        DestroyPolice,
        WrongDirection,
        ReachCombo,
        DestroyTraffic,
        DestroyCarEurope,
        DestroyCarAsia,
        DestroyCarNY,
        DestroyCarUSA,
        Checkpoint,
        CollectCoins,
        CollectBigCoins,
        CollectShield,
        CollectBolt,
        CollectMagnet,
        NeverSlipstream,
        BumpOn,
        MoveAway,
        GetTank,
        GetBigfoot,
        GetUFO,
        GetPlane,
        GetFiretruck,
        DestroyWithTank,
        DestroyWithBigfoot,
        DestroyWithUFO,
        DestroyWithPlane,
        DestroyWithFiretruck,
        CentralLaneFor,
        RightLaneFor,
        LeftLaneFor,
        CheckpointWithTurbo,
        DontChangeLane,
        StartGame,
        Slipstream,
        AvoidBarriers,
        DestroyCarsInTurbo,
        DestroyCarsInTurboWrongLane,
        DestroyCarsWithSpecialInWrongLane,
        CentralLaneUSAFor,
        UseSpecialVehicleFor
    }
    #endregion

    #region Public members
    public NeverendingPlayer playerRef = null;
    public PlayerKinematics playerKinematics = null;
    protected PlayerDraft playerDraftRef = null;
    public int numActiveMissions = 4;

    #endregion

    #region Protected members
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunEnvironment environmentManager;
    protected Transform missionsPanel;
    protected List<DriverMission> missions = null;
    protected List<DriverMission> activeMissions = null;
    protected bool isActive = false;
    protected List<int> missionPassedInGame;
    protected bool canActivatePassedMissions = false;
    protected List<DriverMission> reachComboList = null;

    protected float previousMeters = 0.0f;
    protected int missionPassedCounter;

    protected OpponentKinematics.OpponentId[] specificCars = {  OpponentKinematics.OpponentId.TrafficEu2,
                                                                OpponentKinematics.OpponentId.TrafficNy2,
                                                                OpponentKinematics.OpponentId.TrafficAsia6,
                                                                OpponentKinematics.OpponentId.TrafficUs1};

    protected int[] metersToReachFirstTimeData = { 1000, 0, -1 };
    protected int[] destroyTrafficFirstTimeData = { 10, 0, -1 };
    protected int[] reachComboFirstTimeData = { 1, 0, -1 };
    protected int[] jumpQuantityFirstTimeData = { 2, 0, -1 };
    protected int[] collectBigCoinsFirstTimeData = { 5, 0, -1 };

    protected int[] metersToReachData = { 3000, 500, 25000 };
    protected int[] jumpQuantityData = { 4, 1, 25 };
    protected int[] onAirData = { 4, 1, 12 };
    protected int[] passCheckpointData = { 4, 1, 15 };
    protected int[] collectCoinsData = { 50, 10, 400 };
    protected int[] collectBigCoinsData = { 12, 2, 100 };
    protected int[] fastLaneMetersData = { 2000, 250, 12000 };
    protected int[] centralLaneMetersData = { 2000, 250, 18000 };
    protected int[] leftLaneMetersData = { 2000, 250, 18000 };
    protected int[] rightLaneMetersData = { 2000, 250, 18000 };
    protected int[] avoidBarriersData = { 3000, 250, 25000 };
    protected int[] destroyTrafficData = { 30, 3, 250 };
    protected int[] destroyTrafficInTurboData = { 15, 3, 200 };
    protected int[] destroyTrafficInTurboWrongLaneData = { 8, 2, 150 };
    protected int[] destroyPoliceQuantityData = { 1, 1, 6 };
    protected int[] runWithBigfootForData = { 600, 200, 9000 };
    protected int[] runWithTankForData = { 600, 200, 9000 };
    protected int[] runWithFiretruckForData = { 600, 200, 9000 };
    protected int[] runWithUFOForData = { 600, 200, 9000 };
    protected int[] runWithPlaneForData = { 600, 200, 9000 };
    protected int[] destroyCarsDataWSpecial = { 9, 3, 180 };
    protected int[] destroyCarsWithSpecialInWrongLaneData = { 4, 2, 120 };
    protected int[] reachComboData = { 1, 0, -1 };

    protected bool reachComboActive = false;
    protected List<MissionCategory> activeMissionsType;

    protected int maxReachComboIndex;
    protected int simultaneouslyReachComboNum = 2;

    protected string activeMissionSaveID = "actm_id";
    protected string firstTimeGameStartID = "ftgs_id";
    
    protected string forceSpecificTierCounterID = "fstc_id";
    protected string forceSpecificSpecialVehicleCounterID = "fssvc_id";
    protected int forceSpecificTierCounter = 0;
    protected int forceSpecificSpecialVehicleCounter = 0;

    protected UISharedData uiSharedData;
    protected OnTheRunAchievements.Achievement achMetersMed;
    protected OnTheRunAchievements.Achievement achMetersHard;
    protected float achMetersMediumStartCounter = -1.0f;
    protected float achMetersHardStartCounter = -1.0f;

    protected bool chooseRunTimeMission = true;
    protected DriverMission missionUpdatedRunTime;
    protected List<DriverMission> firstTimeMissions;
    #endregion

    #region Public properties
    public List<DriverMission> ReachComboList
    {
        get { return reachComboList; }
    }

    public bool CanActivatePassedMissions
    {
        get { return canActivatePassedMissions; }
        set { canActivatePassedMissions = value; }
    }

    public List<int> MissionPassedIngame
    {
        get { return missionPassedInGame; }
        set { missionPassedInGame = value; }
    }

    public int MissionPassedCounter
    {
        get { return missionPassedCounter; }
    }
    
    public int ActiveMissionsRemaining
    {
        get
        {
            int remaining = 0;
            for (int i = 0; i < activeMissions.Count; ++i)
            {
                DriverMission mission = activeMissions[i];
                if (mission != null && !mission.Done && !mission.JustPassed)
                    ++remaining;
            }

            return remaining;
        }
    }

    public List<DriverMission> ActiveMissions
    {
        get
        {
            return activeMissions;
        }
    }

    public DriverMission CurrentTierMission
    {
        get
        {
            int tierIndex = gameplayManager.EnvironmentIdx(environmentManager.currentEnvironment);
            if (activeMissions == null)
                return null;
            else
                return activeMissions[tierIndex];
        }
    }

    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;
        }
    }
    #endregion

    #region Generic functions
    void OnChangePlayerCar()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player").GetComponent<NeverendingPlayer>();
        playerKinematics = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
        playerDraftRef = playerKinematics.gameObject.GetComponent<PlayerDraft>();
    }

    void OnInitGameplayTime() //RaceStarted()
    {
        previousMeters = 0.0f;
        isActive = true;

        if (!gameplayManager.restartFromSaveMe)
        {
            chooseRunTimeMission = true;
            ResetActiveGameMissions();
        }

#if !UNITY_WEBPLAYER
        achMetersMed = OnTheRunAchievements.Instance.GetAchievement(OnTheRunAchievements.AchievementType.METERS_MEDIUM);
        achMetersHard = OnTheRunAchievements.Instance.GetAchievement(OnTheRunAchievements.AchievementType.METERS_HARD);

        if (!achMetersMed.Done)
            achMetersMediumStartCounter = achMetersMed.Counter;
        else
            achMetersMediumStartCounter = -1.0f;

        if (!achMetersHard.Done)
            achMetersHardStartCounter = achMetersHard.Counter;
        else
            achMetersHardStartCounter = -1.0f;
#endif
    }

    void OnEndSession()
    {
#if !UNITY_WEBPLAYER
        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.METERS_MEDIUM, uiSharedData.InterfaceDistance);
        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.METERS_HARD, uiSharedData.InterfaceDistance);
#endif
    }

    void PlayerIsDead()
    {
        //Debug.Log("**** MISSIONS DEACTIVATED");
        isActive = false;
    }

    void PlayerIsResurrected()
    {
        //Debug.Log("**** MISSIONS RE-ACTIVATED");
        isActive = true;
    }
    #endregion

    #region Mission update messages
    public void OnGenericMissionEvent(OnTheRunSingleRunMissions.MissionType type)
    {
        DoMissionEvent(type);
    }

    public void OnDestroyCarsTurboWrongLane(OnTheRunSingleRunMissions.MissionType type, int lane)
    {
        DriverMission currentMission = CurrentTierMission;
        if (currentMission.MatchWrongLaneByEnv(lane, environmentManager.currentEnvironment))
            DoMissionEvent(type);
    }

    public void OnDestroyCarsSpecialVehicle(OnTheRunGameplay.CarId specialVehicleId)
    {
        OnTheRunSingleRunMissions.MissionType type = OnTheRunSingleRunMissions.MissionType.None;
        switch (specialVehicleId)
        {
            case OnTheRunGameplay.CarId.Bigfoot:
                type = OnTheRunSingleRunMissions.MissionType.DestroyCarsWithBigfoot;
                break;
            case OnTheRunGameplay.CarId.Tank:
                type = OnTheRunSingleRunMissions.MissionType.DestroyCarsWithTank;
                break;
            case OnTheRunGameplay.CarId.Firetruck:
                type = OnTheRunSingleRunMissions.MissionType.DestroyCarsWithFiretruck;
                break;
            case OnTheRunGameplay.CarId.Ufo:
                type = OnTheRunSingleRunMissions.MissionType.DestroyCarsWithUfo;
                break;
            case OnTheRunGameplay.CarId.Plane:
                type = OnTheRunSingleRunMissions.MissionType.DestroyCarsWithPlane;
                break;
        }

        DoMissionEvent(type);
    }

    public void OnDestroyCarsSpecialWrongLane(OnTheRunGameplay.CarId specialVehicleId, int lane)
    {
        DriverMission currentMission = CurrentTierMission;
        if (currentMission.MatchWrongLaneByEnv(lane, environmentManager.currentEnvironment))
        {
            OnTheRunSingleRunMissions.MissionType type = OnTheRunSingleRunMissions.MissionType.None;
            switch (specialVehicleId)
            {
                case OnTheRunGameplay.CarId.Bigfoot:
                    type = OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithBigfoot;
                    break;
                case OnTheRunGameplay.CarId.Tank:
                    type = OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithTank;
                    break;
                case OnTheRunGameplay.CarId.Firetruck:
                    type = OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithFiretruck;
                    break;
                case OnTheRunGameplay.CarId.Ufo:
                    type = OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithUfo;
                    break;
                case OnTheRunGameplay.CarId.Plane:
                    type = OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithPlane;
                    break;
            }

            DoMissionEvent(type);
        }
    }

    /*public void OnMissionCoinsCollected(int quantity)
    {
        DoMissionEvent(OnTheRunSingleRunMissions.MissionType.CollectCoins, quantity);
    }*/

    public void OnDraft()
    {
        DoMissionEvent(OnTheRunSingleRunMissions.MissionType.CarSlipstream);
    }

    public void OnLaneChanged(int lanesChanged)
    {
        DoMissionEvent(OnTheRunSingleRunMissions.MissionType.ChangeLane, lanesChanged);
    }

    /*public void OnReachSingleCombo(int comboReached)
    {
        if (!isActive)
            return;

        DriverMission currentMission = CurrentTierMission;

        if (currentMission.type == OnTheRunSingleRunMissions.MissionType.ReachCombo)
        {
            if (comboReached >= currentMission.checkCounter && !currentMission.JustPassed)
            {
                //Debug.Log("MISSION DONE: " + mission.id + " c: " + mission.Counter + " cc: " + mission.checkCounter);
                currentMission.Done = true; // PIETRO
                missionsPanel.GetComponent<UIMissionsPanel>().ShowMissionsPanel(currentMission.type);
                NotifyMissionDone(currentMission);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_MEDIUM);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_HARD);
            }
        }
    }*/

    public void OnReachCombo(int comboReached)
    {
        OnTheRunSingleRunMissions.MissionType type = OnTheRunSingleRunMissions.MissionType.None;
        switch (comboReached)
        {
            case 2: type = OnTheRunSingleRunMissions.MissionType.ReachCombo2; break;
            case 3: type = OnTheRunSingleRunMissions.MissionType.ReachCombo3; break;
            case 4: type = OnTheRunSingleRunMissions.MissionType.ReachCombo4; break;
            case 5: type = OnTheRunSingleRunMissions.MissionType.ReachCombo5; break;
            case 6: type = OnTheRunSingleRunMissions.MissionType.ReachCombo6; break;
            case 7: type = OnTheRunSingleRunMissions.MissionType.ReachCombo7; break;
            case 8: type = OnTheRunSingleRunMissions.MissionType.ReachCombo8; break;
            case 9: type = OnTheRunSingleRunMissions.MissionType.ReachCombo9; break;
            case 10: type = OnTheRunSingleRunMissions.MissionType.ReachCombo10; break;
            case 11: type = OnTheRunSingleRunMissions.MissionType.ReachCombo11; break;
            case 12: type = OnTheRunSingleRunMissions.MissionType.ReachCombo12; break;
            case 13: type = OnTheRunSingleRunMissions.MissionType.ReachCombo13; break;
            case 14: type = OnTheRunSingleRunMissions.MissionType.ReachCombo14; break;
            case 15: type = OnTheRunSingleRunMissions.MissionType.ReachCombo15; break;
        }

        if (type != OnTheRunSingleRunMissions.MissionType.None)
            DoMissionEvent(type);
    }
    
    public void OnDestroyspecificCar(OpponentKinematics.OpponentId carId)
    {
        if (carId == OpponentKinematics.OpponentId.TrafficPolice)
        {
            DoMissionEvent(OnTheRunSingleRunMissions.MissionType.DestroyPolice);
        }
        else if (carId == specificCars[0])
        {
            DoMissionEvent(OnTheRunSingleRunMissions.MissionType.DestroyCar1);
        }
        else if (carId == specificCars[1])
        {
            DoMissionEvent(OnTheRunSingleRunMissions.MissionType.DestroyCar2);
        }
        else if (carId == specificCars[2])
        {
            DoMissionEvent(OnTheRunSingleRunMissions.MissionType.DestroyCar3);
        }
        else if (carId == specificCars[3])
        {
            DoMissionEvent(OnTheRunSingleRunMissions.MissionType.DestroyCar4);
        }
    }
    #endregion

    #region New Missions Stuff
    public enum SingleRunMissionsRewardType
    {
        None = -1,
        Exp = 0,
        Coins,
        Gems,
        Rank
    }

    public class SingleRunMissionsReward
    {
        public SingleRunMissionsRewardType type;
        public int quantity;
        public int metersReward = 0;

        private int base_value;
        private int increase_per_level;
        private float meters_multiplier;

        private int expPerMission;
        private int expPerMeters;

        public int ExpForMission
        {
            get { return expPerMission; }
        }

        public int ExpForMeters
        {
            get { return expPerMeters; }
        }

        public SingleRunMissionsReward(SingleRunMissionsRewardType _type)
        {
            type = _type;

            base_value = (int)OnTheRunDataLoader.Instance.GetMissionsRewardData("base_value", 4000f);
            increase_per_level = (int)OnTheRunDataLoader.Instance.GetMissionsRewardData("increase_per_level", 100f);
            meters_multiplier = OnTheRunDataLoader.Instance.GetMissionsRewardData("meters_multiplier", 0.1f);
        }

        public void ComputeReward(bool passed)
        {
            if (passed)
            {
                expPerMission = base_value + PlayerPersistentData.Instance.Level * increase_per_level;
                expPerMeters = 0;// (int)(metersReward * meters_multiplier);
            }
            else
            {
                expPerMission = 0;
                expPerMeters = (int)(metersReward * meters_multiplier);
            }
            quantity = expPerMission + expPerMeters;
        }

        public int GetRewardPassed( )
        {
            return base_value + PlayerPersistentData.Instance.Level * increase_per_level;
        }
    }

    public class LockCondition
    {
        List<OnTheRunEnvironment.Environments> lockByTiers;
        OnTheRunGameplay.CarId lockByVehicle = OnTheRunGameplay.CarId.None;
        MissionType lockByMission = MissionType.None;

        public LockCondition(OnTheRunEnvironment.Environments[] _lockByTiers, OnTheRunGameplay.CarId _lockByVehicle, MissionType _lockByMission)
        {
            lockByTiers = new List<OnTheRunEnvironment.Environments>();
            if (_lockByTiers != null)
            {
                foreach (OnTheRunEnvironment.Environments env in _lockByTiers)
                    lockByTiers.Add(env);
            }

            lockByVehicle = _lockByVehicle;
            lockByMission = _lockByMission;
        }

        public TruckBehaviour.TrasformType GetSpecialVehicleNeeded( )
        {
            return UnlockingManager.Instance.FromCarIdToTrasformType(lockByVehicle);
        }

        public bool MatchTier(OnTheRunEnvironment.Environments tierIndex)
        {
            if(lockByTiers.Count==0)
                return true;
            else
                return lockByTiers.IndexOf(tierIndex)>=0;
        }

        public bool MatchVehicle( OnTheRunGameplay.CarId carId)
        {
            return lockByVehicle == OnTheRunGameplay.CarId.None || lockByVehicle == carId;
        }

        public bool MatchWrongLane(int laneIndex, OnTheRunEnvironment.Environments env)
        {
            bool retValue = false;
            switch (env)
            {
                case OnTheRunEnvironment.Environments.Asia:
                    retValue = laneIndex == 3 || laneIndex == 4;
                    break;

                case OnTheRunEnvironment.Environments.USA:
                    retValue = laneIndex == 0 || laneIndex == 1;
                    break;
            }

            return retValue;
        }

        public bool CheckForReachComboAvailable()
        {
            bool retValue = true;

            if (lockByMission != MissionType.None)
            {
                List<DriverMission> reachComboMissions = OnTheRunSingleRunMissions.Instance.ReachComboList;
                for (int i = 0; i < reachComboMissions.Count; ++i)
                {
                    bool indexLower = (int)reachComboMissions[i].type <= (int)lockByMission;
                    if (indexLower)
                    {
                        //Debug.Log("--> " + reachComboMissions[i].type + "(" + (int)reachComboMissions[i].type + ") -> " + lockByMission + "(" + (int)lockByMission + ") -> " + reachComboMissions[i].Done);
                        if (reachComboMissions[i].Done)
                            retValue = true;
                        else
                        {
                            retValue = false;
                            break;
                        }
                    }
                }
            }

            return retValue;
        }

        public bool IsLocked(OnTheRunEnvironment.Environments tierEnv)
        {
            bool lockedByTier = false;
            bool lockedByCar = false;

            if (lockByTiers.Count > 0)
            {
                for (int i = 0; i < lockByTiers.Count; ++i)
                {
                    if (lockByTiers[i] == tierEnv && PlayerPersistentData.Instance.IsParkingLotLockedEnv(lockByTiers[i]))
                    {
                        lockedByTier = true;
                        break;
                    }
                }
            }

            if (lockByVehicle != OnTheRunGameplay.CarId.None)
                lockedByCar = UnlockingManager.Instance.IsCarLocked(lockByVehicle);

            return lockedByTier || lockedByCar;
        }
    }
    #endregion

    #region Public classes
    public class MissionLevelLock
    {
        public int[] levelBounds;
        public int[] data;

        public MissionLevelLock(int minLevel, int maxLevel, int startGoal, int increaseGoal, int cap)
        {
            levelBounds = new int[2];
            levelBounds[0] = minLevel;
            levelBounds[1] = maxLevel;

            data = new int[3];
            data[0] = startGoal;
            data[1] = increaseGoal;
            data[2] = cap;
        }

        public int[] GetLockData(int currentLevel)
        {
            if (currentLevel >= levelBounds[0] && currentLevel <= levelBounds[1])
                return data;
            else
                return null;
        }
    }

    public class DriverMission
    {
        public String id;
        public String description;
        public MissionCategory category;
        public OnTheRunSingleRunMissions.MissionType type;
        public bool inMatch;
        public int checkCounter;
        protected bool done;
        protected bool rewardTaken;
        protected int active;
        protected float counter;
        protected int missionPrice = -1;
        //protected int missionRewardXP = -1;
        public int currentIndex;

        protected int startGoalValue = -1;
        protected int incrementGoalValue = -1;
        protected int maxCap = -1;
        //public int[] checkCounterArray;

        protected MissionUnit unit;

        private SingleRunMissionsReward missionReward;
        private SingleRunMissionsRewardType rewardType;
        private bool giveDiamond = false;
        private LockCondition lockedBy;

        private int minLevel = 1;

        public String[] descriptionPrefix;
        public String[] descriptionPostfix;

        public String descriptionShortPrefix;
        public String descriptionShortPostfix;

        protected bool justPassed;

        protected int reachComboIndex;

        protected List<MissionLevelLock> playerLevelLock;
        
        #region Getters/Setters
        /*public OnTheRunEnvironment.Environments SpeficifTier
        {
            get { return specificTier; }
        }*/

        /*public bool IsActive
        {
            get { return (int)(ownerTier) >= 0; }
        }*/

        public bool DiamondRewardActive
        {
            get { return giveDiamond; }
        }

        public bool IsANonRepeatibleMission
        {
            get
            {
                return category == OnTheRunSingleRunMissions.MissionCategory.ReachCombo;
            }
        }

        public bool IsLocked(OnTheRunEnvironment.Environments tierEnv)
        {
            if (lockedBy == null)
                return false;
            else
                return lockedBy.IsLocked(tierEnv); 
        }

        /*public OnTheRunEnvironment.Environments Owner
        {
            get { return ownerTier; }
            set { ownerTier = value; }
        }*/

        public bool IsPassed
        {
            get { return counter >= checkCounter; }
        }

        public SingleRunMissionsReward Reward
        {
            get 
            { 
                missionReward.ComputeReward(Done);
                return missionReward; 
            }
        }

        public int MissionPrice
        {
            get
            {
                if (missionPrice < 0)
                    calculateMissionPriceAndXP();
                return missionPrice;
            }
        }

        /*public int RewardXP
        {
            get
            {
                if (missionRewardXP < 0)
                    calculateMissionPriceAndXP();
                return missionRewardXP;
            }
        }*/

        public string GoalUnitIngame
        {
            get
            {
                string retValue = "";
                switch (unit)
                {
                    case MissionUnit.Meters:
                        retValue = OnTheRunDataLoader.Instance.GetLocaleString("meters_short");
                        break;
                    case MissionUnit.Seconds:
                        retValue = OnTheRunDataLoader.Instance.GetLocaleString("seconds_short");
                        break;
                }
                return retValue;
            }
        }
        
        public bool RewardTaken
        {
            get
            {
                return rewardTaken;
            }
            set
            {
                rewardTaken = value;
                EncryptedPlayerPrefs.SetInt(id + "_rt", done ? 1 : 0);
            }
        }

        public bool Done
        {
            get
            {
                return done;
            }
            set
            {
                done = value;
                EncryptedPlayerPrefs.SetInt(id + "_d", done ? 1 : 0);
            }
        }

        public int Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                EncryptedPlayerPrefs.SetInt(id + "_a", active);
            }
        }

        public bool JustPassed
        {
            get
            {
                return justPassed;
            }
            set
            {
                justPassed = value;
                EncryptedPlayerPrefs.SetInt(id + "_jp", justPassed ? 1 : 0);
            }
        }

        public float Counter
        {
            get
            {
                return counter;
            }
            set
            {
                counter = value;
                if (!inMatch)
                    EncryptedPlayerPrefs.SetFloat(id + "_c", counter);
            }
        }
        
        public int CurrentLevel
        {
            get
            {
                return currentIndex;
            }
        }
        
        public TruckBehaviour.TrasformType SpecialVehicleNeeded
        {
            get
            {
                if (lockedBy == null)
                    return TruckBehaviour.TrasformType.None;
                else
                    return lockedBy.GetSpecialVehicleNeeded();
            }
        }

        public string MissionPrefix
        {
            get
            {
                if (type >= OnTheRunSingleRunMissions.MissionType.ReachCombo && type <= OnTheRunSingleRunMissions.MissionType.ReachCombo15)
                    return descriptionShortPrefix + " " + reachComboIndex;
                else
                    return descriptionShortPrefix;
            }
        }
        
        public string MissionPostfix
        {
            get
            {
                string retValue = descriptionShortPostfix;
                if (type == MissionType.DestroyPolice && checkCounter == 1)
                    descriptionShortPostfix = OnTheRunDataLoader.Instance.GetLocaleString("mission_police_car_single");
                return descriptionShortPostfix;
            }
        }
        #endregion

        public  DriverMission(int _num, string _id, MissionCategory _category, MissionUnit _unit, string[] _description_prefix, string[] _description_postfix, string _description_short_prefix, string _description_short_postfix, OnTheRunSingleRunMissions.MissionType _type, bool _inMatch, int[] _checkCounter, LockCondition _lockedBy = null, MissionLevelLock[] _playerLevelLocks = null)
        {
            id = _id;
            category = _category;
            unit = _unit;
            descriptionPrefix = _description_prefix;
            descriptionPostfix = _description_postfix;
            descriptionShortPrefix = _description_short_prefix;
            descriptionShortPostfix = _description_short_postfix;
            description = _description_prefix[0];
            type = _type;
            inMatch = _inMatch;
            startGoalValue = _checkCounter[0];
            incrementGoalValue = _checkCounter[1];
            maxCap = _checkCounter[2];
            playerLevelLock = new List<MissionLevelLock>();
            if (_playerLevelLocks != null)
            {
                for (int i = 0; i < _playerLevelLocks.Length; ++i)
                    playerLevelLock.Add(_playerLevelLocks[i]);
            }

            //checkCounterArray = _checkCounter;
            done = false;
            rewardTaken = false;
            active = -1;
            counter = 0.0f;

            lockedBy = _lockedBy;
            rewardType = SingleRunMissionsRewardType.Exp;

            load();
            calculateMissionPriceAndXP();
        }

        public bool MatchPlayerLevel()
        {
            bool retValue = playerLevelLock.Count == 0 ? true : false;
            int playerLevel = PlayerPersistentData.Instance.Level;
            for (int i = 0; i < playerLevelLock.Count; ++i)
            {
                MissionLevelLock currLevelLock = playerLevelLock[i];
                int[] currLevelData = currLevelLock.GetLockData(playerLevel);
                if (currLevelData != null)
                {
                    retValue = true;
                    break;
                }
            }

            return retValue;
        }

        void EvaluateNextGoal()
        {
            int playerLevel = PlayerPersistentData.Instance.Level;
            for (int i = 0; i < playerLevelLock.Count; ++i)
            {
                MissionLevelLock currLevelLock = playerLevelLock[i];
                int[] currLevelData = currLevelLock.GetLockData(playerLevel);
                if (currLevelData != null)
                {
                    startGoalValue = currLevelLock.data[0];
                    incrementGoalValue = currLevelLock.data[1];
                }
            }
        }

        public void CheckForDiamondReward(int tierIndex)
        {
            int diamondsPerc = (int)OnTheRunDataLoader.Instance.GetMissionsRewardData("diamond_perc", 100.0f);
            bool diamondActive = (UnityEngine.Random.Range(0, 100000) % 100) < diamondsPerc;

            if (OnTheRunSingleRunMissions.Instance.lastMissionWasSpecial[tierIndex])
                diamondActive = false;

#if UNITY_WEBPLAYER
            diamondActive = false;
#endif

            if (diamondActive)
                giveDiamond = true;
            else
                giveDiamond = false;
            OnTheRunSingleRunMissions.Instance.lastMissionWasSpecial[tierIndex] = giveDiamond;

            save();
        }

        public bool MatchSpecificTier(OnTheRunEnvironment.Environments tierIndex)
        {
            bool retValue = false;

            if (lockedBy != null)
            {
                if (lockedBy.MatchTier(tierIndex))
                    retValue = true;
            }
            else
                retValue = true;

            return retValue;
        }

        public bool MatchSpecificVehicle(OnTheRunGameplay.CarId carId)
        {
            bool retValue = false;

            if (lockedBy != null)
            {
                if (lockedBy.MatchVehicle(carId))
                    retValue = true;
            }
            else
                retValue = true;

            return retValue;
        }

        public bool MatchWrongLaneByEnv(int laneIndex, OnTheRunEnvironment.Environments env)
        {
            bool retValue = false;

            if (lockedBy != null)
            {
                if (lockedBy.MatchWrongLane(laneIndex, env))
                    retValue = true;
            }
            else
                retValue = false;

            return retValue;
        }

        public bool MatchReachComboSequence()
        {
            return lockedBy.CheckForReachComboAvailable();
        }

        public void calculateMissionPriceAndXP()
        {
            missionPrice = OnTheRunEconomy.Instance.GetMissionData(OnTheRunSingleRunMissions.Instance.MissionPassedCounter).cost;
            //missionRewardXP = OnTheRunDataLoader.Instance.GetMissionsRewardData(CurrentDifficulty);
            missionReward = new SingleRunMissionsReward(rewardType);
            save();
        }

        public void resetMission()
        {
            updateMission();
            missionPrice = -1;
            if (!IsANonRepeatibleMission)
                done = false;
            active = -1;
            counter = 0.0f;
            justPassed = false;
            rewardTaken = false;
            save();
        }
        
        public void updateMission(bool forceUpdateCounter = true)
        {
            if (forceUpdateCounter && checkCounter != maxCap)
                ++currentIndex;

            EvaluateNextGoal();

            checkCounter = startGoalValue + currentIndex * incrementGoalValue;

            if (type >= OnTheRunSingleRunMissions.MissionType.ReachCombo && type <= OnTheRunSingleRunMissions.MissionType.ReachCombo15)
            {
                int comboIndex = 1;
                if (type > OnTheRunSingleRunMissions.MissionType.ReachCombo)
                    comboIndex = (type - OnTheRunSingleRunMissions.MissionType.ReachCombo2) + 2;

                string prefix = descriptionPrefix.Length == 1 ? descriptionPrefix[0] : descriptionPrefix[1];//currentIndex
                string postfix = "";
                if (descriptionPostfix.Length > 0)
                    postfix = descriptionPostfix.Length == 1 ? descriptionPostfix[0] : descriptionPostfix[1];//currentIndex

                reachComboIndex = comboIndex;
                description = prefix + " " + comboIndex + " " + postfix;
            }
            else
            {
                string prefix = descriptionPrefix.Length == 1 ? descriptionPrefix[0] : descriptionPrefix[1];//currentIndex
                string postfix = "";
                if (descriptionPostfix.Length > 0)
                    postfix = descriptionPostfix.Length == 1 ? descriptionPostfix[0] : descriptionPostfix[1];//currentIndex

                if (type == MissionType.DestroyPolice && checkCounter == 1)
                    postfix = OnTheRunDataLoader.Instance.GetLocaleString("mission_police_car_single");

                description = prefix + " " + Manager<UIRoot>.Get().FormatTextNumber(checkCounter) +" " + postfix;
            }

            //Debug.Log("UPDATE: " + type + " " + checkCounter + " " + maxCap + " " + currentIndex + " " + description);
        }

        public void load()
        {
            done = EncryptedPlayerPrefs.GetInt(id + "_d", 0) == 1 ? true : false;
            rewardTaken = EncryptedPlayerPrefs.GetInt(id + "_rt", 0) == 1 ? true : false;
            active = EncryptedPlayerPrefs.GetInt(id + "_a", -1);
            justPassed = EncryptedPlayerPrefs.GetInt(id + "_jp", 0) == 1 ? true : false;
            counter = EncryptedPlayerPrefs.GetFloat(id + "_c", 0.0f);
            currentIndex = EncryptedPlayerPrefs.GetInt(id + "_ci", 0);
            giveDiamond = EncryptedPlayerPrefs.GetInt(id + "_gd", 0) == 1 ? true : false;

            updateMission(false);
        }

        public void save()
        {
            EncryptedPlayerPrefs.SetInt(id + "_d", done ? 1 : 0);
            EncryptedPlayerPrefs.SetInt(id + "_rt", rewardTaken ? 1 : 0);
            EncryptedPlayerPrefs.SetInt(id + "_a", active);
            EncryptedPlayerPrefs.SetInt(id + "_jp", justPassed ? 1 : 0);
            EncryptedPlayerPrefs.SetFloat(id + "_c", counter);
            EncryptedPlayerPrefs.SetInt(id + "_ci", currentIndex);
            EncryptedPlayerPrefs.SetInt(id + "_gd", giveDiamond ? 1 : 0);
            //EncryptedPlayerPrefs.SetInt(id + "_mp", missionPrice);
        }


        public void reset()
        {
            EncryptedPlayerPrefs.DeleteKey(id + "_d");
            EncryptedPlayerPrefs.DeleteKey(id + "_a");
            EncryptedPlayerPrefs.DeleteKey(id + "_jp");
            EncryptedPlayerPrefs.DeleteKey(id + "_c");
            EncryptedPlayerPrefs.DeleteKey(id + "_p");
            EncryptedPlayerPrefs.DeleteKey(id + "_ci");
            EncryptedPlayerPrefs.DeleteKey(id + "_hr");
            EncryptedPlayerPrefs.DeleteKey(id + "_gd");
            //EncryptedPlayerPrefs.DeleteKey(id + "_mp");
        }
    }
    #endregion

    #region Public methods
    bool CheckForMissionClassDuplicates(DriverMission mission)
    {
        return activeMissionsType.IndexOf(mission.category) >= 0;
    }

    #region Print time for test
    protected float timeForTest = -1;
    void InitTestTime()
    {
        timeForTest = Time.realtimeSinceStartup;
    }

    void PrintTestTime(int index)
    {
        float currTime = Time.realtimeSinceStartup;
        float timeDiff = currTime - timeForTest;
        Debug.Log("TIME DIFF " + index + ": " + timeDiff);
    }
    #endregion

    public void UpdateActiveMissions(bool firstTime=false)
    {
        activeMissionsType = new List<MissionCategory>();
        for (int i = 0; i < activeMissions.Count; ++i)
        { 
            if (activeMissions[i]!=null)
                activeMissionsType.Add(activeMissions[i].category);
        }

        //Add random to the choice
        OnTheRunUtils.Shuffle<DriverMission>(missions);

        List<DriverMission> rndList;
        for (int i = 0; i < numActiveMissions; ++i)
        {
            rndList = new List<DriverMission>();
            DriverMission currentActiveMission = activeMissions[i];
            if (currentActiveMission == null || currentActiveMission.Done || currentActiveMission.JustPassed)
            {
                if (currentActiveMission != null && !currentActiveMission.RewardTaken)
                    continue;

                if (firstTime && i==0)
                {
                    for(int j=0; j<firstTimeMissions.Count; ++j)
                        rndList.Add(firstTimeMissions[j]);
                }
                else
                {
                    for (int j = 0; j < missions.Count; ++j)
                    {
                        DriverMission possibleMission = missions[j];

                        //Debug.Log("11111111--->>>>>> " + possibleMission.id + " " + possibleMission.Done);
                        //No consecutive missions of same type
                        if (currentActiveMission!=null)
                        {
                            if (possibleMission.category == currentActiveMission.category || possibleMission.id == currentActiveMission.id)
                            {
                                //Debug.Log("11111111--->>>>>> " + possibleMission.id);
                                continue;
                            }
                        }

                        //Already done
                        if (possibleMission.Done)
                        {
                            //Debug.Log("22222222--->>>>>> " + possibleMission.id);
                            continue;
                        }

                        //Mission already active
                        if (possibleMission.Active >= 0)
                        {
                            //Debug.Log("2BBBBBB--->>>>>> " + possibleMission.id + " " + possibleMission.Active);
                            continue;
                        }

                        //Mission match tiers locked (by a tier or by a special car)
                        if (possibleMission.IsLocked(gameplayManager.EnvironmentFromIdx(i)))
                        {
                            //Debug.Log("33333333--->>>>>> " + possibleMission.id);
                            continue;
                        }

                        //Check for mission levels interval
                        if (!possibleMission.MatchPlayerLevel())
                        {
                            //Debug.Log("4444444--->>>>>> " + possibleMission.id);
                            continue;
                        }

                        //Check for right tier
                        if (!possibleMission.MatchSpecificTier(gameplayManager.EnvironmentFromIdx(i)))
                        {
                            //Debug.Log("5555555555--->>>>>> " + possibleMission.id);
                            continue;
                        }

                        //Reach combo mission sequence check
                        if (possibleMission.category == MissionCategory.ReachCombo)
                        {
                            if (!possibleMission.MatchReachComboSequence())
                            {
                                //Debug.Log("66666666--->>>>>> " + possibleMission.id);
                                continue;
                            }
                        }

                        //Avoid mission of the same category active togheter
                        if (CheckForMissionClassDuplicates(possibleMission))
                        {
                            //Debug.Log("777777--->>>>>> " + possibleMission.id);
                            continue;
                        }

                        //Debug.Log("ADD--->>>>>> " + possibleMission.id);
                        //Mission added to the list of possible missions
                        rndList.Add(possibleMission);
                    }
                }

                if (currentActiveMission != null)
                    currentActiveMission.resetMission();

                //Normally take the first mission, else choose one
                activeMissions[i] = null;
                if (rndList.Count > 0)
                {
                    if(forceSpecificTierFlags.Count>0)
                    {
                        for (int k = 0; k < rndList.Count; ++k)
                        {
                            DriverMission currForceTierMission = rndList[k];
                            if (currForceTierMission.MatchSpecificTier(forceSpecificTierFlags[0]))
                            {
                                activeMissions[i] = rndList[k];
                                activeMissions[i].Active = i;
                                activeMissions[i].CheckForDiamondReward(i);
                                break;
                            }
                        }
                        forceSpecificTierFlags.RemoveAt(0);
                    }
                    else if (forceSpecificSpecialVehicleFlags.Count>0)
                    {
                        for (int k = 0; k < rndList.Count; ++k)
                        {
                            DriverMission currForceSpecialVehicleMission = rndList[k];
                            if (currForceSpecialVehicleMission.MatchSpecificVehicle(forceSpecificSpecialVehicleFlags[0]))
                            {
                                activeMissions[i] = rndList[k];
                                activeMissions[i].Active = i;
                                activeMissions[i].CheckForDiamondReward(i);
                                break;
                            }
                        }
                        forceSpecificSpecialVehicleFlags.RemoveAt(0);
                    }

                    if (activeMissions[i] == null)
                    {
                       OnTheRunUtils.Shuffle<DriverMission>(rndList);
                        activeMissions[i] = rndList[0];
                        activeMissions[i].Active = i;

                        if(!firstTime)
                            activeMissions[i].CheckForDiamondReward(i);
                    }
                }

                activeMissions[i].RewardTaken = false;
            }
        }

        SaveTiersFlags();
        SaveSpecialVehiclesFlags();

        //FOR DEBUG...............
        for (int i = 0; i < activeMissions.Count; ++i)
        {
            if (activeMissions[i] == null)
                Debug.Log("*** ActiveSingleRunMission[" + i + "]: VOID");
            else
                Debug.Log("*** ActiveSingleRunMission[" + i + "]: " + activeMissions[i].id + " descriptionPrefix[0]: " + activeMissions[i].descriptionPrefix[0] + " description: " + activeMissions[i].description + " active: " + activeMissions[i].Active + " done: " + activeMissions[i].Done + " rewardTaken: " + activeMissions[i].RewardTaken);
        }

        int actvMissIndex = 0;
        for (int i = 0; i < activeMissions.Count; ++i)
        {
            if (activeMissions[i] != null)
                EncryptedPlayerPrefs.SetString(activeMissionSaveID + "_" + actvMissIndex, activeMissions[i].id);
            ++actvMissIndex;
        }

        if (Manager<UIManager>.Get().ActivePageName == "IngamePage")
            Manager<UIManager>.Get().ActivePage.SendMessage("UpdateMissionLateralPanel");
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        Instance = this;
        missionPassedInGame = new List<int>();
        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        missionsPanel = Manager<UIManager>.Get().gameObject.transform.FindChild("UI/MissionsPanel");
        missionPassedCounter = EncryptedPlayerPrefs.GetInt("miss_pc", 0);
    }

    void Start()
    {
        Init(false);
    }

    bool MissonLockedByTier(int tierIndex)
    {
        float[] playerLevelToUnlockTiers = OnTheRunDataLoader.Instance.GetTiersPlayerLevelThreshold();
        return playerLevelToUnlockTiers[tierIndex] == 0 || playerLevelToUnlockTiers[tierIndex] > PlayerPersistentData.Instance.Level;
    }

    public void Init(bool fullReset = false)
    {
        missions = new List<DriverMission>();
        activeMissions = new List<DriverMission>();
        for (int i = 0; i < numActiveMissions; ++i)
            activeMissions.Add(null);

        reachComboList = new List<DriverMission>();

        firstTimeMissions = new List<DriverMission>();
        if (EncryptedPlayerPrefs.GetInt(firstTimeGameStartID, 1)==1)
        {
            DriverMission first = new DriverMission(1, "dm_runfor_first", MissionCategory.RunFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("meters") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_run"), "", OnTheRunSingleRunMissions.MissionType.DriveForMetersMedium, true, metersToReachFirstTimeData, null);
            firstTimeMissions.Add(first);

            first = new DriverMission(2, "dm_destraff_first", MissionCategory.DestroyTraffic, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_traffic") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_vehicles"), "", OnTheRunSingleRunMissions.MissionType.DestroyTraffic, true, destroyTrafficFirstTimeData, null);
            firstTimeMissions.Add(first);

            /*first = new DriverMission(3, "dm_reachcombo3_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo3") }, new string[] { }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo3"), "", OnTheRunSingleRunMissions.MissionType.ReachCombo3, true, reachComboFirstTimeData, null);
            firstTimeMissions.Add(first);
            missions.Add(first);*/

            first = new DriverMission(3, "dm_jump_first", MissionCategory.Jump, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_jump") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_jump"), "", OnTheRunSingleRunMissions.MissionType.Jump, true, jumpQuantityFirstTimeData);
            firstTimeMissions.Add(first);

            first = new DriverMission(4, "dm_bonbigcoins_first", MissionCategory.CollectBigCoins, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_collect") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_bonus_big_coins") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_collect_short"), "", OnTheRunSingleRunMissions.MissionType.CollectBigCoins, true, collectBigCoinsFirstTimeData);
            firstTimeMissions.Add(first);
        }
        

        int missionIdex = 0;
        missions.Add(new DriverMission(++missionIdex, "dm_runfor", MissionCategory.RunFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_run") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("meters") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_run"), "", OnTheRunSingleRunMissions.MissionType.DriveForMetersMedium, true, metersToReachData));
        missions.Add(new DriverMission(++missionIdex, "dm_jump", MissionCategory.Jump, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_jump") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_jump"), "", OnTheRunSingleRunMissions.MissionType.Jump, true, jumpQuantityData));
        missions.Add(new DriverMission(++missionIdex, "dm_onair", MissionCategory.OnAir, MissionUnit.Seconds, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_stay_on_air") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("seconds") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_stay_on_air"), "", OnTheRunSingleRunMissions.MissionType.OnAirFor, true, onAirData));
        missions.Add(new DriverMission(++missionIdex, "dm_ckp", MissionCategory.Checkpoint, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_pass") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_checkpoint") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_through"), "", OnTheRunSingleRunMissions.MissionType.CheckpointPassed, true, passCheckpointData));
        missions.Add(new DriverMission(++missionIdex, "dm_boncoins", MissionCategory.CollectCoins, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_collect") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_coins") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_coins_short"), "", OnTheRunSingleRunMissions.MissionType.CollectCoins, true, collectCoinsData));
        missions.Add(new DriverMission(++missionIdex, "dm_bonbigcoins", MissionCategory.CollectBigCoins, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_collect") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_bonus_big_coins") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_collect_short"), "", OnTheRunSingleRunMissions.MissionType.CollectBigCoins, true, collectBigCoinsData));
        missions.Add(new DriverMission(++missionIdex, "dm_run_fast_lane_for", MissionCategory.CentralLaneUSAFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_fast_lane") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_fast_lane"), OnTheRunSingleRunMissions.MissionType.CentralLaneUSAFor, true, fastLaneMetersData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.USA }, OnTheRunGameplay.CarId.None, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_runcentralfor", MissionCategory.CentralLaneFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_central_lane") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_central_lane_short"), OnTheRunSingleRunMissions.MissionType.CentralLaneMeters, true, centralLaneMetersData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.Europe, OnTheRunEnvironment.Environments.Asia, OnTheRunEnvironment.Environments.NY }, OnTheRunGameplay.CarId.None, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_runleftfor", MissionCategory.LeftLaneFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_left_lane") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_left_lane_short"), OnTheRunSingleRunMissions.MissionType.LeftLaneMeters, true, leftLaneMetersData));
        missions.Add(new DriverMission(++missionIdex, "dm_runrightfor", MissionCategory.RightLaneFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_right_lane") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_right_lane_short"), OnTheRunSingleRunMissions.MissionType.RightLaneMeters, true, rightLaneMetersData));
        missions.Add(new DriverMission(++missionIdex, "dm_destraff", MissionCategory.DestroyTraffic, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_traffic") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_vehicles"), "", OnTheRunSingleRunMissions.MissionType.DestroyTraffic, true, destroyTrafficData));
        missions.Add(new DriverMission(++missionIdex, "dm_des_in_turbo", MissionCategory.DestroyCarsInTurbo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_in_turbo_offgame") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_vehicles"), OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_in_turbo_ingame"), OnTheRunSingleRunMissions.MissionType.DestroyCarsInTurbo, true, destroyTrafficInTurboData));
        missions.Add(new DriverMission(++missionIdex, "dm_des_in_turbo_w_lane", MissionCategory.DestroyCarsInTurboWrongLane, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_in_turbo_wrong_lane_offgame") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_vehicles"), OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_in_turbo_wrong_lane_ingame"), OnTheRunSingleRunMissions.MissionType.DestroyCarsInTurboWrongLane, true, destroyTrafficInTurboWrongLaneData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.Asia, OnTheRunEnvironment.Environments.USA }, OnTheRunGameplay.CarId.None, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_despolice_session_medium", MissionCategory.DestroyPolice, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_police_car_single"), OnTheRunDataLoader.Instance.GetLocaleString("mission_police_cars_plural") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy"), OnTheRunDataLoader.Instance.GetLocaleString("mission_police_cars_plural"), OnTheRunSingleRunMissions.MissionType.DestroyPolice, true, destroyPoliceQuantityData));
        missions.Add(new DriverMission(++missionIdex, "dm_avoid_barriers", MissionCategory.AvoidBarriers, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_avoid_barriers") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_avoid_barriers"), OnTheRunSingleRunMissions.MissionType.AvoidBarriers, true, avoidBarriersData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.NY }, OnTheRunGameplay.CarId.None, MissionType.None)));

        missions.Add(new DriverMission(++missionIdex, "dm_run_with_bigfoot", MissionCategory.UseSpecialVehicleFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_bigfoot") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_bigfoot"), OnTheRunSingleRunMissions.MissionType.UseBigfootFor, true, runWithBigfootForData, new LockCondition(null, OnTheRunGameplay.CarId.Bigfoot, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_run_with_tank", MissionCategory.UseSpecialVehicleFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_tank") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_tank"), OnTheRunSingleRunMissions.MissionType.UseTankFor, true, runWithTankForData, new LockCondition(null, OnTheRunGameplay.CarId.Tank, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_run_with_firetruck", MissionCategory.UseSpecialVehicleFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_firetruck") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_firetruck"), OnTheRunSingleRunMissions.MissionType.UseFiretruckFor, true, runWithFiretruckForData, new LockCondition(null, OnTheRunGameplay.CarId.Firetruck, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_run_with_ufo", MissionCategory.UseSpecialVehicleFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_ufo") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_ufo"), OnTheRunSingleRunMissions.MissionType.UseUFoFor, true, runWithUFOForData, new LockCondition(null, OnTheRunGameplay.CarId.Ufo, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_run_with_plane", MissionCategory.UseSpecialVehicleFor, MissionUnit.Meters, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_plane") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_for"), OnTheRunDataLoader.Instance.GetLocaleString("mission_meters_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_plane"), OnTheRunSingleRunMissions.MissionType.UsePlaneFor, true, runWithPlaneForData, new LockCondition(null, OnTheRunGameplay.CarId.Plane, MissionType.None)));

        missions.Add(new DriverMission(++missionIdex, "dm_des_with_bigfoot", MissionCategory.DestroyWithBigfoot, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_smash_bigfoot_long") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_vehicles_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_bigfoot") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_smash_bigfoot_short"), OnTheRunDataLoader.Instance.GetLocaleString("mission_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_bigfoot"), OnTheRunSingleRunMissions.MissionType.DestroyCarsWithBigfoot, true, destroyCarsDataWSpecial, new LockCondition(null, OnTheRunGameplay.CarId.Bigfoot, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_des_with_tank", MissionCategory.DestroyWithTank, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_vehicles_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_tank") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_vehicles"), OnTheRunDataLoader.Instance.GetLocaleString("mission_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_tank"), OnTheRunSingleRunMissions.MissionType.DestroyCarsWithTank, true, destroyCarsDataWSpecial, new LockCondition(null, OnTheRunGameplay.CarId.Tank, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_des_with_ufo", MissionCategory.DestroyWithUFO, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_abduct_ufo_long") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_vehicles_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_ufo") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_abduct_ufo_short"), OnTheRunDataLoader.Instance.GetLocaleString("mission_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_ufo"), OnTheRunSingleRunMissions.MissionType.DestroyCarsWithUfo, true, destroyCarsDataWSpecial, new LockCondition(null, OnTheRunGameplay.CarId.Ufo, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_des_with_plane", MissionCategory.DestroyWithPlane, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_vehicles_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_plane") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_vehicles"), OnTheRunDataLoader.Instance.GetLocaleString("mission_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_plane"), OnTheRunSingleRunMissions.MissionType.DestroyCarsWithPlane, true, destroyCarsDataWSpecial, new LockCondition(null, OnTheRunGameplay.CarId.Plane, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_des_with_firetruck", MissionCategory.DestroyWithFiretruck, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_pushaway_firetruck_long") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_vehicles_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_firetruck") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_pushaway_firetruck_short"), OnTheRunDataLoader.Instance.GetLocaleString("mission_with") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_firetruck"), OnTheRunSingleRunMissions.MissionType.DestroyCarsWithFiretruck, true, destroyCarsDataWSpecial, new LockCondition(null, OnTheRunGameplay.CarId.Firetruck, MissionType.None)));

        missions.Add(new DriverMission(++missionIdex, "dm_des_bigfoot_w_lane", MissionCategory.DestroyCarsWithSpecialInWrongLane, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_smash_bigfoot_long") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_bigfoot") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_smash_bigfoot_short"), OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_bigfoot"), OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithBigfoot, true, destroyCarsWithSpecialInWrongLaneData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.Asia, OnTheRunEnvironment.Environments.USA }, OnTheRunGameplay.CarId.Bigfoot, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_des_tank_w_lane", MissionCategory.DestroyCarsWithSpecialInWrongLane, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_tank") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_vehicles"), OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_tank"), OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithTank, true, destroyCarsWithSpecialInWrongLaneData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.Asia, OnTheRunEnvironment.Environments.USA }, OnTheRunGameplay.CarId.Tank, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_des_firetruck_w_lane", MissionCategory.DestroyCarsWithSpecialInWrongLane, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_pushaway_firetruck_long") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_firetruck") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_pushaway_firetruck_short"), OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_firetruck"), OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithFiretruck, true, destroyCarsWithSpecialInWrongLaneData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.Asia, OnTheRunEnvironment.Environments.USA }, OnTheRunGameplay.CarId.Firetruck, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_des_ufo_w_lane", MissionCategory.DestroyCarsWithSpecialInWrongLane, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_abduct_ufo_long") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_ufo") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_abduct_ufo_short"), OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_ufo"), OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithUfo, true, destroyCarsWithSpecialInWrongLaneData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.Asia, OnTheRunEnvironment.Environments.USA }, OnTheRunGameplay.CarId.Ufo, MissionType.None)));
        missions.Add(new DriverMission(++missionIdex, "dm_des_plane_w_lane", MissionCategory.DestroyCarsWithSpecialInWrongLane, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_plane") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_destroy_vehicles"), OnTheRunDataLoader.Instance.GetLocaleString("mission_wrong_lane") + " " + OnTheRunDataLoader.Instance.GetLocaleString("special_vehicle_plane"), OnTheRunSingleRunMissions.MissionType.DestroyCarsInWrongLaneWithPlane, true, destroyCarsWithSpecialInWrongLaneData, new LockCondition(new OnTheRunEnvironment.Environments[] { OnTheRunEnvironment.Environments.Asia, OnTheRunEnvironment.Environments.USA }, OnTheRunGameplay.CarId.Plane, MissionType.None)));
       
        /*missions.Add(new DriverMission(++missionIdex, "dm_lane_session_medium", MissionCategory.ChangeLane, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_change_lane") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunSingleRunMissions.MissionType.ChangeLane, true, changeLaneQuantityMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_dontlane_medium", MissionCategory.DontChangeLane, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_dont_change_lane") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("seconds") }, OnTheRunSingleRunMissions.MissionType.DontChangeLane, true, dontchangeLaneQuantityMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_ckp_trubo_medium", MissionCategory.CheckpointWithTurbo, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_through") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_checkpoint_with_turbo") }, OnTheRunSingleRunMissions.MissionType.CheckpointPassedInTurbo, true, checkpointInTurboQuantityMedium));
        missions.Add(new DriverMission(++missionIdex, "dm_rtruck_session_medium", MissionCategory.GetOverRamp, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_getover") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_turbo_truck") }, OnTheRunSingleRunMissions.MissionType.OverRedTruck, true, turboTruckQuantityMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_boncoins_session_medium", MissionCategory.CollectCoins, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_collect") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_coins") }, OnTheRunSingleRunMissions.MissionType.CollectCoins, true, bonusCoinsQuantityMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_bonshield_session_medium", MissionCategory.CollectShield, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_collect") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_bonus_shield") }, OnTheRunSingleRunMissions.MissionType.CollectShield, true, bonusShieldQuantityMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_bonturbo_session_medium", MissionCategory.CollectBolt, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_collect") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_bonus_turbo") }, OnTheRunSingleRunMissions.MissionType.CollectBolt, true, bonusTurboQuantityMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_bonmagnet_medium_session", MissionCategory.CollectMagnet, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_collect") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_bonus_magnet") }, OnTheRunSingleRunMissions.MissionType.CollectMagnet, true, bonusMagnetQuantityMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_bumptraffic_session_medium", MissionCategory.BumpOn, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_bump_on") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_traffic") }, OnTheRunSingleRunMissions.MissionType.BumpOnTraffic, true, bumpOnTrafficQuantityMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_drafts_session_medium", MissionCategory.Slipstream, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_draft") + " " + slipstreamQuantityMedium_Session + " " + OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunSingleRunMissions.MissionType.CarSlipstream, true, slipstreamQuantityMedium_Session));
        */

        /*missions.Add(new DriverMission(++missionIdex, "dm_descar1_session_medium", MissionCategory.DestroyCarEurope, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_campers") }, OnTheRunSingleRunMissions.MissionType.DestroyCar1, true, destroyCar1QuantityMedium_Session));
        if (!MissonLockedByTier(2))
        {
            missions.Add(new DriverMission(++missionIdex, "dm_descar2_session_medium", MissionCategory.DestroyCarNY, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_cabs") }, OnTheRunSingleRunMissions.MissionType.DestroyCar2, true, destroyCar2QuantityMedium_Session));
        }
        if (!MissonLockedByTier(1))
        {
            missions.Add(new DriverMission(++missionIdex, "dm_descar3_session_medium", MissionCategory.DestroyCarAsia, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_oriental_trucks") }, OnTheRunSingleRunMissions.MissionType.DestroyCar3, true, destroyCar3QuantityMedium_Session));
        }
        if (!MissonLockedByTier(3))
        {
            missions.Add(new DriverMission(++missionIdex, "dm_descar4_session_medium", MissionCategory.DestroyCarUSA, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("destroy") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_oil_trucks") }, OnTheRunSingleRunMissions.MissionType.DestroyCar4, true, destroyCar4QuantityMedium_Session));
        }*/

        /*missions.Add(new DriverMission(++missionIdex, "dm_usetank_session_medium", MissionCategory.GetTank, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_use_tank") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("time"), OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunSingleRunMissions.MissionType.UseTank, true, useTankQuantityMedium_session));
        missions.Add(new DriverMission(++missionIdex, "dm_usebigfoot_session", MissionCategory.GetBigfoot, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_use_bigfoot") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("time"), OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunSingleRunMissions.MissionType.UseBigfoot, true, useBigfootQuantity_session));
        missions.Add(new DriverMission(++missionIdex, "dm_useufo_session", MissionCategory.GetUFO, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_use_ufo") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("time"), OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunSingleRunMissions.MissionType.UseUFo, true, useUfoQuantity_session));
        missions.Add(new DriverMission(++missionIdex, "dm_useplane_session_medium", MissionCategory.GetPlane, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_use_plane") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("time"), OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunSingleRunMissions.MissionType.UsePlane, true, usePlaneQuantityMedium_session));
        missions.Add(new DriverMission(++missionIdex, "dm_usefiretruck_session_medium", MissionCategory.GetFiretruck, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_use_firetruck") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("time"), OnTheRunDataLoader.Instance.GetLocaleString("times") }, OnTheRunSingleRunMissions.MissionType.UseFiretruck, true, useFiretruckQuantityMedium_session));
        missions.Add(new DriverMission(++missionIdex, "dm_neverslipstream_session_medium", MissionCategory.NeverSlipstream, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_never_slipstream") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("meters") }, OnTheRunSingleRunMissions.MissionType.NeverSlipstream, true, neverSlipstreamMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_wronddirection_session_medium", MissionCategory.WrongDirection, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_wrong_dir") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("meters") }, OnTheRunSingleRunMissions.MissionType.WrongDirection, true, wrongDirectionMedium_Session));
        missions.Add(new DriverMission(++missionIdex, "dm_wronddirection", MissionCategory.WrongDirection, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_drive_wrong_dir") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("meters").ToLower() }, OnTheRunSingleRunMissions.MissionType.WrongDirection, false, wrongDirection));
        */

        //DriverMission reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo2_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo2") }, new string[] { }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo2"), "", OnTheRunSingleRunMissions.MissionType.ReachCombo2, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.None));
        //missions.Add(reachComboMission);
        //reachComboList.Add(reachComboMission);

        //reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo3_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo3") }, new string[] { }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo3"), "", OnTheRunSingleRunMissions.MissionType.ReachCombo3, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo2));
        //missions.Add(reachComboMission);
        //reachComboList.Add(reachComboMission);

        DriverMission reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo4_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo4, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo3));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo5_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo5, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo4));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo6_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo6, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo5));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo7_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo7, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo6));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo8_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo8, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo7));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo9_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo9, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo8));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo10_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo10, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo9));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo11_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo11, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo10));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo12_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo12, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo11));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo13_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo13, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo12));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo14_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo14, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo13));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);

        reachComboMission = new DriverMission(++missionIdex, "dm_reachcombo15_session", MissionCategory.ReachCombo, MissionUnit.Times, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix") }, new string[] { OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix") }, OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_prefix"), OnTheRunDataLoader.Instance.GetLocaleString("mission_reach_combo_postfix"), OnTheRunSingleRunMissions.MissionType.ReachCombo15, true, reachComboData, new LockCondition(null, OnTheRunGameplay.CarId.None, MissionType.ReachCombo14));
        missions.Add(reachComboMission);
        reachComboList.Add(reachComboMission);
          
        maxReachComboIndex = EncryptedPlayerPrefs.GetInt("maxrc_idx", simultaneouslyReachComboNum + 1);

        OnTheRunUtils.Shuffle<DriverMission>(missions);
        
        List<string> savedActiveMissions = new List<string>();
        for (int i = 0; i < numActiveMissions; ++i)
            savedActiveMissions.Add(EncryptedPlayerPrefs.GetString(activeMissionSaveID + "_" + i, ""));

        /*for (int i = 0; i < savedActiveMissions.Count; ++i)
        {
            Debug.Log("--> " + savedActiveMissions[i]);
        }*/

        for (int i = 0; i < missions.Count; ++i)
        {
            DriverMission mission = missions[i];
            if (savedActiveMissions.IndexOf(mission.id) >= 0)
            {
                //Debug.Log("--> " + mission.id + " ::: " + mission.type+" "+ mission.Active);
                activeMissions[mission.Active] = mission;
            }
        }

        bool oneWasFirst = false;
        for (int i = 0; i < activeMissions.Count; ++i)
        {
            if (activeMissions[i] == null)
            {
                oneWasFirst = true;
                Debug.Log("*** ActiveSingleRunMission[" + i + "]: VOID");
            }
            else
                Debug.Log("*** ActiveSingleRunMission[" + i + "]: " + activeMissions[i].id + " descriptionPrefix[0]: " + activeMissions[i].descriptionPrefix[0] + " description: " + activeMissions[i].description + " active: " + activeMissions[i].Active + " done: " + activeMissions[i].Done + " rewardTaken: " + activeMissions[i].RewardTaken);
        }

        /*int actvMissIndex = 0;
        for (int i = 0; i < activeMissions.Count; ++i)
        {
            DriverMission mission = activeMissions[i];
            if (mission != null)
                EncryptedPlayerPrefs.SetString(activeMissionSaveID + "_" + actvMissIndex, mission.id);
            ++actvMissIndex;
        }*/

        int index = 0;
        for (int i = 0; i < activeMissions.Count; ++i)
        {
            DriverMission currMission = activeMissions[i];
            if (currMission != null)
            {
                if (currMission.JustPassed)
                    missionPassedInGame.Add(index);
                ++index;
            }
        }

        //load force mission flags
        LoadTiersFlags();
        LoadSpecialVehiclesFlags();

        if (oneWasFirst)
            UpdateActiveMissions(true);
    }

    #region Force mission flags
    public void AddMissionTierConstrain(OnTheRunEnvironment.Environments env)
    {
        forceSpecificTierFlags.Add(env);
    }
    public void AddMissionSpecialVehicleConstrain(OnTheRunGameplay.CarId carId)
    {
        forceSpecificSpecialVehicleFlags.Add(carId);
    }

    void LoadTiersFlags()
    {
        forceSpecificTierCounter = PlayerPrefs.GetInt(forceSpecificTierCounterID + "_count", 0);
        forceSpecificTierFlags = new List<OnTheRunEnvironment.Environments>();
        for (int i = 0; i < forceSpecificTierCounter; ++i)
        {
            OnTheRunEnvironment.Environments tmpData = (OnTheRunEnvironment.Environments)PlayerPrefs.GetInt(forceSpecificTierCounterID + "_" + i, 0);
            forceSpecificTierFlags.Add(tmpData);
        }
    }

    void LoadSpecialVehiclesFlags()
    {
        forceSpecificSpecialVehicleCounter = PlayerPrefs.GetInt(forceSpecificSpecialVehicleCounterID + "_count", 0);
        forceSpecificSpecialVehicleFlags = new List<OnTheRunGameplay.CarId>();
        for (int i = 0; i < forceSpecificSpecialVehicleCounter; ++i)
        {
            OnTheRunGameplay.CarId tmpData = (OnTheRunGameplay.CarId)PlayerPrefs.GetInt(forceSpecificSpecialVehicleCounterID + "_" + i, 0);
            forceSpecificSpecialVehicleFlags.Add(tmpData);
        }
    }

    void SaveTiersFlags()
    {
        forceSpecificTierCounter = PlayerPrefs.GetInt(forceSpecificTierCounterID + "_count", 0);
        for (int i = 0; i < forceSpecificTierCounter; ++i)
            PlayerPrefs.DeleteKey(forceSpecificTierCounterID + "_" + i);

        PlayerPrefs.SetInt(forceSpecificTierCounterID + "_count", forceSpecificTierFlags.Count);
        for (int i = 0; i < forceSpecificTierFlags.Count; ++i)
            PlayerPrefs.SetInt(forceSpecificTierCounterID + "_" + i, (int)forceSpecificTierFlags[i]);
    }

    void SaveSpecialVehiclesFlags()
    {
        forceSpecificSpecialVehicleCounter = PlayerPrefs.GetInt(forceSpecificSpecialVehicleCounterID + "_count", 0);
        for (int i = 0; i < forceSpecificSpecialVehicleCounter; ++i)
            PlayerPrefs.DeleteKey(forceSpecificSpecialVehicleCounterID + "_" + i);

        PlayerPrefs.SetInt(forceSpecificSpecialVehicleCounterID + "_count", forceSpecificSpecialVehicleFlags.Count);
        for (int i = 0; i < forceSpecificSpecialVehicleFlags.Count; ++i)
            PlayerPrefs.SetInt(forceSpecificSpecialVehicleCounterID + "_" + i, (int)forceSpecificSpecialVehicleFlags[i]);
    }
    #endregion

    void TryIncrementReachComboMissionWindow(DriverMission currMission)
    {
        if (currMission.id.Contains("reachcombo"))
        {
            string missionPostfix = currMission.id.Substring(currMission.id.IndexOf("reachcombo") + "reachcombo".Length);
            int underscoreIndex = missionPostfix.IndexOf('_');
            if (underscoreIndex > 0)
            {
                int comboNumber = int.Parse(missionPostfix.Substring(0, underscoreIndex), CultureInfo.InvariantCulture);
                ++maxReachComboIndex;
                EncryptedPlayerPrefs.SetInt("maxrc_idx", maxReachComboIndex);
            }
        }
    }

    /*bool CanEvaluateMission(DriverMission currMission)
    {
        bool retValue = false;
        OnTheRunGameplay.CarId carId = gameplayManager.PlayerKinematics.carId;
        int tierIndex = gameplayManager.EnvironmentIdx(environmentManager.currentEnvironment);
        int laneIndex = gameplayManager.PlayerKinematics.CurrentLane;

        if (currMission != null && !currMission.Done)
        {
            bool matchCar = currMission.MatchSpecificVehicle(carId);
            bool matchLane = currMission.MatchSpecificLane(laneIndex);
            retValue = matchCar && matchLane;
        }

        return retValue;
    }*/

    void DoMissionEvent(OnTheRunSingleRunMissions.MissionType type, int increment = 1)
    {
        if (!isActive)
            return;

        if (OnTheRunTutorialManager.Instance.TutorialActive)
            return;

        DriverMission currentMission = CurrentTierMission;
        //if (!CanEvaluateMission(currentMission))
        //    return;

        if (currentMission.type == type)
        {
            currentMission.Counter += increment;

            if (currentMission.Counter >= currentMission.checkCounter && !currentMission.JustPassed && !currentMission.Done)
            {
                //Debug.Log("MISSION DONE: " + currentMission.id + " c: " + currentMission.Counter + " cc: " + currentMission.checkCounter);
                currentMission.Done = true;
                missionsPanel.GetComponent<UIMissionsPanel>().ShowMissionsPanel(currentMission.type);
                NotifyMissionDone(currentMission);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_MEDIUM);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_HARD);
            }
        }
    }


    void HitBarrier()
    {
        DriverMission currentMission = CurrentTierMission;
        if (currentMission != null && currentMission.type == OnTheRunSingleRunMissions.MissionType.AvoidBarriers)
        {
            currentMission.Counter = 0.0f;
        }
    }

    void ResetActiveGameMissions()
    {
        //Debug.Log("MISSSIONS: ResetActiveGameMissions");

        DriverMission currentMission = CurrentTierMission;
        if (currentMission!=null)
            currentMission.Counter = 0.0f;
    }
    
    void Update()
    {
        if (TimeManager.Instance.MasterSource.IsPaused)
            return;

        if (OnTheRunTutorialManager.Instance.TutorialActive)
            return;

        if (playerKinematics == null)
            playerKinematics = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        if (!playerKinematics.PlayerIsStopping && !gameplayManager.Gameplaystarted)
            return;

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        if (!isActive)
            return;

        float sessionMeters = playerRef.SessionMeters * OnTheRunUtils.ToOnTheRunMeters,
              deltaMeters = 0.0f;
        if (float.IsNaN(sessionMeters))
        {
            Debug.Log("***float.IsNaN(sessionMeters): ");
            sessionMeters = 0.0f;
        }
        else
            deltaMeters = SBSMath.Max(0.0f, sessionMeters - previousMeters);

        //Debug.Log("***deltaMeters: " + deltaMeters + " sessionMeters: " + sessionMeters + " previousMeters: " + previousMeters + " playerRef.SessionMeters: " + playerRef.SessionMeters * OnTheRunUtils.ToOnTheRunMeters);

        if (sessionMeters <= 0)
            deltaMeters = 0.0f;
        else if (deltaMeters > 50)
        {
            deltaMeters = 0.0f;
            previousMeters = sessionMeters;// *OnTheRunUtils.ToRealMeters;
        }
        else
            previousMeters = sessionMeters;

        if (achMetersMed != null && achMetersHard != null)
        {
            if (achMetersHardStartCounter >= 0.0f)
            {
                if (achMetersMediumStartCounter + uiSharedData.InterfaceDistance >= achMetersMed.levelToReach)
                {
                    achMetersMediumStartCounter = -1.0f;
                    OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.METERS_MEDIUM, uiSharedData.InterfaceDistance);
                }
            }

            if (achMetersHardStartCounter >= 0.0f)
            {
                if (achMetersHardStartCounter + uiSharedData.InterfaceDistance >= achMetersHard.levelToReach)
                {
                    achMetersHardStartCounter = -1.0f;
                    OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_HARD, uiSharedData.InterfaceDistance);
                }
            }
        }

        //Update runtime mission------------------
        if (chooseRunTimeMission)
        {
            chooseRunTimeMission = false;
            missionUpdatedRunTime = CurrentTierMission;
        }

        if (missionUpdatedRunTime != null)
        {
            switch (missionUpdatedRunTime.type)
            {
                case OnTheRunSingleRunMissions.MissionType.DriveForMetersMedium:
                    missionUpdatedRunTime.Counter += deltaMeters;
                    //Debug.Log("** mission.Counter: " + mission.Counter);
                    break;
                case OnTheRunSingleRunMissions.MissionType.UseBigfootFor:
                case OnTheRunSingleRunMissions.MissionType.UseFiretruckFor:
                case OnTheRunSingleRunMissions.MissionType.UseTankFor:
                case OnTheRunSingleRunMissions.MissionType.UseUFoFor:
                case OnTheRunSingleRunMissions.MissionType.UsePlaneFor:
                    if (missionUpdatedRunTime.MatchSpecificVehicle(gameplayManager.CurrentSpecialCar))
                        missionUpdatedRunTime.Counter += deltaMeters;
                    break;
                case OnTheRunSingleRunMissions.MissionType.CentralLaneUSAFor:
                    if (playerKinematics.CurrentLane == 2 && playerDraftRef.DraftUSAActive)
                    {
                        missionUpdatedRunTime.Counter += deltaMeters;
                    }
                    break;
                case OnTheRunSingleRunMissions.MissionType.AvoidBarriers:
                    missionUpdatedRunTime.Counter += deltaMeters;
                    break;
                case OnTheRunSingleRunMissions.MissionType.CentralLaneMeters:
                    if (playerKinematics.CurrentLane == 2)
                    {
                        missionUpdatedRunTime.Counter += deltaMeters;
                        //Debug.Log("mission.Counter " + mission.Counter + " deltaMeters " + deltaMeters);
                    }
                    break;
                case OnTheRunSingleRunMissions.MissionType.LeftLaneMeters:
                    if (playerKinematics.CurrentLane == 0 || playerKinematics.CurrentLane == 1)
                        missionUpdatedRunTime.Counter += deltaMeters;
                    break;
                case OnTheRunSingleRunMissions.MissionType.RightLaneMeters:
                    if (playerKinematics.CurrentLane == 3 || playerKinematics.CurrentLane == 4)
                        missionUpdatedRunTime.Counter += deltaMeters;
                    break;
                /*case OnTheRunSingleRunMissions.MissionType.NeverSlipstream:
                    if (!playerKinematics.SlipstreamOn)
                        missionUpdatedRunTime.Counter += deltaMeters;
                    else
                        missionUpdatedRunTime.Counter = 0.0f;
                    break;*/
                case OnTheRunSingleRunMissions.MissionType.OnAirFor:
                    if (playerKinematics.IsOnAir)
                        missionUpdatedRunTime.Counter += dt;
                    break;
                /*case OnTheRunSingleRunMissions.MissionType.WrongDirection:
                    if (playerKinematics.WrongDirection)
                        missionUpdatedRunTime.Counter += deltaMeters;
                    break;*/
                /*case OnTheRunSingleRunMissions.MissionType.DontChangeLane:
                    if (playerKinematics.PrevLane != playerKinematics.CurrentLane)
                        missionUpdatedRunTime.Counter = 0.0f;
                    else
                        missionUpdatedRunTime.Counter += dt;
                    break;*/
            }

            if (missionUpdatedRunTime.Counter >= missionUpdatedRunTime.checkCounter && !missionUpdatedRunTime.JustPassed && !missionUpdatedRunTime.Done)
            {
                //Debug.Log("MISSION DONE: " + mission.id + " c: " + mission.Counter + " cc: " + mission.checkCounter + " done: " + mission.Done + " " + mission.Active);
                missionUpdatedRunTime.Done = true; // PIETRO
                missionsPanel.GetComponent<UIMissionsPanel>().ShowMissionsPanel(missionUpdatedRunTime.type);
                NotifyMissionDone(missionUpdatedRunTime);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_MEDIUM);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_HARD);
            }
        }
    }

    public void ResetMissionsData()
    {
        for (int i = 0; i < missions.Count; ++i)
        {
            missions[i].reset();
        }
    }
    
    public void NotifyMissionDone(DriverMission mission)
    {
        int index = 0;
        //int dailyCompleted = 0;
        for (int i = 0; i < activeMissions.Count; ++i)
        {
            if (activeMissions[i] == mission && !missionPassedInGame.Contains(index))
            {
                mission.JustPassed = true;
                //Debug.Log("ADD missionPassedInGame " + index);
                missionPassedInGame.Add(index);
                mission.save();
                if (mission.id.Contains("reachcombo"))
                    reachComboActive = false;

                break;
            }
            ++index;
        }


        /*for (int i = 0; i < activeMissions.Count; ++i)
        {
            if (activeMissions[i].JustPassed || activeMissions[i].Done)
                ++dailyCompleted;
        }*/
    }

    public void SetRewardTaken(DriverMission mission)
    {
        mission.RewardTaken = true;
        if (firstTimeMissions.IndexOf(mission) >= 0 && mission.Done)
        { 
            EncryptedPlayerPrefs.SetInt(firstTimeGameStartID, 0);
            
            for (int i = 0; i < missions.Count; ++i)
            {
                if (missions[i].id == "dm_runfor_first")
                {
                    missions.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void EvaluateMissionPassed(DriverMission mission)
    {
        if (mission.JustPassed)
        {
            int index = 0;
            //foreach (DriverMission currMission in activeMissions)
            for (int i = 0; i < activeMissions.Count; ++i)
            {
                if (activeMissions[i] == mission && !missionPassedInGame.Contains(index))
                {
                    if (missionPassedInGame.Count > index) //???
                        missionPassedInGame.RemoveAt(index);
                    break;
                }
                ++index;
            }
        }

        UpdateActiveMissions();

        mission.JustPassed = false;
        mission.save();

        ++missionPassedCounter;
        EncryptedPlayerPrefs.SetInt("miss_pc", missionPassedCounter);

        if (mission.id.Contains("reachcombo"))
            reachComboActive = false;

        //mission.save();

        OnTheRunInterstitialsManager.Instance.TriggerInterstitial(OnTheRunInterstitialsManager.TriggerPoint.MissionCompleted);
    }

    public void CheatPassMission()
    {
        DriverMission mission = CurrentTierMission;// activeMissions[0];
        if (mission==null || mission.Done || mission.JustPassed)
            return;

        missionsPanel.GetComponent<UIMissionsPanel>().ShowMissionsPanel(mission.type);
        mission.Counter = mission.checkCounter;
        Debug.Log("CheatPassMission: " + mission.type + " " + mission.Counter + " " + mission.checkCounter);
        mission.Done = true; // PIETRO
        NotifyMissionDone(mission);
    }

    public void BuyCurrentTierMission()
    {
        DriverMission currentMission = CurrentTierMission;
        currentMission.Done = true;
        NotifyMissionDone(currentMission);

        Manager<UIManager>.Get().ActivePage.SendMessage("GiveMissionSkippedReward");

        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_MEDIUM);
        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MISSION_HARD);
        //UpdateActiveMissions();
    }
    #endregion
}