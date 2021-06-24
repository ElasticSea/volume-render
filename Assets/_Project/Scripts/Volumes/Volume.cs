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
        public VolumeFormat VolumeFormat { get; }
        public VolumeCluster[,,] Clusters { get; }

        public Volume(int width, int height, int depth, float min, float max, VolumeFormat volumeFormat, VolumeCluster[,,] clusters)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Min = min;
            Max = max;
            VolumeFormat = volumeFormat;
            Clusters = clusters;
        }
    }
}