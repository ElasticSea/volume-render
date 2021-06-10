using UnityEngine;

namespace Volumes
{
    public class RuntimeVolume
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public float Min { get; }
        public float Max { get; }
        public int ChannelDepthBits { get; }
        public Texture3D Texture { get; }

        public RuntimeVolume(int width, int height, int depth, float min, float max, int channelDepthBits, Texture3D texture)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Min = min;
            Max = max;
            ChannelDepthBits = channelDepthBits;
            Texture = texture;
        }
    }
}