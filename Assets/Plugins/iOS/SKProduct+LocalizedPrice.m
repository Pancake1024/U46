#import "SKProduct+LocalizedPrice.h"

@implementation SKProduct (LocalizedPrice)

- (NSString *)localizedPrice
{
    NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
    [numberFormatter setFormatterBehavior:NSNumberFormatterBehavior10_4];
    [numberFormatter setNumberStyle:NSNumberFormatterCurrencyStyle];
    [numberFormatter setLocale:self.priceLocale];
    NSString *formattedString = [numberFormatter stringFromNumber:self.price];
    [numberFormatter release];
    return formattedString;
}

- (NSDictionary *)proxyForJson
{
    NSMutableDictionary* dict = [NSMutableDictionary dictionary];
    [dict setValue:self.localizedDescription forKey:@"description"];
    [dict setValue:self.localizedTitle forKey:@"title"];
    [dict setValue:self.localizedPrice forKey:@"price"];
    [dict setValue:self.productIdentifier forKey:@"id"];
    return dict;
}

@end
