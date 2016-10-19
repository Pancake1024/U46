using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoidGooglePlusImplementation : MonoBehaviour, GooglePlusImplementation
{
	GooglePlusImplementationCallbacksContainer callbacksContainer;
	
	public void SetCallbacksContainer(GooglePlusImplementationCallbacksContainer callbacksContainer)
	{
		this.callbacksContainer = callbacksContainer;
	}
	
	public void Login()
	{
		callbacksContainer.onLoginCompleted(false);
	}
	
	public void RequestUserData()
	{
		callbacksContainer.onUserDataRequestCompleted(new GooglePlusUser(string.Empty, string.Empty, string.Empty, string.Empty));
	}
	
	public void RequestAccessToken(string userEmail)
	{
		callbacksContainer.onAccessTokenRequestCompleted(string.Empty, false);
	}
	
	public void RequestOtherUsersPicureUrl(List<string> idsList)
	{
		callbacksContainer.onOtherUsersPicureUrlRequestCompleted(new Dictionary<string,string>());
	}
}