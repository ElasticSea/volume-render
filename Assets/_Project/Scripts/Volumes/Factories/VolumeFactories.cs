using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ElasticSea.Framework.Util;
using UnityEditor;
using UnityEngine;
using Volumes.Imports;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Volumes.Factories
{
    public class VolumeFactories
    {

#if UNITY_EDITOR
        [MenuItem("Factory/Perfect Sphere")]
#endif
        public static void PerfectSphere()
        {
            var size = 1024;
            RunVolume("perfectSphere", VolumeFormat.Gray8, new Vector3Int(size, size, size), (x, y, z) =>
            {
                var center = new Vector3(0.5f, 0.5f, 0.5f);
                var voxel = new Vector3(x, y, z);
                var dir = voxel - center;

                var threahold = size / 4;
                var value = Mathf.SmoothStep(0.05f, 0.01f, Mathf.InverseLerp(threahold - 1f, threahold + 1f, dir.magnitude));
                return value;
            });
        }


#if UNITY_EDITOR
        [MenuItem("Factory/RedBlueSphere1536")]
#endif
        public static void RedBlueSphere1536()
        {
            var resolution = 1536;
            var noiseScale = 5f;
            var edgeWidth = 0.1f;
            var sphereWidth = 0.2f;
            var middleSphere = 0.5f - sphereWidth / 2;
            var colorA = Color.red;
            var colorB = Color.blue;
            var colorOpacity = 0.01f;
            var colors = new[]
            {
                Color.clear, Color.clear,
                colorA.SetAlpha(0f), colorA.SetAlpha(colorOpacity),
                colorB, colorB,
                colorA.SetAlpha(colorOpacity), colorA.SetAlpha(0f),
                Color.clear, Color.clear,
            };

            NoiseSphere("RedBlueSphere1536", resolution, noiseScale, edgeWidth, middleSphere, sphereWidth, colors);
        }


#if UNITY_EDITOR
        [MenuItem("Factory/RedBlueSphere1024")]
#endif
        public static void RedBlueSphere1024()
        {
            var resolution = 1024;
            var noiseScale = 5f;
            var edgeWidth = 0.1f;
            var sphereWidth = 0.2f;
            var middleSphere = 0.5f - sphereWidth / 2;
            var colorA = Color.red;
            var colorB = Color.blue;
            var colorOpacity = 0.01f;
            var colors = new[]
            {
                Color.clear, Color.clear,
                colorA.SetAlpha(0f), colorA.SetAlpha(colorOpacity),
                colorB, colorB,
                colorA.SetAlpha(colorOpacity), colorA.SetAlpha(0f),
                Color.clear, Color.clear,
            };

            NoiseSphere("RedBlueSphere1024", resolution, noiseScale, edgeWidth, middleSphere, sphereWidth, colors);
        }
       
#if UNITY_EDITOR 
        [MenuItem("Factory/RedBlueSphere128")]
#endif
        public static void RedBlueSphere128()
        {
            var resolution = 128;
            var noiseScale = 5f;
            var edgeWidth = 0.1f;
            var sphereWidth = 0.2f;
            var middleSphere = 0.5f - sphereWidth / 2;
            var colorA = Color.red;
            var colorB = Color.blue;
            var colorOpacity = 0.01f;
            var colors = new[]
            {
                Color.clear, Color.clear,
                colorA.SetAlpha(0f), colorA.SetAlpha(colorOpacity),
                colorB, colorB,
                colorA.SetAlpha(colorOpacity), colorA.SetAlpha(0f),
                Color.clear, Color.clear,
            };

            NoiseSphere("RedBlueSphere128", resolution, noiseScale, edgeWidth, middleSphere, sphereWidth, colors);
        }
        
#if UNITY_EDITOR
        [MenuItem("Factory/RedBlueSphere512")]
#endif
        public static void RedBlueSphere512()
        {
            var resolution = 512;
            var noiseScale = 5f;
            var edgeWidth = 0.1f;
            var sphereWidth = 0.2f;
            var middleSphere = 0.5f - sphereWidth / 2;
            var colorA = Color.red;
            var colorB = Color.blue;
            var colorOpacity = 0.01f;
            var colors = new[]
            {
                Color.clear, Color.clear,
                colorA.SetAlpha(0f), colorA.SetAlpha(colorOpacity),
                colorB, colorB,
                colorA.SetAlpha(colorOpacity), colorA.SetAlpha(0f),
                Color.clear, Color.clear,
            };

            NoiseSphere("RedBlueSphere512", resolution, noiseScale, edgeWidth, middleSphere, sphereWidth, colors);
        }
        
        private static void NoiseSphere(string name, int resolution, float noiseScale, float edgeWidth, float sphereRadius, float sphereWidth, Color[] colors)
        {
            var noise = new OpenSimplexNoise();
            RunVolume(name, VolumeFormat.RGBA32, new Vector3Int(resolution, resolution, resolution), (x, y, z) =>
            {
                var noiseValue = (float) noise.Evaluate(x * noiseScale, y * noiseScale, z * noiseScale) /2 + 0.5f;

                var dir = new Vector3(x, y, z) - new Vector3(0.5f, 0.5f, 0.5f);
                
                // sub sphere outside
                var minT0 = sphereRadius + sphereWidth / 2 - edgeWidth / 2f;
                var maxT0 = sphereRadius + sphereWidth / 2 + edgeWidth / 2f;
                var dd0 = Mathf.InverseLerp(minT0, maxT0, dir.magnitude);
                noiseValue = Mathf.SmoothStep(noiseValue, 0, dd0);
                
                // sub sphere inside
                // minT0 = middleSphere - sphereWidth / 2 - edgeWidth / 2f;
                // maxT0 = middleSphere - sphereWidth / 2 + edgeWidth / 2f;
                // dd0 = Mathf.InverseLerp(minT0, maxT0, dir.magnitude);
                // noiseValue = Mathf.SmoothStep(0, noiseValue, dd0);
                
                var rr = noiseValue * (colors.Length);
                var closestBoundary = (Mathf.RoundToInt(rr));
                var minT = closestBoundary - edgeWidth / 2f;
                var maxT = closestBoundary + edgeWidth / 2f;
                var dd = Mathf.InverseLerp(minT, maxT, rr);
                var dd2 = Mathf.SmoothStep(0, 1, dd);
                
                if (closestBoundary > rr)
                {
                    var ca = colors[(int) rr];
                    var cb = colors[Mathf.Min((int) rr + 1, colors.Length-1)];
                    return Color.Lerp(ca, cb, dd2);
                }
                else
                {
                    var ca = colors[Mathf.Max((int) rr - 1, 0)];
                    var cb = colors[(int) rr ];
                    return Color.Lerp(ca, cb, dd2);
                }
            });
        }
        
#if UNITY_EDITOR
        [MenuItem("Factory/White Noise Texture")]
#endif
        public static void WhiteNoiseTexture()
        {
            var resolution = 512;
            var rng = new Random();

            var bytes = new byte[resolution * resolution];
            rng.NextBytes(bytes);
            
            var texture = new Texture2D(resolution, resolution, TextureFormat.Alpha8, false);
            texture.SetPixelData(bytes, 0);
            texture.Apply(false);

            string path = Path.Combine(Application.dataPath, "whitenoise.png");
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }

        private delegate T Write<T>(float x, float y, float z);
        
        private static void RunVolume<T>(string name, VolumeFormat volumeFormat, Vector3Int size,
            Write<T> calllback)
        {
            var sw = Stopwatch.StartNew();
            var rawVolume = RunVolumeMt(size, calllback);
            var clusters = rawVolume.Data.ToOctClusters(size);
            Clear(rawVolume.Data);
            ForceRamWipe();
            var packed = clusters.PackClusters(volumeFormat, true);
            foreach (var cluster in clusters)
            {
                Clear(cluster.Data);
            }
            ForceRamWipe();

            var volume =  new Volume(size.x, size.y, size.z, 0, 1, volumeFormat, packed);
            
            VolumeManager.SaveVolume(volume, name);
            foreach (var cluster in packed)
            {
                Clear(cluster.Data);
            }
            ForceRamWipe();
            Debug.Log($"Generating volume took {sw.ElapsedMilliseconds} ms");
        }

        // Large byte arrays have problems to be deallocated even when bigarray is set to null
        private static void Clear<T>(BigArray<T> rawVolumeData)
        {
            var dd = rawVolumeData.GetType().GetField("Data").GetValue(rawVolumeData) as T[][];
            for (var i = 0; i < dd.Length; i++)
            {
                dd[i] = null;
            }
        }
        
        private static void ForceRamWipe()
        {
            // Attempt to force the GC release LOH memory and return the memory to OS
            // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
            for (var i = 0; i < 16; i++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }
        }

        private static RawVolume<T> RunVolumeMt<T>(Vector3Int size, Write<T> calllback)
        {
            var voxels = (long) size.x * size.y * size.z;
            
            var chunkSize = Mathf.CeilToInt((float) voxels / Environment.ProcessorCount);

            var data = new BigArray<T>(voxels);

            long i = 0;
            var jobs = new List<(long start, long end)>();
            while (true)
            {
                var start = i;
                var end = Math.Min(i + chunkSize, voxels);
                jobs.Add((start, end));

                i += chunkSize;
                if (end >= voxels)
                {
                    break;
                }
            }
            
            
            Parallel.ForEach(jobs, (tuple, state) =>
            {
                var (startIndex, endIndex) = tuple;

                for (long i = startIndex; i < endIndex; i++)
                {
                    var rest = i;
                    var sizeXy = size.x * size.y;
                    var z = rest / sizeXy;
                    rest -= z * sizeXy;
                    var y = rest / size.x;
                    rest -= y * size.x;
                    var x = rest;

                    data[i] = calllback((float) ((double) x / size.x), (float) ((double) y / size.y), (float) ((double) z / size.z));
                }
            });
            
            return new RawVolume<T>(size.x, size.y, size.z, data);
        }
        
        
    }
}