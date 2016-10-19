using UnityEngine;
using System.Collections;
using SBS.Core;
using System.Collections.Generic;

public class OnTheRunIngameHiScoreCheck : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunIngameHiScoreCheck instance = null;

    public static OnTheRunIngameHiScoreCheck Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    public class SwapData
    {
        public McSocialApiUtils.ScoreData old_better;
        public McSocialApiUtils.ScoreData new_better;

        public SwapData(McSocialApiUtils.ScoreData oldBetter, McSocialApiUtils.ScoreData newBetter)
        {
            old_better = oldBetter;
            new_better = newBetter;
        }
    }

    bool isInGameplay;
    bool hasReachedTopOfTheLeaderboard;
    bool bothRanksAreVisible;
    bool isPlayingSwapAnimation;

    OnTheRunGameplay gameplayManager;
    Dictionary<UIIngameRanks.Position, bool> ranksVisibilityByPosition;

    UIIngamePage ingamePage;
    UISharedData uiSharedData;

    Queue<McSocialApiUtils.ScoreData> better_hiScoresQueue;
    McSocialApiUtils.ScoreData currentScoreToBeat;

    Queue<SwapData> swapDataQueue;

    UIIngameRanks ingameRanks;

    public List<McSocialApiUtils.ScoreData> reverseOrderedScores = null;
    public bool IsInGameplay { get { return isInGameplay; } }
    public bool HasReachedTopOfTheLeaderboard { get { return hasReachedTopOfTheLeaderboard; } }
    public string NextOpponentsName { get { return currentScoreToBeat.Name; } }
    public long NextOpponentsScore { get { return currentScoreToBeat.Score; } }
    public long MetersToBeatNextOpponent { get { return currentScoreToBeat.Score - uiSharedData.InterfaceDistance; } }
    public Sprite NextOpponentsPicture { get { return GetCurrentOpponentsPicture(); } }
    public bool IsNextOpponentFacebookFriend { get { return IsOpponentFacebookFriend(); } }
    public bool IsNextOpponentMe { get { return IsOpponentMe(); } }

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
        DontDestroyOnLoad(gameObject);

        isInGameplay = false;
        hasReachedTopOfTheLeaderboard = false;
        better_hiScoresQueue = null;
        swapDataQueue = null;
        currentScoreToBeat = null;

        ingameRanks = null;

        isPlayingSwapAnimation = false;
        
        ranksVisibilityByPosition = new Dictionary<UIIngameRanks.Position, bool>();
        ranksVisibilityByPosition.Add(UIIngameRanks.Position.Enter, false);
        ranksVisibilityByPosition.Add(UIIngameRanks.Position.Focus, false);
    }

    public void SetUIReferences(UIIngamePage page, UIIngameRanks uiIngameRanks)
    {
        ingamePage = page;
        Asserts.Assert(ingamePage != null);

        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();
        Asserts.Assert(uiSharedData != null);

        if (ingameRanks == null)
        {
            ingameRanks = uiIngameRanks;
            Asserts.Assert(ingameRanks != null);

            //HideRanks();
            //HideSingleRank(UIIngameRanks.Position.Focus);
            //HideSingleRank(UIIngameRanks.Position.Enter);
        }
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    public void OnGameplayStarted()
    {
        isInGameplay = false;
        hasReachedTopOfTheLeaderboard = false;
        better_hiScoresQueue = new Queue<McSocialApiUtils.ScoreData>();
        swapDataQueue = new Queue<SwapData>();
        currentScoreToBeat = null;

        isPlayingSwapAnimation = false;

        ingameRanks.ResetPositions();
        HideSingleRank(UIIngameRanks.Position.Enter);
        HideSingleRank(UIIngameRanks.Position.Focus);

#if(!(UNITY_WEBPLAYER || UNITY_WEBGL))
        StartCoroutine(GameplayStartedCoroutine());
#endif
    }

    IEnumerator GameplayStartedCoroutine()
    {
        while (!McSocialApiManager.Instance.ScoresForIngameAreReady || OnTheRunTutorialManager.Instance.TutorialActive)
            yield return new WaitForEndOfFrame();
        
        if (McSocialApiManager.Instance.LastRequestedScores == null)
        {
            yield break;
        }

		if (McSocialApiManager.Instance.LastRequestedScores != null)
		{
            reverseOrderedScores = new List<McSocialApiUtils.ScoreData>(McSocialApiManager.Instance.LastRequestedScores);

	        /*Debug.Log("######### HI SCORE CHECK");
	        McSocialApiManager.Instance.LogScores(reverseOrderedScores);*/

            reverseOrderedScores.Sort(
	            delegate(McSocialApiUtils.ScoreData score1, McSocialApiUtils.ScoreData score2)
	            {
	                return -1 * score1.Rank.CompareTo(score2.Rank);
	            });

	        //const bool ALLOW_ALSO_NON_FB_USERS = true;

            foreach (var score in reverseOrderedScores)
	            //if (ALLOW_ALSO_NON_FB_USERS || score.LoginType == McSocialApiUtils.LoginType.Facebook)
	                better_hiScoresQueue.Enqueue(score);
		}
        /*const bool FORCE_USE_TEST_HI_SCORES = false;
        if (FORCE_USE_TEST_HI_SCORES)
            better_hiScoresQueue = GetFakeTestScores();*/

        if (better_hiScoresQueue != null && better_hiScoresQueue.Count > 0)
            currentScoreToBeat = better_hiScoresQueue.Dequeue();
                
        ShowSingleRank(UIIngameRanks.Position.Focus); //ingameRanks.ShowRank(UIIngameRanks.Position.Focus);
        ShowRankMeters(UIIngameRanks.Position.Focus);

        if (currentScoreToBeat != null)
        {
            SetOpponentToBeatRank(UIIngameRanks.Position.Focus, currentScoreToBeat.Rank);
            bothRanksAreVisible = true;
            isInGameplay = true;
            
            if (Manager<UIManager>.Get().ActivePageName == "IngamePage")
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().ShowFBAdvise(OnTheRunFacebookManager.Instance.IsLoggedIn);
        }
        else
        {
            HideSingleRank(UIIngameRanks.Position.Enter);
            HideSingleRank(UIIngameRanks.Position.Focus);
            isInGameplay = false;
            bothRanksAreVisible = false;
        }

        if (gameplayManager==null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        gameplayManager.GetComponent<OnTheRunFBFriendsSpawner>().Initialize();
    }

    Queue<McSocialApiUtils.ScoreData> GetFakeTestScores()
    {
        Queue<McSocialApiUtils.ScoreData> testQueue = new Queue<McSocialApiUtils.ScoreData>();
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_10", "IT", 200, 10, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_9", "IT",  300, 9, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_8", "IT",  400, 8, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_7", "IT",  500, 7, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_6", "IT",  600, 6, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_5", "IT",  700, 5, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_4", "IT",  800, 4, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_3", "IT",  900, 3, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_2", "IT",  1000, 2, McSocialApiUtils.LoginType.Guest, string.Empty));
        testQueue.Enqueue(new McSocialApiUtils.ScoreData(string.Empty, "USER_1", "IT",  1100, 1, McSocialApiUtils.LoginType.Guest, string.Empty));

        return testQueue;
    }

    public void OnGameplayFinished()
    {
        StopAllCoroutines();

        hasReachedTopOfTheLeaderboard = false;
        better_hiScoresQueue = null;
        swapDataQueue = null;

        //HideRanks();
        HideSingleRank(UIIngameRanks.Position.Focus);
        HideSingleRank(UIIngameRanks.Position.Enter);

        isInGameplay = false;
    }

    public void OnGameplayRestart()
    {
        OnGameplayFinished();
    }
    
    void Update()
    {
        /*const bool FORCE_SKIP = false;
        if (FORCE_SKIP)
            return;*/

        if (!isInGameplay)
            return;

        if (currentScoreToBeat != null && !hasReachedTopOfTheLeaderboard)
        {
            if (uiSharedData.InterfaceDistance > currentScoreToBeat.Score)
            {
                TriggerSwap();
            }
            else if (!isPlayingSwapAnimation)
                UpdateRankMeters(UIIngameRanks.Position.Focus, (int)MetersToBeatNextOpponent);
        }

        if (!bothRanksAreVisible)
            HideRanksTemporarily();
    }

    void TriggerSwap()
    {
        McSocialApiUtils.ScoreData old_better = currentScoreToBeat;

        if (better_hiScoresQueue.Count > 0)
        {
            currentScoreToBeat = better_hiScoresQueue.Dequeue();
            McSocialApiUtils.ScoreData new_better = currentScoreToBeat;
            
            swapDataQueue.Enqueue(new SwapData(old_better, new_better));

            if (!isPlayingSwapAnimation)
                StartCoroutine(StartSwapAnimation());

            //SetOpponentToBeatRank(UIIngameRanks.Position.Focus, currentScoreToBeat.Rank);
        }
        else
            OnReachedLeaderboardTop(old_better);
    }

    IEnumerator StartSwapAnimation()
    {
        isPlayingSwapAnimation = true;

        Asserts.Assert(swapDataQueue.Count > 0);

        while (!bothRanksAreVisible)
            yield return new WaitForEndOfFrame();

        SwapData swapData = swapDataQueue.Dequeue();

        SetRank(UIIngameRanks.Position.Focus, swapData.old_better, false);
        SetRank(UIIngameRanks.Position.Enter, swapData.new_better, false);

        ShowSingleRank(UIIngameRanks.Position.Focus); //ingameRanks.ShowRank(UIIngameRanks.Position.Focus);
        ShowSingleRank(UIIngameRanks.Position.Enter); //ingameRanks.ShowRank(UIIngameRanks.Position.Enter);

        HideRankMeters(UIIngameRanks.Position.Focus);
        HideRankMeters(UIIngameRanks.Position.Enter);

        ingameRanks.PlayTransitionAnimation();
    }

    public void OnSwapAnimationFinished()
    {
        isPlayingSwapAnimation = false;
        if (swapDataQueue.Count > 0)
            StartCoroutine(StartSwapAnimation());
        else if (hasReachedTopOfTheLeaderboard)
        {
            HideSingleRank(UIIngameRanks.Position.Enter);    // ingameRanks.HideRank(UIIngameRanks.Position.Enter);
            ingameRanks.SetRank(UIIngameRanks.Position.Focus,
                                1,
                                (McSocialApiManager.Instance.UserLoginData.Type == McSocialApiUtils.LoginType.Facebook) ? OnTheRunFacebookManager.Instance.UserName : McSocialApiManager.Instance.UserLoginData.Name, //McSocialApiManager.Instance.UserLoginData.Name,
                                0,
                                OnTheRunMcSocialApiData.Instance.GetPicture());
            HideRankMeters(UIIngameRanks.Position.Focus);
        }
        else
        {
            ShowSingleRank(UIIngameRanks.Position.Focus);   // ingameRanks.ShowRank(UIIngameRanks.Position.Focus);
            HideSingleRank(UIIngameRanks.Position.Enter);   // ingameRanks.HideRank(UIIngameRanks.Position.Enter);

            ShowRankMeters(UIIngameRanks.Position.Focus);
            ShowRankMeters(UIIngameRanks.Position.Enter);

            SetOpponentToBeatRank(UIIngameRanks.Position.Focus, currentScoreToBeat.Rank);
        }
    }

    public void HideRanksTemporarily()
    {
        if (!isInGameplay)
            return;

        bothRanksAreVisible = false;
        if (!ingameRanks)
            return;

        ingameRanks.HideRank(UIIngameRanks.Position.Focus);
        ingameRanks.HideRank(UIIngameRanks.Position.Enter);
    }

    public void ShowRanksBackAgain()
    {
        if (!isInGameplay)
            return;

        bothRanksAreVisible = true;
        if (!ingameRanks)
            return;

        if (ranksVisibilityByPosition[UIIngameRanks.Position.Focus])
            ShowSingleRank(UIIngameRanks.Position.Focus);   // ingameRanks.ShowRank(UIIngameRanks.Position.Focus);

        if (ranksVisibilityByPosition[UIIngameRanks.Position.Enter])
            ShowSingleRank(UIIngameRanks.Position.Enter);   // ingameRanks.ShowRank(UIIngameRanks.Position.Enter);
    }

    void HideSingleRank(UIIngameRanks.Position position)
    {
        ranksVisibilityByPosition[position] = false;
        ingameRanks.HideRank(position);
    }

    void ShowSingleRank(UIIngameRanks.Position position)
    {
        ranksVisibilityByPosition[position] = true;
        ingameRanks.ShowRank(position);
    }

    void SetRank(UIIngameRanks.Position position, McSocialApiUtils.ScoreData scoreData, bool shouldHideMeters)
    {
        ingameRanks.SetRank(position, scoreData.Rank, scoreData.Name, 0, GetOpponentsPicture(scoreData));
        if (shouldHideMeters)
            ingameRanks.HideMeters(position);
        else
            ingameRanks.ShowMeters(position);
    }

    void SetOpponentToBeatRank(UIIngameRanks.Position position, long rank)
    {
        ingameRanks.ShowRank(position);
        ingameRanks.SetRank(position, rank, currentScoreToBeat.Name, (int)MetersToBeatNextOpponent, GetCurrentOpponentsPicture());
    }

    void OnReachedLeaderboardTop(McSocialApiUtils.ScoreData lastBest)
    {
        currentScoreToBeat = null;
        hasReachedTopOfTheLeaderboard = true;

        McSocialApiUtils.ScoreData new_better = new McSocialApiUtils.ScoreData(McSocialApiManager.Instance.UserLoginData.Id,
                                                                               (McSocialApiManager.Instance.UserLoginData.Type == McSocialApiUtils.LoginType.Facebook) ? OnTheRunFacebookManager.Instance.UserName : McSocialApiManager.Instance.UserLoginData.Name, //McSocialApiManager.Instance.UserLoginData.Name,
                                                                               string.Empty,
                                                                               0,
                                                                               1,
                                                                               McSocialApiManager.Instance.UserLoginData.Type,
                                                                               (McSocialApiManager.Instance.UserLoginData.Type == McSocialApiUtils.LoginType.Facebook) ? OnTheRunFacebookManager.Instance.UserId : string.Empty);

        swapDataQueue.Enqueue(new SwapData(lastBest, new_better));

        //Debug.Log("#################### REACHED TOP OF THE LEADERBOARD \nLAST: " + lastBest.ToString() + "\nNEW: " + new_better.ToString());

        if (!isPlayingSwapAnimation)
            StartCoroutine(StartSwapAnimation());

        /*ingameRanks.SetRank(UIIngameRanks.Position.Focus, 1, McSocialApiManager.Instance.UserLoginData.Name, 0, OnTheRunMcSocialApiData.Instance.GetPicture());
        HideRankMeters(UIIngameRanks.Position.Focus);*/
    }

    void UpdateRankMeters(UIIngameRanks.Position position, int meters)
    {
        ingameRanks.UpdateMeters(position, meters);
    }

    void HideRankMeters(UIIngameRanks.Position position)
    {
        ingameRanks.HideMeters(position);
    }

    void ShowRankMeters(UIIngameRanks.Position position)
    {
        ingameRanks.ShowMeters(position);
    }

    public Sprite GetOpponentsPicture(McSocialApiUtils.ScoreData scoreData)
    {
        Sprite upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.defaultUserPicture;
        if (scoreData != null && scoreData.LoginType == McSocialApiUtils.LoginType.Facebook)
        {
            if (OnTheRunFacebookManager.Instance.IsLoggedIn && scoreData.LoginTypeId == OnTheRunFacebookManager.Instance.UserId)
            {
                upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.GetPicture();
            }
            else
            {
                Sprite fbUserPicture = OnTheRunFacebookManager.Instance.GetUsersPicture(scoreData.LoginTypeId);
                if (fbUserPicture != null)
                    upperOpponentsPicture = fbUserPicture;
            }
        }
        else if(scoreData != null && scoreData.LoginType == McSocialApiUtils.LoginType.Guest)
        {
            switch (scoreData.Id)
            {
                case McSocialApiManager.FAKE_ID_00:
                    upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.blueUserPicture;
                    break;
                case McSocialApiManager.FAKE_ID_01:
                    upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.pinkUserPicture;
                    break;
                case McSocialApiManager.FAKE_ID_02:
                    upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.greenUserPicture;
                    break;

            }
        }

        return upperOpponentsPicture;
    }

    Sprite GetCurrentOpponentsPicture()
    {
        Sprite upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.defaultUserPicture;
        if (currentScoreToBeat != null && currentScoreToBeat.LoginType == McSocialApiUtils.LoginType.Facebook)
        {
            Sprite fbUserPicture = OnTheRunFacebookManager.Instance.GetUsersPicture(currentScoreToBeat.LoginTypeId);
            if (fbUserPicture != null)
                upperOpponentsPicture = fbUserPicture;
        }
        else if (currentScoreToBeat != null && currentScoreToBeat.LoginType == McSocialApiUtils.LoginType.Guest)
        {
            switch (currentScoreToBeat.Id)
            {
                case McSocialApiManager.FAKE_ID_00:
                    upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.blueUserPicture;
                    break;
                case McSocialApiManager.FAKE_ID_01:
                    upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.pinkUserPicture;
                    break;
                case McSocialApiManager.FAKE_ID_02:
                    upperOpponentsPicture = OnTheRunMcSocialApiData.Instance.greenUserPicture;
                    break;

            }
        }

        return upperOpponentsPicture;
    }

    bool IsOpponentMe()
    {
        bool result = false;

        if (McSocialApiManager.Instance.IsLoggedIn && currentScoreToBeat != null)
            if (currentScoreToBeat.Id.Equals(McSocialApiManager.Instance.UserLoginData.Id))
                result = true;

        return result;
    }

    bool IsOpponentFacebookFriend()
    {
        bool result = false;

        if (McSocialApiManager.Instance.IsLoggedIn &&
            OnTheRunFacebookManager.Instance.IsLoggedIn &&
            currentScoreToBeat != null &&
            currentScoreToBeat.LoginType == McSocialApiUtils.LoginType.Facebook)
        {
            result = OnTheRunFacebookManager.Instance.UserIsFriend(currentScoreToBeat.LoginTypeId);
        }

        return result;
    }
}