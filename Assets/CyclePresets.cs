using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Render;
using UnityEngine;
using UnityEngine.XR;
using Util;

public class CyclePresets : MonoBehaviour
{
    [SerializeField] private bool isRightController;
    [SerializeField] private VolumeRenderManager manager;

    private bool isDown;

    private void Update()
    {
        var orientationCharacteristics = isRightController ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left;
        var controllerCharacteristics = orientationCharacteristics | InputDeviceCharacteristics.Controller;
        var isAvailable = controllerCharacteristics.GetDevice().TryGetFeatureValue(CommonUsages.primaryButton, out var isHeldButton);
        
        var isHeld = isAvailable && isHeldButton == true;
        var isReleased = isAvailable && isHeldButton == false;

        if (isHeld && isDown == false)
        {
            Cycle();
            isDown = true;
        }

        if (isReleased && isDown == true)
        {
            isDown = false;
        }
    }

    private int i;
    private void Cycle()
    {
        var availablePresets = manager.RenderPresets.ToList();
        manager.ApplyPreset(availablePresets[i++ % availablePresets.Count]);
    }
}
