using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Reflection;
using System.Xml;
#endif
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using SBS.Core;

namespace SBS.Level
{
	public class LevelPreprocessor
	{
#if UNITY_EDITOR
        #region Protected structs
        protected struct ComponentDesc
        {
            public Type type;
            public List<KeyValuePair<PropertyInfo, object>> properties;
            public List<KeyValuePair<FieldInfo, object>> fields;
        };

        protected struct CategoryDesc
        {
            public string name;
            public bool movable;
            public int defaultLayersMask;
            public List<ComponentDesc> components;
        };
        #endregion

        #region Public structs
        public struct MapUnityLayerDesc
        {
            public string unityLayerName;
            public int layersMask;
        };
        #endregion

        #region Public members
        public GameObject levelRoot;
        public TextAsset levelXml;
        #endregion

        #region Protected members
        protected List<KeyValuePair<string, CategoryDesc>> prefixToCatPairs = null;
        protected List<MapUnityLayerDesc> unityLayersMaps = null;
        protected string[] categories = null;
        protected string[] layersName = null;
        protected Layers layers = null;
        protected Bounds worldBounds;
        protected MeshFilter[] meshFilters = null;
        #endregion

        #region Public properties
        public List<MapUnityLayerDesc> UnityLayersMaps
        {
            get
            {
                return unityLayersMaps;
            }
        }

        public string[] Categories
        {
            get
            {
                return categories;
            }
        }

        public string[] LayersName
        {
            get
            {
                return layersName;
            }
        }

        public Layers Layers
        {
            get
            {
                return layers;
            }
        }

        public Bounds WorldBounds
        {
            get
            {
                return worldBounds;
            }
        }
        #endregion

        #region Protected methods
        protected int pairsComparer(KeyValuePair<string, CategoryDesc> a, KeyValuePair<string, CategoryDesc> b)
        {
            return -(null == a.Key ? 0 : a.Key.Length).CompareTo(null == b.Key ? 0 : b.Key.Length);
        }

        protected Type GetComponentType(string name)
        {
            if ("MeshCollider" == name)
                return typeof(MeshCollider);
            if ("Renderer" == name)
                return typeof(Renderer);
            if ("RigidBody" == name)
                return typeof(Rigidbody);
            if ("BoxCollider" == name)
                return typeof(BoxCollider);
            Assembly assembly = Assembly.GetAssembly(typeof(Component));
            Type t = assembly.GetType(assembly.GetName().Name + "." + name);
            if (t != null)
                return t;
            else
                return Type.GetType(name);
        }

        protected void RegisterLayers(string str)
        {
            string[] _layers = str.Split(';');
            foreach (string layer in _layers)
            {
                if (!layers.Exist(layer))
                    layers.Register(layer);
            }
        }

        protected object ParseAttribute(string value, Type type)
        {
            object obj;

