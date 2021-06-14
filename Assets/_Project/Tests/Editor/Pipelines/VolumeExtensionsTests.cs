using System;
using System.Linq;
using NUnit.Framework;
using Volumes;

namespace Tests.Editor.Pipelines
{
    public static class VolumeExtensionsTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public static void Subvolume1(bool multithreaded)
        {
            var data = new BigArray<float>(1)
            {
                [0] = 42
            };

            var expected = new RawVolume(1, 1, 1, data);
            var actual = expected.Crop(new VolumeBounds(0, 0, 0, 1, 1, 1), multithreaded);

            Assert.AreEqual(42, actual.Data[0]);
        }

        [TestCase(true)]
        [TestCase(false)]
        public static void Subvolume3(bool multithreaded)
        {
            var i = 0;
            var data = new BigArray<float>(3 * 3 * 3)
            {
                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,

                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 1, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,

                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,
            };

            var expected = new RawVolume(3, 3, 3, data);
            var actual = expected.Crop(new VolumeBounds(1, 1, 1, 1, 1, 1), multithreaded);

            Assert.AreEqual(1, actual.Data[0]);
        }

        [TestCase(true)]
        [TestCase(false)]
        public static void SubvolumeOffset1(bool multithreaded)
        {
            var i = 0;
            var data = new BigArray<float>(3 * 3 * 3)
            {
                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,

                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 1, [i++] = 2,
                [i++] = 0, [i++] = 3, [i++] = 4,

                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 5, [i++] = 6,
                [i++] = 0, [i++] = 7, [i++] = 8,
            };

            var actual = new RawVolume(3, 3, 3, data).Crop(new VolumeBounds(1, 1, 1, 2, 2, 2), multithreaded);

            i = 0;
            var expected = new BigArray<float>(2 * 2 * 2)
            {
                [i++] = 1, [i++] = 2,
                [i++] = 3, [i++] = 4,
               
                [i++] = 5, [i++] = 6,
                [i++] = 7, [i++] = 8,
            };
            
            AssertArray(expected, actual.Data);
        }
        
        [TestCase()]
        public static void ToClusters_bytePerCluster()
        {
            var i = 0;
            var data = new BigArray<byte>(3 * 3 * 3)
            {
                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 0, [i++] = 0,

                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 1, [i++] = 2,
                [i++] = 0, [i++] = 3, [i++] = 4,

                [i++] = 0, [i++] = 0, [i++] = 0,
                [i++] = 0, [i++] = 5, [i++] = 6,
                [i++] = 0, [i++] = 7, [i++] = 8,
            };

            var clusters = data.ToClusters(3, 3, 3, 1, false, false);
            
            Assert.AreEqual(27, clusters.Length);
        }
        
        [TestCase()]
        public static void ToClusters_2BytePerCluster()
        {
            var i = 0;
            var data = new BigArray<byte>(4 * 4 * 4)
            {
                [i++] = 1, [i++] = 2, [i++] = 1, [i++] = 2, 
                [i++] = 3, [i++] = 4, [i++] = 3, [i++] = 4, 
                [i++] = 1, [i++] = 2, [i++] = 1, [i++] = 2, 
                [i++] = 3, [i++] = 4, [i++] = 3, [i++] = 4, 

                [i++] = 5, [i++] = 6, [i++] = 5, [i++] = 6,
                [i++] = 7, [i++] = 8, [i++] = 7, [i++] = 8,
                [i++] = 5, [i++] = 6, [i++] = 5, [i++] = 6,
                [i++] = 7, [i++] = 8, [i++] = 7, [i++] = 8,
                
                [i++] = 1, [i++] = 2, [i++] = 1, [i++] = 2, 
                [i++] = 3, [i++] = 4, [i++] = 3, [i++] = 4, 
                [i++] = 1, [i++] = 2, [i++] = 1, [i++] = 2, 
                [i++] = 3, [i++] = 4, [i++] = 3, [i++] = 4, 

                [i++] = 5, [i++] = 6, [i++] = 5, [i++] = 6,
                [i++] = 7, [i++] = 8, [i++] = 7, [i++] = 8,
                [i++] = 5, [i++] = 6, [i++] = 5, [i++] = 6,
                [i++] = 7, [i++] = 8, [i++] = 7, [i++] = 8,
            };

            var clusters = data.ToClusters(4, 4, 4, 2, false, false);
            
            Assert.AreEqual(8, clusters.Length);
            foreach (var cluster in clusters)
            {
                AssertArray(new[] {1f, 2, 3, 4, 5, 6, 7, 8}, cluster.Data.ToArray().Select(b => (float) b).ToArray());
            }
        }
        
