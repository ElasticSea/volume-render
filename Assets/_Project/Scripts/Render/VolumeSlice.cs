using System;
using UnityEngine;

namespace Render
{
    public class VolumeSlice : MonoBehaviour
    {
        [SerializeField] private VolumeRenderManager vrm;
        [SerializeField] private VolumeRender volumeRender;

        public VolumeRender VolumeRender
        {
            get => volumeRender;
            set => volumeRender = value;
        }

        private void Awake()
        {
            vrm.OnVolumeLoaded += render =>
            {
                VolumeRender = render;
                render.transform.position = new Vector3(-1.942f, 2.365f, 0.695f);
            };
        }

        private void Update()
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
