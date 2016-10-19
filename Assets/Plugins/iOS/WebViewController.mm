#import "WebViewController.h"

void UnityPause(bool pause);
void UnitySetAudioSessionActive(bool active);

void UnitySendTouchesBegin(NSSet* touches, UIEvent* event);
void UnitySendTouchesEnded(NSSet* touches, UIEvent* event);
void UnitySendTouchesCancelled(NSSet* touches, UIEvent* event);
void UnitySendTouchesMoved(NSSet* touches, UIEvent* event);

@interface UIUnityWindow : UIWindow {}
@end

@implementation WebViewCtrl

@synthesize unityController;

- (BOOL)unityWasPaused
{
	return unityWasPaused;
}

// Singleton accessor.
+(WebViewCtrl*)sharedWebView
{
	static WebViewCtrl* instance;
	@synchronized(self)
	{
	if (!instance)
		instance = [[WebViewCtrl alloc] init];

		return instance;
	}
	return instance;
}

- (void)startHandler:(UnityAppController*)aController;
{
	self.unityController = aController;
}

-(void)openWebUIWithUrl:(NSString*)urlString
              withFrame:(CGRect)aRect
         withBackButton:(BOOL)theBackFlag
             pauseUnity:(BOOL)thePauseFlag
{
    unityWasPaused = thePauseFlag;
    if (thePauseFlag)
    {
        UnitySetAudioSessionActive(false);
        UnityPause(true);
    }

	// alloc a new window for my web view
	overlayWindow = [[UIUnityWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]];

    if (theBackFlag)
    {
        // make a toolbar and add a button right in the center
        UIToolbar * toolbar = [[UIToolbar alloc]
                               initWithFrame:CGRectMake(aRect.origin.x,
                                                        aRect.origin.y + aRect.size.height - 49.0,
                                                        aRect.size.width,
                                                        49.0)];
        
        UIBarButtonItem * okButton = [[UIBarButtonItem alloc] initWithTitle:@"Back" style:UIBarButtonItemStyleBordered target:self action:@selector(closeWebUI)];
        UIBarButtonItem * flexiSpaceItem = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil];
        UIBarButtonItem * flexiSpaceItem2 = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil];
        
        toolbar.items	= [NSArray arrayWithObjects:flexiSpaceItem,okButton,flexiSpaceItem2,nil];
        [okButton release];
        [flexiSpaceItem release];	
        [flexiSpaceItem2 release];	
        
        // add the toolbar to the new window and release it
        [overlayWindow addSubview:toolbar];
        [toolbar release];
    }

	// make my webview, assign self as the delegate
	UIWebView * webView = [[UIWebView alloc] initWithFrame:aRect];//CGRectMake(0, 0, 320, 400.0)];
	webView.scalesPageToFit = YES;
    [webView setBackgroundColor:[UIColor clearColor]];
    [webView setOpaque: NO];
	webView.delegate = self;
	[overlayWindow addSubview:webView];

	// add a spinner so the user is not waiting looking at nothing
	spinner = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhiteLarge];
	spinner.center = CGPointMake(160.0, 240.0);
	spinner.autoresizingMask = UIViewAutoresizingFlexibleRightMargin | UIViewAutoresizingFlexibleLeftMargin |UIViewAutoresizingFlexibleTopMargin | UIViewAutoresizingFlexibleBottomMargin;
	[spinner startAnimating];
	[overlayWindow addSubview:spinner];

	// show the window
	[overlayWindow makeKeyAndVisible];

	//Create a URL object.
	NSURL *url = [NSURL URLWithString:urlString];

	//URL Requst Object
	NSURLRequest *requestObj = [NSURLRequest requestWithURL:url];

	//Load the request in the UIWebView.
	[webView loadRequest:requestObj];

	[webView release]; // the overlay window owns it now

}

// close the window and start unity back up
- (void)closeWebUI;
{
	[overlayWindow release];
	overlayWindow = nil;
    if (unityWasPaused)
    {
        UnitySetAudioSessionActive(true);
        UnityPause(false);

        unityWasPaused = NO;
    }
}

// remove our spinner.
- (void)webViewDidFinishLoad:(UIWebView *)webView;
{
	[spinner stopAnimating];
	[spinner removeFromSuperview];
	[spinner release];
	spinner = nil;
}


- (void)dealloc
{
	[unityController release];
	[overlayWindow release];
	[spinner release];
	[super dealloc];
}

@end

@implementation UIUnityWindow

// Dispatch touches events to UnityEngine
- (void) touchesBegan:(NSSet*)touches withEvent:(UIEvent*)event
{
	if (![WebViewCtrl sharedWebView].unityWasPaused)
		UnitySendTouchesBegin(touches, event);
}
- (void) touchesEnded:(NSSet*)touches withEvent:(UIEvent*)event
{
	if (![WebViewCtrl sharedWebView].unityWasPaused)
		UnitySendTouchesEnded(touches, event);
}
- (void) touchesCancelled:(NSSet*)touches withEvent:(UIEvent*)event
{
	if (![WebViewCtrl sharedWebView].unityWasPaused)
		UnitySendTouchesCancelled(touches, event);
}
- (void) touchesMoved:(NSSet*)touches withEvent:(UIEvent*)event
{
	if (![WebViewCtrl sharedWebView].unityWasPaused)
		UnitySendTouchesMoved(touches, event);
}

@end
