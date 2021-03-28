using UnityEngine;

namespace Render
{
    public class VolumeSlice : MonoBehaviour
    {
        [SerializeField] private VolumeRender volumeRender;

        public VolumeRender VolumeRender
        {
            get => volumeRender;
            set => volumeRender = value;
        }

        void Update()
        {
            if (volumeRender)
            {
                var volTransform = volumeRender.transform;
                var volMat = volumeRender.Material;
                
                volMat.SetVector("_CutNormal", volTransform.InverseTransformVector(transform.up));
                volMat.SetVector("_CutOrigin", volTransform.InverseTransformPoint(transform.position));
            }
        }
    }
}
