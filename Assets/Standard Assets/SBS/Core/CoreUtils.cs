using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Core
{
    public class CoreUtils
    {
        public static string forcedDataPath = null;

        public static void Shuffle<T>(T[] a, System.Random rng)
        {
            byte[] b = new byte[a.Length];
            rng.NextBytes(b);
            Array.Sort(b, a);
        }
        
        public static string GetFullPath(string localFilename)
        {
            string dataPath;

            if (null == forcedDataPath)
            {
                if (Application.isWebPlayer)
                    dataPath = Application.dataPath;
                else
                    dataPath = "file://" + Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            }
            else
                dataPath = forcedDataPath;

            return dataPath + "/" + localFilename;
        }

        public static string GetFullPath(string localFilename, bool noCache)
        {
            string dataPath = GetFullPath(localFilename);

            if (noCache && !Application.isEditor)
                dataPath += "?nocache=" + (int)(Time.realtimeSinceStartup * 1000.0f);

            return dataPath;
        }

        public static Type GetType(string typeName)
        {
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            Type type = Type.GetType(typeName);

            // If it worked, then we're done here
            if (type != null)
                return type;

            // If the TypeName is a full name, then we can try loading the defining assembly directly
            if (typeName.Contains("."))
            {
                // Get the name of the assembly (Assumption is that we are using 
                // fully-qualified type names)
                string assemblyName = typeName.Substring(0, typeName.IndexOf('.'));

                // Attempt to load the indicated Assembly
                Assembly assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                    return null;

                // Ask that assembly to return the proper Type
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            Assembly[] referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in referencedAssemblies)
            {
                // See if that assembly defines the named type
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            // The type just couldn't be found...
            return null;
        }
    }
}
