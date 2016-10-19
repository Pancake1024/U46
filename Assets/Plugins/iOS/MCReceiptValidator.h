#import <Foundation/Foundation.h>

#define RECEIPT_VALIDATOR_ERROR_DOMAIN                  @"MCReceiptValidator Error"

/* error codes */
#define RECEIPT_VALIDATOR_RECEIPT_VALID                 0

// validation failed
#define RECEIPT_VALIDATOR_ERROR_RECEIPT_ALREADY_USED    1
#define RECEIPT_VALIDATOR_ERROR_RECEIPT_INVALID         2

// internal error validating
#define RECEIPT_VALIDATOR_ERROR_INVALID_REQUEST         3
#define RECEIPT_VALIDATOR_ERROR_INVALID_APPLE_RESPONSE  4
#define RECEIPT_VALIDATOR_ERROR_DATABASE_ISSUE          5
#define RECEIPT_VALIDATOR_ERROR_INTERNAL_ERROR          6

// use this macros to check the error code and display a message to the user, log different analytics or retry the request later on
#define RECEIPT_VALIDATOR_ERROR_IS_INVALID_RECEIPT(errorCode) (errorCode == RECEIPT_VALIDATOR_ERROR_RECEIPT_ALREADY_USED || errorCode == RECEIPT_VALIDATOR_ERROR_RECEIPT_INVALID)
#define RECEIPT_VALIDATOR_ERROR_IS_INTERNAL(errorCode) (errorCode != RECEIPT_VALIDATOR_RECEIPT_VALID && !RECEIPT_VALIDATOR_ERROR_IS_INVALID_RECEIPT(errorCode))


@protocol MCReceiptValidatorDelegate <NSObject>

-(void)handleError:(NSError*)error withReceipt:(NSData*)receipt;
-(void)handleErrorNoInternet:(NSError*)error withReceipt:(NSData*)receipt;
-(void)handleSuccessWithReceipt:(NSData*)receipt;

@end


@interface MCReceiptValidator : NSObject

+(void)validateReceipt:(NSData*)receipt withDelegate:(id<MCReceiptValidatorDelegate>)delegate;
+(void)validateReceipt:(NSData*)receipt withDelegate:(id<MCReceiptValidatorDelegate>)delegate bundleId:(NSString*)bundleId platform:(NSString*)platform;

@end
