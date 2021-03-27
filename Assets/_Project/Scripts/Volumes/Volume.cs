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
        public BigArray<byte> Data { get; }

        public Volume(int width, int height, int depth, float min, float max, int channelDepthBits, BigArray<byte> data)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Min = min;
            Max = max;
            ChannelDepthBits = channelDepthBits;
            Data = data;
        }

        public void Save(string path)
        {
            VolumeManager.Save(this, path);
        }

        public static Volume Load(string path)
        {
            return VolumeManager.Load(path);
        }
    }
}