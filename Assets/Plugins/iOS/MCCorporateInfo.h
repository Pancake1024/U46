//
//  MCCorporateInfo.h
//  MCUtils
//
//  Created by Nuno Monteiro on 14/11/12.
//  Copyright 2010 Miniclip. All rights reserved.
//


#ifndef __IPHONE_OS_VERSION_MAX_ALLOWED
#   error "this file should not be included"
#endif

#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 60000
#import <StoreKit/StoreKit.h>
#endif

#import "MCWebPageViewerProtocol.h"

/*****************************************************************************************
 MCCorporateInfo
 *****************************************************************************************/
@interface MCCorporateInfo : NSObject {
}

//Should send bundle id
+(id)startWithAppName:(NSString*)name andDelegate:(id<MCWebpageViewerProtocol>)delegate;
+(void)show;
+(void)show:(UIViewController*)viewController;
+(void) hide;
+(NSString*)infoURL;

@end

