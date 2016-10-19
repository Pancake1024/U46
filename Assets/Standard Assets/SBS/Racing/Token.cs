using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/Token")]
[ExecuteInEditMode]
public class Token : MonoBehaviour
{
    #region Public enums
    public enum TokenType
    {
        Rect = 0,
        Curve
    };
    #endregion

    #region Private members
    static private int uniqueIdCounter = 0;

    private int uniqueId = uniqueIdCounter++;
    private SBSMatrix4x4 matWorld = new SBSMatrix4x4();
    private SBSMatrix4x4 matInvWorld = new SBSMatrix4x4();
    private TrackBranch trackBranch = null;
    private int trackBranchIndex = -1;
    #endregion

    #region Public members
    public TokenType type;
    public float lengthOrRadius;
    public float width;
    public float arcAngle;
    public float curveDir;
    public List<Token> prevLinks = new List<Token>();
    public List<Token> nextLinks = new List<Token>();
#if UNITY_EDITOR
    public bool drawGizmos = true;
#endif
    #endregion

    #region Public properties
    public int UniqueId
    {
        get
        {
            return uniqueId;
        }
    }

    public float Length
    {
        get
        {
            switch (type)
            {
                case TokenType.Rect:
                    return lengthOrRadius;
                case TokenType.Curve:
                    return arcAngle * SBSMath.ToRadians * lengthOrRadius;
                default:
                    return 0.0f;
            }
        }
        set
        {
            switch (type)
            {
                case TokenType.Rect:
                    lengthOrRadius = value;
                    break;
                case TokenType.Curve:
                    arcAngle = value / (lengthOrRadius * SBSMath.ToRadians);
                    break;
            }
        }
    }

    public SBSVector3 Center
    {
        get
        {
            Asserts.Assert(TokenType.Curve == type);
            return matWorld.position - matWorld.MultiplyVector(SBSVector3.right) * (lengthOrRadius * curveDir);
        }
    }

    public bool IsTrackToken
    {
        get
        {
            return (trackBranchIndex != -1);
        }
    }

    public TrackBranch TrackBranch {
        get
        {
            return trackBranch;
        }
    }

    public int TrackBranchIndex {
        get
        {
            return trackBranchIndex;
        }
    }
    #endregion

    #region Messages
    void OnInit()
    {
        LevelObject obj = gameObject.GetComponent<LevelObject>();
        if (obj != null)
            obj.Bounds = this.ComputeBounds();
    }
    #endregion

    #region Protected methods
    public SBSBounds ComputeBounds()
    {
        int i = 0;
        float minX = float.MaxValue,
              minZ = float.MaxValue,
              maxX = float.MinValue,
              maxZ = float.MinValue;
        switch (type)
        {
            case TokenType.Curve:
                float offsetAngle = transform.rotation.y * SBSMath.ToRadians,
                      oneOverArc = 1.0f / arcAngle;
                float[] longitudinals = {
                                            0.0f,
                                            (SBSMath.PI * 0.5f - offsetAngle) * oneOverArc,
                                            (SBSMath.PI * 1.0f - offsetAngle) * oneOverArc,
                                            (SBSMath.PI * 1.5f - offsetAngle) * oneOverArc,
                                            (SBSMath.PI * 2.0f - offsetAngle) * oneOverArc,
                                            1.0f
                                        };
#if UNITY_FLASH
                SBSVector3 pos0 = new SBSVector3(), pos1 = new SBSVector3(), tang = new SBSVector3();
#else
                SBSVector3 pos0, pos1, tang;
#endif
                for (; i < 6; ++i)
                {
#if UNITY_FLASH
                    this.TokenToWorld(Mathf.Clamp01(longitudinals[i]), -1.0f, pos0, tang);
                    this.TokenToWorld(Mathf.Clamp01(longitudinals[i]),  1.0f, pos1, tang);
#else
                    this.TokenToWorld(Mathf.Clamp01(longitudinals[i]), -1.0f, out pos0, out tang);
                    this.TokenToWorld(Mathf.Clamp01(longitudinals[i]), 1.0f, out pos1, out tang);
#endif
                    minX = SBSMath.Min(minX, SBSMath.Min(pos0.x, pos1.x));
                    minZ = SBSMath.Min(minZ, SBSMath.Min(pos0.z, pos1.z));
                    maxX = SBSMath.Max(maxX, SBSMath.Max(pos0.x, pos1.x));
                    maxZ = SBSMath.Max(maxZ, SBSMath.Max(pos0.z, pos1.z));
                }
                break;
            case TokenType.Rect:
                Vector3[] corners = {
                                        new Vector3(-width * 0.5f, 0.0f, 0.0f),
                                        new Vector3( width * 0.5f, 0.0f, 0.0f),
                                        new Vector3(-width * 0.5f, 0.0f, lengthOrRadius),
                                        new Vector3( width * 0.5f, 0.0f, lengthOrRadius)
                                    };
                for (; i < 4; ++i)
                {
                    Vector3 v = transform.TransformPoint(corners[i]);

                    minX = SBSMath.Min(minX, v.x);
                    minZ = SBSMath.Min(minZ, v.z);
                    maxX = SBSMath.Max(maxX, v.x);
                    maxZ = SBSMath.Max(maxZ, v.z);
                }
                break;
        }

        SBSBounds b = new SBSBounds();
        b.min.Set(minX, 0.0f, minZ);
        b.max.Set(maxX, 0.0f, maxZ);

        return b;
    }
    #endregion

