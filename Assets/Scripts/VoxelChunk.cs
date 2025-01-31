using UnityEngine;
using System.Collections.Generic;

public class VoxelChunk : MonoBehaviour
{
    public Material material;
    
    public Vector3Int chunkPosition;

    private VoxelMeshBuilder meshBuilder;

    private ChunkData data;
    private TextureAtlas atlas;

    void Start()
    {
        atlas = new TextureAtlas();
        data = new ChunkData();

        chunkPosition = new Vector3Int(
            Mathf.FloorToInt(transform.position.x / ChunkManager.chunkSize),
            0,
            Mathf.FloorToInt(transform.position.z / ChunkManager.chunkSize)
        );

        meshBuilder = new VoxelMeshBuilder(gameObject, material, 16);

        GenerateChunk();
        RebuildMesh();
    }

    void GenerateChunk()
    {
        for (int x = 0; x < ChunkManager.chunkSize; x++)
        {
            for (int z = 0; z < ChunkManager.chunkSize; z++)
            {
                float height = ChunkManager.Instance.GetTerrainHeight(new Vector2(x, z), new Vector2Int(chunkPosition.x, chunkPosition.z));

                for (int y = 0; y <= height; y++) // Remplit les blocs sous la surface
                {
                    data.SetBlock(x, y, z, true);
                }
            }
        }

        // Maintenant, on génère les faces visibles uniquement
        for (int x = 0; x < ChunkManager.chunkSize; x++)
        {
            for (int y = 0; y < ChunkManager.chunkSize; y++)
            {
                for (int z = 0; z < ChunkManager.chunkSize; z++)
                {
                    if (data.GetBlock(x, y, z))
                    {
                        meshBuilder.AddCube(new Vector3Int(x, y, z), data, atlas.Grass);
                    }
                }
            }
        }
    }

    public void RemoveBlock(Vector3Int position)
    {
        Vector3Int localPos = WorldToLocal(position);

        if (data.IsInsideChunk(localPos))
        {
            data.SetBlock(localPos, false);
            UpdateSurroundingBlocks(localPos);
            RebuildMesh();
        }
    }

    public void AddBlock(Vector3Int position)
    {
        Vector3Int localPos = WorldToLocal(position);

        if (data.IsInsideChunk(localPos))
        {
            data.SetBlock(localPos, true);
            UpdateSurroundingBlocks(localPos);
            RebuildMesh();
        }
    }

    Vector3Int WorldToLocal(Vector3Int worldPos)
    {
        int localX = ((worldPos.x % ChunkManager.chunkSize) + ChunkManager.chunkSize) % ChunkManager.chunkSize;
        int localZ = ((worldPos.z % ChunkManager.chunkSize) + ChunkManager.chunkSize) % ChunkManager.chunkSize;

        return new Vector3Int(localX, worldPos.y, localZ);
    }

    void UpdateSurroundingBlocks(Vector3Int pos)
    {
        // On force la mise à jour des blocs adjacents pour s'assurer que les faces cachées soient bien mises à jour
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down, Vector3Int.forward, Vector3Int.back
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborPos = pos + dir;
            if (data.IsInsideChunk(neighborPos) && data.GetBlock(neighborPos))
            {
                meshBuilder.AddCube(neighborPos, data, atlas.Grass);
            }
        }
    }

    void RebuildMesh()
    {
        meshBuilder.ClearMesh();

        for (int x = 0; x < ChunkManager.chunkSize; x++)
        {
            for (int y = 0; y < ChunkManager.chunkSize; y++)
            {
                for (int z = 0; z < ChunkManager.chunkSize; z++)
                {
                    if (data.GetBlock(x, y, z))
                    {
                        meshBuilder.AddCube(new Vector3Int(x, y, z), data, atlas.Grass);
                    }
                }
            }
        }

        meshBuilder.ApplyMesh();
    }
}
