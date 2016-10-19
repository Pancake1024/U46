#import "Flurry.h"

extern "C"
{
    NSDictionary* flurryCreateParams(const char* keys[], const char* values[], int count)
    {
        NSMutableArray* keysArray = [NSMutableArray arrayWithCapacity:count];
        NSMutableArray* valuesArray = [NSMutableArray arrayWithCapacity:count];
        for (int i = 0; i < count; ++i)
        {
            [keysArray addObject:[NSString stringWithUTF8String:keys[i]]];
            [valuesArray addObject:[NSString stringWithUTF8String:values[i]]];
        }
        NSDictionary* dict = [NSDictionary dictionaryWithObjects:valuesArray forKeys:keysArray];
        return dict;
    }

    void flurryLogEvent(const char* eventName, bool timed)
    {
        NSString* s = [NSString stringWithUTF8String: eventName];
        [Flurry logEvent:s timed:timed];
    }

    void flurryEndTimedEvent(const char* eventName)
    {
        NSString* s = [NSString stringWithUTF8String: eventName];
        [Flurry endTimedEvent:s withParameters:nil];
    }

    void flurryLogEventWithParams(const char* eventName, bool timed, const char* keys[], const char* values[], int count)
    {
        NSString* s = [NSString stringWithUTF8String: eventName];
        NSDictionary* dict = flurryCreateParams(keys, values, count);
        [Flurry logEvent:s withParameters:dict timed:timed];
    }

    void flurryEndTimedEventWithNewParams(const char* eventName, const char* keys[], const char* values[], int count)
    {
        NSString* s = [NSString stringWithUTF8String: eventName];
        NSDictionary* dict = flurryCreateParams(keys, values, count);
        [Flurry endTimedEvent:s withParameters:dict];
    }
}
