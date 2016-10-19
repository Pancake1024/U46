using UnityEngine;
using SBS.Core;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIMissionsPanelPopup")]
public class UIMissionsPanelPopup : MonoBehaviour
{
    public GameObject progressGroup;
    public UITextField titleText;
    public UITextField descriptionText;
    public UITextField progressTextPrefix;
    public UITextField progressTextGoal;
    public UITextField progressTextPostfix;
    public GameObject iconMissionCompleted;
    public GameObject iconDiamond;
    public UITextField specialText;
    
    public void OnEnable()
    {
        if (OnTheRunSingleRunMissions.Instance!=null && OnTheRunSingleRunMissions.Instance.CurrentTierMission != null)
        {
            OnTheRunSingleRunMissions.DriverMission currentMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
            titleText.text = OnTheRunDataLoader.Instance.GetLocaleString("run_goal");
            descriptionText.text = currentMission.description;

            int missionPartial = int.Parse(Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().tfMissionPartial.text);// int.Parse(Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().tfMissionPartial.text.Substring(0, Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().tfMissionPartial.text.Length - 1));
            

            progressTextPrefix.text = "[ " + missionPartial + " / ";
            progressTextGoal.text = currentMission.checkCounter.ToString();
            progressTextPostfix.text = "]";
            titleText.ApplyParameters();
            if (currentMission.Done)
            {
                progressTextPrefix.text = " ";
                progressTextGoal.text = " ";
                progressTextPostfix.text = " ";
                descriptionText.ApplyParameters();
                progressTextPrefix.ApplyParameters();
                progressTextGoal.ApplyParameters();
                progressTextPostfix.ApplyParameters();
            }
            else
            {
                descriptionText.ApplyParameters();
                progressTextPrefix.ApplyParameters();
                progressTextGoal.ApplyParameters();
                progressTextPostfix.ApplyParameters();

                //progressTextGoal.transform.localPosition = new Vector3(progressTextGoal.transform.localPosition.x + 0.025f, progressTextGoal.transform.localPosition.y, progressTextGoal.transform.localPosition.z);

                float xPos = progressTextGoal.transform.localPosition.x + progressTextGoal.GetTextBounds().width + 0.13f;
                progressTextPostfix.transform.localPosition = new Vector3(xPos, progressTextPostfix.transform.localPosition.y, progressTextPostfix.transform.localPosition.z);

                xPos = -progressTextGoal.GetTextBounds().width * 0.5f - 0.16f;
                progressGroup.transform.localPosition = new Vector3(xPos, progressGroup.transform.localPosition.y, progressGroup.transform.localPosition.z);
            }

            iconMissionCompleted.SetActive(currentMission.Done);
            iconDiamond.SetActive(currentMission.DiamondRewardActive);
            specialText.text = " ";
            specialText.ApplyParameters();
            if (currentMission.DiamondRewardActive)
                specialText.text = OnTheRunDataLoader.Instance.GetLocaleString("special_mission");
            specialText.ApplyParameters();
        }
    }
 
}