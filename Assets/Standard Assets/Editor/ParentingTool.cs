using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ParentingTool : EditorWindow
{
    static readonly Vector3 worldOrigin = Vector3.zero;

    [MenuItem("SBS/ParentingTool/Parent Center %g")]
    public static void ParentCenter()
    {
        if (NoObjectsAreSelected())
            return;

        GameObject parent = new GameObject();
        ParentSelectionToGameObjectInPosition(parent, GetSelectionBoundCenterPoint());
    }

    [MenuItem("SBS/ParentingTool/Parent First %#g")]
    public static void ParentFirst()
    {
        if (NoObjectsAreSelected())
            return;

        GameObject parent = new GameObject();
        ParentSelectionToGameObjectInPosition(parent, Selection.activeGameObject.transform.position);
    }

    [MenuItem("SBS/ParentingTool/Parent Origin %&g")]
    public static void ParentOrigin()
    {
        if (NoObjectsAreSelected())
            return;

        GameObject parent = new GameObject();
        ParentSelectionToGameObjectInPosition(parent, worldOrigin);
    }

    static bool NoObjectsAreSelected()
    {
        return Selection.gameObjects.Length == 0;
    }
    
    static Vector3 GetSelectionBoundCenterPoint()
    {
        List<float> minXcorners = new List<float>();
        List<float> minYcorners = new List<float>();
        List<float> minZcorners = new List<float>();
        List<float> maxXcorners = new List<float>();
        List<float> maxYcorners = new List<float>();
        List<float> maxZcorners = new List<float>();

        int boundsCounter = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<MeshFilter>() != null)
            {
                Vector3 minCorner = obj.GetComponent<MeshFilter>().sharedMesh.bounds.min;
                Vector3 maxCorner = obj.GetComponent<MeshFilter>().sharedMesh.bounds.max;

                Vector3[] boundingBoxCorners = new Vector3[8];
                boundingBoxCorners[0] = new Vector3(minCorner.x, minCorner.y, minCorner.z);
                boundingBoxCorners[1] = new Vector3(maxCorner.x, minCorner.y, minCorner.z);
                boundingBoxCorners[2] = new Vector3(maxCorner.x, minCorner.y, maxCorner.z);
                boundingBoxCorners[3] = new Vector3(minCorner.x, minCorner.y, maxCorner.z);
                boundingBoxCorners[4] = new Vector3(minCorner.x, maxCorner.y, maxCorner.z);
                boundingBoxCorners[5] = new Vector3(minCorner.x, maxCorner.y, minCorner.z);
                boundingBoxCorners[6] = new Vector3(maxCorner.x, maxCorner.y, minCorner.z);
                boundingBoxCorners[7] = new Vector3(maxCorner.x, maxCorner.y, maxCorner.z);

                for (int i = 0; i < boundingBoxCorners.Length; i++)
                    boundingBoxCorners[i] = obj.transform.TransformPoint(boundingBoxCorners[i]);

                float[] cornersXcoords = new float[boundingBoxCorners.Length];
                float[] cornersYcoords = new float[boundingBoxCorners.Length];
                float[] cornersZcoords = new float[boundingBoxCorners.Length];

                for (int i = 0; i < boundingBoxCorners.Length; i++)
                {
                    cornersXcoords[i] = boundingBoxCorners[i].x;
                    cornersYcoords[i] = boundingBoxCorners[i].y;
                    cornersZcoords[i] = boundingBoxCorners[i].z;
                }

                float minX = Mathf.Min(cornersXcoords),
                      minY = Mathf.Min(cornersYcoords),
                      minZ = Mathf.Min(cornersZcoords),
                      maxX = Mathf.Max(cornersXcoords),
                      maxY = Mathf.Max(cornersYcoords),
                      maxZ = Mathf.Max(cornersZcoords);

                minXcorners.Add(minX);
                minYcorners.Add(minY);
                minZcorners.Add(minZ);
                maxXcorners.Add(maxX);
                maxYcorners.Add(maxY);
                maxZcorners.Add(maxZ);
            }
            else if (obj.GetComponent<Renderer>() != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();

                minXcorners.Add(renderer.bounds.min.x);
                minYcorners.Add(renderer.bounds.min.y);
                minZcorners.Add(renderer.bounds.min.z);
                maxXcorners.Add(renderer.bounds.max.x);
                maxYcorners.Add(renderer.bounds.max.y);
                maxZcorners.Add(renderer.bounds.max.z);

            }
            else if (obj.GetComponent<Collider>() != null)
            {
                Collider collider = obj.GetComponent<Collider>();

                minXcorners.Add(collider.bounds.min.x);
                minYcorners.Add(collider.bounds.min.y);
                minZcorners.Add(collider.bounds.min.z);
                maxXcorners.Add(collider.bounds.max.x);
                maxYcorners.Add(collider.bounds.max.y);
                maxZcorners.Add(collider.bounds.max.z);
            }
            else
                continue;

            boundsCounter++;
        }
        
        if (boundsCounter == 0)
            return GetSelectionTransformCenterPoint();
        
        Vector3 minBoundPoint = Vector3.zero;
        Vector3 maxBoundPoint = Vector3.zero;
        minBoundPoint.x = Mathf.Min(minXcorners.ToArray());
        minBoundPoint.y = Mathf.Min(minYcorners.ToArray());
        minBoundPoint.z = Mathf.Min(minZcorners.ToArray());
        maxBoundPoint.x = Mathf.Max(maxXcorners.ToArray());
        maxBoundPoint.y = Mathf.Max(maxYcorners.ToArray());
        maxBoundPoint.z = Mathf.Max(maxZcorners.ToArray());

        return (minBoundPoint + maxBoundPoint) / 2.0f;
    }

    static Vector3 GetSelectionTransformCenterPoint()
    {
        Vector3 boundCenter = Vector3.zero;
        foreach (GameObject obj in Selection.gameObjects)
            boundCenter += obj.transform.position;
        boundCenter /= Selection.gameObjects.Length;

        return boundCenter;
    }
    
    static void ParentSelectionToGameObjectInPosition(GameObject parent, Vector3 parentPosition)
    {
        List<GameObject> rootGameObjects = GetParentableGameObjectsInSelection();

        parent.transform.position = parentPosition;

        foreach (GameObject go in rootGameObjects)
        {
            go.transform.parent = parent.transform;
        }
    }

    static List<GameObject> GetParentableGameObjectsInSelection()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        List<GameObject> rootGameObjects = new List<GameObject>(selectedObjects);

        for (int i = rootGameObjects.Count - 1; i >= 0; --i)
        {
            Transform node = rootGameObjects[i].transform.parent;
            while (node != null)
            {
                int index = rootGameObjects.IndexOf(node.gameObject);
                if (index > -1)
                {
                    rootGameObjects.RemoveAt(i);
                    break;
                }
                node = node.parent;
            }
        }

        return rootGameObjects;
    }
}