using System;
using System.Collections.Generic;

#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{
    /*
     * Queue is a simple persistent queue for storing QueueElement-objects.
     * It has no thread-safety mechanisms.
     */
    public class Queue
    {
        private const string TAG = "Queue";

        /**
         * Cache the ElementCount. Getting the Count from 
         * Storage involves accessing the persistent storage,
         * which is an expensive operation. Since the Storage
         * instance is only accesses by the Queue instance, 
         * the caching can safely be done here.
         */
        private int ElementCount { get; set; }

        public Storage Storage { get; set; }

        public Queue(string persistentDataPath)
        {
            Storage = new Storage(persistentDataPath);
            ElementCount = Storage.Read().Count;
            Log.Debug(TAG, "loaded, count: " + ElementCount); 
        }

        /**
         * Appends the given QueueElement to the queue.
         */
        public void Put(QueueElement element)
        {
            List<QueueElement> elements = Storage.Read();
            elements.Add(element);
            Storage.Write(elements);

            ElementCount++;
        }

        /*
         * Prepends the given QueueElement to the queue.
         */
        public void Prepend(QueueElement element)
        {
            List<QueueElement> elements = Storage.Read();
            elements.Insert(0, element);
            Storage.Write(elements);
            
            ElementCount++;
        }

        public QueueElement Peek()
        {
            if (ElementCount == 0)
            {
                return null;
            }
            List<QueueElement> elements = Storage.Read();
            return elements [0];
        }

        /**
         * Takes a QueueElement from the Queue, 
         * throws an Exception if the Queue is empty.
         */
        public QueueElement Take()
        {
            if (ElementCount == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            List<QueueElement> elements = Storage.Read();
            QueueElement element = elements [0];
            elements.RemoveAt(0);
            Storage.Write(elements);
            
            ElementCount--;

            return element;    
        }

        /*
         * Counts the elements.
         */
        public int Count()
        {
            return ElementCount;
        }
    }
}

#endif