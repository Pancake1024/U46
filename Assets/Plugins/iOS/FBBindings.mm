//#import "FBController.h"
//#import "CoinsService.h"

extern "C"
{
    void facebookCheckLogin()
    {
        //[[FBController sharedFBController] checkLogin];
    }
    
    void facebookCheckLoginWithRedirect(const char* nextURL)
    {
        //[[FBController sharedFBController] checkLoginWithRedirect:[NSString stringWithUTF8String:nextURL]];
    }
    
    void facebookLogout()
    {
        //[[FBController sharedFBController] logout];
    }
    
    void facebookRequestGraphPath(const char* path)
    {
        //[[FBController sharedFBController] requestWithGraphPath:[NSString stringWithUTF8String:path]];
    }
    
    void facebookOpenDialog(const char* action, const char* keys[], const char* values[], int count)
    {        
        NSMutableArray* keysArray = [NSMutableArray arrayWithCapacity:count];
        NSMutableArray* valuesArray = [NSMutableArray arrayWithCapacity:count];
        for (int i = 0; i < count; ++i)
        {
            [keysArray addObject:[NSString stringWithUTF8String:keys[i]]];
            [valuesArray addObject:[NSString stringWithUTF8String:values[i]]];
        }
        NSMutableDictionary* dict = [NSMutableDictionary dictionaryWithObjects:valuesArray forKeys:keysArray];
        /*
         [[FBController sharedFBController]
         openDialog:[NSString stringWithUTF8String:action]
         withParams:dict];*/
        /*
         [[[CoinsService getFacebookManager] getLegacyFacebook]
         dialog:[NSString stringWithUTF8String:action]
         andParams:dict
         andDelegate:nil];*/
        
        // REMOVE FACEBOOK FROM COINSERVICE [[CoinsService getFacebookManager] openShareDialogWithParams:dict];
    }
}