using System;
using System.Collections;
using System.IO;
using System.Linq;
using ElasticSea.Framework.Extensions;
using Render;
using UnityEngine;
using Volumes;

namespace Preview
{
    public class FlatPreview : MonoBehaviour
    {
        [SerializeField] private Camera camera;
        [SerializeField] private float distance = 1000;
        [SerializeField] private string volumePath;
        [Range(0, 1)]
        [SerializeField] private float cut = 0;
        [SerializeField] private int skipFrames = 0;

        [SerializeField] private VolumeRenderManager volumeRenderManager;
        
        private VolumeRender volumeRender;

        private void Awake()
        {
            var volumeSource = VolumeManager.GetVolume(volumePath);
            volumeRender = volumeRenderManager.LoadVolume(volumeSource);
            volumeRender.Alpha = 1f;
            volumeRender.StepDistance = 0.00025f;
            volumeRender.AlphaThreshold = 1f;
            volumeRender.transform.rotation = Quaternion.Euler(90, 0, -90);

            var bounds = volumeRender.GetComponent<Renderer>().bounds;
            var height = bounds.size.y;

            var fov = 2 * Mathf.Tan((height / 2) / distance) * Mathf.Rad2Deg;
            camera.fieldOfView = fov;
            
            camera.transform.position = new Vector3(0, 0, -distance);
            camera.nearClipPlane = distance + bounds.min.z - 0.001f;
            camera.farClipPlane = distance + bounds.max.z + 0.001f;

            Position(cut);
        }

        private void Position(float t)
        {
            var bounds = volumeRender.GetComponent<Renderer>().bounds;

            var origin = Vector3.Lerp(new Vector3(0, 0, bounds.min.z), new Vector3(0, 0, bounds.max.z), t);
            var normal = new Vector3(0, 0, -1);
            
            volumeRender.SetCutPlane(origin, normal);
        }

        private void OnValidate()
        {
            Position(cut);
        }

        public void Render()
        {
            Renderdd(4096, "temp.png");
        }
        
        private void Renderdd(int imageSize, string imageName)
        {
            var size = volumeRender.GetComponent<Renderer>().bounds.size;

            var height = imageSize;
            var width = (int) (size.x / size.y * height);
            print($"{imageName}: height:{height} x width:{width}");

            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            var tex = RenderCameraToTexture(camera, rt);

            var path = Path.Combine(Application.persistentDataPath, imageName);
            new FileInfo(path).Directory.Create();
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            camera.targetTexture = null;
            Destroy(rt);
            Destroy(tex);
        }

        private Texture2D RenderCameraToTexture(Camera cam, RenderTexture rt)
        {
            var prevRT = cam.targetTexture;
            var prevAA = QualitySettings.antiAliasing;
            QualitySettings.antiAliasing = 0;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;
            cam.targetTexture = prevRT;
            QualitySettings.antiAliasing = prevAA;

            return tex;
        }

        public void RenderAll()
        {
            StartCoroutine(RenderAllCoroutine());
        }

        private IEnumerator RenderAllCoroutine()
        {
            var seconds = 120f;
            var framesPerSecond = 60;
            var frames = (int) (seconds * framesPerSecond);

            for (var i = skipFrames; i < frames; i++)
            {
                var delta = (float) i / frames;
                Position(delta);
                Renderdd(4096, $"Exports/Export {seconds}/frame {i:0000}.png");
                yield return null;
            }
        }
    }
}