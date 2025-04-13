using AION.CoreFramework;
using UnityEditor;
using UnityEngine;

namespace GameDevKitEditor
{
    [CustomEditor(typeof(CircleImage), true)]
    [CanEditMultipleObjects]
    public class CircleImageEditor : UnityEditor.UI.ImageEditor
    {
        private SerializedProperty _segments;
        private SerializedProperty _fillPercent;

        protected override void OnEnable()
        {
            base.OnEnable();
            _segments = serializedObject.FindProperty("_segments");
            _fillPercent = serializedObject.FindProperty("_fillPercent");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.Slider(_fillPercent, 0, 1, new GUIContent("showPercent"));
            EditorGUILayout.PropertyField(_segments);
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}