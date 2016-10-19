#import "SimpleFreeCoinsManagerImpl.h"

#import "CoinsService.h"

#define ENABLE_MCCPS

#define LAST_CONFIGURATION_UPDATE_DATE_KEY @"LAST_CONFIGURATION_UPDATE_DATE_KEY"

#define LAST_SMS_COINS_UPDATE_KEY @"LAST_SMS_COINS_UPDATE_KEY"
#define LAST_FACEBOOK_LIKE_COINS_UPDATE_KEY @"LAST_FACEBOOK_LIKE_COINS_UPDATE_KEY"
#define LAST_FACEBOOK_LOGIN_COINS_UPDATE_KEY @"LAST_FACEBOOK_LOGIN_COINS_UPDATE_KEY"
#define LAST_VIDEOS_COINS_UPDATE_KEY @"LAST_VIDEOS_COINS_UPDATE_KEY"
#define LAST_SMS_MESSAGE_UPDATE_KEY @"LAST_SMS_MESSAGE_UPDATE_KEY"

#define LIKE_FACEBOOK_KEY @"HasLikedTheObjectOnFacebook"
#define LOGIN_FACEBOOK_KEY @"HasLoggedOnFacebook"


#define SMS_PARAMETER_REQUEST @"sms-coin-reward"
#define FBINVITES_PARAMETER_REQUEST @"fbinvites-coin-reward"
#define VIDEO_ADS_PARAMETER_REQUEST @"video-ads-coin-reward"
#define FACEBOOK_LOGIN_PARAMETER_REQUEST @"facebook-like-coin-reward"
#define FACEBOOK_LIKE_PARAMETER_REQUEST @"facebook-login-coin-reward"
#define SMS_MESSAGE_PARAMETER_REQUEST @"sms-message"
#define SMS_ENABLED_PARAMETER_REQUEST @"sms-enabled"
#define RATE_URL_PARAMETER_REQUEST @"iOS7RateURLFormat"


#define CONFIG_AD_COLONY_APPID @"AdColonyAppID"
#define CONFIG_AD_COLONY_ZONEID_DBG @"AdColonyZoneID_dbg"
#define CONFIG_AD_COLONY_ZONEID @"AdColonyZoneID"
#define CONFIG_AD_COLONY_PRIORITY @"cs-adcolonyads-priority"
#define CONFIG_FACEBOOK_APPID @"FBAppID"
#define CONFIG_FACEBOOK_LIKE_OBJECT @"FBLikeObject"
#define CONFIG_FACEBOOK_LIKE_PAGE @"FBLikePage"
#define CONFIG_FACEBOOK_SCHEME_SUFFIX @"FBSchemeSuffix"
#define CONFIG_FLURRY_APPID @"FlurryAppID"
#define CONFIG_FLURRY_ZONEID @"FlurryZoneID"
#define CONFIG_FLURRY_PRIORITY @"cs-flurryads-priority"
#define CONFIG_VUNGLE_APPID @"VungleAppID"
#define CONFIG_VUNGLE_PRIORITY @"cs-vungleads-priority"
#define CONFIG_FACEBOOK_LIKE_REWARD @"cs-facebook-like-coin-reward"
#define CONFIG_FACEBOOK_LOGIN_REWARD @"cs-facebook-login-coin-reward"
#define CONFIG_FACEBOOK_INVITES_REWARD @"cs-fbinvites-coin-reward"
#define CONFIG_SMS_REWARD @"cs-sms-coin-reward"
#define CONFIG_SMS_ENABLED @"cs-sms-enabled"
#define CONFIG_SMS_MESSAGE @"cs-sms-message"
#define CONFIG_VIDEO_REWARD @"cs-video-ads-coin-reward"
#define CONFIG_RATE_URL @"cs-rate-url"


#define CS_IS_VALUE_EMPTY(str) (str == nil || [str isKindOfClass:[NSNumber class]] ? [[str stringValue] isEqualToString:@""]:[str isEqualToString:@""])

@implementation SimpleFreeCoinsManagerImpl



