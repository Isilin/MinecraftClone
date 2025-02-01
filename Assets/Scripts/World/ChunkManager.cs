using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class ChunkManager : Singleton<ChunkManager>
{
    public int viewDistance = 5; // Distance de génération des chunks (en chunks)
    public GameObject chunkPrefab;
    public Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();

    public Transform player; // Référence au joueur

    private ConcurrentQueue<KeyValuePair<Vector2Int, ChunkData>> chunksToLoad = new ConcurrentQueue<KeyValuePair<Vector2Int, ChunkData>>();
    private ConcurrentDictionary<Vector2Int, bool> chunksBeingGenerated = new ConcurrentDictionary<Vector2Int, bool>();
    private int maxChunksPerFrame = 2;
    private readonly object lockObject = new object();

    void Start()
    {
        UpdateChunksList();

        StartCoroutine(WaitForTerrainGeneration());
        StartCoroutine(ProcessChunksQueue());
    }

    void Update()
    {
        UpdateChunksList();
    }

    void UpdateChunksList()
    {
        Vector2Int playerChunkCoord = MapGeneration.Instance.WorldToChunk(player.position);

        HashSet<Vector2Int> chunksToGenerate = new HashSet<Vector2Int>();

        // Générer de nouveaux chunks autour du joueur
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunkCoord.x + x, playerChunkCoord.y + z);

                lock (lockObject)
                {
                    if (!chunks.ContainsKey(chunkCoord) && !chunksBeingGenerated.TryGetValue(chunkCoord, out bool isGenerating))
                    {
                        chunksToGenerate.Add(chunkCoord);
                        chunksBeingGenerated[chunkCoord] = true;
                    }
                }
            }
        }

        // Lance la génération en batch pour éviter la boucle infinie
        foreach (var chunkCoord in chunksToGenerate)
        {
            Thread thread = new Thread(() => GenerateChunkThread(chunkCoord));
            thread.Start();
        }

        // Supprimer les chunks trop loin du joueur
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunk in chunks)
        {
            int distanceX = Mathf.Abs(chunk.Key.x - playerChunkCoord.x);
            int distanceZ = Mathf.Abs(chunk.Key.y - playerChunkCoord.y);

            if (distanceX > viewDistance || distanceZ > viewDistance)
            {
                Destroy(chunk.Value);
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkKey in chunksToRemove)
        {
            chunks.Remove(chunkKey);
        }
    }

    void GenerateChunkThread(Vector2Int chunkCoord)
    {
        // Ajouter le chunk généré à la file d'attente pour l'affichage
        if (chunksToLoad.Where(item => item.Key == chunkCoord).ToList().Count == 0)
        {
            ChunkData voxelData = MapGeneration.Instance.GenerateChunkData(chunkCoord);
            chunksToLoad.Enqueue(new KeyValuePair<Vector2Int, ChunkData>(chunkCoord, voxelData));
        }

        // Retirer le chunk de la liste des chunks en cours de génération
        chunksBeingGenerated.TryRemove(chunkCoord, out _);
    }

    IEnumerator WaitForTerrainGeneration()
    {
        Vector2Int playerChunkCoord = MapGeneration.Instance.WorldToChunk(player.position);

        // Attendre que le chunk du joueur soit généré
        while (!chunks.ContainsKey(playerChunkCoord))
        {
            yield return null; // Attendre la prochaine frame
        }

        // Une fois que le chunk est généré, placer le joueur au-dessus du terrain
        float highestPoint = MapGeneration.Instance.GetHeight(new Vector2(player.position.x, player.position.z), playerChunkCoord);
        player.position = new Vector3(player.position.x, highestPoint + 2.0f, player.position.z);
    }

    IEnumerator ProcessChunksQueue()
    {
        while (true)
        {
            int processed = 0;

            while (processed < maxChunksPerFrame && chunksToLoad.TryDequeue(out KeyValuePair<Vector2Int, ChunkData> chunkData))
            {
                if (!chunks.ContainsKey(chunkData.Key))
                {
                    CreateChunk(chunkData.Key, chunkData.Value);
                    processed++;
                }
            }

            yield return new WaitForSeconds(0.05f); // Attendre la prochaine frame pour éviter les freezes
        }
    }
    void CreateChunk(Vector2Int chunkCoord, ChunkData voxelData)
    {
        int chunkSize = MapGeneration.Instance.chunkSize;
        if (chunks.ContainsKey(chunkCoord))
            return;

        GameObject chunkObject = Instantiate(chunkPrefab, new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize), Quaternion.identity, gameObject.transform);
        VoxelChunk chunk = chunkObject.GetComponent<VoxelChunk>();

        chunk.SetData(voxelData);
        chunks[chunkCoord] = chunkObject;
    }
}
