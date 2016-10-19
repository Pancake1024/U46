using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class CopyComponentData : ScriptableWizard
{
    #region Public members
    public GameObject objSrc = null;
    public GameObject objDst = null;
    #endregion

    #region Wizard callback
    [MenuItem("SBS/Copy Component Data")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Copy Component Data", typeof(CopyComponentData), "OK");
    }
    #endregion

    #region Unity callbacks
    void OnWizardUpdate()
    {
        helpString = "Copy data of same components from <objSrc> to <objDest>";
        if (null == objSrc)
        {
            errorString = "<objSrc> missing";
            isValid = false;
            return;
        }
        if (null == objDst)
        {
            errorString = "<objDst> missing";
            isValid = false;
            return;
        }

        PrefabType objSrcPrefabType = PrefabUtility.GetPrefabType(objSrc),
                   objDstPrefabType = PrefabUtility.GetPrefabType(objDst);
        if (objSrcPrefabType == PrefabType.Prefab || objSrcPrefabType == PrefabType.ModelPrefab)
        {
            errorString = "<objSrc> can't be a prefab";
            isValid = false;
            return;
        }
        if (objDstPrefabType == PrefabType.Prefab || objDstPrefabType == PrefabType.ModelPrefab)
        {
            errorString = "<objDst> can't be a prefab";
            isValid = false;
            return;
        }

        errorString = "";
        isValid = true;
    }

    void OnWizardCreate()
    {
        MonoBehaviour[] srcBehaviors = objSrc.GetComponents<MonoBehaviour>(),
                        dstBehaviors = objDst.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour srcBeh in srcBehaviors)
        {
            Type srcType = srcBeh.GetType();

            foreach (MonoBehaviour dstBeh in dstBehaviors)
            {
                Type dstType = dstBeh.GetType();
                if (srcType == dstType)
                {
                    FieldInfo[] fields =  srcType.GetFields();
                    foreach (FieldInfo field in fields)
                    {
                        if (field.IsPublic && !field.IsStatic && !field.IsNotSerialized)
                            field.SetValue(dstBeh, field.GetValue(srcBeh));
                    }

                    break;
                }
            }
        }
    }
    #endregion
}
