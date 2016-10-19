//
//  MCWebPageViewerProtocol.h
//  MCUtils
//
//  Created by Administrator on 1/16/13.
//
//

@protocol MCWebpageViewerProtocol <NSObject>

@required


@optional
//override to change the default "Fetching data..." msg
-(NSString*)loadingContentMessage;

//Localization stuff
//Override to define the text on the back button.
-(NSString*)backButtonText;
-(NSString*)noInternetConnectionTitle;
-(NSString*)noInternetConnectionText;
-(NSString*)noInternetConnectionOKButtonText;
-(NSString*)failedToRetrieveContentText;

//Override to define how more games events will be loged.
-(void)logEvent:(NSString*)name andParameters:(NSDictionary*)params;

//If this is defined it will be called once the back button has been pressed and exit animation has ended
-(void)backCallback;

//If this is defined it will be called if the user needs internet but has no active connection
-(void)noInternetCallback;


@end
