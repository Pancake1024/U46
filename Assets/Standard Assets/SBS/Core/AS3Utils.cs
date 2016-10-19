using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Core
{
    public class AS3Utils
    {
#if UNITY_FLASH
        static public int BinarySearch(IComparable[] array, IComparable value)
        {
            int high = array.Length - 1,
                low = 0,
                pivot,
                cmp;

            while (low <= high)
            {
                pivot = (low + high) >> 1;
                cmp = array[pivot].CompareTo(value);

                if (cmp < 0)
                    low = pivot + 1;
                else if (cmp > 0)
                    high = pivot - 1;
                else
                    return pivot;
            }

            return -(low + 1);
        }

        static public int BinarySearchList<T>(List<T> list, T value)
            where T : IComparable
        {
            int high = list.Count - 1,
                low = 0,
                pivot,
                cmp;

            while (low <= high)
            {
                pivot = (low + high) >> 1;
                cmp = list[pivot].CompareTo(value);

                if (cmp < 0)
                    low = pivot + 1;
                else if (cmp > 0)
                    high = pivot - 1;
                else
                    return pivot;
            }

            return -(low + 1);
        }

        static public int BinarySearchList(List<int> list, int value)
        {
            int high = list.Count - 1,
                low = 0,
                pivot;

            while (low <= high)
            {
                pivot = (low + high) >> 1;

                if (list[pivot] < value)
                    low = pivot + 1;
                else if (list[pivot] > value)
                    high = pivot - 1;
                else
                    return pivot;
            }

            return -(low + 1);
        }

        static public int BinarySearchList(List<float> list, float value)
        {
            int high = list.Count - 1,
                low = 0,
                pivot;

            while (low <= high)
            {
                pivot = (low + high) >> 1;

                if (list[pivot] < value)
                    low = pivot + 1;
                else if (list[pivot] > value)
                    high = pivot - 1;
                else
                    return pivot;
            }

            return -(low + 1);
        }

        static public int LastIndexOf(string s, char c, int startIndex, int count)
        {
            for (int j = 0; j < count; ++j)
            {
                int i = startIndex - j;
                if (s[i] == c)
                    return i;
            }
            return -1;
        }

        static public int LastIndexOfAny(string s, char[] anyChars, int startIndex, int count)
        {
            int i = -1;
            foreach (char c in anyChars)
                i = Mathf.Max(i, LastIndexOf(s, c, startIndex, count));
            return i;
        }

        static public void Qsort<T>(List<T> list, int left, int right)
            where T : IComparable
        {
            // ToDo FLASH without recursion
            if (left > right)
                return;
            int i = left,
                j = right;
            T tmp, pivot = list[(left + right) >> 1];
            while (i <= j)
            {
                while (i <= right && list[i].CompareTo(pivot) < 0)
                    ++i;
                while (j >= left && list[j].CompareTo(pivot) > 0)
                    --j;

                if (i <= j && i <= right && j >= left)
                {
                    tmp = list[i];
                    list[i] = list[j];
                    list[j] = tmp;
                    ++i;
                    --j;
                }
            }

            if (left < j)
                Qsort(list, left, j);

            if (i < right)
                Qsort(list, i, right);
        }
#endif
    }
}
