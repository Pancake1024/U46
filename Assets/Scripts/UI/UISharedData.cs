using UnityEngine;
using System.Collections;

public class UISharedData : MonoBehaviour
{
    public int maxSlipstreamMultiplier;
    public int scoreForMeter;
    public int scoreForCollision;

    protected int interfaceDistance;
    protected int interfaceDistanceScore;
    protected int scoreDistanceMultiplier = 0;
    protected int scoreDenominator = 1;
    protected int oldScoreDenominator = 1;
    protected int wrongDirectionMultiplier = 1;

    protected UIIngamePage ingamePage;
    protected int checkpointDistance;
    protected int baseCheckpointDistance;
    protected int maxCheckpointDistance;

    protected float nextFuelTime = 0.0f;
    protected int fuelNumber = 0;
    
    protected PlayerKinematics playerKinematics = null;
    protected OnTheRunGameplay gameplayManager = null;

    protected PriceData.CurrencyType currentCurrency = PriceData.CurrencyType.FirstCurrency;

    protected int lastGarageLocationIndex = 0;

    public int InterfaceDistance { get { return interfaceDistance; } }
    public int InterfaceDistanceScore { get { return interfaceDistanceScore; } }
    public int ScoreDistanceMultiplier { get { return scoreDistanceMultiplier; } }
    public int WrongDirectionMultiplier { get { return wrongDirectionMultiplier; } }
    public int MaxCheckpointDistance { get { return maxCheckpointDistance; } set { maxCheckpointDistance = value; } }
    public int CheckpointDistance { get { return checkpointDistance; } }
    public int BaseCheckPointDistance { get { return baseCheckpointDistance; } }
    public PriceData.CurrencyType CurrentCurrency { get { return currentCurrency; } set { currentCurrency = value; } }

    public int LastGarageLocationIndex { get { return lastGarageLocationIndex; } }
    
    public int MaxComboIndex
    {
        get { return maxSlipstreamMultiplier; }
    }
    
    public void IncCheckpointDistance()
    {
        maxCheckpointDistance += baseCheckpointDistance;
    }

    void Awake()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            playerKinematics = playerGO.GetComponent<PlayerKinematics>();

        GameObject gameplayGO = GameObject.FindGameObjectWithTag("GameplayManagers");
        if (gameplayGO != null)
            gameplayManager = gameplayGO.GetComponent<OnTheRunGameplay>();

        ingamePage = GameObject.Find("IngamePage").GetComponent<UIIngamePage>();
    }

    public void OnRestartRace()
    {
        GetLostReferences();

        interfaceDistance = 0;
        interfaceDistanceScore = 0;
        scoreDenominator = 1;
        oldScoreDenominator = 1;

        baseCheckpointDistance = gameplayManager.GetComponent<OnTheRunSpawnManager>().checkpointDistance;
        maxCheckpointDistance = baseCheckpointDistance;
        checkpointDistance = maxCheckpointDistance;
    }

    public void IncreaseScoreMultiplier( )
    {
        if (gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.None)
        {
            scoreDistanceMultiplier = Mathf.Clamp(scoreDistanceMultiplier + 1, 1, maxSlipstreamMultiplier);

            gameplayManager.UpdateRunParameter(OnTheRunGameplay.RunParameters.MaxCombo, scoreDistanceMultiplier);

            ingamePage.SendMessage("OnScoreMultiplierChange");
            int coinsForSlipstream = scoreDistanceMultiplier;
            /*if (coinsForSlipstream <= 2)
                coinsForSlipstream = 0;
            else
            {
                coinsForSlipstream = Mathf.RoundToInt(Mathf.Pow(exponent, (float)coinsForSlipstream - 2.0f));
            }*/

            float coinsBase = gameplayManager.coinsBase + gameplayManager.coinsStep * (coinsForSlipstream-1);
            coinsForSlipstream = Mathf.RoundToInt(Mathf.Pow(coinsBase, gameplayManager.coinsExponent));

            //LevelRoot.Instance.BroadcastMessage("OnReachSingleCombo", scoreDistanceMultiplier);
            LevelRoot.Instance.BroadcastMessage("OnReachCombo", scoreDistanceMultiplier);

            int coinsGainedInSlipstream = coinsForSlipstream; //playerKinematics.WrongDirection ? 5 : coinsForSlipstream;
            if (coinsGainedInSlipstream>0)
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(playerKinematics.PlayerRigidbody.position, coinsGainedInSlipstream);
        }
    }

    public void ResetScoreMultiplier()
    {
        scoreDistanceMultiplier = 0;
        ingamePage.SendMessage("OnScoreMultiplierChange", SendMessageOptions.DontRequireReceiver);
    }

    public void IncreaseWrongDirectionMultiplier()
    {
        wrongDirectionMultiplier = 2;
    }

    public void ResetWrongDirectionMultiplier()
    {
        wrongDirectionMultiplier = 1;
    }

    public void AddScore(int scoreToAdd)
    {
        interfaceDistanceScore += scoreToAdd;
    }

    public void UpdateSharedData()
    {
        GetLostReferences();

        if (!TimeManager.Instance.MasterSource.IsPaused && (gameplayManager.Gameplaystarted || playerKinematics.PlayerIsStopping))
        {
            if (interfaceDistance != (int)(playerKinematics.Distance) && playerKinematics.Distance > 0)
            {
                checkpointDistance = Mathf.Max(0, maxCheckpointDistance  - (int)(playerKinematics.Distance));
                interfaceDistance = (int)(playerKinematics.Distance * OnTheRunUtils.ToOnTheRunMeters);
                
                float delta = playerKinematics.Distance - scoreDenominator;
                if (delta > 0)
                {
                    scoreDenominator += (1 + (int)Mathf.FloorToInt(delta));
                    interfaceDistanceScore += (scoreDenominator - oldScoreDenominator) * scoreDistanceMultiplier * scoreForMeter * wrongDirectionMultiplier;
                    oldScoreDenominator = scoreDenominator;
                }
            }
        }
    }
    
    void GetLostReferences()
    {
        if (playerKinematics == null || !playerKinematics.gameObject.activeInHierarchy)
            playerKinematics = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
    }

    void SetCurrentCurrency(PriceData.CurrencyType c)
    {
        currentCurrency = c;
    }

    public void OnNewGarageLocationChosen(int locationIndex)
    {
        if (Manager<UIManager>.Get().ActivePage.name == "GaragePage")
        {
            lastGarageLocationIndex = locationIndex;
            UIRankingsPage.RankingsPageWorldId = lastGarageLocationIndex;
        }
    }
}