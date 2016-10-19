using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CheckpointTrigger : MonoBehaviour
{
    protected bool entered = false;

    #region Unity callbacks
    void OnEnable()
    {
        entered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player") && !entered)
        {
            //Debug.Log("*** CHECKPOINT ENTER ***");
            entered = true;
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.CHECKPOINT_EASY);
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.CHECKPOINT_HARD);
            Manager<UIRoot>.Get().OnCheckpointEnter();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            //Debug.Log("*** CHECKPOINT EXIT ***");
            entered = false;
        }
    }
    #endregion
}
