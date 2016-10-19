using UnityEngine;
using SBS.Pool;
using SBS.Core;

[AddComponentMenu("UI/UIFlyer")]
public class UIFlyer : IntrusiveListNode<UIFlyer>, IPoolObject<UIFlyer>, IPoolCollectionObject<UIFlyer>
{
    #region Public members
    public Signal onPlay = Signal.Create<UIFlyer>();
    public Signal onEnd = Signal.Create<UIFlyer>();
    #endregion

    #region Protected members
    protected Animation flyerAnimation;
    protected string clipName;
    protected AnimationState flyerAnimState;
    protected bool isPlaying = false;
    protected bool isPaused = false;
    #endregion

    #region IPoolObject
    protected Pool<UIFlyer> pool;
    protected bool isUsed = false;
    protected uint instanceID;

    public Pool<UIFlyer> Pool
    {
        get
        {
            return pool;
        }
        set
        {
            pool = value;
        }
    }

    public bool IsUsed
    {
        get
        {
            return isUsed;
        }
        set
        {
            isUsed = value;
        }
    }

    public uint InstanceID
    {
        get
        {
            return instanceID;
        }
        set
        {
            instanceID = value;
        }
    }

    public void OnInstantiation()
    {
        flyerAnimState.speed = 0.0f;
    }

    public void OnRequested()
    {
        flyerAnimation.Play(clipName);
        flyerAnimState = flyerAnimation[clipName];
        flyerAnimState.normalizedTime = 0.0f;
        flyerAnimation.Sample();

        isPlaying = false;
        isPaused = false;
    }

    public void OnFreed()
    {
        onEnd.Invoke(this);

        isPlaying = false;
        isPaused = false;
    }
    #endregion

    #region IPoolCollectionObject
    protected PoolCollection<UIFlyer> poolCollection;
    protected uint originalID;

    public uint OriginalID
    {
        get
        {
            return originalID;
        }
        set
        {
            originalID = value;
        }
    }

    public PoolCollection<UIFlyer> PoolCollection
    {
        get
        {
            return poolCollection;
        }
        set
        {
            poolCollection = value;
        }
    }
    #endregion

    #region Public methods
    public void Play()
    {
        if (!isPlaying)
        {
            flyerAnimState.speed = 1.0f;

            isPlaying = true;
            isPaused = false;

            onPlay.Invoke(this);
        }
        else
        {
            if (isPaused)
            {
                flyerAnimState.speed = 1.0f;
                isPaused = false;
            }
        }
    }

    public void UpdateFlyer()
    {
        if (flyerAnimState.normalizedTime >= 1.0f)
        {
            if (!poolCollection.IsNull)
                poolCollection.Free(this);
        }
    }

    public void Pause()
    {
        if (isPlaying && !isPaused)
        {
            flyerAnimState.speed = 0.0f;
            isPaused = true;
        }
    }

    public void Stop()
    {
        if (isPlaying)
        {
            flyerAnimState.normalizedTime = 0.0f;
            flyerAnimState.speed = 0.0f;

            isPlaying = false;

            if (!poolCollection.IsNull)
                poolCollection.Free(this);
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        flyerAnimation = gameObject.GetComponentInChildren<Animation>();

#if UNITY_EDITOR
        if (null == flyerAnimation)
        {
            Debug.LogError("UIFlyer w/out Animation!", gameObject);
            return;
        }

        if (null == flyerAnimation.clip)
        {
            Debug.LogError("UIFlyer w/out AnimationClip!", gameObject);
            return;
        }

        if (flyerAnimation.gameObject == gameObject)
            Debug.LogWarning("UIFlyer with Animation on root node! (coudn't offset it)");
#endif

        flyerAnimation.clip.wrapMode = WrapMode.ClampForever;

        clipName = flyerAnimation.clip.name;
        if (!flyerAnimation.isPlaying)
            flyerAnimation.Play(clipName);

        flyerAnimState = flyerAnimation[clipName];
    }

    void OnDestroy()
    {
        poolCollection.OnDestroyed(this);
        pool.OnDestroyed(this);
    }
    #endregion
}
