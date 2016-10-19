using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UICoinsFlyer")]
public class UICoinsFlyer: MonoBehaviour
{
    protected OnTheRunGameplay gameplayManager;
    protected UISharedData uiSharedData;
    protected Vector3 goalPosition;
    protected float scale_speed = 2.5f;
    protected float move_speed = 4.0f;
    protected float timeInIdle = 0.5f;
    protected float timeToExit = 1.0f;
    protected FlyerState state = FlyerState.SCALING;
    protected float timeToMove;
    protected int coinsgained;
    protected int scoreGained;
    protected float currentAlpha = 1.0f;
    protected float currentAlphaTimer = -1.0f;
    protected float currentAlphaSpeed = -1.0f;
    protected UITextField flyierTextfield;
    protected UITextField flyierStroke;

    protected bool flashing = false;
    protected Color baseColor;
    protected Color flashColor;
    protected float timeToFlash = -1.0f;
    protected float currentFlashTimer = -1.0f;
    protected float flashLerp;

    public class CoinsFlyerData
    {
        public string text;
        public Vector3 initPosition;
        public Vector3 goalPosition;
        public float scaleSpeed;
        public float timeInIdle;
        public float timeToExit = 1.0f;
        public int coinsGained = 0;
        public int scoreGained = 0;
        public float flyerWidth = 0.0f;
        public float fadeAfter = -1.0f;
        public float fadeSpeed = -1.0f;

        public Color flashColor = Color.white;
        public float flashAfter = -1.0f;
        public float flashTime = -1.0f;
    }

    protected enum FlyerState
    {
        SCALING,
        IDLE,
        MOVING
    }

    void Awake()
    {
        if (gameObject.transform.FindChild("Text") != null)
        {
            flyierTextfield = gameObject.transform.FindChild("Text").GetComponent<UITextField>();
            flyierStroke = gameObject.transform.FindChild("Text/stroke").GetComponent<UITextField>();
        }
    }

    void Initialize(CoinsFlyerData data)
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
		uiSharedData = Manager<UIRoot>.Get().gameObject.GetComponent<UISharedData>();
		coinsgained = data.coinsGained;
        scoreGained = data.scoreGained;
        if (flyierTextfield != null)
            flyierTextfield.text = data.text;
		state = FlyerState.SCALING;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        scale_speed = data.scaleSpeed;
        timeInIdle = data.timeInIdle;
        timeToExit = data.timeToExit;
        transform.position = data.initPosition;
        goalPosition = data.goalPosition;
        currentAlpha = 1.0f;
        currentAlphaTimer = data.fadeAfter;
        currentAlphaSpeed = data.fadeSpeed;

