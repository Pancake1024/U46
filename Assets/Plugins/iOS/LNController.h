#import <Foundation/Foundation.h>

@interface LNController : NSObject
{
}

+ (LNController*) sharedLNController;

- (void)cancelAllLocalNotifications;
- (void)scheduleLocalNotification:(NSString *)text alertAction:(NSString *)action delay:(int)delay;

@end