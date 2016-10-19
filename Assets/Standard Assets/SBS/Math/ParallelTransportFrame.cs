using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("SBS/Math/ParallelTransportFrame")]
public class ParallelTransportFrame : MonoBehaviour
{
    #region Protected structs
    protected struct State
    {
        public float space;
        public SBSMatrix4x4 frame;
        public SBSVector3 tangent;
        public SBSMatrix4x4 final;

        public State(float _space, SBSMatrix4x4 _frame, SBSVector3 _tangent, SBSMatrix4x4 _final)
        {
            space = _space;
            frame = _frame;
            tangent = _tangent;
            final = _final;
        }
    }
    #endregion

    #region Public members
    public List<PTFPoint> points = new List<PTFPoint>();
    public Curve.Interpolation curveIntType = Curve.Interpolation.CatmullRom;
    public bool curveIsClosed = false;
#if UNITY_FLASH
    [NonSerialized]
#endif
    public AnimationCurve data;
#if UNITY_FLASH
    public float[] dataKeys;
#endif
    #endregion

    #region Protected members
    protected Curve curve = new Curve(Curve.Interpolation.CatmullRom, false);
    protected float prevSpace;
    protected SBSMatrix4x4 prevFrame = new SBSMatrix4x4();
    protected SBSVector3 prevTangent;
    protected SBSMatrix4x4 finalFrame;
    protected Stack<State> statesStack = new Stack<State>();
    #endregion

    #region Public properties
    public Curve Curve
    {
        get
        {
            return curve;
        }
    }

    public float Space
    {
        get
        {
            return prevSpace;
        }
    }

    public SBSMatrix4x4 Frame
    {
        get
        {
            return finalFrame;
        }
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
#if UNITY_FLASH
        Keyframe[] keys = new Keyframe[dataKeys.Length / 2];
        for (int c = 0; c < keys.Length; ++c)
        {
            keys[c].time  = dataKeys[(c * 2) + 0];
            keys[c].value = dataKeys[(c * 2) + 1];
        }
        data = new AnimationCurve(keys);
#endif
        curve.Clear();

        curve.InterpolationType = curveIntType;
        curve.IsClosed = curveIsClosed;

        for (int i = 0; i < points.Count; ++i)
            curve.Add(points[i].transform.position);

        if (curve.IsClosed)
            curve.Add(points[0].transform.position);

        this.Reset();
    }
    #endregion

    #region Protected methods
    #endregion

    #region Public methods
    #region Editor functions
#if UNITY_EDITOR
    public int EditorGetPointIndex(PTFPoint point)
    {
        for (int i = points.Count - 1; i >= 0; --i)
        {
            if (points[i] == point)
                return i;
        }
        return -1;
    }

    public void EditorRemovePointAt(int index)
    {
        Asserts.Assert(index >= 0 && index < points.Count);

        GameObject point = points[index].gameObject;

        points.RemoveAt(index);
        curve.RemoveAt(index);

        if (0 == index && curve.IsClosed && curve.Count > 0)
            curve[curve.Count - 1] = curve[0];

        GameObject.DestroyImmediate(point);
    }

    public GameObject EditorInsertPointAt(int index)
    {
        GameObject go = new GameObject("Point", typeof(PTFPoint));
        go.transform.parent = transform;

        int count = points.Count;
        if (0 == count)
        {
            go.transform.position = Vector3.zero;

            points.Add(go.GetComponent<PTFPoint>());

            curve.Add(go.transform.position);
            if (curve.IsClosed)
                curve.Add(go.transform.position);
        }
        else
        {
            if ((index > 0 && index < count) || count < 2)
            {
                if (count < 2)
                {
                    go.transform.position = points[0].transform.position + new Vector3(0.0f, 0.0f, 1.0f);
                }
                else
                {
                    index = index % count;
                    int prevIndex = index - 1;
                    if (prevIndex < 0)
                        prevIndex += count;

                    go.transform.position = (points[prevIndex].transform.position + points[index].transform.position) * 0.5f;
                }
            }
            else if (0 == index)
            {
                go.transform.position = points[0].transform.position * 2.0f - points[1].transform.position;
            }
            else
            {
                go.transform.position = points[count - 1].transform.position * 2.0f - points[count - 2].transform.position;
            }

            points.Insert(index, go.GetComponent<PTFPoint>());

            curve.Insert(index, go.transform.position);
            if (0 == index && curve.IsClosed)
                curve[curve.Count - 1] = go.transform.position;
        }

        return go;
    }

