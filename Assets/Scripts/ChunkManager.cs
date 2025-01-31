using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public int viewDistance = 3; // Distance de génération des chunks (en chunks)
    public GameObject chunkPrefab;
    public Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();

    public Transform player; // Référence au joueur

    public static int chunkSize = 16;
    public static int worldSeed;
    public static MapGeneration terrainGenerator;

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

        // Placer le joueur au-dessus du terrain
        float highestPoint = GetTerrainHeight(new Vector2(player.position.x, player.position.z), Vector2Int.zero) + 1.0f;
        player.position = new Vector3(player.position.x, highestPoint + 3f, player.position.z);
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
            Mathf.FloorToInt(player.position.x / 16),
            Mathf.FloorToInt(player.position.z / 16)
        );

        // Générer de nouveaux chunks autour du joueur
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunkCoord.x + x, playerChunkCoord.y + z);

                if (!chunks.ContainsKey(chunkCoord))
                {
                    GameObject newChunk = Instantiate(chunkPrefab, new Vector3(chunkCoord.x * 16, 0, chunkCoord.y * 16), Quaternion.identity);
                    chunks.Add(chunkCoord, newChunk);
                }
            }
        }

        // Supprimer les chunks trop loin du joueur
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunk in chunks)
        {
            if (Vector2Int.Distance(chunk.Key, playerChunkCoord) > viewDistance)
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
}
