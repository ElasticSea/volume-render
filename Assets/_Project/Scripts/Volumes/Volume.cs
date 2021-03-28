using System;
using System.IO;
using ElasticSea.Framework.Util;
using UnityEngine;

namespace Volumes
{
    public class Volume
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public float Min { get; }
        public float Max { get; }
        public int ChannelDepthBits { get; }
        public BigArray<byte> Data { get; }

        public Volume(int width, int height, int depth, float min, float max, int channelDepthBits, BigArray<byte> data)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Min = min;
            Max = max;
            ChannelDepthBits = channelDepthBits;
            Data = data;
        }

        public Texture3D ToTexture()
        {
            var texture = new Texture3D(Width, Height, Depth, TextureFormat.R8, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.SetPixelData(Flaten(Data), 0);
            texture.Apply();
            return texture;
        }

        private byte[] Flaten(BigArray<byte> bigArray)
        {
            if (bigArray.Length > 2147483591)
            {
                throw new ArgumentException("BigArray is too big to fit into regular array");
            }
            
            var output = new byte[bigArray.Length];
            var chunks = bigArray.Data;
            var offset = 0;
            for (var i = 0; i < chunks.Length; i++)
            {
                var chunkLength = chunks[i].Length;
                Array.Copy(chunks[i], 0, output, offset, chunkLength);
                offset += chunkLength;
            }

            return output;
        }
    }
}