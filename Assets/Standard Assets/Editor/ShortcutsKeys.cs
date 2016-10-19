using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class ShortcutsKeys : MonoBehaviour
{
    //DISABLE ALL OBJECTS IN HIERARCHY BY PRESSING CTRL+SHIFT+UP_ARROW
    [MenuItem("Tools/Enable-Disable Selected Object %#UP")]
    static void EnableDisableSelectedObject()
    {
        //if (Selection.activeTransform != null && Selection.activeTransform.parent == null)
            Selection.activeTransform.gameObject.SetActive(!Selection.activeTransform.gameObject.activeInHierarchy);
    }

    //ADD 1 TO SORTING ORDER OF UI OBJECTS CTRL+SHIFT+"+"
    [MenuItem("Tools/SortingOrder +1 %#+")]
    static void IncreaseSortingOrderOfOne()
    {
        if (Selection.activeTransform != null)
        {
            if (Selection.activeTransform.GetComponent<SpriteRenderer>() != null)
                Selection.activeTransform.GetComponent<SpriteRenderer>().sortingOrder++;
            if (Selection.activeTransform.GetComponent<UINineSlice>() != null)
                Selection.activeTransform.GetComponent<UINineSlice>().sortingOrder++;
            if (Selection.activeTransform.GetComponent<UITextField>() != null)
                Selection.activeTransform.GetComponent<UITextField>().sortingOrder++;

            UINineSlice[] ns = Selection.activeTransform.GetComponentsInChildren<UINineSlice>();
            foreach (UINineSlice n in ns)
            {
                Debug.Log("adding 1 to " + n.name);
                n.sortingOrder++;
            }

            SpriteRenderer[] sr = Selection.activeTransform.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer s in sr)
            {
                Debug.Log("adding 1 to " + s.name);
                s.sortingOrder++;
            }

            UITextField[] tf = Selection.activeTransform.GetComponentsInChildren<UITextField>();
            foreach (UITextField t in tf)
            {
                Debug.Log("adding 1 to " + t.name);
                t.sortingOrder++;
            }
        }
    }

    //SUBTRACT 1 TO SORTING ORDER OF UI OBJECTS CTRL+SHIFT+Keypad2
    [MenuItem("Tools/SortingOrder -1 %#-")]
    static void DecreaseSortingOrderOfOne()
    {
        if (Selection.activeTransform != null)
        {
            if (Selection.activeTransform.GetComponent<SpriteRenderer>() != null)
                Mathf.Clamp(Selection.activeTransform.GetComponent<SpriteRenderer>().sortingOrder--, 0, 2000);
            if (Selection.activeTransform.GetComponent<UINineSlice>() != null)
                Mathf.Clamp(Selection.activeTransform.GetComponent<UINineSlice>().sortingOrder--, 0, 2000);
            if (Selection.activeTransform.GetComponent<UITextField>() != null)
                Mathf.Clamp(Selection.activeTransform.GetComponent<UITextField>().sortingOrder--, 0, 2000);

            UINineSlice[] ns = Selection.activeTransform.GetComponentsInChildren<UINineSlice>();
            foreach (UINineSlice n in ns)
                Mathf.Clamp(n.sortingOrder--, 0, 2000);

            SpriteRenderer[] sr = Selection.activeTransform.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer s in sr)
                Mathf.Clamp(s.sortingOrder--, 0, 2000);

            UITextField[] tf = Selection.activeTransform.GetComponentsInChildren<UITextField>();
            foreach (UITextField t in tf)
                Mathf.Clamp(t.sortingOrder--, 0, 2000);
        }
    }

    //SET Z=0 IN EVERY OBJECT UNDER SELECTED CTRL+SHIFT++Keypad0
    [MenuItem("Tools/Zeta = 0 %#Keypad0")]
    static void SetZetaZero()
    {
        if (Selection.activeTransform != null)
        {
            Transform[] selectedTransforms = Selection.activeTransform.GetComponentsInChildren<Transform>();
            Selection.activeTransform.localPosition = new Vector3(Selection.activeTransform.localPosition.x, Selection.activeTransform.localPosition.y, 0.0f);

            foreach (Transform t in selectedTransforms)
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, 0.0f);
        }
    }

    //DISABLE ALL OBJECTS IN HIERARCHY BY PRESSING CTRL+SHIFT+LEFT_ARROW
    [MenuItem("Tools/Disable All Objects Except Selected %#LEFT")]
    static void DisableObjectsInHierarchyExceptSelected()
    {
        UnityEngine.Object[] obj = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject));
        foreach (UnityEngine.Object o in obj)
        {
            GameObject g = (GameObject)o;
            if (g.transform.parent == null && !GameObjectIsPrefab(g) && g.hideFlags == HideFlags.None)
            {
                if (g.name.Equals("UIManager") || g.tag.Equals("MainCamera"))
                    continue;

                Debug.Log(g.name + "DISABLED");
                g.SetActive(false);
            }
        }
        if (Selection.activeTransform != null && Selection.activeTransform.parent == null)
            Selection.activeTransform.gameObject.SetActive(true);
    }

    //ENABLE ALL OBJECTS IN HIERARCHY BY PRESSING CTRL+SHIFT+RIGHT_ARROW
    [MenuItem("Tools/Enable All Objects in Hierarchy %#RIGHT")]
    static void EnableObjectsInHierarchy()
    {
        UnityEngine.Object[] obj = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject));
        foreach (UnityEngine.Object o in obj)
        {
            GameObject g = (GameObject)o;
            if (g.transform.parent == null && !GameObjectIsPrefab(g) && g.hideFlags == HideFlags.None)
            {
                Debug.Log(g.name + "ENABLED");
                g.SetActive(true);
            }
        }
    }

    public static bool GameObjectIsPrefab(GameObject go)
    {
        string assetPath = AssetDatabase.GetAssetPath(go.transform.root.gameObject);
        return (!string.IsNullOrEmpty(assetPath));
    }
}
