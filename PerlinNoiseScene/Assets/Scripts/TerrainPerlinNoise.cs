using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomPerlinNoise;

public class TerrainPerlinNoise : MonoBehaviour
{

    public int octaves;
    public float maximumHeight;
    public float heightOfFields;
    public float heightForMountains;
    public int mountainsCenter;
    public float mountainRadius;
    public float grassBlend;
    public float mountainBlend;
    public float snowBlend;

    public int[] treeIndicies;
    public float threshholdForTree;
    public int[] shrubIndicies;
    public float threshholdForShrub;
    private PerlinNoise myPerlinNoiseGenerator;

    void Start()
    {
        Terrain obj = FindObjectOfType<Terrain>();

        myPerlinNoiseGenerator = new PerlinNoise();

        if (obj != null)
        {
            GenerateHeights(obj);
            GenerateFoliage(obj);
        }

    }

    public void GenerateHeights(Terrain terrain)
    {
        float[][] heights = PerlinNoise.GetEmptyArray<float>(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);

        TerrainAreaInfo densityInfo = new TerrainAreaInfo(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight, octaves, 0, terrain.terrainData.heightmapWidth);

        var perlinHeights = myPerlinNoiseGenerator.GenerateHeightMap(densityInfo);

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

        var newTexture = myPerlinNoiseGenerator.GenerateBlendTexture(heights, grassBlend, mountainBlend, snowBlend);

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

    public void GenerateFoliage(Terrain terrain)
    {
        var densityMapReductionFactor = 16;
        var densityMapWidth = terrain.terrainData.heightmapHeight / densityMapReductionFactor;

        TerrainAreaInfo fieldsInfo = new TerrainAreaInfo(densityMapWidth, densityMapWidth, 1, 0, densityMapWidth);

        var perlinFoliageDensity = myPerlinNoiseGenerator.GenerateHeightMap(fieldsInfo);

        // Convert from float[][] to float[,] which Unity needs for height map
        List<TreeInstance> treeData = new List<TreeInstance>();
        for (int i = 0; i < densityMapWidth; i++)
        {
            for (int k = 0; k < densityMapWidth; k++)
            {
                var currentDensity = perlinFoliageDensity[i][k];
                if (currentDensity > threshholdForTree)
                {
                    var randomPositionOffset = Random.Range(-4, 4);
                    var terrainSpaceX = i * densityMapReductionFactor + randomPositionOffset;
                    var terrainSpaceZ = k * densityMapReductionFactor + randomPositionOffset;

                    TreeInstance newTree = new TreeInstance();
                    newTree.prototypeIndex = treeIndicies[Random.Range(0, treeIndicies.Length)];
                    newTree.color = new Color(1, 1, 1);
                    newTree.lightmapColor = new Color(1, 1, 1);
                    newTree.heightScale = 1;
                    newTree.widthScale = 1;
                    newTree.position = new Vector3(((float)terrainSpaceX / (float)terrain.terrainData.heightmapHeight), 
                        ((float)terrain.terrainData.GetHeight(terrainSpaceX, terrainSpaceZ) / (float)terrain.terrainData.size.y), 
                        ((float)terrainSpaceZ / (float)terrain.terrainData.heightmapHeight)
                    );
                
                    treeData.Add(newTree);
                }

                if (currentDensity < threshholdForShrub)
                {
                    var randomPositionOffset = Random.Range(-4, 4);
                    var terrainSpaceX = i * densityMapReductionFactor + randomPositionOffset;
                    var terrainSpaceZ = k * densityMapReductionFactor + randomPositionOffset;

                    TreeInstance shrubData = new TreeInstance();
                    shrubData.prototypeIndex = shrubIndicies[Random.Range(0, shrubIndicies.Length)];
                    shrubData.color = new Color(1, 1, 1);
                    shrubData.lightmapColor = new Color(1, 1, 1);
                    shrubData.heightScale = 1;
                    shrubData.widthScale = 1;
                    shrubData.position = new Vector3(((float)terrainSpaceX / (float)terrain.terrainData.heightmapHeight),
                        ((float)terrain.terrainData.GetHeight(terrainSpaceX, terrainSpaceZ) / (float)terrain.terrainData.size.y),
                        ((float)terrainSpaceZ / (float)terrain.terrainData.heightmapHeight)
                    );

                    treeData.Add(shrubData);
                }
            }
        }

        terrain.terrainData.treeInstances = treeData.ToArray();
    }
}