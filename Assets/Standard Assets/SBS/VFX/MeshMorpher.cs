using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("SBS/VFX/MeshMorpher")]
class MeshMorpher : MonoBehaviour
{
    #region Public members
    public Mesh morphMesh = null;
    #endregion

    #region Protected members
    protected MeshFilter meshFilter;

    protected Vector3[] vertices;
    protected Vector3[] morphVertices;
    protected Vector3[] normals;
    protected Vector3[] morphNormals;

    protected int[] morphMap;
    protected Vector3[] destVertices;
    protected Vector3[] destNormals;
    #endregion

    #region Unity callbacks
    void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        vertices = mesh.vertices;
        morphVertices = morphMesh.vertices;

        normals = mesh.normals;
        morphNormals = morphMesh.normals;

        int[] triangles = meshFilter.sharedMesh.triangles;
        int[] morphTriangles = morphMesh.triangles;
        Asserts.Assert(triangles.Length == morphTriangles.Length);

        morphMap = new int[vertices.Length];
        destVertices = new Vector3[vertices.Length];
        destNormals = new Vector3[vertices.Length];

        Vector2[] uvs = mesh.uv;
        Vector2[] morphUvs = morphMesh.uv;

        int numTris = triangles.Length / 3,
            numMorphTris = morphTriangles.Length / 3;
        for (int i = 0; i < numTris; ++i)
        {
            int idx0 = triangles[i * 3 + 0],
                idx1 = triangles[i * 3 + 1],
                idx2 = triangles[i * 3 + 2];

            Vector2 uv0 = uvs[idx0], uv1 = uvs[idx1], uv2 = uvs[idx2];

            int j = 0, offset = 0;
            for (; j < numMorphTris; ++j)
            {
                int morphIdx0 = morphTriangles[j * 3 + 0],
                    morphIdx1 = morphTriangles[j * 3 + 1],
                    morphIdx2 = morphTriangles[j * 3 + 2];

                if (
                  Vector2.Distance(uv0, morphUvs[morphIdx0]) < 1.0e-3 &&
                  Vector2.Distance(uv1, morphUvs[morphIdx1]) < 1.0e-3 &&
                  Vector2.Distance(uv2, morphUvs[morphIdx2]) < 1.0e-3)
                {
                    offset = 0;
                    break;
                }
                else if (
                  Vector2.Distance(uv0, morphUvs[morphIdx1]) < 1.0e-3 &&
                  Vector2.Distance(uv1, morphUvs[morphIdx2]) < 1.0e-3 &&
                  Vector2.Distance(uv2, morphUvs[morphIdx0]) < 1.0e-3)
                {
                    offset = 1;
                    break;
                }
                else if (
                  Vector2.Distance(uv0, morphUvs[morphIdx2]) < 1.0e-3 &&
                  Vector2.Distance(uv1, morphUvs[morphIdx0]) < 1.0e-3 &&
                  Vector2.Distance(uv2, morphUvs[morphIdx1]) < 1.0e-3)
                {
                    offset = 2;
                    break;
                }
            }

            if (j < numMorphTris)
            {
                morphMap[idx0] = morphTriangles[j * 3 + ((offset + 0) % 3)];
                morphMap[idx1] = morphTriangles[j * 3 + ((offset + 1) % 3)];
                morphMap[idx2] = morphTriangles[j * 3 + ((offset + 2) % 3)];
            }
            else
            {
                Asserts.Assert(false);
                morphMap[idx0] = morphTriangles[idx0];
                morphMap[idx1] = morphTriangles[idx1];
                morphMap[idx2] = morphTriangles[idx2];
            }
        }
    }
    #endregion

    #region Messages
    void SetMorph(float t)
    {
        t = Mathf.Clamp01(t);

        int count = destVertices.Length;
        for (int i = 0; i < count; ++i)
        {
            destVertices[i] = Vector3.Lerp(vertices[i], morphVertices[morphMap[i]], t);
            destNormals[i] = Vector3.Lerp(normals[i], morphNormals[morphMap[i]], t).normalized;
        }

        meshFilter.mesh.vertices = destVertices;
        meshFilter.mesh.normals = destNormals;
    }
    #endregion
}
