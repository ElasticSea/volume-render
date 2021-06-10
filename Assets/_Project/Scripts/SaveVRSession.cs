using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Project.Scripts.Gameplay.Replays;
using ElasticSea.Framework.Util;
using Replays;
using UnityEngine;

public class SaveVRSession : MonoBehaviour
{
    [SerializeField] private Transform[] transforms;
    
    [SerializeField] private string replayName;
    [SerializeField] private float replayLengthSeconds = 10;
    private bool save = true;
    private ReplayRecorder replayRecorder;

    void Start()
    {
        replayRecorder = this.gameObject.AddComponent<ReplayRecorder>();
        replayRecorder.Setup(transforms.ToList());
    }

    // Update is called once per frame
    void Update()
    {
        if (save && Time.time > replayLengthSeconds)
        {
            Save();
            save = false;
        }
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
