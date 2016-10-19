//
//  MCMoreGames.h
//  GSwitch
//
//  Created by Nuno Monteiro on 10/23/10.
//  Copyright 2010 Miniclip. All rights reserved.
//

//#import "MCAsynchDataLoader.h"
//#import "MGUtils.h"

#ifndef __IPHONE_OS_VERSION_MAX_ALLOWED
#   error "this file should not be included"
#endif

#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 60000
#import <StoreKit/StoreKit.h>
#endif

#import "MCWebPageViewerProtocol.h"

/*****************************************************************************************
 MCMoreGames
 *****************************************************************************************/
@interface MCMoreGames : NSObject {
}

+(id)startWithAppName:(NSString*)name andDelegate:(id<MCWebpageViewerProtocol>)delegate;
+(void)show;
+(void)show:(UIViewController*)viewController;
+(void) hide;

@end

