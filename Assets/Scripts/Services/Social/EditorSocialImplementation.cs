using UnityEngine;
using System.Collections;

public class EditorSocialImplementation : MonoBehaviour, SocialImplementation
{
    public bool IsLoggedIn { get { return true; } }

    public void Init() { }
    public void LogIn()
    {
        if (OnTheRunAchievements.Instance != null)
            OnTheRunAchievements.Instance.ResendProgressNotAcknowledged();
    }
    public void OnApplicationResumed() { }
    public void ReportLeaderboardTotalMeters(int meters) { }
    public void ReportLeaderboardBestMeters(int meters) { }
    public void ShowLeaderboards() { }
    public void ShowAchievements() { }
    public void ReportAchievementDone(string id, float perc)
    {
        //Debug.Log("###----###----###----###----### REPORT ACHIEVEMENT - id: " + id + " - perc: " + perc);
        if (OnTheRunAchievements.Instance != null)
            OnTheRunAchievements.Instance.OnAchievementProgressSent(OnTheRunSocial.Instance.GetAchievementType(id));
    }
    public string GetAchievementID(OnTheRunAchievements.AchievementType type) { return type.ToString(); }
}