        [TestCase()]
        public static void ToClusters_DifferentClusterSizes()
        {
            var i = 0;
            var data = new BigArray<byte>(3 * 3 * 3)
            {
                //x*y z=0
                [i++] = 1, [i++] = 2, [i++] = 1,
                [i++] = 3, [i++] = 4, [i++] = 3,
                [i++] = 1, [i++] = 2, [i++] = 1,

                //x*y z=1
                [i++] = 5, [i++] = 6, [i++] = 5,
                [i++] = 7, [i++] = 8, [i++] = 7,
                [i++] = 5, [i++] = 6, [i++] = 5,

                //x*y z=2
                [i++] = 1, [i++] = 2, [i++] = 1,
                [i++] = 3, [i++] = 4, [i++] = 3,
                [i++] = 1, [i++] = 2, [i++] = 1,
            };

            var clusters = data.ToClusters(3, 3, 3, 2, false, false).Cast<UnpackedVolumeCluster<byte>>().ToArray();
            
            Assert.AreEqual(8, clusters.Length);
            Assert.AreEqual(8, clusters[0].Data.Length);
            Assert.AreEqual(4, clusters[1].Data.Length);
            Assert.AreEqual(4, clusters[2].Data.Length);
            Assert.AreEqual(2, clusters[3].Data.Length);
            Assert.AreEqual(4, clusters[4].Data.Length);
            Assert.AreEqual(2, clusters[5].Data.Length);
            Assert.AreEqual(2, clusters[6].Data.Length);
            Assert.AreEqual(1, clusters[7].Data.Length);
        }
        
        [TestCase()]
        public static void ToClusters_DifferentClusterSizesPadding()
        {
            var i = 0;
            var data = new BigArray<byte>(3 * 3 * 3)
            {
                //x*y z=0
                [i++] = 1, [i++] = 2, [i++] = 1,
                [i++] = 3, [i++] = 4, [i++] = 3,
                [i++] = 1, [i++] = 2, [i++] = 1,

                //x*y z=1
                [i++] = 5, [i++] = 6, [i++] = 5,
                [i++] = 7, [i++] = 8, [i++] = 7,
                [i++] = 5, [i++] = 6, [i++] = 5,

                //x*y z=2
                [i++] = 1, [i++] = 2, [i++] = 1,
                [i++] = 3, [i++] = 4, [i++] = 3,
                [i++] = 1, [i++] = 2, [i++] = 1,
            };

            var clusters = data.ToClusters(3, 3, 3, 2, true, false).Cast<UnpackedVolumeCluster<byte>>().ToArray();
            
            Assert.AreEqual(8, clusters.Length);
            Assert.AreEqual(8, clusters[0].Data.Length);
            Assert.AreEqual(8, clusters[1].Data.Length);
            Assert.AreEqual(8, clusters[2].Data.Length);
            Assert.AreEqual(8, clusters[3].Data.Length);
            Assert.AreEqual(8, clusters[4].Data.Length);
            Assert.AreEqual(8, clusters[5].Data.Length);
            Assert.AreEqual(8, clusters[6].Data.Length);
            Assert.AreEqual(8, clusters[7].Data.Length);
            
            AssertArray(new[] {1f, 2, 3, 4, 5, 6, 7, 8}, clusters[0].Data.ToArray().Select(b => (float) b).ToArray());
        }

        private static void AssertArray(BigArray<float> expected, BigArray<float> actual)
        {
            AssertArray(expected?.ToArray(), actual?.ToArray());
        }
        

        private static void AssertArray(float[] expected, float[] actual)
        {
            if (expected.Length != actual.Length)
            {
                throw new ArgumentException($"Length [{expected.Length}] is different than length [{actual.Length}]");
            }

            for (var i = 0L; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                {
                    throw new ArgumentException($"Arrays differ at index [{i}]");
                }
            }
        }
    }
}