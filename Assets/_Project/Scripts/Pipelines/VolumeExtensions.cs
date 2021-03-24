using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Pipelines
{
    public static class VolumeExtensions
    {
        public static RawVolume Subvolume(this RawVolume data, VolumeBounds bounds, bool multithreaded = true)
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
            
            var jobs = new List<(int x, int y, int z,int w, int h, int d, BigArray<float> source, BigArray<float> target)>();
            for (int k = 0; k < bounds.Depth; k += chunkSize)
            {
                for (int j = 0; j < bounds.Height; j += chunkSize)
                {
                    for (int i = 0; i < bounds.Width; i += chunkSize)
                    {
                        var jx = i + bounds.X;
                        var jy = j + bounds.Y;
                        var jz = k + bounds.Z;
                        var jw = Math.Min(bounds.X + bounds.Width, jx + chunkSize);
                        var jh = Math.Min(bounds.Y + bounds.Height, jy + chunkSize);
                        var jd = Math.Min(bounds.Z + bounds.Depth, jz + chunkSize);
                        jobs.Add((jx, jy, jz, jw, jh, jd, data.Data, result));
                    }
                }
            }
            
            Parallel.ForEach(jobs, (tuple, state) =>
            {
                var ( x, y, z, w, h, d, source, target) = tuple;

                for (long k = z; k < d; k++)
                {
                    for (long j = y; j < h; j++)
                    {
                        for (long i = x; i < w; i++)
                        {
                            long targetIndex = (i - x) +
                                               (j - y) * bounds.Width +
                                               (k - z) * bounds.Width * bounds.Height;
                            
                            long sourceIndex = i +
                                               j * data.Width +
                                               k * data.Width * data.Height;

                            if (targetIndex >= target.Length || sourceIndex >= source.Length ||
                                targetIndex < 0 || sourceIndex < 0)
                            {
                                int d3 = 0;
                            }
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
            var bytes = channelDepth.GetByteSize();

            var newData = new BigArray<byte>((long) data.Length * bytes);

            for (long i = 0; i < data.Length; i++)
            {
                WriteNormalized(data[i], bytes, newData, i);
            }
            
            return newData;
        }

        private static void WriteNormalized(double t, int bytes, BigArray<byte> destination, long destinationIndex)
        {
            var integer = (ulong) (((1 << (bytes * 8)) - 1) * t);
                
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

            var bytes = channelDepth.GetByteSize();
            var jobs2 = jobs.Select(i => (i.startIndex, i.endIndex));

            var newData = new BigArray<byte>((long) data.Length * bytes);
                
            Parallel.ForEach(jobs2, (tuple, state) =>
            {
                var (startIndex, endIndex) = tuple;

                for (long i = startIndex; i < endIndex; i++)
                {
                    WriteNormalized(data[i], bytes, newData, i);
                }
            });
            
            return newData;
        }
    }
}