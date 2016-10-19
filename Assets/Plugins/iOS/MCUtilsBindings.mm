#import "MCUtilsController.h"
#import <UIKit/UIDevice.h>
#import "UIDevice+IdentifierAddition.h"

extern "C"
{
    //UIViewController* UnityGetGLViewControllerEx();

	void MCUtilsGetMessagesCount(int* unread, int* total)
	{
		*unread = [MCPostman nrOfUnreadMessages];
		*total = [MCPostman nrOfMessages];
	}

	void MCUtilsShowBoard()
	{
		[MCPostman showBoard];
	}

	bool MCUtilsShowUrgentBoard()
	{
		return [MCPostman showUrgentBoard];
	}

	void MCUtilsDismissBoard()
	{
		[MCPostman dismissBoard];
	}

	void MCUtilsSetInGameFlag(bool flag)
	{
		[[MCUtilsController sharedMCUtilsController] setIngameFlag:flag];
	}

	void MCUtilsShowCorporateInfo()
	{
		//[MCCorporateInfo show:UnityGetGLViewControllerEx()];
        [MCCorporateInfo show:UnityGetGLViewController()];
	}

	void MCUtilsShowMoreGames()
	{
		//[MCMoreGames show:UnityGetGLViewControllerEx()];
        [MCMoreGames show:UnityGetGLViewController()];
	}

	const char* MCUtilsGetUniqueDeviceId()
	{
        NSString* identifyer = [[UIDevice currentDevice] uniqueDeviceIdentifier];
		if(identifyer == nil)
            identifyer = [[UIDevice currentDevice] advertisingUserIdentifier];

        const char* utfString = [identifyer UTF8String];
        char* stringBuffer = (char*)calloc([identifyer length] + 1, 1);
        strncpy(stringBuffer, utfString, [identifyer length]);
        return stringBuffer;
	}
}
