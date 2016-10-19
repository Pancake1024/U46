using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using System.Collections;

[AddComponentMenu("OnTheRun/OnTheRunFBFriendsSpawner")]
public class OnTheRunFBFriendsSpawner : MonoBehaviour
{
    public GameObject friendMarkerRef;

    protected OnTheRunSpawnManager spawnManager;
    protected OnTheRunGameplay gameplayManager;
    protected PlayerKinematics player;
    protected UISharedData uiSharedData;
    protected List<McSocialApiUtils.ScoreData> reverseOrderedScores;
    protected float nextFriendDistance = -1.0f;
    protected int nextFriendDistanceIdx = -1;
    protected List<GameObject> usedMarkers;
    
    protected struct FBFriendData
    {
        public string id;
        public string name;
        public float distance;
    }

    #region Unity Callbacks
    void Start()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        spawnManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunSpawnManager>();
        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();

    }

    public void Initialize()
    {
        usedMarkers = new List<GameObject>();
        reverseOrderedScores = McSocialApiManager.Instance.LastRequestedScores;
        reverseOrderedScores.Sort(
            delegate(McSocialApiUtils.ScoreData score1, McSocialApiUtils.ScoreData score2)
            {
                return -1 * score1.Rank.CompareTo(score2.Rank);
            });

        if (reverseOrderedScores.Count > 0)
        {
            nextFriendDistanceIdx = 0;
            nextFriendDistance = reverseOrderedScores[nextFriendDistanceIdx].Score;
            /*for (int i = 0; i < reverseOrderedScores.Count; ++i)
            {
                nextFriendDistanceIdx = i;
                nextFriendDistance = reverseOrderedScores[nextFriendDistanceIdx].Score;

                if (nextFriendDistance < uiSharedData.InterfaceDistance + 200.0f)
                    break;
            }*/
        }
    }

    public void DestroyAll()
    {
        if (usedMarkers != null)
        {
            for (int i = 0; i < usedMarkers.Count; ++i)
            {
                if (usedMarkers[i] != null)
                    Destroy(usedMarkers[i]);
            }
            usedMarkers.Clear();
        }
    }

    protected int pairsComparer(FBFriendData a, FBFriendData b)
    {
        return a.distance.CompareTo(b.distance);
    }

    void FixedUpdate()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        if (!gameplayManager.Gameplaystarted || OnTheRunTutorialManager.Instance.TutorialActive || nextFriendDistanceIdx<0)
            return;

        if (uiSharedData.InterfaceDistance > nextFriendDistance - 200.0f && nextFriendDistanceIdx >= 0)
        {
            PlaceFriendMarker(reverseOrderedScores[nextFriendDistanceIdx].Id);
            ++nextFriendDistanceIdx;
            if (nextFriendDistanceIdx < reverseOrderedScores.Count)
                nextFriendDistance = reverseOrderedScores[nextFriendDistanceIdx].Score;
            else
                nextFriendDistanceIdx = -1;
        }

    }
    #endregion

    #region Functions
    public void PlaceFriendMarker(string facebookId)
    {
        float initPosition = player.PlayerRigidbody.position.z;
        float forwardDistance = 200.0f / OnTheRunUtils.ToOnTheRunMeters;
        GameObject marker = Instantiate(friendMarkerRef, new Vector3(0.0f, 0.0f, initPosition + forwardDistance), Quaternion.identity) as GameObject;

        SpriteRenderer spriteRenderer = marker.transform.Find("player_sign/fb_user").GetComponent<SpriteRenderer>();
        Sprite spriteToUSe = spriteRenderer.sprite = OnTheRunMcSocialApiData.Instance.defaultUserPicture;
        Sprite fbUserPicture = OnTheRunIngameHiScoreCheck.Instance.NextOpponentsPicture;
        if (fbUserPicture != null)
            spriteToUSe = fbUserPicture;

        spriteRenderer.sprite = spriteToUSe;
        usedMarkers.Add(marker);
    }
    #endregion
}