    public void EditorDrawCurve(bool selected)
    {
        if (0 == curve.Count)
        {
            curve.Clear();

            curve.IsClosed = curveIsClosed;
            curve.InterpolationType = curveIntType;

            for (int i = 0; i < points.Count; ++i)
                curve.Add(points[i].transform.position);

            if (curve.IsClosed)
                curve.Add(points[0].transform.position);
        }
        else
        {
            curve.IsClosed = curveIsClosed;
            curve.InterpolationType = curveIntType;

            if (!curve.IsClosed && curve.Count > points.Count)
                curve.RemoveAt(curve.Count - 1);
            if (curve.IsClosed && curve.Count == points.Count)
                curve.Add(points[0].transform.position);

            for (int i = 0; i < points.Count; ++i)
                curve[i] = points[i].transform.position;

            if (curve.IsClosed)
                curve[points.Count] = points[0].transform.position;
        }

        Gizmos.color = selected ? Color.white : Color.gray;

        int numSteps = Mathf.RoundToInt(curve.Length / 0.33f);
        numSteps = Mathf.Min(100, numSteps);

        float step = curve.Length / numSteps, space = 0.0f;
        Curve.Evaluation prevEval = curve.EvaluateAtSpace(space);
        for (int i = 0; i < numSteps; ++i)
        {
            space += step;

            Curve.Evaluation eval = curve.EvaluateAtSpace(space);
            Gizmos.DrawLine(prevEval.f0, eval.f0);

            prevEval = eval;
        }
    }
#endif
    #endregion

    public Curve.Evaluation Step(float space, float smoothMult, float rotThreshold)
    {
        if (space < SBSMath.Epsilon)
            return curve.EvaluateAtSpace(prevSpace);

        prevSpace = Mathf.Clamp(prevSpace + space, 0.0f, curve.Length);

        Curve.Evaluation next = curve.EvaluateAtSpace(prevSpace);
        SBSVector3 tan = next.f1.normalized,
                   axis = SBSVector3.Cross(prevTangent, tan);
        float angle = SBSVector3.Angle(prevTangent, tan);

#if UNITY_FLASH
        SBSMatrix4x4 newFrame = prevFrame.Clone();
#else
        SBSMatrix4x4 newFrame = prevFrame;
#endif
        newFrame.position = next.f0;

        SBSMatrix4x4 invFrame = newFrame.inverseFast;
//      Debug.Log(axis.magnitude + ", " + SBSMath.Abs(SBSVector3.Dot(tan, SBSVector3.up)));
        if (axis.Normalize() > rotThreshold)//1.0e-3f)//2f)
        {
            SBSMatrix4x4 rotation = SBSMatrix4x4.TRS(SBSVector3.zero, new SBSAngleAxis(angle, invFrame.MultiplyVector(axis)), SBSVector3.one);
            newFrame.Append(rotation);
        }
        else if (SBSMath.Abs(SBSVector3.Dot(tan, SBSVector3.up)) < 1.0e-3f)
        {
            // rotate to get up vector right
            SBSVector3 worldAxis = newFrame.MultiplyVector(SBSVector3.right),
                       wantedAxis = worldAxis - SBSVector3.up * SBSVector3.Dot(SBSVector3.up, worldAxis);

            wantedAxis.Normalize();

            axis = SBSVector3.Cross(worldAxis, wantedAxis);
            angle = SBSVector3.Angle(worldAxis, wantedAxis);

            if (axis.Normalize() > 1.0e-2f)
            {
                SBSMatrix4x4 rotation = SBSMatrix4x4.TRS(SBSVector3.zero, new SBSAngleAxis(angle * SBSMath.Min(1.0f, space * smoothMult), invFrame.MultiplyVector(axis)), SBSVector3.one);
                newFrame.Append(rotation);
            }
        }

        prevFrame = newFrame;
        prevTangent = tan;

        SBSMatrix4x4 finalRot = SBSMatrix4x4.TRS(SBSVector3.zero, new SBSAngleAxis(data.Evaluate(prevSpace / curve.Length) * 90.0f, SBSVector3.forward), SBSVector3.one);
        finalFrame = prevFrame * finalRot;

        return next;
    }

