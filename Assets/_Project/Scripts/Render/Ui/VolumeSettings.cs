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
            get => volumeRenderManager.Size;
            set => volumeRenderManager.Size = value;
        }

        public float Alpha
        {
            get => volumeRenderManager.VolumeRender.Alpha;
            set => volumeRenderManager.VolumeRender.Alpha = value;
        }

        public float AlphaThreshold
        {
            get => volumeRenderManager.VolumeRender.AlphaThreshold;
            set => volumeRenderManager.VolumeRender.AlphaThreshold = value;
        }

        public float StepDistance
        {
            get => volumeRenderManager.VolumeRender.StepDistance;
            set => volumeRenderManager.VolumeRender.StepDistance = value;
        }


        public float ClipMinimumThreashold
        {
            get => volumeRenderManager.VolumeRender.ClipMinimumThreashold;
            set => volumeRenderManager.VolumeRender.ClipMinimumThreashold = value;
        }


        public float ClipMaximumThreashold
        {
            get => volumeRenderManager.VolumeRender.ClipMaximumThreashold;
            set => volumeRenderManager.VolumeRender.ClipMaximumThreashold = value;
        }


        public int MaxStepThreshold
        {
            get => volumeRenderManager.VolumeRender.MaxStepThreshold;
            set => volumeRenderManager.VolumeRender.MaxStepThreshold = value;
        }

        public IEnumerable<RenderPreset> RenderPresets => volumeRenderManager.RenderPresets;

        public void ApplyPreset(RenderPreset present)
        {
            volumeRenderManager.ApplyPreset(present);
        }
    }
}