using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIIngameRanks : MonoBehaviour
{
    public enum Position
    {
        Focus,
        Enter
    }

    Dictionary<Position, GameObject> ranksGOsByPosition;
    Dictionary<Position, UITextField> ranksByPosition;
    Dictionary<Position, UITextField> namesByPosition;
    Dictionary<Position, UITextField> metersByPosition;
    Dictionary<Position, SpriteRenderer> pictureSpriteRenderersByPosition;
    Dictionary<Position, SpriteRenderer> frameSpriteRenderersByPosition;

    Vector3 focusGoStartPosition;
    Vector3 enterGoStartPosition;

    bool isPlayingAnimation;

    void Awake()
    {
        ranksGOsByPosition = new Dictionary<Position, GameObject>();
        ranksGOsByPosition.Add(Position.Focus, transform.Find("focus_rank").gameObject);
        ranksGOsByPosition.Add(Position.Enter, transform.Find("new_rank").gameObject);

        ranksByPosition = new Dictionary<Position, UITextField>();
        ranksByPosition.Add(Position.Focus, transform.Find("focus_rank/tfRank").GetComponent<UITextField>());
        ranksByPosition.Add(Position.Enter, transform.Find("new_rank/tfRank").GetComponent<UITextField>());

        namesByPosition = new Dictionary<Position, UITextField>();
        namesByPosition.Add(Position.Focus, transform.Find("focus_rank/tfName").GetComponent<UITextField>());
        namesByPosition.Add(Position.Enter, transform.Find("new_rank/tfName").GetComponent<UITextField>());

        metersByPosition = new Dictionary<Position, UITextField>();
        metersByPosition.Add(Position.Focus, transform.Find("focus_rank/tfMeters").GetComponent<UITextField>());
        metersByPosition.Add(Position.Enter, transform.Find("new_rank/tfMeters").GetComponent<UITextField>());

        pictureSpriteRenderersByPosition = new Dictionary<Position, SpriteRenderer>();
        pictureSpriteRenderersByPosition.Add(Position.Focus, transform.Find("focus_rank/fb_user").GetComponent<SpriteRenderer>());
        pictureSpriteRenderersByPosition.Add(Position.Enter, transform.Find("new_rank/fb_user").GetComponent<SpriteRenderer>());

        frameSpriteRenderersByPosition = new Dictionary<Position, SpriteRenderer>();
        frameSpriteRenderersByPosition.Add(Position.Focus, transform.Find("focus_rank/frame_avatar_facebook").GetComponent<SpriteRenderer>());
        frameSpriteRenderersByPosition.Add(Position.Enter, transform.Find("new_rank/frame_avatar_facebook").GetComponent<SpriteRenderer>());
        
        isPlayingAnimation = false;

        focusGoStartPosition = ranksGOsByPosition[Position.Focus].transform.localPosition;
        enterGoStartPosition = ranksGOsByPosition[Position.Enter].transform.localPosition;
    }

    public void SetRank(Position position, long rank, string name, int meters, Sprite picture)
    {
        UITextField rankTextField = ranksByPosition[position];
        UITextField nameTextField = namesByPosition[position];
        UITextField metersTextField = metersByPosition[position];
        SpriteRenderer spriteRenderer = pictureSpriteRenderersByPosition[position];

        rankTextField.text = rank.ToString();
        nameTextField.text = OnTheRunMcSocialApiData.Instance.TrimStringAtMaxChars(OnTheRunDataLoader.Instance.GetLocaleString(name), 17);
        metersTextField.text = Manager<UIRoot>.Get().FormatTextNumber(meters) + " " + OnTheRunDataLoader.Instance.GetLocaleString("meters_short");
        spriteRenderer.sprite = picture;
    }

    public void UpdateMeters(Position position, int meters)
    {
        metersByPosition[position].text = Manager<UIRoot>.Get().FormatTextNumber(meters) + " " + OnTheRunDataLoader.Instance.GetLocaleString("meters_short");
    }

    public void HideMeters(Position position)
    {
        metersByPosition[position].gameObject.SetActive(false);
    }

    public void ShowMeters(Position position)
    {
        metersByPosition[position].gameObject.SetActive(true);
    }

    public void HideRank(Position position)
    {
        ranksGOsByPosition[position].SetActive(false);
    }

    public void ShowRank(Position position)
    {
        ranksGOsByPosition[position].SetActive(true);
    }

    public void PlayTransitionAnimation()
    {
        string animationName = "RankSwap";
        
        FadeRank(Position.Focus, 1.0f);
        FadeRank(Position.Enter, 0.0f);

        GetComponent<Animation>().Play(animationName);
        isPlayingAnimation = true;
    }
     
    public void OnRankSwapFinished()
    {
        isPlayingAnimation = false;
        GetComponent<Animation>().Stop();

        ResetPositions();

        FadeRank(Position.Focus, 1.0f);
        FadeRank(Position.Enter, 1.0f);

        OnTheRunIngameHiScoreCheck.Instance.OnSwapAnimationFinished();
    }

    public void ResetPositions()
    {
        ranksGOsByPosition[Position.Focus].transform.localPosition = focusGoStartPosition;
        ranksGOsByPosition[Position.Enter].transform.localPosition = enterGoStartPosition;
    }

    void Update()
    {
        if (isPlayingAnimation)
        {
            FadeRank(Position.Focus, 1.0f - GetComponent<Animation>()["RankSwap"].normalizedTime);
            FadeRank(Position.Enter, GetComponent<Animation>()["RankSwap"].normalizedTime);
        }
    }

    void FadeRank(Position position, float alpha)
    {
        UITextField rankTextField = ranksByPosition[position];
        UITextField nameTextField = namesByPosition[position];
        UITextField metersTextField = metersByPosition[position];
        SpriteRenderer spriteRenderer = pictureSpriteRenderersByPosition[position];
        SpriteRenderer frameSpriteRenderer = frameSpriteRenderersByPosition[position];

        rankTextField.color.a = alpha;
        nameTextField.color.a= alpha;
        metersTextField.color.a = alpha;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        frameSpriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
    }
}