using UnityEngine;
using SBS.Core;
using System.Collections;
using System.Globalization;

[AddComponentMenu("OnTheRun/UI/UIFBFeedbackPopup")]
public class UIFBFeedbackPopup: MonoBehaviour
{
    //public SpriteRenderer icon;
    //public UITextField numText;
    public UITextField friendNameText;
    //public UITextField centralText;
    public UITextField rewardText;
    public UITextField okButtonText;
    //public GameObject diamond;

    protected bool pressed = false;
    //protected Vector3 backupCollectButtonPos;

    void OnEnable()
    {
        //if (backupCollectButtonPos == null)
        //    backupCollectButtonPos = okButtonText.transform.parent.transform.localPosition;

        friendNameText.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_5_thanks");
        //centralText.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_reward_text");
        rewardText.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_5_comeback");
        okButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        //numText.text = Manager<UIRoot>.Get().FormatTextNumber(1);
    }

    void Signal_OnShow(UIPopup popup)
    {
        pressed = false;
    }

    void Signal_OnExit(UIPopup popup)
    {
    }

    void Signal_OnCollectRelease(UIButton button)
    {
        if (pressed)
            return;

        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        //OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("fireworks"));

        pressed = true;

        OnTheRunNotificationManager.Instance.PopupClosed();

        Manager<UIRoot>.Get().PauseExperienceBarAnimation(false);
    }
        
    void OnBackButtonAction()
    {
        UIButton collectButton = transform.FindChild("content/CollectButton").GetComponent<UIButton>();
        collectButton.onReleaseEvent.Invoke(collectButton);
    }
}