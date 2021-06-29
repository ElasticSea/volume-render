using Render;
using UnityEngine;

namespace Volumes.Render
{
    public class VolumeSlice : MonoBehaviour
    {
        [SerializeField] private VolumeRenderManager vrm;

        private void LateUpdate()
        {
            vrm.SetCutPlane(transform.position, transform.up);
        }
    }
}
