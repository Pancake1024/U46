using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SBS/Managers/ManagersRoot")]
public class ManagersRoot : MonoBehaviour
{
    #region Unity callbacks
    void Awake()
    {
        foreach (Transform child in transform)
            GameObject.DontDestroyOnLoad(child.gameObject);

        GameObject.DontDestroyOnLoad(gameObject);
  

    }

    void OnLevelWasLoaded()
    {
        UnityEngine.Object levelRoot = FindObjectOfType(typeof(LevelRoot));
        if (levelRoot != null)
        {
            transform.parent = ((LevelRoot)levelRoot).transform;
        }
    }
    #endregion

    #region Messages
    void OnLoadNewLevel()
    {
        transform.parent = null;
    }
    #endregion
}
