using System.IO;
using System.Linq;
using _Project.Scripts.Gameplay.Replays;
using UnityEditor;
using UnityEngine;

namespace Replays.Editor
{
    [CustomEditor(typeof(ReplayPlayer))]
    public class ReplayPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var player = target as ReplayPlayer;

            GUILayout.BeginHorizontal();
            var sign = Mathf.Sign(player.TimeScale);
            if (GUILayout.Button("1/64")) player.TimeScale = 1 / 64f * sign;
            if (GUILayout.Button("1/32")) player.TimeScale = 1 / 32f * sign;
            if (GUILayout.Button("1/16")) player.TimeScale = 1 / 16f * sign;
            if (GUILayout.Button("1/8")) player.TimeScale = 1 / 8f * sign;
            if (GUILayout.Button("1/4")) player.TimeScale = 1 / 4f * sign;
            if (GUILayout.Button("1/2")) player.TimeScale = 1 / 2f * sign;
            if (GUILayout.Button("1")) player.TimeScale = 1 * sign;
            if (GUILayout.Button("2")) player.TimeScale = 2 * sign;
            if (GUILayout.Button("4")) player.TimeScale = 4 * sign;
            if (GUILayout.Button("8")) player.TimeScale = 8 * sign;
            if (GUILayout.Button("16")) player.TimeScale = 16 * sign;
            if (GUILayout.Button("32")) player.TimeScale = 32 * sign;
            if (GUILayout.Button("64")) player.TimeScale = 64 * sign;
            if (GUILayout.Button("-/+")) player.TimeScale = -player.TimeScale;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (player.enabled)
            {
                if (GUILayout.Button("Stop")) player.enabled = false;
            }
            else
            {
                if (GUILayout.Button("Play")) player.enabled = true;
            }
            GUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Time", player.Time.ToString("F3"));
            var newTime = GUILayout.HorizontalSlider(player.Time, player.MinTime, player.MaxTime);
            if (newTime != player.Time)
            {
                player.Time = newTime;
            }
            EditorGUILayout.LabelField("Frame", player.Frame.ToString());
            EditorGUILayout.LabelField("Fps", $"{player.Frames / player.MaxTime:F2}");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous"))
            {
                player.Frame--;
            }
                
            if (GUILayout.Button("Next"))
            {
                player.Frame++;
            }
            GUILayout.EndHorizontal();
        }
        
        protected virtual void OnSceneGUI()
        {
            Repaint();
        }
    }
}