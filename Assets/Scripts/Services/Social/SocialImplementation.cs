using UnityEngine;
using System.Collections;

public interface SocialImplementation
{
    bool IsLoggedIn { get; }

    void Init();
    void LogIn();
    void OnApplicationResumed();

    void ReportLeaderboardTotalMeters(int meters);
    void ReportLeaderboardBestMeters(int meters);

    void ReportAchievementDone(string id, float perc);

    void ShowLeaderboards();
    void ShowAchievements();

    string GetAchievementID(OnTheRunAchievements.AchievementType type);
}