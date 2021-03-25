using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MathNet.Numerics.RootFinding;
using Nifti.NET;
using Pipelines;
using Pipelines.Imports;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class VolumeImport : MonoBehaviour
{
    [SerializeField] private string sourcePath;
    [SerializeField] private string volumeName;
    [SerializeField] private ChannelDepth channelDepth;
    
    
    public static void WriteVolume(string path, Volume volume)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
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
                var writeLength = Math.Min((int) bytesToWrite, chunks[i].Length);
                Write(stream, chunks[i], writeLength);
                bytesToWrite -= writeLength;
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
    
    public static Volume Load(string path)
    {
        using (var reader = File.OpenRead(path))
        {
            var dimensionsBuffer = new byte[32];
            reader.Read(dimensionsBuffer, 0, dimensionsBuffer.Length);
            var width = BitConverter.ToInt32(dimensionsBuffer, 0);
            var height = BitConverter.ToInt32(dimensionsBuffer, 4);
            var depth = BitConverter.ToInt32(dimensionsBuffer, 8);
            var min = BitConverter.ToDouble(dimensionsBuffer, 12);
            var max = BitConverter.ToDouble(dimensionsBuffer, 20);
            var bits = BitConverter.ToInt32(dimensionsBuffer, 28);

            var data = new BigArray<byte>((long) width * height * depth * bits/8);

            if (bits % 8 != 0)
            {
                throw new InvalidOperationException("Does not support partial bytes.");
            }

            for (var i = 0; i < data.Data.Length; i++)
            {
                var chunk = data.Data[i];
                var read = reader.Read(chunk, 0, chunk.Length);
                if (read == 0)
                {
                    break;
                }
            }
            
            return new Volume(width, height, depth, min, max, bits, data);
        }
    }
}