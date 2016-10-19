using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Math;

[CustomEditor(typeof(Token))]
public class TokenEditor : Editor
{
    protected enum CurveDir
    {
        Left,
        Right
    }

    protected void UpdateNextLinkedTokens(Token prevToken, Token token, List<Token> changedTokens)
    {
        if (null == token)
            return;

#if UNITY_FLASH
        SBSVector3 pos = new SBSVector3(), tan = new SBSVector3();
        prevToken.TokenToWorld(1.0f, 0.0f, pos, tan);
#else
        SBSVector3 pos, tan;
        prevToken.TokenToWorld(1.0f, 0.0f, out pos, out tan);
#endif

        token.transform.position = pos;
        token.transform.LookAt(pos + tan, SBSVector3.up);
        token.OnMove();

        changedTokens.Add(token);

        EditorUtility.SetDirty(token);

        foreach (Token nextToken in token.nextLinks)
        {
            if (nextToken == target || changedTokens.Contains(nextToken))
                continue;

            this.UpdateNextLinkedTokens(token, nextToken, changedTokens);
        }
    }
    
    public override void OnInspectorGUI()
    {
        if (serializedObject.isEditingMultipleObjects)
            return;

        Token token = target as Token;

        token.type = (Token.TokenType)EditorGUILayout.EnumPopup("Type", token.type);
        token.width = EditorGUILayout.FloatField("Width", token.width);

        switch (token.type)
        {
            case Token.TokenType.Rect:
                token.lengthOrRadius = EditorGUILayout.FloatField("Length", token.lengthOrRadius);
                break;
            case Token.TokenType.Curve:
                token.lengthOrRadius = EditorGUILayout.FloatField("Radius", token.lengthOrRadius);
                token.arcAngle = EditorGUILayout.Slider("Angle", token.arcAngle, 0.0f, 90.0f);
                token.curveDir = CurveDir.Left == (CurveDir)EditorGUILayout.EnumPopup("Direction", token.curveDir >= 0.0f ? CurveDir.Left : CurveDir.Right) ? 1.0f : -1.0f;
                EditorGUILayout.LabelField("Length", token.Length.ToString());
                break;
        }

        SerializedProperty prevLinks = serializedObject.FindProperty("prevLinks"),
                           nextLinks = serializedObject.FindProperty("nextLinks");

        EditorGUILayout.PropertyField(prevLinks, true);
        EditorGUILayout.PropertyField(nextLinks, true);

        if (GUILayout.Button("Append"))
        {
#if UNITY_FLASH
            SBSVector3 pos = new SBSVector3(), tan = new SBSVector3();
            token.TokenToWorld(1.0f, 0.0f, pos, tan);
#else
            SBSVector3 pos, tan;
            token.TokenToWorld(1.0f, 0.0f, out pos, out tan);
#endif

            GameObject go = new GameObject("Token");
            Token newToken = go.AddComponent<Token>();

            newToken.type = token.type;
            newToken.lengthOrRadius = token.lengthOrRadius;
            newToken.width = token.width;
            newToken.arcAngle = token.arcAngle;
            newToken.curveDir = token.curveDir;

            go.transform.parent = token.transform.parent;
            go.transform.position = pos;
            go.transform.LookAt(pos + tan, SBSVector3.up);

            newToken.prevLinks.Add(token);
            token.nextLinks.Add(newToken);

            Selection.activeGameObject = go;
        }

        bool updateLinkedTokens = EditorWindow.GetWindow<TokenUtils>("Tokens", false).updateLinkedTokens;

        if (GUILayout.Button("Insert"))
        {
#if UNITY_FLASH
            SBSVector3 pos = new SBSVector3(), tan = new SBSVector3();
            token.TokenToWorld(1.0f, 0.0f, pos, tan);
#else
            SBSVector3 pos, tan;
            token.TokenToWorld(1.0f, 0.0f, out pos, out tan);
#endif

            GameObject go = new GameObject("Token");
            Token newToken = go.AddComponent<Token>();

            newToken.type = token.type;
            newToken.lengthOrRadius = token.lengthOrRadius;
            newToken.width = token.width;
            newToken.arcAngle = token.arcAngle;
            newToken.curveDir = token.curveDir;

            go.transform.parent = token.transform.parent;
            go.transform.position = pos;
            go.transform.LookAt(pos + tan, SBSVector3.up);

            newToken.OnMove();

            List<Token> oldNextLinks = new List<Token>(token.nextLinks);
            newToken.prevLinks.Add(token);
            token.nextLinks.Clear();
            token.nextLinks.Add(newToken);
            newToken.nextLinks = oldNextLinks;
            foreach (Token nextToken in oldNextLinks)
            {
                nextToken.prevLinks.Remove(token);
                nextToken.prevLinks.Add(newToken);
            }

            Selection.activeGameObject = go;

            if (updateLinkedTokens)
            {
                //Undo.RegisterSceneUndo("Token links");//ToDo
                foreach (Token nextToken in newToken.nextLinks)
                    this.UpdateNextLinkedTokens(newToken, nextToken, new List<Token>());
            }
        }

        if (GUILayout.Button("Delete"))
        {
            foreach (Token prevLink in token.prevLinks)
                prevLink.nextLinks.Remove(token);
            foreach (Token nextLink in token.nextLinks)
                nextLink.prevLinks.Remove(token);

            GameObject.DestroyImmediate(token.gameObject);
        }
/*
        if (GUILayout.Button("Create Box Colliders"))
        {
            BoxCollider[] boxes = token.gameObject.GetComponents<BoxCollider>();
        }
        */
        if (GUI.changed)
        {
#if UNITY_4_3
            Undo.RecordObject(target, "Token edit");
#else
            Undo.SetSnapshotTarget(target, "Token edit");
            Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
#endif
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            if (updateLinkedTokens)
            {
                //Undo.RegisterSceneUndo("Token links");//ToDo
                foreach (Token nextToken in token.nextLinks)
                    this.UpdateNextLinkedTokens(token, nextToken, new List<Token>());
            }
        }
    }
}
