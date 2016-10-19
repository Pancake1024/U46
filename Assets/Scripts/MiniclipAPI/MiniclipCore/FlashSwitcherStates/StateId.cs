using System;

namespace com.miniclip
{
	public class StateId
	{
		public const string READY 						= "ready";
		public const string TAKING_SCREEN_GRAB			= "taking_screen_grab"; 
		public const string CLEARING_BMP_DATA			= "clearing_bmp_data"; 	
		public const string TRANSMITTING_BMP_DATA		= "transmitting_bmp_data";
		public const string RENDERING_SCREEN_GRAB		= "rendering_screen_grab";
		public const string CALLING_REQUESTED_FUNCTION 	= "calling_requested_function";	
	}
}

