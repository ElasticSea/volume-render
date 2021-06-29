using System.IO;
using System.Linq;
using _Project.Scripts.Gameplay.Replays;
using ElasticSea.Framework.Util;
using Replays;
using UnityEngine;

public class SaveVRSession : MonoBehaviour
{
    [SerializeField] private Transform[] transforms;

    [SerializeField] private ReplayRecorder replayRecorder;
    [SerializeField] private string replayName;

    void Start()
    {
        replayRecorder.Setup(transforms.ToList());
        replayRecorder.enabled = false;
    }
    
    public void StartRecording()
    {
        replayRecorder.Clear();
        replayRecorder.enabled = true;
    }

    public void StopRecording()
    {
        replayRecorder.enabled = false;
        Save();
    }

    private void Save()
    {
        var replay = new Replay
        {
            Transforms = transforms.Select(t => t.name).ToArray(),
            Snapshots = replayRecorder.Snapshots.ToList()
        };

        var bytes = ReplaySeriliazer.Write(replay);

        var file = new FileInfo(Path.Combine(Application.persistentDataPath, "Replays", $"{replayName}.rpl"));
        Utils.EnsureDirectory(file.Directory.FullName);
        File.WriteAllBytes(file.FullName, bytes);
    }
}
