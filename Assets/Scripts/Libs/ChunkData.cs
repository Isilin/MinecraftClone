using UnityEngine;

public class ChunkData
{
    public Vector2Int position;
    public bool[,,] voxelMap;

    public ChunkData(Vector2Int position, bool[,,] voxelMap)
    {
        this.position = position;
        this.voxelMap = voxelMap;
    }
}