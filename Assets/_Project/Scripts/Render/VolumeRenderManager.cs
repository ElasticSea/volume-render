using System.Diagnostics;
using Render.Ui;
using UnityEngine;
using Volumes;

namespace Render
{
    public class VolumeRenderManager : MonoBehaviour
    {
        [SerializeField] private GameObject hand;
        [SerializeField] private VolumeRender volumePrefab;
        
        private VolumeRender volumeRender;

        public VolumeRender VolumeRender => volumeRender;

        public void LoadVolume(string path)
        {
            var sw = Stopwatch.StartNew();

            var volume = VolumeManager.LoadVolume(path);
            var texture = volume.ToTexture();

            if (volumeRender)
            {
                Destroy(volumeRender.Material.GetTexture("_Volume"));
                Destroy(volumeRender.gameObject);
            }
        
            volumeRender = Instantiate(volumePrefab);
            volumeRender.Material.SetTexture("_Volume", texture);
            volumeRender.Material.SetFloat("_Alpha", 0.03f);
            volumeRender.Material.SetFloat("_StepDistance", 0.0001f);

            volumeRender.transform.position = new Vector3(-1.942f, 2.365f, 0.695f);

            var slice = hand.AddComponent<VolumeSlice>();
            slice.VolumeRender = volumeRender;
        
            print($"Elapsed {sw.ElapsedMilliseconds}ms");
        }
    }
}