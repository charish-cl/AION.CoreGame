using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace AION.CoreFramework
{
    [InitializeOnLoad]
    public class SceneSwitchLeftButton
    {

        static SceneSwitchLeftButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add((2,OnToolbarGUI));
        }

        static readonly string ButtonStyleName = "Tab middle";
        static GUIStyle _buttonGuiStyle;

        //缓存所有场景的名字，以及场景的路径
        
        private static string[] _sceneNames;
        private static string[] _scenePaths;
        private static int currentSceneIndex;
        static void OnToolbarGUI()
        {
            _buttonGuiStyle ??= new GUIStyle(ButtonStyleName)
            {
                // padding = new RectOffset(2, 8, 2, 2),
                fixedWidth = 120,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if (_scenePaths == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:scene ", null);
                    if (guids.Length == 0)
                    {
                        Debug.LogWarning("Couldn't find scene file");
                    }

                    _scenePaths = new string[guids.Length];

                    for (int i = 0; i < guids.Length; i++)
                    {
                        _scenePaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                        //获取当前场景的Index
                        if (EditorSceneManager.GetActiveScene().path == _scenePaths[i])
                        {
                            currentSceneIndex = i;
                        }
                    }
                }
                //scenePath用‘/'分割取最后的Name
                _sceneNames ??= _scenePaths.Select(e => e.Split('/').Last()).ToArray();

                //下拉框选择所有的场景
                int selectedIndex = EditorGUILayout.Popup("", currentSceneIndex, _sceneNames, _buttonGuiStyle);

                if (selectedIndex < _scenePaths.Length && selectedIndex != currentSceneIndex)
                {
                    EditorSceneManager.OpenScene(_scenePaths[selectedIndex]);
                    _sceneNames = null;
                    currentSceneIndex = selectedIndex;
                    _sceneNames = null;
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    static class SceneHelper
    {
        static string _sceneToOpen;

        public static void StartScene(string sceneName)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            _sceneToOpen = sceneName;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (_sceneToOpen == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.update -= OnUpdate;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string[] guids = AssetDatabase.FindAssets("t:scene " + _sceneToOpen, null);
                if (guids.Length == 0)
                {
                    Debug.LogWarning("Couldn't find scene file");
                }
                else
                {
                    string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    EditorSceneManager.OpenScene(scenePath);
                    EditorApplication.isPlaying = true;
                }
            }

            _sceneToOpen = null;
        }
    }
}