#import "MCUtilsController.h"

void UnityPause(bool pause);
void UnitySetAudioSessionActive(bool active);

void UnitySendMessage(const char* gameobject, const char* message, const char* param);

@implementation MCUtilsController

+(MCUtilsController*)sharedMCUtilsController
{
	static MCUtilsController* instance;
	@synchronized(self)
	{
		if (!instance)
			instance = [[MCUtilsController alloc] init];

		return instance;
	}
	return instance;
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

	[MCABTester start];
	[MCCorporateInfo startWithAppName:@"OnTheRun" andDelegate:nil];
	[MCMoreGames startWithAppName:@"OnTheRun" andDelegate:nil];
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
	UnitySendMessage("__objcdispatcher", "dispatch", "MCUtilsBoardWillDisappear");
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
        	UnitySendMessage("__objcdispatcher", "dispatch", "MCUtilsAvailabilityChanged|1");
        else
        	UnitySendMessage("__objcdispatcher", "dispatch", "MCUtilsAvailabilityChanged|0");
}

- (void)nrOfUnreadMessagesChanged:(int)nrOfUnreadMessages
{
	NSString* param = @"MCUtilsNewMessages|";
	param = [param stringByAppendingString:[[NSNumber numberWithInt:nrOfUnreadMessages] stringValue]];
	UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
}

- (BOOL)shouldShowUrgentMessage
{
	//UnitySendMessage("__objcdispatcher", "dispatch", "MCUtilsShouldShowUrgentMessage");
	//return YES;
	//return !isInGame;
	return NO;
}

- (void)newApplicationVersionAvailable:(NSString*)version
{
	NSString* param = @"MCUtilsNewAppAvailable|";
	param = [param stringByAppendingString:version];
	UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
}

/*
- (void)logEvent:(NSString*)eventName withParameters:(NSDictionary*)parameters
{
    [Apsalar event:eventName withArgs:parameters];
}*/

@end
