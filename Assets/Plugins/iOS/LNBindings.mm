#import "LNController.h"

extern "C"
{
    void cancelAllLocalNotifications()
    {
        [[LNController sharedLNController] cancelAllLocalNotifications];
    }
    
    void scheduleLocalNotification(const char* text, const char* action, int delay)
    {
        [[LNController sharedLNController] scheduleLocalNotification:[NSString stringWithUTF8String:text]
                                                         alertAction:[NSString stringWithUTF8String:action]
                                                               delay:delay];
    }
}
