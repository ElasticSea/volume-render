using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                return data.SumvolumeSt(bounds);
            }
        }

        private static RawVolume SumvolumeSt(this RawVolume source, VolumeBounds bounds)
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
                            var targetIndex = (i - x) +
                                              (j - y) * bounds.Width +
                                              (k - x) * bounds.Width * bounds.Height;
                            
                            var sourceIndex = i +
                                        j * data.Width +
                                        k * data.Width * data.Height;
                            
                            target[targetIndex] = source[sourceIndex];
                        }
                    }
                }
            });
            
            return new RawVolume(bounds.Width, bounds.Height, bounds.Depth, result);
        }
        
        public static Volume Normalize(this RawVolume data, int bits, bool multithreaded = true)
        {
            if (bits % 8 != 0)
            {
                throw new ArgumentException("Does not support partial bytes.");
            }
            
            if (multithreaded)
            {
                return data.NormalizeMT(bits);
            }
            else
            {
                return data.NormalizeST(bits);
            }
        }

        private static Volume NormalizeST(this RawVolume data, int bits)
        {
            var min = double.MaxValue;
            var max = double.MinValue;

            for (long i = 0; i < data.Data.Length; i++)
            {
                var f = data.Data[i];
                min = Math.Min(min, f);
                max = Math.Max(max, f);
            }

            var newData = new BigArray<byte>((long) data.Width * data.Height * data.Depth * bits / 8);

            for (long i = 0; i < data.Data.Length; i++)
            {
                var normalized = (data.Data[i] - min) / (max - min);

                WriteNormalized(normalized, bits, newData, i);
            }
            
            return new Volume(data.Width, data.Height, data.Depth, min, max, bits, newData);
        }

        private static void WriteNormalized(double t, int bits, BigArray<byte> destination, long destinationIndex)
        {
            var integer = (ulong) (((1 << bits) - 1) * t);
            var bytes = bits / 8;
                
            for (var i = 0; i < bytes; i++)
            {
                destination[destinationIndex * bytes + i] = (byte)((integer >> (i * 8)) & 255);
            }
        }

        private static Volume NormalizeMT(this RawVolume data, int bits)
        {
            var chunkSize = 1024 * 1024;

            var jobs = Enumerable
                .Range(0, (int) (Math.Ceiling((float)data.Data.Length / chunkSize)))
                .Select(i =>
                {
                    var startIndex = (long) i * chunkSize;
                    var endIndex = Math.Min(((long) i + 1) * chunkSize, data.Data.Length);
                    return (startIndex, endIndex);
                });

            var minMaxes = jobs.AsParallel().Select(tuple =>
            {
                var (startIndex, endIndex) = tuple;

                var min = double.MaxValue;
                var max = double.MinValue;

                for (long i = startIndex; i < endIndex; i++)
                {
                    var f = data.Data[i];
                    min = Math.Min(min, f);
                    max = Math.Max(max, f);
                }

                return (min, max);
            }).ToArray();

            var globalMin = double.MaxValue;
            var globalMax = double.MinValue;

            foreach (var (min, max) in minMaxes)
            {
                globalMin = Math.Min(globalMin, min);
                globalMax = Math.Max(globalMax, max);
            }

            var jobs2 = jobs
                .Select(i => (i.startIndex, i.endIndex, globalMin, globalMax));

            var newData = new BigArray<byte>((long) data.Width * data.Height * data.Depth * bits / 8);
                
            Parallel.ForEach(jobs2, (tuple, state) =>
            {
                var (startIndex, endIndex, min, max) = tuple;

                for (long i = startIndex; i < endIndex; i++)
                {
                    var normalized = (data.Data[i] - min) / (max - min);
                    WriteNormalized(normalized, bits, newData, i);
                }
            });
            
            return new Volume(data.Width, data.Height, data.Depth, globalMin, globalMax, bits, newData);
        }
    }
}