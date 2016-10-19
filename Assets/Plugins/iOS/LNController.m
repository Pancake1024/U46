#import "LNController.h"

@implementation LNController

+(LNController*)sharedLNController
{
	static LNController* instance;
	@synchronized(self)
	{
        if (!instance)
            instance = [[LNController alloc] init];
        
		return instance;
	}
	return instance;
}

- (void)cancelAllLocalNotifications
{
    [[UIApplication sharedApplication] cancelAllLocalNotifications];
}

- (void)scheduleLocalNotification:(NSString *)text alertAction:(NSString *)action delay:(int)delay
{
    UILocalNotification *localNotification = [[[UILocalNotification alloc] init] autorelease];
    
    if (!localNotification)
        return;
    
    NSDate *date = [NSDate date];
    NSDate *dateDalayed = [date dateByAddingTimeInterval:delay];
    
    [localNotification setFireDate:dateDalayed];
    [localNotification setTimeZone:[NSTimeZone defaultTimeZone]];

    [localNotification setAlertBody:text];
    [localNotification setAlertAction:action];
    [localNotification setHasAction:YES];
    
    //[localNotification setApplicationIconBadgeNumber:[[UIApplication sharedApplication] applicationIconBadgeNumber] + 1];
    
    [localNotification setSoundName:UILocalNotificationDefaultSoundName];
    
    [[UIApplication sharedApplication] scheduleLocalNotification:localNotification];
}

@end
