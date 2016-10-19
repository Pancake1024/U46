//
//  RRTransactionVerifier.h
//  Rowmote
//
//  Created by Evan Schoenberg on 7/20/12.
//
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import "RRVerificationController.h"

@interface RRTransactionVerifier : NSObject
{
	SKPaymentTransaction *transaction;
	BOOL restored;
}

@property (nonatomic, assign) SKPaymentTransaction *transaction;
@property (nonatomic, assign) BOOL restored;
@property (nonatomic, assign) NSObject<RRVerificationControllerDelegate> *delegate;
@property (nonatomic, assign) RRVerificationController *controller;

@property (nonatomic, retain) NSDictionary *originalPurchaseInfoDict;
@property (nonatomic, retain) NSMutableData *receivedData;


- (id)initWithPurchase:(SKPaymentTransaction *)inTransaction delegate:(NSObject<RRVerificationControllerDelegate> *)inVerificationDelegate controller:(RRVerificationController *)inController;
- (id)initWithRestoredPurchase:(SKPaymentTransaction *)inTransaction delegate:(NSObject<RRVerificationControllerDelegate> *)inVerificationDelegate controller:(RRVerificationController *)inController;

- (BOOL)beginVerificationWithError:(out NSError **)outError;

@end
