using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ElasticSea.Framework.Extensions;
using UnityEngine;
using Volumes;

namespace Render
{
    public class VolumeRenderManager : MonoBehaviour
    {
        [SerializeField] private VolumeRender volumePrefab;

        public event Action<VolumeRender> OnVolumeLoaded = render => { };

        public VolumeRender LoadVolume(VolumeSource volumeSource)
        {
            using (var stream = File.OpenRead(volumeSource.FilePath))
            {
                var volume = VolumeManager.LoadRuntimeVolume(stream);
                var volumeRender = LoadVolume(volume);
                // Attempt to force the GC release LOH memory and return the memory to OS
                // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
                for (var i = 0; i < 16; i++)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                }
                // Attempt to force the GC release LOH memory and return the memory to OS
                // Running GC.Collect one or twice does not seem to be enough trigger the memory return to OS
                for (var i = 0; i < 16; i++)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                }

                return volumeRender;
            }
        }
        
        public VolumeRender LoadVolume(RuntimeVolume volume)
        {
            if (volumeRender)
            {
                Destroy(volumeRender);
            }
            
            this.volume = volume;
  
            var renderVolumeOct = new Texture3D[2, 2, 2];
            renderVolumeOct[0, 0, 0] = volume.Clusters[0, 0, 0].Texture;
            renderVolumeOct[0, 0, 1] = volume.Clusters[0, 0, 1].Texture;
            renderVolumeOct[0, 1, 0] = volume.Clusters[0, 1, 0].Texture;
            renderVolumeOct[0, 1, 1] = volume.Clusters[0, 1, 1].Texture;
            renderVolumeOct[1, 0, 0] = volume.Clusters[1, 0, 0].Texture;
            renderVolumeOct[1, 0, 1] = volume.Clusters[1, 0, 1].Texture;
            renderVolumeOct[1, 1, 0] = volume.Clusters[1, 1, 0].Texture;
            renderVolumeOct[1, 1, 1] = volume.Clusters[1, 1, 1].Texture;
            
            volumeRender = Instantiate(volumePrefab);
            volumeRender.SetVolume(renderVolumeOct);
            volumeRender.IsGrayscale = volume.Format != VolumeFormat.RGBA32;

            var longestDimensions = Mathf.Max(volume.Width, volume.Height, volume.Depth);
            volumeRender.transform.localScale = new Vector3(
                (float) volume.Width / longestDimensions,
                (float) volume.Height / longestDimensions,
                (float) volume.Depth / longestDimensions
            );
            
            OnVolumeLoaded(volumeRender);
            
            ApplyPreset(High);

            return volumeRender;
        }

        private static readonly RenderPreset Low = new RenderPreset
        {
            Name = "Low",
            Settings = new RenderSettings
            {
                Alpha = 10f,
                AlphaThreshold = 0.95f,
                StepDistance = 0.0128f
            }
        };

        private static readonly RenderPreset Medium = new RenderPreset
        {
            Name = "Medium",
            Settings = new RenderSettings
            {
                Alpha = 2.8f,
                AlphaThreshold = 0.99f,
                StepDistance = 0.0016f
            }
        };
        
        private static readonly RenderPreset High = new RenderPreset
        {
            Name = "High",
            Settings = new RenderSettings
            {
                Alpha = 1.2f,
                AlphaThreshold = 0.995f,
                StepDistance = 0.0006f
            }
        };

        private static readonly RenderPreset VeryHigh = new RenderPreset
        {
            Name = "VeryHigh",
            Settings = new RenderSettings
            {
                Alpha = 0.4f,
                AlphaThreshold = 0.999f,
                StepDistance = 0.0002f
            }
        };

        private static readonly RenderPreset Ultra = new RenderPreset
        {
            Name = "Ultra",
            Settings = new RenderSettings
            {
                Alpha = 0.1f,
                AlphaThreshold = 0.9995f,
                StepDistance = 0.00005f
            }
        };

        private IEnumerable<RenderPreset> renderPresets = new[] {Low, Medium, High, VeryHigh, Ultra};
        private VolumeRender volumeRender;
        private RuntimeVolume volume;

        public IEnumerable<RenderPreset> RenderPresets => renderPresets;
        
        // TODO Fix size
        public float Size
        {
            // get => volumeRenderManager?.VolumeRender?.transform?.localScale.x ?? -1;
            // set
            // {
            //     var volumeRenderTransform = volumeRenderManager?.VolumeRender?.transform;
            //     if (volumeRenderTransform)
            //     {
            //         volumeRenderTransform.localScale = new Vector3(value, value, value);
            //     }
            // }  
            get => 1;
            set
            {
                // var volumeRenderTransform = volumeRenderManager?.VolumeRender?.transform;
                // if (volumeRenderTransform)
                // {
                //     volumeRenderTransform.localScale = new Vector3(value, value, value);
                // }
            }
        }

        public VolumeRender VolumeRender => volumeRender;

        public void ApplyPreset(RenderPreset present)
        {
            var clustersX = volume.Clusters.GetLength(0);
            var clustersY = volume.Clusters.GetLength(1);
            var clustersZ = volume.Clusters.GetLength(2);
            var maxL = Mathf.Max(clustersX, clustersY, clustersZ);
            volumeRender.Alpha = present.Settings.Alpha;
            volumeRender.AlphaThreshold = present.Settings.AlphaThreshold;
            volumeRender.StepDistance = present.Settings.StepDistance * maxL;
        }

        public void SetCutPlane(Vector3 position, Vector3 normal)
        {
            if (volumeRender)
            {
                volumeRender.SetCutPlane(position, normal);
            }
        }
    }
}