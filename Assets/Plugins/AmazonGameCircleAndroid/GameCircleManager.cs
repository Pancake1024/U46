/**
 * © 2012-2014 Amazon Digital Services, Inc. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with the License. A copy
 * of the License is located at
 *
 * http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if UNITY_ANDROID && UNITY_KINDLE

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// GameCircle manager.
/// </summary>
/// <remarks>
/// Helper script for managing native to unity messages
/// </remarks>

public class GameCircleManager : MonoBehaviour
{
    
    void Awake()
    {
        // Set the GameObject name to the class name for easy access from native code
        gameObject.name = this.GetType().ToString();
        DontDestroyOnLoad( this );
    }

    public void serviceReady( string empty )
    {
        //Debug.Log("###-XXX-### GameCircleManager - serviceReady");
        AGSClient.Log ("GameCircleManager - serviceReady");
        
        AGSClient.
            ServiceReady(empty);
    }
    
    public void serviceNotReady( string param )
    {
        //Debug.Log("###-XXX-### GameCircleManager - serviceNotReady");
        AGSClient.Log ("GameCircleManager - serviceNotReady");

        AGSClient.
            ServiceNotReady( param );
    }
    
    public void playerReceived( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - playerReceived");
        AGSClient.Log ("GameCircleManager - playerReceived");
        AGSPlayerClient.
            PlayerReceived( json );
    }
    
    public void playerFailed( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - playerFailed");
        AGSClient.Log ("GameCircleManager - playerFailed");
        AGSPlayerClient.
            PlayerFailed( json );
    }

    public void localPlayerFriendRequestComplete(string json)
    {
        //Debug.Log("###-XXX-### GameCircleManager - localPlayerFriendRequestComplete");
        AGSClient.Log ("GameCircleManager - localPlayerFriendRequestComplete");
        AGSPlayerClient.LocalPlayerFriendsComplete (json);
    }

    public void batchFriendsRequestComplete(string json)
    {
        //Debug.Log("###-XXX-### GameCircleManager - batchFriendsRequestComplete");
        AGSClient.Log ("GameCircleManager - batchFriendsRequestComplete");
        AGSPlayerClient.BatchFriendsRequestComplete (json);
    }

    public void onSignedInStateChange(string isSignedIn)
    {
        //Debug.Log("###-XXX-### GameCircleManager - onSignedInStateChange");
        AGSClient.Log ("GameCircleManager - onSignedInStateChange");
        AGSPlayerClient.OnSignedInStateChanged (Boolean.Parse (isSignedIn));
    }

    public void submitScoreFailed( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - submitScoreFailed");
        AGSClient.Log ("GameCircleManager - submitScoreFailed");

        AGSLeaderboardsClient.
            SubmitScoreFailed( json );
    }

    public void submitScoreSucceeded( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - submitScoreSucceeded");
        AGSClient.Log ("GameCircleManager - submitScoreSucceeded");
        AGSLeaderboardsClient.
            SubmitScoreSucceeded( json );
    }

    public void requestLeaderboardsFailed( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestLeaderboardsFailed");
        AGSClient.Log ("GameCircleManager - requestLeaderboardsFailed");
        AGSLeaderboardsClient.
            RequestLeaderboardsFailed( json );
    }

    public void requestLeaderboardsSucceeded( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestLeaderboardsSucceeded");
        AGSClient.Log ("GameCircleManager - requestLeaderboardsSucceeded");
        AGSLeaderboardsClient.
            RequestLeaderboardsSucceeded(json);
    }

    public void requestLocalPlayerScoreFailed( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestLocalPlayerScoreFailed");
        AGSClient.Log ("GameCircleManager - requestLocalPlayerScoreFailed");
        AGSLeaderboardsClient.
            RequestLocalPlayerScoreFailed( json );
    }

    public void requestLocalPlayerScoreSucceeded( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestLocalPlayerScoreSucceeded");
        AGSClient.Log ("GameCircleManager - requestLocalPlayerScoreSucceeded");
        AGSLeaderboardsClient.
                RequestLocalPlayerScoreSucceeded(json);
    }

    public void requestPlayerScoreCompleted( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestPlayerScoreCompleted");
        AGSClient.Log ("GameCircleManager - requestPlayerScoreCompleted");
        AGSLeaderboardsClient.RequestScoreForPlayerComplete (json);
    }


    public void requestScoresSucceeded( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestScoresSucceeded");
        AGSClient.Log ("GameCircleManager - requestScoresSucceeded:");
        AGSLeaderboardsClient.RequestScoresSucceeded(json);
    }

    public void requestScoresFailed( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestScoresFailed");
        AGSClient.Log ("GameCircleManager - requestScoresFailed");
        AGSLeaderboardsClient.RequestScoresFailed(json);
    }
    
    public void requestPercentileRanksSucceeded(string json)
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestPercentileRanksSucceeded");
        AGSClient.Log ("GameCircleManager - requestPercentileRanksSucceeded");
        AGSLeaderboardsClient.RequestPercentileRanksSucceeded(json);
    }
    
    public void requestPercentileRanksFailed(string json)
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestPercentileRanksFailed");
        AGSClient.Log ("GameCircleManager - requestPercentileRanksFailed");
        AGSLeaderboardsClient.RequestPercentileRanksFailed(json);
    }

    public void requestPercentileRanksForPlayerSucceeded(string json)
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestPercentileRanksForPlayerSucceeded");
        AGSClient.Log ("GameCircleManager - requestPercentileRanksForPlayerSucceeded");
        AGSLeaderboardsClient.RequestPercentileRanksForPlayerComplete (json);
    }

    public void updateAchievementSucceeded( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - updateAchievementSucceeded");
        AGSClient.Log ("GameCircleManager - updateAchievementSucceeded");
        AGSAchievementsClient.UpdateAchievementSucceeded( json );
    }
    
    public void updateAchievementFailed( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - updateAchievementsFailed");
        AGSClient.Log ("GameCircleManager - updateAchievementsFailed");
        AGSAchievementsClient.
            UpdateAchievementFailed( json );
    }
    
    public void requestAchievementsSucceeded( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestAchievementsSucceeded");
        AGSClient.Log ("GameCircleManager - requestAchievementsSucceeded");

        AGSAchievementsClient.
            RequestAchievementsSucceeded( json );
    }
    
    public void requestAchievementsFailed( string json )
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestAchievementsFailed");
        AGSClient.Log ("GameCircleManager -  requestAchievementsFailed");
        AGSAchievementsClient.
            RequestAchievementsFailed( json );
    }

    public void requestAchievementsForPlayerCompleted(string json)
    {
        //Debug.Log("###-XXX-### GameCircleManager - requestAchievementsForPlayerCompleted");
        AGSClient.Log ("GameCircleManager -  requestAchievementsForPlayerCompleted");
        AGSAchievementsClient.RequestAchievementsForPlayerComplete (json);
    }


    public void onNewCloudData( string empty ){
        AGSWhispersyncClient.OnNewCloudData();    
    }

    public void onDataUploadedToCloud (string empty){
        AGSWhispersyncClient.OnDataUploadedToCloud ();
    }

    public void onThrottled (string empty){
        AGSWhispersyncClient.OnThrottled ();
    }

    public void onDiskWriteComplete (string empty){
        AGSWhispersyncClient.OnDiskWriteComplete ();
    }
    
    public void onFirstSynchronize (string empty){
        AGSWhispersyncClient.OnFirstSynchronize ();
    }

    public void onAlreadySynchronized (string empty){
        AGSWhispersyncClient.OnAlreadySynchronized ();
    }

    public void onSyncFailed(string failReason){
        AGSWhispersyncClient.OnSyncFailed (failReason);
    }

    public void OnApplicationFocus(Boolean focusStatus){

        //Debug.Log("###-XXX-### GameCircleManager - OnApplicationFocus - AGSClient.ReinitializeOnFocus: " + AGSClient.ReinitializeOnFocus);
        if (!AGSClient.ReinitializeOnFocus){
            return;
        }
        
        if(focusStatus)
        {
        //Debug.Log("###-XXX-### GameCircleManager - OnApplicationFocus - Calling AGSClient.Init()");
            AGSClient.Init();
        } else {
        //Debug.Log("###-XXX-### GameCircleManager - OnApplicationFocus - Calling AGSClient.release()");
            AGSClient.release();
        }
    }

    public void OnAppplicationQuit()
    {
        //Debug.Log("###-XXX-### GameCircleManager - OnApplicationQuit");
        AGSClient.Log ("GameCircleManager - OnApplicationQuit");
        AGSClient.Shutdown();
    }
    
}


#endif