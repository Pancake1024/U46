#import "AdsManager.h"
#import "FlurryAds.h"
#import "Flurry.h"
#import "FlurryAdDelegate.h"
//#import "AdColony.h"
//#import <AdColony/AdColony.h>
#import "AdColony/AdColony.h"
//#import  "vunglepub/vunglepub.h"
#import "MCCPS.h"

#define FLURRYADS_PRIORITY_PARAMETER @"flurryads-priority"
#define VUNGLEADS_PRIORITY_PARAMETER @"vungleads-priority"
#define ADCOLONYADS_PRIORITY_PARAMETER @"adcolonyads-priority"


typedef enum {FLURRY, VUNGLE, ADCOLONY } ADS;


@protocol ADProviderProtocol

@required
- (BOOL) isAdAvailable;
- (void) displayAd;
- (int) getPriority;
- (NSString*) getName;
- (void) start;

@end



@interface FlurryAdProvider : NSObject<ADProviderProtocol, FlurryAdDelegate> {
    
    
    NSString* mFlurryAppID;
    NSString* mFlurryAdSpace;
    int mPriority;
    BOOL mInitialized;
    
    BOOL mVideoFinished;
    
    NSObject<AdDelegateProtocol> *mDelegate;

	UIView* blackScreen;
	UIViewController* viewController;
}


- (id) initWithDelegate:(NSObject<AdDelegateProtocol>*)newDelegate
        defaultPriority:(int)defaultPriority
            flurryAppId:(NSString*)newFlurryAppId
          flurryAdSpace:(NSString*)newFlurryAdSpace
		  viewController:(UIViewController*)_viewController;
- (BOOL) isAdAvailable;
- (void) displayAd;
- (NSString*) getName;

@end


@implementation FlurryAdProvider

- (id) initWithDelegate:(NSObject<AdDelegateProtocol>*)newDelegate
        defaultPriority:(int)defaultPriority
            flurryAppId:(NSString*)newFlurryAppId
          flurryAdSpace:(NSString*)newFlurryAdSpace
		  viewController:(UIViewController*)_viewController {
    
	if ( ( self = [self init]) != nil) {
                
        mFlurryAppID = [newFlurryAppId retain];
        mFlurryAdSpace = [newFlurryAdSpace retain];
        
        mDelegate = [newDelegate retain];
        
        mPriority = defaultPriority;
        
        mVideoFinished = NO;
        
        mInitialized = NO;
        if(mPriority >= 0)
            [self initialize];
        
		blackScreen = nil;
		viewController = _viewController;
#ifdef ENABLE_MCCPS
        [[NSNotificationCenter defaultCenter]
         addObserver:self
         selector:@selector(priorityUpdated:)
         name:MCCPS_UPDATE_NOTIFICATION
         object:[mDelegate getCloudPropertyService]];
#endif
        return self;
    }
    return nil;
    
    
}

-(void) start {
#ifdef ENABLE_MCCPS
    [[mDelegate getCloudPropertyService]
     registerForUpdatingPropertyWithName:FLURRYADS_PRIORITY_PARAMETER
     havingDefaultValue:[NSNumber numberWithInt:mPriority]
     andUpdatePolicy:UpdateOnSession];
    
    
    mPriority = [[[mDelegate getCloudPropertyService] getValueForPropertyNamed:FLURRYADS_PRIORITY_PARAMETER] intValue];
#endif
}

