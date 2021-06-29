using System.IO;
using System.Linq;
using _Project.Scripts.Gameplay.Replays;
using ElasticSea.Framework.Extensions;
using UnityEngine;
using UnityEngine.SpatialTracking;

public class PlayVRSession : MonoBehaviour
{
    [SerializeField] private ReplayPlayer replayPlayer;
    [SerializeField] private string replayName;

    void Start()
    {
        var file = new FileInfo(Path.Combine(Application.persistentDataPath, "Replays", $"{replayName}.rpl"));
        var bytes = File.ReadAllBytes(file.FullName);
        var replay = ReplaySeriliazer.Read(bytes);

        var map = FindObjectsOfType<Transform>().DistinctBy(t => t.name).ToDictionary(t => t.name, t => t);
        var transforms = replay.Transforms.Select(t => map[t]).ToList();

        foreach (var tpd in FindObjectsOfType<TrackedPoseDriver>())
        {
            tpd.enabled = false;
        }
        
        replayPlayer.Load(replay.Snapshots, transforms);
    }
}
