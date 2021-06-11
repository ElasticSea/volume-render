using System;
using System.Collections.Generic;
using UnityEngine;
using Volumes;

namespace Render
{
    public class VolumeRenderManager : MonoBehaviour
    {
        [SerializeField] private VolumeRender volumePrefab;
        
        private VolumeRender volumeRender;

        public VolumeRender VolumeRender => volumeRender;

        public event Action<VolumeRender> OnVolumeLoaded = render => { }; 

        public void LoadVolume(RuntimeVolume volume)
        {
            if (volumeRender)
            {
                Destroy(volumeRender.Material.GetTexture("_Volume"));
                Destroy(volumeRender.gameObject);
            }
        
            volumeRender = Instantiate(volumePrefab);
            volumeRender.Material.SetTexture("_Volume", volume.Texture);
            volumeRender.Material.SetFloat("_Alpha", 0.1f);
            volumeRender.Material.SetFloat("_AlphaThreshold", 0.999f);
            volumeRender.Material.SetFloat("_StepDistance", 0.0002f);
            volumeRender.Material.SetInt("_MaxStepThreshold", 2048);
            OnVolumeLoaded(volumeRender);
        }

        public void Cut(Vector3 position, Vector3 normal)
        {
            if (volumeRender)
            {
                var localPos = volumeRender.transform.InverseTransformPoint(position);
                var localPosition = volumeRender.transform.InverseTransformVector(normal);

                volumeRender.Material.SetVector("_CutOrigin", localPos);
                volumeRender.Material.SetVector("_CutNormal", localPosition);
            }
        }
        
        private IEnumerable<RenderPreset> renderPresets = new[]
        {
            new RenderPreset
            {
                Name = "Low",
                Settings = new RenderSettings
                {
                    Alpha = 5f,
                    AlphaThreshold = 0.95f,
                    StepDistance = 0.0128f,
                    MaxStepThreshold = 32
                }
            },
            new RenderPreset
            {
                Name = "Medium",
                Settings = new RenderSettings
                {
                    Alpha = 0.77f,
                    AlphaThreshold = 0.99f,
                    StepDistance = 0.0016f,
                    MaxStepThreshold = 256
                }
            },
            new RenderPreset
            {
                Name = "Ultra",
                Settings = new RenderSettings
                {
                    Alpha = 0.1f,
                    AlphaThreshold = 0.999f,
                    StepDistance = 0.0002f,
                    MaxStepThreshold = 2048
                }
            }
        };

        public IEnumerable<RenderPreset> RenderPresets => renderPresets;

        public void ApplyPreset(RenderPreset present)
        {
            if (volumeRender)
            {
                volumeRender.Material.SetFloat("_Alpha", present.Settings.Alpha);
                volumeRender.Material.SetFloat("_AlphaThreshold", present.Settings.AlphaThreshold);
                volumeRender.Material.SetFloat("_StepDistance", present.Settings.StepDistance);
                volumeRender.Material.SetInt("_MaxStepThreshold", present.Settings.MaxStepThreshold);
            }
        }
    }
}