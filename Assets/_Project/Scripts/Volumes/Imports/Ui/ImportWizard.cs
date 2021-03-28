using System.IO;
using ElasticSea.Framework.Util;
using UnityEngine;

namespace Volumes.Imports.Ui
{
    public class ImportWizard : MonoBehaviour
    {
        [SerializeField] private string sourcePath;
        [SerializeField] private string volumeName;
        [SerializeField] private ChannelDepth channelDepth;
        [SerializeField] private bool multithreaded = true;

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
    
        public bool Multithreaded
        {
            get => multithreaded;
            set => multithreaded = value;
        }

        public void Import()
        {
            var nftiImport = new NiftiImport(sourcePath, multithreaded);
            var volume = ImportManager.Import(nftiImport, channelDepth, multithreaded);
            
            var dir = VolumeManager.VolumeDirectoryPath;
            Utils.EnsureDirectory(dir);
            var path = Path.Combine(dir, $"{volumeName}.vlm");
            VolumeManager.SaveVolume(volume, path);
            
            //TODO ? better api
            //volume.Save(volumeName);
        }

        public RawVolumeMetadata GetHeader()
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
}