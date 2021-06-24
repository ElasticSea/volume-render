namespace Volumes.Imports
{
    public interface IRawVolumeImport
    {
        RawVolumeMetadata ReadHeader();
        RawVolume<float> ReadData();
    }
}