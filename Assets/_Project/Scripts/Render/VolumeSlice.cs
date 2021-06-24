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
        [SerializeField] private bool sliceWhenHeld;

        private void Update()
        {
            var orientationCharacteristics = isRightController ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left;
            var controllerCharacteristics = orientationCharacteristics | InputDeviceCharacteristics.Controller;
            var isHeld = controllerCharacteristics.GetDevice().TryGetFeatureValue(CommonUsages.trigger, out var heldValue) && heldValue > 0.7f;

            if (sliceWhenHeld == false || isHeld)
            {
                vrm.VolumeRender.SetCutPlane(transform.position, transform.up);
            }
        }
    }
}
