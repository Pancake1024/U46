#import <Foundation/Foundation.h>
#import "MCPostman.h"
#import "MCCorporateInfo.h"
#import "MCMoreGamesIOS.h"
#import "MCABTester.h"

@interface MCUtilsController : NSObject<MCPostmanDelegate>
{
    BOOL unityWasPaused;
    BOOL isInGame;
}

+ (MCUtilsController*) sharedMCUtilsController;

- (void)initWithLaunchOptions:(NSDictionary*)options;
- (void)setIngameFlag:(bool)flag;

@end
