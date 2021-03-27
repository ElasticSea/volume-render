namespace Volumes
{
    public class RawVolume
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public BigArray<float> Data { get; }
        
        public RawVolume(int width, int height, int depth, BigArray<float> data)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Data = data;
        }
    }
}