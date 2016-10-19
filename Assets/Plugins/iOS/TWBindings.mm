//#import "TWController.h"

extern "C"
{
    BOOL canSendTweet()
    {
        return false;
        /*return [[TWController sharedTWController] canSendTweet];*/
    }

    BOOL sendTweet(const char* text)
    {
        return false;
        /*return [[TWController sharedTWController] sendTweetWithText:[NSString stringWithUTF8String:text]];*/
    }

    BOOL sendTweetWithURL(const char* text, const char* url)
    {
        return false;
        /*return [[TWController sharedTWController] sendTweetWithText:[NSString stringWithUTF8String:text]
                                                             andURL:[NSString stringWithUTF8String:url]];*/
    }
    
    BOOL sendTweetWithURLAndImage(const char* text, const char* url, const char* image)
    {
        return false;
        /*return [[TWController sharedTWController] sendTweetWithText:[NSString stringWithUTF8String:text]
                                                             andURL:[NSString stringWithUTF8String:url]
                                                           andImage:[NSString stringWithUTF8String:image]];*/
    }
}
