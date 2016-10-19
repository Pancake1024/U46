//
//  MCRate.h
//  MCUtils
//
//  Created by Ana Oliveira on 11/2/12.
//
//

#import <Foundation/Foundation.h>

@protocol MCRateDelegate <NSObject>

@optional

// Will be called to initialize nr of times that rate popup could appear per app version.
// Default value 3
-(uint)questionLimitPerVersion;

// Will be called to initialize minimum session active time to present rate popup
// Default value 20 seconds
-(double)minimumActiveTimePerSession;

// Will be called to customize title
-(NSString*)titleText;

// Will be called to customize message
-(NSString*)messageText;

// Will be called to customize cancel button text
-(NSString*)cancelText;

// Will be called to customize confirm button text
-(NSString*)rateText;


// Will be called if rate popup is about to be displayed
-(void)ratePopupWillAppear;

// Will be called if rate popup was displayed
-(void)ratePopupDidAppear;

// Will be called if rate popup is about to be dismissed
-(void)ratePopupWillDisappear;

// Will be called if rate popup was dismissed
-(void)ratePopupDidDisappear;


// Will be called if passes all internal rules
// 
// this method exists to customize the popup that appears, ratePopup management passes to RateDelegate
// RateDelegate will have to call the params invocations in order to maintain the correct state of Rate functionality
//
// the following methods will not be called by MCRate:
// ratePopupWillAppear, ratePopupDidAppear, ratePopupWillDisappear and ratePopupDidDisappear
-(void)showRatePopupWithOk:(NSInvocation*)rateInvocation andCancel:(NSInvocation*)cancelInvocation;


@end

@interface MCRate : NSObject

// Initializes Rate
// @param delegate is a callback object which will be called to initialize popup string values or customize rules. Provide nil if you do not care (popup will still appear with default values)
+(void)startWithDelegate:(id<MCRateDelegate>)delegate andAppId:(NSString*)appId;

// Displays rate popup if passes all the rules
// Request is dismissed otherwise
// returns YES if the popup is actually shown, returns NO otherwise
+(BOOL)showRatePopup;

// Displays rate popup if passes all 'normal' rules
// additionally checks if someone already tried to show with this id (only shows the first time id is called)
//
// Request is dismissed otherwise
// returns YES if the popup is actually shown, returns NO otherwise
+(BOOL)showRatePopupWithId:(NSString*)stringId;

+(NSURL*)rateURL;

@end
