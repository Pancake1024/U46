//
//  SPUser+Unity.h
//  Unity-iPhone
//
//  Created by Daniel Barden on 23/10/14.
//
//

#import "SPUser.h"


@interface SPUser (Unity)

- (NSString *)dataWithKey:(NSString *)key;
- (NSString *)setDataWithKey:(NSString *)key value:(id)value;

@end

FOUNDATION_EXPORT NSString *const SPUserActionParam;

FOUNDATION_EXPORT NSString *const SPUserValueParam;
FOUNDATION_EXPORT NSString *const SPUserKeyParam;
FOUNDATION_EXPORT NSString *const SPUserSuccessParam;
FOUNDATION_EXPORT NSString *const SPUserErrorParam;

FOUNDATION_EXPORT NSString *const SPUserGetAction;
FOUNDATION_EXPORT NSString *const SPUserPutAction;
