#import "Chartboost.h"

@interface ChartboostManager : NSObject <ChartboostDelegate>
{
	BOOL areInterstitialsLocked;
	BOOL interstitialIsVideo;
	BOOL soundDisabled;
}

+(ChartboostManager*)sharedInstance;

-(void)startSession;
-(void)lockIntertitials;
-(void)unlockInterstitials;

-(BOOL)shouldRequestInterstitialsInFirstSession;
-(BOOL)shouldDisplayInterstitial:(NSString *)location;
-(void)willDisplayVideo:(CBLocation)location;
-(void)didDisplayInterstitial:(CBLocation)location;
-(void)didDismissInterstitial:(CBLocation)location;

@end
