using UnityEngine;
using System;

#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{
    public class DefaultReachability : Reachability
    {
        public bool Reachable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }       
    }
}

#endif