    #region Public methods
    public void LinkToTrack(TrackBranch _trackBranch, int _trackBranchIndex)
    {
        trackBranch = _trackBranch;
        trackBranchIndex = _trackBranchIndex;
    }

    public void OnMove()
    {
        matWorld = transform.localToWorldMatrix;
        matInvWorld = matWorld.inverseFast;
    }

    public void CreateMesh(int numSegments, ref Mesh mesh, bool append)
    {
        Vector3[] vertices = new Vector3[(numSegments + 1) * 2];
        int[] tris = new int[numSegments * 6];

        float l = 0.0f,
              s = 1.0f / (float)numSegments;
        this.OnMove();
        SBSVector3 p0, p1, t0, t1;
        int i = 0;
        for (; i < (numSegments + 1); ++i)
        {
            this.TokenToWorld(l, -1.0f, out p0, out t0);
            this.TokenToWorld(l,  1.0f, out p1, out t1);

            vertices[(i * 2) + 0] = p0;
            vertices[(i * 2) + 1] = p1;

            l += s;
        }

        for (i = 0; i < numSegments; ++i)
        {
            tris[(i * 6) + 0] = (i * 2) + 0;
            tris[(i * 6) + 1] = (i * 2) + 3;
            tris[(i * 6) + 2] = (i * 2) + 1;

            tris[(i * 6) + 3] = (i * 2) + 3;
            tris[(i * 6) + 4] = (i * 2) + 0;
            tris[(i * 6) + 5] = (i * 2) + 2;
        }

        if (null == mesh)
        {
            mesh = new Mesh();
            append = false;
        }

        if (append)
        {
            Vector3[] oldVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[oldVertices.Length + vertices.Length];
            int j = 0;
            for (; j < newVertices.Length; ++j)
            {
                if (j < oldVertices.Length)
                    newVertices[j] = oldVertices[j];
                else
                    newVertices[j] = vertices[j - oldVertices.Length];
            }
            mesh.vertices = newVertices;

            int[] oldTris = mesh.triangles;
            int[] newTris = new int[oldTris.Length + tris.Length];
            for (j = 0; j < newTris.Length; ++j)
            {
                if (j < oldTris.Length)
                    newTris[j] = oldTris[j];
                else
                    newTris[j] = oldVertices.Length + tris[j - oldTris.Length];
            }
            mesh.triangles = newTris;
        }
        else
        {
            mesh.vertices = vertices;
            mesh.triangles = tris;
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

#if UNITY_FLASH
    public void TokenToWorld(float longitudinal, float trasversal, SBSVector3 pos, SBSVector3 tang)
#else
    public void TokenToWorld(float longitudinal, float trasversal, out SBSVector3 pos, out SBSVector3 tang)
#endif
    {
        switch (type)
        {
            case TokenType.Rect:
#if UNITY_FLASH
                matWorld.MultiplyPoint3x4(SBSVector3.right * (trasversal * width * 0.5f) + SBSVector3.forward * (longitudinal * lengthOrRadius), pos);
                matWorld.MultiplyVector(SBSVector3.forward, tang);
#else
                pos = matWorld.MultiplyPoint3x4(SBSVector3.right * (trasversal * width * 0.5f) + SBSVector3.forward * (longitudinal * lengthOrRadius));
                tang = matWorld.MultiplyVector(SBSVector3.forward);
#endif
                break;

            case TokenType.Curve:
                SBSVector3 center = (-curveDir * lengthOrRadius) * SBSVector3.right,
                           offset = ((trasversal * width * 0.5f) + (curveDir * lengthOrRadius)) * SBSVector3.right;
                SBSMatrix4x4 matTr = SBSMatrix4x4.TRS(center, SBSQuaternion.identity, SBSVector3.one),
                             matRot = SBSMatrix4x4.TRS(SBSVector3.zero, SBSQuaternion.AngleAxis(-arcAngle * curveDir * longitudinal, SBSVector3.up), SBSVector3.one);

#if UNITY_FLASH
                (matWorld * matTr * matRot).MultiplyPoint3x4(offset, pos);
                (matWorld * matRot).MultiplyVector(SBSVector3.forward, tang);
#else
                pos = (matWorld * matTr * matRot).MultiplyPoint3x4(offset);
                tang = (matWorld * matRot).MultiplyVector(SBSVector3.forward);
#endif
                break;

            default:
                pos.x = pos.y = pos.z = 0.0f;
                tang.x = tang.y = tang.z = 0.0f;
                break;
        }
    }

    public void WorldToToken(SBSVector3 worldPos, out float longitudinal, out float trasversal)
    {
        switch (type)
        {
            case TokenType.Rect:
                SBSVector3 localPos = matInvWorld.MultiplyPoint3x4(worldPos);
                longitudinal = SBSVector3.Dot(localPos, SBSVector3.forward) / lengthOrRadius;
                trasversal = SBSVector3.Dot(localPos, SBSVector3.right) * 2.0f / width;
                break;

            case TokenType.Curve:
                SBSVector3 c = this.Center,
                           p0 = matWorld.position - c,
                           p1 = worldPos - c;

                p0.DecrementBy(SBSVector3.Dot(p0, SBSVector3.up) * SBSVector3.up);
                p1.DecrementBy(SBSVector3.Dot(p1, SBSVector3.up) * SBSVector3.up);

                float sign = SBSMath.Sign(SBSVector3.Dot(SBSVector3.Cross(p0, p1), SBSVector3.up));
                float angle = SBSVector3.Angle(p0, p1);

                longitudinal = angle / arcAngle;
                if (sign == curveDir)
                    longitudinal *= -1.0f;
                trasversal = (p1.magnitude - lengthOrRadius) * 2.0f * curveDir / width;
                break;

            default:
                longitudinal = 0.0f;
                trasversal = 0.0f;
                break;
        }
    }

    public void CreateBoxColliders(int numSegments, float trasversalOffset, float wallsHeight, float depth)
    {
        if (numSegments < 1)
            return;

        BoxCollider[] colliders = gameObject.GetComponentsInChildren<BoxCollider>();
        foreach (BoxCollider coll in colliders)
            GameObject.DestroyImmediate(coll.gameObject);
        colliders = null;

        float l = 0.0f,
              s = 1.0f / (float)numSegments;

        for (int i = 0; i < numSegments; ++i)
        {
#if UNITY_FLASH
            SBSVector3 p0 = new SBSVector3(), p1 = new SBSVector3(), q0 = new SBSVector3(), q1 = new SBSVector3(), t = new SBSVector3();

            this.TokenToWorld(l, -1.0f - trasversalOffset, p0, t);
            this.TokenToWorld(l + s, -1.0f - trasversalOffset, p1, t);
            this.TokenToWorld(l, 1.0f + trasversalOffset, q0, t);
            this.TokenToWorld(l + s, 1.0f + trasversalOffset, q1, t);
#else
            SBSVector3 p0, p1, q0, q1, t;

            this.TokenToWorld(l, -1.0f - trasversalOffset, out p0, out t);
            this.TokenToWorld(l + s, -1.0f - trasversalOffset, out p1, out t);
            this.TokenToWorld(l, 1.0f + trasversalOffset, out q0, out t);
            this.TokenToWorld(l + s, 1.0f + trasversalOffset, out q1, out t);
#endif

            SBSVector3 tanL = p1 - p0,
                       tanR = q1 - q0,
                       biTan = q0 - p0;
            float sizeX = biTan.Normalize(),
                  sizeZL = tanL.Normalize() + 0.2f,
                  sizeZR = tanR.Normalize() + 0.2f;

            SBSQuaternion rotL = SBSQuaternion.AngleAxis(SBSVector3.Angle(SBSVector3.forward, tanL), SBSVector3.Cross(SBSVector3.forward, tanL)),
                          rotR = SBSQuaternion.AngleAxis(SBSVector3.Angle(SBSVector3.forward, tanR), SBSVector3.Cross(SBSVector3.forward, tanR));

            SBSVector3 groundCenter = (p0 + p1 + q0 + q1) * 0.25f - SBSVector3.up * depth * 0.5f,
                       wallLeftCenter = (p0 + p1 + SBSVector3.up * wallsHeight - biTan * depth) * 0.5f,
                       wallRightCenter = (q0 + q1 + SBSVector3.up * wallsHeight + biTan * depth) * 0.5f,
                       groundSize = new SBSVector3(sizeX, depth, SBSMath.Max(sizeZL, sizeZR)),
                       wallLeftSize = new SBSVector3(depth, wallsHeight, sizeZL),
                       wallRightSize = new SBSVector3(depth, wallsHeight, sizeZR);

            GameObject ground = new GameObject("Ground", typeof(BoxCollider));
            ground.transform.parent = transform;
            ground.transform.rotation = rotL;
            ground.transform.position = groundCenter;
            ground.GetComponent<BoxCollider>().size = groundSize;

            GameObject wallLeft = new GameObject("WallLeft", typeof(BoxCollider)),
                       wallRight = new GameObject("WallRight", typeof(BoxCollider));
            wallLeft.transform.parent = wallRight.transform.parent = transform;

            wallLeft.transform.rotation = rotL;
            wallLeft.GetComponent<BoxCollider>().size = wallLeftSize;
            wallLeft.transform.position = wallLeftCenter;

            wallRight.transform.rotation = rotR;
            wallRight.GetComponent<BoxCollider>().size = wallRightSize;
            wallRight.transform.position = wallRightCenter;

            l += s;
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        matWorld = transform.localToWorldMatrix;
        matInvWorld = matWorld.inverseFast;
    }

#if UNITY_EDITOR
    public void DrawGizmos(float trasversal, Color col)
    {
        int numSegments = (TokenType.Rect == type ? 1 : 10);//Math.Max(1, (int)(lengthOrRadius / 10.0f)));

        float l = 0.0f,
              s = 1.0f / (float)numSegments;

        Gizmos.color = col;

        for (int i = 0; i < numSegments; ++i)
        {
#if UNITY_FLASH
            SBSVector3 p0 = new SBSVector3(), t0 = new SBSVector3(), p1 = new SBSVector3(), t1 = new SBSVector3();

            this.TokenToWorld(l, trasversal, p0, t0);
            this.TokenToWorld(l + s, trasversal, p1, t1);
#else
            SBSVector3 p0, t0, p1, t1;

            this.TokenToWorld(l, trasversal, out p0, out t0);
            this.TokenToWorld(l + s, trasversal, out p1, out t1);
#endif

            Gizmos.DrawLine(p0, p1);
//          Gizmos.DrawRay(p0, t0);

            l += s;
        }
    }
    
    void OnDrawGizmos()
    {
        matWorld = transform.localToWorldMatrix;
        matInvWorld = matWorld.inverseFast;

        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(transform.position, HandleUtility.GetHandleSize(transform.position) * 0.1f);

        if (!drawGizmos)
        {
            drawGizmos = true;
            return;
        }

        this.DrawGizmos(-1.0f, Color.gray);
        this.DrawGizmos(1.0f, Color.gray);
    }

    void OnDrawGizmosSelected()
    {
        matWorld = transform.localToWorldMatrix;
        matInvWorld = matWorld.inverseFast;

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, HandleUtility.GetHandleSize(transform.position) * 0.1f);

        this.DrawGizmos(-1.0f, Color.white);
        this.DrawGizmos(1.0f, Color.white);
    }
#endif
    #endregion
}
