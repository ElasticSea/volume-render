using System;
using MathNet.Numerics.RootFinding;
using UnityEngine;

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
            var originalBounds = source.ReadHeader().Bounds;
            var originalVolume = source.ReadData();

            var needToCrop = targetBounds.Width != originalBounds.Width ||
                             targetBounds.Height != originalBounds.Height ||
                             targetBounds.Depth != originalBounds.Depth;
            
            if (needToCrop)
            {
                originalVolume = originalVolume.Crop(targetBounds, false);
            }

            var (normalized, min, max) = originalVolume.Data.Normalize(multithreaded);

            var packed = normalized.Pack(channelDepth, multithreaded);

            var w = targetBounds.Width;
            var h = targetBounds.Height;
            var d = targetBounds.Depth;
            var bits = channelDepth.GetBitsSize();
            
            return new Volume(w, h, d, min, max, bits, packed);
        }
    }
}