-(id)init{
    
    self = [super init];
    if ( self != nil ) {        
        isBeingShown = NO;
        
        hasStarted = NO;
	isPortrait = TRUE;
        isInUILessMode = NO;
        
        disabledSound = NO;
        
        showDialogOnVideoEnd = TRUE;
        remainingVideos = -1;
        
        return self;
    }
    
    return nil;
}

+(id)sharedInstance{
    
    static SimpleFreeCoinsManagerImpl *sharedSingleton;
    
    @synchronized(self){
        if (!sharedSingleton){
            sharedSingleton = [[SimpleFreeCoinsManagerImpl alloc] init];
        }
        
        return sharedSingleton;
    }
}

-(void) verifyConfigurationIntegrity {
    NSAssert(configuration != nil, @"CoinsService error: No valid configuration file supplied.");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_AD_COLONY_APPID]), @"CoinsService error: Invalid Ad colony AppID on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_AD_COLONY_ZONEID]), @"CoinsService error: Invalid Ad colony Zone on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_AD_COLONY_PRIORITY]), @"CoinsService error: Invalid Ad colony priority on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FACEBOOK_APPID]), @"CoinsService error: Invalid facebook appid on configuration");
    //NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FACEBOOK_LIKE_OBJECT]), @"CoinsService error: Invalid facebook like object on configuration");
    //NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FACEBOOK_LIKE_PAGE]), @"CoinsService error: Invalid facebook like page on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FLURRY_APPID]), @"CoinsService error: Invalid flurry appid on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FLURRY_ZONEID]), @"CoinsService error: Invalid flurry zoneid on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FLURRY_PRIORITY]), @"CoinsService error: Invalid flurry priority on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_VUNGLE_APPID]), @"CoinsService error: Invalid vungle appid on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_VUNGLE_PRIORITY]), @"CoinsService error: Invalid vungle priority on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FACEBOOK_LIKE_REWARD]), @"CoinsService error: Invalid facebook like reward on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FACEBOOK_LOGIN_REWARD]), @"CoinsService error: Invalid facebook login reward on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_FACEBOOK_INVITES_REWARD]), @"CoinsService error: Invalid facebook invites reward on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_SMS_REWARD]), @"CoinsService error: Invalid sms reward on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_SMS_ENABLED]), @"CoinsService error: Invalid sms enabled on configuration");
    //NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_SMS_MESSAGE]), @"CoinsService error: Invalid sms message on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_VIDEO_REWARD]), @"CoinsService error: Invalid video reward on configuration");
    NSAssert(!CS_IS_VALUE_EMPTY([configuration objectForKey:CONFIG_RATE_URL]), @"CoinsService error: Invalid rate it url on configuration");
}


-(void)         setupWithDelegate:(id<FreeCoinsProtocol>)_delegate
                    configuration:(NSDictionary*)iconfiguration
    facebookAdditionalPermissions:(NSArray*)additionalPermissions
                   viewController:(UIViewController*)_viewController {
    
    
    
    
    
    configuration = [iconfiguration retain];
    [self verifyConfigurationIntegrity];
    
    
    hasBeenSetup = YES;
    
    return;
    //these calls order matters
    
    viewController = _viewController;
    
    delegate = _delegate;
    
    NSString *adColonyZoneID = [configuration objectForKey:CONFIG_AD_COLONY_ZONEID];
#ifdef DEBUG
    id dbgID = [configuration objectForKey:CONFIG_AD_COLONY_ZONEID_DBG];
    if (dbgID != nil && ![dbgID isEqualToString:@""]) {
        adColonyZoneID = dbgID;
    }
#endif
    
    adsManager = [[AdsManager alloc] initWithDelegate:self
                                          flurryAppId:[configuration objectForKey:CONFIG_FLURRY_APPID]
                                        flurryAdSpace:[configuration objectForKey:CONFIG_FLURRY_ZONEID]
                                       flurryPriority:[[configuration objectForKey:CONFIG_FLURRY_PRIORITY] intValue]
                                        adColonyAppId:[configuration objectForKey:CONFIG_AD_COLONY_APPID]
                                         adColonyZone:adColonyZoneID
                                     adColonyPriority:[[configuration objectForKey:CONFIG_AD_COLONY_PRIORITY] intValue]
                                          vungleAppID:[configuration objectForKey:CONFIG_VUNGLE_APPID]
                                       vunglePriority:[[configuration objectForKey:CONFIG_VUNGLE_PRIORITY] intValue]];
    
    
    adViewState = AdViewAvailable;
    
    isIpad3 = [self getScreenType] == ScreenTypeiPadHD;
    
    
    mCoinsReward = [[configuration objectForKey:CONFIG_VIDEO_REWARD] intValue];
}

