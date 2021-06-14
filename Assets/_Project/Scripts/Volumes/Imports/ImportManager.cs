using System;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.RootFinding;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Volumes.Imports
{
    public class ImportManager
    {
        public static Volume Import(IRawVolumeImport source, ChannelDepth channelDepth, VolumeBounds targetBounds, bool multithreaded = true)
        {
            var volumeData = ImportInternal(source, channelDepth, targetBounds, multithreaded);

            // Attempt to force the GC release LOH memory and return the memory to OS
            // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
            for (var i = 0; i < 16; i++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }
            
            return volumeData;
        }

        private static Volume ImportInternal(IRawVolumeImport source, ChannelDepth channelDepth, VolumeBounds targetBounds, bool multithreaded = true)
        {
            Stopwatch sw = null;
            
            sw = Stopwatch.StartNew();
            var originalBounds = source.ReadHeader().Bounds;
            PrintSw("ReadHeader",sw);
            sw = Stopwatch.StartNew();
            var originalVolume = source.ReadData();
            PrintSw("ReadData",sw);

            var needToCrop = targetBounds.Width != originalBounds.Width ||
                             targetBounds.Height != originalBounds.Height ||
                             targetBounds.Depth != originalBounds.Depth;
            
            if (needToCrop)
            {
                sw = Stopwatch.StartNew();
                originalVolume = originalVolume.Crop(targetBounds, false);
                PrintSw("Crop",sw);
            }

            sw = Stopwatch.StartNew();
            var (normalized, min, max) = originalVolume.Data.Normalize(multithreaded);
            PrintSw("Normalize",sw);
            
            var w = originalVolume.Width;
            var h = originalVolume.Height;
            var d = originalVolume.Depth;
            var isPadded = true;
            
            // Cluster size?
            sw = Stopwatch.StartNew();
            var maxClusterSize = 256;
            var clusters = normalized.ToClusters(originalBounds.Width, originalBounds.Height, originalBounds.Depth, maxClusterSize, isPadded, multithreaded);
            PrintSw("ToClusters",sw);

            if (isPadded)
            {
                w = clusters.GetLength(0) * maxClusterSize;
                h = clusters.GetLength(1) * maxClusterSize;
                d = clusters.GetLength(2) * maxClusterSize;
            }

            sw = Stopwatch.StartNew();
            var packedClusters = clusters.PackClusters(channelDepth, multithreaded);
            PrintSw("PackClusters",sw);

            var bits = channelDepth.GetBitsSize();
            
            return new Volume(w, h, d, min, max, bits, packedClusters);
        }

        private static void PrintSw(string title, Stopwatch sw)
        {
            Debug.Log(title +" took "+sw.ElapsedMilliseconds+"ms");
        }
    }
}