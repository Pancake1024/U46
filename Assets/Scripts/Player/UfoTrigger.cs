using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Math;

public class UfoTrigger: MonoBehaviour
{
    protected List<GameObject> carInsideCone;
    protected float upSpeed = 8.5f;//10.0f;
    protected float scaleSpeed = 1.40f;//0.8f;
     
    #region Unity Callbacks
    void Awake()
    {
        carInsideCone = new List<GameObject>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Traffic") || other.gameObject.tag.Equals("Police")) // || other.gameObject.tag.Equals("Bonus") )
        {
            if (!carInsideCone.Contains(other.gameObject))
            {
                carInsideCone.Add(other.gameObject);
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.UfoTractorBeam);
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyCarsWithUfo);
                other.gameObject.SendMessage("OnTrappedByUfo");
            }
        }
    }

    void LateUpdate()
    {
        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < carInsideCone.Count; ++i)
        {
            GameObject currCar = carInsideCone[i];
            float currY = currCar.transform.position.y;
            currY += dt * upSpeed;
            //currCar.transform.Rotate(Vector3.up, dt * 360.0f);
            currCar.transform.position = new Vector3(gameObject.transform.position.x, currY, gameObject.transform.position.z);
            currCar.transform.localScale -= Vector3.one * dt * scaleSpeed;
        }
    }
    #endregion

    #region Messages
    void OnFlyingEnded( )
    {
        carInsideCone.Clear();
    }

    void OnExitFromUfo(GameObject go)
    {
        StartCoroutine(ScaleCoroutine());

        if (carInsideCone.Contains(go))
            carInsideCone.Remove(go);
    }

    IEnumerator ScaleCoroutine()
    {
        transform.parent.localScale = Vector3.one;

        float phaseDuration = 0.15f;
        float startTime = TimeManager.Instance.MasterSource.TotalTime;
        float elapsedTime = 0.0f;
        
        // Scale up
        while (elapsedTime < phaseDuration)
        {
            elapsedTime = TimeManager.Instance.MasterSource.TotalTime - startTime;

            float scale = SBSEasing.EaseInSine(elapsedTime, 1.0f, 0.3f, phaseDuration);
            transform.parent.localScale = Vector3.one * scale;

            yield return new WaitForEndOfFrame();
        }

        // Scale down
        startTime = TimeManager.Instance.MasterSource.TotalTime;
        elapsedTime = 0.0f;
        phaseDuration *= 5.0f;

        while (elapsedTime < phaseDuration)
        {
            elapsedTime = TimeManager.Instance.MasterSource.TotalTime - startTime;

            float scale = SBSEasing.EaseOutElastic(elapsedTime, 1.3f, -0.3f, phaseDuration);
            transform.parent.localScale = Vector3.one * scale;

            yield return new WaitForEndOfFrame();
        }

        transform.parent.localScale = Vector3.one;
    }
    #endregion
}
