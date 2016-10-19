using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public interface FacebookImplementation
{
    bool IsInitialized { get; }
    bool IsLoggedIn { get; }
    string UserId { get; }
    string AccessToken { get; }
    string Country { get; }
    string Gender { get; }
    string Birthday { get; }
    string Name { get; }
    string MeAndFriendsIdsCommaSeparated { get; }
    Sprite UserPicture { get; }
    Dictionary<string, string> FriendsDictionary { get; }

    bool AreInvitablePicturesReady { get; }

    void InitFb(string fbAppId, Action initWithSuccessfulLoginCallback, Action initWithoutLoginCallback);
    void Login(string permissions, Action successCallback, Action failCallback, Action errorCallback);
    void GetFriends();

    void Feed(string toId = "",
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
              Action<bool> callback = null);

    void Logout();

    void DownloadPictures(List<string> userIds, Action picturesReadyCallback);
    Sprite GetUsersPicture(string facebookId);
    void ClearPictures();

    void Test_PrintNumPictures();

    bool UserIsFriend(string userId);

    void RequestInvitableFriends(Action<List<OnTheRunFacebookManager.InvitableFriend>> invitableFriendsReadyCallback);
    void InviteFriend(OnTheRunFacebookManager.InvitableFriend friend, Action<bool, string, string> callback);
    //void InviteAllFriends(Action<bool> callback);

	void LogEvent(string eventName, Dictionary<string, object> parameters);
}