using System;
using System.Collections.Generic;
using System.Diagnostics;
using ElasticSea.Framework.Extensions;
using ElasticSea.Framework.Util;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Volumes.Imports.Ui
{
    public class ImportWizardView : MonoBehaviour
    {
        [SerializeField] private ImportWizard importWizard;
        [SerializeField] private UIDocument uiDocument;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var container = root.Q<VisualElement>("container");
            var import = root.Q<Button>("import");
            var name = root.Q<TextField>("name");
            var source = root.Q<TextField>("source");
            var multithreaded = root.Q<Toggle>("multithreaded");
            var infoBox = root.Q<HelpBox>("info");
            infoBox.visible = false;

            EnumField(Utils.GetEnumValues<ChannelDepth>(), importWizard.Depth, container, depth =>
            {
                HandleErrors(() =>
                {
                    importWizard.Depth = depth;
                    TriggerChange();
                });
            });

            import.clicked += () =>
            {
                HandleErrors(() =>
                {
                    var sw = Stopwatch.StartNew();
                    importWizard.Import();
                    ShowMessage($"Volume successfully created in {sw.ElapsedMilliseconds}ms", HelpBoxMessageType.Info);
                });
            };

            name.value = importWizard.VolumeName;
            name.RegisterValueChangedCallback(evt =>
            {
                HandleErrors(() =>
                {
                    importWizard.VolumeName = evt.newValue;
                    TriggerChange();
                });
            });

            source.value = importWizard.SourcePath;
            source.RegisterValueChangedCallback(evt =>
            {
                HandleErrors(() =>
                {
                    importWizard.SourcePath = evt.newValue;
                    TriggerChange();
                });
            });

            multithreaded.value = importWizard.Multithreaded;
            multithreaded.RegisterValueChangedCallback(evt =>
            {
                HandleErrors(() =>
                {
                    importWizard.Multithreaded = evt.newValue;
                    TriggerChange();
                });
            });
        }

        private void HandleErrors(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ShowMessage(e.Message, HelpBoxMessageType.Error);
            }
        }

        private void TriggerChange()
        {
            var header = importWizard.GetHeader();
            var bounds = importWizard.GetValidBounds();

            var needToCrop = header.Width != bounds.Width || header.Height != bounds.Height || header.Depth != bounds.Depth;
            if (needToCrop)
            {
                var warning = $"Volume is too large. Volume has to be under 2048x2048x2048 voxels and 2GB\n" +
                              $"Loaded volume with resolution {header.Width}x{header.Height}x{header.Depth} at " +
                              $"{importWizard.Depth.GetBitsSize()/8}B per voxel at {importWizard.OriginalVolumeSize()}B will " +
                              $"be cropped to {bounds.Width}x{bounds.Height}x{bounds.Depth} at {importWizard.ResultVolumeSize()}B";
            
                ShowMessage(warning, HelpBoxMessageType.Warning);
            }
            else
            {
                ShowMessage($"Resulting volume is {bounds.Width}x{bounds.Height}x{bounds.Depth} and {importWizard.ResultVolumeSize()}B", HelpBoxMessageType.Info);
            }
        }

        private void ShowMessage(string text, HelpBoxMessageType type)
        {
            var root = uiDocument.rootVisualElement;
            var infoBox = root.Q<HelpBox>("info");
            infoBox.messageType = type;
            infoBox.text = text;
            infoBox.visible = true;
        }

        private void EnumField<T>(IEnumerable<T> getEnumValues,T defaultValue, VisualElement container, Action<T> callback)
        {
            var buttons = new Dictionary<T, Button>();

            void Select(Button btn)
            {
                foreach (var (key, value) in buttons)
                {
                    var selectedColor = new Color(.5f, .5f, .5f, 1f);
                    var defaultColor = new Color(.894f, .894f, .894f, 1f);
                    var b1 = Equals(buttons[key], btn);
                    var color = b1 ? selectedColor : defaultColor;
                    value.style.backgroundColor = new StyleColor(color);
                }
            }

            foreach (var channelDepth in getEnumValues)
            {
                var button = new Button();
                button.text = channelDepth.ToString();
                button.clicked += () =>
                {
                    Select(button);

                    callback(channelDepth);
                };
                container.Add(button);
                buttons[channelDepth] = button;
            }

            Select(buttons[defaultValue]);
        }
    }
}
