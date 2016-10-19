using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using SBS.Core;

[CustomPropertyDrawer(typeof(Signal))]
public class SignalDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty targetsProperty = property.FindPropertyRelative("targets");

        GUI.Label(position, label.text + " (Signal)", EditorStyles.label);
        int c = targetsProperty.arraySize;
        float indent  = 30.0f,
              rowSize = 0 == c ? 32.0f : ((position.height - 32.0f) / (float)c);
        Rect prefixRect = new Rect(position.x + indent, position.y + 16.0f, (position.width - indent) * 0.25f, rowSize * 0.5f);
        Rect valueRect = new Rect(prefixRect.x + prefixRect.width, prefixRect.y, (position.width - indent) * 0.65f, rowSize * 0.5f);
        Rect delRect = new Rect(valueRect.x + valueRect.width, prefixRect.y, (position.width - indent) * 0.10f, rowSize);

        for (int i = 0; i < c; ++i)
        {
            SerializedProperty targetProperty = targetsProperty.GetArrayElementAtIndex(i);

            GUI.Label(prefixRect, "Target", EditorStyles.miniLabel);
            SerializedProperty goProperty = targetProperty.FindPropertyRelative("gameObject");
            if (null == goProperty)
                continue;
            goProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, goProperty.objectReferenceValue, typeof(GameObject), true);

            prefixRect.y += prefixRect.height + 1;
            valueRect.y += valueRect.height + 1;

            SerializedProperty methodProperty = targetProperty.FindPropertyRelative("method");
            GUI.Label(prefixRect, "Method", EditorStyles.miniLabel);

            string argType = property.FindPropertyRelative("argType").stringValue;
            List<string> names = new List<string>();
            List<string> methodNames = new List<string>();
            names.Add("None");
            methodNames.Add(null);
            int index = 0;
            GameObject obj = goProperty.objectReferenceValue as GameObject;
            if (obj != null)
            {
                MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in components)
                {
                    if (null == component)
                        continue;

                    Type type = component.GetType();
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    foreach (MethodInfo method in methods)
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        bool addMethod = false;

                        if (string.IsNullOrEmpty(argType))
                        {
                            if (parameters.Length == 0)
                                addMethod = true;
                        }
                        else
                        {
                            if (parameters.Length == 1 && parameters[0].ParameterType.FullName.Equals(argType))
                                addMethod = true;
                        }

                        if (addMethod)
                        {
                            string name = ObjectNames.NicifyVariableName(method.Name);
                            names.Add(type.Name + ": " + name);
                            if (methodProperty.stringValue == method.Name)
                                index = methodNames.Count;
                            methodNames.Add(method.Name);
                        }
                    }
                }

                if (names.Count > 1)
                {
                    EditorGUI.BeginChangeCheck();
                    index = EditorGUI.Popup(valueRect, index, names.ToArray());
                    if (EditorGUI.EndChangeCheck())
                        methodProperty.stringValue = methodNames[index];
                }
                else
                    GUI.Label(valueRect, "None");
            }
            else
                GUI.Label(valueRect, "None");

            prefixRect.y += prefixRect.height + 1;
            valueRect.y += valueRect.height + 1;

            if (GUI.Button(delRect, "-"))
            {
                targetsProperty.DeleteArrayElementAtIndex(i);
                break;
            }

            delRect.y += delRect.height + 1;
        }

        delRect.height = Mathf.Max(delRect.height * 0.5f, 16.0f);
        if (GUI.Button(delRect, "+"))
        {
            targetsProperty.InsertArrayElementAtIndex(targetsProperty.arraySize);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty targetsProperty = property.FindPropertyRelative("targets");
        return 16.0f + 32.0f * targetsProperty.arraySize + 16.0f;
    }
}
