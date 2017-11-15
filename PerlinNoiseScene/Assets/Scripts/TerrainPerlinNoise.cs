using UnityEditor;
using UnityEngine;
using System.Collections;

public class TerrainPerlinNoise : MonoBehaviour
{

    public float Tiling = 10.0f;

    private PerlinNoise myPerlinNoiseGenerator;

    void Start()
    {
        Terrain obj = FindObjectOfType<Terrain>();

        myPerlinNoiseGenerator = new PerlinNoise();

        if (obj != null)
        {
            GenerateHeights(obj, Tiling);
        }

    }

    public void GenerateHeights(Terrain terrain, float tileSize)
    {
        float[,] heights = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];

        var perlinHeights = myPerlinNoiseGenerator.GenerateHeightMap(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight, 6);

        for (int i = 0; i < terrain.terrainData.size.x; i++)
        {
            for (int k = 0; k < terrain.terrainData.size.z; k++)
            {
                heights[i, k] = perlinHeights[i][k];
            }
        }

        /*for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                heights[i, k] = Mathf.PerlinNoise(((float)i / (float)terrain.terrainData.heightmapWidth) * tileSize, ((float)k / (float)terrain.terrainData.heightmapHeight) * tileSize) / 10.0f;
            }
        }*/

        terrain.terrainData.SetHeights(0, 0, heights);
    }
}