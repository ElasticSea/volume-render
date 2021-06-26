using System;

namespace Volumes
{
    public static class VolumeFormatExtensions
    {
        public static int GetBitsPerVoxel(this VolumeFormat depth)
        {
            switch (depth)
            {
                case VolumeFormat.Gray32:
                    return 32;
                case VolumeFormat.Gray16:
                    return 16;
                case VolumeFormat.Gray8:
                    return 8;
                case VolumeFormat.RGBA64:
                    return 64;
                case VolumeFormat.RGBA32:
                    return 32;
                case VolumeFormat.RGBA16:
                    return 16;
                default:
                    throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
            }
        }
    }
}