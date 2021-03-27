using System;
using MathNet.Numerics.RootFinding;
using UnityEngine;

namespace Volumes.Imports
{
    public class ImportManager
    {
        public static Volume Import(IRawVolumeImport source, ChannelDepth channelDepth, bool multithreaded = true)
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

        private static Volume ImportInternal(IRawVolumeImport source, ChannelDepth channelDepth, bool multithreaded = true)
        {
            var originalBounds = source.ReadHeader().Bounds;

            var targetBounds = GetValidBounds(originalBounds, channelDepth);

            var originalVolume = source.ReadData();

            var needToCrop = targetBounds.Width != originalBounds.Width ||
                             targetBounds.Height != originalBounds.Height ||
                             targetBounds.Depth != originalBounds.Depth;
            
            // Volume is too large to fit into Texture3D
            if (needToCrop)
            {
                originalVolume = originalVolume.Crop(targetBounds, multithreaded);
            }

            var (normalized, min, max) = originalVolume.Data.Normalize(multithreaded);

            var packed = normalized.Pack(channelDepth, multithreaded);

            var w = targetBounds.Width;
            var h = targetBounds.Height;
            var d = targetBounds.Depth;
            var bits = channelDepth.GetBitsSize();
            
            return new Volume(w, h, d, min, max, bits, packed);
        }

        public static VolumeBounds GetValidBounds(VolumeBounds bounds, ChannelDepth depth)
        {
            var bytesPerPixel = depth.GetBitsSize() / 8f;

            bounds.Width = Mathf.Min(bounds.Width, 2048);
            bounds.Height = Mathf.Min(bounds.Height, 2048);
            bounds.Depth = Mathf.Min(bounds.Depth, 2048);

            var totalBytes = (long) bounds.Width * bounds.Height * bounds.Depth * bytesPerPixel;

            var textureSizeLimit = 2146435071;
            if (totalBytes > textureSizeLimit)
            {
                // 2146435071 is maximum index of non byte array
                var maxVolume = (long) ((double) textureSizeLimit / bytesPerPixel);
                var offset = CalculateOffset(bounds.Width, bounds.Height, bounds.Depth, maxVolume);
                bounds.X += offset;
                bounds.Y += offset;
                bounds.Z += offset;
                bounds.Width -= offset * 2;
                bounds.Height -= offset * 2;
                bounds.Depth -= offset * 2;
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
    }
}