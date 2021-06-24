using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Volumes
{
    public static class VolumeExtensions
    {
        public static RawVolume<T> Crop<T>(this RawVolume<T> data, VolumeBounds bounds, bool multithreaded = true)
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

        private static RawVolume<T> SubvolumeSt<T>(this RawVolume<T> source, VolumeBounds bounds)
        {
            var result = new BigArray<T>((long) bounds.Width * bounds.Height * bounds.Depth);

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

            return new RawVolume<T>(bounds.Width, bounds.Height, bounds.Depth, result);
        }

        private static RawVolume<T> SubvolumeMt<T>(this RawVolume<T> data, VolumeBounds bounds)
        {
            var chunkSize = 128;

            var result = new BigArray<T>((long) bounds.Width * bounds.Height * bounds.Depth);

            var jobs = new List<(int x0, int y0, int z0, int x1, int y1, int z1, BigArray<T> source, BigArray<T> target)>();
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
            
            return new RawVolume<T>(bounds.Width, bounds.Height, bounds.Depth, result);
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
            var chunkSize = data.Length / Environment.ProcessorCount;

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

        private interface IBytePackerStrategy<in T>
        {
            int Size { get; }
            void Write(T value, BigArray<byte> destination, long destinationIndex);
        }

        private class FloatPackerStrategy : IBytePackerStrategy<float>
        {
            private int bits;

            public FloatPackerStrategy(int bits)
            {
                this.bits = bits;
            }

            public int Size => bits / 8;

            public void Write(float value, BigArray<byte> destination, long destinationIndex)
            {
                var integer = (ulong) (((1 << (bits)) - 1) * value);

                var bytes = Size;
                for (var i = 0; i < bytes; i++)
                {
                    destination[destinationIndex + i] = (byte)((integer >> (i * 8)) & 255);
                }
            }
        }

        private class ColorPacker : IBytePackerStrategy<Color>
        {
            public int Size => 4;

            public void Write(Color value, BigArray<byte> destination, long destinationIndex)
            {
                destination[destinationIndex + 0] = (byte)(value.r * 255);
                destination[destinationIndex + 1] = (byte)(value.g * 255);
                destination[destinationIndex + 2] = (byte)(value.b * 255);
                destination[destinationIndex + 3] = (byte)(value.a * 255);
            }
        }

        public static BigArray<byte> Pack<T>(this BigArray<T> data, VolumeFormat channelDepth, bool multithreaded = true)
        {
            var strategy = GetPackerStrategy<T>(channelDepth);

            if (multithreaded)
            {
                return data.PackMt(strategy);
            }
            else
            {
                return data.PackSt(strategy);
            }
        }

        private static IBytePackerStrategy<T> GetPackerStrategy<T>(VolumeFormat depth)
        {
            switch (depth)
            {
                case VolumeFormat.Gray32:
                    return new FloatPackerStrategy(32) as IBytePackerStrategy<T>;
                case VolumeFormat.Gray16:
                    return new FloatPackerStrategy(16) as IBytePackerStrategy<T>;
                case VolumeFormat.Gray8:
                    return new FloatPackerStrategy(8) as IBytePackerStrategy<T>;
                // case ChannelDepth.RGBA64:
                //     break;
                case VolumeFormat.RGBA32:
                    return new ColorPacker() as IBytePackerStrategy<T>;
                // case ChannelDepth.RGBA16:
                //     break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
            }
        }

        private static BigArray<byte> PackSt<T>(this BigArray<T> data, IBytePackerStrategy<T> strategy)
        {
            var valueSize = strategy.Size;
            var newData = new BigArray<byte>((long) data.Length * valueSize);
            
            for (long i = 0; i < data.Length; i++)
            {
                strategy.Write(data[i], newData, i * valueSize);
            }
            
            return newData;
        }

        private static BigArray<byte> PackMt<T>(this BigArray<T> data, IBytePackerStrategy<T> strategy)
        {
            var chunkSize = data.Length / Environment.ProcessorCount;

            var jobs = Enumerable
                .Range(0, (int) (Math.Ceiling((float)data.Length / chunkSize)))
                .Select(i =>
                {
                    var startIndex = (long) i * chunkSize;
                    var endIndex = Math.Min(((long) i + 1) * chunkSize, data.Length);
                    return (startIndex, endIndex);
                });

            var valueSize = strategy.Size;
            var jobs2 = jobs.Select(i => (i.startIndex, i.endIndex));

            var newData = new BigArray<byte>((long) data.Length * valueSize);
                
            Parallel.ForEach(jobs2, (tuple, state) =>
            {
                var (startIndex, endIndex) = tuple;

                for (long i = startIndex; i < endIndex; i++)
                {
                    strategy.Write(data[i], newData, i * valueSize);
                }
            });
            
            return newData;
        }

        public static UnpackedVolumeCluster<T>[,,] ToOctClusters<T>(this BigArray<T> data, Vector3Int size, bool multithreaded = true)
        {
            var maxChunkSize = new Vector3Int(size.x / 2, size.y / 2, size.z / 2);
            var clusters = data.ToClusters(size, maxChunkSize, false, multithreaded);
            
            var oldClusters = clusters;
            clusters = new UnpackedVolumeCluster<T>[2,2,2];
            clusters[0, 0, 0] = oldClusters[0, 0, 0];
            clusters[0, 0, 1] = oldClusters[0, 0, 1];
            clusters[0, 1, 0] = oldClusters[0, 1, 0];
            clusters[0, 1, 1] = oldClusters[0, 1, 1];
            clusters[1, 0, 0] = oldClusters[1, 0, 0];
            clusters[1, 0, 1] = oldClusters[1, 0, 1];
            clusters[1, 1, 0] = oldClusters[1, 1, 0];
            clusters[1, 1, 1] = oldClusters[1, 1, 1];

            return clusters;
        }

        public static UnpackedVolumeCluster<T>[,,] ToClusters<T>(this BigArray<T> data, Vector3Int size, Vector3Int maxChunkSize, bool padding, bool multithreaded = true)
        {
            if (multithreaded)
            {
                return data.ToClustersMt(size, maxChunkSize, padding);
            }
            else
            {
                return data.ToClustersSt(size, maxChunkSize, padding);
            }
        }
        
        public static VolumeCluster[,,] PackClusters<T>(this UnpackedVolumeCluster<T>[,,] clusters, VolumeFormat channelDepth, bool multithreaded)
        {
            var width = clusters.GetLength(0);
            var height = clusters.GetLength(1);
            var depth = clusters.GetLength(2);

            var packed = new VolumeCluster[width,height,depth];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var z = 0; z <depth; z++)
                    {
                        var c = clusters[x, y, z];
                        packed[x,y,z] = new VolumeCluster
                        {
                            Width = c.Width,
                            Height = c.Height,
                            Depth = c.Depth,
                            X = c.X,
                            Y = c.Y,
                            Z = c.Z,
                            Data = c.Data.Pack(channelDepth, multithreaded)
                        };
                    }
                }
            }

            return packed;
        }
        
        private static UnpackedVolumeCluster<T>[,,] ToClustersMt<T>(this BigArray<T> data, Vector3Int size, Vector3Int maxChunkSize, bool padding)
        {
            var clusters = InitClusters<T>(size, maxChunkSize, padding);
            
            var chunkSize = data.Length / Environment.ProcessorCount;
            var jobs = new List<(long min, long max)>();

            for (var i = 0L; i < data.Length; i += chunkSize)
            {
                var min = i;
                var max = (long) Mathf.Min(data.Length, i + chunkSize);
                jobs.Add((min, max));
            }
            
            Parallel.ForEach(jobs, (tuple, state) =>
            {
                var ( min, max) = tuple;

                for (var i = min; i < max; i++)
                {
                    var rest = i;
                    var sizeXy = size.x * size.y;
                    var z = rest / sizeXy;
                    rest -= z * sizeXy;
                    var y = rest / size.x;
                    rest -= y * size.x;
                    var x = rest;

                    var cx = x / maxChunkSize.x;
                    var cy = y / maxChunkSize.y;
                    var cz = z / maxChunkSize.z;

                    var cluster = clusters[cx, cy, cz];

                    var ccx = x - (cx * maxChunkSize.x);
                    var ccy = y - (cy * maxChunkSize.y);
                    var ccz = z - (cz * maxChunkSize.z);

                    var clusterIndex = ccx + ccy * cluster.Width + ccz * cluster.Width * cluster.Height;

                    cluster.Data[clusterIndex] = data[i];
                }
            });

            return clusters;
        }

        private static UnpackedVolumeCluster<T>[,,] InitClusters<T>(Vector3Int size, Vector3Int maxChunkSize, bool padding)
        {
            var clustersX = Mathf.CeilToInt((float) size.x / maxChunkSize.x);
            var clustersY = Mathf.CeilToInt((float) size.y / maxChunkSize.y);
            var clustersZ = Mathf.CeilToInt((float) size.z / maxChunkSize.z);
            var clusters = new UnpackedVolumeCluster<T>[clustersX, clustersY, clustersZ];
            for (var z = 0; z < clustersZ; z++)
            {
                for (var y = 0; y < clustersY; y++)
                {
                    for (var x = 0; x < clustersX; x++)
                    {
                        var chunkX = x ;
                        var chunkY = y;
                        var chunkZ = z;
                        var chunkWidth = maxChunkSize.x;
                        var chunkHeight = maxChunkSize.y;
                        var chunkDepth = maxChunkSize.z;
                        if (padding == false)
                        {
                            chunkWidth = Mathf.Min(size.x - chunkX * maxChunkSize.x, maxChunkSize.x);
                            chunkHeight = Mathf.Min(size.y - chunkY * maxChunkSize.y, maxChunkSize.y);
                            chunkDepth = Mathf.Min(size.z - chunkZ * maxChunkSize.z, maxChunkSize.z);
                        }
                        
                        clusters[x, y, z] = new UnpackedVolumeCluster<T>
                        {
                            X = chunkX,
                            Y = chunkY,
                            Z = chunkZ,
                            Width = chunkWidth,
                            Height = chunkHeight,
                            Depth = chunkDepth,
                            Data = new BigArray<T>(chunkWidth * chunkHeight * chunkDepth)
                        };
                    }
                }
            }

            return clusters;
        }
        
        private static UnpackedVolumeCluster<T>[,,] ToClustersSt<T>(this BigArray<T> data, Vector3Int size, Vector3Int maxChunkSize, bool padding)
        {
            var clusters = InitClusters<T>(size, maxChunkSize, padding);
            
            for (long i = 0; i < data.Length; i++)
            {
                var rest = i;
                var sizeXy = size.x * size.y;
                var z = rest / sizeXy;
                rest -= z * sizeXy;
                var y = rest / size.x;
                rest -= y * size.x;
                var x = rest;

                var cx = x / maxChunkSize.x;
                var cy = y / maxChunkSize.y;
                var cz = z / maxChunkSize.z;

                var cluster = clusters[cx, cy, cz];


                var ccx = x - (cx * maxChunkSize.x);
                var ccy = y - (cy * maxChunkSize.y);
                var ccz = z - (cz * maxChunkSize.z);

                var clusterIndex = ccx + ccy * cluster.Width + ccz * cluster.Width * cluster.Height;

                cluster.Data[clusterIndex] = data[i];
            }

            return clusters;
        }
    }
}