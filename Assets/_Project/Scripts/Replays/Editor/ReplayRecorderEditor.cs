using UnityEditor;
using UnityEngine;

namespace Replays.Editor
{
    [CustomEditor(typeof(ReplayRecorder))]
    public class ReplayRecorderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var replayRecorder = target as ReplayRecorder;
            
            if (GUILayout.Button("Save"))
            {
                replayRecorder.Save();
            };
        }
    }
}