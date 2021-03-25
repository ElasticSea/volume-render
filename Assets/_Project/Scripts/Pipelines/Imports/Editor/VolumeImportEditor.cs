using System;
using System.Diagnostics;
using System.IO;
using MathNet.Numerics.RootFinding;
using Pipelines;
using Pipelines.Imports;
using UnityEditor;
using UnityEngine;

namespace Scripts.Pipelines.Imports.Editor
{
    [CustomEditor(typeof(VolumeImport))]
    public class VolumeImportEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var path = serializedObject.FindProperty("sourcePath").stringValue;
            var volumeName = serializedObject.FindProperty("volumeName").stringValue;
            var channelDepth = ((ChannelDepth)serializedObject.FindProperty("channelDepth").enumValueIndex);

            try
            {

                var header = NiftiImport.ReadHeader(path);
            
                var bounds = new VolumeBounds(0, 0, 0, header.Dimensions[0], header.Dimensions[1], header.Dimensions[2]);
                var fit = GetValidBounds(bounds, channelDepth);

            
                EditorGUILayout.LabelField("Original Dimensions", $"{bounds.Width} x {bounds.Height} x {bounds.Depth}");
                
                var cropped = fit.Width != bounds.Width || fit.Height != bounds.Height || fit.Depth != bounds.Depth;
                if (cropped)
                {
                    EditorGUILayout.HelpBox("Volume is too large and will be cropped. Volume has to be under 2048x2048x2048 and 2GB", MessageType.Error, true);
                    EditorGUILayout.LabelField("New Dimensions", $"{fit.Width} x {fit.Height} x {fit.Depth}");
                }

                if (GUILayout.Button("Save"))
                {
                    var sw = Stopwatch.StartNew();
                    var rawVolume = NiftiImport.ReadVolume(path);
                    sw = Tag("Read original volume", sw);

                    if (cropped)
                    {
                        rawVolume = rawVolume.Subvolume(bounds);
                        sw = Tag("Crop volume", sw);
                    }

                    var (normalized, min, max) = rawVolume.Data.Normalize();
                    sw = Tag("Normalize volume", sw);
                    var packed = normalized.Pack(channelDepth);
                    sw = Tag("Pack volume", sw);
        
                    var newVolume =  new Volume(rawVolume.Width, rawVolume.Height, rawVolume.Depth, min, max, channelDepth.GetBitsSize(), packed);
                    var to = Path.Combine(new DirectoryInfo(path).Parent.FullName, volumeName +".vlm");
                    WriteVolume(to, newVolume);
                    sw = Tag("Save volume", sw);
                }
            }
            catch (Exception e)
            {
                
                EditorGUILayout.HelpBox(e.Message, MessageType.Error, true);
            }
        }
        
        public static void WriteVolume(string path, Volume volume)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Write(stream, BitConverter.GetBytes(volume.Width));
                Write(stream, BitConverter.GetBytes(volume.Height));
                Write(stream, BitConverter.GetBytes(volume.Depth));
                Write(stream, BitConverter.GetBytes(volume.Min));
                Write(stream, BitConverter.GetBytes(volume.Max));
                Write(stream, BitConverter.GetBytes(volume.ChannelDepthBits));

                var volumeData = volume.Data;
                var chunks = volumeData.Data;
                var bytesToWrite = volumeData.Length;
                for (var i = 0; i < chunks.Length; i++)
                {
                    var writeLength = Math.Min((int) bytesToWrite, chunks[i].Length);
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
        
        private Stopwatch Tag(string message, Stopwatch sw)
        {
            UnityEngine.Debug.Log($"{message} took {sw.ElapsedMilliseconds}ms");
            return Stopwatch.StartNew();
        }
        
        private VolumeBounds GetValidBounds(VolumeBounds bounds, ChannelDepth depth)
        {
            var bytesPerPixel = depth.GetBitsSize() / 8f;

            bounds.Width = Mathf.Min(bounds.Width, 2048);
            bounds.Height = Mathf.Min(bounds.Height, 2048);
            bounds.Depth = Mathf.Min(bounds.Depth, 2048);
        
            var totalBytes = (long)bounds.Width * bounds.Height * bounds.Depth * bytesPerPixel;

            var textureSizeLimit = 2146435071;
            if (totalBytes > textureSizeLimit )
            {
                // 2146435071 is maximum index of non byte array
                var maxVolume = (long)((double)textureSizeLimit / bytesPerPixel);
                var offset = CalculateOffset(bounds.Width, bounds.Height, bounds.Depth, maxVolume);
                bounds.X += offset;
                bounds.Y += offset;
                bounds.Z += offset;
                bounds.Width -= offset*2;
                bounds.Height -= offset*2;
                bounds.Depth -= offset*2;
            }

            return bounds;
        }
    
        private static int CalculateOffset(int width, int height, int depth, long volume)
        {
            // Solve volume = (width - 2*offset) * (height - 2*offset) * (depth - 2*offset)

            var w = (double) width;
            var h = (double) height;
            var d = (double) depth;
            var p3 = -1; // -x^3
            var p2 = d + w + h; // x^2d + wx^2 + hx^2
            var p1 = -w * d - h * d - w * h; // whd -wdx - hdx - whx
            var p0 = w * h * d - volume; // whd - volume
            var (r0, r1, r2) = Cubic.RealRoots(p0 / p3, p1 / p3, p2 / p3); // normalize p3
            return (int) Math.Ceiling(Math.Max((float) r0 / 2, 0)); // offset is half
        }
    }
}