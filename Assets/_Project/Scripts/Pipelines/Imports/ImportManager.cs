using System;
using MathNet.Numerics.RootFinding;
using UnityEngine;

namespace Pipelines.Imports
{
    public class ImportManager
    {
        public static Volume Import(IRawVolumeImport source, ChannelDepth channelDepth)
        {
            var header = source.ReadHeader();
            var bounds = header.Bounds;

            var fit = GetValidBounds(bounds, channelDepth);

            var needToCrop = fit.Width != bounds.Width || fit.Height != bounds.Height || fit.Depth != bounds.Depth;
            if (needToCrop)
            {
                //
            }

            var rawVolume = source.ReadData();

            if (needToCrop)
            {
                rawVolume = rawVolume.Subvolume(bounds);
            }

            var (normalized, min, max) = rawVolume.Data.Normalize();
            var packed = normalized.Pack(channelDepth);

            return new Volume(new VolumeData(rawVolume.Width, rawVolume.Height, rawVolume.Depth, min, max, channelDepth.GetBitsSize(), packed));
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