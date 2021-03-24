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

public class Import : MonoBehaviour
{
    public void Awake()
    {
        var sw = Stopwatch.StartNew();
        
        var from = @"E:\Brain\Acquired_FA25_downsampled_500um.nii";
        var volume = NiftiImport.ReadVolume(from);
        ImportIt(volume, ChannelDepth.Single_32);
        
        
        Debug.Log("elapsed " + sw.ElapsedMilliseconds);
        //
        // string[] arr = new string[0];
        //
        // arr.
        //
        //
        // // THese will be optionable
        //
        //
        // var sw = Stopwatch.StartNew();
        // var dd = NiftiFile.Read(from);
        // Debug.Log("elapsed " + sw.ElapsedMilliseconds);
        //
        // var payload = dd.Data as IEnumerable<float>;
    }

    public enum ChannelDepth
    {
        Single_32, Half_16, Quarter_8
    }

    public void ImportIt(RawVolume volume, ChannelDepth channelDepth)
    {
        var bounds = GetValidBounds(volume, channelDepth);
        var isChanged = volume.Width != bounds.Width ||
                        volume.Height != bounds.Height ||
                        volume.Depth != bounds.Depth;
        
        if (isChanged)
        {
            volume = volume.Subvolume(bounds);
        }
        
        
       
        
        
    }

    private VolumeBounds GetValidBounds(RawVolume volume, ChannelDepth depth)
    {
        var bytesPerPixel = GetPixelSizeInBytes(depth);

        var bounds = new VolumeBounds(0, 0, 0, volume.Width, volume.Height, volume.Depth);

        bounds.Width = Mathf.Min(bounds.Width, 2048);
        bounds.Height = Mathf.Min(bounds.Height, 2048);
        bounds.Depth = Mathf.Min(bounds.Depth, 2048);
        
        var totalBytes = (long) (bounds.Width * bounds.Height * bounds.Depth) * bytesPerPixel;

        var textureSizeLimit = 2146435071;
        if (totalBytes > textureSizeLimit )
        {
            // 2146435071 is maximum index of non byte array
            var maxVolume = (long)((double)textureSizeLimit / bytesPerPixel);
            var offset = CalculateOffset(bounds.Width, bounds.Height, bounds.Depth, maxVolume);
            bounds.X += offset;
            bounds.Y += offset;
            bounds.Z += offset;
            bounds.Width -= offset*2;
            bounds.Height -= offset*2;
            bounds.Depth -= offset*2;
        }

        return bounds;
    }
    
    private static int CalculateOffset(int width, int height, int depth, long volume)
    {
        // Solve volume = (width - 2*offset) * (height - 2*offset) * (depth - 2*offset)

        var w = (double) width;
        var h = (double) height;
        var d = (double) depth;
        var p3 = -1; // -x^3
        var p2 = d + w + h; // x^2d + wx^2 + hx^2
        var p1 = -w * d - h * d - w * h; // whd -wdx - hdx - whx
        var p0 = w * h * d - volume; // whd - volume
        var (r0, r1, r2) = Cubic.RealRoots(p0 / p3, p1 / p3, p2 / p3); // normalize p3
        return (int) Math.Ceiling(Math.Max((float) r0 / 2, 0)); // offset is half
    }

    private int GetPixelSizeInBytes(ChannelDepth depth)
    {
        switch (depth)
        {
            case ChannelDepth.Single_32:
                return 4;
            case ChannelDepth.Half_16:
                return 2;
            case ChannelDepth.Quarter_8:
                return 1;
            default:
                throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
        }
    }
}