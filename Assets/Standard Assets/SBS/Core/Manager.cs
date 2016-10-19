using UnityEngine;

using Asserts = System.Diagnostics.Debug;

public class Manager<T> : MonoBehaviour
    where T : Manager<T>
{
    private static T instance = null;

    protected void Awake()
    {
        Asserts.Assert(null == instance);
        instance = (T)this;
    }

    protected void Start()
    { }

    protected void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    public static T Get()
    {
        if (null == instance)
            return FindObjectOfType<T>();
        return instance;
    }
}
