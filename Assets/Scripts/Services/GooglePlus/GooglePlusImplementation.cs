using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface GooglePlusImplementation
{
    void SetCallbacksContainer(GooglePlusImplementationCallbacksContainer callbacksContainer);

    void Login();
    void RequestUserData();
    void RequestAccessToken(string userEmail);
    void RequestOtherUsersPicureUrl(List<string> idsList);
}