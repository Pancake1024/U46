using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

namespace SBS.Math
{
    public class SBSClip
	{
#if !UNITY_FLASH
        public struct Vertex
        {
            int[] vertexIndices;
            float[] ratios;

            public Vertex(int index)
            {
                vertexIndices = new int[1] { index };
                ratios = new float[1] { 1.0f };
            }

            public Vertex(int index0, int index1, float r)
            {
                vertexIndices = new int[2] { index0, index1 };
                ratios = new float[2] { 1.0f - r, r };
            }

            public T Sample<T, M>(T[] buffer) where M : IMix<T>, new()
            {
                return SBSMix<T, M>.Mix(buffer, vertexIndices, ratios);
            }

            static public Vertex Merge(Vertex p, Vertex q, float ratio)
            {
                int pc = p.ratios.Length,
                    qc = q.ratios.Length,
                    tc = pc + qc;
                if (1 == pc & 1 == qc)
                {
                    return new Vertex(p.vertexIndices[0], q.vertexIndices[0], ratio);
                }
                else
                {
                    float invRatio = 1.0f - ratio;

                    Vertex v = new Vertex();
                    v.vertexIndices = new int[tc];
                    v.ratios = new float[tc];

                    for (int i = 0; i < tc; ++i)
                    {
                        if (i < pc)
                        {
                            v.vertexIndices[i] = p.vertexIndices[i];
                            v.ratios[i] = p.ratios[i] * invRatio;
                        }
                        else
                        {
                            v.vertexIndices[i] = q.vertexIndices[i - pc];
                            v.ratios[i] = q.ratios[i - pc] * ratio;
                        }
                    }

                    return v;
                }
            }
        }

        static public bool ClipPolygon(SBSVector3[] vertices, SBSPlane[] clipPlanes, List<Vertex> output, int offset, int count)
        {
            List<Vertex> points = new List<Vertex>(),
                         oldPoints = null;
            for (int i = 0; i < count; ++i)
                points.Add(new Vertex(offset + i));

            float[] vd = new float[count * 2];
            bool[] vIn = new bool[count * 2];

            foreach (SBSPlane plane in clipPlanes)
            {
                count = points.Count;

                int inCount = 0, k = 0;
                for (; k < count; ++k)
                {
                    vd[k]  = plane.GetDistanceToPoint(points[k].Sample<SBSVector3, SBSVector3Mix>(vertices));
                    vIn[k] = false;

                    if (vd[k] >= 0.0f)
                    {
                        vIn[k] = true;
                        ++inCount;
                    }
                }

                if (0 == inCount)
                {
                    points.Clear();
                    break; // all outside, clear & exit
                }

                if (count == inCount)
                    continue; // all inside, nothing to clip

                oldPoints = new List<Vertex>(points);
                points.Clear();

                for (k = 0; k < count; ++k)
                {
                    int l = (k + 1) % count;

                    float pd = vd[k],
                          qd = vd[l];
                    bool pIn = vIn[k],
                         qIn = vIn[l];
                    Vertex p = oldPoints[k],
                           q = oldPoints[l];

                    if (pIn)
                    {
                        points.Add(p);
                        if (!qIn)
                            points.Add(Vertex.Merge(p, q, pd / (pd - qd)));
                    }
                    else if (qIn)
                    {
                        points.Add(Vertex.Merge(p, q, -pd / (qd - pd)));
                    }
                }
            }

            foreach (Vertex v in points)
                output.Add(v);
            return points.Count > 0;
        }

        static public bool ClipPolygon(SBSVector3[] vertices, SBSPlane[] clipPlanes, List<Vertex> output)
        {
            return ClipPolygon(vertices, clipPlanes, output, 0, vertices.Length);
        }
#endif
	}
}
