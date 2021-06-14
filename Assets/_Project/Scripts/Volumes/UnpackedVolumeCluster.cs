namespace Volumes
{
    public class UnpackedVolumeCluster<T>
    {
        public int X;
        public int Y;
        public int Z;
        public int Width;
        public int Height;
        public int Depth;
        public BigArray<T> Data;
    }
}