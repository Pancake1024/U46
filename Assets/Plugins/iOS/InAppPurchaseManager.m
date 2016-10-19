void UnitySendMessage(const char* gameobject, const char* message, const char* param);

#import "InAppPurchaseManager.h"
#import "SKProduct+LocalizedPrice.h"

@implementation InAppPurchaseManager

#pragma mark -
#pragma mark singleton method

+ (InAppPurchaseManager*)sharedInstance
{
    static dispatch_once_t predicate = 0;
    //__strong static id sharedObject = nil;
    static id sharedObject = nil;  //if you're not using ARC
    dispatch_once(&predicate, ^{
        //sharedObject = [[self alloc] init];
        sharedObject = [[[self alloc] init] retain]; // if you're not using ARC
    });
    return sharedObject;
}

#pragma mark -
#pragma mark SKProductsRequestDelegate methods

- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
{
    NSString* call = [NSString stringWithFormat:@"InAppPurchProductsRequestStart|%d", [response.products count]];
    UnitySendMessage("__objcdispatcher", "dispatch", call.UTF8String);

    for (NSString *invalidProductId in response.invalidProductIdentifiers)
    {
        NSString* param = [NSString stringWithFormat:@"InAppPurchInvalidProductId|%@", invalidProductId];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
    }

    productsDict = [[NSMutableDictionary alloc] init];

    int i = 0;
    for (SKProduct *product in response.products)
    {
        [productsDict setValue:product forKey:product.productIdentifier];

        NSString* param = [NSString stringWithFormat:@"InAppPurchProductId|%d|%@", i, product.productIdentifier];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
        
        param = [NSString stringWithFormat:@"InAppPurchProductTitle|%d|%@", i, product.localizedTitle];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
        
        param = [NSString stringWithFormat:@"InAppPurchProductDesc|%d|%@", i, product.localizedDescription];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);

        param = [NSString stringWithFormat:@"InAppPurchProductPrice|%d|%@", i, product.localizedPrice];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
        
        param = [NSString stringWithFormat:@"InAppPurchProductCurrencyCode|%d|%@", i, [product.priceLocale objectForKey:NSLocaleCurrencyCode]];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
        
        param = [NSString stringWithFormat:@"InAppPurchProductTrackableLocalizedPrice|%d|%f", i, [product.price doubleValue]];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);

        ++i;
    }

    UnitySendMessage("__objcdispatcher", "dispatch", "InAppPurchProductsRequestEnd");

    // finally release the reqest we alloc/init'ed in requestProductsData
    [productsRequest release];
}

- (void)request:(SKRequest *)request didFailWithError:(NSError *)error
{
    //NSLog(@"We have a error in the request");
    NSString* param = [NSString stringWithFormat:@"InAppPurchRequestError|%@", error];
    UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
}

#pragma -
#pragma Public methods

- (void)dealloc
{
    [productsDict release];
    [receiptsToValidate release];
    [productIdsToValidateByReceipt release];
    [super dealloc];
}

- (void)requestProductsData
{
    // we will release the request object in the delegate callback
    
    NSArray* productsIdList = [[[NSBundle mainBundle] infoDictionary] objectForKey:kInAppPurchaseManagerProductsList];
    NSSet* productIdentifiers = [NSSet setWithArray:productsIdList];
    
    productsRequest = [[SKProductsRequest alloc] initWithProductIdentifiers:productIdentifiers];
    productsRequest.delegate = self;
    [productsRequest start];
}

- (void)loadStoreInternal
{
    NSLog(@"vr: %d, vt: %d", validateReceipt ? 1 : 0, verifyTransaction ? 1 : 0);

    // restarts any purchases if they were interrupted last time the app was open
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    
    productIdsToValidateByReceipt = [[NSMutableDictionary alloc] initWithDictionary:[[NSUserDefaults standardUserDefaults] objectForKey:kInAppPurchaseManagerProductsToValidate] copyItems:YES];
    if (nil == productIdsToValidateByReceipt)
        productIdsToValidateByReceipt = [[NSMutableDictionary alloc] initWithCapacity:8];
    
    receiptsToValidate = [[NSMutableArray alloc] initWithArray:[[NSUserDefaults standardUserDefaults] objectForKey:kInAppPurchaseManagerReceiptsList] copyItems:YES];
    if (nil == receiptsToValidate)
        receiptsToValidate = [[NSMutableArray alloc] initWithCapacity:8];
    else if ([receiptsToValidate count] > 0 && validateReceipt)
    {
        NSData* receipt = [receiptsToValidate objectAtIndex:0];
        
        [MCReceiptValidator validateReceipt:receipt withDelegate:self];
    }
    NSLog(@"receiptsToValidate.count: %d", [receiptsToValidate count]);
    
    // get the product description (defined in early sections)
    [self requestProductsData];
}

