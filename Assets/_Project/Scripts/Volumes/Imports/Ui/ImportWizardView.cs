using System;
using System.Collections.Generic;
using System.Diagnostics;
using ElasticSea.Framework.Extensions;
using ElasticSea.Framework.Util;
using UnityEngine;
using UnityEngine.UIElements;
using Util.Ui;
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
            var offset = root.Q<TextField>("offset");
            var infoBox = root.Q<HelpBox>("info");
            
            infoBox.visible = false;

            UiUtils.EnumField(Utils.GetEnumValues<ChannelDepth>(), importWizard.Depth, container, depth =>
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

            offset.value = importWizard.Offset.ToString("N");
            offset.RegisterValueChangedCallback(evt =>
            {
                HandleErrors(() =>
                {
                    if (int.TryParse(evt.newValue, out var val))
                    {
                        importWizard.Offset = val;
                        TriggerChange();
                    }
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
            // var needToCrop = importWizard.IsAutomaticCrop;
            var result = importWizard.Bounds;
            // if (needToCrop)
            // {
            //     var warning =
            //         $"Volume is too large and will be automatically cropped. Volume has to be under 2048x2048x2048 voxels and 2GB\n"+
            //         $"Resulting volume is {result.Width}x{result.Height}x{result.Depth} and {importWizard.VolumeSize(result)}B";
            //
            //     ShowMessage(warning, HelpBoxMessageType.Warning);
            // }
            // else
            // {
                ShowMessage($"Resulting volume is {result.Width}x{result.Height}x{result.Depth} and {importWizard.VolumeSize(result)}B", HelpBoxMessageType.Info);
            // }
        }

        private void ShowMessage(string text, HelpBoxMessageType type)
        {
            var root = uiDocument.rootVisualElement;
            var infoBox = root.Q<HelpBox>("info");
            infoBox.messageType = type;
            infoBox.text = text;
            infoBox.visible = true;
        }
    }
}
