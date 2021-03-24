using System;

namespace Pipelines
{
    public static class ChannelDepthExtensions
    {
        public static int GetByteSize(this ChannelDepth depth)
        {
            switch (depth)
            {
                case ChannelDepth.Single32:
                    return 4;
                case ChannelDepth.Half16:
                    return 2;
                case ChannelDepth.Quarter8:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
            }
        }
    }
}