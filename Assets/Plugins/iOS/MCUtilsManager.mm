#import "MCUtilsManager.h"
#import "UIDevice+IdentifierAddition.h"

void UnityPause(bool pause);
void UnitySetAudioSessionActive(bool active);
void UnitySendMessage(const char* gameobject, const char* message, const char* param);

@implementation MCUtilsManager

+ (MCUtilsManager*)sharedInstance
{
    static dispatch_once_t predicate = 0;
    //__strong static id sharedObject = nil;
    static id sharedObject = nil;  //if you're not using ARC
    dispatch_once(&predicate, ^{
        //sharedObject = [[self alloc] init];
        sharedObject = [[[self alloc] init] retain]; // if you're not using ARC
    });
    return sharedObject;
}

- (void)initWithLaunchOptions:(NSDictionary*)options
{
    unityWasPaused = NO;
    isInGame = NO;

    [MCPostman setLaunchOptions:options];
#ifdef DEBUG_BUILD
    [MCPostman setSandBox:YES];
#endif
    [MCPostman startWithDelegate:self];

    //[MCABTester start];
    [MCCorporateInfo startWithAppName:@"BeastQuest" andDelegate:nil];
    [MCMoreGames startWithAppName:@"BeastQuest" andDelegate:nil];
}

- (void)setIngameFlag:(bool)flag
{
    isInGame = flag;
}

- (void)boardWillAppear
{ }

- (void)boardDidAppear
{/*
    if (!unityWasPaused)
    {
        UnitySetAudioSessionActive(false);
        UnityPause(true);

        unityWasPaused = YES;
    }*/
}

- (void)boardWillDisappear
{
    UnitySendMessage("__dispatcher", "dispatch", "MCUtilsBoardWillDisappear");
}

- (void)boardDidDisappear
{
    if (unityWasPaused)
    {
        UnitySetAudioSessionActive(true);
        UnityPause(false);

        unityWasPaused = NO;
    }
}

- (void)availabilityChanged:(BOOL)availability
{
    if (availability)
        UnitySendMessage("__dispatcher", "dispatch", "MCUtilsAvailabilityChanged|1");
    else
        UnitySendMessage("__dispatcher", "dispatch", "MCUtilsAvailabilityChanged|0");
}

- (void)nrOfUnreadMessagesChanged:(int)nrOfUnreadMessages
{
    NSString* param = @"MCUtilsNewMessages|";
    param = [param stringByAppendingString:[[NSNumber numberWithInt:nrOfUnreadMessages] stringValue]];
    UnitySendMessage("__dispatcher", "dispatch", param.UTF8String);
}

- (BOOL)shouldShowUrgentMessage
{
    //UnitySendMessage("__dispatcher", "dispatch", "MCUtilsShouldShowUrgentMessage");
    //return YES;
    //return !isInGame;
    return NO;
}

- (void)newApplicationVersionAvailable:(NSString*)version
{
    NSString* param = @"MCUtilsNewAppAvailable|";
    param = [param stringByAppendingString:version];
    UnitySendMessage("__dispatcher", "dispatch", param.UTF8String);
}

/*
- (void)logEvent:(NSString*)eventName withParameters:(NSDictionary*)parameters
{
    [Apsalar event:eventName withArgs:parameters];
}*/

@end
