using UnityEditor;
using UnityEngine;
using System.Collections;
using CustomPerlinNoise;

public class TerrainPerlinNoise : MonoBehaviour
{

    public float Tiling = 10.0f;
    public int octaves;
    public float maximumHeight;
    public float heightOfFields;
    public float heightForMountains;
    public int mountainsCenter;
    public float mountainRadius;

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
        TerrainAreaInfo fieldsInfo = new TerrainAreaInfo(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight, octaves, 0, terrain.terrainData.heightmapWidth);
        TerrainAreaInfo mountainInfo = new TerrainAreaInfo(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight, octaves, 0, terrain.terrainData.heightmapWidth);

        var perlinHeights = myPerlinNoiseGenerator.GenerateHeightMap(fieldsInfo);

        var maximumMountianHeight = (heightForMountains / maximumHeight);
        var maximumFieledHeight = (heightOfFields / maximumHeight);

        for (int i = 0; i < terrain.terrainData.size.x; i++)
        {
            for (int k = 0; k < terrain.terrainData.size.z; k++)
            {
                var distanceToMountains = Mathf.Sqrt((mountainsCenter - i) * (mountainsCenter - i) + (mountainsCenter - k) * (mountainsCenter - k));

                // Mountainas areas
                if (distanceToMountains <= mountainRadius)
                {
                    var multiplierToMaxHeight = (float)(mountainRadius - distanceToMountains) / (float)mountainRadius;
                    var nextHeight = Mathf.Max(perlinHeights[i][k] * maximumMountianHeight * multiplierToMaxHeight, perlinHeights[i][k] * maximumFieledHeight);
                    heights[i, k] = nextHeight;
                }
                // Field area
                else
                {
                    heights[i, k] = perlinHeights[i][k] * maximumFieledHeight;
                }
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