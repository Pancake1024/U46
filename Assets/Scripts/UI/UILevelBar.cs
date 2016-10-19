using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UILevelBar")]
public class UILevelBar : MonoBehaviour
{
    UITextField tfLevelValue;
    UITextField tfCompleteLevel;
    UINineSlice expBar;
    float totalBarWidth;

    float animatedExperienceDelay = 0.0f;
    float animatedExperienceSpeed = 1.0f;
    public bool startedAnimation = false;
    float animatedExperienceValue;
    float goalBarScale;
    int currentExperienceGained;
    Transform particleEffect;
    SpriteRenderer userPictureSpriteRenderer;
    protected bool pauseAnimation = false;
    protected UIRoot uiRoot;

    void Awake()
    {
        tfLevelValue = transform.FindChild("tfLevelValue").gameObject.GetComponent<UITextField>();
        tfCompleteLevel = transform.FindChild("tfCompleteLevel").gameObject.GetComponent<UITextField>();

        expBar = transform.FindChild("Bar/bar_green_bg").gameObject.GetComponent<UINineSlice>();
        expBar.width = 0.0f;

        totalBarWidth = transform.FindChild("Bar/bar_top_bg").gameObject.GetComponent<UINineSlice>().width;
        particleEffect = transform.FindChild("FxStars2D");

        userPictureSpriteRenderer = transform.Find("fb_user").GetComponent<SpriteRenderer>();

#if UNITY_WEBPLAYER
        transform.FindChild("fb_user").gameObject.SetActive(false);
        transform.FindChild("frame_avatar_facebook").gameObject.SetActive(false);
        transform.FindChild("Background_normal").GetComponent<UINineSlice>().width = 1.78f;
        transform.FindChild("Background_normal").transform.localPosition = new Vector3(0.45f, transform.FindChild("Background_normal").transform.localPosition.y, 0.0f);
        transform.localPosition = new Vector3(1.47f, transform.localPosition.y, 0.0f);
        transform.FindChild("star_fb_user").gameObject.transform.localScale = Vector3.one * 0.85f;
        UIButton missionBtn = transform.FindChild("button").gameObject.GetComponent<UIButton>();
        missionBtn.transform.localPosition = new Vector3(0.33f, missionBtn.transform.localPosition.y, missionBtn.transform.localPosition.z);
        BoxCollider2D boxCollider2D = missionBtn.GetComponent<BoxCollider2D>();
        boxCollider2D.size = new Vector2(1.8f, boxCollider2D.size.y);
#endif

    }

    void OnEnable()
    {
        uiRoot = Manager<UIRoot>.Get();
        //transform.FindChild("Bar/bar_gray_bg").gameObject.SetActive(PlayerPersistentData.Instance.Experience > 0.0f);
        UpdateExperienceBarImmediatly(0);
        userPictureSpriteRenderer.sprite = OnTheRunMcSocialApiData.Instance.GetPicture();

        McSocialApiManager.ScaleAvatarSpriteRenderer(userPictureSpriteRenderer);
    }

    public void UpdateFacebookButtonText()
    {
        if (OnTheRunFacebookManager.Instance.IsLoggedIn)
            Manager<UIRoot>.Get().UpdateAvatarPicture();
    }

    public void UpdateAvatarPicture()
    {
        userPictureSpriteRenderer.sprite = OnTheRunMcSocialApiData.Instance.GetPicture();
        McSocialApiManager.ScaleAvatarSpriteRenderer(userPictureSpriteRenderer);
    }

    public void SetLevelText()
    {
        tfLevelValue.text = uiRoot.FormatTextNumber(PlayerPersistentData.Instance.Level);
        tfCompleteLevel.text = uiRoot.FormatTextNumber(PlayerPersistentData.Instance.Experience) + "/" + uiRoot.FormatTextNumber(PlayerPersistentData.Instance.NextExperienceLevelThreshold);
    }

    public void UpdateExperienceBarImmediatly(int experienceGained)
    {
        if (PlayerPersistentData.Instance.Level >= OnTheRunDataLoader.Instance.GetMaxPlayerLevel())
            return;

        if (startedAnimation)
            StopExperienceLevelAnimation();

        PlayerPersistentData.Instance.UpdateExperiencePoints(experienceGained, true);
        transform.FindChild("Bar/bar_green_bg").gameObject.SetActive(PlayerPersistentData.Instance.Experience > 0.0f);

        SetLevelText();
        float barWidth = PlayerPersistentData.Instance.CurrentExperiencePerc * totalBarWidth;
        expBar.width = Mathf.Clamp(barWidth, 0.0f, totalBarWidth);
    }
    
