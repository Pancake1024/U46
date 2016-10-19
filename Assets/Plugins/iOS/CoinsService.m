#import "CoinsService.h"
#import "SimpleFreeCoinsManagerImpl.h"

@implementation CoinsService


+ (void) setupWithDelegate:(id<FreeCoinsProtocol>)delegate
    configurationPlistPath:(NSString*)configurationPlistPath
facebookAdditionalPermissions:(NSArray*)additionalPermissions
            viewController:(UIViewController*)viewController {
    
	NSDictionary *configuration = [[NSDictionary alloc] initWithContentsOfFile:configurationPlistPath];
    [self setupWithDelegate:delegate
              configuration:configuration
facebookAdditionalPermissions:additionalPermissions
             viewController:viewController];
    [configuration release];
}

+ (void) setupWithDelegate:(id<FreeCoinsProtocol>)delegate
             configuration:(NSDictionary*)configuration
facebookAdditionalPermissions:(NSArray*)additionalPermissions
            viewController:(UIViewController*)viewController {
    [[SimpleFreeCoinsManagerImpl sharedInstance] setupWithDelegate:delegate
                                                     configuration:configuration
                                     facebookAdditionalPermissions:additionalPermissions
                                                    viewController:viewController];
}

+ (void) start {
    
	[(SimpleFreeCoinsManagerImpl*)[SimpleFreeCoinsManagerImpl sharedInstance] start];
}

+ (void) showAd {
	[(SimpleFreeCoinsManagerImpl*)[SimpleFreeCoinsManagerImpl sharedInstance] showAd];
}

+(BOOL)handleOpenURL:(NSURL *)url{
	return [[SimpleFreeCoinsManagerImpl sharedInstance] handleOpenURL:url];
}

+(UIViewController*)getRootViewController{
	return [[SimpleFreeCoinsManagerImpl sharedInstance] getRootViewController];
}

@end
