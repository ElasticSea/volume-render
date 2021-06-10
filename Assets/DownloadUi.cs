using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ElasticSea.Framework.Util;
using UnityEngine;
using UnityEngine.UIElements;

public class DownloadUi : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private void Start()
    {
        var root = uiDocument.rootVisualElement;
        var downloadUrlField = root.Q<TextField>("downloadUrl");
        var downloadButton = root.Q<Button>("download");
        var downloadProgress = root.Q<Label>("downloadProgress");

        downloadButton.clicked += async () =>
        {
            ServicePointManager.DefaultConnectionLimit = 16;

            var voluesDirPath = Path.Combine(Application.persistentDataPath, "Volumes");
            Utils.EnsureDirectory(voluesDirPath);
            var filePath = Path.Combine(voluesDirPath, Utils.GetRandomHexNumber(16) + ".vlm");
            
            using var client = new HttpClient {MaxResponseContentBufferSize = 1000000};
            using var fileStream = File.Create(filePath);
            await client.DownloadAsync(downloadUrlField.value, fileStream, new Progress<float>(f =>
            {
                downloadProgress.text = f.ToString("P2");
            }));
        };
    }
}
public static class HttpClientExtensions
{
    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default) {
        // Get the http headers first to examine the content length
        using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead)) {
            var contentLength = response.Content.Headers.ContentLength;

            using (var download = await response.Content.ReadAsStreamAsync()) {

                // Ignore progress reporting when no progress reporter was 
                // passed or when the content length is unknown
                if (progress == null || !contentLength.HasValue) {
                    await download.CopyToAsync(destination);
                    return;
                }

                // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                // Use extension method to report progress while downloading
                await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
                progress.Report(1);
            }
        }
    }
}
    
public static class StreamExtensions
{
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default) {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}
