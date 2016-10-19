#import <Foundation/Foundation.h>

/*****************************************************************************************
 * MCRefundProtocol
 *
 *
 * WARNING: (Required Frameworks)
 *
 * storeKit.framework
 * systemConfiguration.framework
 * libc++.dylib
 *
 * WARNING: you must add a row to the info.plist a row called 'URL types' with 2 items:
 *				item 0 : A dictionary with 'URL identifier' => app Bundle Id
 *				item 1 : A dictionary with 'URL Schemes' => url App name
 *
 * see http://mobiledevelopertips.com/cocoa/launching-your-own-application-via-a-custom-url-scheme.html
 *****************************************************************************************/
@protocol MCRefundProtocol <NSObject>

@required

/*
 * Callback called when the game must refund some in game currency
 * NSDictionary has the products/currency types as keys to the corresponding values
 * example:  [ "coins" => 99 , "diamonds" => 100 ] where [ key => value ]
 */
-(void)refund:(NSDictionary*)refunds forUrl:(NSURL*)url;

/*
 *callback called when the user tried to use the same refund ticket more than once
 */
-(void)refundAlreadyUsedForUrl:(NSURL*)url;

-(void)handleError:(NSError*)error forUrl:(NSURL*)url;

@end


/*****************************************************************************************
 MCRefund
 *****************************************************************************************/

@interface MCRefund : NSObject

/* Initialize CurrencyRefund library with delegate
 * TODO: something must be inserted in the info.plist under url schemas
 */
+(void)startWithDelegate:(id<MCRefundProtocol>)delegate;

/*this needs to be called from the app delegate
 */
+(BOOL)handleOpenURL:(NSURL *)url;

@end