using UnityEngine;

public class ChunkData
{
    public bool[,,] voxelMap;

    public ChunkData()
    {
        voxelMap = new bool[ChunkManager.chunkSize, ChunkManager.chunkSize, ChunkManager.chunkSize];
    }
    public bool GetBlock(int x, int y, int z) => voxelMap[x, y, z];
    public bool GetBlock(Vector3Int pos) => GetBlock(pos.x, pos.y, pos.z);

    public void SetBlock(int x, int y, int z, bool value) => voxelMap[x, y, z] = value;
    public void SetBlock(Vector3Int pos, bool value) => SetBlock(pos.x, pos.y, pos.z, value);

    public bool IsInsideChunk(int x, int y, int z) =>
        x >= 0 && x < ChunkManager.chunkSize &&
        y >= 0 && y < ChunkManager.chunkSize &&
        z >= 0 && z < ChunkManager.chunkSize;
    public bool IsInsideChunk(Vector3Int pos) => IsInsideChunk(pos.x, pos.y, pos.z);

    public bool IsBlockAt(int x, int y, int z) => IsInsideChunk(x, y, z) && voxelMap[x, y, z];
    public bool IsBlockAt(Vector3Int pos) => IsBlockAt(pos.x, pos.y, pos.z);
}