        flashColor = data.flashColor;
        timeToFlash = data.flashAfter;
        currentFlashTimer = data.flashTime;
    }

    void InitializeWithPercentage(CoinsFlyerData data)
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();
        coinsgained = data.coinsGained;
        scoreGained = data.scoreGained;
        if (flyierTextfield != null)
            flyierTextfield.text = data.text;
        state = FlyerState.SCALING;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        scale_speed = data.scaleSpeed;
        timeInIdle = data.timeInIdle;
        timeToExit = data.timeToExit;

        Camera cam = Manager<UIManager>.Get().UICamera;
        float pixelsPerUnit = Manager<UIManager>.Get().pixelsPerUnit;
        float heightScreen = Manager<UIManager>.Get().baseScreenHeight * 0.5f / pixelsPerUnit;
        float widthScreen = heightScreen * cam.aspect;
        transform.position = new Vector3(data.initPosition.x * widthScreen, data.initPosition.y * heightScreen, 0.0f);
        goalPosition = new Vector3(data.goalPosition.x * widthScreen, data.goalPosition.y * heightScreen, 0.0f);
        //Debug.Log(heightScreen + " " + widthScreen);
        currentAlpha = 1.0f;
        currentAlphaTimer = data.fadeAfter;
        currentAlphaSpeed = data.fadeSpeed;
    }

    void InitializeWithPercentageY(CoinsFlyerData data)
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();
        coinsgained = data.coinsGained;
        scoreGained = data.scoreGained;
        if (flyierTextfield != null)
            flyierTextfield.text = data.text;
        state = FlyerState.SCALING;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        scale_speed = data.scaleSpeed;
        timeInIdle = data.timeInIdle;
        timeToExit = data.timeToExit;

        Camera cam = Manager<UIManager>.Get().UICamera;
        float pixelsPerUnit = Manager<UIManager>.Get().pixelsPerUnit;
        float heightScreen = Manager<UIManager>.Get().baseScreenHeight * 0.5f / pixelsPerUnit;
        float widthScreen = heightScreen * cam.aspect;

        float xPos = widthScreen - data.flyerWidth;
        transform.position = new Vector3(-xPos, data.initPosition.y * heightScreen, 0.0f);
        goalPosition = new Vector3(data.goalPosition.x * (widthScreen / heightScreen), data.goalPosition.y * heightScreen, 0.0f);
        currentAlpha = 1.0f;
        currentAlphaTimer = data.fadeAfter;
        currentAlphaSpeed = data.fadeSpeed;
    }

    void InitializeWithPercentageY2(CoinsFlyerData data)
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();
        coinsgained = data.coinsGained;
        scoreGained = data.scoreGained;
        if (flyierTextfield != null)
            flyierTextfield.text = data.text;
        state = FlyerState.SCALING;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        scale_speed = data.scaleSpeed;
        timeInIdle = data.timeInIdle;
        timeToExit = data.timeToExit;

        Camera cam = Manager<UIManager>.Get().UICamera;
        float pixelsPerUnit = Manager<UIManager>.Get().pixelsPerUnit;
        float heightScreen = Manager<UIManager>.Get().baseScreenHeight * 0.5f / pixelsPerUnit;
        float widthScreen = heightScreen * cam.aspect;

        float xPos = widthScreen - data.flyerWidth + 0.9f;
        transform.position = new Vector3(xPos, data.initPosition.y * heightScreen, 0.0f);
        goalPosition = new Vector3(-data.goalPosition.x * (widthScreen / heightScreen), data.goalPosition.y * heightScreen, 0.0f);
        currentAlpha = 1.0f;
        currentAlphaTimer = data.fadeAfter;
        currentAlphaSpeed = data.fadeSpeed;
    }

    public void setTextAlpha(UITextField text, float newAlpha)
    {
        Color mObjColor = text.color;
        mObjColor = new Color(mObjColor.r, mObjColor.g, mObjColor.b, newAlpha);
        text.color = mObjColor;
        text.ApplyParameters();
    }

    void Update( )
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        if (timeToFlash >= 0.0f && !flashing)
        {
            timeToFlash -= dt;
            if (timeToFlash <= 0.0f)
            {
                flashing = true;
                baseColor = flyierTextfield.color;
                flashLerp = 0.0f;
            }
        }

        if (flashing)
        {
            currentFlashTimer -= dt;
            if (currentFlashTimer <= 0.0f)
            {
                flashLerp -= dt * 16.0f;
                if (flashLerp <= 0.0f)
                    flashing = false;
                flashLerp = Mathf.Max(0.0f, flashLerp);
            }
            else
            {
                flashLerp += dt * 16.0f;
                flashLerp = Mathf.Min(1.0f, flashLerp);
            }

            Color c = Color.Lerp(baseColor, flashColor, flashLerp);
            float s = 1.0f + Mathf.Lerp(0.0f, 0.25f, flashLerp);
            transform.localScale = Vector3.one * s;
            flyierTextfield.color = c;
            flyierTextfield.ApplyParameters();
        }

        if (state == FlyerState.SCALING)
        {
            float scaleDelta = dt * scale_speed;
            transform.localScale += new Vector3(scaleDelta, scaleDelta, scaleDelta);
            if (transform.localScale.x >= 1.0f)
            {
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                state = FlyerState.IDLE;
                timeToMove = timeInIdle;
            }
        }
        else if (state == FlyerState.IDLE)
        {
            timeToMove -= dt;
            if (timeToMove < 0)
                state = FlyerState.MOVING;
        }
        else
        {
            if (goalPosition != null)
            {
                float dist = (goalPosition - transform.position).magnitude;
                if (dist > 0.2f)
                {
                    float totalTime = timeToExit;

                    float deltaX = (goalPosition.x - transform.position.x) / totalTime;
                    float deltaY = (goalPosition.y - transform.position.y) / totalTime;

                    transform.position += new Vector3(deltaX * dt * move_speed, deltaY * dt * move_speed, 0.0f);
                }
                else
                {
                    if (coinsgained > 0)
                    {
                        gameplayManager.CoinsCollected += coinsgained;
                        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.COINS_MEDIUM, coinsgained);
                        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.COINS_HARD, coinsgained);
                    }
                    else if (scoreGained > 0)
                        uiSharedData.AddScore(scoreGained);
                    Destroy(gameObject);
                }
            }

            if (currentAlphaSpeed > 0.0f)
            {
                currentAlphaTimer -= dt;
                if (currentAlphaTimer < 0.0f)
                {
                    currentAlpha -= dt * currentAlphaSpeed;
                    setTextAlpha(flyierTextfield, currentAlpha);
                    setTextAlpha(flyierStroke, currentAlpha);
                }
            }
        }
    }
}