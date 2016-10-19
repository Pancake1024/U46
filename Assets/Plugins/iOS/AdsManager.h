#import <Foundation/Foundation.h>

@class VGUserData;
@class MCCPS;
@class UIViewController;

@protocol AdDelegateProtocol

@required

-(void)enableSound; // Flurry HACK
-(void)handleCloseWithRewardFromAdProvider:(NSString*)adProvider;
-(void)handleCloseWithoutReward;
-(void)handleCloseWithoutRewardFromAdProvider:(NSString*)adProvider;

-(void)adsAreAvailable;
-(UIViewController*)getRootViewController;
-(MCCPS*)getCloudPropertyService;

@end

@interface AdsManager : NSObject{
	NSObject<AdDelegateProtocol> *delegate;
    
    MCCPS* cps;
    
    NSMutableArray* adProviderSortedByPriority;
}

-(id)initWithDelegate:(NSObject<AdDelegateProtocol>*)delegte
          flurryAppId:(NSString*)flurryAppId
        flurryAdSpace:(NSString*)flurryAdSpace
       flurryPriority:(int)flurryPriority
        adColonyAppId:(NSString*)colonyAppId
         adColonyZone:(NSString*)zone
     adColonyPriority:(int)adColonyPriority
          vungleAppID:(NSString*)vungleAppID
       vunglePriority:(int)vunglePriority
	   viewController:(UIViewController*)_viewController;
-(void)start;
-(void)watchMovie;
-(BOOL)canWatchMovie;
- (NSMutableDictionary*) getAdProviderAvailability;


@end
