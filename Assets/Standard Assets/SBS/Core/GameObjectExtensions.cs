using UnityEngine;

namespace SBS.Core
{
    public static class GameObjectExtensions
    {
        public static T GetComponentInParents<T>(this GameObject self)
            where T : Component
        {
            Transform t = self.transform;
            T component = t.GetComponent<T>();
            while (null == component)
            {
                t = t.parent;
                if (null == t)
                    break;
                component = t.GetComponent<T>();
            }
            return component;
        }
    }
}
