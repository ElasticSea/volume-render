using UnityEngine;

namespace Render
{
    public class VolumeGroupRender : MonoBehaviour, IVolumeRenderer
    {
        private VolumeRender[] volumeRenders = new VolumeRender[0];

        private float alpha;
        public float Alpha
        {
            get => alpha;
            set
            {
                alpha = value;
                for (var i = 0; i < volumeRenders.Length; i++)
                {
                    volumeRenders[i].Alpha = value;
                }
            }
        }

        private float alphaThreshold;
        public float AlphaThreshold
        {
            get => alphaThreshold;
            set
            {
                alphaThreshold = value;
                for (var i = 0; i < volumeRenders.Length; i++)
                {
                    volumeRenders[i].AlphaThreshold = value;
                }
            }
        }

        private float stepDistance;
        public float StepDistance
        {
            get => stepDistance;
            set
            {
                stepDistance = value;
                for (var i = 0; i < volumeRenders.Length; i++)
                {
                    volumeRenders[i].StepDistance = value;
                }
            }
        }

        private float clipMinimumThreashold;
        public float ClipMinimumThreashold
        {
            get => clipMinimumThreashold;
            set
            {
                clipMinimumThreashold = value;
                for (var i = 0; i < volumeRenders.Length; i++)
                {
                    volumeRenders[i].ClipMinimumThreashold = value;
                }
            }
        }

        private float clipMaximumThreashold;
        public float ClipMaximumThreashold
        {
            get => clipMaximumThreashold;
            set
            {
                clipMaximumThreashold = value;
                for (var i = 0; i < volumeRenders.Length; i++)
                {
                    volumeRenders[i].ClipMaximumThreashold = value;
                }
            }
        }

        private int maxStepThreshold;
        public int MaxStepThreshold
        {
            get => maxStepThreshold;
            set
            {
                maxStepThreshold = value;
                for (var i = 0; i < volumeRenders.Length; i++)
                {
                    volumeRenders[i].MaxStepThreshold = value;
                }
            }
        }

        public void SetCutPlane(Vector3 position, Vector3 normal)
        {
            for (var i = 0; i < volumeRenders.Length; i++)
            {
                volumeRenders[i].SetCutPlane(position, normal);
            }
        }

        public VolumeRender[] VolumeRenders
        {
            get => volumeRenders;
            set => volumeRenders = value;
        }
    }
}