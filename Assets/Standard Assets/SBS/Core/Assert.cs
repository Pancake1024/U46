using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Core
{
	public class Asserts
	{
        public static void Assert(bool value)
        {
            if (UnityEngine.Debug.isDebugBuild && !value)
            {
#if UNITY_FLASH && !UNITY_EDITOR
                object e = ActionScript.Expression<object>("new Error('assertion failed')");
                ActionScript.Statement("trace({0}.getStackTrace())", e);
#else
                StackTrace st = new StackTrace(true);
                string filename = "empty";
                int line = -1;
                if (st.FrameCount > 1) {
                    filename = st.GetFrame(1).GetFileName();
                    line     = st.GetFrame(1).GetFileLineNumber();
                }
                UnityEngine.Debug.LogError("assertion failed @ " + filename + " (" + line.ToString() + ")");
                UnityEngine.Debug.Break();
#endif
            }
        }

        public static void Assert(bool value, string msg)
        {
            if (UnityEngine.Debug.isDebugBuild && !value)
            {
#if UNITY_FLASH && !UNITY_EDITOR
                object e = ActionScript.Expression<object>("new Error('assertion failed: ' + {0})", msg);
                ActionScript.Statement("trace({0}.getStackTrace())", e);
#else
                StackTrace st = new StackTrace(true);
                string filename = "empty";
                int line = -1;
                if (st.FrameCount > 1)
                {
                    filename = st.GetFrame(1).GetFileName();
                    line = st.GetFrame(1).GetFileLineNumber();
                }
                UnityEngine.Debug.LogError("assertion failed @ " + filename + " (" + line.ToString() + ")\nmessage: " + msg);
                UnityEngine.Debug.Break();
#endif
            }
        }
    }
}
