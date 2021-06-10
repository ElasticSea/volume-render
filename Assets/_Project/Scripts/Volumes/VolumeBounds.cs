namespace Volumes
{
    public struct VolumeBounds
    {
        public int X;
        public int Y;
        public int Z;
        public int Width;
        public int Height;
        public int Depth;

        public VolumeBounds(int x, int y, int z, int width, int height, int depth)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            Depth = depth;
        }

        public VolumeBounds Offset(int offset)
        {
            X += offset;
            Y += offset;
            Z += offset;
            Width -= offset * 2;
            Height -= offset * 2;
            Depth -= offset * 2;
            return this;
        }
    }
}