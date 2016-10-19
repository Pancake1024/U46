using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GooglePlusImplementationCallbacksContainer
{
    public Action<bool> onLoginCompleted;
    public Action<GooglePlusUser> onUserDataRequestCompleted;
    public Action<string, bool> onAccessTokenRequestCompleted;
    public Action<Dictionary<string, string>> onOtherUsersPicureUrlRequestCompleted;
}