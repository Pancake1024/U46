using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GooglePlusCallbacksContainer
{
    public Action<bool> onLoginCompleted;
    public Action<GooglePlusUser> onUserDataRequestCompleted;
    public Action<string> onAccessTokenRequestCompleted;
    public Action<Dictionary<string, string>> onOtherUsersPicureUrlRequestCompleted;
}