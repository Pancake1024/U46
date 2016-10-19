#import "ChartboostManager.h"

extern "C"
{
    void chartboostCacheInterstitialForLocation(char *stringLocation)
    {
    	[Chartboost cacheInterstitial:[NSString stringWithUTF8String:stringLocation]];
		
		/*
		UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Chartboost cache"
								message:[NSString stringWithUTF8String:stringLocation]
							       delegate:nil
						      cancelButtonTitle:@"Ok"
						      otherButtonTitles:nil];
		[alert show];
		[alert release];
		*/
    }

    void chartboostShowInterstitialForLocation(char* stringLocation)
    {
        [Chartboost showInterstitial:[NSString stringWithUTF8String:stringLocation]];
		
		/*
		UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Chartboost show"
								message:[NSString stringWithUTF8String:stringLocation]
								   delegate:nil
							  cancelButtonTitle:@"Ok"
							  otherButtonTitles:nil];
		[alert show];
		[alert release];
		*/
    }

    void chartboostLockInterstitials()
    {
        [[ChartboostManager sharedInstance] lockIntertitials];
    }

    void chartboostUnlockInterstitials()
    {
        [[ChartboostManager sharedInstance] unlockInterstitials];
    }
}
