namespace Pipelines.Imports
{
    public interface IRawVolumeImport
    {
        RawVolumeHeader ReadHeader();
        RawVolume ReadData();
    }
}