using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Volumes
{
    public static class VolumeExtensions
    {
        public static RawVolume Crop(this RawVolume data, VolumeBounds bounds, bool multithreaded = true)
        {
            if (multithreaded)
            {
                return data.SubvolumeMt(bounds);
            }
            else
            {
                return data.SubvolumeSt(bounds);
            }
        }

        private static RawVolume SubvolumeSt(this RawVolume source, VolumeBounds bounds)
        {
            var result = new BigArray<float>((long) bounds.Width * bounds.Height * bounds.Depth);

            for (long k = 0; k < bounds.Depth; k++)
            {
                for (long j = 0; j < bounds.Height; j++)
                {
                    for (long i = 0; i < bounds.Width; i++)
                    {
                        var resultIndex = i +
                                          j * bounds.Width +
                                          k * bounds.Width * bounds.Height;

                        var sourceIndex = (i + bounds.X) +
                                          (j + bounds.Y) * source.Width +
                                          (k + bounds.Z) * source.Width * source.Height;

                        result[resultIndex] = source.Data[sourceIndex];
                    }
                }
            }

            return new RawVolume(bounds.Width, bounds.Height, bounds.Depth, result);
        }

        private static RawVolume SubvolumeMt(this RawVolume data, VolumeBounds bounds)
        {
            var chunkSize = 128;

            var result = new BigArray<float>((long) bounds.Width * bounds.Height * bounds.Depth);

            var jobs = new List<(int x0, int y0, int z0, int x1, int y1, int z1, BigArray<float> source, BigArray<float> target)>();
            for (int z = bounds.Z; z < bounds.Z + bounds.Depth; z += chunkSize)
            {
                for (int y = bounds.Y; y < bounds.Y + bounds.Height; y += chunkSize)
                {
                    for (int x = bounds.X; x < bounds.X + bounds.Width; x += chunkSize)
                    {
                        var x1 = Math.Min(data.Width, x + chunkSize);
                        var y1 = Math.Min(data.Height, y + chunkSize);
                        var z1 = Math.Min(data.Depth, z + chunkSize);
                        jobs.Add((x, y, z, x1, y1, z1, data.Data, result));
                    }
                }
            }
            
            Parallel.ForEach(jobs, (tuple, state) =>
            {
                var ( x0, y0, z0, x1, y1, z1, source, target) = tuple;

                for (long z = z0; z < z1; z++)
                {
                    for (long y = y0; y < y1; y++)
                    {
                        for (long x = x0; x < x1; x++)
                        {
                            long targetIndex = (x - bounds.X) +
                                               (y - bounds.Y) * bounds.Width +
                                               (z - bounds.Z) * bounds.Width * bounds.Height;
                            
                            long sourceIndex = x +
                                               y * data.Width +
                                               z * data.Width * data.Height;

                            target[targetIndex] = source[sourceIndex];
                        }
                    }
                }
            });
            
            return new RawVolume(bounds.Width, bounds.Height, bounds.Depth, result);
        }
        
        public static  (BigArray<float> volume, float min, float max) Normalize(this BigArray<float> data, bool multithreaded = true)
        {
            if (multithreaded)
            {
                return data.NormalizeMt();
            }
            else
            {
                return data.NormalizeSt();
            }
        }

        private static  (BigArray<float> volume, float min, float max) NormalizeSt(this BigArray<float> data)
        {
            var min = float.MaxValue;
            var max = float.MinValue;

            for (long i = 0; i < data.Length; i++)
            {
                var f = data[i];
                min = Math.Min(min, f);
                max = Math.Max(max, f);
            }

            var result = new BigArray<float>(data.Length);

            for (long i = 0; i < data.Length; i++)
            {
                result[i] = (float) (((double)data[i] - min) / (max - min));
            }
            
            return (result, min, max);
        }

        private static (BigArray<float> volume, float min, float max) NormalizeMt(this BigArray<float> data)
        {
            var chunkSize = 1024 * 1024;

            var jobs = Enumerable
                .Range(0, (int) (Math.Ceiling((float)data.Length / chunkSize)))
                .Select(i =>
                {
                    var startIndex = (long) i * chunkSize;
                    var endIndex = Math.Min(((long) i + 1) * chunkSize, data.Length);
                    return (startIndex, endIndex);
                });

            var minMaxes = jobs.AsParallel().Select(tuple =>
            {
                var (startIndex, endIndex) = tuple;

                var min = float.MaxValue;
                var max = float.MinValue;

                for (long i = startIndex; i < endIndex; i++)
                {
                    var f = data[i];
                    min = Mathf.Min(min, f);
                    max = Mathf.Max(max, f);
                }

                return (min, max);
            }).ToArray();

            var globalMin = float.MaxValue;
            var globalMax = float.MinValue;

            foreach (var (min, max) in minMaxes)
            {
                globalMin = Mathf.Min(globalMin, min);
                globalMax = Mathf.Max(globalMax, max);
            }

            var jobs2 = jobs
                .Select(i => (i.startIndex, i.endIndex, globalMin, globalMax));

            var result = new BigArray<float>(data.Length);
                
            Parallel.ForEach(jobs2, (tuple, state) =>
            {
                var (startIndex, endIndex, min, max) = tuple;

                for (long i = startIndex; i < endIndex; i++)
                {
                    result[i] = (float) (((double)data[i] - min) / (max - min));
                }
            });
            
            return (result, globalMin, globalMax);
        }
        
           public static BigArray<byte> Pack(this BigArray<float> data, ChannelDepth channelDepth, bool multithreaded = true)
        {
            if (channelDepth.GetBitsSize() % 8 != 0)
            {
                throw new ArgumentException("Partial bytes are not supported");
            }
            
            if (multithreaded)
            {
                return data.PackMt(channelDepth);
            }
            else
            {
                return data.PackSt(channelDepth);
            }
        }

        private static BigArray<byte> PackSt(this BigArray<float> data, ChannelDepth channelDepth)
        {
            var bits = channelDepth.GetBitsSize();

            var newData = new BigArray<byte>((long) data.Length * bits / 8);

            for (long i = 0; i < data.Length; i++)
            {
                WriteNormalized(data[i], bits, newData, i);
            }
            
            return newData;
        }

        private static void WriteNormalized(double t, int bits, BigArray<byte> destination, long destinationIndex)
        {
            var integer = (ulong) (((1 << (bits)) - 1) * t);

            var bytes = bits / 8;
            for (var i = 0; i < bytes; i++)
            {
                destination[destinationIndex * bytes + i] = (byte)((integer >> (i * 8)) & 255);
            }
        }

        private static BigArray<byte> PackMt(this BigArray<float> data, ChannelDepth channelDepth)
        {
            var chunkSize = 1024 * 1024;

            var jobs = Enumerable
                .Range(0, (int) (Math.Ceiling((float)data.Length / chunkSize)))
                .Select(i =>
                {
                    var startIndex = (long) i * chunkSize;
                    var endIndex = Math.Min(((long) i + 1) * chunkSize, data.Length);
                    return (startIndex, endIndex);
                });

            var bits = channelDepth.GetBitsSize();
            var jobs2 = jobs.Select(i => (i.startIndex, i.endIndex));

            var newData = new BigArray<byte>((long) data.Length * bits / 8);
                
            Parallel.ForEach(jobs2, (tuple, state) =>
            {
                var (startIndex, endIndex) = tuple;

                for (long i = startIndex; i < endIndex; i++)
                {
                    WriteNormalized(data[i], bits, newData, i);
                }
            });
            
            return newData;
        }
    }
}