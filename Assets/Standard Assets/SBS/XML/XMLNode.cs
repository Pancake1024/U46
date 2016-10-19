using System;
using System.Collections;
using System.Collections.Generic;
using SBS.Math;
using System.Globalization;

namespace SBS.XML
{
	public class XMLNode
	{
        public String tagName;
        public XMLNode parentNode;
        public ArrayList children;
        public Dictionary<String, String> attributes;
        public String innerText;
        public String cdata;

        public XMLNode this[string childName]
        {
            get
            {
                foreach (XMLNode node in children)
                {
                    if (node.tagName == childName)
                        return node;
                }

                return null;
            }
        }

        public bool HasAttribute(string name)
        {
            return attributes.ContainsKey(name);
        }

        public string GetAttributeAsString(string name, string defaultValue)
        {
            string ret;
            if (attributes.TryGetValue(name, out ret))
                return ret;
            else
                return defaultValue;
        }

        public string GetAttributeAsString(string name)
        {
            return this.GetAttributeAsString(name, null);
        }

        public float GetAttributeAsFloat(string name, float defaultValue)
        {
            string ret;
            if (attributes.TryGetValue(name, out ret))
                return float.Parse(ret, CultureInfo.InvariantCulture);
            else
                return defaultValue;
        }

        public float GetAttributeAsFloat(string name)
        {
            return this.GetAttributeAsFloat(name, float.NaN);
        }

        public int GetAttributeAsInt(string name, int defaultValue)
        {
            string ret;
            if (attributes.TryGetValue(name, out ret))
                return int.Parse(ret, CultureInfo.InvariantCulture);
            else
                return defaultValue;
        }

        public int GetAttributeAsInt(string name)
        {
            return this.GetAttributeAsInt(name, 0);
        }

        public bool GetAttributeAsBool(string name, bool defaultValue)
        {
            string ret;
            if (attributes.TryGetValue(name, out ret))
                return ret.ToLower().Equals("true") || ret.Equals("1");
            else
                return defaultValue;
        }

        public bool GetAttributeAsBool(string name)
        {
            return this.GetAttributeAsBool(name, false);
        }

        public SBSVector3 GetAttributeAsVector3(string name, SBSVector3 defaultValue)
        {
            string ret;
            if (attributes.TryGetValue(name, out ret))
            {
                char[] delimeters = { ',' };
                string[] parts = ret.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);
                int c = parts.Length;

                float x = c > 0 ? float.Parse(parts[0], CultureInfo.InvariantCulture) : 0.0f,
                      y = c > 1 ? float.Parse(parts[1], CultureInfo.InvariantCulture) : 0.0f,
                      z = c > 2 ? float.Parse(parts[2], CultureInfo.InvariantCulture) : 0.0f;

                return new SBSVector3(x, y, z);
            }
            else
                return defaultValue;
        }

        public SBSVector3 GetAttributeAsVector3(string name)
        {
            return this.GetAttributeAsVector3(name, SBSVector3.zero);
        }

        public XMLNode()
        {
            tagName = "NONE";
            parentNode = null;
            children = new ArrayList();
            attributes = new Dictionary<String, String>();
            innerText = "";
            cdata = "";
        }
    }
}
