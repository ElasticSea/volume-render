using System;
using UnityEngine;
using UnityEngine.XR;
using Util;

namespace Render
{
    public class VolumeSlice : MonoBehaviour
    {
        [SerializeField] private bool isRightController;
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
            var orientationCharacteristics = isRightController ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left;
            var controllerCharacteristics = orientationCharacteristics | InputDeviceCharacteristics.Controller;
            var isHeld = controllerCharacteristics.GetDevice().TryGetFeatureValue(CommonUsages.trigger, out var heldValue) && heldValue > 0.7f;

            if (isHeld)
            {
                vrm.Cut(transform.position, transform.up);
            }
        }
    }
}
