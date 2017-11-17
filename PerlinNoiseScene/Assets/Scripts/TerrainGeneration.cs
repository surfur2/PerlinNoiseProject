using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomPerlinNoise;

/// <summary>
/// Class exposing all parameters used for generating the fields and mountains biomes.
/// </summary>
public class TerrainGeneration : MonoBehaviour
{
    // Noise generation for heightmap
    public int octaves; // Octaves for heightmap noise
    public float maximumHeight; // Maximum height of any point in the terrain
    public float heightOfFields; // Maximum height of any point in the fields
    public float heightForMountains; // Max imum height for any point in the mountians.
    public int mountainsCenter; // Center of the mountains
    public float mountainRadius; // radius for the mountians

    // Image blending for the overall terrain texture.
    public float grassBlend; // Height threshhold for onlt grass.
    public float mountainBlend; // Height threshhold for only mountain.
    public float snowBlend; // Height threshhold for only snow.

    // Foliage generation
    public int[] treeIndicies; // Array of tress to use
    public float threshholdForTree; // Control on amount of trees spawned
    public int[] shrubIndicies;
    public float threshholdForShrub;
    public int[] rockIndicies;
    public float threshholdForRock;

    private PerlinNoise myPerlinNoiseGenerator; // Class for generating perlin noise

    private Terrain terrain;

    void Start()
    {
        terrain = FindObjectOfType<Terrain>();

        myPerlinNoiseGenerator = new PerlinNoise();

        if (terrain != null)
        {
            GenerateHeights();
            GenerateFoliage();
        }

    }

    /// <summary>
    /// Create a heightmap for the terrain using perlin noise generator.
    /// Create a blend texture based on this heightmap
    /// Save the texture and assign the heightmap to the unity terrain system
    /// </summary>
    public void GenerateHeights()
    {
        float[][] heights = PerlinNoise.GetEmptyArray<float>(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);

        TerrainAreaInfo densityInfo = new TerrainAreaInfo(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight, octaves, 0, terrain.terrainData.heightmapWidth);

        var perlinHeights = myPerlinNoiseGenerator.GenerateHeightMap(densityInfo);

        // Create a percentage amplitude multiplier for each area of the map that will anchor the maximum height it can be.
        var maximumMountianHeight = (heightForMountains / maximumHeight);
        var maximumFieledHeight = (heightOfFields / maximumHeight);

        // Multiply our raw perlin noise by the multiplier based on the area it is in on the map.
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                var distanceToMountains = Mathf.Sqrt((mountainsCenter - i) * (mountainsCenter - i) + (mountainsCenter - k) * (mountainsCenter - k));

                // Mountainas areas
                if (distanceToMountains <= mountainRadius)
                {
                    // Grow the height of mountain areas based on how far from the center we are.
                    // This aalso causes the smooth transition from fields to mountain.
                    var multiplierToMaxHeight = (float)(mountainRadius - distanceToMountains) / (float)mountainRadius;
                    // Make sure the height choosen is never less than field height or we have a hold in the gorund
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

        // Pass out "custom" perlin noise into the texture blend algorithm so that it knows what area should be snow, mountain, or grass.
        var newTexture = myPerlinNoiseGenerator.GenerateBlendTexture(heights, grassBlend, mountainBlend, snowBlend);

        // Convert from float[][] to float[,] which Unity needs for height map
        float[,] unityHeightMap = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                unityHeightMap[i, k] = heights[i][k];
            }
        }

