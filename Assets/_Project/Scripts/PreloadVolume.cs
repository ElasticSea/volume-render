using System;
using System.IO;
using System.Linq;
using Render;
using UnityEngine;
using Volumes;

public class PreloadVolume : MonoBehaviour
{
    [SerializeField] private VolumeRenderManager manager;
    [SerializeField] private string volumeName;
    [SerializeField] private string presetName;

    private void Start()
    {
        var volumePath = Path.Combine(Application.persistentDataPath, "Volumes", $"{volumeName}.vlm");
        using (var stream = File.OpenRead(volumePath))
        {
            var volume = VolumeManager.LoadRuntimeVolume(stream);
            manager.LoadVolume(volume);
            manager.ApplyPreset(manager.RenderPresets.First(p => p.Name.Equals(presetName, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}