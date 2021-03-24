using Nifti.NET;

namespace Pipelines
{
    public class Volume
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public double Min { get; }
        public double Max { get; }
        public int Bits { get; }
        public BigArray<byte> Data { get; }

        public Volume(int width, int height, int depth, double min, double max, int bits, BigArray<byte> data)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Min = min;
            Max = max;
            Bits = bits;
            Data = data;
        }
    }
}