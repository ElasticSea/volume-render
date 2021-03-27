using System;

namespace Volumes
{
    public static class ChannelDepthExtensions
    {
        public static int GetBitsSize(this ChannelDepth depth)
        {
            switch (depth)
            {
                case ChannelDepth.Single32:
                    return 4 * 8;
                case ChannelDepth.Half16:
                    return 2 * 8;
                case ChannelDepth.Quarter8:
                    return 1 * 8;
                default:
                    throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
            }
        }
    }
}