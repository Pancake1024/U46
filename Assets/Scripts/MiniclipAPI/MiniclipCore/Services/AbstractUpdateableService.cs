using System;

namespace com.miniclip
{
	public abstract class AbstractUpdateableService : AbstractService
	{
		internal abstract AbstractUpdateable Updateable
		{
			get;
		}	
	}
}

