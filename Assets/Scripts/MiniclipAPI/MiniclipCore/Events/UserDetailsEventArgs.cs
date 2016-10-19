using System;

namespace com.miniclip
{
	public class UserDetailsEventArgs : EventArgs
	{
	    private UserDetails _userDetails;
		
		public UserDetails UserDetails
	   	{
	    	get { return this._userDetails; }
	    }
		
		public bool UserLoggedIn
		{
			get
			{
				return (_userDetails != null);
			}
		}
	
	   	public UserDetailsEventArgs(UserDetails userDetails)
	    {
	   		this._userDetails = userDetails;
		}
	}
}
