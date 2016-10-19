//
//  UIDevice(Identifier).h
//  UIDeviceAddition
//
//  Created by Georg Kitz on 20.08.11.
//  Copyright 2011 Aurora Apps. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface UIDevice (IdentifierAddition)

/*
 * @method uniqueDeviceIdentifier
 * @description use this method when you need a unique identifier in one app.
 * It generates a hash from the MAC-address in combination with the bundle identifier
 * of your app.
 */

- (NSString *) uniqueDeviceIdentifier;

/*
 * @method uniqueGlobalDeviceIdentifier
 * @description use this method when you need a unique global identifier to track a device
 * with multiple apps. as example a advertising network will use this method to track the device
 * from different apps.
 * It generates a hash from the MAC-address only.
 */

- (NSString *) uniqueGlobalDeviceIdentifier;


/*
 * @method vendorDeviceIdentifier
 * @description use this method when you need the vendor identifier
 * @returns: uniqueVendorId of OS >= 6
 *      , nil for OS < 6
 */
- (NSString *) vendorDeviceIdentifier;


/*
 * @method advertisingUserIdentifier
 * @description use this method when you Advertising identifier
 * @returns: uniqueVendorId of OS >= 6
 *      , nil for OS < 6
 */
- (NSString *) advertisingUserIdentifier;

@end
