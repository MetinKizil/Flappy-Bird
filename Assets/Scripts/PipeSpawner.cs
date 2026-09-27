using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boru çiftlerini belirli aralıklarla, rastgele yükseklikte üretir.
/// Instantiate/Destroy yerine Object Pooling kullanır (mobilde GC takılmalarını önler).
/// </summary>
public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private PipePair pipePrefab;
    [SerializeField] private int initialPoolSize = 5;
    [SerializeField] private float spawnInterval = 1.6f;
    [SerializeField] private float minY = -1.5f;
    [SerializeField] private float maxY = 2.5f;

    private readonly Queue<PipePair> pool = new Queue<PipePair>();
    private float timer;

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
            pool.Enqueue(CreatePipe());
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            Spawn();
        }
    }

    private void Spawn()
    {
        PipePair pipe = pool.Count > 0 ? pool.Dequeue() : CreatePipe();
        pipe.transform.position = new Vector3(transform.position.x, Random.Range(minY, maxY), 0f);
        pipe.gameObject.SetActive(true);
    }

    private PipePair CreatePipe()
    {
        PipePair pipe = Instantiate(pipePrefab, transform.position, Quaternion.identity, transform);
        pipe.Init(this);
        pipe.gameObject.SetActive(false);
        return pipe;
    }

    public void ReturnToPool(PipePair pipe)
    {
        pipe.gameObject.SetActive(false);
        pool.Enqueue(pipe);
    }
}
