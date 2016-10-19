#import <Foundation/Foundation.h>
#import "AdsManager.h"
//#import "cocos2d.h"
#import "MCCPS.h"
//#import "SMSManager.h"
//#import "FBInvitesManager.h"
//#import "CoinsServiceTouchAbsorvingLayer.h"
#import "CoinsService.h"
//#import "FacebookManager.h"
//#import "FacebookInviteTracker.h"
//#import "CoinsServiceMenu.h"

// ---- UNCOMMENT this so it doesnt show the coins offered ---
//#define SIMPLE_MENU

@class FBSession; //for the start method

#ifndef CCLOG
#define CCLOG(fmt, ...) \
NSLog((@"%@|" fmt), [NSString stringWithFormat: @"%24s", \
					[[[self class] description] UTF8String]], ##__VA_ARGS__)
#endif

@interface SimpleFreeCoinsManagerImpl : NSObject</*FBInvitesDelegateProtocol, */AdDelegateProtocol, /*SMSDelegateProtocol, */MCCPSPersistenceDelegate/*, UIActionSheetDelegate, FBITPersistenceDelegate*/> {
	AdsManager* adsManager;

    // REMOVE FACEBOOK FROM COINSERVICE FacebookManager* facebookManager;
    NSArray* facebookAdditionalPermissions;
    //FacebookInviteTracker* facebookInviteTracker;
    MCCPS* cloudPropertyService;
	//FBInvitesManager* fbInvitesManager;
	//SMSManager* smsManager;

	//NSURL* facebookLikePage;
    //NSString* facebookLikeObject;

    //BOOL alreadyLiked;
    //BOOL alreadyLoggedIn;


	BOOL hasBeenSetup;
    BOOL hasStarted;
	BOOL isBeingShown;

    id<FreeCoinsProtocol> delegate;

	UIViewController* viewController;

	BOOL isAlertFromVideos;

	BOOL isPortrait;//not updated if the device changes orientation while the coinservice is being shown
	BOOL isIpad3;

    //taken from the sprite size of the background sprite
    //every coordinate for positioning will be relative to this
    //CGSize backgroundContentSize;


    /*

    //Menu stuff
    bool menuIsLoaded;
    bool initialMenuSetupDone;

	CCNode* parentNode;
	CoinsServiceTouchAbsorvingLayer* touchAbsorvingLayer;
	CCNode* likeTittle;
	CCNode* adViewTittle;
	CCNode* invitesTittle;
	CoinsServiceMenu* menu;
    CoinsServiceMenu* closeMenu;

    CCLayer* parentLayer;


    //states
    LikeState likeState;*/
    AdViewState adViewState;
    /*InvitesState invitesState;

    bool smsEnabled;

    bool likeIsDoneOnFBPage;
    bool likeButtonTapped;
    */
    NSMutableDictionary* saveData;
    NSDictionary* configuration;

    int earningsSum;

    bool isInUILessMode; //if true, on show doesn't create the UI and one the first updateCoinsUI
                         //it will hide the coinsservice (updateCoinsUI is done after each action)

    bool disabledSound;

    BOOL showDialogOnVideoEnd;
    int remainingVideos;

    int mCoinsReward;

    NSString* dialogTitleCongratulations;
    NSString* dialogTitleSorry;
    NSString* dialogMessageWatchMore;
    NSString* dialogMessageCurrentlyNoMoreAvailable;
    NSString* dialogMessageCurrentlyNotAvailable;
    NSString* dialogButtonLater;
    NSString* dialogButtonWatchNow;
    NSString* dialogButtonOk;
}


+(id)sharedInstance;


-(void)         setupWithDelegate:(id<FreeCoinsProtocol>)delegate
                    configuration:(NSDictionary*)configuration
    facebookAdditionalPermissions:(NSArray*)additionalPermissions
                   viewController:(UIViewController*)viewController;

- (void) start;

-(ScreenType) getScreenType;


//-(void)show:(CCLayer*)parent;
//-(void)showDirectlyToFacebookInvites:(CCLayer*)parent;
//-(void)showDirectlyToFacebookLike:(CCLayer*)parent;
//-(void)hide;
//-(BOOL)handleOpenURL:(NSURL *)url;
-(UIViewController*)getRootViewController;

-(void)showAd;

// REMOVE FACEBOOK FROM COINSERVICE -(FacebookManager*) getFacebookManager;

-(void) setFacebookAdditionalPermissions:(NSArray*)permissions;

-(void) propellerFacebookLoginWithCache:(BOOL)allowCache;

-(NSString*) getRateItUrl;

-(BOOL) areVideoAdsAvailable;

-(void)enableDialogOnVideoEnd;
-(void)disableDialogOnVideoEnd;

-(void)setRemainingVideos:(int)numRemainingVideos;

-(void)setText:(NSString*)localizedText forType:(NSString*)textType;

-(int)getCoinsReward;

@end
