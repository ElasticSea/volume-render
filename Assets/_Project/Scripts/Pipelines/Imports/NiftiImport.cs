using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nifti.NET;

namespace Pipelines.Imports
{
    public class NiftiImport
    {
        public static Nifti.NET.Nifti ReadHeader(string path)
        {
            var stream = File.OpenRead(path);

            var result = new Nifti.NET.Nifti();
            var header = NiftiFile.ReadHeaderFromStream(stream);
            result.Header = header;

            return result;
        }
        
        public static RawVolume ReadVolume(string path)
        {
            var stream = File.OpenRead(path);

            var result = new Nifti.NET.Nifti();
            var header = NiftiFile.ReadHeaderFromStream(stream);
            result.Header = header;

            return ReadVolume(result, stream);
        }

        private static RawVolume ReadVolume(Nifti.NET.Nifti nifti, FileStream stream)
        {
            if (nifti.Header.datatype != NiftiHeader.DT_FLOAT32)
            {
                throw new ArgumentException("Only float volumes are supported.");
            }

            var bytelen = stream.Length - stream.Position;
            var data = new BigArray<float>(bytelen / sizeof(float));
            var littleEndian = nifti.Header.SourceIsBigEndian() == false;
            RealFloats(data, stream, littleEndian);

            var width = nifti.Dimensions[0];
            var height = nifti.Dimensions[1];
            var depth = nifti.Dimensions[2];
            return new RawVolume(width, height, depth, data);
        }

        private static void ReadFloatsSt(BigArray<float> data, Stream stream, bool littleEndian)
        {
            var buffer = new byte[1024 * 1024];

            long i = 0;
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                var valueCount = read / sizeof(float);
                for (var j = 0; j < valueCount; j++)
                {
                    if (BitConverter.IsLittleEndian != littleEndian)
                    {
                        // Swap endianess in place 
                        var tmp = buffer[j];
                        buffer[j] = buffer[j + 3];
                        buffer[j + 3] = tmp;
                        tmp = buffer[j + 1];
                        buffer[j + 1] = buffer[j + 2];
                        buffer[j + 2] = tmp;
                    }

                    var value = BitConverter.ToSingle(buffer, j * sizeof(float));
                    data[i++] = value;
                }
            }
        }

        private static void RealFloats(BigArray<float> data, Stream stream, bool littleEndian, bool multithreaded = true)
        {
            var batches = Environment.ProcessorCount * 2;
            var bufferSize = batches * 1024 * 1024;
            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int readBytes;
            while ((readBytes = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                if (multithreaded)
                {
                    totalBytesRead = RealFloatsMt(readBytes, totalBytesRead, buffer, data, littleEndian);
                }
                else
                {
                    totalBytesRead = RealFloatsSt(readBytes, totalBytesRead, buffer, data, littleEndian);
                }
            }
        }

        private static long RealFloatsSt(int bytesToRead, long totalRead, byte[] buffer, BigArray<float> data, bool littleEndian)
        {
            for (var i = 0; i < bytesToRead; i += 4)
            {
                var floatIndex = totalRead / 4;
                var floatValue = ToSingle(buffer, i, littleEndian);
                data[floatIndex] = floatValue;
                totalRead += 4;
            }

            return totalRead;
        }

        private static long RealFloatsMt(int bytesToRead, long totalBytesRead, byte[] buffer, BigArray<float> data, bool littleEndian)
        {
            var chunkSize = 1024 * 1024;
            var jobs = new List<(int sourceIndex, long destinationIndex, int length)>();
            var sourceIndex = 0;
            while (bytesToRead > 0)
            {
                var currentBytesToRead = Math.Min(bytesToRead, chunkSize);
                jobs.Add((sourceIndex, totalBytesRead / 4, currentBytesToRead));
                bytesToRead -= currentBytesToRead;
                totalBytesRead += currentBytesToRead;
                sourceIndex += currentBytesToRead;
            }
            Parallel.ForEach(jobs, (tuple, state) =>
            {
                var (sourceIndex, destinationIndex, length) = tuple;
                for (int r = 0; r < length; r += 4)
                {
                    var floatValue = ToSingle(buffer, sourceIndex + r, littleEndian);
                    data[destinationIndex] = floatValue;
                    destinationIndex++;
                }
            });
            return totalBytesRead;
        }

        private static float ToSingle(byte[] value, int index, bool littleEndian)
        {
            if (BitConverter.IsLittleEndian != littleEndian)
            {
                // Swap endianess in place 
                var tmp = value[index];
                value[index] = value[index + 3];
                value[index + 3] = tmp;
                tmp = value[index + 1];
                value[index + 1] = value[index + 2];
                value[index + 2] = tmp;
            }

            return BitConverter.ToSingle(value, index);
        }
    }
}