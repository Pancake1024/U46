using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Math;

public class TokenUtils : EditorWindow
{
    protected bool keepHeight = true;

    protected int collNumSegments = 1;
    protected float collTrasvOffset = 0.0f;
    protected float collWallsHeight = 10.0f;
    protected float collDepth = 0.2f;

    public bool updateLinkedTokens = false;

    [MenuItem("SBS/Tokens")]
    static void Init()
    {
        EditorWindow.GetWindow<TokenUtils>("Tokens", true);
    }

    void OnGUI()
    {
        GUILayout.Label("Options", EditorStyles.boldLabel);
        updateLinkedTokens = EditorGUILayout.Toggle("Update linked tokens", updateLinkedTokens);
        keepHeight = EditorGUILayout.Toggle("Keep height when moving on tokens", keepHeight);
        EditorGUILayout.Space();

        GUILayout.Label("Selected Token", EditorStyles.boldLabel);
        if (null == Selection.activeGameObject || null == Selection.activeGameObject.GetComponent<Token>())
        {
            GUILayout.Label("None");
        }
        else
        {
            collNumSegments = EditorGUILayout.IntSlider("Segments", collNumSegments, 1, 50);
            collTrasvOffset = EditorGUILayout.FloatField("Trasversal Offset", collTrasvOffset);
            collWallsHeight = EditorGUILayout.FloatField("Walls Height", collWallsHeight);
            collDepth = EditorGUILayout.FloatField("Depth", collDepth);

            if (GUILayout.Button("Create/Refresh Colliders"))
            {
                Selection.activeGameObject.GetComponent<Token>().CreateBoxColliders(collNumSegments, collTrasvOffset, collWallsHeight, collDepth);
            }
        }

        GUILayout.Label("Selected transform", EditorStyles.boldLabel);
        if (null == Selection.activeTransform || Selection.activeTransform.gameObject.GetComponent<Token>() != null)
        {
            GUILayout.Label("None");
        }
        else
        {
            Transform tr = Selection.activeTransform = EditorGUILayout.ObjectField(Selection.activeTransform, typeof(Transform), true) as Transform;

            SBSVector3 pos = tr.position, tan = new SBSVector3();
            float l = 0.0f, t = 0.0f;
            Token tokenFound = null;
            Token[] tokens = FindObjectsOfType(typeof(Token)) as Token[];
            foreach (Token token in tokens)
            {
                token.WorldToToken(pos, out l, out t);
                if (l >= -1.0e-3f && l <= 1.001f && t >= -1.001f && t <= 1.001f)
                {
                    tokenFound = token;
                    break;
                }
            }

            if (tokenFound != null)
            {
                Token prevTokenFound = tokenFound;
                float l0 = l, t0 = t;

                tokenFound = EditorGUILayout.ObjectField("Token", tokenFound, typeof(Token), true) as Token;
                l = EditorGUILayout.Slider("Longitudial", l, 0.0f, 1.0f);
                t = EditorGUILayout.Slider("Trasversal", t, -1.0f, 1.0f);

                if (tokenFound != null && prevTokenFound != tokenFound || Mathf.Abs(l - l0) > 1.0e-3f || Mathf.Abs(t - t0) > 1.0e-3f)
                {
#if UNITY_FLASH
                    tokenFound.TokenToWorld(l, t, pos, tan);
#else
                    tokenFound.TokenToWorld(l, t, out pos, out tan);
#endif

                    if (keepHeight)
                        pos += SBSVector3.up * (SBSVector3.Dot(tr.position, SBSVector3.up) - SBSVector3.Dot(pos, SBSVector3.up));

                    tr.position = pos;

                    EditorUtility.SetDirty(tr);
                }
            }
        }
    }
}
