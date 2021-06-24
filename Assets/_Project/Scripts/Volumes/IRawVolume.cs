namespace Volumes
{
    public class RawVolume<T>
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public BigArray<T> Data { get; }

        public RawVolume(int width, int height, int depth, BigArray<T> data)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Data = data;
        }
    }
}