    public void SetExperienceBarValue(int experience)
    {
        PlayerPersistentData.Instance.Experience = experience < PlayerPersistentData.Instance.NextExperienceLevelThreshold ? experience : experience - PlayerPersistentData.Instance.NextExperienceLevelThreshold;
        transform.FindChild("Bar/bar_green_bg").gameObject.SetActive(PlayerPersistentData.Instance.Experience > 0.0f);
      
        SetLevelText();
        float barWidth = PlayerPersistentData.Instance.CurrentExperiencePerc * totalBarWidth;
        expBar.width = Mathf.Clamp(barWidth, 0.0f, totalBarWidth);
    }

    public void UpdateExperienceBarAnimated(float delay, int experienceGained, float speed = 1.0f)
    {
        animatedExperienceSpeed = speed;

        if (delay > 0.0f)
        {
            animatedExperienceDelay = delay;
            currentExperienceGained = experienceGained;
        }
        else
        {
            UpdateExperienceBarImmediatly(0);
            transform.FindChild("Bar/bar_green_bg").gameObject.SetActive(true);

            startedAnimation = true;
            animatedExperienceValue = PlayerPersistentData.Instance.CurrentExperiencePerc * totalBarWidth;
            goalBarScale = ((float)(PlayerPersistentData.Instance.Experience + experienceGained) / (float)PlayerPersistentData.Instance.NextExperienceLevelThreshold) * totalBarWidth;
            currentExperienceGained = experienceGained;
        }
    }
    
    public void PauseExperienceLevelAnimation()
    {
        pauseAnimation = true;
    }
    
    public void ResumeExperienceLevelAnimation()
    {
        pauseAnimation = false;
    }

    public void StopExperienceLevelAnimation()
    {
        startedAnimation = false;
        UpdateExperienceBarImmediatly(currentExperienceGained);
    }

    void Signal_OnLevelBarClicked(UIButton button)
    {
        UIManager uiManager = Manager<UIManager>.Get();
        bool wheelIsSpinning = false;
        if (uiManager.FrontPopup!=null && uiManager.FrontPopup.name.Equals("WheelPopup"))
            wheelIsSpinning = uiManager.FrontPopup.gameObject.GetComponent<UIWheelPopup>().IsSpinning;
        if ((uiManager.ActivePageName != "RewardPage" || !UIRewardPage.firstStep) && !wheelIsSpinning)
        {
            OnTheRunUITransitionManager.Instance.OpenPopup("RankBarPopup");
            Manager<UIManager>.Get().FrontPopup.gameObject.transform.FindChild("content/popupTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("rank_bar_title");
            Manager<UIManager>.Get().FrontPopup.BroadcastMessage("InitRankScroller", true);
        }
    }

    public float GetLevelBarProgress()
    {
        return animatedExperienceValue / totalBarWidth;
    }

    void Update()
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        if(pauseAnimation)
            return;

        if (startedAnimation)
        {
            animatedExperienceValue += dt * animatedExperienceSpeed;
            float barWidth = Mathf.Clamp(animatedExperienceValue, 0.0f, totalBarWidth);
            expBar.width = barWidth;

            string percText = uiRoot.FormatTextNumber(((int)((barWidth / totalBarWidth) * PlayerPersistentData.Instance.NextExperienceLevelThreshold)));
            tfCompleteLevel.text = percText + "/" + uiRoot.FormatTextNumber(PlayerPersistentData.Instance.NextExperienceLevelThreshold);

            if (animatedExperienceValue > totalBarWidth && goalBarScale > totalBarWidth)
            {
                //End of current level reached
                int poinToNextLevel = PlayerPersistentData.Instance.NextExperienceLevelThreshold - PlayerPersistentData.Instance.Experience;
                PlayerPersistentData.Instance.UpdateExperiencePoints(poinToNextLevel);
                animatedExperienceValue = 0.0f;
                currentExperienceGained -= poinToNextLevel;
                particleEffect.GetComponent<ParticleSystem>().Play();
                SetLevelText();
                goalBarScale = ((float)(PlayerPersistentData.Instance.Experience + currentExperienceGained) / (float)PlayerPersistentData.Instance.NextExperienceLevelThreshold) * totalBarWidth;
            }

            if (animatedExperienceValue >= goalBarScale)
            {
                //end animation
                startedAnimation = false;
                UpdateExperienceBarImmediatly(currentExperienceGained);
            }
        }

        if (animatedExperienceDelay >= 0.0f)
        {
            animatedExperienceDelay -= dt;
            //pelle : serve a qualcosa??
            //if (animatedExperienceDelay < 0.0f)
                //UpdateExperienceBarAnimated(0.0f, currentExperienceGained, animatedExperienceSpeed);
        }
    }
}