        // Set the heightmap on unity terrain.
        terrain.terrainData.SetHeights(0, 0, unityHeightMap);
    }

    /// <summary>
    /// Generate foliage on the ground based on perlin noise density.
    /// </summary>
    public void GenerateFoliage()
    {
        // To avoid clusters of trees, bushes, and rocks that overlap we are actually generating less noise than the heightmap and then spreading our density out by a factor.
        var densityMapReductionFactor = 8;
        var densityMapWidth = terrain.terrainData.heightmapHeight / densityMapReductionFactor;

        // This perlin noise only has octave 1. Basically areas with a lot of folliage and some with none.
        TerrainAreaInfo densityInfo = new TerrainAreaInfo(densityMapWidth, densityMapWidth, 1, 0, densityMapWidth);

        var perlinFoliageDensity = myPerlinNoiseGenerator.GenerateHeightMap(densityInfo);

        // Map the density information to the heightmap information, spawn the items in the correct amounts passed on threshholds from inputs, and only spawn some items in certain areas.
        List<TreeInstance> treeData = new List<TreeInstance>();
        for (int i = 0; i < densityMapWidth; i++)
        {
            for (int k = 0; k < densityMapWidth; k++)
            {
                var currentDensity = perlinFoliageDensity[i][k];

                var distanceToMountains = ((mountainsCenter - (densityMapReductionFactor * i)) * (mountainsCenter - (densityMapReductionFactor * i)) + (mountainsCenter - (densityMapReductionFactor * k)) * (mountainsCenter - (densityMapReductionFactor * k)));

                // Field area
                if (distanceToMountains > (mountainRadius * mountainRadius))
                { 
                    // Spawn a tree
                    if (currentDensity > threshholdForTree)
                    {
                        var randomPositionOffset = Random.Range(-4, 4);

                        var terrainSpaceX = i * densityMapReductionFactor + randomPositionOffset;
                        var terrainSpaceZ = k * densityMapReductionFactor + randomPositionOffset;

                        TreeInstance tree = GetDefaultTerrainEntity(terrainSpaceX, terrainSpaceZ, treeIndicies);
                        treeData.Add(tree);
                    }

                    // Spawn foliage
                    if (currentDensity > threshholdForShrub)
                    {
                        var randomPositionOffset = Random.Range(-4, 4);
                        var terrainSpaceX = i * densityMapReductionFactor + randomPositionOffset;
                        var terrainSpaceZ = k * densityMapReductionFactor + randomPositionOffset;

                        TreeInstance shrubData = GetDefaultTerrainEntity(terrainSpaceX, terrainSpaceZ, shrubIndicies);
                        treeData.Add(shrubData);
                    }
                }
                // Mountain area
                else
                {
                    // Spawn rocks
                    if (currentDensity > threshholdForRock)
                    {
                        var randomPositionOffset = Random.Range(-4, 4);
                        var terrainSpaceX = i * densityMapReductionFactor + randomPositionOffset;
                        var terrainSpaceZ = k * densityMapReductionFactor + randomPositionOffset;

                        TreeInstance rockData = GetDefaultTerrainEntity(terrainSpaceX, terrainSpaceZ, rockIndicies);
                        treeData.Add(rockData);
                    }
                }
            }
        }

        // Set the list of terrain items.
        terrain.terrainData.treeInstances = treeData.ToArray();
    }

    // Helper method for setting data for an entity spawned in the terrain system.
    // Not all of the items are actually "trees"
    private TreeInstance GetDefaultTerrainEntity(float terrainSpaceX, float terrainSpaceZ, int[] indexArray)
    {
        TreeInstance terrainData = new TreeInstance();
        terrainData.prototypeIndex = indexArray[Random.Range(0, indexArray.Length)];
        terrainData.color = new Color(1, 1, 1);
        terrainData.lightmapColor = new Color(1, 1, 1);
        terrainData.heightScale = 1;
        terrainData.widthScale = 1;
        terrainData.position = new Vector3((terrainSpaceX / (float)terrain.terrainData.heightmapHeight),
            (terrain.terrainData.GetHeight((int)terrainSpaceX, (int)terrainSpaceZ) / terrain.terrainData.size.y),
            (terrainSpaceZ / (float)terrain.terrainData.heightmapHeight)
        );

        return terrainData;
    }
}