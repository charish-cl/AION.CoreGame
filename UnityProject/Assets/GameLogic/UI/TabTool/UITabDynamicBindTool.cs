using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AION.CoreFramework;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLogic
{
    public class UITabDynamicBindTool: BaseTabBindTool
    {
        
        [LabelText("动态Tab路径")] public Dictionary<int, string> DynammicTabPathDic;


        [LabelText("动态Tab相对父物体的路径")]
        public Dictionary<int, string> DynammicTabRelativePathDic;
        
        
        [Button("创建预制体并挂载ComponetAutoBindTool", ButtonHeight = 50)]
        public void AddUIItemToSubGroup()
        {
#if UNITY_EDITOR

            var uiFormName = UISelectTool.GetRoot(transform).name;

            DynammicTabPathDic ??= new Dictionary<int, string>();
            DynammicTabRelativePathDic ??= new Dictionary<int, string>();
            string folderPath = Path.Combine("Assets/Game/UIComponent/", uiFormName + "_SubGroup");
            //目标没有创建
            if (!Directory.Exists(folderPath))
            {
                Debug.Log($"创建目录 {folderPath}");
                Directory.CreateDirectory(folderPath);
            }

            for (var i = 0; i < BindGo.Count; i++)
            {
                var group = BindGo[i];
                if (group._gameObjects.Count < 1)
                {
                    throw new Exception("请先选择绑定对象");
                }

                var go = group._gameObjects.First();
                if (PrefabUtility.IsPartOfAnyPrefab(go))
                {
                    Debug.Log($"Prefab {go.name} is part of a prefab instance. Skipping.");
                    continue;
                }
                
                var className = uiFormName + "_" + group.TitleName;
                

                var itemPath = Path.Combine(folderPath, className + ".prefab").Replace('\\', '/');

                Debug.Log(itemPath);
                if (DynammicTabPathDic.ContainsKey(i))
                {
                    DynammicTabPathDic[i] = itemPath;
                    var relativePath =
                        UISelectTool.GetTransformPath(Root, group._gameObjects.First().transform);
                    DynammicTabRelativePathDic[i] = relativePath;
                }
                else
                {
                    DynammicTabPathDic.Add(i, itemPath);

                    var relativePath =
                        UISelectTool.GetTransformPath(Root, group._gameObjects.First().transform);
                    DynammicTabRelativePathDic[i] = relativePath;
                }

                Debug.Log(group._gameObjects.First());

                PrefabUtility.SaveAsPrefabAssetAndConnect(group._gameObjects.First(), itemPath,
                    InteractionMode.UserAction);

                AssetDatabase.Refresh();
            }

#endif
        }
        public void DetachSubGroup(string prefabPath, Transform root)
        {
#if UNITY_EDITOR

            Debug.Log(prefabPath);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabContents == null)
            {
                Debug.LogError("Failed to load prefab contents.");
                return;
            }

            foreach (var (tabIndex, value) in DynammicTabPathDic)
            {
                var group = BindGo[tabIndex];
                var go = group._gameObjects.First();
                Debug.Log($"移除 {go.name}");
                var goPath = DynammicTabRelativePathDic[tabIndex];
                prefabContents.transform.Find(TabPath).GetComponent<UITabBindTool>().BindGo[tabIndex]._gameObjects
                    .RemoveAt(0);
                var nestedPrefab = prefabContents.transform.Find(goPath);
                if (nestedPrefab == null)
                {
                    Debug.LogError($"Failed to find nested prefab with path: {goPath}");
                }

                DestroyImmediate(nestedPrefab.gameObject);
                EditorUtility.SetDirty(TabParent.gameObject);
            }

            // 保存修改并卸载
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);

            AssetDatabase.Refresh();
#endif
        }


        [Button("预览", ButtonHeight = 50)]
        public void AttachSubGroup()
        {
#if UNITY_EDITOR

            foreach (var (tabIndex, value) in DynammicTabPathDic)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(value);

                PrefabUtility.InstantiatePrefab(go, TabParent);
                BindGo[tabIndex]._gameObjects.Insert(0, go);
            }

            EditorUtility.SetDirty(UISelectTool.GetRoot(Selection.activeTransform));
            AssetDatabase.SaveAssets();
#endif
        }

        [Button("拷贝所有SubGroup类代码", ButtonHeight = 50)]
        public void GenerateSubGroupClass()
        {
#if UNITY_EDITOR
            StringBuilder builder = new StringBuilder();
            foreach (var (tabIndex, value) in DynammicTabPathDic)
            {
                var go = BindGo[tabIndex]._gameObjects.First();
                Selection.activeGameObject = go;
                var str = go.GetComponent<ComponentAutoBindTool>().GenerateUIBindings();
                builder.AppendLine(str);
            }
            Selection.activeGameObject = TabParent.gameObject;
            GUIUtility.systemCopyBuffer = builder.ToString();
#endif
        }
        public void DeletePrefabs()
        {
#if UNITY_EDITOR
            // 选择一个目标预制体
            string prefabPath = AssetDatabase.GetAssetPath(Selection.activeGameObject);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("Please select a prefab asset in the Project window.");
                return;
            }

            // 加载预制体内容到内存
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabContents == null)
            {
                Debug.LogError("Failed to load prefab contents.");
                return;
            }

            // 查找要删除的嵌套预制体（假设名称为 "NestedPrefab"）
            Transform nestedPrefab = prefabContents.transform.Find("State/DebugUI_State1");
            if (nestedPrefab != null)
            {
                // 删除嵌套预制体
                Debug.Log($"Removed nested prefab: {nestedPrefab.name}");
                Object.DestroyImmediate(nestedPrefab.gameObject);
            }
            else
            {
                Debug.LogWarning("No nested prefab found with the specified name.");
            }

            // 保存修改并卸载
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);

            Debug.Log("Prefab modifications saved successfully.");
#endif     
        }
    }
}