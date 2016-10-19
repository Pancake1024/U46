using UnityEngine;
using System.Collections;
using System;

public class GooglePlusManagerCallbacksContainer
{
    public Action<bool> loginCallback;
    public Action<bool> userDataRequestCallback;
    public Action<bool> userPictureRequestCallback;
    public Action<bool> accessTokenRequestCallback;
    public Action<bool> otherUsersPicureUrlRequestCallback;
}