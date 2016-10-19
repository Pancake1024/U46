#import "WebViewController.h"

extern "C"
{
	void openWebView(const char* url, float x, float y, float width, float height, bool withBackButton, bool pauseUnity)
	{
		[[WebViewCtrl sharedWebView] openWebUIWithUrl:[NSString stringWithUTF8String:url]
                                            withFrame:CGRectMake(x, y, width, height)
                                       withBackButton:withBackButton
                                           pauseUnity:pauseUnity];
	}

    void closeWebView()
    {
        [[WebViewCtrl sharedWebView] closeWebUI];
    }
}
