using System.Collections.Generic;
using UnityEngine;

namespace Render.Ui
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private VolumeRenderManager volumeRenderManager;

        public bool IsActive => true;

        public float Size
        {
            get => volumeRenderManager?.Size ?? 0f;
            set
            {
                if (volumeRenderManager.VolumeRender)
                {
                    volumeRenderManager.Size = value;
                }
            }
        }

        public float Alpha
        {
            get => volumeRenderManager.VolumeRender?.Alpha ?? 0f;
            set
            {
                if (volumeRenderManager.VolumeRender)
                {
                    volumeRenderManager.VolumeRender.Alpha = value;
                }
            }
        }

        public float AlphaThreshold
        {
            get => volumeRenderManager.VolumeRender?.AlphaThreshold ?? 0f;
            set
            {
                if (volumeRenderManager.VolumeRender)
                {
                    volumeRenderManager.VolumeRender.AlphaThreshold = value;
                }
            }
        }

        public float StepDistance
        {
            get => volumeRenderManager.VolumeRender?.StepDistance ?? 0f;
            set
            {
                if (volumeRenderManager.VolumeRender)
                {
                    volumeRenderManager.VolumeRender.StepDistance = value;
                }
            }
        }

        public float ClipMinimumThreashold
        {
            get => volumeRenderManager.VolumeRender?.ClipMinimumThreashold ?? 0f;
            set
            {
                if (volumeRenderManager.VolumeRender)
                {
                    volumeRenderManager.VolumeRender.ClipMinimumThreashold = value;
                }
            }
        }

        public float ClipMaximumThreashold
        {
            get => volumeRenderManager.VolumeRender?.ClipMaximumThreashold ?? 0f;
            set
            {
                if (volumeRenderManager.VolumeRender)
                {
                    volumeRenderManager.VolumeRender.ClipMaximumThreashold = value;
                }
            }
        }

        public int MaxStepThreshold
        {
            get => volumeRenderManager.VolumeRender?.MaxStepThreshold  ?? 0;
            set
            {
                if (volumeRenderManager.VolumeRender)
                {
                    volumeRenderManager.VolumeRender.MaxStepThreshold = value;
                }
            }
        }

        public IEnumerable<RenderPreset> RenderPresets => volumeRenderManager.RenderPresets;

        public void ApplyPreset(RenderPreset present)
        {
            volumeRenderManager.ApplyPreset(present);
        }
    }
}