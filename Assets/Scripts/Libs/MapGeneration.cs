using UnityEngine;

public class MapGeneration
{
    private float scale;  // Échelle du bruit (valeur ajustable)
    private float scaleBis;
    private int heightMultiplier;  // Hauteur max des montagnes
    private int heightMultiplierBis;

    private int worldSeed;

    public MapGeneration(int seed, float scale = 0.05f, float scaleBis = 0.01f, int heightMultiplier = 10, int heightMultiplierBis = 5)
    {
        this.worldSeed = seed;
        this.scale = scale;
        this.scaleBis = scaleBis;
        this.heightMultiplier = heightMultiplier;
        this.heightMultiplierBis = heightMultiplierBis;
    }


    public float GetHeight(Vector2 pos, Vector2 chunkPos)
    {
        float heightBase = Mathf.PerlinNoise((pos.x + chunkPos.x * ChunkManager.chunkSize + worldSeed) * scale,
                                 (pos.y + chunkPos.y * ChunkManager.chunkSize + worldSeed) * scale) * heightMultiplier;

        float heightVariation = Mathf.PerlinNoise(
            (pos.x + ChunkManager.worldSeed) * scaleBis,
            (pos.y + ChunkManager.worldSeed) * scaleBis
        ) * heightMultiplierBis;

        float height = heightBase + heightVariation;
        return height;
    }
}
