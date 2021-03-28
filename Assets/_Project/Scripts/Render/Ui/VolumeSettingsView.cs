using UnityEngine;
using UnityEngine.UIElements;

namespace Render.Ui
{
    public class VolumeSettingsView : MonoBehaviour
    {
        [SerializeField] private VolumeSettings volumeSettings;
        [SerializeField] private UIDocument uiDocument;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var panel = root.Q<VisualElement>("renderPanel");
            var alpha = panel.Q<TextField>("alpha");
            var alphaThreshold = panel.Q<TextField>("alphaThreshold");
            var stepDistance = panel.Q<TextField>("stepDistance");
            var minClipThreshold = panel.Q<Slider>("minClipThreshold");
            var maxClipThreshold = panel.Q<Slider>("maxClipThreshold");

            panel.visible = volumeSettings.IsActive;

            alpha.value = volumeSettings.Alpha.ToString("F16");
            alpha.RegisterValueChangedCallback(evt =>
            {
                if (float.TryParse(evt.newValue, out var val))
                {
                    volumeSettings.Alpha = val;
                }
            });

            alphaThreshold.value = volumeSettings.AlphaThreshold.ToString("F16");
            alphaThreshold.RegisterValueChangedCallback(evt =>
            {
                if (float.TryParse(evt.newValue, out var val))
                {
                    volumeSettings.AlphaThreshold = val;
                }
            });

            stepDistance.value = volumeSettings.StepDistance.ToString("F16");
            stepDistance.RegisterValueChangedCallback(evt =>
            {
                if (float.TryParse(evt.newValue, out var val))
                {
                    volumeSettings.StepDistance = val;
                }
            });

            minClipThreshold.value = volumeSettings.ClipMinimumThreashold;
            minClipThreshold.RegisterValueChangedCallback(evt =>
            {
                volumeSettings.ClipMinimumThreashold = evt.newValue;
            });

            maxClipThreshold.value = volumeSettings.ClipMaximumThreashold;
            maxClipThreshold.RegisterValueChangedCallback(evt =>
            {
                volumeSettings.ClipMaximumThreashold = evt.newValue;
            });
            
            // TODO Remove
            uiDocument.rootVisualElement.Q<Button>("refreshAll").clicked += () =>
            {
                panel.visible = volumeSettings.IsActive;
                alpha.value = volumeSettings.Alpha.ToString("F16");
                alphaThreshold.value = volumeSettings.AlphaThreshold.ToString("F16");
                stepDistance.value = volumeSettings.StepDistance.ToString("F16");
                minClipThreshold.value = volumeSettings.ClipMinimumThreashold;
                maxClipThreshold.value = volumeSettings.ClipMaximumThreashold;
            };
        }
    }
}