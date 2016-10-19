using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/VehicleTrackData")]
public class VehicleTrackData : MonoBehaviour
{
    #region Public members
    public int branchesMask = -1;
    public bool alwaysUpdate = true;  //update only first frame VS update every frame (default)
    #endregion

    #region Protected members
    protected TrackBranch trackBranch = null;
    protected float trackPos = 0.0f;
    protected TokensManager.TokenHit lastTokenHit = new TokensManager.TokenHit();
    protected bool firstFrame = true;
    #endregion

    #region Public properties
    public bool IsOnToken
    {
        get
        {
            return (lastTokenHit.token != null);
        }
    }

    public bool IsOnTrack
    {
        get
        {
            return (trackBranch != null);
        }
    }

    public TrackBranch TrackBranch
    {
        get
        {
            return trackBranch;
        }
    }

    public float TrackPosition
    {
        get
        {
            return trackPos;
        }
    }

    public Token Token
    {
        get
        {
            return lastTokenHit.token;
        }
    }

    public float Longitudinal
    {
        get
        {
            return lastTokenHit.longitudinal;
        }
    }

    public float Trasversal
    {
        get
        {
            return lastTokenHit.trasversal;
        }
    }

    public SBSVector3 TokenPosition
    {
        get
        {
            return lastTokenHit.position;
        }
    }

    public SBSVector3 TokenTangent
    {
        get
        {
            return lastTokenHit.tangent;
        }
    }
    #endregion

    #region Messages
    void OnPostInit()
    {
        this.UpdateTrackData();
    }
    #endregion

    #region Protected methods
    protected void UpdateTrackData()
    {
        List<TokensManager.TokenHit> hits;
        TokensManager.TokenHit hitInfo = null;

//      bool tokenFound = RacingGame.Instance.TokensManager.GetToken(transform.position, lastTokenHit.token, out hitInfo);
        hits = TokensManager.Instance.GetTokens(transform.position, lastTokenHit.token, null, true, branchesMask);
      
        foreach (TokensManager.TokenHit hit in hits)
        {
            if ((hit.token.TrackBranch.mask & branchesMask) != 0)
            {
                hitInfo = hit;
                break;
            }
        }

        if (null == hitInfo && hits.Count > 0)
            hitInfo = hits[0];
//      if (tokenFound) {
        if (hitInfo != null)
        {
            if (hitInfo.token.IsTrackToken)
            {
                trackBranch = hitInfo.token.TrackBranch;

                Track track = RacingManager.Instance.track;
                float trackOffset = track.GetStartOffset();

                trackPos = trackBranch.GetTrackPosition(hitInfo.token, hitInfo.longitudinal) - trackOffset;
                trackPos = (trackPos >= 0.0f ? trackPos : track.Length + trackPos);
                trackPos = (trackPos <= track.Length ? trackPos : trackPos - track.Length);
            }
            else
            {
//              trackBranch = null;
//              trackPos = 0.0f;
            }

            lastTokenHit.token = hitInfo.token;
            lastTokenHit.longitudinal = hitInfo.longitudinal;
            lastTokenHit.trasversal = hitInfo.trasversal;
            lastTokenHit.position = hitInfo.position;
            lastTokenHit.tangent = hitInfo.tangent;

            //Debug.Log("t " + hitInfo.trasversal + " l " + hitInfo.longitudinal + " tp " + trackPos + " " + hitInfo.token.name);
        }
        else
        {
//          trackBranch = null;
//          trackPos = 0.0f;

            lastTokenHit.token = null;
            lastTokenHit.longitudinal = 0.0f;
            lastTokenHit.trasversal = 0.0f;
            lastTokenHit.position.Set(0.0f, 0.0f, 0.0f);
            lastTokenHit.tangent.Set(0.0f, 0.0f, 0.0f);
        }
    }
    #endregion

    #region Unity callbacks
    void Update()
    {
        if (!alwaysUpdate)
            return;

        this.UpdateTrackData();
    }

    public void ForceUpdate()
    {
        if (!alwaysUpdate)
            return;

        this.UpdateTrackData();
    }
    #endregion
}
