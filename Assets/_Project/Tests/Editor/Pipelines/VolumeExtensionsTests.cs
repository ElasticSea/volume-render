using System;
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

        private static void AssertArray(BigArray<float> expected, BigArray<float> actual)
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