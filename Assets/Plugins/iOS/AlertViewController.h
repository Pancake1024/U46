#import "UnityAppController.h"

@interface AlertViewCtrl : NSObject<UIAlertViewDelegate> {
	UnityAppController* unityController;

    BOOL unityWasPaused;
}

@property (retain) UnityAppController* unityController;

- (BOOL)unityWasPaused;

+ (AlertViewCtrl*)sharedAlertView;
- (void)dealloc;
- (void)openAlertBoxWithTitle:(NSString*)aBoxTitle
                      message:(NSString*)aMessage
            cancelButtonTitle:(NSString*)aButtonTitle
            otherButtonTitles:(NSArray*)aList
                   pauseUnity:(BOOL)thePauseFlag;
- (void)startHandler:(UnityAppController*)aController;
- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex;

@end
