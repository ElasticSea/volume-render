using Render;
using UnityEngine;

public class ArThingie : MonoBehaviour
{
    [SerializeField] private Camera cam;
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
        };
    }

    private void Update()
    {
        if (volumeRender)
        {
            var volTransform = volumeRender.transform;
            var volMat = volumeRender.Material;

            var lookDir = cam.transform.position - volumeRender.transform.position;
            
            volMat.SetVector("_CutNormal", volTransform.InverseTransformVector(lookDir));
            volMat.SetVector("_CutOrigin", Vector4.zero);
        }
    }
}
