using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Math;

public class LightProbesTokenTool : EditorWindow
{
    protected LightProbeGroup group;
    protected float widthResolution;
    protected float lengthResolution;
    protected Token startToken;
    protected Token endToken;
    protected float height0;
    protected float height1;

    protected List<Token> GetTokens()
    {
        List<Token> tokens = new List<Token>();

        Token tok = startToken;
        while (endToken != null && tok != endToken)
        {
            if (tok.nextLinks.Count != 1)
                break;

            tokens.Add(tok);

            tok = tok.nextLinks[0];
        }

        tokens.Add(tok);

        return tokens;
    }

    protected void FillPositions(Token token, List<Vector3> positions)
    {
        float longProbes = token.Length * lengthResolution,
              trasvProbes = token.width * widthResolution;
        int longSteps = Mathf.FloorToInt(longProbes),
            trasvSteps = Mathf.FloorToInt(trasvProbes);
        float longitudinalStep = 1.0f / longProbes,
              trasversalStep = 2.0f / trasvProbes,
              longitudinalOffset = (1.0f - longSteps * longitudinalStep) * 0.5f,
              trasversalOffset = (2.0f - trasvSteps * trasversalStep) * 0.5f,
              longitudinal = longitudinalOffset;
        while (longitudinal < 1.0f)
        {
            float trasversal = -1.0f + trasversalOffset;
            while (trasversal < 1.0f)
            {
                SBSVector3 pos, tang;
                token.TokenToWorld(longitudinal, trasversal, out pos, out tang);

                RaycastHit[] hits = Physics.RaycastAll(pos + SBSVector3.up * 100.0f, Vector3.down);
                if (hits.Length > 0)
                {
                    int i = 0;
                    pos.y = hits[i++].point.y;
                    for (; i < hits.Length; ++i)
                        pos.y = Mathf.Min(pos.y, hits[i].point.y);
                }

                positions.Add(group.transform.InverseTransformPoint(pos + SBSVector3.up * height0));
                positions.Add(group.transform.InverseTransformPoint(pos + SBSVector3.up * height1));

                trasversal += trasversalStep;
            }

            longitudinal += longitudinalStep;
        }
    }

    protected Vector3[] ComputeLightProbePositions()
    {
        List<Token> tokens = this.GetTokens();
        List<Vector3> positions = new List<Vector3>();

        foreach (Token tok in tokens)
            this.FillPositions(tok, positions);

        return positions.ToArray();
    }

    [MenuItem("SBS/Light Probes Token Tool")]
    static void Init()
    {
        EditorWindow.GetWindow<LightProbesTokenTool>("Light Probes Token Tool", true);
    }

    void OnGUI()
    {
        group = EditorGUILayout.ObjectField("Group", group, typeof(LightProbeGroup), true) as LightProbeGroup;
        widthResolution = EditorGUILayout.FloatField("Width Resolution", widthResolution);
        lengthResolution = EditorGUILayout.FloatField("Length Resolution", lengthResolution);
        startToken = EditorGUILayout.ObjectField("Start Token", startToken, typeof(Token), true) as Token;
        endToken = EditorGUILayout.ObjectField("End Token", endToken, typeof(Token), true) as Token;
        height0 = EditorGUILayout.FloatField("Height 0", height0);
        height1 = EditorGUILayout.FloatField("Height 1", height1);

        if (/*group != null && */startToken != null && GUILayout.Button("Create Light Probes"))
        {
            if (null == group)
            {
                Transform tmp = startToken.transform.parent.FindChild("lightprobes");
                if (tmp != null)
                    group = tmp.GetComponent<LightProbeGroup>();

                if (null == group)
                    group = new GameObject("lightprobes", typeof(LightProbeGroup)).GetComponent<LightProbeGroup>();

                group.transform.parent = startToken.transform.parent;
                group.transform.localPosition = Vector3.zero;
                group.transform.localRotation = Quaternion.identity;
            }

            group.probePositions = this.ComputeLightProbePositions();
        }
    }
}
