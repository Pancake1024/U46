using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using McSocialApiUtils;

[AddComponentMenu("OnTheRun/OnTheRunNotificationManager")]
public class OnTheRunNotificationManager : Manager<OnTheRunNotificationManager>
{
    protected const string fuel_notification_id = "gift_fuel_notification";
    public int fuel_gift_value = 1;

    protected bool neverResetInvites;
    protected int daysToResetDiamondsreward;
    protected int daysToResetGiftsLimit;

    protected int maxInvites;
    protected int maxGifts;

    protected List<string> alreadyInvited;
    protected List<string> reInviteList;
    protected List<string> alreadyGifted;
    protected List<string> waitingForInviteReward;
    protected int friendsInvitedToday;
    protected int giftsDoneToday;

    protected string kInvitedFriendsId = "fb_tinv";
    protected const string kDaysToResetReward = "key_dtrrew";
    protected const string kDaysToResetGifts = "key_dtrg";
    protected const string kGiftsDoneId = "key_gd";
    protected const string kWaitForReward = "key_wr";
    protected const string kAlreadyInvited = "key_ai";
    protected const string kReInvited = "key_rei";
    protected const string kAlreadyGifted = "key_ag";

    public FuelGiftNotification fuelGiftNotification = null;
    
    protected List<NotificationData> fuelNotificationList;

    protected UIRoot uiRoot;

    public struct NotificationData
    {
        public string id;
        public string messageId;
    }

    public int PendingNotifications
    {
        get {
                if (fuelNotificationList == null)
                    return 0;
                else
                    return fuelNotificationList.Count;
            }
    }

    public int InvitesRemaining
    {
        get { return Mathf.Clamp(maxInvites - friendsInvitedToday, 0, maxInvites); }
    }

    public int GiftRemaining
    {
        get { return Mathf.Clamp(maxGifts - giftsDoneToday, 0, maxGifts); }
    }

    #region Singleton instance
    public static OnTheRunNotificationManager Instance
    {
        get
        {
            OnTheRunNotificationManager instance = Manager<OnTheRunNotificationManager>.Get();
            if (instance.fuelGiftNotification == null)
                instance.Initialize();
            return instance;
        }
    }
    #endregion

    #region FuelGiftNotification class
    public class FuelGiftNotification
    {
        public string type;
        public string content;

        public FuelGiftNotification(string _type, string _content)
        {
            type = _type;
            content = _content;
        }
    }
    #endregion

    #region Initialization
    void Initialize()
    {
        fuelGiftNotification = new FuelGiftNotification(fuel_notification_id, fuel_gift_value.ToString());

        waitingForInviteReward = GetIdsListFromPlayerPrefs(kWaitForReward);
        alreadyInvited = GetIdsListFromPlayerPrefs(kAlreadyInvited);
        alreadyGifted = GetIdsListFromPlayerPrefs(kAlreadyGifted);
        reInviteList = GetIdsListFromPlayerPrefs(kReInvited);

        maxInvites = OnTheRunDataLoader.Instance.GetMaxInvites();
        maxGifts = OnTheRunDataLoader.Instance.GetMaxGifts();

        neverResetInvites = OnTheRunDataLoader.Instance.GetDaysResetInvites() == 0;
        daysToResetDiamondsreward = EncryptedPlayerPrefs.GetInt(kDaysToResetReward, OnTheRunDataLoader.Instance.GetDaysResetInvites());
        friendsInvitedToday = EncryptedPlayerPrefs.GetInt(kInvitedFriendsId, 0);

        daysToResetGiftsLimit = EncryptedPlayerPrefs.GetInt(kDaysToResetGifts, OnTheRunDataLoader.Instance.GetDaysResetGifts());
        giftsDoneToday = EncryptedPlayerPrefs.GetInt(kGiftsDoneId, 0);
    }

    void AddToGiftedList(string id)
    {
        if (!alreadyGifted.Contains(id))
            alreadyGifted.Add(id);
        SaveIdsListToPlayerPrefs(kAlreadyGifted, alreadyGifted);
    }

    public bool GiftAlreadyDone(string id)
    {
        return alreadyGifted.IndexOf(id) >= 0;
    }

    public void AddToInvitedList(string friendId, string friendName)
    {
        if (!IsReInvited(friendName))
            ++friendsInvitedToday;
        EncryptedPlayerPrefs.SetInt(kInvitedFriendsId, friendsInvitedToday);
        if (friendsInvitedToday == maxInvites)
            OnTheRunUITransitionManager.Instance.OpenPopup("FBFeedbackPopup");

        if (!alreadyInvited.Contains(friendName))
            alreadyInvited.Add(friendName);
        SaveIdsListToPlayerPrefs(kAlreadyInvited, alreadyInvited);

        if (!waitingForInviteReward.Contains(friendId))
            waitingForInviteReward.Add(friendId);
        SaveIdsListToPlayerPrefs(kWaitForReward, waitingForInviteReward);
    }

