using System;
using AION.CoreFramework;
using System.Collections.Generic;
using AION.CoreFramework.Editor.Inspector;
using UnityEditor;
using UnityEngine;

namespace AION.CoreFramework.Editor
{
    [CustomEditor(typeof(RootModule))]
    internal sealed class RootModuleInspector : GameFrameworkInspector
    {
        private const string NoneOptionName = "<None>";
        private static readonly float[] GameSpeed = new float[] { 0f, 0.01f, 0.1f, 0.25f, 0.5f, 1f, 1.5f, 2f, 4f, 8f };
        private static readonly string[] GameSpeedForDisplay = new string[] { "0x", "0.01x", "0.1x", "0.25x", "0.5x", "1x", "1.5x", "2x", "4x", "8x" };

        private SerializedProperty m_TextHelperTypeName = null;
        private SerializedProperty m_VersionHelperTypeName = null;
        private SerializedProperty m_LogHelperTypeName = null;
        private SerializedProperty m_CompressionHelperTypeName = null;
        private SerializedProperty m_JsonHelperTypeName = null;
        private SerializedProperty m_FrameRate = null;
        private SerializedProperty m_GameSpeed = null;
        private SerializedProperty m_RunInBackground = null;
        private SerializedProperty m_NeverSleep = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            RootModule t = (RootModule)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Global Helpers", EditorStyles.boldLabel);

                    UpdateHelperType<Utility.Text.ITextHelper>("Text Helper", m_TextHelperTypeName);
                    UpdateHelperType<Version.IVersionHelper>("Version Helper", m_VersionHelperTypeName);
                    UpdateHelperType<GameFrameworkLog.ILogHelper>("Log Helper", m_LogHelperTypeName);
                    UpdateHelperType<Utility.Json.IJsonHelper>("JSON Helper", m_JsonHelperTypeName);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            int frameRate = EditorGUILayout.IntSlider("Frame Rate", m_FrameRate.intValue, 1, 120);
            if (frameRate != m_FrameRate.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.FrameRate = frameRate;
                }
                else
                {
                    m_FrameRate.intValue = frameRate;
                }
            }

            EditorGUILayout.BeginVertical("box");
            {
                float gameSpeed = EditorGUILayout.Slider("Game Speed", m_GameSpeed.floatValue, 0f, 8f);
                int selectedGameSpeed = GUILayout.SelectionGrid(GetSelectedGameSpeed(gameSpeed), GameSpeedForDisplay, 5);
                if (selectedGameSpeed >= 0)
                {
                    gameSpeed = GetGameSpeed(selectedGameSpeed);
                }

                if (Math.Abs(gameSpeed - m_GameSpeed.floatValue) > 0.01f)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.GameSpeed = gameSpeed;
                    }
                    else
                    {
                        m_GameSpeed.floatValue = gameSpeed;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            bool runInBackground = EditorGUILayout.Toggle("Run in Background", m_RunInBackground.boolValue);
            if (runInBackground != m_RunInBackground.boolValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.RunInBackground = runInBackground;
                }
                else
                {
                    m_RunInBackground.boolValue = runInBackground;
                }
            }

            bool neverSleep = EditorGUILayout.Toggle("Never Sleep", m_NeverSleep.boolValue);
            if (neverSleep != m_NeverSleep.boolValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.NeverSleep = neverSleep;
                }
                else
                {
                    m_NeverSleep.boolValue = neverSleep;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_TextHelperTypeName = serializedObject.FindProperty("m_TextHelperTypeName");
            m_VersionHelperTypeName = serializedObject.FindProperty("m_VersionHelperTypeName");
            m_LogHelperTypeName = serializedObject.FindProperty("m_LogHelperTypeName");
            m_CompressionHelperTypeName = serializedObject.FindProperty("m_CompressionHelperTypeName");
            m_JsonHelperTypeName = serializedObject.FindProperty("m_JsonHelperTypeName");
            m_FrameRate = serializedObject.FindProperty("m_FrameRate");
            m_GameSpeed = serializedObject.FindProperty("m_GameSpeed");
            m_RunInBackground = serializedObject.FindProperty("m_RunInBackground");
            m_NeverSleep = serializedObject.FindProperty("m_NeverSleep");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            RefreshHelperTypeNames<Utility.Text.ITextHelper>(m_TextHelperTypeName);
            RefreshHelperTypeNames<Version.IVersionHelper>(m_VersionHelperTypeName);
            RefreshHelperTypeNames<GameFrameworkLog.ILogHelper>(m_LogHelperTypeName);
            RefreshHelperTypeNames<Utility.Json.IJsonHelper>(m_JsonHelperTypeName);

            serializedObject.ApplyModifiedProperties();
        }

        private void RefreshHelperTypeNames<T>(SerializedProperty helperTypeName)
        {
            var typeNames = new List<string> { NoneOptionName };
            typeNames.AddRange(Type.GetRuntimeTypeNames(typeof(T)));

            if (!string.IsNullOrEmpty(helperTypeName.stringValue))
            {
                if (!typeNames.Contains(helperTypeName.stringValue))
                {
                    helperTypeName.stringValue = null;
                }
            }
        }

        private void UpdateHelperType<T>(string label, SerializedProperty helperTypeName)
        {
            var typeNames = new List<string> { NoneOptionName };
            typeNames.AddRange(Type.GetRuntimeTypeNames(typeof(T)));

            int selectedIndex = EditorGUILayout.Popup(label, typeNames.IndexOf(helperTypeName.stringValue), typeNames.ToArray());
            helperTypeName.stringValue = selectedIndex <= 0 ? null : typeNames[selectedIndex];
        }

        private float GetGameSpeed(int selectedGameSpeed)
        {
            if (selectedGameSpeed < 0)
            {
                return GameSpeed[0];
            }

            if (selectedGameSpeed >= GameSpeed.Length)
            {
                return GameSpeed[GameSpeed.Length - 1];
            }

            return GameSpeed[selectedGameSpeed];
        }

        private int GetSelectedGameSpeed(float gameSpeed)
        {
            for (int i = 0; i < GameSpeed.Length; i++)
            {
                if (gameSpeed == GameSpeed[i])
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
