﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CustomPerlinNoise
{
    public struct TerrainAreaInfo
    {
        public int width;
        public int height;
        public int octaveCount;
        public int beginningIndex;
        public int endingIndex;

        public TerrainAreaInfo(int _width, int _height, int _octaveCount, int _beginningIndex, int _endingIndex)
        {
            width = _width;
            height = _height;
            octaveCount = _octaveCount;
            beginningIndex = _beginningIndex;
            endingIndex = _endingIndex;
        }
    }

    // Base code taken from http://devmag.org.za/2009/04/25/perlin-noise/
    // Modifications made to the noise generation functions and the inputs to the noise generating functions.
    public class PerlinNoise
    {
        static System.Random random = new System.Random();
       
        public float[][] GenerateHeightMap(TerrainAreaInfo fieldTerrainInfo)
        {
            Color32 gradientStart = new Color32(255, 0, 0, 255);
            Color32 gradientEnd = new Color32(255, 0, 255, 255);

            float[][] perlinNoise = GeneratePerlinNoise(fieldTerrainInfo);
            Color32[][] perlinImage = MapGradient(gradientStart, gradientEnd, perlinNoise);
            SaveImage(perlinImage, "perlin_noise.png");

            return perlinNoise;
        }

        public Texture2D GenerateBlendTexture(float[][] perlinNoise, float lowerClamp, float highClamp, float highestClamp)
        {
            Color32[][] image1 = LoadImage("snowTexture");
            Color32[][] image2 = LoadImage("rockTexture");
            Color32[][] image3 = LoadImage("grassTexture");

            int textureQuality = 4;
            int desiredTextureWidth = (perlinNoise.Length - 1) * textureQuality;
            int desiredTextureHeight = (perlinNoise[0].Length - 1) * textureQuality;

            // Blend the first two images
            var blendImageNoise = AdjustLevels(perlinNoise, lowerClamp, highClamp, desiredTextureWidth, desiredTextureHeight);
            Color32[][] perlinImageBlendLevelOne = BlendImages(image2, image3, blendImageNoise, desiredTextureWidth, desiredTextureHeight);

            // Blend the resulting image and the last image
            blendImageNoise = AdjustLevels(perlinNoise, highClamp, highestClamp, desiredTextureWidth, desiredTextureHeight);
            Color32[][] perlinImageBlendLevelTwo = BlendImages(image1, perlinImageBlendLevelOne, blendImageNoise, desiredTextureWidth, desiredTextureHeight);

            var newTexture = SaveImage(perlinImageBlendLevelTwo, "perlin_noise_blended.png");

            return newTexture;
        }

        #region Reusable Functions

        public static float[][] GenerateWhiteNoise(int width, int height)
        {
            float[][] noise = GetEmptyArray<float>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    noise[i][j] = (float)random.NextDouble() % 1;
                }
            }

            return noise;
        }

        public static float Interpolate(float x0, float x1, float alpha)
        {
            return x0 * (1 - alpha) + alpha * x1;
        }

        public static Color32 Interpolate(Color32 col0, Color32 col1, float alpha)
        {
            float beta = 1 - alpha;
            return new Color32(
                (byte)(col0.r * alpha + col1.r * beta),
                (byte)(col0.g * alpha + col1.g * beta),
                (byte)(col0.b * alpha + col1.b * beta),
                255);
        }

        public static Color32 Interpolate(Color32 col0, Color32 col1, Color32 col2, float alpha)
        {
            float beta = 1 - alpha;
            return new Color32(
                (byte)(col0.r * alpha + col1.r * beta),
                (byte)(col0.g * alpha + col1.g * beta),
                (byte)(col0.b * alpha + col1.b * beta),
                255);
        }

        public static Color32 GetColor(Color32 gradientStart, Color32 gradientEnd, float t)
        {
            float u = 1 - t;

            Color32 color = new Color32(
                (byte)(gradientStart.r * u + gradientEnd.r * t),
                (byte)(gradientStart.g * u + gradientEnd.g * t),
                (byte)(gradientStart.b * u + gradientEnd.b * t),
                255);

            return color;
        }

        public static Color32[][] MapGradient(Color32 gradientStart, Color32 gradientEnd, float[][] perlinNoise)
        {
            int width = perlinNoise.Length;
            int height = perlinNoise[0].Length;

            Color32[][] image = GetEmptyArray<Color32>(width, height); //an array of colours

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image[i][j] = GetColor(gradientStart, gradientEnd, perlinNoise[i][j]);
                }
            }

            return image;
        }

        public static T[][] GetEmptyArray<T>(int width, int height)
        {
            T[][] image = new T[width][];

            for (int i = 0; i < width; i++)
            {
                image[i] = new T[height];
            }

            return image;
        }

        public static float[][] GenerateSmoothNoise(float[][] baseNoise, int octave)
        {
            int width = baseNoise.Length;
            int height = baseNoise[0].Length;

            float[][] smoothNoise = GetEmptyArray<float>(width, height);

            int samplePeriod = 1 << octave; // calculates 2 ^ k
            samplePeriod *= 4;
            float sampleFrequency = 1.0f / samplePeriod;

            for (int i = 0; i < width; i++)
            {
                //calculate the horizontal sampling indices
                int sample_i0 = (i / samplePeriod) * samplePeriod;
                int sample_i1 = (sample_i0 + samplePeriod) % width; //wrap around
                float horizontal_blend = (i - sample_i0) * sampleFrequency;

                for (int j = 0; j < height; j++)
                {
                    //calculate the vertical sampling indices
                    int sample_j0 = (j / samplePeriod) * samplePeriod;
                    int sample_j1 = (sample_j0 + samplePeriod) % height; //wrap around
                    float vertical_blend = (j - sample_j0) * sampleFrequency;

                    //blend the top two corners
                    float top = Interpolate(baseNoise[sample_i0][sample_j0],
                        baseNoise[sample_i1][sample_j0], horizontal_blend);

                    //blend the bottom two corners
                    float bottom = Interpolate(baseNoise[sample_i0][sample_j1],
                        baseNoise[sample_i1][sample_j1], horizontal_blend);

                    //final blend
                    smoothNoise[i][j] = Interpolate(top, bottom, vertical_blend);
                }
            }

            return smoothNoise;
        }

        public static float[][] GeneratePerlinNoise(float[][] baseNoise, TerrainAreaInfo fieldInfo)
        {
            int width = baseNoise.Length;
            int height = baseNoise[0].Length;

            float[][][] smoothNoise = new float[fieldInfo.octaveCount][][]; //an array of 2D arrays containing

            float persistance = 0.5f;

            //generate smooth noise
            for (int i = 0; i < fieldInfo.octaveCount; i++)
            {
                smoothNoise[i] = GenerateSmoothNoise(baseNoise, i);
            }

            float[][] perlinNoise = GetEmptyArray<float>(width, height); //an array of floats initialised to 0

            float amplitude = persistance;
            float totalAmplitude = 0.0f;

            //blend noise together
            for (int octave = fieldInfo.octaveCount - 1; octave >= 0; octave--)
            {
                amplitude *= persistance;
                totalAmplitude += amplitude;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        perlinNoise[i][j] += smoothNoise[octave][i][j] * amplitude;
                    }
                }
            }

            //normalisation
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    perlinNoise[i][j] /= totalAmplitude;
                }
            }

            return perlinNoise;
        }

        public static float[][] GeneratePerlinNoise(TerrainAreaInfo fieldsInfo)
        {
            float[][] baseNoise = GenerateWhiteNoise(fieldsInfo.width, fieldsInfo.height);

            return GeneratePerlinNoise(baseNoise, fieldsInfo);
        }

        public static Color[][] MapToGrey(float[][] greyValues)
        {
            int width = greyValues.Length;
            int height = greyValues[0].Length;

            Color[][] image = GetEmptyArray<Color>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    byte grey = (byte)(255 * greyValues[i][j]);
                    Color32 color = new Color32(grey, grey, grey, 255);

                    image[i][j] = color;
                }
            }

            return image;
        }

        public Texture2D SaveImage(Color32[][] image, string fileName)
        {
            int width = image.Length;
            int height = image[0].Length;

            Texture2D texture = new Texture2D(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    texture.SetPixel(i, j, image[i][j]);
                }
            }

            var bytes = texture.EncodeToPNG();
            var file = File.Open(Application.dataPath + "/" + fileName, FileMode.OpenOrCreate);
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();

            return texture;
        }

        public static float[][] AdjustLevels(float[][] image, float low, float high, int textureWidth, int textureHeight)
        {
            float[][] newImage = GetEmptyArray<float>(textureWidth, textureHeight);

            int imageInterpolationFactorImage = Mathf.Max(1, textureWidth / (image.Length - 1));

            for (int i = 0; i < textureWidth; i++)
            {
                for (int j = 0; j < textureHeight; j++)
                {
                    float col = image[i/ imageInterpolationFactorImage][j / imageInterpolationFactorImage];

                    if (col <= low)
                    {
                        newImage[i][j] = 0;
                    }
                    else if (col >= high)
                    {
                        newImage[i][j] = 1;
                    }
                    else
                    {
                        newImage[i][j] = (col - low) / (high - low);
                    }
                }
            }

            return newImage;
        }

        public static Color32[][] BlendImages(Color32[][] image1, Color32[][] image2, float[][] perlinNoise, int textureWidth, int textureHeight)
        {
            int imageInterpolationFactorImageOne = Mathf.Max(1, textureWidth / image1.Length);
            int imageInterpolationFactorImageTwo = Mathf.Max(1, textureWidth / image2.Length);
            Color32[][] image = GetEmptyArray<Color32>(textureWidth, textureHeight); //an array of colours for the new image

            for (int i = 0; i < textureWidth; i++)
            {
                for (int j = 0; j < textureHeight; j++)
                {
                    image[i][j] = Interpolate(image1[i/ imageInterpolationFactorImageOne][j/ imageInterpolationFactorImageOne], image2[i/ imageInterpolationFactorImageTwo][j/ imageInterpolationFactorImageTwo], perlinNoise[i][j]);
                }
            }

            return image;
        }

        public static Color32[][] LoadImage(string fileName)
        {
            Texture2D texture = Resources.Load(fileName) as Texture2D;

            int width = texture.width;
            int height = texture.height;

            Color32[][] image = GetEmptyArray<Color32>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image[i][j] = (Color32)texture.GetPixel(i, j);
                }
            }

            return image;
        }
        #endregion
    }
}