    public Curve.Evaluation Step(float space, float smoothMult)
    {
        return this.Step(space, smoothMult, 1.0e-3f);
    }

    public Curve.Evaluation Step(float space)
    {
        return this.Step(space, 5.0f, 1.0e-3f);
    }

    public Curve.Evaluation Reset()
    {
        prevSpace = 0.0f;

        Curve.Evaluation start;
        if (curve.IsClosed)
            start = curve.EvaluateAtSpace(curve.Length);
        else
            start = curve.EvaluateAtSpace(prevSpace);

        SBSVector3 forward = start.f1.normalized,
                   right   = SBSVector3.Cross(SBSVector3.up, start.f1).normalized,
                   up      = SBSVector3.Cross(forward, right);

        prevFrame.xAxis = right;
        prevFrame.yAxis = up;
        prevFrame.zAxis = forward;
        prevFrame.position = start.f0;

        prevTangent = forward;

        SBSMatrix4x4 rotation = SBSMatrix4x4.TRS(SBSVector3.zero, new SBSAngleAxis(data.Evaluate(0.0f) * 90.0f, SBSVector3.forward), SBSVector3.one);
        finalFrame = prevFrame * rotation;

        return start;
    }

    public Curve.Evaluation ResetAtSpace(float spaceStep, float space, float maxError)
    {
        this.Reset();
        float lastStep = this.StepForward(spaceStep, space, maxError);
        return this.Step(lastStep);
    }

    public Curve.Evaluation ResetAtSpace(float spaceStep, float space)
    {
        this.Reset();
        float lastStep = this.StepForward(spaceStep, space);
        return this.Step(lastStep);
    }

    public Curve.Evaluation ResetAtSpaceFast(float space)
	{
        prevSpace = space;

        Curve.Evaluation start = curve.EvaluateAtSpace(space);

        SBSVector3 forward = start.f1.normalized,
                   right   = SBSVector3.Cross(SBSVector3.up, start.f1).normalized,
                   up      = SBSVector3.Cross(forward, right);

        prevFrame.xAxis = right;
        prevFrame.yAxis = up;
        prevFrame.zAxis = forward;
        prevFrame.position = start.f0;

        prevTangent = forward;

        SBSMatrix4x4 rotation = SBSMatrix4x4.TRS(SBSVector3.zero, new SBSAngleAxis(data.Evaluate(space / curve.Length) * 90.0f, SBSVector3.forward), SBSVector3.one);
        finalFrame = prevFrame * rotation;

        return start;
	}

    public Curve.Evaluation MoveToSpace(float spaceStep, float space)
    {
        float lastStep = this.StepForward(spaceStep, SBSMath.Max(0.0f, Math.Min(curve.Length, space) - prevSpace));
        return this.Step(lastStep);
    }

    public void PushState()
    {
        statesStack.Push(new State(prevSpace, prevFrame, prevTangent, finalFrame));
    }

    public void PopState()
    {
        if (0 == statesStack.Count)
            return;

        State prevState = statesStack.Pop();
        prevSpace = prevState.space;
        prevFrame = prevState.frame;
        prevTangent = prevState.tangent;
        finalFrame = prevState.final;
    }

