using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameDevKitEditor
{
    [TreeWindow("选中工具")]
    public class SelectionWindow : OdinEditorWindow
    {
        public Texture2D texture;

        [Button]
        public void ProfilerCreateSprite()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            sw.Stop();
            UnityEngine.Debug.Log("Sprite.Create耗时: " + sw.ElapsedMilliseconds + " ms");
        }

        public static string folder => SelectionExtend.GetCurrentAssetDirectory();

        [MenuItem("Assets/SelectTool/生成或移动到UIForm")]
        public static void MoveSelectPrefabToUIForm()
        {
            var selectObj = Selection.activeGameObject;
            if (selectObj == null)
            {
                return;
            }

            string uiFormPath = "Assets/Game/UIForm";
            
            //看选中对象有没有挂载ComponentAutoBind
            var autoBind = selectObj.GetComponent<ComponentAutoBindTool>();
            if (autoBind == null)
            {
                selectObj.AddComponent<ComponentAutoBindTool>();
            }

            if ( PrefabUtility.IsPartOfAnyPrefab(Selection.activeObject))
            {
                string fileName  = Path.GetFileName(AssetDatabase.GetAssetPath(selectObj));
                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(selectObj), Path.Combine(uiFormPath, fileName));
                Debug.Log($"已移动到 {uiFormPath}");
            }
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(selectObj));
                PrefabUtility.SaveAsPrefabAssetAndConnect((GameObject)selectObj, Path.Combine(uiFormPath, fileName + ".prefab"),InteractionMode.UserAction);
                Debug.Log($"已生成Prefab并移动到 {uiFormPath}");
            }

        }
        [MenuItem("Assets/SelectTool/选中导出当前文件夹子路径List", priority = 4)]
        public static void GeneratelistResouseCode()
        {
            var folder = IOUtility.GetCurrentAssetDirectory();

            var files = Directory.GetFiles(folder, "*").Where(e => !e.Contains(".meta")).ToList();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var file in files)
            {
                string s = file.Replace("\\", "/");
                stringBuilder.AppendLine($"\"{s}\",");
            }

            GUIUtility.systemCopyBuffer = stringBuilder.ToString();
        }

        [MenuItem("Assets/SelectTool/选中导出当前文件夹子NameList", priority = 4)]
        public static void GeneratelistNameResouseCode()
        {
            var folder = IOUtility.GetCurrentAssetDirectory();

            var files = Directory.GetFiles(folder, "*").Where(e => !e.Contains(".meta")).ToList();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var file in files)
            {
                string s = file.Replace("\\", "/");
                stringBuilder.AppendLine($"{s.Split('/').Last().Split('.').First()}");
            }

            GUIUtility.systemCopyBuffer = stringBuilder.ToString();
        }

        
        //删除选中文件夹下所有xml文件
        [MenuItem("Assets/SelectTool/删除选中文件夹下所有xml文件", priority = 2)]
        [Button("删除选中文件夹下所有xml文件")]
        public static void DeleteXml()
        {
            var fileInfo = new DirectoryInfo(folder).GetFiles("*.xml", SearchOption.AllDirectories).ToList();
            foreach (var file in fileInfo)
            {
                Debug.Log(file.Name);
                File.Delete(file.FullName);
            }

            AssetDatabase.Refresh();
        }


        public GameObject SelectionObj;


        [OnInspectorGUI]
        private void MyUpdate()
        {
            if (Selection.activeObject == null)
            {
                return;
            }

            SelectionObj = Selection.activeGameObject;
        }

        [Button]
        public void DebugRectTransform()
        {
            Debug.Log("RectTransform.position" + SelectionObj.GetComponent<RectTransform>().position);

            var rectTransform = SelectionObj.GetComponent<RectTransform>();
            var transform = SelectionObj.GetComponent<Transform>();
            Debug.Log($"rectTransform.anchoredPosition{rectTransform.anchoredPosition}");

            //转屏幕坐标
            Debug.Log("Camera.main.WorldToScreenPoint(transform.position)" +
                      Camera.main.WorldToScreenPoint(transform.position));

            // 将 UI 元素的本地坐标转换为世界坐标
            Vector3 worldPosition = transform.TransformPoint(Vector3.zero);

            // 将世界坐标转换为屏幕坐标
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            Debug.Log("UI 元素在屏幕上的坐标：" + screenPosition);
        }

        [Button]
        public void DebugTransformPostion()
        {
            var transform = SelectionObj.GetComponent<Transform>();

            Debug.Log("transform.position" + transform.position);
            Debug.Log("transform.localPosition" + transform.localPosition);

            var Particle = SelectionObj.GetComponent<ParticleSystem>();


            var renderers = Particle.GetComponentsInChildren<Renderer>();
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                renderer.enabled = true;


                // renderer.sor
                renderer.sortingLayerName = "ChessEvent";
                // renderer.sortingLayerID
                EditorUtility.SetDirty(renderer);
            }
        }

        [Button]
        public void LoadSprite()
        {
            var spritePath = "Assets/Game/DynamicResource/ReplaceableImage/MilestoneActivity/1/TitleImg.png";
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(spritePath, typeof(Sprite));

            Debug.Log(asset);
        }

        //打印下选中对象的依赖信息
        [Button]
        public void PrintDependencies()
        {
            var dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(Selection.activeObject));
            foreach (var dependency in dependencies)
            {
                Debug.Log(dependency);
            }
        }

        //获取选中文件大小
        [Button]
        public void GetFileSize()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var fileInfo = new FileInfo(path);
            long fileSizeInBytes = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(Selection.activeObject);

            //小于1KB的直接显示字节数，大于1KB小于1MB的显示KB数，大于1MB的显示MB数
            if (fileSizeInBytes < 1024)
            {
                Debug.Log($"文件大小：{fileSizeInBytes} B");
            }
            else if (fileSizeInBytes < 1024 * 1024)
            {
                float fileSizeInKB = fileSizeInBytes / 1024f;
                Debug.Log($"文件大小：{fileSizeInKB} KB");
            }
            else
            {
                float fileSizeInMB = fileSizeInBytes / (1024f * 1024f);
                Debug.Log($"文件大小：{fileSizeInMB} MB");
            }
        }
    }
}