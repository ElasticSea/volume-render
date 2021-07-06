using ElasticSea.Framework.Extensions;
using UnityEngine;
using UnityEngine.XR;
using Util;
using Volumes.Render;

public class SliceHandSync : MonoBehaviour
{
    [SerializeField] private bool isRightController;
    [SerializeField] private VolumeSlice slice;
    [SerializeField] private bool syncAlways = false;

    private void Update()
    {
        var orientationCharacteristics = isRightController ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left;
        var controllerCharacteristics = orientationCharacteristics | InputDeviceCharacteristics.Controller;
        var isHeld = controllerCharacteristics.GetDevice().TryGetFeatureValue(CommonUsages.grip, out var heldValue) && heldValue > 0.7f;

        if (syncAlways || isHeld)
        {
            slice.transform.CopyWorldFrom(transform);
        }
    }
}