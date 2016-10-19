using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UIEditorWindow : EditorWindow
{
    [MenuItem("SBS/UI Editor Window")]
    static void Init()
    {
        EditorWindow.GetWindow<UIEditorWindow>("UI Editor", true);
    }

    protected static void WorldToScreen(Vector3 position, out int x, out int y)
    {
        UIManager uiMng = Manager<UIManager>.Get();
        if (null == uiMng)
        {
            x = 0;
            y = 0;
            return;
        }
        Camera uiCamera = uiMng.UICamera;
        float unitsToPixels = uiMng.pixelsPerUnit,
              halfWidth = uiMng.baseScreenWidth * 0.5f,
              halfHeight = uiMng.baseScreenHeight * 0.5f;
        Vector3 vp = uiCamera.worldToCameraMatrix * position;
        x = Mathf.RoundToInt(halfWidth + vp.x * unitsToPixels);
        y = Mathf.RoundToInt(halfHeight - vp.y * unitsToPixels);
    }

    protected static void ScreenToWorld(int x, int y, ref Vector3 position)
    {
        UIManager uiMng = Manager<UIManager>.Get();
        if (null == uiMng)
            return;
        float pixelsToUnits = 1.0f / uiMng.pixelsPerUnit,
              halfWidth = uiMng.baseScreenWidth * 0.5f,
              halfHeight = uiMng.baseScreenHeight * 0.5f;
        position.x = ((float)x - halfWidth) * pixelsToUnits;
        position.y = (halfHeight - (float)y) * pixelsToUnits;
    }

    protected static int UnitsToPixels(float x)
    {
        UIManager uiMng = Manager<UIManager>.Get();
        if (null == uiMng)
            return 0;
        return Mathf.RoundToInt(x * uiMng.pixelsPerUnit);
    }

    protected static float PixelsToUnits(int x)
    {
        UIManager uiMng = Manager<UIManager>.Get();
        if (null == uiMng)
            return 0.0f;
        return Mathf.RoundToInt((float)x / uiMng.pixelsPerUnit);
    }

    public struct TransformData
    {
        public Transform component;
        public int x, y;

        public TransformData(GameObjectData data, GameObject go)
        {
            component = go.transform;
            WorldToScreen(go.transform.position, out x, out y);
        }

        public void Revert(GameObjectData data)
        {
            WorldToScreen(component.position, out x, out y);
        }

        public void Apply(GameObjectData data)
        {
            Undo.RecordObject(component, "UI Editor");

            Vector3 p = component.position;
            ScreenToWorld(x, y, ref p);
            component.position = p;
        }
    }

    public struct NineSliceData
    {
        public UINineSlice component;
        public int x, y;
        public int w, h;

        public NineSliceData(GameObjectData data, GameObject go)
        {
            component = go.GetComponent<UINineSlice>();
            if (component != null)
            {
                WorldToScreen(go.transform.TransformPoint(new Vector3(component.pivot.x, component.pivot.y, 0.0f)), out x, out y);

                w = UnitsToPixels(component.width);
                h = UnitsToPixels(component.height);
            }
            else
            {
                x = y = w = h = 0;
            }
        }

        public void Revert(GameObjectData data)
        {
            if (null == component)
                return;

            WorldToScreen(component.transform.TransformPoint(new Vector3(component.pivot.x, component.pivot.y, 0.0f)), out x, out y);

            w = UnitsToPixels(component.width);
            h = UnitsToPixels(component.height);
        }

        public void Apply(GameObjectData data)
        {
            if (null == component)
                return;

            Undo.RecordObject(component, "UI Editor");

            Vector3 p = component.transform.TransformPoint(new Vector3(component.pivot.x, component.pivot.y, 0.0f));
            ScreenToWorld(x, y, ref p);
            p = component.transform.InverseTransformPoint(p);

            component.pivot = new Vector2(p.x, p.y);
            component.width = PixelsToUnits(w);
            component.height = PixelsToUnits(h);

            component.UpdateSlices();
        }
    }

    public struct TextFieldData
    {
        public UITextField component;
        public int x, y;
        public int w, h;

        public TextFieldData(GameObjectData data, GameObject go)
        {
            component = go.GetComponent<UITextField>();
            if (component != null)
            {
                WorldToScreen(go.transform.TransformPoint(new Vector3(component.pivot.x, component.pivot.y, 0.0f)), out x, out y);

                w = UnitsToPixels(component.width);
                h = UnitsToPixels(component.height);
            }
            else
            {
                x = y = w = h = 0;
            }
        }
    }

    public class GameObjectData
    {
        public TransformData transform;
        public NineSliceData nineSlice;
        public TextFieldData textField;

        public GameObjectData(GameObject go)
        {
            transform = new TransformData(this, go);
            nineSlice = new NineSliceData(this, go);
            textField = new TextFieldData(this, go);
        }
    }

    protected Dictionary<GameObject, GameObjectData> selection = new Dictionary<GameObject, GameObjectData>();

    void OnEnable()
    { }

    void OnDisable()
    { }

    void OnFocus()
    {
        this.OnSelectionChange();
    }

    void OnSelectionChange()
    {
        selection.Clear();
        foreach (GameObject go in Selection.gameObjects)
            selection.Add(go, new GameObjectData(go));
    }

    void OnGUI()
    {
        // for each transform change x & y in pixels
        EditorGUILayout.BeginVertical();
        foreach (KeyValuePair<GameObject, GameObjectData> item in selection)
        {
            EditorGUILayout.LabelField(item.Key.name, EditorStyles.boldLabel);

            item.Value.transform.Revert(item.Value);

            EditorGUILayout.BeginHorizontal();
            item.Value.transform.x = EditorGUILayout.IntField("X", item.Value.transform.x);
            item.Value.transform.y = EditorGUILayout.IntField("Y", item.Value.transform.y);
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
                item.Value.transform.Apply(item.Value);

            if (item.Value.nineSlice.component != null)
            {
                item.Value.nineSlice.Revert(item.Value);

                EditorGUILayout.BeginHorizontal();
                item.Value.nineSlice.x = EditorGUILayout.IntField("X", item.Value.nineSlice.x);
                item.Value.nineSlice.y = EditorGUILayout.IntField("Y", item.Value.nineSlice.y);
                item.Value.nineSlice.w = EditorGUILayout.IntField("W", item.Value.nineSlice.w);
                item.Value.nineSlice.h = EditorGUILayout.IntField("H", item.Value.nineSlice.h);
                EditorGUILayout.EndHorizontal();

                if (GUI.changed)
                    item.Value.nineSlice.Apply(item.Value);
            }
        }
        EditorGUILayout.EndVertical();
        // for each 9-slice & textfield
        //  change pivot & size in pixels
        //  change pivot position
        //  change width & height keeping edge (alignment)
    }
}
