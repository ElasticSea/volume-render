using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Volumes.Imports;

namespace Volumes.Factories
{
    public class VolumeFactories
    {
        [MenuItem("Factory/Perfect Sphere")]
        public static void PerfectSphere()
        {
            var volumesDir = Path.Combine(Application.persistentDataPath, "Volumes");

            var size = 256;
            var data = new BigArray<float>(size * size * size);
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    for (var z = 0; z < size; z++)
                    {
                        var center = new Vector3(size/2,size/2,size/2);
                        var voxel = new Vector3(x+1,y+1,z+1);
                        var dir = voxel - center;

                        // var value = dir.magnitude >= .5f ? 0 : 1;
                        var threahold = size / 4;
                        var value =Mathf.SmoothStep( 0.05f, 0.01f, Mathf.InverseLerp(threahold - 1f, threahold + 1f, dir.magnitude));

                        var index = z + y * size + x * size * size;
                        data[index] = value;
                    }
                }
            }
            var rawVolume = new RawVolume(size, size, size, data);
            
            var packed = rawVolume.Data.Pack( ChannelDepth.Quarter8, false);

            var bits = ChannelDepth.Quarter8.GetBitsSize();
            
            var volume =  new Volume(size, size, size, 0, 1, bits, packed);
            
            var volumePath = Path.Combine(volumesDir, "perfectSphere.vlm");
            VolumeManager.SaveVolume(volume, volumePath);
        }
    }
}