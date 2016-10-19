using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIArrowButtons")]
public class UIArrowButtons : MonoBehaviour
{
    protected OnTheRunInterfaceSounds interfaceSounds;
    UIButton leftArrow;
    UIButton rightArrow;

    void Awake()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        leftArrow = transform.FindChild("ArrowLeftButton").GetComponent<UIButton>();
        rightArrow = transform.FindChild("ArrowRightButton").GetComponent<UIButton>();
    }

    void OnShowArrowButtons()
    {
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);
    }

    void Signal_OnMoveSelectedCarLeft(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIManager>.Get().ActivePage.SendMessage("OnMoveSelectedCarLeft");
    }

    void Signal_OnMoveSelectedCarRight(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIManager>.Get().ActivePage.SendMessage("OnMoveSelectedCarRight");
    }

    void Signal_OnLeftArrowReleased(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        switch (Manager<UIManager>.Get().ActivePageName)
        {
            case "GaragePage": Manager<UIManager>.Get().ActivePage.SendMessage("OnLeftArrowReleased"); break;
        }
    }

    void Signal_OnRightArrowReleased(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        switch (Manager<UIManager>.Get().ActivePageName)
        {
            case "GaragePage": Manager<UIManager>.Get().ActivePage.SendMessage("OnRightArrowReleased"); break;
        }
    }
}