- (void) start {
    NSAssert(hasBeenSetup, @"CoinsService error: start called without setup being called first");
    
    hasStarted = YES;
    
    
#ifdef ENABLE_MCCPS
    cloudPropertyService = [[MCCPS alloc] initWithPersistenceDelegate:self];
    [cloudPropertyService registerForUpdatingPropertyWithName:SMS_PARAMETER_REQUEST
                                           havingDefaultValue:[configuration objectForKey:CONFIG_SMS_REWARD]
                                              andUpdatePolicy:UpdateOnSession];
    
    [cloudPropertyService registerForUpdatingPropertyWithName:FBINVITES_PARAMETER_REQUEST
                                           havingDefaultValue:[configuration objectForKey:CONFIG_FACEBOOK_INVITES_REWARD]
                                              andUpdatePolicy:UpdateOnSession];
    [cloudPropertyService registerForUpdatingPropertyWithName:VIDEO_ADS_PARAMETER_REQUEST
                                           havingDefaultValue:[configuration objectForKey:CONFIG_VIDEO_REWARD]
                                              andUpdatePolicy:UpdateOnSession];
    
    [cloudPropertyService registerForUpdatingPropertyWithName:FACEBOOK_LOGIN_PARAMETER_REQUEST
                                           havingDefaultValue:[configuration objectForKey:CONFIG_FACEBOOK_LOGIN_REWARD]
                                              andUpdatePolicy:UpdateOnSession];
    [cloudPropertyService registerForUpdatingPropertyWithName:FACEBOOK_LIKE_PARAMETER_REQUEST
                                           havingDefaultValue:[configuration objectForKey:CONFIG_FACEBOOK_LIKE_REWARD]
                                              andUpdatePolicy:UpdateOnSession];
    
    
    [cloudPropertyService registerForUpdatingPropertyWithName:SMS_MESSAGE_PARAMETER_REQUEST
                                           havingDefaultValue:[configuration objectForKey:CONFIG_SMS_MESSAGE]
                                              andUpdatePolicy:UpdateOnSession];
    
    [cloudPropertyService registerForUpdatingPropertyWithName:SMS_ENABLED_PARAMETER_REQUEST
                                           havingDefaultValue:[configuration objectForKey:CONFIG_SMS_ENABLED]
                                              andUpdatePolicy:UpdateOnSession];
    
    [cloudPropertyService registerForUpdatingPropertyWithName:RATE_URL_PARAMETER_REQUEST
                                           havingDefaultValue:[configuration objectForKey:CONFIG_RATE_URL]
                                              andUpdatePolicy:UpdateOnce];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(cloudPropertyServiceUpdate:) name:MCCPS_UPDATE_NOTIFICATION object:cloudPropertyService];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(cloudPropertyServiceFailure:) name:MCCPS_FAILURE_NOTIFICATION object:cloudPropertyService];
#endif
    
    [adsManager start];
}

-(void)dealloc{
    [adsManager release];
    
    if(saveData != nil)
        [saveData release];
    [configuration release];
    
    [super dealloc];
}

-(BOOL)handleOpenURL:(NSURL *)url{
    return NO;
}

- (void) logEvent:(NSString*)eventName params:(NSDictionary *)dict {
    NSNumber* coins = [dict objectForKey:LOGGED_EVENT_COINS_TAG];
    if(coins != nil) earningsSum += [coins intValue];
    if ( delegate != nil && [delegate respondsToSelector:@selector(logEvent:params:)]) {
        [delegate performSelector:@selector(logEvent:params:) withObject:eventName withObject:dict];
        return;
    }
    NSLog(@"CoinsService Event <%@> params: %@", eventName, dict);
}

