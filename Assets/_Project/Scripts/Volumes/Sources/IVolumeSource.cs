namespace Volumes.Sources
{
    public interface IVolumeSource
    {
        RawVolumeMetadata ReadHeader();
        RawVolume<float> ReadData();
    }
}