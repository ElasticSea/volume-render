using System.IO;
using System.Linq;
using ElasticSea.Framework.Extensions;
using ElasticSea.Framework.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace Volumes.Ui
{
    public class VolumesView : MonoBehaviour
    {
        [SerializeField] private VolumesViewModel volumesViewModel;
        [SerializeField] private UIDocument uiDocument;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var listView = root.Q<ListView>("volumes");
                
            listView.makeItem = () => new Label();
            listView.bindItem =  (e, i) =>
            {
                var volumeSource = (listView.itemsSource[i] as VolumeSource);
                var volume = volumeSource.Volume;
                var name = new FileInfo(volumeSource.FilePath).Name;
                var bytes = (long)volume.Width * volume.Height * volume.Depth * volume.VolumeFormat.GetBitsPerVoxel() / 8;
                var text = $"{name} {volume.Width}x{volume.Height}x{volume.Depth} at {Utils.BytesToString(bytes)}";
                (e as Label).text = text;
                (e as Label).style.fontSize = 14;
            };
            listView.itemsSource = volumesViewModel.Volumes.ToList();
            listView.Refresh();
            
            var loadVolume = root.Q<Button>("loadVolume");
            loadVolume.clicked += () =>
            {
                volumesViewModel.LoadVolume(listView.selectedItem as VolumeSource);
            };
            
            // TODO Remove
            uiDocument.rootVisualElement.Q<Button>("refreshAll").clicked += () =>
            {
                listView.itemsSource = volumesViewModel.Volumes.ToList();
                listView.Refresh();
            };
        }
    }
}
