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
        float[][] heights = PerlinNoise.GetEmptyArray<float>(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);

        TerrainAreaInfo fieldsInfo = new TerrainAreaInfo(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight, octaves, 0, terrain.terrainData.heightmapWidth);

        var perlinHeights = myPerlinNoiseGenerator.GenerateHeightMap(fieldsInfo);

        var maximumMountianHeight = (heightForMountains / maximumHeight);
        var maximumFieledHeight = (heightOfFields / maximumHeight);

        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                var distanceToMountains = Mathf.Sqrt((mountainsCenter - i) * (mountainsCenter - i) + (mountainsCenter - k) * (mountainsCenter - k));

                // Mountainas areas
                if (distanceToMountains <= mountainRadius)
                {
                    var multiplierToMaxHeight = (float)(mountainRadius - distanceToMountains) / (float)mountainRadius;
                    var nextHeight = Mathf.Max(perlinHeights[i][k] * maximumMountianHeight * multiplierToMaxHeight, perlinHeights[i][k] * maximumFieledHeight);
                    heights[i][k] = nextHeight;
                }
                // Field area
                else
                {
                    heights[i][k] = perlinHeights[i][k] * maximumFieledHeight;
                }
            }
        }

        myPerlinNoiseGenerator.GenerateBlendTexture(heights);

        float[,] unityHeightMap = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];

        // Convert from float[][] to float[,] which Unity needs for height map
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                unityHeightMap[i, k] = heights[i][k];
            }
        }
        terrain.terrainData.SetHeights(0, 0, unityHeightMap);
    }
}