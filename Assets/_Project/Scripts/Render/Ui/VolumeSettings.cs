using UnityEngine;

namespace Render.Ui
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private Material material;

        public Material Material
        {
            get => material;
            set => material = value;
        }

        public float Alpha
        {
            get => material.GetFloat("_Alpha");
            set => material.SetFloat("_Alpha", value);
        }

        public float AlphaThreshold
        {
            get => material.GetFloat("_AlphaThreshold");
            set => material.SetFloat("_AlphaThreshold", value);
        }

        public float StepDistance
        {
            get => material.GetFloat("_StepDistance");
            set => material.SetFloat("_StepDistance", value);
        }

        public float ClipMinimumThreashold
        {
            get => material.GetFloat("_ClipMin");
            set => material.SetFloat("_ClipMin", Mathf.Clamp01(value));
        }

        public float ClipMaximumThreashold
        {
            get => material.GetFloat("_ClipMax");
            set => material.SetFloat("_ClipMax", Mathf.Clamp01(value));
        }
    }
}