#import "ChartboostManager.h"

@implementation ChartboostManager

+ (ChartboostManager*)sharedInstance
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

- (id)init
{
    self = [super init];

    if (self)
    {
        areInterstitialsLocked = NO;
        interstitialIsVideo = NO;
        soundDisabled = NO;
    }

    return self;
}

- (void)startSession
{/*
    Chartboost *cb = [Chartboost sharedChartboost];
    
    cb.appId = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"ChartboostAppId"];
    cb.appSignature = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"ChartboostAppSignature"];
    cb.delegate = self;
    
    [cb startSession];*/
    [Chartboost startWithAppId:[[NSBundle mainBundle] objectForInfoDictionaryKey:@"ChartboostAppId"]
                  appSignature:[[NSBundle mainBundle] objectForInfoDictionaryKey:@"ChartboostAppSignature"]
                      delegate:self];
}

- (void)lockIntertitials
{
    areInterstitialsLocked = YES;
}

- (void)unlockInterstitials
{
    areInterstitialsLocked = NO;
}

- (BOOL) shouldRequestInterstitialsInFirstSession {
    return YES; // NO;
}

- (BOOL)shouldDisplayInterstitial:(NSString *)location
{
    return !areInterstitialsLocked;
}

- (void)willDisplayVideo:(CBLocation)location
{
    interstitialIsVideo = YES;
}

- (void)didDisplayInterstitial:(CBLocation)location
{
    if (interstitialIsVideo)
    {
        UnitySendMessage("__dispatcher", "dispatch", "cbDidDisplayInterstitial|1");
        UnitySendMessage("__objcdispatcher", "dispatch", "csDisableSound");
        soundDisabled = YES;
        interstitialIsVideo = NO;
    }
    else
    {
        UnitySendMessage("__dispatcher", "dispatch", "cbDidDisplayInterstitial|0");
    }
}

- (void)didDismissInterstitial:(CBLocation)location
{
    if (soundDisabled)
    {
        UnitySendMessage("__objcdispatcher", "dispatch", "csEnableSound");
        soundDisabled = NO;
    }
}

@end
