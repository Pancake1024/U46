using System;
using System.Collections.Generic;

namespace SBS.Core
{
	public class AS3Stack<T>
	{
        protected List<T> list = new List<T>();

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public void Clear()
        {
            list.Clear();
        }

        public void Push(T value)
        {
            list.Add(value);
        }

        public T Pop()
        {
            int index = list.Count - 1;
            T retValue = list[index];
            list.RemoveAt(index);
            return retValue;
        }

        public T Peek()
        {
            int index = list.Count - 1;
            return list[index];
        }
    }
}
