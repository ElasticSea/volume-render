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
            volumeRenderManager.LoadVolume(volumeSource);
        }
    }
}