//
// call this method once on startup
//
- (void)loadStore
{
    verifyTransaction = false;
    validateReceipt = false;

    [self loadStoreInternal];
}

- (void)loadStoreWithReceiptValidation:(BOOL)_validateReceipt
{
    verifyTransaction = false;
    validateReceipt = _validateReceipt;
    
    [self loadStoreInternal];
}

- (void)loadStoreWithReceiptValidation:(BOOL)_validateReceipt andVerifyTransaction:(BOOL)_verifyTransaction
{
    verifyTransaction = _verifyTransaction;
    validateReceipt = _validateReceipt;
    
    [self loadStoreInternal];
}

//
// call this before making a purchase
//
- (BOOL)canMakePurchases
{
    return [SKPaymentQueue canMakePayments];
}

- (void)purchaseProductWithId:(NSString *)identifier
{
    SKProduct* product = [productsDict objectForKey:identifier];
    if (nil == product)
        return;
    SKPayment *payment = [SKPayment paymentWithProductIdentifier:product.productIdentifier];
    [[SKPaymentQueue defaultQueue] addPayment:payment];
}

- (void)restorePurchases
{
    [self showPleaseWaitAlert];
    [[SKPaymentQueue defaultQueue] restoreCompletedTransactions];
}

#pragma -
#pragma Alert with spinned

- (void)showPleaseWaitAlert
{
    if (alert)
        return;

    alert = [[UIAlertView alloc] initWithTitle:@"Please wait..." message:nil delegate:self cancelButtonTitle:nil otherButtonTitles:nil];
    [alert show];
    
    UIActivityIndicatorView *indicator = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhiteLarge];
    indicator.center = CGPointMake(alert.bounds.size.width * 0.5f, alert.bounds.size.height * 0.5f);
    
    [indicator startAnimating];
    [alert addSubview:indicator];
    [indicator release];
}

- (void)hidePleaseWaitAlert
{
    [alert dismissWithClickedButtonIndex:0 animated:YES];
    [alert release];
    alert = nil;
}

#pragma -
#pragma Purchase helpers

//
// saves a record of the transaction by storing the receipt to disk
//
- (void)recordTransaction:(SKPaymentTransaction *)transaction
{
    NSRange range = [transaction.payment.productIdentifier rangeOfString:@"." options:NSBackwardsSearch];
    NSString* key = [NSString stringWithFormat:@"%@TransactionReceipt",
                     [transaction.payment.productIdentifier substringFromIndex:range.location + 1]];
    [[NSUserDefaults standardUserDefaults] setValue:transaction.transactionReceipt forKey:key];
    [[NSUserDefaults standardUserDefaults] synchronize];
}

//
// provide purchased content
//
- (void)provideContent:(NSString *)productId
           wasRestored:(BOOL)wasRestored
{
    if (wasRestored)
    {
        NSString* param = [NSString stringWithFormat:@"InAppPurchRestoreContent|%@", productId];
        NSLog(param);
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
    }
    else
    {
        NSString* param = [NSString stringWithFormat:@"InAppPurchProvideContent|%@", productId];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
    }
}

//
// removes the transaction from the queue and posts a notification with the transaction result
//
- (void)finishTransaction:(SKPaymentTransaction *)transaction wasSuccessful:(BOOL)wasSuccessful
{
    // remove the transaction from the payment queue.
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
    
    NSString* param = [NSString
                       stringWithFormat:@"InAppPurchFinishTransaction|%@",
                       wasSuccessful ? @"1" : @"0"];
    UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
}

//
// called when the transaction was successful
//
- (void)completeTransaction:(SKPaymentTransaction *)transaction
{
    [self recordTransaction:transaction];
    [self provideContent:transaction.payment.productIdentifier wasRestored:NO];
    [self finishTransaction:transaction wasSuccessful:YES];
}

//
// called when a transaction has been restored and and successfully completed
//
- (void)restoreTransaction:(SKPaymentTransaction *)transaction
{
    [self recordTransaction:transaction.originalTransaction];
    [self provideContent:transaction.originalTransaction.payment.productIdentifier wasRestored:YES];
    [self finishTransaction:transaction wasSuccessful:YES];
}

