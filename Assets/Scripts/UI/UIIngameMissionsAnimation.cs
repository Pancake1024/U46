using UnityEngine;
using SBS.Core;
using System.Collections.Generic;
using System;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIIngameMissionsAnimation")]
public class UIIngameMissionsAnimation : MonoBehaviour
{
    public GameObject missionRef;

    protected List<Transform> missionRows;
    protected UIRoot uiRoot;
    protected OnTheRunGameplay gameplayManager;
    protected List<OnTheRunSingleRunMissions.DriverMission> missionsList;
    protected int rowsToShow;

    void Initialize()
    {
        if (uiRoot == null)
            uiRoot = Manager<UIRoot>.Get();
        
        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        missionsList = OnTheRunSingleRunMissions.Instance.ActiveMissions;

        //Create rows........
        if (missionRows == null)
        {
            missionRows = new List<Transform>();
            missionRows.Add(missionRef.transform);
            for (int i = 0; i < 2; ++i)
            {
                GameObject currRow = Instantiate(missionRef, Vector3.zero, Quaternion.identity) as GameObject;
                currRow.transform.parent = gameObject.transform;
                missionRows.Add(currRow.transform);
            }
        }

        //reset rows...
        float rowDeltaDistance = 0.55f;
        for (int i = 0; i < 3; ++i)
        {
            Transform currentRow = missionRows[i];
            switch (i)
            {
                case 0: currentRow.transform.localPosition = Vector3.zero + Vector3.up * rowDeltaDistance; break;
                case 1: currentRow.transform.localPosition = Vector3.zero; break;
                case 2: currentRow.transform.localPosition = Vector3.zero + Vector3.down * rowDeltaDistance; break;
            }
            currentRow.FindChild("tfDescription").GetComponent<UITextField>().text = " ";
            currentRow.FindChild("tfValue").gameObject.SetActive(false);
            currentRow.FindChild("icon_mission_completed").gameObject.SetActive(false);
            currentRow.position = new Vector3(15.0f, currentRow.position.y, currentRow.position.z);
        }

        //setup rows
        OnTheRunSingleRunMissions.DriverMission currentMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
        int currentRowIndex = 0;
        rowsToShow = 0;
        for (int i = 0; i < 1; ++i)
        {
            OnTheRunSingleRunMissions.DriverMission currMission = currentMission;// missionsList[i];
            Transform currentRow = missionRows[currentRowIndex];
            Debug.Log("Popup Pause: " + currMission + " " + (currMission != null) + " " + missionsList.Count);
            if (currMission != null)
            {
                bool missionPassed = currMission.checkCounter <= currMission.Counter || currMission.JustPassed || currMission.Done;
                currentRow.FindChild("tfDescription").GetComponent<UITextField>().text = currMission.description;
                currentRow.FindChild("tfValue").GetComponent<UITextField>().text = uiRoot.FormatTextNumber((int)(Mathf.Floor(currMission.Counter))) + "/" + uiRoot.FormatTextNumber(currMission.checkCounter);
                currentRow.FindChild("tfValue").gameObject.SetActive(!missionPassed);
                currentRow.FindChild("icon_mission_completed").gameObject.SetActive(missionPassed);
                currentRow.FindChild("NewsBadge").gameObject.SetActive(OnTheRunDailyBonusManager.Instance.NewsBadgdesActive);
                if (currentRow.FindChild("NewsBadge").gameObject.activeInHierarchy)
                    currentRow.FindChild("NewsBadge").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("mission_new");
                ++currentRowIndex;
                ++rowsToShow;
            }
        }

    }

    void StartIngameMissionAnimation()
    {
        if (OnTheRunTutorialManager.Instance.WasTutorialActive)
            return;

        iTween.Stop();
        StartCoroutine(StartAnimation());
    }

    void StopIngameMissionAnimation()
    {
        iTween.Stop();
    }

    IEnumerator StartAnimation()
    {
        float transitionDuration = 0.45f,
              timeBetweenRows = 0.5f,
              timeRowStill = 2.5f,
              initialDelay = 0.5f;

        OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate = false;
        OnTheRunDailyBonusManager.Instance.NewsBadgdesActive = false;

        yield return new WaitForSeconds(initialDelay);

        if (rowsToShow>=1)
            StartCoroutine(SingleRowAnimation(missionRows[0].gameObject, transitionDuration, timeRowStill));

        yield return new WaitForSeconds(timeBetweenRows);

        if (rowsToShow >= 2)
            StartCoroutine(SingleRowAnimation(missionRows[1].gameObject, transitionDuration, timeRowStill));

        yield return new WaitForSeconds(timeBetweenRows);

        if (rowsToShow >= 3)
            StartCoroutine(SingleRowAnimation(missionRows[2].gameObject, transitionDuration, timeRowStill));

        yield return new WaitForSeconds(transitionDuration * 2 + timeRowStill);
    }

    IEnumerator SingleRowAnimation(GameObject row, float transitionDuration, float timeRowStill)
    {
        iTween.MoveTo(row, iTween.Hash("x", 0.0f, "easeType", "easeOutBack", "time", transitionDuration));

        yield return new WaitForSeconds(transitionDuration + timeRowStill);

        iTween.MoveTo(row, iTween.Hash("x", -15.0f, "easeType", "easeInBack", "time", transitionDuration));
    }

}
