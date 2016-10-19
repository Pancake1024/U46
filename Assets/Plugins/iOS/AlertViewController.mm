#import "AlertViewController.h"

void UnityPause(bool pause);
void UnitySetAudioSessionActive(bool active);

void UnitySendMessage(const char* gameobject, const char* message, const char* param);

void UnitySendTouchesBegin(NSSet* touches, UIEvent* event);
void UnitySendTouchesEnded(NSSet* touches, UIEvent* event);
void UnitySendTouchesCancelled(NSSet* touches, UIEvent* event);
void UnitySendTouchesMoved(NSSet* touches, UIEvent* event);

@interface UIUnityAlertView : UIAlertView {}
@end

@implementation AlertViewCtrl

@synthesize unityController;

- (BOOL)unityWasPaused
{
	return unityWasPaused;
}

// Singleton accessor.
+(AlertViewCtrl*)sharedAlertView
{
	static AlertViewCtrl* instance;
	@synchronized(self)
	{
	if (!instance)
		instance = [[AlertViewCtrl alloc] init];

		return instance;
	}
	return instance;
}

- (void)startHandler:(UnityAppController*)aController;
{
	self.unityController = aController;
}

- (void)openAlertBoxWithTitle:(NSString*)aBoxTitle
                      message:(NSString*)aMessage
            cancelButtonTitle:(NSString*)aButtonTitle
            otherButtonTitles:(NSArray*)aList
                   pauseUnity:(BOOL)thePauseFlag
{
    unityWasPaused = thePauseFlag;
    if (thePauseFlag)
    {
        UnitySetAudioSessionActive(false);
        UnityPause(true);
    }

    UIUnityAlertView* alert = [[UIUnityAlertView alloc] initWithTitle:aBoxTitle
                                                              message:aMessage 
                                                             delegate:self 
                                                    cancelButtonTitle:aButtonTitle
                                                    otherButtonTitles:nil];

    for (NSString* title in aList)
        [alert addButtonWithTitle:title];
    
    [alert show];
    [alert release];
}

- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    if (unityWasPaused)
    {
        UnitySetAudioSessionActive(true);
        UnityPause(false);

        unityWasPaused = NO;
    }

    NSString* param = @"alertButtonClicked|";
    param = [param stringByAppendingString:[alertView buttonTitleAtIndex:buttonIndex]];

    UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
    
    [alertView dismissWithClickedButtonIndex:-1 animated:NO];
}

- (void)dealloc
{
	[unityController release];
	[super dealloc];
}
@end

@implementation UIUnityAlertView

// Dispatch touches events to UnityEngine
- (void) touchesBegan:(NSSet*)touches withEvent:(UIEvent*)event
{
	if (![AlertViewCtrl sharedAlertView].unityWasPaused)
		UnitySendTouchesBegin(touches, event);
}
- (void) touchesEnded:(NSSet*)touches withEvent:(UIEvent*)event
{
	if (![AlertViewCtrl sharedAlertView].unityWasPaused)
		UnitySendTouchesEnded(touches, event);
}
- (void) touchesCancelled:(NSSet*)touches withEvent:(UIEvent*)event
{
	if (![AlertViewCtrl sharedAlertView].unityWasPaused)
		UnitySendTouchesCancelled(touches, event);
}
- (void) touchesMoved:(NSSet*)touches withEvent:(UIEvent*)event
{
	if (![AlertViewCtrl sharedAlertView].unityWasPaused)
		UnitySendTouchesMoved(touches, event);
}

@end
