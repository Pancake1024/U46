using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIMsgPopup")]
public class UIMsgPopup : MonoBehaviour
{
    public UITextField descriptionText;
    //public UITextField titleText;
    public UITextField okButtonText;
   
    
	void OnEnable()
	{
        //UINineSlice bgNineSlice = transform.FindChild("background/bg_black_gradient").gameObject.GetComponent<UINineSlice>();
        //bgNineSlice.color = new Color(bgNineSlice.color.r, bgNineSlice.color.g, bgNineSlice.color.b, 0.0f);
        okButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        //Debug.LogError("CIPPA");
    }
    

    public void SetPopupText(string descr) //string title, 
    {
        //titleText.text = title;
        descriptionText.text = descr;
        //Debug.LogError("CIPPA");
    }

    void Signal_OnResumeButtonRelease(UIButton button)
    {
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIManager>.Get().PopPopup();
    }

    void OnShow(UIPopup popup)
    {
    }

    void OnBackButtonAction()
    {
        UIButton okButton = transform.FindChild("content/ResumeButton").GetComponent<UIButton>();
        okButton.onReleaseEvent.Invoke(okButton);
    }
}