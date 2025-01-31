using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class ChunkManager : MonoBehaviour
{
    public int viewDistance = 5; // Distance de génération des chunks (en chunks)
    public GameObject chunkPrefab;
    public Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();

    public Transform player; // Référence au joueur

    public static int chunkSize = 16;
    public static int worldSeed;
    public static MapGeneration terrainGenerator;

    private ConcurrentQueue<KeyValuePair<Vector2Int, ChunkData>> chunksToLoad = new ConcurrentQueue<KeyValuePair<Vector2Int, ChunkData>>();
    private ConcurrentDictionary<Vector2Int, bool> chunksBeingGenerated = new ConcurrentDictionary<Vector2Int, bool>();
    private int maxChunksPerFrame = 2;
    private readonly object lockObject = new object();

    public static ChunkManager Instance { get; private set; } // Singleton

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (worldSeed == 0)
        {
            worldSeed = Random.Range(0, 9999999);
        }
        Debug.Log("Seed du monde : " + worldSeed);

        terrainGenerator = new MapGeneration(worldSeed);

        UpdateChunks();

        StartCoroutine(WaitForTerrainGeneration());
        StartCoroutine(ProcessChunksQueue());
    }

    // Fonction qui retourne la hauteur du terrain à un point donné
    public float GetTerrainHeight(Vector2 localPos, Vector2Int chunkPos)
    {
        return terrainGenerator.GetHeight(localPos, chunkPos);
    }

    void Update()
    {
        UpdateChunks();
    }

    void UpdateChunks()
    {
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.z / chunkSize)
        );

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
        ChunkData voxelData = new ChunkData();

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float height = terrainGenerator.GetHeight(new Vector3(x, 0, z), chunkCoord);
                for (int y = 0; y <= height; y++)
                {
                    voxelData.SetBlock(x, y, z, true);
                }
            }
        }

        // Ajouter le chunk généré à la file d'attente pour l'affichage
        if (!chunksToLoad.Contains(new KeyValuePair<Vector2Int, ChunkData>(chunkCoord, voxelData)))
        {
            chunksToLoad.Enqueue(new KeyValuePair<Vector2Int, ChunkData>(chunkCoord, voxelData));
        }

        // Retirer le chunk de la liste des chunks en cours de génération
        chunksBeingGenerated.TryRemove(chunkCoord, out _);
    }

    IEnumerator WaitForTerrainGeneration()
    {
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.z / chunkSize)
        );

        Debug.Log("Waiting for player chunk to generate...");

        // Attendre que le chunk du joueur soit généré
        while (!chunks.ContainsKey(playerChunkCoord))
        {
            yield return null; // Attendre la prochaine frame
        }

        // Une fois que le chunk est généré, placer le joueur au-dessus du terrain
        float highestPoint = GetTerrainHeight(new Vector2(player.position.x, player.position.z), playerChunkCoord) - 2.0f;
        player.position = new Vector3(player.position.x, highestPoint + 3f, player.position.z);

        Debug.Log($"Player placed at {player.position}");
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
        if (chunks.ContainsKey(chunkCoord))
            return;

        GameObject chunkObject = Instantiate(chunkPrefab, new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize), Quaternion.identity);
        VoxelChunk chunk = chunkObject.GetComponent<VoxelChunk>();

        chunk.Initialize(voxelData);
        chunks[chunkCoord] = chunkObject;
    }
}
