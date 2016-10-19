using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class FakeFacebookImplementation : MonoBehaviour, FacebookImplementation
{
    const bool TEST_LOGIN_SUCCESS = true;

    public bool IsInitialized { get { return true; } }//false; } }
    public bool IsLoggedIn { get { return TEST_LOGIN_SUCCESS; } }//false; } }
    public string UserId { get { return string.Empty; } }
    public string AccessToken { get { return string.Empty; } }
    public string Country { get { return string.Empty; } }
    public string Gender { get { return string.Empty; } }
    public string Birthday { get { return string.Empty; } }
    public string Name { get { return string.Empty; } }
    public string MeAndFriendsIdsCommaSeparated { get { return string.Empty; } }
    public Sprite UserPicture { get { return null; } }
    public Dictionary<string, string> FriendsDictionary { get { return new Dictionary<string, string>(); } }

    public bool AreInvitablePicturesReady { get { return true; } }

    public void InitFb(string fbAppId, Action initWithSuccessfulLoginCallback, Action initWithoutLoginCallback)
    {
        initWithoutLoginCallback();
    }

    public void Login(string permissions, Action successCallback, Action failCallback, Action errorCallback)
    {
        //errorCallback();
        if (TEST_LOGIN_SUCCESS)
            successCallback();
        else
            failCallback();
    }

    public void GetFriends()
    {
    }

    public void Feed(string toId = "",
              string link = "",
              string linkName = "",
              string linkCaption = "",
              string linkDescription = "",
              string picture = "",
              string mediaSource = "",
              string actionName = "",
              string actionLink = "",
              string reference = "",
              Dictionary<string, string[]> properties = null,
              Action<bool> callback = null)
    {
        callback(false);
    }

    public void Logout()
    {
    }

    public void DownloadPictures(List<string> userIds, Action picturesReadyCallback)
    {
        picturesReadyCallback();
    }

    public Sprite GetUsersPicture(string facebookId)
    {
        return null;
    }

    public void ClearPictures()
    {
    }

    public void Test_PrintNumPictures()
    {
    }

    public bool UserIsFriend(string userId)
    {
        return false;
    }

    public void RequestInvitableFriends(Action<List<OnTheRunFacebookManager.InvitableFriend>> invitableFriendsReadyCallback)
    {
        List<OnTheRunFacebookManager.InvitableFriend> testInvitableFriends = new List<OnTheRunFacebookManager.InvitableFriend>();

        if (TEST_LOGIN_SUCCESS)
        {
            testInvitableFriends.Add(new OnTheRunFacebookManager.InvitableFriend("fr_1_id", "fr_1", string.Empty));
            testInvitableFriends.Add(new OnTheRunFacebookManager.InvitableFriend("fr_2_id", "fr_2", string.Empty));
            testInvitableFriends.Add(new OnTheRunFacebookManager.InvitableFriend("fr_3_id", "fr_3", string.Empty));
        }

        invitableFriendsReadyCallback(testInvitableFriends);
    }

    public void InviteFriend(OnTheRunFacebookManager.InvitableFriend friend, Action<bool, string, string> callback)
    {
        //PushBlockButtonPopup();
        StartCoroutine(InviteFriendCoroutine(friend, callback));
    }

    IEnumerator InviteFriendCoroutine(OnTheRunFacebookManager.InvitableFriend friend, Action<bool, string, string> callback)
    {
        yield return new WaitForSeconds(0.5f);

        //PopBlockButtonPopup();

        if (TEST_LOGIN_SUCCESS)
            callback(true, friend.Name, string.Empty);
        else
            callback(false, string.Empty, string.Empty);
    }

    /*public void InviteAllFriends(Action<bool> callback)
    {
        callback(false);
    }*/
	
	public void LogEvent(string eventName, Dictionary<string, object> parameters)
	{
	}

    /*void PushBlockButtonPopup()
    {
        Debug.Log("PushBlockButtonPopup");
    }*/
}