using System.Collections.Generic;
using UnityEngine;

namespace Render.Ui
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private VolumeRenderManager volumeRenderManager;

        private Material material => volumeRenderManager?.VolumeRender?.Material;

        public bool IsActive => material != null;
        
        public float Size
        {
            get => volumeRenderManager?.VolumeRender?.transform?.localScale.x ?? -1;
            set
            {
                var volumeRenderTransform = volumeRenderManager?.VolumeRender?.transform;
                if (volumeRenderTransform)
                {
                    volumeRenderTransform.localScale = new Vector3(value, value, value);
                }
            }
        }

        public float Alpha
        {
            get => material?.GetFloat("_Alpha") ?? -1;
            set => material?.SetFloat("_Alpha", value);
        }

        public float AlphaThreshold
        {
            get => material?.GetFloat("_AlphaThreshold") ?? -1;
            set => material?.SetFloat("_AlphaThreshold", value);
        }

        public float StepDistance
        {
            get => material?.GetFloat("_StepDistance") ?? -1;
            set => material?.SetFloat("_StepDistance", value);
        }

        public float ClipMinimumThreashold
        {
            get => material?.GetFloat("_ClipMin") ?? -1;
            set => material?.SetFloat("_ClipMin", Mathf.Clamp01(value));
        }

        public float ClipMaximumThreashold
        {
            get => material?.GetFloat("_ClipMax") ?? -1;
            set => material?.SetFloat("_ClipMax", Mathf.Clamp01(value));
        }

        public int MaxStepThreshold
        {
            get => material?.GetInt("_MaxStepThreshold") ?? -1;
            set => material?.SetInt("_MaxStepThreshold", value);
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
            material?.SetFloat("_Alpha", present.Settings.Alpha);
            material?.SetFloat("_AlphaThreshold", present.Settings.AlphaThreshold);
            material?.SetFloat("_StepDistance", present.Settings.StepDistance);
            material?.SetInt("_MaxStepThreshold", present.Settings.MaxStepThreshold);
        }
    }
}