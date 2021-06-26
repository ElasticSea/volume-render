using System;
using System.Diagnostics;
using UnityEngine;
using Volumes.Sources;
using Debug = UnityEngine.Debug;

namespace Volumes.Imports
{
    public class ImportManager
    {
        public static Volume Import(IVolumeSource source, VolumeFormat channelDepth, bool multithreaded = true)
        {
            var volumeData = ImportInternal(source, channelDepth, multithreaded);

            // Attempt to force the GC release LOH memory and return the memory to OS
            // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
            for (var i = 0; i < 16; i++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }
            
            return volumeData;
        }

        private static Volume ImportInternal(IVolumeSource source, VolumeFormat channelDepth, bool multithreaded = true)
        {
            Stopwatch sw = null;
            
            sw = Stopwatch.StartNew();
            var originalBounds = source.ReadHeader().Bounds;
            PrintSw("ReadHeader",sw);
            sw = Stopwatch.StartNew();
            var originalVolume = source.ReadData();
            PrintSw("ReadData",sw);

            // var needToCrop = targetBounds.Width != originalBounds.Width ||
            //                  targetBounds.Height != originalBounds.Height ||
            //                  targetBounds.Depth != originalBounds.Depth;
            //
            // if (needToCrop)
            // {
            //     sw = Stopwatch.StartNew();
            //     originalVolume = originalVolume.Crop(targetBounds, false);
            //     PrintSw("Crop",sw);
            // }

            sw = Stopwatch.StartNew();
            var (normalized, min, max) = originalVolume.Data.Normalize(multithreaded);
            PrintSw("Normalize",sw);
            
            var w = originalVolume.Width;
            var h = originalVolume.Height;
            var d = originalVolume.Depth;
            
            sw = Stopwatch.StartNew();
            var size = new Vector3Int(originalBounds.Width, originalBounds.Height, originalBounds.Depth);
            
            var clusters = normalized.ToOctClusters(size, multithreaded);

            PrintSw("ToClusters",sw);

            sw = Stopwatch.StartNew();
            var packedClusters = clusters.PackClusters(channelDepth, multithreaded);
            PrintSw("PackClusters",sw);

            return new Volume(w, h, d, min, max, channelDepth, packedClusters);
        }

        private static void PrintSw(string title, Stopwatch sw)
        {
            Debug.Log(title +" took "+sw.ElapsedMilliseconds+"ms");
        }
    }
}