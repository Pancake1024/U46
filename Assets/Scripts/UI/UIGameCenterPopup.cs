using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIGameCenterPopup")]
public class UIGameCenterPopup : MonoBehaviour
{
    protected OnTheRunInterfaceSounds interfaceSounds;

    void OnEnable()
    {
        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
    }

    void Signal_OnLeaderboardRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Debug.Log("LEADERBOARD: Signal_OnLeaderboardRelease");
        OnTheRunSocial.Instance.ShowLeaderboards();
    }

    void Signal_OnAchievementsRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Debug.Log("ACHIEVEMENTS: Signal_OnAchievementsRelease");
        OnTheRunSocial.Instance.ShowAchievements();
    }

    void Signal_OnCloseRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIManager>.Get().PopPopup();
    }
}