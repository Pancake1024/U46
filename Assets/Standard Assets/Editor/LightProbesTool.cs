using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LightProbesTool : EditorWindow
{
    protected struct LPNodeId
    {
        public int x;
        public int y;

        public LPNodeId(int _x = 0, int _y = 0)
        {
            x = _x;
            y = _y;
        }
    }

    protected class LPNode
    {
        public Vector3 position;
        public LPNodeId id;

        public LPNode(Vector3 _position, LPNodeId _id)
        {
            position = _position;
            id = _id;
        }

        public void GetLinkPositions(float dist, ref Vector3[] array, ref LPNodeId[] ids)
        {
            array[0] = position + new Vector3( 0.0f, 0.0f,  dist); ids[0] = new LPNodeId(id.x + 0, id.y + 1);
            array[1] = position + new Vector3( dist, 0.0f,  dist); ids[1] = new LPNodeId(id.x + 1, id.y + 1);
            array[2] = position + new Vector3( dist, 0.0f,  0.0f); ids[2] = new LPNodeId(id.x + 1, id.y + 0);
            array[3] = position + new Vector3( dist, 0.0f, -dist); ids[3] = new LPNodeId(id.x + 1, id.y - 1);
            array[4] = position + new Vector3( 0.0f, 0.0f, -dist); ids[4] = new LPNodeId(id.x + 0, id.y - 1);
            array[5] = position + new Vector3(-dist, 0.0f, -dist); ids[5] = new LPNodeId(id.x - 1, id.y - 1);
            array[6] = position + new Vector3(-dist, 0.0f,  0.0f); ids[6] = new LPNodeId(id.x - 1, id.y + 0);
            array[7] = position + new Vector3(-dist, 0.0f,  dist); ids[7] = new LPNodeId(id.x - 1, id.y + 1);
        }
    }

    protected Renderer bounds;
    protected LightProbeGroup group;
    protected Transform start;
    protected float capsuleRadius;
    protected float capsuleHeight;
    protected float castDistance;

    [MenuItem("SBS/Light Probes Tool")]
    static void Init()
    {
        EditorWindow.GetWindow<LightProbesTool>("Light Probes Tool", true);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        bounds = EditorGUILayout.ObjectField("Bounds", bounds, typeof(Renderer), true) as Renderer;
        group = EditorGUILayout.ObjectField("Group", group, typeof(LightProbeGroup), true) as LightProbeGroup;
        start = EditorGUILayout.ObjectField("Start", start, typeof(Transform), true) as Transform;

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Capsule", EditorStyles.boldLabel);
        capsuleRadius = EditorGUILayout.FloatField("Radius", capsuleRadius);
        capsuleHeight = EditorGUILayout.FloatField("Height", capsuleHeight);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Casting", EditorStyles.boldLabel);
        castDistance = EditorGUILayout.FloatField("Distance", castDistance);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        if (group != null && start != null && GUILayout.Button("Create Light Probes"))
            group.probePositions = this.ComputeLightProbePositions();

        EditorGUILayout.EndVertical();
    }

    protected Vector3[] ComputeLightProbePositions()
    {
        List<Vector3> positions = new List<Vector3>();

        Vector3 h = new Vector3(0.0f, capsuleHeight, 0.0f);
        Bounds aabb = bounds.bounds;
        aabb.Expand(new Vector3(capsuleRadius, capsuleRadius + capsuleHeight + 2.0f, capsuleRadius));

        Vector3[] neighbours = new Vector3[8];
        LPNodeId[] neighbourIds = new LPNodeId[8];

        LPNode startNode = new LPNode(start.position, new LPNodeId());

        positions.Add(Vector3.zero);
        positions.Add(start.InverseTransformPoint(start.position + h));

        Stack<LPNode> nodes = new Stack<LPNode>();
        HashSet<LPNodeId> visitedNodes = new HashSet<LPNodeId>();
        nodes.Push(startNode);
        while (nodes.Count > 0)
        {
            LPNode node = nodes.Pop();

            node.GetLinkPositions(castDistance, ref neighbours, ref neighbourIds);
            for (int i = 0; i < 8; ++i)
            {
                if (!aabb.Contains(neighbours[i]) || visitedNodes.Contains(neighbourIds[i]))
                    continue;

                visitedNodes.Add(neighbourIds[i]);

                Vector3 dir = neighbours[i] - node.position;
                if (!Physics.CapsuleCast(node.position, node.position + h, capsuleRadius, dir, dir.magnitude))
                {
                    LPNode newNode = new LPNode(neighbours[i], neighbourIds[i]);

                    positions.Add(start.InverseTransformPoint(newNode.position));
                    positions.Add(start.InverseTransformPoint(newNode.position + h));
 
                    nodes.Push(newNode);
                }
            }
        }

        return positions.ToArray();
    }
}
