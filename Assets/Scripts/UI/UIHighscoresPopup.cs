using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIHighscoresPopup")]
public class UIHighscoresPopup : MonoBehaviour
{
    public GameObject[] comingSoonObjects;

    protected OnTheRunInterfaceSounds interfaceSounds;
    protected UIGaragePage garagePage;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
    }

    void OnShow(UIPopup popup)
    {
        transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_back");

        transform.FindChild("content/title").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_highscores_title");
        transform.FindChild("content/description").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_highscores_description");

        transform.FindChild("content/location1/tfTextField").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("europeanCarsTitle");
        transform.FindChild("content/location2/tfTextField").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("orientalCarsTitle");
        transform.FindChild("content/location3/tfTextField").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("americanCarsTitle");
        transform.FindChild("content/location4/tfTextField").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("muscleCarsTitle");

        if (garagePage == null)
            garagePage = Manager<UIManager>.Get().GetPage("GaragePage").GetComponent<UIGaragePage>();

        for (int i = 0; i < comingSoonObjects.Length; ++i)
        {
            bool disable = garagePage.IsComingSoonActive(i);
            comingSoonObjects[i].SetActive(disable);
            transform.FindChild("content/location" + (i + 1)).GetComponent<BoxCollider2D>().enabled = !disable;
            transform.FindChild("content/location" + (i + 1)).GetComponent<UIButton>().State = disable ? UIButton.StateType.Disabled : UIButton.StateType.Normal;
        }
    }
    
    private void OnTier1Click(UIButton kButton)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        GameObject.Find("MiniclipAPI").GetComponent<MiniclipAPIManager>().ShowHighscores(new MiniclipAPIManager.HighscoreData(1, OnTheRunDataLoader.Instance.GetLocaleString("europeanCarsTitle")));
    }

    private void OnTier2Click(UIButton kButton)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        GameObject.Find("MiniclipAPI").GetComponent<MiniclipAPIManager>().ShowHighscores(new MiniclipAPIManager.HighscoreData(2, OnTheRunDataLoader.Instance.GetLocaleString("orientalCarsTitle")));
    }

    private void OnTier3Click(UIButton kButton)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        GameObject.Find("MiniclipAPI").GetComponent<MiniclipAPIManager>().ShowHighscores(new MiniclipAPIManager.HighscoreData(3, OnTheRunDataLoader.Instance.GetLocaleString("americanCarsTitle")));
    }

    private void OnTier4Click(UIButton kButton)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        GameObject.Find("MiniclipAPI").GetComponent<MiniclipAPIManager>().ShowHighscores(new MiniclipAPIManager.HighscoreData(4, OnTheRunDataLoader.Instance.GetLocaleString("muscleCarsTitle")));
    }
}