using ElasticSea.Framework.Extensions;
using UnityEngine;
using UnityEngine.XR;
using Util;
using Volumes.Render;

public class SliceHandSync : MonoBehaviour
{
    [SerializeField] private bool isRightController;
    [SerializeField] private VolumeSlice slice;

    private void Update()
    {
        var orientationCharacteristics = isRightController ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left;
        var controllerCharacteristics = orientationCharacteristics | InputDeviceCharacteristics.Controller;
        var isHeld = controllerCharacteristics.GetDevice().TryGetFeatureValue(CommonUsages.trigger, out var heldValue) && heldValue > 0.7f;

        if (isHeld)
        {
            slice.transform.CopyWorldFrom(transform);
        }
    }
}