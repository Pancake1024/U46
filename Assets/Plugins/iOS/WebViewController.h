#import "UnityAppController.h"

@interface WebViewCtrl : NSObject<UIWebViewDelegate> {
	UnityAppController* unityController;

	UIWindow* overlayWindow;
	UIActivityIndicatorView* spinner;
    BOOL unityWasPaused;
}

@property (retain) UnityAppController* unityController;

- (BOOL)unityWasPaused;

+ (WebViewCtrl*)sharedWebView;
- (void)dealloc;
- (void)closeWebUI;
- (void)openWebUIWithUrl:(NSString*)urlString
               withFrame:(CGRect)aRect
          withBackButton:(BOOL)theBackFlag
              pauseUnity:(BOOL)thePauseFlag;
- (void)startHandler:(UnityAppController*)aController;
- (void)webViewDidFinishLoad:(UIWebView*)webView;

@end
