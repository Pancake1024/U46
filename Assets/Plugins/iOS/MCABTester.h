#import <UIKit/UIKit.h>

@interface MCABTester : NSObject

+(void)start;

+(int)getValueForTest:(NSString*)testName withDefault:(int)defaultValue;
+(BOOL)isInTest:(NSString*)testName;

@end
