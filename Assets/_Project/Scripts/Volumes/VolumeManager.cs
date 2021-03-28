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
        
        public static Volume LoadVolume(string path)
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

            var dataArray = new BigArray<byte>((long) width * height * depth * bits/8);

            if (bits % 8 != 0)
            {
                throw new InvalidOperationException("Does not support partial bytes.");
            }

            for (var i = 0; i < dataArray.Data.Length; i++)
            {
                var chunk = dataArray.Data[i];
                var read = reader.Read(chunk, 0, chunk.Length);
                if (read == 0)
                {
                    break;
                }
            }
                
            return new Volume(width, height, depth, min, max, bits, dataArray);
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