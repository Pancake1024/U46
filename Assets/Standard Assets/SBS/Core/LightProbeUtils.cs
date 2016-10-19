using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Core
{
	public class LightProbeUtils
	{
        public struct ExtractedDirLights
        {
            public Vector3[] Directions;
            public float[] Intensities;
            public float[] Ambients;

            public ExtractedDirLights(int probesCount)
            {
                Directions = new Vector3[probesCount * 3];
                Intensities = new float[probesCount * 3];
                Ambients = new float[probesCount * 3];
            }
        }

        public static ExtractedDirLights ExtractDirectionalLights(Tuple<float[], Vector3[]> probesCoeff)
        {
            int count = probesCoeff.Second.Length,
                i     = 0;

            float[] coefficients = probesCoeff.First;
            ExtractedDirLights dirLights = new ExtractedDirLights(count);

            Tuple<float[], Vector3[]> constDirLight = AllocateProbes(1);

            float kIntensityMult = 0.87333756747261554501026723318968f; // 867/(316*PI)
            float kHalfSqrtPI = 0.88622692545275801364908374167057f; // 0.5*sqrt(kPI)
            float kConst = 0.94117647058823529411764705882353f;// 16/17
            float kNorm = 0.25f;// 1.0f / (kConst * Mathf.PI);

            for (; i < count; ++i)
            {
                int index     = i * 27,
                    component = 0;
                for (; component < 3; ++component)
                {
                    Vector3 dir = new Vector3(
                        -coefficients[index + 3 * 3 + component],
                        -coefficients[index + 1 * 3 + component],
                         coefficients[index + 2 * 3 + component]);
                    dir.Normalize();

                    dirLights.Directions[(i * 3) + component] = dir;

                    AddDirectionalLight(new Color(0 == component ? 1.0f : 0.0f, 1 == component ? 1.0f : 0.0f, 2 == component ? 1.0f : 0.0f), dir, kNorm * 0.5f, constDirLight, 0);

                    float dot = 0.0f;
                    for (int j = 1; j < 9; ++j)
                        dot += (coefficients[index + (j * 3) + component] * constDirLight.First[(j * 3) + component]);

                    ResetProbe(constDirLight, 0);

                    float c = kIntensityMult * dot;
                    dirLights.Intensities[(i * 3) + component] = c;
                    dirLights.Ambients[(i * 3) + component] = (coefficients[index + component] - c * kConst * kHalfSqrtPI) * kHalfSqrtPI;
                }
            }

            return dirLights;
        }

        public static Tuple<float[], Vector3[]> AllocateProbes(int probesCount)
        {
            return new Tuple<float[], Vector3[]>(new float[probesCount * 27], new Vector3[probesCount]);
        }

        public static void ResetProbe(Tuple<float[], Vector3[]> probesCoeff, int index)
        {
            if (index < 0 || index >= probesCoeff.Second.Length)
                return;

            float[] coefficients = probesCoeff.First;
            index *= 27;

            for (int i = 0; i < 27; ++i)
                coefficients[index + i] = 0.0f;
        }

        public static void ResetProbes(Tuple<float[], Vector3[]> probesCoeff)
        {
            int count = probesCoeff.Second.Length;
            for (int i = 0; i < count; ++i)
                ResetProbe(probesCoeff, i);
        }

        public static void AddAmbientLight(Color color, Tuple<float[], Vector3[]> probesCoeff, int index)
        {
            if (index < 0 || index >= probesCoeff.Second.Length)
                return;

            float[] coefficients = probesCoeff.First;
            index *= 27;

            float k2SqrtPI = 3.54490770181103205459633496668229f; // 2*sqrt(kPI)
            coefficients[index + 0] += color.r * k2SqrtPI;
            coefficients[index + 1] += color.g * k2SqrtPI;
            coefficients[index + 2] += color.b * k2SqrtPI;
        }

        public static void AddDirectionalLight(Color color, Vector3 direction, float intensity, Tuple<float[], Vector3[]> probesCoeff, int index)
        {
            if (index < 0 || index >= probesCoeff.Second.Length)
                return;

            float[] coefficients = probesCoeff.First;
            index *= 27;

            // Read more about Spherical Harmonics in Peter Sloan's "Stupid Spherical Harmonics Tricks"
            float kInv2SqrtPI = 0.28209479177387814347403972578039f; // 1 / (2*sqrt(kPI))
            float kSqrt3Div2SqrtPI = 0.48860251190291992158638462283835f; // sqrt(3) / (2*sqrt(kPI))
            float kSqrt15Div2SqrtPI = 1.0925484305920790705433857058027f; // sqrt(15) / (2*sqrt(kPI))
            float k3Sqrt5Div4SqrtPI = 0.94617469575756001809268107088713f; // 3 * sqrtf(5) / (4*sqrt(kPI))
            float kSqrt15Div4SqrtPI = 0.54627421529603953527169285290135f; // sqrt(15) / (4*sqrt(kPI))
            float kOneThird = 0.3333333333333333333333f; // 1.0/3.0

            float[] dirFactors = new float[9];
            dirFactors[0] = kInv2SqrtPI;
            dirFactors[1] = -direction.y * kSqrt3Div2SqrtPI;
            dirFactors[2] = direction.z * kSqrt3Div2SqrtPI;
            dirFactors[3] = -direction.x * kSqrt3Div2SqrtPI;
            dirFactors[4] = direction.x * direction.y * kSqrt15Div2SqrtPI;
            dirFactors[5] = -direction.y * direction.z * kSqrt15Div2SqrtPI;
            dirFactors[6] = (direction.z * direction.z - kOneThird) * k3Sqrt5Div4SqrtPI;
            dirFactors[7] = -direction.x * direction.z * kSqrt15Div2SqrtPI;
            dirFactors[8] = (direction.x * direction.x - direction.y * direction.y) * kSqrt15Div4SqrtPI;

            float kNormalization = 2.9567930857315701067858823529412f; // 16*kPI/17

            // Unity doubles the light intensity internally
            intensity *= 2.0f;

            float rscale = color.r * intensity * kNormalization;
            float gscale = color.g * intensity * kNormalization;
            float bscale = color.b * intensity * kNormalization;
            for (int i = 0; i < 9; ++i)
            {
                float c = dirFactors[i];
                coefficients[index + 3 * i + 0] += c * rscale;
                coefficients[index + 3 * i + 1] += c * gscale;
                coefficients[index + 3 * i + 2] += c * bscale;
            }
        }

        public static void AddPointLight(Color color, Vector3 position, float range, float intensity, Tuple<float[], Vector3[]> probesCoeff, int index)
        {
            if (index < 0 || index >= probesCoeff.Second.Length)
                return;

            Vector3 probeToLight = position - probesCoeff.Second[index];
            float attenuation = 1.0f / (1.0f + (25.0f * probeToLight.sqrMagnitude / (range * range)));
            AddDirectionalLight(color, probeToLight.normalized, intensity * attenuation, probesCoeff, index);
        }
    }
}