#pragma mark --
#pragma mark ButtonHandlers

-(void)showAd{//adViewButtonHandler{
    [self logEvent:LOGGED_EVENT_VIDEO_AVAILABILITY params:[adsManager getAdProviderAvailability]];
    if ( [adsManager canWatchMovie] ){
        adViewState = AdViewInProgress;
        //[[CCDirector sharedDirector] pause];
        [self disableSound];
        [adsManager watchMovie];
    }else {
        
        UIAlertView* dialog = [[UIAlertView alloc] init];
        [dialog setDelegate:self];
        
        //[dialog setTitle:@"Sorry!"];
        //[dialog setMessage:@"There are currently no video ads available"];
        //[dialog addButtonWithTitle:@"Ok"];
        
	[dialog setTitle:dialogTitleSorry];
	[dialog setMessage:dialogMessageCurrentlyNotAvailable];
        [dialog addButtonWithTitle:dialogButtonOk];
        
        [dialog show];
        [dialog release];
        adViewState = AdViewUnavailable;
        //[self updateCoinsUI];
        
    }
}

-(BOOL) areVideoAdsAvailable {
    if (adsManager != nil)
        return [adsManager canWatchMovie];
    else
        return FALSE;
}


-(void)enableDialogOnVideoEnd {
    showDialogOnVideoEnd = TRUE;
}

-(void)disableDialogOnVideoEnd {
    showDialogOnVideoEnd = FALSE;
}

-(NSString*) getRateItUrl {
    NSString* rateItAppId = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"RateItAppId"];
    
    NSString* rateURL = @"itms-apps://ax.itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id="; // This is iOS 6 rateIt url
    rateURL = [rateURL stringByAppendingString:rateItAppId];
    
#ifdef ENABLE_MCCPS
    if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 7.0) {
        NSString *rateString = [[cloudPropertyService getValueForPropertyNamed:RATE_URL_PARAMETER_REQUEST] description];
        rateString = [rateString stringByReplacingOccurrencesOfString:@"##LANGUAGE##" withString:[[[NSLocale preferredLanguages] objectAtIndex:0] description]];
        rateString = [rateString stringByReplacingOccurrencesOfString:@"##APPID##" withString:rateItAppId];
        
        rateURL = rateString;
    }
#endif
    
    return rateURL;
}

-(void)setRemainingVideos:(int)numRemainingVideos {
    remainingVideos = numRemainingVideos;
}

-(void)setText:(NSString*)localizedText forType:(NSString*)textType {
    
    if ([[textType lowercaseString] isEqualToString:@"titlecongratulations"])
        dialogTitleCongratulations = [localizedText retain];
    else if ([[textType lowercaseString] isEqualToString:@"titlesorry"])
        dialogTitleSorry = [localizedText retain];
    else if ([[textType lowercaseString] isEqualToString:@"messagewatchmore"])
        dialogMessageWatchMore = [localizedText retain];
    else if ([[textType lowercaseString] isEqualToString:@"messagecurrentlynomoreavailable"])
        dialogMessageCurrentlyNoMoreAvailable = [localizedText retain];
    else if ([[textType lowercaseString] isEqualToString:@"messagecurrentlynotavailable"])
        dialogMessageCurrentlyNotAvailable = [localizedText retain];
    else if ([[textType lowercaseString] isEqualToString:@"buttonlater"])
        dialogButtonLater = [localizedText retain];
    else if ([[textType lowercaseString] isEqualToString:@"buttonwatchnow"])
        dialogButtonWatchNow = [localizedText retain];
    else if ([[textType lowercaseString] isEqualToString:@"buttonok"])
        dialogButtonOk = [localizedText retain];
}

-(int) getCoinsReward {
    return mCoinsReward;
}

#pragma mark --
#pragma mark Util functions

-(ScreenType) getScreenType {
    
    if ( delegate != nil &&  [delegate respondsToSelector:@selector(getScreenType)]) {
        return [delegate getScreenType];
    }
    if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad && [SimpleFreeCoinsManagerImpl isRetinaPhone])
        return ScreenTypeiPadHD;
    if([SimpleFreeCoinsManagerImpl isRetinaPhone] || UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad)
        return ScreenTypeHD;
    else
        return ScreenTypeSD;
        
}

