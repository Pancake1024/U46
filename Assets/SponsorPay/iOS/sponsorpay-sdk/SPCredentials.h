//
//  SPCredentials.h
//  SponsorPay iOS SDK
//
//  Copyright (c) 2012 SponsorPay. All rights reserved.
//

#import <Foundation/Foundation.h>

FOUNDATION_EXPORT NSString *const SPVCSConfigCurrencyName;

@interface SPCredentials : NSObject<NSCopying>

@property (nonatomic, copy) NSString *appId;
@property (nonatomic, copy) NSString *userId;
@property (nonatomic, copy) NSString *securityToken;
@property (nonatomic, weak, readonly) NSString *credentialsToken;
@property (nonatomic, readonly) NSMutableDictionary *userConfig;

+ (SPCredentials *)credentialsWithAppId:(NSString *)appId userId:(NSString *)userId securityToken:(NSString *)securityToken;

+ (NSString *)credentialsTokenForAppId:(NSString *)appId userId:(NSString *)userId;

/**
 *  Used to determine if a given credential belongs to a publisher of advertiser
 *  @return YES if the credential belong to an advertiser. NO otherwise.
 */
- (BOOL)isAdvertiser;

@end
