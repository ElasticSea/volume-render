using System.IO;
using System.Linq;
using _Project.Scripts.Gameplay.Replays;
using UnityEngine;
using UnityEngine.SpatialTracking;

public class PlayVRSession : MonoBehaviour
{
    [SerializeField] private string replayName;

    void Start()
    {
        var file = new FileInfo(Path.Combine(Application.persistentDataPath, "Replays", $"{replayName}.rpl"));
        var bytes = File.ReadAllBytes(file.FullName);
        var replay = ReplaySeriliazer.Read(bytes);
        var addComponent = gameObject.AddComponent<ReplayPlayer>();

        var map = FindObjectsOfType<Transform>().ToDictionary(t => t.name, t => t);
        var transforms = replay.Transforms.Select(t => map[t]).ToList();

        foreach (var tpd in FindObjectsOfType<TrackedPoseDriver>())
        {
            tpd.enabled = false;
        }
        
        addComponent.Load(replay.Snapshots, transforms);
    }
}
