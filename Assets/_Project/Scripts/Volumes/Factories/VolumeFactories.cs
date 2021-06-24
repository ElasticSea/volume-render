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

namespace Volumes.Factories
{
    public class VolumeFactories
    {

#if UNITY_EDITOR
        [MenuItem("Factory/Perfect Sphere")]
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
        
        [MenuItem("Factory/RGBBox")]
        public static void RGBBox()
        {
            var size = 256;
            RunVolume("RGBBox", VolumeFormat.RGBA32, new Vector3Int(size, size, size), (x, y, z) =>
            {
                return new Color(x, y, z, 0.1f);
            });
        }
        
        [MenuItem("Factory/Noise")]
        public static void Noise()
        {
            var size = 1024;
            var scale = 5f;
            var colors = new[] {Color.clear, Color.clear,Color.clear, Color.red.SetAlpha(0.005f), Color.blue, Color.blue};
            // var colors = new[] {Color.blue.SetAlpha(0.01f), Color.blue.SetAlpha(0.01f), Color.cyan.SetAlpha(0.02f), Color.yellow, Color.yellow};
            var threashold = 0.05f;

            var noise = new OpenSimplexNoise();
            RunVolume("Noise", VolumeFormat.RGBA32, new Vector3Int(size, size, size), (x, y, z) =>
            {
                var noiseValue = (float) noise.Evaluate(x * scale, y * scale, z * scale) /2 + 0.5f;

                var rr = noiseValue * (colors.Length);
                var closestBoundary = (Mathf.RoundToInt(rr));
                var minT = closestBoundary - threashold / 2f;
                var maxT = closestBoundary + threashold / 2f;
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

        private delegate T Write<T>(float x, float y, float z);
        
        private static void RunVolume<T>(string name, VolumeFormat volumeFormat, Vector3Int size,
            Write<T> calllback)
        {
            var sw = Stopwatch.StartNew();
            var volumesDir = Path.Combine(Application.persistentDataPath, "Volumes");
            Utils.EnsureDirectory(volumesDir);

            var rawVolume = RunVolumeMt(size, calllback);
            
            var clusters = rawVolume.Data.ToOctClusters(size);
            var packed = clusters.PackClusters(volumeFormat, true);

            var volume =  new Volume(size.x, size.y, size.z, 0, 1, volumeFormat, packed);
            
            var volumePath = Path.Combine(volumesDir, $"{name}.vlm");
            VolumeManager.SaveVolume(volume, volumePath);
            Debug.Log($"{sw.ElapsedMilliseconds} ms");
        }

        private static RawVolume<T> RunVolumeMt<T>(Vector3Int size, Write<T> calllback)
        {
            var voxels = size.x * size.y * size.z;
            
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
        
        
#endif
    }
}