    public bool IsAlreadyInvited(string friendName)
    {
        return alreadyInvited.IndexOf(friendName) >= 0;
    }

    public bool IsReInvited(string friendName)
    {
        return reInviteList.IndexOf(friendName) >= 0;
    }

    List<string> GetIdsListFromPlayerPrefs(string listKey)
    {
        List<string> idsList = new List<string>();

        string csvIds = EncryptedPlayerPrefs.GetString(listKey, string.Empty);
        if (!string.IsNullOrEmpty(csvIds))
        {
            string[] ids = csvIds.Split(',');
            if (ids.Length > 0)
            {
                foreach (var id in ids)
                {
                    if (!idsList.Contains(id))
                        idsList.Add(id);
                }
            }
        }

        //Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX Loaded " + listKey + "\n" + csvIds);

        return idsList;
    }
    
    void SaveIdsListToPlayerPrefs(string listKey, List<string> list)
    {
        string csvIds = string.Empty;

        if (list != null && list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                csvIds += list[i];

                if (i < list.Count - 1)
                    csvIds += ",";
            }
        }

        //Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX Saving " + listKey + "\n" + csvIds);

        EncryptedPlayerPrefs.SetString(listKey, csvIds);
    }

    #endregion

    #region Social Api calls
    public void OnLoginSuccessuful()
    {
        CheckForFuelGiftNotifications();
        if (Manager<UIManager>.Get().FrontPopup == null)
            CheckForInviteAccepted();
    }

    public void SendFuelGift(string friendId)
    {
        ++giftsDoneToday;
        EncryptedPlayerPrefs.SetInt(kGiftsDoneId, giftsDoneToday);

        AddToGiftedList(friendId);
        McSocialApiManager.Instance.SendNotification(friendId, fuelGiftNotification.type, fuelGiftNotification.content); 
		OnTheRunOmniataManager.Instance.TrackFuelSent();
    }

    public void CheckForFuelGiftNotifications()
    {
        McSocialApiManager.Instance.ReceiveAllNotifications(OnNotificationReceivedSuccess, OnNotificationReceivedFailed);
    }

    public void ResetGiftAndInviteDone(int daysPassed)
    {
        daysToResetDiamondsreward -= daysPassed;
        daysToResetGiftsLimit -= daysPassed;

        if (daysToResetDiamondsreward <= 0 && !neverResetInvites)
        {
            daysToResetDiamondsreward = OnTheRunDataLoader.Instance.GetDaysResetInvites();

            reInviteList = new List<string>(alreadyInvited);
            SaveIdsListToPlayerPrefs(kReInvited, reInviteList);

            alreadyInvited.Clear();
            SaveIdsListToPlayerPrefs(kAlreadyInvited, alreadyInvited);

            friendsInvitedToday = 0;
            EncryptedPlayerPrefs.SetInt(kInvitedFriendsId, friendsInvitedToday);
        }

        if (daysToResetGiftsLimit <= 0)
        {
            daysToResetGiftsLimit = OnTheRunDataLoader.Instance.GetDaysResetGifts();

            alreadyGifted.Clear();
            SaveIdsListToPlayerPrefs(kAlreadyGifted, alreadyGifted);

            giftsDoneToday = 0;
            EncryptedPlayerPrefs.SetInt(kGiftsDoneId, giftsDoneToday);
        }

        EncryptedPlayerPrefs.SetInt(kDaysToResetReward, daysToResetDiamondsreward);
        EncryptedPlayerPrefs.SetInt(kDaysToResetGifts, daysToResetGiftsLimit);
    }

    List<string> testIds = new List<string>
            {
                /*"100008464654504",
                "100008456974525",
                "1379057299051874",
                "100007160887838",
                "1378267922463798",
                "100008468193383",
                "1378312189126404",
                "100008428206617",
                "100004812850277",
                "277929219054101"*/
            };

    public void CheckForInviteAccepted()
    {
        if (waitingForInviteReward.Count > 0)
        {
#if UNITY_EDITOR
            foreach (var testFriendId in testIds)
            {
                if (waitingForInviteReward.IndexOf(testFriendId) >= 0)
                {
                    /*OnTheRunUITransitionManager.Instance.OpenPopup("FBFeedbackPopup");
                    UIPopup frontPopup = Manager<UIManager>.Get().FrontPopup;

                    string friendName = "friend_name";
                    if (OnTheRunFacebookManager.Instance.FriendsDictionary != null)
                        if (OnTheRunFacebookManager.Instance.FriendsDictionary.ContainsKey(testFriendId))
                            friendName = OnTheRunFacebookManager.Instance.FriendsDictionary[testFriendId];

                    frontPopup.GetComponent<UIFBFeedbackPopup>().SetFriendName(friendName);*/

                    waitingForInviteReward.Remove(testFriendId);
                    SaveIdsListToPlayerPrefs(kWaitForReward, waitingForInviteReward);

                    break;
                }
            }
#else
            McSocialApiManager.Instance.GetFriends(success =>
            {
                if (success && McSocialApiManager.Instance.Friends != null && McSocialApiManager.Instance.Friends.Count > 0)
                {
                    foreach(var friend in McSocialApiManager.Instance.Friends)
                    {
                        if (friend.Type == LoginType.Facebook && waitingForInviteReward.IndexOf(friend.LoginTypeId) >= 0)
                        {
                            /*OnTheRunUITransitionManager.Instance.OpenPopup("FBFeedbackPopup");
                            UIPopup frontPopup = Manager<UIManager>.Get().FrontPopup;

                            string friendName = friend.Name;
                            if (OnTheRunFacebookManager.Instance.FriendsDictionary != null)
                                if (OnTheRunFacebookManager.Instance.FriendsDictionary.ContainsKey(friend.LoginTypeId))
                                    friendName = OnTheRunFacebookManager.Instance.FriendsDictionary[friend.LoginTypeId];
                            frontPopup.GetComponent<UIFBFeedbackPopup>().SetFriendName(friendName);*/
                            
                            waitingForInviteReward.Remove(friend.LoginTypeId);
                            SaveIdsListToPlayerPrefs(kWaitForReward, waitingForInviteReward);

                            break;
                        }
                    }
                }
            });
#endif
        }
    }
    #endregion

    #region Callbacks
    void OnNotificationReceivedSuccess(List<ReceivedNotification> list)
    {
        fuelNotificationList = new List<NotificationData>();

        foreach (ReceivedNotification notification in list)
        {
            NotificationData data = new NotificationData();
            data.id = notification.SenderId;
            data.messageId = notification.MessageId;
            fuelNotificationList.Add(data);
        }

        Manager<UIManager>.Get().BroadcastMessage("UpdateFacebookButtonText", true, SendMessageOptions.DontRequireReceiver);
    }

    void OnNotificationReceivedFailed()
    {
        Debug.Log("*** OnNotificationReceivedFailed");
    }

    void OnNotificationSingleReceivedSuccess(ReceivedNotification not)
    {
        uiRoot.HomeButton.State = UIButton.StateType.Disabled;
        OnTheRunUITransitionManager.Instance.OpenPopup("FBGiftReceivedPopup");
        Manager<UIManager>.Get().FrontPopup.GetComponent<UIFBGiftPopup>().Initialize(not.SenderId, not.SenderName, not.SenderPicture, !GiftAlreadyDone(not.SenderId));

        int notificationToDelete = -1;
        for (int i = 0; i < fuelNotificationList.Count; i++)
        {
            if (fuelNotificationList[i].messageId.Equals(not.MessageId))
            {
                notificationToDelete = i;
                break;
            }
        }

        if (notificationToDelete >= 0 && notificationToDelete < fuelNotificationList.Count)
        {
            McSocialApiManager.Instance.DeleteNotification(fuelNotificationList[notificationToDelete].messageId);  //fuelNotificationList[0].messageId);//need callbacks??
            fuelNotificationList.RemoveAt(notificationToDelete);    //0);
        }

        Manager<UIRoot>.Get().BroadcastMessage("UpdateFacebookButtonText", true);
        Manager<UIRoot>.Get().BroadcastMessage("HideLoadingPopup");
    }
    void OnNotificationSingleReceivedFailed()
    {
        Debug.Log("*** OnNotificationReceivedFailed");
    }
    #endregion

    #region Show popups
    public void ShowGiftReceivedPopup()
    {
        if (uiRoot == null)
            uiRoot = Manager<UIRoot>.Get();

        McSocialApiManager.Instance.ReceiveSingleNotification(fuelNotificationList[0].messageId, OnNotificationSingleReceivedSuccess, OnNotificationSingleReceivedFailed);
    }

    public void PopupClosed()
    {
        if (uiRoot == null)
            uiRoot = Manager<UIRoot>.Get();

        if (fuelNotificationList != null && fuelNotificationList.Count > 0)
            OnTheRunUITransitionManager.Instance.ClosePopup(ShowGiftReceivedPopup);
        else if (waitingForInviteReward != null && waitingForInviteReward.Count > 0)
        {
            OnTheRunUITransitionManager.Instance.ClosePopup(CheckForInviteAccepted);
            uiRoot.HomeButton.State = UIButton.StateType.Normal;
        }
        else
        {
            OnTheRunUITransitionManager.Instance.ClosePopup();
            uiRoot.HomeButton.State = UIButton.StateType.Normal;
        }
    }
    #endregion
}