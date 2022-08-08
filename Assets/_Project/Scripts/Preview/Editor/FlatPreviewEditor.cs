using UnityEditor;
using UnityEngine;

namespace Preview.Editor
{
    [CustomEditor(typeof(FlatPreview))]
    public class FlatPreviewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Render"))
            {
                (target as FlatPreview).Render();
            }

            if (GUILayout.Button("Render All"))
            {
                (target as FlatPreview).RenderAll();
            }
        }
    }
}