+(BOOL)isRetinaPhone{
    return [[UIScreen mainScreen] respondsToSelector:@selector(scale)] && [[UIScreen mainScreen] scale] == 2.0;
}

+(BOOL)isIPad{
    NSString* valueDevice = [[UIDevice currentDevice] model];
    return [valueDevice hasPrefix:@"iPad"];
}

-(NSString*) getResourceBundleFormatPath{
    switch ([self getScreenType]) {
        case ScreenTypeHD:
            return @"CoinsServiceResourcesHD.bundle/%@";
        case ScreenTypeiPadHD:
            return @"CoinsServiceResourcesIPad3.bundle/%@";
        case ScreenTypeSD:
            return @"CoinsServiceResourcesSD.bundle/%@";
    }
}


#pragma mark --
#pragma mark Customize SMS behaviour

#pragma mark --

#pragma mark --


#pragma mark CLOUD PROPERTY SERVICE
-(void) cloudPropertyServiceFailure:(NSDictionary*)dict {
    
    
    
}
-(void) cloudPropertyServiceUpdate:(NSDictionary*)dict {
        
}

-(float)getActualNumberOfFBInviteCoins:(float)baseCoins{
    if ( [delegate respondsToSelector:@selector(getActualNumberOfFBInviteCoins:)] ) {
        return [delegate getActualNumberOfFBInviteCoins:baseCoins];
    }
    return baseCoins;
}

-(float)getActualNumberOfSMSCoins:(float)baseCoins{
    if ( [delegate respondsToSelector:@selector(getActualNumberOfSMSCoins:)] ) {
        return [delegate getActualNumberOfSMSCoins:baseCoins];
    }
    return baseCoins;
}

-(int)getActualNumberOfFacebookLoginCoins:(int)baseCoins{
    if ( [delegate respondsToSelector:@selector(getActualNumberOfFacebookLoginCoins:)] ) {
        return [delegate getActualNumberOfFacebookLoginCoins:baseCoins];
    }
    return baseCoins;
}

-(int)getActualNumberOfFacebookLikeCoins:(int)baseCoins{
    if ( [delegate respondsToSelector:@selector(getActualNumberOfFacebookLikeCoins:)] ) {
        return [delegate getActualNumberOfFacebookLikeCoins:baseCoins];
    }
    return baseCoins;
}

-(int)getActualNumberOfVideoAdsCoins:(int)baseCoins{
    
    if ([delegate respondsToSelector:@selector(getActualNumberOfVideoAdsCoins:)]) {
        return [delegate getActualNumberOfVideoAdsCoins:baseCoins];
    }
    return baseCoins;
}

#pragma mark --

#pragma mark CoinsResponseProtocol
#pragma mark --

#pragma mark SMSDelegateProtocol
#pragma mark --

#pragma mark FBInvitesDelegate
#pragma mark --

#pragma mark FacebookLikeDelegate
#pragma mark --

#pragma mark AdDelegateProtocol

-(UIViewController*)getRootViewController{
    return viewController;
}

-(void)adsAreAvailable{
    
    adViewState = AdViewAvailable;
}

-(void)adsAreNotAvailable{
    adViewState = AdViewUnavailable;
}

- (NSString*) convertRewardToFancyStringThisCoinValue:(int )reward {
    
    NSString* text;
    if (reward < 1000){
        text = [NSString stringWithFormat:@"%d", reward];
    }else if (reward < 10000){
        text = [NSString stringWithFormat:@"%.1fK", reward/1000.f];
    }else if (reward < 1000000){
        text = [NSString stringWithFormat:@"%dK", reward/1000];
    }else if (reward < 10000000){
        text = [NSString stringWithFormat:@"%.1fM", reward/1000000.f];
    }else{
        text = [NSString stringWithFormat:@"%dM", reward/1000000];
    }
    return text;
}


