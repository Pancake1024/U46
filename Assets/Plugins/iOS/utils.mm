#import <MediaPlayer/MPMusicPlayerController.h>
#import <AdSupport/ASIdentifierManager.h>

extern "C"
{
    bool _deviceIsPad()
    {
        return (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad);
    }
    
    bool _isMusicPlaying()
    {
        BOOL isPlaying = NO;
        MPMusicPlayerController* iPodMusicPlayer = [MPMusicPlayerController iPodMusicPlayer];
        if (iPodMusicPlayer.playbackState == MPMusicPlaybackStatePlaying)
        {
            isPlaying = YES;
        }
        NSLog(@"Music is %@.", isPlaying ? @"on" : @"off");
        return isPlaying;
    }
    
    void _consoleLog(const char* text)
    {
        NSLog(@"Unity log: %@.", [NSString stringWithUTF8String:text]);
    }
    
    // -- old NTP stuff -------------------------------------------------------------------------------
    
    const char* ntpGetNetworkDate()
    {
        NSString* str = @"";
        int len = [str length] + 1;
        char* strBuffer = static_cast<char*>(malloc(len));
        strncpy(strBuffer, str.UTF8String, len);
        return strBuffer;
    }
    
    double ntpGetTimeOffsetInSeconds()
    {
        return 0.0;
    }
    
    int ntpGetUsefulAssociationsCount()
    {
        return 0;
    }
    
    
    // -- SBS -------------------------------------------------------------------------------
    
    bool _isMassiveTestBuild()
    {
#ifdef MASSIVE_BUILD
        return true;
#else
        return false;
#endif
    }
    
    bool _isJailbroken()
    {
        FILE *f = fopen("/bin/bash", "r");
        BOOL isbash = NO;
        if (f != NULL)
        {
            //Device is jailbroken
            isbash = YES;
        }
        fclose(f);
        
        return isbash;
    }
    
    const char* getIdfa()
    {
        bool ios60orNewer = [[[UIDevice currentDevice] systemVersion] compare: @"6.0" options: NSNumericSearch] != NSOrderedAscending;
        
        NSString* idfa = @"";
        int len = 0;
        if (ios60orNewer)
        {
            idfa = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
            len = [idfa length] + 1;
        }
        
        char* stringBuffer = static_cast<char*>(malloc(len));
        strncpy(stringBuffer, idfa.UTF8String, len);
        return stringBuffer;
    }
    
    const char* _getLocale()
    {
        NSString *locale = [[NSLocale preferredLanguages] objectAtIndex:0];
        int len = [locale length] + 1;
        char* strBuffer = static_cast<char*>(malloc(len));
        strncpy(strBuffer, locale.UTF8String, len);
        return strBuffer;
    }
    
    const char* getPlistStringValue(NSString* pListKey)
    {
        NSString* pListValue = [[NSBundle mainBundle] objectForInfoDictionaryKey:pListKey];
        int len = [pListValue length] + 1;
        char* stringBuffer = static_cast<char*>(malloc(len));
        strncpy(stringBuffer, pListValue.UTF8String, len);
        return stringBuffer;
    }
    
    const char* getVersionString()
    {
        return getPlistStringValue(@"CFBundleShortVersionString");
    }
    
    const char* getRateItAppId()
    {
        return getPlistStringValue(@"RateItAppId");
    }
    
    const char* getFacebookAppId()
    {
        return getPlistStringValue(@"FacebookAppID");
    }
    
    const char*  getBuySpecialVehicleId()
    {
        return getPlistStringValue(@"BuySpecialVehicleAchievementId");
    }
    
    const char*  getMaxOutSpecialVehicleId()
    {
        return getPlistStringValue(@"MaxOutSpecialVehicleAchievementId");
    }
    const char*  getBuyCarId()
    {
        return getPlistStringValue(@"BuyCarAchievementId");
    }
    const char*  getMaxOutCarId()
    {
        return getPlistStringValue(@"MaxOutCarAchievementId");
    }
    const char*  getAllScenarioCarsId()
    {
        return getPlistStringValue(@"AllCarsStageAchievementId");
    }
    const char*  getNewScenarioUnlockedId()
    {
        return getPlistStringValue(@"NewStageUnlockedAchievementId");
    }
    const char*  getSpinDiamondId()
    {
        return getPlistStringValue(@"WinDiamondsSpinAchievementId");
    }
    const char*  getMissionEasyId()
    {
        return getPlistStringValue(@"CompleteMissionsMediumAchievementId");
    }
    
    const char*  getMissionHardId()
    {
        return getPlistStringValue(@"CompleteMissionsHardAchievementId");
    }
    
    const char*  getLevelVeteranId()
    {
        return getPlistStringValue(@"LevelVeteranAchievementId");
    }
    
    const char*  getFBLoginId()
    {
        return getPlistStringValue(@"FBLoginAchievementId");
    }
    
    const char*  getComeBackEasyId()
    {
        return getPlistStringValue(@"ComeBackAchievementId");
    }
    
    const char*  getComeBackMediumId()
    {
        return getPlistStringValue(@"FinalDailyBonusPrizeAchievementId");
    }
    
    const char*  getCheckpointEasyId()
    {
        return getPlistStringValue(@"CrossCheckpointsEasyAchievementId");
    }
    
    const char*  getCheckpointHardId()
    {
        return getPlistStringValue(@"CrossCheckpointsHardAchievementId");
    }
    
    const char*  getDestroyVehicleEasyId()
    {
        return getPlistStringValue(@"DestroyVehiclesEasyAchievementId");
    }
    
    const char*  getDestroyVehicleMediumId()
    {
        return getPlistStringValue(@"DestroyVehiclesMediumAchievementId");
    }
    
    const char*  getUseBoostEasyId()
    {
        return getPlistStringValue(@"UseBoostsEasyAchievementId");
    }
    
    const char*  getUseBoostHardId()
    {
        return getPlistStringValue(@"UseBoostHardAchievementId");
    }
    
    const char*  getMetersMediumId()
    {
        return getPlistStringValue(@"MetersMediumAchievementId");
    }
    
    const char*  getMetersHardId()
    {
        return getPlistStringValue(@"MetersHardAchievementId");
    }
    
    const char*  getCoinsMediumId()
    {
        return getPlistStringValue(@"EarnCoinsMediumAchievementId");
    }
    
    const char*  getCoinsHardId()
    {
        return getPlistStringValue(@"EarnCoinsHardAchievementId");
    }

}
