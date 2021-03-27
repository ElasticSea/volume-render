namespace Volumes
{
    public class RawVolumeMetadata
    {
        public int Width;
        public int Height;
        public int Depth;

        public VolumeBounds Bounds => new VolumeBounds(0, 0, 0, Width, Height, Depth);
    }
}