    public float StepForward(float spaceStep, float relSpace, float maxError)
    {
        relSpace = SBSMath.Min(relSpace, curve.Length - prevSpace);
        float totSpace = prevSpace + relSpace;

        int numSteps = Mathf.FloorToInt(relSpace / spaceStep);

        float error = (numSteps - 1) * spaceStep - relSpace,
              errPerStep = error / (float)numSteps,
              maxMult = SBSMath.Min(1.0f, maxError / SBSMath.Abs(errPerStep));//1.0e-2f
//      Debug.Log("error: " + error + ", errPerStep: " + errPerStep + ", maxMult: " + maxMult);
        spaceStep -= (errPerStep * maxMult);

        for (int i = 0; i < numSteps; ++i)
            this.Step(spaceStep);

        return totSpace - prevSpace;
    }

    public float StepForward(float spaceStep, float relSpace)
    {
        return this.StepForward(spaceStep, relSpace, 1.0e-2f);
    }

    public void Extrude(Mesh mesh, SBSMatrix4x4 invWorld, SBSVector3[] points, Vector2[][] uvs, bool closeFigure, float spaceStep, float relSpace, float maxError)
    {
        int pointsCount = points.Length;
        relSpace = SBSMath.Min(relSpace, curve.Length - prevSpace);
        int numSteps = Mathf.CeilToInt(relSpace / spaceStep);

        float error = (numSteps - 1) * spaceStep - relSpace,
              errPerStep = error / (float)numSteps,
              maxMult = SBSMath.Min(1.0f, maxError / SBSMath.Abs(errPerStep));
//      Debug.Log("error: " + error + ", errPerStep: " + errPerStep + ", maxMult: " + maxMult);
        spaceStep -= (errPerStep * maxMult);

        int numFacesPerStep;
        if (closeFigure)
            numFacesPerStep = pointsCount * 2;
        else
            numFacesPerStep = (pointsCount - 1) * 2;

        int numFaces;
        if (curve.IsClosed)
            numFaces = numFacesPerStep * (numSteps + 1);
        else
            numFaces = numFacesPerStep * (numSteps + 0);

        int numVertices = pointsCount * (1 + numSteps),
            vertexIndex = 0;

        int[] tris = new int[numFaces * 3];
        Vector3[] vertexList = new Vector3[numVertices];
        Vector2[] uvList = null;

        bool fillUVs = (uvs != null);
        int numTexCoords = 0,
            uvSwap = 0,
            texCoordIndex = 0;
        if (fillUVs)
        {
            Asserts.Assert(pointsCount == uvs[0].Length);
            Asserts.Assert(pointsCount == uvs[1].Length);

            numTexCoords = numVertices;
            uvList = new Vector2[numTexCoords];
        }

        for (int i = 0; i < pointsCount; ++i)
        {
            vertexList[vertexIndex++] = invWorld.MultiplyPoint3x4(finalFrame.MultiplyPoint3x4(points[i]));
            if (fillUVs)
                uvList[texCoordIndex++] = uvs[uvSwap][i];
        }

        uvSwap = (uvSwap + 1) % 2;

        int baseVertexIndex = 0,
            numFaceIters = (numFacesPerStep / 2) - 1;
        for (int counter = 1; counter <= numSteps; ++counter)
        {
            for (int i = 0; i < pointsCount; ++i)
            {
                vertexList[vertexIndex++] = invWorld.MultiplyPoint3x4(finalFrame.MultiplyPoint3x4(points[i]));
                if (fillUVs)
                    uvList[texCoordIndex++] = uvs[uvSwap][i];
            }

            uvSwap = (uvSwap + 1) % 2;

            int baseFaceIndex = (counter - 1) * numFacesPerStep * 3;
            for (int faceCounter = 0; faceCounter <= numFaceIters; ++faceCounter)
            {
                if (closeFigure && faceCounter == numFaceIters)
                {
                    tris[baseFaceIndex + (faceCounter * 6) + 0] = baseVertexIndex + 0;
                    tris[baseFaceIndex + (faceCounter * 6) + 1] = baseVertexIndex + 1 - pointsCount;
                    tris[baseFaceIndex + (faceCounter * 6) + 2] = baseVertexIndex + 1;

                    tris[baseFaceIndex + (faceCounter * 6) + 3] = baseVertexIndex + 1;
                    tris[baseFaceIndex + (faceCounter * 6) + 4] = baseVertexIndex + pointsCount + 0;
                    tris[baseFaceIndex + (faceCounter * 6) + 5] = baseVertexIndex + 0;
                }
                else
                {
                    tris[baseFaceIndex + (faceCounter * 6) + 0] = baseVertexIndex + 0;
                    tris[baseFaceIndex + (faceCounter * 6) + 1] = baseVertexIndex + 1;
                    tris[baseFaceIndex + (faceCounter * 6) + 2] = baseVertexIndex + pointsCount + 1;

                    tris[baseFaceIndex + (faceCounter * 6) + 3] = baseVertexIndex + pointsCount + 1;
                    tris[baseFaceIndex + (faceCounter * 6) + 4] = baseVertexIndex + pointsCount + 0;
                    tris[baseFaceIndex + (faceCounter * 6) + 5] = baseVertexIndex + 0;

                    ++baseVertexIndex;
                }
            }
            ++baseVertexIndex;

            if (counter < numSteps)
                this.Step(spaceStep);
        }

        if (curve.IsClosed)
        {
            baseVertexIndex = numVertices - pointsCount;

            int baseFaceIndex = numSteps * numFacesPerStep;
            for (int faceCounter = 0; faceCounter <= numFaceIters; ++faceCounter)
            {
                if (closeFigure && faceCounter == numFaceIters)
                {
                    tris[baseFaceIndex + (faceCounter * 6) + 0] = baseVertexIndex + 0;
                    tris[baseFaceIndex + (faceCounter * 6) + 1] = baseVertexIndex + 1 - pointsCount;
                    tris[baseFaceIndex + (faceCounter * 6) + 2] = baseVertexIndex + 1 - numVertices;

                    tris[baseFaceIndex + (faceCounter * 6) + 3] = baseVertexIndex + 1 - numVertices;
                    tris[baseFaceIndex + (faceCounter * 6) + 4] = baseVertexIndex + pointsCount - numVertices;
                    tris[baseFaceIndex + (faceCounter * 6) + 5] = baseVertexIndex + 0;
                }
                else
                {
                    tris[baseFaceIndex + (faceCounter * 6) + 0] = baseVertexIndex + 0;
                    tris[baseFaceIndex + (faceCounter * 6) + 1] = baseVertexIndex + 1;
                    tris[baseFaceIndex + (faceCounter * 6) + 2] = baseVertexIndex + pointsCount + 1 - numVertices;

                    tris[baseFaceIndex + (faceCounter * 6) + 3] = baseVertexIndex + pointsCount + 1 - numVertices;
                    tris[baseFaceIndex + (faceCounter * 6) + 4] = baseVertexIndex + pointsCount + 0 - numVertices;
                    tris[baseFaceIndex + (faceCounter * 6) + 5] = baseVertexIndex + 0;

                    ++baseVertexIndex;
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertexList;
        mesh.uv = uvList;
		mesh.uv2 = null;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
#if UNITY_EDITOR
        mesh.Optimize();
#endif
    }

    public void DuplicateMerge(Mesh mesh, SBSMatrix4x4 invWorld, Mesh baseMesh, SBSVector3[] points, bool orient, bool skipLast, float spaceStep, float relSpace, float maxError)
    {
        relSpace = SBSMath.Min(relSpace, curve.Length - prevSpace);
        int numSteps = Mathf.CeilToInt(relSpace / spaceStep),
            numPoints = points.Length,
            numFaces = 0,
            numVertices = 0,
            numNormals = 0,
            numColors = 0,
            numTexCoords = 0,
            numSubMeshes = baseMesh.subMeshCount;

        float error = (numSteps - 1) * spaceStep - relSpace,
              errPerStep = error / (float)numSteps,
              maxMult = SBSMath.Min(1.0f, maxError / SBSMath.Abs(errPerStep));
//      Debug.Log("error: " + error + ", errPerStep: " + errPerStep + ", maxMult: " + maxMult);
        spaceStep -= (errPerStep * maxMult);

        List<Vector3> vertexList = new List<Vector3>();
        List<Vector3> normalList = new List<Vector3>();
        List<Color> colorList = new List<Color>();
        List<Vector2> uvList = new List<Vector2>();
        List<int> vtxOffsets = new List<int>();
        List<int> faceOffsets = new List<int>();

        if (skipLast)
            --numSteps;
        for (int counter = 1; counter <= numSteps; ++counter)
        {
            for (int pointCounter = 0; pointCounter < numPoints; ++pointCounter)
            {
                vtxOffsets.Add(numVertices);

                SBSVector3 pivot = finalFrame.MultiplyPoint3x4(points[pointCounter]);

                numVertices += baseMesh.vertexCount;
                numNormals += baseMesh.normals.Length;
                numColors += baseMesh.colors.Length;
                numTexCoords += baseMesh.uv.Length;

                foreach (Vector3 v in baseMesh.vertices)
                {
                    SBSVector3 _v = pivot + (orient ? finalFrame.MultiplyVector(v) : (SBSVector3)v);
                    vertexList.Add(invWorld.MultiplyPoint3x4(_v));
                }

                foreach (Vector3 n in baseMesh.normals)
                    normalList.Add(n);
                foreach (Color c in baseMesh.colors)
                    colorList.Add(c);
                foreach (Vector2 uv in baseMesh.uv)
                    uvList.Add(uv);

                for (int i = 0; i < numSubMeshes; ++i)
                {
                    faceOffsets.Add(numFaces);

                    numFaces += (baseMesh.GetTriangles(i).Length / 3);
                }
            }

            if (counter < numSteps)
                this.Step(spaceStep);
        }

        int[] tris = new int[numFaces * 3];

        mesh.Clear();
        mesh.vertices = vertexList.ToArray();

        bool recalcNormals = true;
        if (normalList.Count > 0)
        {
            mesh.normals = normalList.ToArray();
            recalcNormals = false;
        }

        if (colorList.Count > 0)
            mesh.colors = colorList.ToArray();
        if (uvList.Count > 0)
            mesh.uv = uvList.ToArray();

        for (int counter = 1; counter <= numSteps; ++counter)
        {
            for (int pointCounter = 0; pointCounter < numPoints; ++pointCounter)
            {
                int vtxOffset = vtxOffsets[numPoints * (counter - 1) + pointCounter];

                for (int i = 0; i < numSubMeshes; ++i)
                {
                    int faceOffset = faceOffsets[(numPoints * (counter - 1) + pointCounter) * numSubMeshes + i];
                    int[] baseTris = baseMesh.GetTriangles(i);

                    numFaces = baseTris.Length / 3;
                    int faceBaseIndex = 0;
                    for (int k = 0; k < numFaces; ++k)
                    {
                        int index = faceOffset + faceBaseIndex + k;

                        tris[(index * 3) + 0] = vtxOffset + baseTris[(k * 3) + 0];
                        tris[(index * 3) + 1] = vtxOffset + baseTris[(k * 3) + 1];
                        tris[(index * 3) + 2] = vtxOffset + baseTris[(k * 3) + 2];
                    }

                    faceBaseIndex += numFaces;
                }
            }
        }

        mesh.triangles = tris;
		mesh.uv2 = null;
        mesh.RecalculateBounds();
        if (recalcNormals)
            mesh.RecalculateNormals();
#if UNITY_EDITOR
        mesh.Optimize();
#endif
    }
    #endregion
}
