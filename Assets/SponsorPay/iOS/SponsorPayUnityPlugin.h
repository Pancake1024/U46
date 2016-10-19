#import "SponsorPaySDK.h"
#import "SPURLParametersProvider.h"
#import "SPLogger.h"
#import "SPInterstitialClient.h"

@interface SPUnityPluginParametersProvider : NSObject <SPURLParametersProvider>

@property (readonly) NSDictionary *pluginParameters;
@property (retain) NSString *pluginVersion;

@end

@interface SPUnityOfferWallDelegate : NSObject <SPOfferWallViewControllerDelegate>
@property (retain) SPOfferWallViewController *offerWallVC;

+ (SPUnityOfferWallDelegate *)instance;
@end

@interface SPUnityVCSDelegate : NSObject <SPVirtualCurrencyConnectionDelegate>
+ (SPUnityVCSDelegate *)instance;
@end

@interface SPUnityMBEDelegate : NSObject <SPBrandEngageClientDelegate>

@property (retain) NSMutableSet *queryingOfferSet;
@property (assign) BOOL shouldQueryVCS;
@property (copy) NSString *mbeToken;
@property (copy) NSString *currencyId;
+ (SPUnityMBEDelegate *)instance;
@end

@interface SPUnityInterstitialDelegate : NSObject <SPInterstitialClientDelegate>
@property (assign) BOOL canShowInterstitial;
+ (SPUnityInterstitialDelegate *)instance;
@end

@interface SPAdditionalParametersProvider : NSObject <SPURLParametersProvider>

@property (retain) NSDictionary *additionalParameters;

-(void)addParameters:(NSString*)json;

@end

extern "C"
{
    void _SPSetCallbackGameObjectName(const char* name);
    
    void _SPSetPluginVersion(const char* pluginVersion);
    
    const char* _SPStartSDK(const char* appId, const char* userId, const char*secretToken);
    
    void _SPLaunchOfferWall(const char* credentialsToken, const char* currencyName, const char* placmentId);
    
    void _SPSetShouldShowNotificationOnVCSCoins(int should);
    
    void _SPSendDeltaOfCoinsRequest(const char* credentialsToken, const char* currencyId);
    
    void _SPRequestBrandEngageOffers(const char* credentialsToken, const char* currencyName, int queryVCS, const char* currencyId, const char* placementId);
    
    void _SPStartBrandEngage();
    
    void _SPSetShouldShowBrandEngageRewardNotification(int should);
    
    void _SPReportActionCompletion(const char* credentialsToken, const char* name);
    
    void _SPVideoDownloadPause(bool pause);

    void _SPEnableLogging(int should);
    
    void _SPSetLogLevel(SPLogLevel logLevel);
    
    void _SPRequestIntersitialAds(const char* credentialsToken, const char* placementId);
    
    void _SPShowInterstitialAd();
    
    void _SPAddParameters(const char* json);
    
    void _SPClearParameters();

    const char* _SPUser(const char *json);

    void _SPUserReset();
}
