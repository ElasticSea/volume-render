using System;
using System.IO;
using ElasticSea.Framework.Util;
using UnityEngine;

namespace Volumes
{
    public class Volume
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public float Min { get; }
        public float Max { get; }
        public int ChannelDepthBits { get; }
        public VolumeCluster[,,] Clusters { get; }

        public Volume(int width, int height, int depth, float min, float max, int channelDepthBits, VolumeCluster[,,] clusters)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Min = min;
            Max = max;
            ChannelDepthBits = channelDepthBits;
            Clusters = clusters;
        }
    }
}