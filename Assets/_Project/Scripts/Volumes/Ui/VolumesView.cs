using System.Linq;
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
                var text = $"{volume.Width}x{volume.Height}x{volume.Depth}:{volume.ChannelDepthBits}Bit\n{volumeSource.FilePath}";
                (e as Label).text = text;
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
