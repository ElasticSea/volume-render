using System;
using System.IO;
using ElasticSea.Framework.Util;
using MathNet.Numerics.RootFinding;
using UnityEngine;

namespace Volumes.Imports.Ui
{
    public class ImportWizard : MonoBehaviour
    {
        [SerializeField] private string sourcePath;
        [SerializeField] private string volumeName;
        [SerializeField] private ChannelDepth channelDepth;
        [SerializeField] private bool multithreaded = true;
        [SerializeField] private int offset;

        public string SourcePath
        {
            get => sourcePath;
            set => sourcePath = value;
        }

        public string VolumeName
        {
            get => volumeName;
            set => volumeName = value;
        }

        public ChannelDepth Depth
        {
            get => channelDepth;
            set => channelDepth = value;
        }
    
        public bool Multithreaded
        {
            get => multithreaded;
            set => multithreaded = value;
        }
    
        public int Offset
        {
            get => offset;
            set => offset = value;
        }

        public VolumeBounds OriginalBounds
        {
            get
            {
                return new NiftiImport(sourcePath).ReadHeader().Bounds;
            }
        }
        
        public VolumeBounds Bounds
        {
            get
            {
                var targetBounds = OriginalBounds;
                targetBounds.Offset(offset);
                return targetBounds;
            }
        }
        //
        // public bool IsAutomaticCrop
        // {
        //     get
        //     {
        //         var original = OriginalBounds;
        //         var target = original;
        //         target.Offset(offset);
        //
        //         var result = GetValidBounds(target, channelDepth);
        //         
        //         
        //         var needToCrop = result.Width != target.Width || 
        //                          result.Height != target.Height || 
        //                          result.Depth != target.Depth;
        //
        //         return needToCrop;
        //     }
        // }
 
        public long VolumeSize(VolumeBounds bounds)
        {
            return (long)bounds.Width * bounds.Height * bounds.Depth * channelDepth.GetBitsSize() / 8;
        }

        public void Import()
        {
            var nftiImport = new NiftiImport(sourcePath, multithreaded);
            var volume = ImportManager.Import(nftiImport, channelDepth, Bounds, multithreaded);
            
            var dir = VolumeManager.VolumeDirectoryPath;
            Utils.EnsureDirectory(dir);
            var path = Path.Combine(dir, $"{volumeName}.vlm");
            VolumeManager.SaveVolume(volume, path);
            
            //TODO ? better api
            //volume.Save(volumeName);
        }

        // private static VolumeBounds GetValidBounds(VolumeBounds bounds, ChannelDepth depth)
        // {
        //     var bytesPerPixel = depth.GetBitsSize() / 8f;
        //
        //     bounds.Width = Mathf.Min(bounds.Width, 2048);
        //     bounds.Height = Mathf.Min(bounds.Height, 2048);
        //     bounds.Depth = Mathf.Min(bounds.Depth, 2048);
        //
        //     var totalBytes = (long) bounds.Width * bounds.Height * bounds.Depth * bytesPerPixel;
        //
        //     var textureSizeLimit = 2146435071;
        //     if (totalBytes > textureSizeLimit)
        //     {
        //         // 2146435071 is maximum index of non byte array
        //         var maxVolume = (long) ((double) textureSizeLimit / bytesPerPixel);
        //         var offset = CalculateOffset(bounds.Width, bounds.Height, bounds.Depth, maxVolume);
        //         bounds.Offset(offset);
        //     }
        //
        //     return bounds;
        // }

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
    }
}