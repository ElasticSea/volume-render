using System;
using System.Collections.Generic;
using System.IO;
using Render;
using UnityEngine;

namespace Volumes.Ui
{
    public class VolumesViewModel : MonoBehaviour
    {
        [SerializeField] private VolumeRenderManager volumeRenderManager;
        
        public IEnumerable<VolumeSource> Volumes => VolumeManager.ListVolumes(Path.Combine(Application.persistentDataPath, "Volumes"));

        public void LoadVolume(VolumeSource volumeSource)
        {
            using (var stream = File.OpenRead(volumeSource.FilePath))
            {
                var volume = VolumeManager.LoadRuntimeVolume(stream);
                volumeRenderManager.LoadVolume(volume);
                // Attempt to force the GC release LOH memory and return the memory to OS
                // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
                for (var i = 0; i < 16; i++)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                }
                // Attempt to force the GC release LOH memory and return the memory to OS
                // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
                for (var i = 0; i < 16; i++)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                }
            }
        }
    }
}