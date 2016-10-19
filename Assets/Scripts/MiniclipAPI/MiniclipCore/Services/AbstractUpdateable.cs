using System;

namespace com.miniclip
{
	public abstract class AbstractUpdateable
	{	
		internal int currentUpdatePool = UpdatePoolId.NONE;
		
		internal abstract event EventHandler<UpdateEventArgs> updatePoolRequested;
		
		//public event EventHandler<MessageEventArgs>		PurchaseCancelled;
		
		internal abstract void Update();			
	}
}

