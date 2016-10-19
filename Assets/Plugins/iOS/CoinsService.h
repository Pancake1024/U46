#import <Foundation/Foundation.h>
//#import "cocos2d.h"
/*
 
 WARNING: READ THIS BEFORE USING
 
 THIS MODULE HAS THE RESPONSABILITY TO INITIALIZE FLURRY
 
 Go to the dependencies folder inside the CoinService and 
 add all the files present there as dependencies.
 
 don't include libJSON if you have MCUtils
 ALSO THIS MODULE ASSUMES you added the
 
 libstdc++.dylib
 QuartzCore.framework
 libobjc.dylib
 SystemConfiguration.framework
 MessageUI.framework
 libz.dylib
 OpenGLES.framework
 libcocos2d.a (provided by the coin service, but you can use your own)
 AddressBook.framework
 AddressBookUI.framework
 MediaPlayer.framework
 CoreTelephony.framework,
 UiKit.framework
 StoreKit.framework
 EventKit.framework
 Social.framework (set to optional)
 Accounts.framework (set to optional)
 AdSupport.framework (set to optional)
 libsqlite3.dylib
 AVFoundation.framework
 AudioToolbox.framework
 CoreMedia.framework
 CoreLocation.framework
 CFNetwork.framework
 
 ALSO the project must include the "-all_load” and “-ObjC" flags
 

 */


typedef enum {
    LikeInitialState,
    LikeNeedsLogin,LikeNeedsLoginForReward,
    LikeLoginInProgress,
    LikeCheckingIfAlreadyLiked,
    LikeNotDone,
    LikeDone,
    LikeInProgress,
    LikeFailure
} LikeState;

typedef enum {
    AdViewInitialState,
    AdViewAvailable,
    AdViewInProgress,
    AdViewUnavailable
} AdViewState;


typedef enum {
    InvitesInitialState,
    InvitesAvailable,
    InvitesInProgress,
    InvitesUnavailable
} InvitesState;

typedef enum {
    ScreenTypeHD,
    ScreenTypeSD,
    ScreenTypeiPadHD
} ScreenType;


#define LOGGED_EVENT_SHOW @"CoinsService shown"                                     // Properties: nil
#define LOGGED_EVENT_VIDEO_FINISHED @"CoinsService video finished"                  // Properties: (@"AD Provider",providerName), (@"coins", coinsEarned)
#define LOGGED_EVENT_VIDEO_CANCELED @"CoinsService video canceled"                  // Properties: nil
#define LOGGED_EVENT_VIDEO_AVAILABILITY @"CoinsService video availability"          // Properties: (providerName:, @"Available"/@"Unavailable"/@"Blocked")*
#define LOGGED_EVENT_FBLOGIN_REWARDED @"CoinsService FB login rewarded"             // Properties: (@"coins", coinsEarned)
#define LOGGED_EVENT_FBLOGIN_WITHOUT_REWARD @"CoinsService FB login without reward" // Properties: (@"coins", coinsEarned)
#define LOGGED_EVENT_FBINVITE_DONE @"CoinsService FB invite done"                   // Properties: (@"number of friends", numFriendsInvited), (@"coins", coinsEarned)
#define LOGGED_EVENT_FBINVITE_SUCCESSFUL @"CoinsService FB invite successful"       // Properties: (@"number of friends", numFriendsInvited), (@"coins", coinsEarned)
#define LOGGED_EVENT_FBINVITE_CANCELED @"CoinsService FB invite canceled"           // Properties: nil
#define LOGGED_EVENT_FBLIKE_CONFIRMED @"CoinsService FB like confirmed"             // Properties: (@"coins", coinsEarned)
#define LOGGED_EVENT_FBLIKE_NOT_CONFIRMED @"CoinsService FB like not confirmed"     // Properties: nil
#define LOGGED_EVENT_SMS_SUCCESSFUL @"CoinsService SMS successful"                  // Properties: (@"coins", coinsEarned)
#define LOGGED_EVENT_SMS_CANCELLED @"CoinsService SMS cancelled"                    // Properties: nil
#define LOGGED_EVENT_EXIT @"CoinsService Exit"                                      // Properties: (@"coins", totalEarningsOnThisSession)

#define LOGGED_EVENT_COINS_TAG @"coins"


@protocol FreeCoinsProtocol<NSObject>

@required


//*********************************************************
//********* Reward handling *******************************
//*********************************************************
//These methods are called when a specific event
//in the coin service rewards the player. In these methods
//you should reward the player, but without any visual effect, as
//that as been done by the coinservice. The ammount of coins rewarded is
//set by you in the "getActual" methods.
//*********************************************************


