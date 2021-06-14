using System;
using System.Collections.Generic;
using System.Linq;
using ElasticSea.Framework.Extensions;
using UnityEngine;
using Volumes;

namespace Render
{
    public class VolumeRenderManager : MonoBehaviour
    {
        [SerializeField] private VolumeRender volumePrefab;

        public event Action<VolumeGroupRender> OnVolumeLoaded = render => { };

        public void LoadVolume(RuntimeVolume volume)
        {
            if (volumeRender)
            {
                Destroy(volumeRender);
            }
            
            volumeRender = new GameObject("Volume Render").AddComponent<VolumeGroupRender>();

            var volumeRenderers = new List<VolumeRender>();
            this.volume = volume;
            var clustersX = volume.Clusters.GetLength(0);
            var clustersY = volume.Clusters.GetLength(1);
            var clustersZ = volume.Clusters.GetLength(2);
            var offset = new Vector3();
            // for (var x = 0; x < clustersX; x++)
            // {
            //     for (var y = 0; y < clustersY; y++)
            //     {
            //         for (var z = 0; z < clustersZ; z++)
            //         {
            //             var cluster = volume.Clusters[x, y, z];
            //             var render = Instantiate(volumePrefab, volumeRender.transform, false);
            //             render.Volume = cluster.Texture;
            //
            //             var xPosAbs = x == clustersX - 1
            //                 ? volume.Clusters[x - 1, y, z].Width * (x - 1) + cluster.Width / 2
            //                 : cluster.Width * (x - 1 + 0.5f);
            //             
            //             var yPosAbs = y == clustersY - 1
            //                 ? volume.Clusters[x, y - 1, z].Height * (y - 1) + cluster.Height / 2
            //                 : cluster.Height * (y - 1 + 0.5f);
            //             
            //             var zPosAbs = z == clustersZ - 1
            //                 ? volume.Clusters[x , y, z- 1].Depth * (z - 1) + cluster.Depth / 2
            //                 : cluster.Depth * (z - 1 + 0.5f);
            //             
            //             var csx = (float) xPosAbs / volume.Width;
            //             var csy = (float) yPosAbs / volume.Height;
            //             var csz = (float) zPosAbs / volume.Depth;
            //             
            //             var csx2 = (float) cluster.Width / volume.Width;
            //             var csy2 = (float) cluster.Height / volume.Height;
            //             var csz2 = (float) cluster.Depth / volume.Depth;
            //             render.transform.localPosition = new Vector3(csx, csy, csz);
            //             
            //             
            //             render.transform.localScale = new Vector3(csx2, csy2, csz2);
            //             volumeRenderers.Add(render);
            //         }
            //     }
            // }
            var maxL = Mathf.Max(clustersX, clustersY, clustersZ);
            for (var x = 0; x < clustersX; x++)
            {
                for (var y = 0; y < clustersY; y++)
                {
                    for (var z = 0; z < clustersZ; z++)
                    {
                        
                        var cluster = volume.Clusters[x, y, z];
                        var render = Instantiate(volumePrefab, volumeRender.transform, false);
                        render.Volume = cluster.Texture;
                        var offset222 = 0.000f;
                        render.transform.localPosition = new Vector3((1f/maxL + offset222) * x, (1f/maxL + offset222)* y, (1f/maxL+ offset222) * z);
                        render.transform.localScale = new Vector3(1f/maxL, 1f/maxL, 1f/maxL);
                        volumeRenderers.Add(render);
                    }
                }
            }

            volumeRender.VolumeRenders = volumeRenderers.ToArray();
            OnVolumeLoaded(volumeRender);
            
            ApplyPreset(High);
        }

        private static readonly RenderPreset Low = new RenderPreset
        {
            Name = "Low",
            Settings = new RenderSettings
            {
                Alpha = 5f,
                AlphaThreshold = 1f,
                StepDistance = 0.0128f,
                MaxStepThreshold = 640
            }
        };

        private static readonly RenderPreset High = new RenderPreset
        {
            Name = "High",
            Settings = new RenderSettings
            {
                Alpha = 0.77f,
                AlphaThreshold = 1f,
                StepDistance = 0.0016f,
                MaxStepThreshold = 5120
            }
        };

        private static readonly RenderPreset Ultra = new RenderPreset
        {
            Name = "Ultra",
            Settings = new RenderSettings
            {
                Alpha = 0.1f,
                AlphaThreshold = 1f,
                StepDistance = 0.0002f,
                MaxStepThreshold = 40960
            }
        };

        private static readonly RenderPreset Test = new RenderPreset
        {
            Name = "Test",
            Settings = new RenderSettings
            {
                Alpha = 0.1f,
                AlphaThreshold = 1f,
                StepDistance = 0.00005f,
                MaxStepThreshold = 40960
            }
        };

        private IEnumerable<RenderPreset> renderPresets = new[] {Low, High, Ultra, Test};
        private VolumeGroupRender volumeRender;
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

        public VolumeGroupRender VolumeRender => volumeRender;

        public void ApplyPreset(RenderPreset present)
        {
            var clustersX = volume.Clusters.GetLength(0);
            var clustersY = volume.Clusters.GetLength(1);
            var clustersZ = volume.Clusters.GetLength(2);
            var maxL = Mathf.Max(clustersX, clustersY, clustersZ);
            volumeRender.Alpha = present.Settings.Alpha;
            volumeRender.AlphaThreshold = present.Settings.AlphaThreshold;
            volumeRender.StepDistance = present.Settings.StepDistance * maxL;
            volumeRender.MaxStepThreshold = present.Settings.MaxStepThreshold;
        }
    }
}