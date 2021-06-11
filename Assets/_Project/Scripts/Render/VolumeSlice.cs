using System;
using UnityEngine;

namespace Render
{
    public class VolumeSlice : MonoBehaviour
    {
        [SerializeField] private VolumeRenderManager vrm;

        private void Awake()
        {
            // TODO Remove
            vrm.OnVolumeLoaded += render =>
            {
                render.transform.position = new Vector3(-1.942f, 2.365f, 0.695f);
            };
        }

        private void Update()
        {
            vrm.Cut(transform.position, transform.up);
        }
    }
}