//called when the coinsservice detects that the user has liked the page on facebook
//here you should take the reward value returned by you in getActualNumberOfFacebookLikeCoins
//and reward the player.
-(void)handleFacebookLike;


//called when the coinsservice detects that the user has logged in on facebook
//here you should take the reward value returned by you in getActualNumberOfFacebookLoginCoins
//and reward the player. You just need to increase the currency value of the player
-(void)handleFacebookLogin;


//called when the coinsservice detects that the player has successfully viewed a video ad
//here you should take the reward value returned by you in getActualNumberOfVideoAdsCoins
//and reward the player. 
-(void)handleCloseWithReward;

//called when the coinsservice detects that the player has failed to view a video ad
//No actual implementation is required here, and this method will probably be removed.
-(void)handleCloseWithoutReward;

//called when the coinsservice detects that the player has successfully sent a number of sms
//here you should take the reward value returned by you in getActualNumberOfSMSCoins, multiply it
//by the number of sms sent, and reward the player. 
-(void)handleSMSCloseWithReward:(int)sentSMSNumber;


//called when the coinsservice detects that the player's invited facebook friends have logged in to facebook
//here you should take the reward value returned by you in getActualNumberOfFBInviteCoins, multiply it
//by the number of successful invites, and reward the player.
-(void)handleFBInvitesWithReward:(int)numberOfSuccessfulReferrals;

//*********************************************************
//********* Customization of rewards **********************
//*********************************************************
//the coinservice has a value for coins to be given on each reward event, here is where you can customize that value.
//for example, the currency value might change as the player progresses, so you should take the parameter, apply
//the transformation associated with your game and return that. Before returning the transformed (or not transformed) coins,
//you should save the value so that you can reward the player in the handle methods.
//*********************************************************

//value for each sms sent.
-(float)getActualNumberOfSMSCoins:(float)coinsRatio;

//value for successful facebook invites
-(float)getActualNumberOfFBInviteCoins:(float)coinsRatio;

//value for liking facebook
-(int)getActualNumberOfFacebookLikeCoins:(int)baseCoins;

//value for facebook login
-(int)getActualNumberOfFacebookLoginCoins:(int)baseCoins;

//value for viewing a video ad
-(int)getActualNumberOfVideoAdsCoins:(int)baseCoins;



//*********************************************************
//********* Sound disabling *******************************
//*********************************************************
//The coinsservice needs to be able to disable/enable the game sound
//when it shows the video ads. Here you can implement the sound enabling/disabling
//in your game
//*********************************************************
- (void) disableSound;
- (void) enableSound;


//*********************************************************
//********* CoinsService persistence **********************
//*********************************************************
//The coinsservice needs to save data. Since most games already have a builtin
//save system, these methods serve to integrate the coinservice saving with the same
//system. Basically the idea here is to save the dictionary provided in the saveData, and
//return that same dictionary in loadData. If you do not provide these methods, the coinsservice
//will save its data in NSUserDefaults which is easily hackable.
//*********************************************************
- (void) saveData:(NSDictionary*)dict;
- (NSDictionary*) loadData;


//*********************************************************
//********* CoinsService event logging ********************
//*********************************************************
//This method will be called when a certain event happens in the CoinsService
//You should log this event with, e.g. Flurry.
//*********************************************************
- (void) logEvent:(NSString*)eventName params:(NSDictionary*)dict;


//@optional
//*********************************************************
//********* COCOS2D customization *************************
//*********************************************************
//games usually have tweaked cocos2ds. We provide these methods
//so that you can tweak how the coinsservice uses cocos2d. 
//I advise you to check the actual implementation of these methods,
//and tweak them to make the coinsservice work.
//*********************************************************

//***** IMPORTANT METHODS

//the coinsservice currently supports three kinds of resources:
//SD - iphone/ipod non retina
//HD - iphone retina/ipad non retina
//IPAD3 - ipad retina
//
//the tweaks are usually, e.g, if your game scales in ipad3, meaning it doesn't truly support it, then return DeviceTypeHD.
-(ScreenType) getScreenType;

//this method is needed if [CCDirector winSize] retuns something weird.
//-(CGSize)getScreenSize;


//the coinsservice ignores the automatic suffix appending for different resolutions, but if things go wrong you might need to tweak this
//-(CCSprite*)getSprite:(NSString*)spriteName;