- (void) initialize{
    
    [Flurry initialize];
#if defined(DEBUG) || defined(RELEASE)
    NSString *version = [NSString stringWithFormat:@"%@ debug", [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"]];
#else
    NSString *version = [NSString stringWithFormat:@"%@", [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"]];
#endif
    [Flurry setAppVersion:version];
    [FlurryAds initialize:[mDelegate getRootViewController]];
    
    [FlurryAds setAdDelegate:self]; //Can be set with any object
    
    [FlurryAds fetchAdForSpace:mFlurryAdSpace
                         frame:[mDelegate getRootViewController].view.frame size:FULLSCREEN];
    
    NSSetUncaughtExceptionHandler(&uncaughtExceptionHandler);
    [Flurry startSession:mFlurryAppID];
    
    mInitialized = YES;
}

void uncaughtExceptionHandler(NSException *exception) {
    [Flurry logError:@"Uncaught" message:@"Crash!" exception:exception];
}

- (void) priorityUpdated:(NSDictionary*)properties {
#ifdef ENABLE_MCCPS
    mPriority = [[[mDelegate getCloudPropertyService] getValueForPropertyNamed:FLURRYADS_PRIORITY_PARAMETER] intValue];
#endif
}

- (BOOL) isAdAvailable {
    if(mPriority < 0)
        return NO;
    
    if([FlurryAds adReadyForSpace:mFlurryAdSpace])
        return YES;
    
    [FlurryAds fetchAdForSpace:mFlurryAdSpace
                         frame:[mDelegate getRootViewController].view.frame
                          size:FULLSCREEN];
    return NO;
}
- (void) displayAd {
    
    
    mVideoFinished = NO;
    [FlurryAds displayAdForSpace:mFlurryAdSpace
                          onView:[mDelegate getRootViewController].view];
    
}

- (int) getPriority {
    return mPriority;
}
- (NSString*) getName {
    return @"Flurry";
}

-(void)dealloc{
    [mFlurryAppID release];
    [mFlurryAdSpace release];
    [mDelegate release];
	[super dealloc];
}

//FLURRY DELEGATE
- (void) spaceDidReceiveAd:(NSString*)adSpace {
    [mDelegate adsAreAvailable];
}

- (void) spaceDidFailToReceiveAd:(NSString*) adSpace error:(NSError *)error {
    
}

-(BOOL) spaceShouldDisplay:(NSString*)adSpace interstitial:(BOOL)interstitial {
    return true;
}

- (BOOL)spaceShouldDisplay:(NSString*)adSpace  forType:(FlurryAdType)type __attribute__ ((deprecated)) {
    return true;
}

- (void) spaceDidFailToRender:(NSString *)space error:(NSError *)error {
    
    [mDelegate handleCloseWithoutRewardFromAdProvider:[self getName]];
}

- (void)spaceWillDismiss:(NSString *)adSpace interstitial:(BOOL)interstitial {
    
}

- (void)spaceDidDismiss:(NSString *)adSpace interstitial:(BOOL)interstitial {
    if(mVideoFinished)
        [mDelegate handleCloseWithRewardFromAdProvider:[self getName]];
    else
        [mDelegate handleCloseWithoutRewardFromAdProvider:[self getName]];
    [mDelegate enableSound];
}

- (void) spaceWillLeaveApplication:(NSString *)adSpace {
    
}

- (void) spaceWillExpand:(NSString *)adSpace {
    
}

- (void) spaceWillCollapse:(NSString *)adSpace {
    
}

- (void) spaceDidCollapse:(NSString *)adSpace {
    
}

- (void) spaceDidReceiveClick:(NSString*)adSpace {
    
}

/*
 called after video did not finish (did not complete)
 */
- (void) videoDidNotFinish:(NSString *)hook{
    
    mVideoFinished = NO;
}

/*
 called after video finished (completed). pass user cookies with reward info if completion is rewarded.
 */
- (void) videoDidFinish:(NSString*)hook{
    
    mVideoFinished = YES;
}




@end

@interface AdColonyProvider : NSObject<ADProviderProtocol, AdColonyDelegate, AdColonyAdDelegate/*AdColonyTakeoverAdDelegate*/> {
    
    
    NSString* mAdColonyAppId;
    NSString* mAdColonyZone;
    int mPriority;
    BOOL mInitialized;
    
    BOOL mAdColonyVideoAvailable;
    
    NSObject<AdDelegateProtocol> *mDelegate;
}


- (id) initWithDelegate:(NSObject<AdDelegateProtocol>*)newDelegate
        defaultPriority:(int)defaultPriority
          adColonyAppId:(NSString*)newAdColonyAppId
           adColonyZone:(NSString*)newAdColonyZone;
- (BOOL) isAdAvailable;
- (void) displayAd;
- (NSString*) getName;

@end


@implementation AdColonyProvider



- (id) initWithDelegate:(NSObject<AdDelegateProtocol>*)newDelegate
        defaultPriority:(int)defaultPriority
            adColonyAppId:(NSString*)newAdColonyAppId
             adColonyZone:(NSString*)newAdColonyZone{
    
	if ( ( self = [self init]) != nil) {
        
        mAdColonyAppId = [newAdColonyAppId retain];
        mAdColonyZone = [newAdColonyZone retain];
        
        mDelegate = [newDelegate retain];
        
        mPriority = defaultPriority;
        
        mInitialized = NO;
        if(mPriority >= 0)
            [self initialize];
        
#ifdef ENABLE_MCCPS
        [[NSNotificationCenter defaultCenter]
         addObserver:self
         selector:@selector(priorityUpdated:)
         name:MCCPS_UPDATE_NOTIFICATION
         object:[mDelegate getCloudPropertyService]];
#endif
        return self;
    }
    return nil;
    
    
}

-(void) start {
#ifdef ENABLE_MCCPS
    [[mDelegate getCloudPropertyService]
     registerForUpdatingPropertyWithName:ADCOLONYADS_PRIORITY_PARAMETER
     havingDefaultValue:[NSNumber numberWithInt:mPriority]
     andUpdatePolicy:UpdateOnSession];
    
    
    mPriority = [[[mDelegate getCloudPropertyService] getValueForPropertyNamed:ADCOLONYADS_PRIORITY_PARAMETER] intValue];
#endif
}

- (void) initialize{
    
    
    // Configure AdColony only once, on initial launch
	[AdColony configureWithAppID:mAdColonyAppId zoneIDs:@[mAdColonyZone] delegate:self logging:YES];
    mAdColonyVideoAvailable = NO;
    mInitialized = YES;
}


- (void) priorityUpdated:(NSDictionary*)properties {
#ifdef ENABLE_MCCPS
    mPriority = [[[mDelegate getCloudPropertyService] getValueForPropertyNamed:ADCOLONYADS_PRIORITY_PARAMETER] intValue];
#endif    
    
}
- (BOOL) isAdAvailable {
    return mPriority >= 0 && mAdColonyVideoAvailable;
}
- (void) displayAd {
    [AdColony playVideoAdForZone:mAdColonyZone
                    withDelegate:self
                withV4VCPrePopup:NO
               andV4VCPostPopup:NO];
}


- (int) getPriority {
    return mPriority;
}
- (NSString*) getName {
    return @"AdColony";
}

-(void)dealloc{
    [mAdColonyAppId release];
    [mAdColonyZone release];
    [mDelegate release];
	[super dealloc];
}

//ADCOLONY DELEGATE

- ( void ) onAdColonyAdAvailabilityChange:(BOOL)available inZone:(NSString*) zoneID {
    if(available) {
        
        mAdColonyVideoAvailable = YES;
        [mDelegate adsAreAvailable];
        
    } else {
        
        mAdColonyVideoAvailable = NO;
        
    }
}

- ( void ) onAdColonyV4VCReward:(BOOL)success currencyName:(NSString*)currencyName currencyAmount:(int)amount inZone:(NSString*)zoneID {
    
    if(success) {
        
        [mDelegate handleCloseWithRewardFromAdProvider:[self getName]];
        
    } else {
        
        [mDelegate handleCloseWithoutReward];
    }
}

- ( void ) onAdColonyAdStartedInZone:( NSString * )zoneID {
    
}


- ( void ) onAdColonyAdAttemptFinished:(BOOL)shown inZone:( NSString * )zoneID {
    if(shown) {
        
        
    } else {
        
        mAdColonyVideoAvailable = NO;
        
        [self performSelector:@selector(warnUserAboutError) withObject:nil afterDelay:0.1f];
        
        [mDelegate handleCloseWithoutReward];
    }
}

-(NSString *)adColonyApplicationID{
	return mAdColonyAppId;
}


-(NSDictionary *)adColonyAdZoneNumberAssociation{
	NSMutableDictionary* result = [NSMutableDictionary dictionaryWithCapacity:1];
	
	[result setObject:mAdColonyZone forKey:[NSNumber numberWithInt:1]];
	
	return result;
}


-(void)warnUserAboutError{
	UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"There was a problem retrieving the ad."
													message:@"Try again later!"
												   delegate:self
										  cancelButtonTitle:@"Ok"
										  otherButtonTitles:nil];
	[alert show];
	[alert release];
}
@end

/*
@interface VungleProvider : NSObject<ADProviderProtocol, VGVunglePubDelegate> {
    
    
    NSString* mVungleAppId;
    int mPriority;
    BOOL mInitialized;
    
    BOOL mVungleVideoFinished;
    
    NSObject<AdDelegateProtocol> *mDelegate;
}


- (id) initWithDelegate:(NSObject<AdDelegateProtocol>*)newDelegate
        defaultPriority:(int)defaultPriority
            vungleAppId:(NSString*)newVungleAppId;
- (BOOL) isAdAvailable;
- (void) displayAd;
- (NSString*) getName;

@end


@implementation VungleProvider



- (id) initWithDelegate:(NSObject<AdDelegateProtocol>*)newDelegate
        defaultPriority:(int)defaultPriority
            vungleAppId:(NSString*)newVungleAppId{
    
	if ( ( self = [self init]) != nil) {
        mDelegate = [newDelegate retain];
        
        mVungleAppId = [newVungleAppId retain];
        
        mPriority = defaultPriority;
        
        mInitialized = NO;
        if(mPriority >= 0)
            [self initialize];
#ifdef ENABLE_MCCPS
        [[NSNotificationCenter defaultCenter]
         addObserver:self
         selector:@selector(priorityUpdated:)
         name:MCCPS_UPDATE_NOTIFICATION
         object:[mDelegate getCloudPropertyService]];
#endif
        
        return self;
    }
    return nil;
    
    
}

-(void) start {
#ifdef ENABLE_MCCPS
    [[mDelegate getCloudPropertyService]
     registerForUpdatingPropertyWithName:VUNGLEADS_PRIORITY_PARAMETER
     havingDefaultValue:[NSNumber numberWithInt:mPriority]
     andUpdatePolicy:UpdateOnSession];
    
    
    mPriority = [[[mDelegate getCloudPropertyService] getValueForPropertyNamed:VUNGLEADS_PRIORITY_PARAMETER] intValue];
#endif
}

- (void) priorityUpdated:(NSDictionary*)properties {
#ifdef ENABLE_MCCPS
    mPriority = [[[mDelegate getCloudPropertyService] getValueForPropertyNamed:VUNGLEADS_PRIORITY_PARAMETER] intValue];
#endif    
    //if(mPriority > 0 && !mInitialized)
        [self initialize];
    
}

- (void) initialize {
    
    // start vungle publisher library
    [VGVunglePub startWithPubAppID:mVungleAppId userData:[VGUserData defaultUserData]];
    [VGVunglePub setDelegate:self];
}

- (BOOL) isAdAvailable {
    return mPriority >= 0 && [VGVunglePub adIsAvailable];
}
- (void) displayAd {
    [VGVunglePub playModalAd:[mDelegate getRootViewController] animated:YES];
}

- (int) getPriority {
    return mPriority;
}

- (NSString*) getName {
    return @"Vungle";
}

-(void)dealloc{
    [mVungleAppId release];
    [mDelegate release];
	[super dealloc];
}


//vungle delegate
-(void)vungleMoviePlayed:(VGPlayData*)playData {
    mVungleVideoFinished = [playData playedFull];
    
    
}
-(void)vungleStatusUpdate:(VGStatusData*)statusData{
    
}
-(void)vungleViewDidDisappear:(UIViewController*)viewController{
    
    if(mVungleVideoFinished)
        [mDelegate handleCloseWithRewardFromAdProvider:[self getName]];
    else
        [mDelegate handleCloseWithoutReward];
    
}
-(void)vungleViewWillAppear:(UIViewController*)viewController{
    mVungleVideoFinished = NO;
}
@end
*/


@implementation AdsManager


-(void)dealloc{
	[super dealloc];
}

-(id)initWithDelegate:(NSObject<AdDelegateProtocol>*)delegte
	   flurryAppId:(NSString*)flurryAppId
     flurryAdSpace:(NSString*)flurryAdSpace
       flurryPriority:(int)flurryPriority
	 adColonyAppId:(NSString*)colonyAppId
         adColonyZone:(NSString*)zone
       adColonyPriority:(int)adColonyPriority
          vungleAppID:(NSString*)vungleAppID
       vunglePriority:(int)vunglePriority {
    
	if ( ( self = [super init]) != nil) {
        
        
        
		delegate = delegte;
        
        adProviderSortedByPriority = [[NSMutableArray alloc] init];
        AdColonyProvider* adColony = [[AdColonyProvider alloc] initWithDelegate:delegate
                                                               defaultPriority:adColonyPriority
                                                                 adColonyAppId:colonyAppId adColonyZone:zone];
        /*VungleProvider* vungle = [[VungleProvider alloc] initWithDelegate:delegate
                                                         defaultPriority:vunglePriority
                                                            vungleAppId:vungleAppID];*/
        
        FlurryAdProvider* flurry = [[FlurryAdProvider alloc] initWithDelegate:delegate
                                                              defaultPriority:flurryPriority
                                                                  flurryAppId:flurryAppId
                                                                flurryAdSpace:flurryAdSpace
																viewController:[delegate getRootViewController]];
        [adProviderSortedByPriority addObject:flurry];
        [adProviderSortedByPriority addObject:adColony];
        //[adProviderSortedByPriority addObject:vungle];
        
        [flurry release];
        [adColony release];
        //[vungle release];
        
		
		return self;
	}
    
	return nil;
}

-(void)start {
    for (NSObject<ADProviderProtocol> *obj in adProviderSortedByPriority) {
        [obj start];
    }
}

-(BOOL)canWatchMovie{
    for(NSObject<ADProviderProtocol> *adProvider in adProviderSortedByPriority) {
        if([adProvider isAdAvailable] && [adProvider getPriority] >= 0) {
            NSLog(@"adProvider Available: %@", [adProvider getName]);
            return YES;
        }
    }
    return NO;
}

- (NSMutableDictionary*) getAdProviderAvailability {
    NSMutableDictionary* dict = [NSMutableDictionary dictionary];
    for(NSObject<ADProviderProtocol> *adProvider in adProviderSortedByPriority) {
        if([adProvider getPriority] >= 0) {
            if([adProvider isAdAvailable])
                [dict setObject:@"Available" forKey:[adProvider getName]];
            else
                [dict setObject:@"Unavailable" forKey:[adProvider getName]];
        } else
            [dict setObject:@"Blocked" forKey:[adProvider getName]];
    }
    return dict;
}



NSInteger compareAdProvidersByPriority(id p1,
                                        id p2,
                                        void *context) {
    //make the comparaison
    if ([(NSObject<ADProviderProtocol> *)p1 getPriority] < [(NSObject<ADProviderProtocol> *)p2 getPriority])
        return  NSOrderedDescending;
    else if ([(NSObject<ADProviderProtocol> *)p1 getPriority] > [(NSObject<ADProviderProtocol> *)p2 getPriority])
        return NSOrderedAscending;
    else
        return NSOrderedSame;
    
}

-(void)watchMovie{
	[adProviderSortedByPriority sortUsingFunction:compareAdProvidersByPriority context:nil];
    for(NSObject<ADProviderProtocol>* adProvider in adProviderSortedByPriority) {
        if([adProvider getPriority] < 0 ) {
            break;
        }
        if([adProvider isAdAvailable]) {
            [adProvider displayAd];
            return;
        }
    }
    
    [delegate handleCloseWithoutReward];
    
}

@end
