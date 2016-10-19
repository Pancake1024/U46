#import "CustomAppController.h"
#import "Flurry.h"
#import "ChartboostManager.h"
#import "MCUtilsManager.h"
#import "CSController.h"
#import "CoinsService.h"

IMPL_APP_CONTROLLER_SUBCLASS(CustomAppController)

@implementation CustomAppController

- (CSController*) csController
{
    return csController;
}

/*void uncaughtExceptionHandler(NSException *exception)
{
    [Flurry logError:@"Uncaught" message:@"Crash!" exception:exception];
}*/

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    [super application:application didFinishLaunchingWithOptions:launchOptions];
    
    /*[Flurry initialize];
    NSSetUncaughtExceptionHandler(&uncaughtExceptionHandler);
    [Flurry startSession:[[NSBundle mainBundle] objectForInfoDictionaryKey:@"FlurryAppID"]];*/
    
    [[MCUtilsManager sharedInstance] initWithLaunchOptions:launchOptions];
    csController = [[CSController alloc] initWithViewController:_rootController];
    
    return YES;
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    [super applicationDidBecomeActive:application];

    [[ChartboostManager sharedInstance] startSession];
    
    NSString *filename = [NSString stringWithFormat:@"%@_coins_service_configuration", [[NSBundle mainBundle] bundleIdentifier]];
    NSString* plistPath = [[NSBundle mainBundle] pathForResource:filename ofType:@"plist"];
    NSDictionary *configuration = [NSDictionary dictionaryWithContentsOfFile:plistPath];
    [CoinsService setupWithDelegate:csController
                      configuration:configuration
      facebookAdditionalPermissions:nil
                     viewController:_rootController];
}

- (void)dealloc
{
    [csController release];
    
	[super dealloc];
}

@end
