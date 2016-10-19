//
//  MCCPS.h
//  MCUtils
//
//  Created by Rui Campos on 11/28/12.
//  Copyright (c) 2012 miniclip. All rights reserved.
//

#import <Foundation/Foundation.h>


#define MCCPS_UPDATE_NOTIFICATION @"MCCPSUpdateNotification"
#define MCCPS_FAILURE_NOTIFICATION @"MCCPSFailureNotification"

typedef enum {
    UpdateOnSession = 0,
    UpdateOnce = 1,
    UpdateEvery = 2
} PropertyUpdatePolicy;

@class MCCPS;

@protocol MCCPSPersistenceDelegate

@required
- (void) saveData:(NSDictionary*)dict forThisService:(MCCPS*)cps;
- (NSDictionary*) loadDataForThisService:(MCCPS*)cps;


@end

@interface MCCPS : NSObject

- (id) init;
- (id) initWithPersistenceDelegate:(NSObject<MCCPSPersistenceDelegate>*) persistenceDelegate;

- (void) registerForUpdatingPropertyWithName:(NSString*)name
                          havingDefaultValue:(id)defaultValue
                             andUpdatePolicy:(PropertyUpdatePolicy)policy;

- (void) registerForUpdatingPropertyWithName:(NSString*)name
                          havingDefaultValue:(id)defaultValue
                                  beingEvery:(NSTimeInterval)timeInterval;

- (NSDate*) getLastUpdateDateOfPropertyWithName:(NSString*)propertyName;

- (BOOL) hasUpdatedPropertyWithName:(NSString*)propertyName;

- (id) getValueForPropertyNamed:(NSString*)propertyName;

- (void) forceUpdateOnAllProperties;


@end
