using System;
using UnityEngine;
using Volumes.Sources;

namespace Volumes.Imports.Ui
{
    public class ImportWizard : MonoBehaviour
    {
        [SerializeField] private string sourcePath;
        [SerializeField] private string volumeName;
        [SerializeField] private VolumeFormat channelDepth;
        [SerializeField] private bool multithreaded = true;
        [SerializeField] private int offset;

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

        public VolumeFormat Depth
        {
            get => channelDepth;
            set => channelDepth = value;
        }
    
        public bool Multithreaded
        {
            get => multithreaded;
            set => multithreaded = value;
        }
    
        public int Offset
        {
            get => offset;
            set => offset = value;
        }

        public VolumeBounds Bounds => GetSource(sourcePath).ReadHeader().Bounds;

        public long VolumeSize(VolumeBounds bounds)
        {
            return (long)bounds.Width * bounds.Height * bounds.Depth * channelDepth.GetBitsPerVoxel() / 8;
        }

        public void Import()
        {
            var volumeSource = GetSource(sourcePath);
            var volume = ImportManager.Import(volumeSource, channelDepth, multithreaded);
            VolumeManager.SaveVolume(volume, volumeName);
        }

        private IVolumeSource GetSource(string sourcePath)
        {
            if (sourcePath.EndsWith(".nii", StringComparison.InvariantCultureIgnoreCase))
            {
                return new NiftiSource(sourcePath, multithreaded);
            }
            
            throw new ArgumentException($"Volume \"{sourcePath}\" is not supported.");
        }
    }
}