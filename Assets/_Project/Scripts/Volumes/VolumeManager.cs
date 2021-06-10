using System;
using System.Collections.Generic;
using System.IO;
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
                    var volume = LoadVolumeMetadata(vlmFile);
                    var voluemSource = new VolumeSource();
                    voluemSource.FilePath = vlmFile;
                    voluemSource.Volume = volume;
                    yield return voluemSource;
                }
            }
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
            var bits = BitConverter.ToInt32(dimensionsBuffer, 20);

            return new Volume(width, height, depth, min, max, bits, null);
        }
        
        public static RuntimeVolume LoadRuntimeVolume(Stream stream)
        {
            var dimensionsBuffer = new byte[24];
            stream.Read(dimensionsBuffer, 0, dimensionsBuffer.Length);
            var width = BitConverter.ToInt32(dimensionsBuffer, 0);
            var height = BitConverter.ToInt32(dimensionsBuffer, 4);
            var depth = BitConverter.ToInt32(dimensionsBuffer, 8);
            var min = BitConverter.ToSingle(dimensionsBuffer, 12);
            var max = BitConverter.ToSingle(dimensionsBuffer, 16);
            var bits = BitConverter.ToInt32(dimensionsBuffer, 20);

            TextureFormat format;
            switch (bits)
            {
                case 8: 
                    format = TextureFormat.R8;
                    break;
                case 16:
                    format = TextureFormat.R16;
                    break;
                case 32:
                    format = TextureFormat.RFloat;
                    break;
                default:
                    throw new ArgumentException("Does not support partial bytes.");
            }

            var texture = new Texture3D(width, height, depth, format, false);
            texture.filterMode = FilterMode.Bilinear;
            SetPixelData(texture, stream);
            Debug.Log(GC.GetTotalMemory(true));
            
            // Attempt to force the GC release LOH memory and return the memory to OS
            // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
            for (var i = 0; i < 32; i++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }

            texture.Apply(false, true);
                
            Debug.Log(GC.GetTotalMemory(true));
            return new RuntimeVolume(width, height, depth, min, max, bits, texture);
        }

        private static void SetPixelData(Texture3D texture3D, Stream stream)
        {
            var bytelen = stream.Length - stream.Position;
            var bytes = new byte[bytelen];
            
            int read;
            int index = 0;
            var buffer = new byte[1024 * 1024];
            while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                Array.Copy(buffer, 0, bytes, index, read);
                index += read;
            }
            
            // There is not destination index on the texture so I have allocate byte[]
            texture3D.SetPixelData(bytes, 0);
        }
        
        public static void SaveVolume(Volume volume, string path)
        {
            using var stream = new FileStream(path, FileMode.Create);
            Write(stream, BitConverter.GetBytes(volume.Width));
            Write(stream, BitConverter.GetBytes(volume.Height));
            Write(stream, BitConverter.GetBytes(volume.Depth));
            Write(stream, BitConverter.GetBytes(volume.Min));
            Write(stream, BitConverter.GetBytes(volume.Max));
            Write(stream, BitConverter.GetBytes(volume.ChannelDepthBits));
    
            var volumeData = volume.Data;
            var chunks = volumeData.Data;
            var bytesToWrite = volumeData.Length;
            for (var i = 0; i < chunks.Length; i++)
            {
                var writeLength = (int) Math.Min(bytesToWrite, chunks[i].Length);
                Write(stream, chunks[i], writeLength);
                bytesToWrite -= writeLength;
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
    }
}