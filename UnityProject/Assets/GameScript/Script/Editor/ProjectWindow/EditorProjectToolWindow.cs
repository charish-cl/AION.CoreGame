using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace GameDevKitEditor
{
    public class EditorProjectToolWindow:OdinEditorWindow
    {
        [Serializable]
        public class Collect
        {
            public string name;
            public string path;
        }
        [OnInspectorGUI("DrawBtn")]
        public  List<Collect> collects=new List<Collect>()
        {
            new Collect(){name="UIComponent",path="Assets/Game/UIComponent"},
            new Collect(){name="通用",path="Assets/Game/Sprites/Common"},
        };

        [MenuItem("EditorTool/EditorProjectWindow")]
        public static void Open()
        {
            var window = GetWindow<EditorProjectToolWindow>();
            window.Show();
        }

        void DrawBtn()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < collects.Count; i++)
            {
                if (GUILayout.Button(collects[i].name))
                {
                    Debug.Log("点击");
                    var obj = AssetDatabase.LoadAssetAtPath( collects[i].path,typeof(Object));
                    Debug.Log(obj);
                    EditorGUIUtility.PingObject(obj);
                    AssetDatabase.OpenAsset(obj);
                }

            }
            EditorGUILayout.EndHorizontal();
        }
    
    }

}