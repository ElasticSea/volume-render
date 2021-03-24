using System;
using System.IO;
using Nifti.NET;

namespace Pipelines.Imports
{
    public class NiftiImport
    {
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
            ReadFloats(data, stream, littleEndian);

            var width = nifti.Dimensions[0];
            var height = nifti.Dimensions[1];
            var depth = nifti.Dimensions[2];
            return new RawVolume(width, height, depth, data);
        }

        private static void ReadFloats(BigArray<float> data, Stream stream, bool littleEndian)
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
    }
}