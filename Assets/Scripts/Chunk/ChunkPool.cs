using System.Collections.Generic;
using UnityEngine;

public class ChunkPool
{
    static int maxSize = 10;

    private Queue<GameObject> pool;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ChunkPool()
    {
        pool = new Queue<GameObject>();
    }

    public bool Enqueue(GameObject chunk)
    {
        if (pool.Count >= maxSize)
        {
            return false;
        }

        chunk.SetActive(false);
        pool.Enqueue(chunk);
        return true;
    }

    public GameObject TryDequeue()
    {
        if (pool.Count <= 0)
            return null;

        GameObject chunk = pool.Dequeue();
        chunk.SetActive(true);
        return chunk;
    }
}
