using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

public class BounceBehaviour : MonoBehaviour
{
    float startTime;
    float elapsedTime;
    float startYPosition = -9999.0f;
    float startYPositionTemp = -9999.0f;
    float animationDuration1 = 0.2f; //0.3f;//0.4f;
    float animationDuration2 = 0.5f; //0.6f;//1.0f;
    int direction = 1;

    iTween currentTween;

    #region Unity callbacks
    void Start()
    {
        startTime = TimeManager.Instance.MasterSource.TotalTime;
        elapsedTime = 0.0f;
    }

    void OnDestroy()
    {
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, startYPosition, gameObject.transform.localPosition.z);
    }

    void Update()
    {
        currentTween = gameObject.GetComponent<iTween>();
        if (currentTween == null)
        {
            if (startYPosition == -9999.0f)
            {
                startTime = TimeManager.Instance.MasterSource.TotalTime;
                startYPosition = gameObject.transform.localPosition.y;
            }

            elapsedTime = TimeManager.Instance.MasterSource.TotalTime - startTime;

            if (direction == 1)
            {
                if (elapsedTime < animationDuration1)
                {
                    float yPosition = SBSEasing.EaseOutCubic(elapsedTime, startYPosition, 0.2f, animationDuration1);
                    gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, yPosition, gameObject.transform.localPosition.z);
                }
                else
                {
                    direction = -1;
                    startTime = TimeManager.Instance.MasterSource.TotalTime;
                    elapsedTime = 0.0f;
                    startYPositionTemp = gameObject.transform.localPosition.y;
                    //gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, startYPosition, gameObject.transform.localPosition.z);
                }
            }
            else
            {
                if (elapsedTime < animationDuration2)
                {
                    float yPosition = SBSEasing.EaseOutBounce(elapsedTime, startYPositionTemp, -0.2f, animationDuration2);
                    gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, yPosition, gameObject.transform.localPosition.z);
                }
                else
                {
                    direction = 1;
                    startTime = TimeManager.Instance.MasterSource.TotalTime;
                    elapsedTime = 0.0f;
                    gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, startYPosition, gameObject.transform.localPosition.z);
                }
            }
        }
        else
        {
            direction = 1;
            startYPosition = -9999.0f;
        }
    }
    #endregion
}
