namespace Pipelines
{
    public class Volume
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public double Min { get; }
        public double Max { get; }
        public int ChannelDepthBits { get; }
        public BigArray<byte> Data { get; }

        public Volume(int width, int height, int depth, double min, double max, int channelDepthBits, BigArray<byte> data)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Min = min;
            Max = max;
            ChannelDepthBits = channelDepthBits;
            Data = data;
        }
    }
}