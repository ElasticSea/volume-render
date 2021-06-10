using System.IO;
using System.Linq;
using _Project.Scripts.Gameplay.Replays;
using UnityEditor;
using UnityEngine;

namespace Replays.Editor
{
    [CustomEditor(typeof(ReplayPlayerSpawner))]
    public class ReplayPlayerSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var replayPlayer = target as ReplayPlayerSpawner;
            
            if (GUILayout.Button("Load"))
            {
                var file = new FileInfo(Path.Combine(Application.persistentDataPath, "Replays", "replayTest.rpl"));
                var bytes = File.ReadAllBytes(file.FullName);
                var replay = ReplaySeriliazer.Read(bytes);
                var addComponent = replayPlayer.gameObject.AddComponent<ReplayPlayer>();

                var map = replayPlayer.replay.GetComponentsInChildren<Transform>().ToDictionary(t => t.name, t => t);
                
                addComponent.Load(replay.Snapshots, replay.Transforms.Select(t => map[t]).ToList());
            };
        }
    }
}