            if (typeof(bool) == type)
            {
                obj = ("true" == value.ToLowerInvariant());
            }
            else if (typeof(int) == type)
            {
                obj = Int32.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (typeof(float) == type)
            {
                obj = Single.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (typeof(string) == type)
            {
                obj = value;
            }
            else if (typeof(Vector3) == type)
            {
                string[] xyz = value.Split(',');

                obj = new Vector3(
                    Single.Parse(xyz[0], CultureInfo.InvariantCulture),
                    Single.Parse(xyz[1], CultureInfo.InvariantCulture),
                    Single.Parse(xyz[2], CultureInfo.InvariantCulture)
                );
            }
            else if (typeof(PhysicMaterial) == type)
            {
                obj = Resources.Load(value, type);
            }
            else
            {
                Asserts.Assert(false, "Invalid type " + type.ToString());
                obj = null;
            }

            return obj;
        }

        protected void ParseCategory(XmlNode node, List<string> catNames)
        {
            CategoryDesc cat = new CategoryDesc();

            cat.name = node.Attributes["name"].Value;
            cat.components = new List<ComponentDesc>();

            XmlAttribute defaultLayersAttr = node.Attributes["defaultLayers"];
            if (defaultLayersAttr != null)
            {
                this.RegisterLayers(defaultLayersAttr.Value);

                cat.defaultLayersMask = layers.StringToMask(defaultLayersAttr.Value);
            }
            else
            {
                cat.defaultLayersMask = 0;
            }

            XmlAttribute movable = node.Attributes["movable"];
            if (movable != null)
                cat.movable = ("true" == movable.Value.ToLowerInvariant());
            else
                cat.movable = false;

            foreach (XmlNode child in node.ChildNodes)
            {
                if ("Component" == child.Name)
                {
                    ComponentDesc desc = new ComponentDesc();

                    desc.type = this.GetComponentType(child.Attributes["name"].Value);
                    desc.properties = new List<KeyValuePair<PropertyInfo, object>>();
                    desc.fields = new List<KeyValuePair<FieldInfo, object>>();

                    Asserts.Assert(desc.type != null, "Cannot find class \"" + child.Attributes["name"].Value + "\"");
                    PropertyInfo[] propInfoArray = desc.type.GetProperties();
                    FieldInfo[] fieldsInfoArray = desc.type.GetFields();

                    foreach (XmlAttribute attr in child.Attributes)
                    {
                        if ("name" == attr.Name) continue;

                        bool found = false;
                        foreach (PropertyInfo info in propInfoArray)
                        {
                            if (info.Name == attr.Name && info.CanWrite)
                            {
                                KeyValuePair<PropertyInfo, object> prop = new KeyValuePair<PropertyInfo, object>(info, this.ParseAttribute(attr.Value, info.PropertyType));
                                desc.properties.Add(prop);
                                found = true;

                                break;
                            }
                        }

                        if (!found)
                        {
                            foreach (FieldInfo info in fieldsInfoArray)
                            {
                                if (info.Name == attr.Name && 0 == (info.Attributes & FieldAttributes.Static) && 0 == (info.Attributes & FieldAttributes.InitOnly))
                                {
                                    KeyValuePair<FieldInfo, object> field = new KeyValuePair<FieldInfo, object>(info, this.ParseAttribute(attr.Value, info.FieldType));
                                    desc.fields.Add(field);
                                    found = true;
                                }
                            }
                        }

                        if (!found)
                            Debug.LogWarning("Attribute " + attr.Name + " invalid");
                    }

                    cat.components.Add(desc);
                }
            }

            string[] prefixes = node.Attributes["prefixes"].Value.Split(';');
            foreach (string prefix in prefixes)
                prefixToCatPairs.Add(new KeyValuePair<string, CategoryDesc>(prefix, cat));
            prefixToCatPairs.Sort(pairsComparer);

            catNames.Add(cat.name);
        }

        protected void ParseMap(XmlNode node)
        {
            MapUnityLayerDesc desc = new MapUnityLayerDesc();

            desc.unityLayerName = node.Attributes["name"].Value;

            XmlAttribute maskAttr = node.Attributes["mask"];
            bool inv = false;
            if (null == maskAttr)
            {
                maskAttr = node.Attributes["invmask"];
                inv = true;
            }

            desc.layersMask = layers.StringToMask(maskAttr.Value);
            if (inv)
                desc.layersMask = ~desc.layersMask;

            unityLayersMaps.Add(desc);
        }

        protected void ApplyCategory(GameObject go, CategoryDesc cat)
        {
            foreach (ComponentDesc desc in cat.components)
            {
                Component component = go.GetComponent(desc.type);
                if (null == component)
                    component = go.AddComponent(desc.type);

                foreach (KeyValuePair<PropertyInfo, object> prop in desc.properties)
                    prop.Key.SetValue(component, prop.Value, null);

                foreach (KeyValuePair<FieldInfo, object> field in desc.fields)
                    field.Key.SetValue(component, field.Value);
            }

            LevelObject levelObj = go.GetComponent<MovableLevelObject>();
            while (levelObj != null)
            {
                GameObject.DestroyImmediate(levelObj);
                levelObj = go.GetComponent<MovableLevelObject>();
            }
            levelObj = go.GetComponent<LevelObject>();
            while (levelObj != null)
            {
                GameObject.DestroyImmediate(levelObj);
                levelObj = go.GetComponent<LevelObject>();
            }
            if (cat.movable)
                levelObj = go.AddComponent<MovableLevelObject>();
            else
                levelObj = go.AddComponent<LevelObject>();

            levelObj.Category = cat.name;
            levelObj.LayersMask = cat.defaultLayersMask;
        }

        protected void Preprocess()
        {
            // setup game objects
            foreach (MeshFilter f in meshFilters)
            {
                string name = f.gameObject.name;

                foreach (KeyValuePair<string, CategoryDesc> item in prefixToCatPairs)
                {
                    if (name.StartsWith(item.Key))
                    {
                        this.ApplyCategory(f.gameObject, item.Value);
                        break;
                    }
                }
            }
        }
        #endregion

        #region Public methods
        public void Run()
        {
            layers = new Layers();

            prefixToCatPairs = new List<KeyValuePair<string, CategoryDesc>>();
            unityLayersMaps = new List<MapUnityLayerDesc>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(levelXml.text);

            XmlNode root = doc.ChildNodes[1];
            Asserts.Assert("SetupLevel" == root.Name, "Invalid LevelRoot xml");

            List<string> catNames = new List<string>();
            foreach (XmlNode child in root.ChildNodes)
            {
                if ("Category" == child.Name)
                {
                    this.ParseCategory(child, catNames);
                }
                else if ("MapUnityLayer" == child.Name)
                {
                    this.ParseMap(child);
                }
                else if (XmlNodeType.Comment == child.NodeType)
                {
                    // do nothing
                }
                else
                {
                    Asserts.Assert(false, "Invalid LevelRoot xml tag \"" + child.Name + "\"");
                }
            }

            categories = new string[catNames.Count];
            int i = 0;
            foreach (string name in catNames)
                categories[i++] = name;

            layersName = new string[layers.Count];
            for (i = 0; i < layers.Count; ++i)
                layersName[i] = layers[i];

            meshFilters = levelRoot.GetComponentsInChildren<MeshFilter>();

            Renderer rendererRef = null;
            i = 0;
            do
            {
                rendererRef = meshFilters[i++].GetComponent<Renderer>();
            } while (null == rendererRef);
            worldBounds = new Bounds(rendererRef.bounds.center, rendererRef.bounds.size);
            foreach (MeshFilter f in meshFilters)
            {
                rendererRef = f.GetComponent<Renderer>();
                if (rendererRef != null)
                {
                    worldBounds.Encapsulate(rendererRef.bounds);
                }
            }

            this.Preprocess();

            // purge level setup data
            prefixToCatPairs.Clear();
            prefixToCatPairs = null;

            meshFilters = null;
        }
        #endregion
#endif
    }
}