//
// called when a transaction has failed
//
- (void)failedTransaction:(SKPaymentTransaction *)transaction
{
    if (transaction.error.code != SKErrorPaymentCancelled)
    {
        // error!
        [self finishTransaction:transaction wasSuccessful:NO];
    }
    else
    {
        // this is fine, the user just cancelled, so don�t notify
        [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
    }
}

#pragma mark -
#pragma mark RRVerificationControllerDelegate methods

- (void)verificationControllerDidVerifyPurchase:(SKPaymentTransaction *)transaction wasRestored:(BOOL)wasRestored isValid:(BOOL)isValid
{
    if (isValid)
    {
    	if (wasRestored)
            [self restoreTransaction:transaction];
    	else
            [self completeTransaction:transaction];
    }
    else
        [self finishTransaction:transaction wasSuccessful:NO];
}

- (void)verificationControllerDidFailToVerifyPurchase:(SKPaymentTransaction *)transaction error:(NSError *)error
{
    NSString *message = NSLocalizedString(@"Your purchase could not be verified with Apple's servers. Please try again later.", nil);
/* nmonteiro - commented this again, just to be safe
if (error != nil) {
        //Check if there's a localized description for the error
        NSString* localizedDescription = [error localizedDescription];
        
        //Only if the message exists and is not empty do we add it to the error message
       	//nuno.monteiro NOTE: This was the real problem! Although error existed and wasn't nil,
       	//calling localizedDescription returned nil and crashed because nil value was being sent to stringWithFormat!
        if (localizedDescription != nil && ![localizedDescription isEqualToString:@""]) {
            message = [message stringByAppendingString:@"\n\n"];
            message = [message stringByAppendingFormat:NSLocalizedString(@"The error was: %@.", nil), localizedDescription];
        }
    }
*/
    [[[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Purchase Verification Failed", nil)
                                message:message
                               delegate:nil
                      cancelButtonTitle:NSLocalizedString(@"Dismiss", nil)
                      otherButtonTitles:nil] show];
}


#pragma mark -
#pragma mark ReceiptValidatorDelegate methods

-(void)handleError:(NSError*)error withReceipt:(NSData*)receipt
{
    NSLog(@"handleError: %@", error.description);

    if (RECEIPT_VALIDATOR_ERROR_IS_INVALID_RECEIPT(error.code))
    {
        NSLog(@"INVALID RECEIPT!");
        
        NSString* receiptStr = [[NSString alloc] initWithData:receipt encoding:NSUTF8StringEncoding];
        
        NSString* productId = [productIdsToValidateByReceipt objectForKey:receiptStr];
        if (nil != productId)
        {
            NSString* param = [NSString stringWithFormat:@"InAppPurchValidationFailed|%@", productId];
            UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
            
            [productIdsToValidateByReceipt removeObjectForKey:receiptStr];
            
            [[NSUserDefaults standardUserDefaults] setObject:productIdsToValidateByReceipt forKey:kInAppPurchaseManagerProductsToValidate];
            [[NSUserDefaults standardUserDefaults] synchronize];
        }
        
        int index = [receiptsToValidate indexOfObject:receipt];
        if (index != NSNotFound)
        {
            [receiptsToValidate removeObjectAtIndex:index];
            
            [[NSUserDefaults standardUserDefaults] setObject:receiptsToValidate forKey:kInAppPurchaseManagerReceiptsList];
            [[NSUserDefaults standardUserDefaults] synchronize];
        }
    }

    if ([receiptsToValidate count] > 0)
    {
        NSData* receipt = [receiptsToValidate objectAtIndex:0];
        
        [MCReceiptValidator validateReceipt:receipt withDelegate:self];
    }
}

-(void)handleErrorNoInternet:(NSError*)error withReceipt:(NSData*)receipt
{
    NSLog(@"handleErrorNoInternet: %@, error: %@", receipt.description, error.description);
}

-(void)handleSuccessWithReceipt:(NSData*)receipt
{
    NSLog(@"handleSuccessWithReceipt: %@", receipt.description);

    NSString* receiptStr = [[NSString alloc] initWithData:receipt encoding:NSUTF8StringEncoding];
    
    NSString* productId = [productIdsToValidateByReceipt objectForKey:receiptStr];
    if (nil != productId)
    {
        NSString* param = [NSString stringWithFormat:@"InAppPurchValidated|%@", productId];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
        
        [productIdsToValidateByReceipt removeObjectForKey:receiptStr];
        
        [[NSUserDefaults standardUserDefaults] setObject:productIdsToValidateByReceipt forKey:kInAppPurchaseManagerProductsToValidate];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }
    
    int index = [receiptsToValidate indexOfObject:receipt];
    if (index > -1)
    {
        [receiptsToValidate removeObjectAtIndex:index];

        [[NSUserDefaults standardUserDefaults] setObject:receiptsToValidate forKey:kInAppPurchaseManagerReceiptsList];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }

    if ([receiptsToValidate count] > 0)
    {
        NSData* receipt = [receiptsToValidate objectAtIndex:0];
        
        [MCReceiptValidator validateReceipt:receipt withDelegate:self];
    }
}

#pragma mark -
#pragma mark SKPaymentTransactionObserver methods

- (void)paymentQueueRestoreCompletedTransactionsFinished:(SKPaymentQueue *)queue
{
    [self hidePleaseWaitAlert];

    [[[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Purchases restored", nil)
                                message:@"All your previously purchased products are now available in the shop"
                               delegate:nil
                      cancelButtonTitle:NSLocalizedString(@"OK", nil)
                      otherButtonTitles:nil] show];
}

- (void)paymentQueue:(SKPaymentQueue *)queue restoreCompletedTransactionsFailedWithError:(NSError *)error
{
    [self hidePleaseWaitAlert];

    [[[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Restore Failed", nil)
                                message:error.localizedDescription
                               delegate:nil
                      cancelButtonTitle:NSLocalizedString(@"Dismiss", nil)
                      otherButtonTitles:nil] show];
}

//
// called when the transaction status is updated
//
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions
{
    [RRVerificationController sharedInstance].itcContentProviderSharedSecret = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"IAPSharedSecret"];

    for (SKPaymentTransaction *transaction in transactions)
    {
        switch (transaction.transactionState)
        {
            case SKPaymentTransactionStatePurchasing:
            {
                NSString* param = [NSString stringWithFormat:@"InAppPurchProcessing|%@", transaction.payment.productIdentifier];
                UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
                break;
            }
            case SKPaymentTransactionStatePurchased:
                //[self completeTransaction:transaction];
                if (validateReceipt)
                {
                    NSData* receipt = transaction.transactionReceipt;
                    
                    // TO TEST A FAILING VALIDATION ----->
                    //receipt = [@"For Inapp Validation Testing: an invalid receipt." dataUsingEncoding:NSUTF8StringEncoding];
                    // -----> TO TEST A FAILING VALIDATION
                    
                    NSString* receiptStr = [[NSString alloc] initWithData:receipt encoding:NSUTF8StringEncoding];
                    
                    [productIdsToValidateByReceipt setObject:transaction.payment.productIdentifier forKey:receiptStr];
                    [[NSUserDefaults standardUserDefaults] setObject:productIdsToValidateByReceipt forKey:kInAppPurchaseManagerProductsToValidate];
                    [[NSUserDefaults standardUserDefaults] synchronize];
                    
                    [receiptsToValidate addObject:(id)receipt];
                    [[NSUserDefaults standardUserDefaults] setObject:receiptsToValidate forKey:kInAppPurchaseManagerReceiptsList];
                    [[NSUserDefaults standardUserDefaults] synchronize];

                    [MCReceiptValidator validateReceipt:receipt withDelegate:self];
                }

                if( verifyTransaction && [ [[UIDevice currentDevice] systemVersion] compare: @"5.0" options: NSNumericSearch ] != NSOrderedAscending ) {
                    // iOS 5.0 or newer
                    if ([[RRVerificationController sharedInstance] verifyPurchase:transaction
                                                                     withDelegate:self
                                                                            error:NULL] == FALSE) {
                        [self failedTransaction:transaction];
                    }
                } else {
                    [self completeTransaction:transaction];
                }
                break;
            case SKPaymentTransactionStateFailed:
                [self failedTransaction:transaction];
                break;
            case SKPaymentTransactionStateRestored:
                //[self restoreTransaction:transaction];
                if( verifyTransaction && [ [[UIDevice currentDevice] systemVersion] compare: @"5.0" options: NSNumericSearch ] != NSOrderedAscending ) {
                    // iOS 5.0 or newer
                    if ([[RRVerificationController sharedInstance] verifyRestoredPurchase:transaction
                                                                             withDelegate:self
                                                                                    error:NULL] == FALSE) {
                        [self failedTransaction:transaction];
                    }
                } else {
                    [self restoreTransaction:transaction];
                }
                break;
            default:
                break;
        }
    }
}

- (void)paymentQueue:(SKPaymentQueue *)queue removedTransactions:(NSArray *)transactions
{
    for (SKPaymentTransaction *transaction in transactions)
    {
        NSString* param = [NSString stringWithFormat:@"InAppPurchEndProcessing|%@", transaction.payment.productIdentifier];
        UnitySendMessage("__objcdispatcher", "dispatch", param.UTF8String);
    }
}

@end
