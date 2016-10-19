using UnityEngine;
using System.Collections;

public class EffectBehaviour : MonoBehaviour {

    public class EffectSpawnParameters
    {
        Transform spawnTransform;
        Vector3 spawnOffset;
        Vector3 spawnPoint;
        bool shouldParentToTransform;

        public Transform SpawnTransform { get { return spawnTransform; } }
        public Vector3 SpawnOffset { get { return spawnOffset; } }
        public bool ParentToTransform { get { return shouldParentToTransform; } }
        public Vector3 SpawnPoint { get { return spawnPoint; } set { spawnPoint = value; } }

        public EffectSpawnParameters(Transform spawnTransform, Vector3 spawnOffset, bool parentToTransform)
        {
            this.spawnTransform = spawnTransform;
            this.spawnOffset = spawnOffset;
            this.shouldParentToTransform = parentToTransform;
        }
    }

    public OnTheRunObjectsPool.ObjectType effectType;
    ParticleSystem particleSystem;

    void Awake()
    {
        particleSystem = gameObject.GetComponent<ParticleSystem>();
        if (particleSystem!=null)
            particleSystem.Stop(true);
    }

    void Spawn(EffectSpawnParameters spawn)
    {
        transform.position = spawn.SpawnTransform.position + spawn.SpawnOffset;
        transform.rotation = spawn.SpawnTransform.rotation;

        if (spawn.ParentToTransform)
        {
            transform.parent = spawn.SpawnTransform;
        }

        if (!transform.gameObject.activeInHierarchy)
            return;

        //Debug.Log(">>>>>>>> SPAWN - name: " + gameObject.name + " - spawn.position: " + spawn.SpawnTransform.position.ToString() + ", transform.position:" + transform.position.ToString() + ", shouldParent:" + spawn.ParentToTransform + ", parent =! null: " + (transform.parent != null));
        if (particleSystem != null)
        {
            particleSystem.Clear(true);
            particleSystem.Play(true);
            StartCoroutine(ReleaseCoroutine());
        }
        else
            StartCoroutine(ReleaseCoroutineNoParticles());
    }

    void SpawnInPoint(Vector3 spawnPoint)
    {
        transform.position = spawnPoint;

        //Debug.Log(">>>>>>>> SPAWNINPOINT - name: " + gameObject.name + " - spawn.position: " + spawnPoint);// + spawn.SpawnTransform.position.ToString() + ", transform.position:" + transform.position.ToString() + ", shouldParent:" + spawn.ParentToTransform + ", parent =! null: " + (transform.parent != null));
        if (particleSystem != null)
            particleSystem.Play(true);
        StartCoroutine(ReleaseCoroutine());
    }

    IEnumerator ReleaseCoroutine()
    {
        yield return new WaitForSeconds(particleSystem.duration);

        /*if (particleSystem != null)
            particleSystem.Stop(true);
        OnTheRunObjectsPool.Instance.EffectFinshed(gameObject);*/
        StartCoroutine(ReleaseCoroutineWParticles());
    }

    IEnumerator ReleaseCoroutineWParticles()
    {
        yield return new WaitForSeconds(0.5f);

        if (particleSystem != null)
            particleSystem.Stop(true);
        OnTheRunObjectsPool.Instance.EffectFinshed(gameObject);
    }

    IEnumerator ReleaseCoroutineNoParticles()
    {
        yield return new WaitForSeconds(2.0f);
        OnTheRunObjectsPool.Instance.EffectFinshed(gameObject);
    }

    void OnGameover()
    {
        Debug.Log("FREEING EFFECTS");
        OnTheRunObjectsPool.Instance.EffectFinshed(gameObject);
    }

}