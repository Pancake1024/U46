#import <StoreKit/StoreKit.h>
#import "RRVerificationController.h"
#import "MCReceiptValidator.h"

#define kInAppPurchaseManagerProductsList @"ProductsList"
#define kInAppPurchaseManagerReceiptsList @"ReceiptsList"
#define kInAppPurchaseManagerProductsToValidate @"PendingTransactions"

@interface InAppPurchaseManager : NSObject <SKProductsRequestDelegate, SKPaymentTransactionObserver, RRVerificationControllerDelegate, MCReceiptValidatorDelegate>
{
    SKProductsRequest *productsRequest;
    NSMutableDictionary* productsDict;
    UIAlertView* alert;

    NSMutableArray* receiptsToValidate;
    bool verifyTransaction;
    bool validateReceipt;
    
    NSMutableDictionary* productIdsToValidateByReceipt;
}

+ (InAppPurchaseManager*)sharedInstance;

- (void)loadStore;
- (void)loadStoreWithReceiptValidation:(BOOL)_validateReceipt;
- (void)loadStoreWithReceiptValidation:(BOOL)_validateReceipt andVerifyTransaction:(BOOL)_verifyTransaction;
- (BOOL)canMakePurchases;
- (void)purchaseProductWithId:(NSString *)identifier;
- (void)restorePurchases;
- (void)dealloc;

@end
