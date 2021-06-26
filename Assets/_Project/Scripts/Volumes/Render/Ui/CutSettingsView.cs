using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Util.Ui;
using Volumes;

namespace Render.Ui
{
    public class CutSettingsView : MonoBehaviour
    {
        [SerializeField] private CutSettings cutSettings;
        [SerializeField] private UIDocument uiDocument;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var panel = root.Q<VisualElement>("cutPanel");
            var cutProgress = panel.Q<Slider>("Progress");

            cutProgress.value = cutSettings.CutProgress;
            cutProgress.RegisterValueChangedCallback(evt =>
            {
                cutSettings.CutProgress = evt.newValue;
            });
        }
    }
}