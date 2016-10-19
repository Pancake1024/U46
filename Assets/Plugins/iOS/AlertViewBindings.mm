#import "AlertViewController.h"

extern "C"
{
    void openAlertBox(const char* title, const char* message, const char* button, const char* otherButtons[], int otherButtonsCount, bool pauseUnity)
    {
        NSMutableArray* aList = otherButtonsCount > 0 ? [NSMutableArray arrayWithCapacity:otherButtonsCount] : nil;
        for (int i = 0; i < otherButtonsCount; ++i)
            [aList addObject:[NSString stringWithUTF8String:otherButtons[i]]];

        [[AlertViewCtrl sharedAlertView] openAlertBoxWithTitle:[NSString stringWithUTF8String:title]
                                                       message:[NSString stringWithUTF8String:message]
                                             cancelButtonTitle:[NSString stringWithUTF8String:button]
                                             otherButtonTitles:aList
                                                    pauseUnity:pauseUnity];
    }
}
