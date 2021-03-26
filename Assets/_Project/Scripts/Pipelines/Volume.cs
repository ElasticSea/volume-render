using System;
using System.IO;

namespace Pipelines
{
    public class Volume
    {
        private VolumeData data;

        public Volume(VolumeData data)
        {
            this.data = data;
        }

        public void Load(string path)
        {
            using (var reader = File.OpenRead(path))
            {
                var dimensionsBuffer = new byte[32];
                reader.Read(dimensionsBuffer, 0, dimensionsBuffer.Length);
                var width = BitConverter.ToInt32(dimensionsBuffer, 0);
                var height = BitConverter.ToInt32(dimensionsBuffer, 4);
                var depth = BitConverter.ToInt32(dimensionsBuffer, 8);
                var min = BitConverter.ToDouble(dimensionsBuffer, 12);
                var max = BitConverter.ToDouble(dimensionsBuffer, 20);
                var bits = BitConverter.ToInt32(dimensionsBuffer, 28);

                var dataArray = new BigArray<byte>((long) width * height * depth * bits/8);

                if (bits % 8 != 0)
                {
                    throw new InvalidOperationException("Does not support partial bytes.");
                }

                for (var i = 0; i < dataArray.Data.Length; i++)
                {
                    var chunk = dataArray.Data[i];
                    var read = reader.Read(chunk, 0, chunk.Length);
                    if (read == 0)
                    {
                        break;
                    }
                }
                
                data = new VolumeData(width, height, depth, min, max, bits, dataArray);
            }
        }

        public void Save(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Write(stream, BitConverter.GetBytes(data.Width));
                Write(stream, BitConverter.GetBytes(data.Height));
                Write(stream, BitConverter.GetBytes(data.Depth));
                Write(stream, BitConverter.GetBytes(data.Min));
                Write(stream, BitConverter.GetBytes(data.Max));
                Write(stream, BitConverter.GetBytes(data.ChannelDepthBits));

                var volumeData = data.Data;
                var chunks = volumeData.Data;
                var bytesToWrite = volumeData.Length;
                for (var i = 0; i < chunks.Length; i++)
                {
                    var writeLength = (int) Math.Min(bytesToWrite, chunks[i].Length);
                    Write(stream, chunks[i], writeLength);
                    bytesToWrite -= writeLength;
                }
            }
        }

        private static void Write(Stream stream, byte[] bytes)
        {
            Write(stream, bytes, bytes.Length);
        }
    
        private static void Write(Stream stream, byte[] bytes, int length)
        {
            stream.Write(bytes, 0, length);
        }

    }
}