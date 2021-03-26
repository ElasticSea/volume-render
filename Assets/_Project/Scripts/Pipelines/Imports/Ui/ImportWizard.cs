using System.IO;
using Pipelines;
using Pipelines.Imports;
using UnityEngine;

public class ImportWizard : MonoBehaviour
{
    [SerializeField] private string sourcePath;
    [SerializeField] private string volumeName;
    [SerializeField] private ChannelDepth channelDepth;

    public string SourcePath
    {
        get => sourcePath;
        set => sourcePath = value;
    }

    public string VolumeName
    {
        get => volumeName;
        set => volumeName = value;
    }

    public ChannelDepth Depth
    {
        get => channelDepth;
        set => channelDepth = value;
    }

    public string VolumePath => Path.Combine(new DirectoryInfo(sourcePath).Parent.FullName, $"{volumeName}.vlm");

    public void Import()
    {
        var nftiImport = new NiftiImport(sourcePath);
        var volume = ImportManager.Import(nftiImport, channelDepth);
        volume.Save(VolumePath);
    }

    public RawVolumeHeader GetHeader()
    {
        return new NiftiImport(sourcePath).ReadHeader();
    }

    public VolumeBounds GetValidBounds()
    {
        var nftiImport = new NiftiImport(sourcePath);
        return ImportManager.GetValidBounds(nftiImport.ReadHeader().Bounds, channelDepth);
    }
    
    public long OriginalVolumeSize()
    {
        var nftiImport = new NiftiImport(sourcePath);
        var bounds = nftiImport.ReadHeader().Bounds;
        return (long)bounds.Width * bounds.Height * bounds.Depth * channelDepth.GetBitsSize() / 8;
    }
    
    public long ResultVolumeSize()
    {
        var nftiImport = new NiftiImport(sourcePath);
        var bounds = nftiImport.ReadHeader().Bounds;
        bounds = ImportManager.GetValidBounds(nftiImport.ReadHeader().Bounds, channelDepth);
        return (long)bounds.Width * bounds.Height * bounds.Depth * channelDepth.GetBitsSize() / 8;
    }
}