using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CustomPerlinNoise;

public class TerrainPerlinNoise : MonoBehaviour
{

    public int heightmapWidth;
    public int heightmapLength;

    public int octaves;
    public float maximumHeight;
    public float heightOfFields;
    public float heightForMountains;
    public int mountainsCenter;
    public float mountainRadius;
    public float grassBlend;
    public float mountainBlend;
    public float snowBlend;

    public List<Terrain> terrain = new List<Terrain>();

    private PerlinNoise myPerlinNoiseGenerator;

    void Start()
    {

        myPerlinNoiseGenerator = new PerlinNoise();

        if (terrain != null)
        {
            GenerateHeights(terrain);
        }

    }

    public void GenerateHeights(List<Terrain> terrain)
    {
        float[][] heights = PerlinNoise.GetEmptyArray<float>(heightmapWidth, heightmapLength);

        TerrainAreaInfo fieldsInfo = new TerrainAreaInfo(heightmapWidth, heightmapLength, octaves, 0, heightmapWidth);

        var perlinHeights = myPerlinNoiseGenerator.GenerateHeightMap(fieldsInfo);

        var maximumMountianHeight = (heightForMountains / maximumHeight);
        var maximumFieledHeight = (heightOfFields / maximumHeight);

        for (int i = 0; i < heightmapWidth; i++)
        {
            for (int k = 0; k < heightmapWidth; k++)
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

        myPerlinNoiseGenerator.GenerateBlendTexture(heights, grassBlend, mountainBlend, snowBlend, terrain.Count);

        




        // Map a section of the heightmap to a section of the terrain we are using
        int numberTerrainOneSide = Mathf.Max(1, terrain.Count / 2);
        int unityUnitsPerSide = heightmapLength / numberTerrainOneSide;

        for (int l = 0; l < numberTerrainOneSide; l++)
        {
            for (int m = 0; m < numberTerrainOneSide; m++)
            {
                float[,] unityHeightMap = new float[unityUnitsPerSide, unityUnitsPerSide];

                for (int i = 0; i < unityUnitsPerSide; i++)
                {
                    for (int k = 0; k < unityUnitsPerSide; k++)
                    {
                        unityHeightMap[i, k] = heights[i + (l * unityUnitsPerSide)][k + (m * unityUnitsPerSide)];
                    }
                }
                terrain[l + 2*m].terrainData.SetHeights(0, 0, unityHeightMap);
            }
        }
    }
}