using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using McSocialApiUtils;

public class McSocialApiCallbacksHandler
{
    public Action<LoginData> loginSuccessCallback;
    public Action loginFailCallback;

    public Action<List<FriendData>> getFriendsSuccessCallback;
    public Action getFriendsFailCallback;

    public Action<List<ScoreData>> getScoresSuccessCallback;
    public Action getScoresFailCallback;

    public Action postScoreSuccessCallback;
    public Action postScoreFailCallback;

    public Action storeDataSuccessCallback;
    public Action storeDataFailCallback;

    public Action<List<RetrievedData>> getDataSuccessCallback;
    public Action getDataFailCallback;

    public Action sendNotificationSuccessCallback;
    public Action sendNotificationFailCallback;

    public Action<List<ReceivedNotification>> receiveAllNotificationsSuccessCallback;
    public Action receiveAllNotificationsFailCallback;

    public Action<ReceivedNotification> receiveSingleNotoficationSuccessCallback;
    public Action receiveSingleNotificationFailCallback;

    public Action<ReceivedNotification> deleteNotificationSuccessCallback;
    public Action deleteNotificationFailCallback;

    public Action<DateTime> getUtcTimeSuccessCallback;
    public Action getUtcTimeFailCallback;
}