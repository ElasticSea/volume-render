using Render;
using UnityEngine;

public class ArThingie : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private VolumeRenderManager vrm;

    private void Update()
    {
        if (vrm.VolumeRender)
        {
            var lookDir = cam.transform.position - vrm.VolumeRender.transform.position;
            vrm.VolumeRender.SetCutPlane(Vector3.zero, lookDir);
        }
    }
}
