#import "Chartboost.h"
#import "ChartboostManager.h"

@class CbDelegate;

@interface CbDelegate : NSObject <ChartboostDelegate>
@end

@implementation CbDelegate

- (BOOL) shouldRequestInterstitialsInFirstSession {
    return NO;
}

- (BOOL)shouldDisplayInterstitial:(NSString *)location
{
    if ([[ChartboostManager sharedInstance] getAreInterstitialsLocked])
        return NO;
    else
        return YES;
}

@end

