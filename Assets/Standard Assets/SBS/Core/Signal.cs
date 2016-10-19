using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Reflection;
#endif
using UnityEngine;

namespace SBS.Core
{
    [Serializable]
    public class Signal
    {
        [SerializeField]
        protected List<SignalTarget> targets = null;
        [SerializeField]
        protected string argType = null;

        protected bool invokeGuard = false;
        protected List<int> indicesToRemove = new List<int>();

        public void Invoke()
        {
            if (indicesToRemove.Count > 0)
            {
                foreach (int i in indicesToRemove)
                    targets.RemoveAt(i);
                indicesToRemove.Clear();
            }
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(argType))
                Debug.LogError("Incorrect parameter type, expected [" + argType + "], got [void].");
#endif
            invokeGuard = true;
            foreach (SignalTarget target in targets)
                target.gameObject.SendMessage(target.method, SendMessageOptions.DontRequireReceiver);
            invokeGuard = false;
        }

        public void Invoke(object value)
        {
            if (indicesToRemove.Count > 0)
            {
                foreach (int i in indicesToRemove)
                    targets.RemoveAt(i);
                indicesToRemove.Clear();
            }
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(argType) || !argType.Equals(value.GetType().FullName))
                Debug.LogError("Incorrect parameter type, expected [" + (string.IsNullOrEmpty(argType) ? "void" : argType) + "], got [" + value.GetType().FullName + "].");
#endif
            invokeGuard = true;
            foreach (SignalTarget target in targets)
                target.gameObject.SendMessage(target.method, value, SendMessageOptions.DontRequireReceiver);
            invokeGuard = false;
        }

        public void AddTarget(GameObject gameObject, string method)
        {
            if (indicesToRemove.Count > 0)
            {
                foreach (int i in indicesToRemove)
                    targets.RemoveAt(i);
                indicesToRemove.Clear();
            }
#if UNITY_EDITOR
            MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
            MethodInfo foundMethod = null;
            foreach (Component component in components)
            {
                MethodInfo info = component.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (null == info)
                    continue;
                ParameterInfo[] parameters = info.GetParameters();
                if (string.IsNullOrEmpty(argType))
                {
                    if (parameters.Length > 0)
                        continue;
                }
                else
                {
                    if (parameters.Length != 1)
                        continue;
                    if (!parameters[0].ParameterType.FullName.Equals(argType))
                        continue;
                }
                foundMethod = info;
            }
            if (null == foundMethod)
            {
                Debug.LogError("Method \"" + method + "\" not found with parameter [" + (string.IsNullOrEmpty(argType) ? "void" : argType) + "]");
                return;
            }
#endif
            targets.Add(new SignalTarget(gameObject, method));
        }

        public void RemoveTarget(GameObject gameObject, string method)
        {
            if (!invokeGuard && indicesToRemove.Count > 0)
            {
                foreach (int i in indicesToRemove)
                    targets.RemoveAt(i);
                indicesToRemove.Clear();
            }

            for (int i = targets.Count - 1; i >= 0; --i)
            {
                SignalTarget target = targets[i];

                if (target.gameObject == gameObject && target.method == method)
                {
                    if (invokeGuard)
                        indicesToRemove.Add(i);
                    else
                        targets.RemoveAt(i);
                }
            }
        }

        public void RemoveTarget(GameObject gameObject)
        {
            if (!invokeGuard && indicesToRemove.Count > 0)
            {
                foreach (int i in indicesToRemove)
                    targets.RemoveAt(i);
                indicesToRemove.Clear();
            }

            for (int i = targets.Count - 1; i >= 0; --i)
            {
                if (targets[i].gameObject == gameObject)
                {
                    if (invokeGuard)
                        indicesToRemove.Add(i);
                    else
                        targets.RemoveAt(i);
                }
            }
        }

        public void RemoveAllTargets()
        {
            if (!invokeGuard && indicesToRemove.Count > 0)
            {
                foreach (int i in indicesToRemove)
                    targets.RemoveAt(i);
                indicesToRemove.Clear();
            }

            if (invokeGuard)
            {
                for (int i = 0, c = targets.Count; i < c; ++i)
                    indicesToRemove.Add(i);
            }
            else
                targets.Clear();
        }

        public static Signal CreateVoid()
        {
            Signal s = new Signal();
            s.targets = new List<SignalTarget>();
            s.argType = null;
            return s;
        }

        public static Signal Create<T>()
        {
            Signal s = new Signal();
            s.targets = new List<SignalTarget>();
            s.argType = typeof(T).FullName;
            return s;
        }
    }
}
