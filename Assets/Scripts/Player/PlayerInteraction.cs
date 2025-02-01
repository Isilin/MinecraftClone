using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float reachDistance = 5f; // Distance max pour miner/placer
    public LayerMask voxelLayer; // Filtre pour ne toucher que les blocs
    public GameObject blockPrefab; // Préfabriqué du bloc à placer
    public Camera playerCamera;

    void Update()
    {
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * reachDistance, Color.red);
        if (Input.GetMouseButtonDown(0)) // Clic gauche pour miner
        {
            MineBlock();
        }
        else if (Input.GetMouseButtonDown(1)) // Clic droit pour placer un bloc
        {
            PlaceBlock();
        }
    }

    void MineBlock()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, reachDistance, voxelLayer))
        {
            Vector3 blockPos = hit.point - (hit.normal * 0.1f); // Légère correction pour éviter les erreurs de flottants
            Vector3Int gridPos = new Vector3Int(
                Mathf.FloorToInt(blockPos.x),
                Mathf.FloorToInt(blockPos.y),
                Mathf.FloorToInt(blockPos.z)
            );

            VoxelChunk chunk = FindChunk(gridPos);
            if (chunk != null)
            {
                chunk.RemoveBlock(gridPos);
            }
        }
    }

    void PlaceBlock()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, reachDistance, voxelLayer))
        {
            Vector3 blockPos = hit.point + (hit.normal * 0.1f); // Légère correction pour éviter les erreurs de flottants
            Vector3Int gridPos = new Vector3Int(
                Mathf.FloorToInt(blockPos.x),
                Mathf.FloorToInt(blockPos.y),
                Mathf.FloorToInt(blockPos.z)
            );

            VoxelChunk chunk = FindChunk(gridPos);
            if (chunk != null)
            {
                chunk.AddBlock(gridPos);
            }
        }
    }

    VoxelChunk FindChunk(Vector3Int worldPos)
    {
        Vector2Int chunkCoord = MapGeneration.Instance.WorldToChunk(worldPos);

        // Vérifier si le chunk existe dans `ChunkManager`
        if (ChunkManager.Instance.chunks.ContainsKey(chunkCoord))
        {
            return ChunkManager.Instance.chunks[chunkCoord].GetComponent<VoxelChunk>();
        }

        return null;
    }
}
