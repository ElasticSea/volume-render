using System;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Render
{
    public class VolumeRender : MonoBehaviour, IVolumeRenderer
    {
        private Material material;

        public Material Material
        {
            set
            {
                material = value;
                GetComponent<Renderer>().material = material;
            }
        }

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
        }

        private float alpha;

        public float Alpha
        {
            get => alpha;
            set
            {
                alpha = value;
                material.SetFloat("_Alpha", alpha);
            }
        }

        private float alphaThreshold;

        public float AlphaThreshold
        {
            get => alphaThreshold;
            set
            {
                alphaThreshold = value;
                material.SetFloat("_AlphaThreshold", alphaThreshold);
                material.EnableKeyword("_ALPHATHRESHOLD_ON", alphaThreshold < 1);
            }
        }

        private float stepDistance;

        public float StepDistance
        {
            get => stepDistance;
            set
            {
                stepDistance = value;
                material.SetFloat("_StepDistance", stepDistance);
            }
        }

        private float clipMinimumThreashold;

        public float ClipMinimumThreashold
        {
            get => clipMinimumThreashold;
            set
            {
                clipMinimumThreashold = value;
                material.SetFloat("_ClipMin", clipMinimumThreashold);
                material.EnableKeyword("_CLIP_ON", clipMinimumThreashold > 0 || clipMaximumThreashold < 1);
            }
        }

        private float clipMaximumThreashold;

        public float ClipMaximumThreashold
        {
            get => clipMaximumThreashold;
            set
            {
                clipMaximumThreashold = value;
                material.SetFloat("_ClipMax", clipMaximumThreashold);
                material.EnableKeyword("_CLIP_ON", clipMinimumThreashold > 0 || clipMaximumThreashold < 1);
            }
        }

        private int maxStepThreshold;
        private Texture3D volume;

        public int MaxStepThreshold
        {
            get => maxStepThreshold;
            set
            {
                maxStepThreshold = value;
                material.SetInt("_MaxStepThreshold", maxStepThreshold);
            }
        }

        public Texture3D Volume
        {
            get => volume;
            set
            {
                volume = value;
                material.SetTexture("_Volume", volume);
            }
        }

        public void SetCutPlane(Vector3 position, Vector3 normal)
        {
            var localPos = transform.InverseTransformPoint(position);
            var localPosition = transform.InverseTransformVector(normal);

            material.SetVector("_CutOrigin", localPos);
            material.SetVector("_CutNormal", localPosition);
        }

        private void OnDestroy()
        {
            Destroy(Volume);
        }

        private void OnDrawGizmos()
        {
            var pos = material.GetVector("_CutOrigin");
            var nor = material.GetVector("_CutNormal");
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(pos, pos + nor);
        }
    }
}