using UnityEditor;
using UnityEngine;

namespace Replays.Editor
{
    [CustomEditor(typeof(SaveVRSession))]
    public class ReplayRecorderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var replayRecorder = target as SaveVRSession;
            
            if (GUILayout.Button("StartRecording"))
            {
                replayRecorder.StartRecording();
            };
            
            if (GUILayout.Button("StopRecording"))
            {
                replayRecorder.StopRecording();
            };
        }
    }
}