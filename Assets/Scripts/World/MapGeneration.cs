using UnityEngine;

public class MapGeneration : Singleton<MapGeneration>
{
    [SerializeField] private float scale = 0.05f;  // Échelle du bruit (valeur ajustable)
    [SerializeField] private float scaleBis = 0.01f;
    [SerializeField] private int heightMultiplier = 10;  // Hauteur max des montagnes
    [SerializeField] private int heightMultiplierBis = 5;

    [SerializeField] public int worldSeed = 0;
    [SerializeField] public int chunkSize = 16;

    protected override void OnAwake()
    {
        if (worldSeed == 0)
        {
            this.worldSeed = Random.Range(0, 9999999);
        }
        Debug.Log("Seed du monde : " + worldSeed);
    }

    public float GetHeight(Vector2 pos, Vector2Int chunkPos)
    {
        float heightBase = Mathf.PerlinNoise((pos.x + chunkPos.x * chunkSize + worldSeed) * scale,
                                 (pos.y + chunkPos.y * chunkSize + worldSeed) * scale) * heightMultiplier;

        float heightVariation = Mathf.PerlinNoise(
            (pos.x + worldSeed) * scaleBis,
            (pos.y + worldSeed) * scaleBis
        ) * heightMultiplierBis;

        float height = heightBase + heightVariation;
        return height;
    }

    public ChunkData GenerateChunkData(Vector2Int chunkPos)
    {
        ChunkData chunkData = new ChunkData();
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float height = GetHeight(new Vector3(x, z), chunkPos);
                for (int y = 0; y <= height; y++)
                {
                    chunkData.SetBlock(x, y, z, true);
                }
            }
        }
        return chunkData;
    }

    public Vector2Int WorldToChunk(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / chunkSize),
            Mathf.FloorToInt(pos.z / chunkSize)
        );
    }
    public Vector3Int WorldToLocal(Vector3Int worldPos)
    {
        return new Vector3Int(Maths.mod(worldPos.x, chunkSize), worldPos.y, Maths.mod(worldPos.z, chunkSize));
    }
}
