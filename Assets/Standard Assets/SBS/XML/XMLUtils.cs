using System;
using System.Collections.Generic;
using SBS.Core;
using SBS.Math;
using System.Globalization;

namespace SBS.XML
{
	public class XMLUtils
	{
        public static int ParseInt(XMLNode element)
        {
            if (element == null)
            {
                Asserts.Assert(false);//, "Invalid node name: " + element.tagName + " in: " + element.parentNode.tagName);
                return 0;
            }

            return int.Parse(element.innerText, CultureInfo.InvariantCulture);
        }

        public static float ParseFloat(XMLNode element)
        {
            if (element == null)
            {
                Asserts.Assert(false);//, "Invalid node name: " + element.Name + " in: " + element.ParentNode.Name);
                return float.NaN;
            }

            return float.Parse(element.innerText, CultureInfo.InvariantCulture);
        }

        public static SBSVector3 ParseVector3(XMLNode element)
        {
            if (element == null)
            {
                Asserts.Assert(false);//, "Invalid node name: " + element.Name + " in: " + element.ParentNode.Name);
                return SBSVector3.zero;
            }
            char[] delimeters = { ',' };
            string[] parts = element.innerText.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                Asserts.Assert(false);//, "Invalid Vector3 param length node: " + element.Name + " in: " + element.ParentNode.Name);

            float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float z = float.Parse(parts[2], CultureInfo.InvariantCulture);

            return new SBSVector3(x, y, z);
        }

        public static float[] ParseFloatArray(XMLNode element)
        {
            if (element == null)
            {
                Asserts.Assert(false);//, "Invalid node name: " + element.Name + " in: " + element.ParentNode.Name);
                return null;
            }
            char[] delimeters = { ',' };
            string[] parts = element.innerText.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);

            float[] result = new float[parts.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = float.Parse(parts[i], CultureInfo.InvariantCulture);

            return result;
        }
    }
}
