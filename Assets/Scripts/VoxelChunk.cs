using UnityEngine;
using System.Collections.Generic;

public class VoxelChunk : MonoBehaviour
{
    public Material material;
    private MeshFilter meshFilter;
    private Mesh mesh;
    public Vector3Int chunkPosition;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    private bool[,,] voxelMap;

    // Indices des textures dans l'atlas
    private Vector2Int grassTop = new Vector2Int(8, 13); // Texture herbe (haut)
    private Vector2Int grassSide = new Vector2Int(3, 15); // Texture herbe (côté)
    private Vector2Int dirt = new Vector2Int(2, 15); // Texture terre


    private int atlasSizeInBlocks = 16; // Nombre de textures par ligne dans l'atlas

    void Start()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        if (material != null)
        {
            gameObject.AddComponent<MeshRenderer>().material = material;
        }
        else
        {
            gameObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        }

        chunkPosition = new Vector3Int(
            Mathf.FloorToInt(transform.position.x / ChunkManager.chunkSize),
            0,
            Mathf.FloorToInt(transform.position.z / ChunkManager.chunkSize)
        );

        GenerateChunk();
        BuildMesh();

        gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
    }

    void GenerateChunk()
    {
        voxelMap = new bool[ChunkManager.chunkSize, ChunkManager.chunkSize, ChunkManager.chunkSize];

        for (int x = 0; x < ChunkManager.chunkSize; x++)
        {
            for (int z = 0; z < ChunkManager.chunkSize; z++)
            {
                float height = ChunkManager.Instance.GetTerrainHeight(new Vector2(x, z), new Vector2Int(chunkPosition.x, chunkPosition.z));

                for (int y = 0; y <= height; y++) // Remplit les blocs sous la surface
                {
                    voxelMap[x, y, z] = true; // Stocke l'existence d'un bloc
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
                    if (voxelMap[x, y, z])
                    {
                        AddCube(x, y, z);
                    }
                }
            }
        }
    }

    void AddCube(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z);
        int vertexIndex = vertices.Count;

        // Vérifie si un voisin cache cette face (évite IndexOutOfBounds)
        bool IsBlockAt(int nx, int ny, int nz)
        {
            if (nx < 0 || ny < 0 || nz < 0 || nx >= ChunkManager.chunkSize || ny >= ChunkManager.chunkSize || nz >= ChunkManager.chunkSize)
                return false; // Les blocs en bordure sont visibles
            return voxelMap[nx, ny, nz];
        }

        // Face avant
        if (!IsBlockAt(x, y, z - 1))
        {
            AddFace(pos, new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0), grassSide);
        }
        // Face arrière
        if (!IsBlockAt(x, y, z + 1))
        {
            AddFace(pos, new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 1), grassSide);
        }
        // Face bas
        if (!IsBlockAt(x, y - 1, z))
        {
            AddFace(pos, new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 0, 0), dirt);
        }
        // Face haut
        if (!IsBlockAt(x, y + 1, z))
        {
            AddFace(pos, new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1), grassTop);
        }
        // Face gauche
        if (!IsBlockAt(x - 1, y, z))
        {
            AddFace(pos, new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1), grassSide);
        }
        // Face droite
        if (!IsBlockAt(x + 1, y, z))
        {
            AddFace(pos, new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), grassSide);
        }
    }

    void AddFace(Vector3 pos, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2Int texturePos)
    {
        int vertexIndex = vertices.Count;

        // Ajout des sommets
        vertices.Add(pos + v1);
        vertices.Add(pos + v2);
        vertices.Add(pos + v3);
        vertices.Add(pos + v4);

        // Ajout des triangles
        triangles.Add(vertexIndex + 0);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 0);
        triangles.Add(vertexIndex + 3);
        triangles.Add(vertexIndex + 2);

        // Calcul des UVs en fonction de la texture dans l'atlas
        float texSize = 1f / atlasSizeInBlocks;
        Vector2 uvBase = new Vector2(texturePos.x * texSize, texturePos.y * texSize);

        uvs.Add(uvBase + new Vector2(0, 0));
        uvs.Add(uvBase + new Vector2(texSize, 0));
        uvs.Add(uvBase + new Vector2(texSize, texSize));
        uvs.Add(uvBase + new Vector2(0, texSize));
    }

    void BuildMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
    public void RemoveBlock(Vector3Int position)
    {
        Vector3Int localPos = WorldToLocal(position);

        if (IsInsideChunk(localPos))
        {
            voxelMap[localPos.x, localPos.y, localPos.z] = false;
            UpdateSurroundingBlocks(localPos);
            RebuildMesh();
        }
    }

    public void AddBlock(Vector3Int position)
    {
        Vector3Int localPos = WorldToLocal(position);

        if (IsInsideChunk(localPos))
        {
            voxelMap[localPos.x, localPos.y, localPos.z] = true;
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

    bool IsInsideChunk(Vector3Int localPos)
    {
        return localPos.x >= 0 && localPos.x < ChunkManager.chunkSize &&
               localPos.y >= 0 && localPos.y < ChunkManager.chunkSize &&
               localPos.z >= 0 && localPos.z < ChunkManager.chunkSize;
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
            if (IsInsideChunk(neighborPos))
            {
                AddCube(neighborPos.x, neighborPos.y, neighborPos.z); // Mettre à jour les faces des blocs voisins
            }
        }
    }

    void RebuildMesh()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        mesh.Clear();

        for (int x = 0; x < ChunkManager.chunkSize; x++)
        {
            for (int y = 0; y < ChunkManager.chunkSize; y++)
            {
                for (int z = 0; z < ChunkManager.chunkSize; z++)
                {
                    if (voxelMap[x, y, z])
                    {
                        AddCube(x, y, z);
                    }
                }
            }
        }

        ApplyMeshUpdates();
    }
    void ApplyMeshUpdates()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }
}
