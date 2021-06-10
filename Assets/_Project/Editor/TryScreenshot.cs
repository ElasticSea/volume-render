using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using File = UnityEngine.Windows.File;

namespace Editor
{
    [CustomEditor(typeof(TryScreenshot))]
    public class TryScreenshotEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Make"))
            {
                (target as TryScreenshot).Make();

            }
        }
    }
}