-(void)handleCloseWithRewardFromAdProvider:(NSString*)provider{
    if (![provider isEqualToString:@"Flurry"])
		[self enableSound];
#ifdef ENABLE_MCCPS
    int coinsReward = [self getActualNumberOfVideoAdsCoins:
					  [(NSNumber*)[cloudPropertyService getValueForPropertyNamed:VIDEO_ADS_PARAMETER_REQUEST] intValue]];
#else
	int coinsReward = [self getActualNumberOfVideoAdsCoins:[[configuration objectForKey:CONFIG_VIDEO_REWARD] intValue]];
#endif
    [self logEvent:LOGGED_EVENT_VIDEO_FINISHED params:
     [NSDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:coinsReward], LOGGED_EVENT_COINS_TAG,
      provider, @"AD Provider",nil]];
    
	if ( delegate != nil && [delegate respondsToSelector:@selector(handleCloseWithReward)]) {
        [delegate performSelector:@selector(handleCloseWithReward)];
    }
    
    remainingVideos--;
    
    if (showDialogOnVideoEnd) {
        isAlertFromVideos = YES;
        if ( [adsManager canWatchMovie] && remainingVideos >= 0) {
        
            UIAlertView* dialog = [[UIAlertView alloc] init];
            [dialog setDelegate:self];
        
            //[dialog setTitle:[NSString stringWithFormat:[self getVideoPopupTittleFormatLabel],[self convertRewardToFancyStringThisCoinValue:coinsReward]]];
            //[dialog setMessage:[NSString stringWithFormat:[self getVideoPopupTextFormatLabel],[self convertRewardToFancyStringThisCoinValue:coinsReward]]];
            //[dialog addButtonWithTitle:@"Later"];
            //[dialog addButtonWithTitle:@"Watch Now!"];
            
            [dialog setTitle:dialogTitleCongratulations];
            [dialog setMessage:dialogMessageWatchMore];
            [dialog addButtonWithTitle:dialogButtonLater];
            [dialog addButtonWithTitle:dialogButtonWatchNow];
            
            [dialog show];
            [dialog release];
        
            adViewState = AdViewAvailable;
        }else {
        
            UIAlertView* dialog = [[UIAlertView alloc] init];
            [dialog setDelegate:self];
            
            //[dialog setTitle:[NSString stringWithFormat:[self getVideoPopupTittleFormatLabel],[self convertRewardToFancyStringThisCoinValue:coinsReward]]];
            //[dialog setMessage:@"Unfortunately there are no more video ads available!"];
            //[dialog addButtonWithTitle:@"Ok"];
            
            [dialog setTitle:dialogTitleCongratulations];
            [dialog setMessage:dialogMessageCurrentlyNoMoreAvailable];
            [dialog addButtonWithTitle:dialogButtonOk];
            
            [dialog show];
            [dialog release];
        
            adViewState = AdViewUnavailable;
        }
    }
        
    return;
}


-(void)handleCloseWithoutReward{
		[self enableSound];
    if ( delegate != nil && [delegate respondsToSelector:@selector(handleCloseWithoutReward)]) {
        [delegate performSelector:@selector(handleCloseWithoutReward)];
    }
    
    [self logEvent:LOGGED_EVENT_VIDEO_CANCELED params:nil];
    adViewState = AdViewAvailable;
}

-(void)handleCloseWithoutRewardFromAdProvider:(NSString *)adProvider{
    if (![adProvider isEqualToString:@"Flurry"])
        [self enableSound];
    if ( delegate != nil && [delegate respondsToSelector:@selector(handleCloseWithoutReward)]) {
        [delegate performSelector:@selector(handleCloseWithoutReward)];
    }
    
    [self logEvent:LOGGED_EVENT_VIDEO_CANCELED params:nil];
    adViewState = AdViewAvailable;
}

- (MCCPS* ) getCloudPropertyService {
    return cloudPropertyService;
}




- (void) disableSound {
    if(disabledSound)
        return;
    disabledSound = YES;
    if ( delegate != nil && [delegate respondsToSelector:@selector(disableSound)]) {
        [delegate performSelector:@selector(disableSound)];
        return;
    }
    
    return;
}
- (void) enableSound {
    if(!disabledSound)
        return;
    disabledSound = NO;
    if ( delegate != nil && [delegate respondsToSelector:@selector(enableSound)]) {
        [delegate performSelector:@selector(enableSound)];
        return;
    }
    return;
}

