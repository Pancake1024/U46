using System;
using System.Collections;
using System.Collections.Generic;

using Asserts = System.Diagnostics.Debug;

namespace SBS.Core
{
    public class IntrusiveList<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
        where T : IntrusiveListNode<T>
    {
        protected T head = null;
        protected T tail = null;
        protected int size = 0;

        public int Count
        {
            get
            {
                return size;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return null;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T Head
        {
            get
            {
                return head;
            }
        }

        public T Tail
        {
            get
            {
                return tail;
            }
        }

        public void Add(T item)
        {
            if (null == head)
            {
                Asserts.Assert(0 == size);
                head = tail = item;
                item.previous = item.next = null;
            }
            else
            {
                tail.next = item;
                item.previous = tail;
                item.next = null;
                tail = item;
            }
            ++size;
        }

        public void Clear()
        {
            T node = head;
            while (node != null)
            {
                T tmp = node;
                node = node.next;
                tmp.previous = tmp.next = null;
            }

            head = tail = null;
            size = 0;
        }

        public bool Contains(T item)
        {
            T node = head;
            while (node != null)
            {
                if (node == item)
                    return true;
                node = node.next;
            }
            return false;
        }

        public void CopyTo(Array array, int index)
        {
            Asserts.Assert(index >= 0 && (array.Length - index) >= size);
            T node = head;
            while (node != null)
            {
                array.SetValue(node, index++);
                node = node.next;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Asserts.Assert(arrayIndex >= 0 && (array.Length - arrayIndex) >= size);
            T node = head;
            while (node != null)
            {
                array[arrayIndex++] = node;
                node = node.next;
            }
        }

        public bool Remove(T item)
        {
            if (item == head)
            {
                if (item == tail)
                {
                    Asserts.Assert(1 == size);
                    head = tail = null;
                }
                else
                {
                    head = head.next;
                    head.previous = null;
                }
            }
            else if (item == tail)
            {
                tail = tail.previous;
                tail.next = null;
            }
            else
            {
                if (null == item.previous && null == item.next)
                    return false;

                item.previous.next = item.next;
                item.next.previous = item.previous;
            }

            item.previous = item.next = null;
            --size;

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator() { list = this, node = null };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void ForEach(Action<T> function)
        {
            T node = head;
            while (head != null)
            {
                function.Invoke(node);
                node = node.next;
            }
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            internal IntrusiveList<T> list;
            internal T node;

            public T Current
            {
                get
                {
                    return node;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return node;
                }
            }

            public bool MoveNext()
            {
                if (null == node)
                {
                    Asserts.Assert(list != null);
                    node = list.head;
                    return (node != null);
                }
                else
                {
                    node = node.next;
                    return (node != null);
                }
            }

            public void Reset()
            {
                node = null;
            }

            public void Dispose()
            {
                list = null;
                node = null;
            }
        }
    }
}
