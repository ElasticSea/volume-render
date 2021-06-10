using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class TryScreenshot : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private ReplayPlayer player;
    [SerializeField] private int frames = 10;
    [SerializeField] private int width = 3840;
    [SerializeField] private int height = 2160;

    public void Make()
    {
        StartCoroutine(Render());
    }

    private IEnumerator Render()
    {
        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);

        for (var i = 0; i < frames; i++)
        {
            camera.targetTexture = rt;
            camera.Render();
            camera.targetTexture = null;

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);

            var pngPath = Path.Combine(Application.persistentDataPath, $"frame_{player.Frame}.png");
            var png = tex.EncodeToPNG();
            File.WriteAllBytes(pngPath, png);

            player.Frame++;
            yield return new WaitForEndOfFrame();
        }
    }
}