#pragma mark --
#pragma mark FacebookManagerDelegateProtocol

- (void) checkIfAlreadyLoggedInOrLikedOrInvitesBefore {
    
}

#pragma mark Customizable menu

#define LABEL_SIZE_RELATIVE_TO_BACKGROUND 0.050
#define LABEL_SIZE_RELATIVE_TO_BACKGROUND_IPAD3 0.030

-(NSString*)getVideoPopupTittleFormatLabel{
    
    
    if ( [delegate respondsToSelector:@selector(getVideoPopupTittleFormatLabel)] ) {
        return [delegate getAdViewPopupTittleFormatLabel];
    }
    
#ifndef SIMPLE_MENU
    return @"Congratulations! You have just been awarded %@ coins!";
#else
    return @"Congratulations! You have just been awarded coins!";
#endif
}


-(NSString*)getVideoPopupTextFormatLabel{
    
    if ( [delegate respondsToSelector:@selector(getVideoPopupTextFormatLabel)] ) {
        return [delegate getAdViewPopupTextFormatLabel];
    }
    
#ifndef SIMPLE_MENU
    return @"Do you want to watch another video for %@ more coins?";
#else
    return @"Do you want to watch another video for more coins?";
#endif
}

- (void) alertView:(UIAlertView *)alert clickedButtonAtIndex:(NSInteger)buttonIndex{
    
    if ( buttonIndex == 0 ) {
    }else {
        if (isAlertFromVideos) {
			NSTimer* timer = [NSTimer scheduledTimerWithTimeInterval:1 target:self selector:@selector(showAd/*adViewButtonHandler*/) userInfo:nil repeats:NO];
            [[NSRunLoop currentRunLoop] addTimer:timer forMode:NSDefaultRunLoopMode];
        }
    }
}

#define DEFAULT_COINSERVICE_SAVE @"DEFAULT_COINSERVICE_SAVE"


- (void) load {
    NSAssert(hasStarted, @"CoinsService error: tried to load before starting!");
    if(saveData != nil)
        return;
    
    NSDictionary* savedFile;
    if([delegate respondsToSelector:@selector(loadData)])
        savedFile = [delegate loadData];
    else
        savedFile = (NSMutableDictionary*)[[NSUserDefaults standardUserDefaults] objectForKey:DEFAULT_COINSERVICE_SAVE];
    if(savedFile != nil)
        saveData = [[NSMutableDictionary alloc] initWithDictionary:savedFile copyItems:YES];
    
    if(saveData == nil)
        saveData = [[NSMutableDictionary alloc] init];
        
}

- (void) save {
    NSAssert(hasStarted, @"CoinsService error: tried to save before starting!");
    
    if(saveData == nil)
        return;
    
    NSDictionary* savedDict = [[[NSDictionary alloc] initWithDictionary:saveData copyItems:YES] autorelease];
    if([delegate respondsToSelector:@selector(saveData:)])
        return [delegate saveData:savedDict];
	[[NSUserDefaults standardUserDefaults] setObject:savedDict
                                              forKey:DEFAULT_COINSERVICE_SAVE];
	[[NSUserDefaults standardUserDefaults] synchronize];
}

- (void) saveData:(NSDictionary*)dict forThisService:(MCCPS *)cps{
    [self load];
        
    if(saveData == nil)
        saveData = [[NSMutableDictionary alloc] init];
    [saveData setValue:dict forKey:@"cps"];
    
    
    [self save];
    
    
}

- (NSDictionary*) loadDataForThisService:(MCCPS*)cps {
    [self load];
    
    if([saveData valueForKey:@"cps"] == nil || [[saveData valueForKey:@"cps"] class] != [NSDictionary class])
        return nil;
    return [[[NSDictionary alloc] initWithDictionary:[saveData valueForKey:@"cps"] copyItems:YES] autorelease];
}

extern "C"
{
    void UnitySendMessage(const char* gameobject, const char* message, const char* param);
}

@end
