using UnityEngine;
using System.Collections.Generic;

public class VoxelMeshBuilder
{
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;
    private int atlasSizeInBlocks;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public VoxelMeshBuilder(GameObject chunkObject, Material material, int atlasSize)
    {
        // Initialisation des composants Mesh
        meshFilter = chunkObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = chunkObject.AddComponent<MeshFilter>();

        meshCollider = chunkObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = chunkObject.AddComponent<MeshCollider>();

        MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = material;
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Initialisation des listes
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        atlasSizeInBlocks = atlasSize;
    }

    // Ajoute une face avec la texture correspondante
    public void AddFace(Vector3 pos, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2Int texturePos)
    {
        int vertexIndex = vertices.Count;

        // Ajouter les sommets
        vertices.Add(pos + v1);
        vertices.Add(pos + v2);
        vertices.Add(pos + v3);
        vertices.Add(pos + v4);

        // Ajouter les triangles
        triangles.Add(vertexIndex + 0);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 0);
        triangles.Add(vertexIndex + 3);
        triangles.Add(vertexIndex + 2);

        // Ajouter les UVs en fonction de la texture
        float texSize = 1f / atlasSizeInBlocks;
        Vector2 uvBase = new Vector2(texturePos.x * texSize, texturePos.y * texSize);

        uvs.Add(uvBase + new Vector2(0, 0));
        uvs.Add(uvBase + new Vector2(texSize, 0));
        uvs.Add(uvBase + new Vector2(texSize, texSize));
        uvs.Add(uvBase + new Vector2(0, texSize));
    }

    // Applique les modifications au mesh
    public void ApplyMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        // Mise à jour du MeshCollider
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    // Nettoie les listes (pour une reconstruction de mesh)
    public void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    public void AddCube(Vector3Int pos, ChunkData data, TextureAtlas.Block block)
    {
        // Ajout des faces seulement si elles sont visibles
        if (!data.IsBlockAt(pos.x, pos.y, pos.z - 1)) AddFace(pos, new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0), block.side);
        if (!data.IsBlockAt(pos.x, pos.y, pos.z + 1)) AddFace(pos, new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 1), block.side);
        if (!data.IsBlockAt(pos.x, pos.y - 1, pos.z)) AddFace(pos, new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 0, 0), block.bottom);
        if (!data.IsBlockAt(pos.x, pos.y + 1, pos.z)) AddFace(pos, new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1), block.top);
        if (!data.IsBlockAt(pos.x - 1, pos.y, pos.z)) AddFace(pos, new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1), block.side);
        if (!data.IsBlockAt(pos.x + 1, pos.y, pos.z)) AddFace(pos, new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), block.side);
    }

}
