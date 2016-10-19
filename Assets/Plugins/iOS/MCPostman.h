//
//  MCPostman.h
//  MCUtils
//
//  Created by Mikael Suvi on 1/17/11.
//  Copyright 2011 Miniclip. All rights reserved.
//

#import <UIKit/UIKit.h>

/*
 * WARNING: (Required Frameworks)
 *
 * storeKit.framework
 * systemConfiguration.framework
 * libc++.dylib
 */

@protocol MCPostmanDelegate <NSObject>

@optional

// Will be called if newsfeed board is about to be displayed
- (void)boardWillAppear;

// Will be called if newsfeed board was displayed
- (void)boardDidAppear;

// Will be called if newsfeed board is about to be dismissed
- (void)boardWillDisappear;

// Will be called if newsfeed board is about to be dismissed
- (void)boardDidDisappear;

// Will be called if newsfeed becomes available or unavailable
// User can show or hide news button when this function is called
- (void)availabilityChanged:(BOOL)availability;

// Will be sent if new message is received
- (void)nrOfUnreadMessagesChanged:(int)nrOfUnreadMessages;

// Will be called if urgent message is received and should be shown
// Urgent message is shown once per session and no earlier than after a certain
// delay from session start. The delay is defined in server-side.
// If NO is returned, library retries the call after every 20 seconds.
- (BOOL)shouldShowUrgentMessage;

// Sent to the application in the beginning of launch when database records indicate
// more recent version is available than the usre currently has.
- (void)newApplicationVersionAvailable:(NSString*)version;

// If the client has any Analytics tracking, it should implement this method and log these events there
// the newsfeed will try and send relevant information to be tracked
// Ignore, if you dont have any analytics integration
- (void)logEvent:(NSString *)eventName withParameters:(NSDictionary *)parameters;

@end

@interface MCPostman : NSObject

// Initialize Newsfeed library with delegate and initial orientation
// MCDeveloperKey in Info.plist must be specified
+ (void)startWithDelegate:(id<MCPostmanDelegate>)delegate;

// Deprecated, use startWithDelegate: instead
+ (void)startWithDelegate:(id<MCPostmanDelegate>)delegate andOrientation:(UIInterfaceOrientation)orientation __attribute__((deprecated));

// Call when it is safe for the MCPostman to start logging analytics events.
// Call this as soon as you initialize the Analytics library so that no events are lost
+ (void)startEventLogging;

// Displays newsfeed board
+ (BOOL)showBoard;

// Displays urgent messages board
+ (BOOL)showUrgentBoard;

// Dismisses the newsfeed board
+ (void)dismissBoard;

// Returns the number of unread messages
+ (int)nrOfUnreadMessages;

// Returns the number of unread messages
+ (int)nrOfUnreadUrgentMessages;

// Returns the number of total messages
+ (int)nrOfMessages;

// Enables/disables sandbox mode. 
// In sandbox news are fetched every 10 seconds
// In real environment it is fetched after every 3 hours
// Make sure you use non-sandbox mode for release builds.
+ (void)setSandBox:(BOOL)sandBoxMode;

// Give MCPostman library to react to push notifications that caused the app launch
+ (void)setLaunchOptions:(NSDictionary *)launchOptions;

// Sets the remote notifications token
+ (void)setRemoteNotificationToken:(NSData*)deviceToken;

// Processes remote notification and initiates newsfeed poll if needed
+ (void)didReceiveRemoteNotification:(NSDictionary*)userDict;

// Set to YES if library should set application badge to
// the number of unread messages upon exit
+ (void)setShowBadge:(BOOL)showBadge;

// Set to NO if the Newsfeed isnt supposed to show any urgent messages
+ (void)setShouldShowMessages:(BOOL)value;

// Set to NO if the Newsfeed isnt supposed to log any messages
+ (void)setShouldLog:(BOOL)value;


@end