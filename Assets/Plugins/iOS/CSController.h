//#import "cocos2d.h"
#import "CoinsService.h"

@interface CSController : /*CCScene*/NSObject <FreeCoinsProtocol>
{
	//CCNode* parent;

	int fbLoginNuggets;
	int fbLikeNuggets;
	int fbInviteNuggets;
	int videoAdsNuggets;
	int smsNuggets;
}

-(id)initWithViewController:(UIViewController*)viewController;

//-(void)show;

- (void)showAd;

@end
