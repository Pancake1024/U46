using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Math;

public class LockedBehaviour: MonoBehaviour
{
    protected List<string> unaffectObjects = new List<string>{"baron_propellers"};

    #region Messages
    public void OnObjectLocked(Material[] lockedMaterial, MeshRenderer[] objectsToAffect)
    {
        for (int i = 0; i < objectsToAffect.Length; ++i)
        {
            if (unaffectObjects.IndexOf(objectsToAffect[i].gameObject.name) < 0)
            {
                if (objectsToAffect[i].gameObject.name.Contains("pl_ufo"))
                    objectsToAffect[i].materials = lockedMaterial;
                else
                    objectsToAffect[i].material = lockedMaterial[0];

                if (objectsToAffect[i].gameObject.name.Contains("pl_biplane"))
                {
                    objectsToAffect[i].transform.GetChild(0).gameObject.GetComponent<Renderer>().material = lockedMaterial[1];
                }
            }
        }
    }

    public void OnObjectUnlocked(Material[] defaultMaterial, MeshRenderer[] objectsToAffect)
    {
        for (int i = 0; i < objectsToAffect.Length; ++i)
        {
            if (unaffectObjects.IndexOf(objectsToAffect[i].gameObject.name) < 0)
            {
                if (objectsToAffect[i].gameObject.name.Contains("pl_ufo"))
                    objectsToAffect[i].materials = defaultMaterial;
                else
                    objectsToAffect[i].material = defaultMaterial[0];

                if (objectsToAffect[i].gameObject.name.Contains("pl_biplane"))
                {
                    objectsToAffect[i].transform.GetChild(0).gameObject.GetComponent<Renderer>().material = defaultMaterial[1];
                }
            }
        }
    }
    #endregion
}
