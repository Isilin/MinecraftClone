using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChunkMesh))]
public class VoxelChunk : MonoBehaviour
{
    private ChunkMesh mesh;
    private ChunkData data;

    private void Awake()
    {
        mesh = GetComponent<ChunkMesh>();
        data = new ChunkData();
    }
    public void SetData(ChunkData voxelData)
    {
        data = voxelData;
        GenerateMesh();
    }

    public void RemoveBlock(Vector3Int position)
    {
        Vector3Int localPos = MapGeneration.Instance.WorldToLocal(position);

        if (data.IsInsideChunk(localPos))
        {
            data.SetBlock(localPos, false);
            UpdateSurroundingBlocks(localPos);
            GenerateMesh();
        }
    }

    public void AddBlock(Vector3Int position)
    {
        Vector3Int localPos = MapGeneration.Instance.WorldToLocal(position);

        if (data.IsInsideChunk(localPos))
        {
            data.SetBlock(localPos, true);
            UpdateSurroundingBlocks(localPos);
            GenerateMesh();
        }
    }

    void UpdateSurroundingBlocks(Vector3Int pos)
    {
        List<Vector3Int> neighbors = data.GetSurroundingBlocks(pos);

        foreach (Vector3Int neighbor in neighbors)
        {
            mesh.AddCube(neighbor, data, TextureAtlas.Instance.Grass);
        }
    }

    void GenerateMesh()
    {
        int chunkSize = MapGeneration.Instance.chunkSize;
        mesh.ClearMesh();

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if (data.GetBlock(x, y, z))
                    {
                        mesh.AddCube(new Vector3Int(x, y, z), data, TextureAtlas.Instance.Grass);
                    }
                }
            }
        }

        mesh.ApplyMesh();
    }
}
