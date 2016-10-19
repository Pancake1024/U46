//
//  SPBrandEngageClient+Unity.h
//  SponsorPaySDK
//
//  Created by Daniel Barden on 15/12/14.
//  Copyright (c) 2014 SponsorPay. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SPBrandEngageClient.h"

@class SPCredentials;

@interface SPBrandEngageClient (Unity)

@property (nonatomic, strong, readonly) SPCredentials *credentials;

@end
