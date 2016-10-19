using System;
using UnityEngine;

public class IntrusiveListNode<T> : MonoBehaviour
    where T : MonoBehaviour
{
    [NonSerialized]
    public T previous = null;
    [NonSerialized]
    public T next = null;
}
