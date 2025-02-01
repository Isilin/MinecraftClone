using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkData
{
    private bool[,,] voxelMap;

    public ChunkData()
    {
        int chunkSize = MapGeneration.Instance.chunkSize;
        voxelMap = new bool[chunkSize, chunkSize, chunkSize];
    }
    public bool GetBlock(int x, int y, int z) => voxelMap[x, y, z];
    public bool GetBlock(Vector3Int pos) => GetBlock(pos.x, pos.y, pos.z);

    public void SetBlock(int x, int y, int z, bool value) => voxelMap[x, y, z] = value;
    public void SetBlock(Vector3Int pos, bool value) => SetBlock(pos.x, pos.y, pos.z, value);

    public bool IsInsideChunk(int x, int y, int z)
    {
        int chunkSize = MapGeneration.Instance.chunkSize;
        return x >= 0 && x<chunkSize && y >= 0 && y<chunkSize && z >= 0 && z<chunkSize;
    }
    public bool IsInsideChunk(Vector3Int pos) => IsInsideChunk(pos.x, pos.y, pos.z);

    public bool IsBlockAt(int x, int y, int z) => IsInsideChunk(x, y, z) && voxelMap[x, y, z];
    public bool IsBlockAt(Vector3Int pos) => IsBlockAt(pos.x, pos.y, pos.z);

    public List<Vector3Int> GetSurroundingBlocks(Vector3Int pos)
    {
        List<Vector3Int> directions = new List<Vector3Int>
        {
            Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down, Vector3Int.forward, Vector3Int.back
        };
        return directions.Where(dir => IsInsideChunk(pos + dir)).ToList();
    }
}