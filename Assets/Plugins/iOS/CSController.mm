#import "CSController.h"
#import "CoinsService.h"

void UnitySendMessage(const char* gameobject, const char* message, const char* param);
void UnitySetAudioSessionActive(bool active);

@implementation CSController

-(id)initWithViewController:(UIViewController*)viewController
{
	self = [super init];

	if (self != nil)
    {
		return self;
	}
	
	return nil;
}

-(void)showAd
{
    [CoinsService showAd];
}

- (void) disableSound
{
    UnitySendMessage("__objcdispatcher", "dispatch", "csDisableSound");
}

- (void) enableSound
{
    UnitySendMessage("__objcdispatcher", "dispatch", "csEnableSound");
}

-(void)handleFacebookLike
{
    NSLog(@"REWARDED WITH COINS FOR LIKING FACEBOOK");
    NSString* call = [NSString stringWithFormat:@"csReward|%d", fbLikeNuggets];
    UnitySendMessage("__objcdispatcher", "dispatch", call.UTF8String);
}

-(void)handleFacebookLogin
{
    NSLog(@"REWARDED WITH COINS FOR LOGGING INTO FACEBOOK");
    NSString* call = [NSString stringWithFormat:@"csReward|%d", fbLoginNuggets];
    UnitySendMessage("__objcdispatcher", "dispatch", call.UTF8String);
}

-(void)handleCloseWithReward
{
    NSLog(@"REWARDED WITH COINS FOR VIDEO WATCHING");
    NSString* call = [NSString stringWithFormat:@"csReward|%d", videoAdsNuggets];
    UnitySendMessage("__objcdispatcher", "dispatch", call.UTF8String);
}

-(void)handleCloseWithoutReward
{
    NSLog(@"NO REWARDED FOR VIDEO WATCHING");
    
    UnitySendMessage("__objcdispatcher", "dispatch", "csCloseNoReward");
}

-(void)handleSMSCloseWithReward:(int)sentSMSNumber
{
    NSLog(@"REWARDED WITH COINS FOR %d SMS sent", sentSMSNumber);
    int totalNuggets = smsNuggets * sentSMSNumber;
    NSString* call = [NSString stringWithFormat:@"csReward|%d", totalNuggets];
    UnitySendMessage("__objcdispatcher", "dispatch", call.UTF8String);
}

-(void)handleFBInvitesWithReward:(int)numberOfSuccessfulReferrals
{
    NSLog(@"REWARDED WITH COINS FOR %d Successful referrals sent", numberOfSuccessfulReferrals);
    int totalNuggets = fbInviteNuggets * numberOfSuccessfulReferrals;
    NSString* call = [NSString stringWithFormat:@"csReward|%d", totalNuggets];
    UnitySendMessage("__objcdispatcher", "dispatch", call.UTF8String);
}

-(void)handleError:(NSError*)error
{
    NSLog(@"Error occured");
}

-(void)handlePopupHide
{
}

-(int)getDefaultFacebookLikeCoins
{
    return 999;
}

-(int)getDefaultFacebookLoginCoins
{
    return 999;
}

-(int)getDefaultVideoCoins
{
    return 99;
}

-(float)getDefaultSMSCoins
{
    return 9.9;
}

-(float)getDefaultFBInviteCoins
{
    return 400;
}

-(NSString*)getDefaultSMSMessage
{
    return @"Default message";
}

-(float)getActualNumberOfSMSCoins:(float)coinsRatio
{
    smsNuggets = coinsRatio;
    return smsNuggets;
}

-(float)getActualNumberOfFBInviteCoins:(float)coinsRatio
{
    fbInviteNuggets = coinsRatio;
    return fbInviteNuggets;
}

-(int)getActualNumberOfFacebookLikeCoins:(int)baseCoins
{
    fbLikeNuggets = baseCoins;
    return fbLikeNuggets;
}

-(int)getActualNumberOfFacebookLoginCoins:(int)baseCoins
{
    fbLoginNuggets = baseCoins;
    return fbLoginNuggets;
}

-(int)getActualNumberOfVideoAdsCoins:(int)baseCoins
{
    videoAdsNuggets = baseCoins;
    return videoAdsNuggets;
}

@end
