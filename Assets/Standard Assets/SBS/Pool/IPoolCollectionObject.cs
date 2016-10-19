using System;
using System.Collections;
using System.Collections.Generic;

namespace SBS.Pool
{
    public interface IPoolCollectionObject<T>
        where T : IntrusiveListNode<T>, IPoolCollectionObject<T>, IPoolObject<T>
    {
        PoolCollection<T> PoolCollection { get; set; }

        uint OriginalID { get; set; }
    }
}
