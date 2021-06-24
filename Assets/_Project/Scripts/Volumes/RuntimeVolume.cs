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
        public VolumeFormat Format { get; }
        public RuntimeVolumeCluster[,,] Clusters { get; }

        public RuntimeVolume(int width, int height, int depth, float min, float max, VolumeFormat format, RuntimeVolumeCluster[,,] clusters)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Min = min;
            Max = max;
            Format = format;
            Clusters = clusters;
        }
    }
}