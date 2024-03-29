using System;
using System.Collections.Generic;
using System.IO;
using ElasticSea.Framework.Util;
using UnityEngine;

namespace Volumes
{
    public class VolumeManager
    {
        public static IEnumerable<VolumeSource> ListVolumes(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var vlmFile in Directory.EnumerateFiles(path, "*.vlm", SearchOption.AllDirectories))
                {
                    yield return GetVolume(vlmFile);
                }
            }
        }

        public static VolumeSource GetVolume(string path)
        {
            var volume = LoadVolumeMetadata(path);
            var volumeSource = new VolumeSource();
            volumeSource.FilePath = path;
            volumeSource.Volume = volume;
            return volumeSource;
        }

        public static Volume LoadVolumeMetadata(string path)
        {
            using var reader = File.OpenRead(path);
            var dimensionsBuffer = new byte[24];
            reader.Read(dimensionsBuffer, 0, dimensionsBuffer.Length);
            var width = BitConverter.ToInt32(dimensionsBuffer, 0);
            var height = BitConverter.ToInt32(dimensionsBuffer, 4);
            var depth = BitConverter.ToInt32(dimensionsBuffer, 8);
            var min = BitConverter.ToSingle(dimensionsBuffer, 12);
            var max = BitConverter.ToSingle(dimensionsBuffer, 16);
            var bits = (VolumeFormat) BitConverter.ToInt32(dimensionsBuffer, 20);

            return new Volume(width, height, depth, min, max, bits, null);
        }

        public static RuntimeVolume LoadRuntimeVolume(Stream stream)
        {
            var dimensionsBuffer = new byte[36];
            stream.Read(dimensionsBuffer, 0, dimensionsBuffer.Length);
            var width = BitConverter.ToInt32(dimensionsBuffer, 0);
            var height = BitConverter.ToInt32(dimensionsBuffer, 4);
            var depth = BitConverter.ToInt32(dimensionsBuffer, 8);
            var min = BitConverter.ToSingle(dimensionsBuffer, 12);
            var max = BitConverter.ToSingle(dimensionsBuffer, 16);
            var format = (VolumeFormat) BitConverter.ToInt32(dimensionsBuffer, 20);
            var clustersWidth = BitConverter.ToInt32(dimensionsBuffer, 24);
            var clustersHeight = BitConverter.ToInt32(dimensionsBuffer, 28);
            var clustersDepth = BitConverter.ToInt32(dimensionsBuffer, 32);

            TextureFormat textureFormat = TextureFormat.Alpha8;
            int bytesPerVoxel = -1;
            switch (format)
            {
                case VolumeFormat.Gray32:
                    textureFormat = TextureFormat.RFloat;
                    bytesPerVoxel = 4;
                    break;
                case VolumeFormat.Gray16:
                    textureFormat = TextureFormat.R16;
                    bytesPerVoxel = 2;
                    break;
                case VolumeFormat.Gray8:
                    textureFormat = TextureFormat.R8;
                    bytesPerVoxel = 1;
                    break;
                case VolumeFormat.RGBA64:
                    break;
                case VolumeFormat.RGBA32:
                    textureFormat = TextureFormat.RGBA32;
                    bytesPerVoxel = 4;
                    break;
                case VolumeFormat.RGBA16:
                    break;
                default:
                    throw new ArgumentException("Does not support partial bytes.");
            }

            var clusters = new RuntimeVolumeCluster[clustersWidth, clustersHeight, clustersDepth];
            var clusterDimensionsBuffer = new byte[24];
            for (var cx = 0; cx < clusters.GetLength(0); cx++)
            {
                for (var cy = 0; cy < clusters.GetLength(1); cy++)
                {
                    for (var cz = 0; cz < clusters.GetLength(2); cz++)
                    {
                        stream.Read(clusterDimensionsBuffer, 0, clusterDimensionsBuffer.Length);
                        
                        var cluster  = new RuntimeVolumeCluster
                        {
                            X = BitConverter.ToInt32(clusterDimensionsBuffer, 0),
                            Y = BitConverter.ToInt32(clusterDimensionsBuffer, 4),
                            Z = BitConverter.ToInt32(clusterDimensionsBuffer, 8),
                            Width = BitConverter.ToInt32(clusterDimensionsBuffer, 12),
                            Height = BitConverter.ToInt32(clusterDimensionsBuffer, 16),
                            Depth = BitConverter.ToInt32(clusterDimensionsBuffer, 20)
                        };
                        clusters[cx, cy, cz] = cluster;

                        var texture = new Texture3D(cluster.Width, cluster.Height, cluster.Depth, textureFormat, false);
                        texture.filterMode = FilterMode.Bilinear;
                        texture.wrapMode = TextureWrapMode.Clamp;
                        var len = cluster.Width * cluster.Height * cluster.Depth * bytesPerVoxel;
                        SetPixelData(texture, stream, len);
                        texture.Apply(false, true);
                        cluster.Texture = texture;
                    }
                }
            }

            Debug.Log(GC.GetTotalMemory(true));

            // Attempt to force the GC release LOH memory and return the memory to OS
            // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
            for (var i = 0; i < 32; i++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }

            Debug.Log(GC.GetTotalMemory(true));
            return new RuntimeVolume(width, height, depth, min, max, format, clusters);
        }

        private static void SetPixelData(Texture3D texture3D, Stream stream, long length)
        {
            var bytes = new byte[length];

            int read;
            int index = 0;
            var bufferSize = (int) Mathf.Min(1024 * 1024, length);
            var buffer = new byte[bufferSize];
            while ((read = stream.Read(buffer, 0, (int) Mathf.Min(buffer.Length, length - index))) != 0)
            {
                Array.Copy(buffer, 0, bytes, index, read);
                index += read;
                if (index == length)
                {
                    break;
                }
            }

            // There is not destination index on the texture so I have allocate byte[]
            texture3D.SetPixelData(bytes, 0);
        }

        private static void SaveVolumeAtPath(Volume volume, string path)
        {
            using var stream = new FileStream(path, FileMode.Create);
            Write(stream, BitConverter.GetBytes(volume.Width));
            Write(stream, BitConverter.GetBytes(volume.Height));
            Write(stream, BitConverter.GetBytes(volume.Depth));
            Write(stream, BitConverter.GetBytes(volume.Min));
            Write(stream, BitConverter.GetBytes(volume.Max));
            Write(stream, BitConverter.GetBytes((int)volume.VolumeFormat));

            Write(stream, BitConverter.GetBytes(volume.Clusters.GetLength(0)));
            Write(stream, BitConverter.GetBytes(volume.Clusters.GetLength(1)));
            Write(stream, BitConverter.GetBytes(volume.Clusters.GetLength(2)));
            for (var cx = 0; cx < volume.Clusters.GetLength(0); cx++)
            {
                for (var cy = 0; cy < volume.Clusters.GetLength(1); cy++)
                {
                    for (var cz = 0; cz < volume.Clusters.GetLength(2); cz++)
                    {
                        var cluster = volume.Clusters[cx, cy, cz];
                        Write(stream, BitConverter.GetBytes(cluster.X));
                        Write(stream, BitConverter.GetBytes(cluster.Y));
                        Write(stream, BitConverter.GetBytes(cluster.Z));
                        Write(stream, BitConverter.GetBytes(cluster.Width));
                        Write(stream, BitConverter.GetBytes(cluster.Height));
                        Write(stream, BitConverter.GetBytes(cluster.Depth));
                        var volumeData = cluster.Data;
                        var chunks = volumeData.Data;
                        var bytesToWrite = volumeData.Length;
                        for (var i = 0; i < chunks.Length; i++)
                        {
                            var writeLength = (int) Math.Min(bytesToWrite, chunks[i].Length);
                            Write(stream, chunks[i], writeLength);
                            bytesToWrite -= writeLength;
                        }
                    }
                }
            }
        }

        private static void Write(Stream stream, byte[] bytes)
        {
            Write(stream, bytes, bytes.Length);
        }

        private static void Write(Stream stream, byte[] bytes, int length)
        {
            stream.Write(bytes, 0, length);
        }

        public static string VolumeDirectoryPath => Path.Combine(Application.persistentDataPath, "Volumes");

        public static void SaveVolume(Volume volume, string name)
        {
            var dir = VolumeDirectoryPath;
            Utils.EnsureDirectory(dir);
            var path = Path.Combine(dir, $"{name}.vlm");
            SaveVolumeAtPath(volume, path);
        }
    }
}