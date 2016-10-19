using UnityEngine;
using SBS.Core;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIMissionsPanel")]
public class UIMissionsPanel : MonoBehaviour
{

    protected List<OnTheRunSingleRunMissions.MissionType> missionsCompletedList;
    protected bool panelUsed = false;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected UIManager uiManager;

    protected float startYPosition;
    protected float panelSpeed = 2.0f;
    protected float stayYPosition = -3.16f;
    protected float stayIdleTime = 2.0f;
    protected AnimationState state = AnimationState.IDLE;
    protected float panelTimer = -1.0f;
    protected enum AnimationState
    {
        IDLE,
        ENTER,
        STAY,
        LEAVE
    }

    public void ShowMissionsPanel(OnTheRunSingleRunMissions.MissionType type)
    {
        if (missionsCompletedList == null)
            missionsCompletedList = new List<OnTheRunSingleRunMissions.MissionType>();

        if (missionsCompletedList.IndexOf(type)<0)
        {
            missionsCompletedList.Add(type);

            if (!panelUsed)
            {
                //StartPanelAnimation(missionsCompletedList[0]);
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().StartMissionPassedAnimation();
            }
        }
    }

    public void ResetMissionsPanel()
    {
        if (missionsCompletedList != null && missionsCompletedList.Count > 0)
        {
            missionsCompletedList.Clear();
            state = AnimationState.IDLE;
            transform.position = new Vector3(transform.position.x, startYPosition, transform.position.z);
        }
    }

    public void OnMissionsPanelEnded()
    {
        state = AnimationState.IDLE;
        transform.position = new Vector3(transform.position.x, startYPosition, transform.position.z);

        missionsCompletedList.RemoveAt(0);
        if (missionsCompletedList.Count > 0)
            StartPanelAnimation(missionsCompletedList[0]);
        else
            panelUsed = false;
    }

    protected void StartPanelAnimation(OnTheRunSingleRunMissions.MissionType type)
    {
        panelUsed = true;

        OnTheRunSingleRunMissions.DriverMission missionCompleted = null;
        List<OnTheRunSingleRunMissions.DriverMission> missionsList = OnTheRunSingleRunMissions.Instance.ActiveMissions;
        for (int i = 0; i < missionsList.Count; ++i)
        {
            if (missionsList[i] != null)
            {
                if (missionsList[i].type == type)
                {
                    missionCompleted = missionsList[i];
                    break;
                }
            }
        }

        if (missionCompleted != null)
        { 
            transform.FindChild("titleText").gameObject.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("mission_completed");
            transform.FindChild("missionText").gameObject.GetComponent<UITextField>().text = missionCompleted.description;
        }
        else
        {
            Debug.LogError("Mission completed error: no active mission found -> " + type);
        }

        state = AnimationState.ENTER;
    }

    void Start()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        startYPosition = transform.position.y;

        float heightScale = Manager<UIManager>.Get().baseScreenHeight / Manager<UIManager>.Get().UICamera.pixelHeight,
              aspectScale = (Manager<UIManager>.Get().baseScreenHeight * Manager<UIManager>.Get().UICamera.aspect) / Manager<UIManager>.Get().baseScreenWidth;
        transform.FindChild("titleText").gameObject.GetComponent<UILayoutAndScaling>().UpdateLayoutAndScaling(heightScale, aspectScale);
        transform.FindChild("missionText").gameObject.GetComponent<UILayoutAndScaling>().UpdateLayoutAndScaling(heightScale, aspectScale);
    }

    void Update( )
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        if (uiManager==null)
            uiManager = Manager<UIManager>.Get();

        if (uiManager.ActivePageName != "IngamePage")
            ResetMissionsPanel();
        else
        {
            switch (state)
            {
                case AnimationState.ENTER:
                    transform.position += Vector3.up * panelSpeed * dt;
                    if (transform.position.y >= stayYPosition)
                    {
                        state = AnimationState.STAY;
                        panelTimer = stayIdleTime;
                        transform.position = new Vector3(transform.position.x, stayYPosition, transform.position.z);
                    }
                    break;
                case AnimationState.STAY:
                    panelTimer -= dt;
                    if (panelTimer < 0.0f)
                    {
                        state = AnimationState.LEAVE;
                    }
                    break;
                case AnimationState.LEAVE:
                    transform.position += Vector3.down * panelSpeed * dt;
                    if (transform.position.y <= startYPosition)
                    {
                        OnMissionsPanelEnded();
                    }
                    break;
            }
        }
    }
}
