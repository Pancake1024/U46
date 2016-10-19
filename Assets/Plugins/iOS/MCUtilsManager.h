#import <Foundation/Foundation.h>
#import "MCPostman.h"
#import "MCCorporateInfo.h"
#import "MCMoreGamesIOS.h"
#import "MCABTester.h"

@interface MCUtilsManager : NSObject<MCPostmanDelegate>
{
    BOOL unityWasPaused;
    BOOL isInGame;
}

+ (MCUtilsManager*)sharedInstance;

- (void)initWithLaunchOptions:(NSDictionary*)options;
- (void)setIngameFlag:(bool)flag;

@end
