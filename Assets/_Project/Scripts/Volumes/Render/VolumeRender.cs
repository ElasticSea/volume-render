using System;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Render
{
    public class VolumeRender : MonoBehaviour
    {
        [SerializeField] private MeshRenderer renderer;
        
        [SerializeField] private float alpha;
        [SerializeField] private float alphaThreshold;
        [SerializeField] private float stepDistance;
        [SerializeField] private float clipMinimumThreashold;
        [SerializeField] private float clipMaximumThreashold;
        [SerializeField] private bool isGrayscale;
        
        private Material material;
        private Texture3D[,,] volume;

        private void Awake()
        {
            material = renderer.material;
            SetAlpha(alpha);
            SetAlphaThreshold(alphaThreshold);
            SetStepDistance(stepDistance);
            SetClipMinimumThreashold(clipMinimumThreashold);
            SetClipMaximumThreashold(clipMaximumThreashold);
            SetGrayscale(isGrayscale);
        }

        private void SetAlpha(float alpha)
        {
            material.SetFloat("_Alpha", alpha);
        }

        private void SetAlphaThreshold(float alphaThreshold)
        {
            material.SetFloat("_AlphaThreshold", alphaThreshold);
            material.EnableKeyword("_ALPHATHRESHOLD_ON", alphaThreshold < 1);
        }
        
        private void SetStepDistance(float stepDistance)
        {
            material.SetFloat("_StepDistance", stepDistance);
        }
        
        private void SetClipMinimumThreashold(float clipMinimumThreashold)
        {
            material.SetFloat("_ClipMin", clipMinimumThreashold);
            material.EnableKeyword("_CLIP_ON", clipMinimumThreashold > 0 || clipMaximumThreashold < 1);
        }
        
        private void SetClipMaximumThreashold(float clipMaximumThreashold)
        {
            material.SetFloat("_ClipMax", clipMaximumThreashold);
            material.EnableKeyword("_CLIP_ON", clipMinimumThreashold > 0 || clipMaximumThreashold < 1);
        }
        
        private void SetGrayscale(bool grayscale)
        {
            if (grayscale)
            {
                material.DisableKeyword("_COLOR_ON");
                material.EnableKeyword("_COLOR_OFF");
            }
            else
            {
                material.EnableKeyword("_COLOR_ON");
                material.DisableKeyword("_COLOR_OFF");
            }
        }

        public float Alpha
        {
            get => alpha;
            set
            {
                alpha = value;
                SetAlpha(alpha);
            }
        }

        public float AlphaThreshold
        {
            get => alphaThreshold;
            set
            {
                alphaThreshold = value;
                SetAlphaThreshold(alphaThreshold);
            }
        }

        public float StepDistance
        {
            get => stepDistance;
            set
            {
                stepDistance = value;
                SetStepDistance(stepDistance);
            }
        }

        public float ClipMinimumThreashold
        {
            get => clipMinimumThreashold;
            set
            {
                clipMinimumThreashold = value;
                SetClipMinimumThreashold(clipMinimumThreashold);
            }
        }

        public float ClipMaximumThreashold
        {
            get => clipMaximumThreashold;
            set
            {
                clipMaximumThreashold = value;
                SetClipMaximumThreashold(clipMaximumThreashold);
            }
        }

        public bool IsGrayscale
        {
            get => isGrayscale;
            set
            {
                isGrayscale = value;
                SetGrayscale(isGrayscale);
            }
        }

        public void SetVolume(Texture3D[,,] volume)
        {
            this.volume = volume;
            var volumeCount = volume.GetLength(0) * volume.GetLength(1) * volume.GetLength(2);

            switch (volumeCount)
            {
                case 1:
                    material.SetTexture("_Volume", volume[0, 0, 0]);
                    material.EnableKeyword("_OCTVOLUME_OFF", true);
                    material.EnableKeyword("_OCTVOLUME_ON", false);
                    break;
                case 8:
                    material.SetTexture("_Volume000", volume[0, 0, 0]);
                    material.SetTexture("_Volume001", volume[0, 0, 1]);
                    material.SetTexture("_Volume010", volume[0, 1, 0]);
                    material.SetTexture("_Volume011", volume[0, 1, 1]);
                    material.SetTexture("_Volume100", volume[1, 0, 0]);
                    material.SetTexture("_Volume101", volume[1, 0, 1]);
                    material.SetTexture("_Volume110", volume[1, 1, 0]);
                    material.SetTexture("_Volume111", volume[1, 1, 1]);
                    material.EnableKeyword("_OCTVOLUME_OFF", false);
                    material.EnableKeyword("_OCTVOLUME_ON", true);
                    break;
                default:
                    throw new ArgumentException($"Renderer does not support {volumeCount} volumes.");
            }
        }

        public void SetCutPlane(Vector3 position, Vector3 normal)
        {
            var localPos = transform.InverseTransformPoint(position);
            var localRotationWithoutScale = transform.rotation * normal;

            material.SetVector("_CutOrigin", localPos);
            material.SetVector("_CutNormal", localRotationWithoutScale);
        }

        private void OnDestroy()
        {
            foreach (var v in volume)
            {
                Destroy(v);
            }
        }

        private void OnDrawGizmos()
        {
            if (material)
            {
                var pos = material.GetVector("_CutOrigin");
                var nor = material.GetVector("_CutNormal");
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawLine(pos, pos + nor);
            }
        }
    }
}