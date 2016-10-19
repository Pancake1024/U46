#import "CoinsService.h"
#import "UnityAppController.h"
#import "SimpleFreeCoinsManagerImpl.h"
#import "CSController.h"
#import "MCRate.h"

extern "C"
{
    void csStart()
    {
        [CoinsService start];
    }
    
    void csShow(bool showDialogOnVideoEnd)
    {
    	// This is old, and should not be used any more.
    	// Use instead csShow_noDialogAtEnd and csShow_withDialogAtEnd
    }
    
    void csShow_noDialogAtEnd()
    {
        CSController* csController = [(UnityAppController*)[UIApplication sharedApplication].delegate csController];
        [csController showAd];
        
        [[SimpleFreeCoinsManagerImpl sharedInstance] disableDialogOnVideoEnd];
        [[SimpleFreeCoinsManagerImpl sharedInstance] setRemainingVideos:-1];
    }
    
    void csShow_withDialogAtEnd(int remainingVideos)
    {
        CSController* csController = [(UnityAppController*)[UIApplication sharedApplication].delegate csController];
        [csController showAd];
        
        [[SimpleFreeCoinsManagerImpl sharedInstance] enableDialogOnVideoEnd];
        [[SimpleFreeCoinsManagerImpl sharedInstance] setRemainingVideos:remainingVideos];
    }
    
    void csSetDialogText(char* textType, char* localizedText)
    {
        [[SimpleFreeCoinsManagerImpl sharedInstance] setText:[NSString stringWithUTF8String:localizedText]
                                                     forType:[NSString stringWithUTF8String:textType]];
    }
    
    bool csAreVideoAdsAvailable()
    {
        return [[SimpleFreeCoinsManagerImpl sharedInstance] areVideoAdsAvailable];
    }
        
    int getNumberOfVideoAdsCoins()
    {
        return [[SimpleFreeCoinsManagerImpl sharedInstance] getCoinsReward];
    }
    
    // OLD VERSION - WITH MINICLIP CLOUD PROPERTY SYSTEM
    const char* getRateItUrl()
    {
        NSString* rateURL = [[SimpleFreeCoinsManagerImpl sharedInstance] getRateItUrl];
        int len = [rateURL length] + 1;
        
        char* stringBuffer = static_cast<char*>(malloc(len));
        strncpy(stringBuffer, rateURL.UTF8String, len);
        return stringBuffer;
     }
}
