using UnityEngine;

namespace Render.Ui
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private VolumeRenderManager volumeRenderManager;

        private Material material => volumeRenderManager?.VolumeRender?.Material;

        public bool IsActive => material != null;
        
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
    }
}