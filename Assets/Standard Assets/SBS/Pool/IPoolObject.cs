using System;
using System.Collections;
using System.Collections.Generic;

namespace SBS.Pool
{
	public interface IPoolObject<T>
        where T : IntrusiveListNode<T>, IPoolObject<T>
	{
        Pool<T> Pool { get; set; }
        bool IsUsed { get; set; }

        uint InstanceID { get; set; }

        void OnInstantiation();
        void OnRequested();
        void OnFreed();
	}
}
