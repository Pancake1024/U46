#import "UnityAppController.h"
#import "InAppPurchaseManager.h"

extern "C"
{
    void loadStore()
    {
        [[InAppPurchaseManager sharedInstance] loadStore];
    }

    void loadStore_WithReceiptValidation()   //BOOL validateReceipt, BOOL verifyTransaction)
    {
        [[InAppPurchaseManager sharedInstance] loadStoreWithReceiptValidation:YES andVerifyTransaction:NO];
    }

    bool canMakePurchases()
    {
        return [[InAppPurchaseManager sharedInstance] canMakePurchases];
    }

    void purchaseProduct(const char* identifier)
    {
        [[InAppPurchaseManager sharedInstance] purchaseProductWithId:[NSString stringWithUTF8String:identifier]];
    }

    void restorePurchases()
    {
        [[InAppPurchaseManager sharedInstance] restorePurchases];
    }
}