//in case labels are weird
//-(CCLabelTTF*)getLabelTTF:(NSString*)label fontName:(NSString*)fontName fontSize:(CGFloat)size;

//in case menu items are weird
/*-(CCMenuItemSprite*)getMenuItemSprite:(CCSprite*)normal
					   selectedSprite:(CCSprite*)selected
					   disabledSprite:(CCSprite*)disabled
							   target:(id)delegate
							 selector:(SEL)selector;
*/
//- (int) getTouchPriorityForCoinsServiceUI;


//FOR GRAPHICAL CUSTOMIZATION

//-(CCMenuItem*)getInvitesButtonWithDelegate:(id)delegate selector:(SEL)buttonSelector state:(InvitesState)invitesState;
//-(CCMenuItem*)getLikeButtonWithDelegate:(id)delegate selector:(SEL)buttonSelector state:(LikeState)likeState;
//-(CCMenuItem*)getAdViewButtonWithDelegate:(id)delegate selector:(SEL)buttonSelector state:(AdViewState)adViewState;
//-(CCMenuItem*)getCloseButtonWithDelegate:(id)delegate selector:(SEL)buttonSelector;

/*
- (CCNode*) getTittleWithTopText:(NSString*)topText
                      bottomText:(NSString*)botText;
- (CCNode*) getTittleWithTopText:(NSString*)topText
                      bottomText:(NSString*)botText
              fontSizeScaleShift:(CGFloat) fontScale;

-(CCNode*)getLikeTittle:(int) coins state:(LikeState) likeState;
-(CCNode*)getAdViewTittle:(int) coins state:(AdViewState) adViewState;
-(CCNode*)getInvitesTittle:(InvitesState) smsState;

-(CCNode*)getTittle;
-(CCNode*)getBackground;

-(NSString*)getFontName;
*/



//-(NSString*)getInvitesTittle;

// these methods must return a format and '%@' must exist in the format so the number of bonus is placed
//-(NSString*)getSMSBonus1FormatLabel;
//-(NSString*)getSMSBonus2FormatLabel;
//-(NSString*)getSelect50ButtonFormatLabel;

//-(NSString*)getSMSPopupTittleFormatLabel;
//-(NSString*)getAdViewPopupTittleFormatLabel;
//-(NSString*)getFBLikePopupTittleFormatLabel;
//-(NSString*)getFBLoginPopupTittleFormatLabel;


//-(NSString*)getFBInvitesPopupTextFormatLabel;
//-(NSString*)getFBInvitesPopupTittleFormatLabel;

//-(NSString*)getSMSPopupTextFormatLabel;
//-(NSString*)getFBLoginPopupTextFormatLabel;
//-(NSString*)getFBLikePopupTextFormatLabel;
//-(NSString*)getAdViewPopupTextFormatLabel;


//-(NSString*)getFBInvitesSuccessPopupTittleFormatLabel;
//-(NSString*)getFBInvitesSuccessPopupSingularTextFormatLabel;
//-(NSString*)getFBInvitesSuccessPopupPluralTextFormatLabel;
//-(NSString*)getFBInvitesSuccessPopupPluralMoreTextFormatLabel;


//-(NSString*)getInvitesSelectionTittle;
//-(NSString*)getInvitesSelectionMessage;



@end

@class FBSession;
//@class FacebookManager;

@interface CoinsService : NSObject

// this was deprecated so that we try and always use encryption to guarantee that noone changes the file
+ (void) setupWithDelegate:(id<FreeCoinsProtocol>)delegate
    configurationPlistPath:(NSString*)configurationPlistPath
facebookAdditionalPermissions:(NSArray*)additionalPermissions
            viewController:(UIViewController*)viewController DEPRECATED_ATTRIBUTE;

// this is the preferred implementation
+ (void) setupWithDelegate:(id<FreeCoinsProtocol>)delegate
             configuration:(NSDictionary*)configuration
facebookAdditionalPermissions:(NSArray*)additionalPermissions
            viewController:(UIViewController*)viewController;

+ (void) start;


//+(void)show:(CCLayer*)parent;

//+(void)showDirectlyToFacebookInvites:(CCLayer*)parent;
//+(void)showDirectlyToFacebookLike:(CCLayer*)parent;



//+(void)hide;
+(BOOL)handleOpenURL:(NSURL *)url;
+(UIViewController*)getRootViewController;


// REMOVE FACEBOOK FROM COINSERVICE +(FacebookManager*) getFacebookManager;


+(void) setFacebookAdditionalPermissions:(NSArray*)permissions;

+(void)showAd;

@end
