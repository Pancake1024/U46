using System;
using System.Collections.Generic;

namespace SBS.Core
{
	public struct SBSTuple<A, B>
	{
        public A First;
        public B Second;

        public SBSTuple(A first, B second)
        {
            First = first;
            Second = second;
        }
    }
}
