// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text;
// using AION.CoreFramework.SwitchAction;
// using Sirenix.OdinInspector;
// using Sirenix.Serialization;
//
// #if UNITY_EDITOR
// using UnityEditor;
// #endif
//
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace AION.CoreFramework
// {
//     [Serializable]
//     public class Group
//     {
//         public string TitleName;
//
//         public string Comment;
//
// #if UNITY_EDITOR
//         [ValueDropdown("TreeViewAdd", ExpandAllMenuItems = true, IsUniqueList = false,
//             DrawDropdownForListElements = true)]
// #endif
//         [LabelText("绑定对象")]
//         public List<GameObject> _gameObjects;
//
//
//         [HideInInspector] public Transform transform;
//
// #if UNITY_EDITOR
//         [ValueDropdown("SwitchActionsDropdown")]
// #endif
//         [LabelText("进入行为")]
//         [OdinSerialize, SerializeReference]
//         public List<BaseAction> EnterActions = new List<BaseAction>();
//
//         [LabelText("离开行为")] [OdinSerialize, SerializeReference]
//         public List<BaseAction> ExitActions = new List<BaseAction>();
//
//
// #if UNITY_EDITOR
//         IEnumerable SwitchActionsDropdown()
//         {
//             return UnityEditor.TypeCache.GetTypesDerivedFrom<BaseAction>().Where(t => t.IsAbstract == false).Select(t =>
//                 new ValueDropdownItem(t.Name, Activator.CreateInstance(t) as BaseAction));
//         }
//
// #endif
//
//         public Group()
//         {
//             _gameObjects = new List<GameObject>();
//         }
//
//         public Group(string titleName, string comment, List<GameObject> gameObjects)
//         {
//             TitleName = titleName;
//             Comment = comment;
//             _gameObjects = gameObjects;
//         }
//
// #if UNITY_EDITOR
//         public IEnumerable TreeViewAdd()
//         {
//             if (transform == null)
//             {
//                 transform = SelectionHelper.GetRoot(Selection.activeTransform);
//             }
//
//             return transform.GetComponentsInChildren<Transform>(true)
//                 .Select(x =>
//                     new ValueDropdownItem(SelectionHelper.GetTransformPath(transform, x.transform), x.gameObject));
//         }
// #endif
//     }
//
//     public class UISetActiveBindTool : SerializedMonoBehaviour
//     {
//         [TableList] public List<Group> BindGo = new List<Group>();
//
//         [LabelText("动态Tab路径")] public Dictionary<int, string> DynammicTabPathDic;
//
//
//         [LabelText("动态Tab相对父物体的路径")]
//         public Dictionary<int, string> DynammicTabRelativePathDic;
//
//         [ReadOnly] public string TabPath;
//
//         [OnInspectorInit]
//         private void OnInit()
//         {
//             BindGo ??= new List<Group>();
//             if (TabParent == null)
//             {
//                 TabParent = transform;
//             }
//
//             if (string.IsNullOrEmpty(TabPath))
//             {
//                 var relativePath = SelectionHelper.GetTransformPath(TabParent.transform.root, TabParent);
//
//                 relativePath = relativePath.Substring(relativePath.IndexOf("/", StringComparison.Ordinal) + 1);
//                 TabPath = relativePath;
//             }
//         }
//
//         [LabelText("父物体")] public Transform TabParent;
//
//         [Button("创建", ButtonHeight = 50)]
//         public void Create()
//         {
//             if (TabParent == null)
//             {
//                 throw new Exception("请先选择父物体");
//             }
//
//             for (int i = 0; i < TabParent.childCount; i++)
//             {
//                 var go = TabParent.GetChild(i).gameObject;
//
//                 var (comment, s) = SelectionHelper.GenerateConstantName(go.name);
//                 BindGo.Add(new Group(s, comment, new List<GameObject>() { go }));
//             }
//         }
//
//         [OnValueChanged("OpenTab")] [ValueDropdown("GetAllTabNames")]
//         public string SelectTabName;
//
//         
//
//         [Button("清空所有绑定对象", ButtonHeight = 50)]
//         public void ClearAllBindGo()
//         {
//             foreach (var group in BindGo)
//             {
//                 group._gameObjects.Clear();
//             }
//             
//             #if UNITY_EDITOR
//                    EditorUtility.SetDirty(this);
//             #endif
//         
//         }
//         public string LastSelectTabName;
//
//         public List<string> GetAllTabNames()
//         {
//             return BindGo.Select(e => e.Comment).ToList();
//         }
//
//         //OpenTab
//         public void OpenTab()
//         {
//             //一致则不处理
//             if (LastSelectTabName == SelectTabName)
//             {
//                 return;
//             }
//
//             //触发上一个Tab的退出动作
//             if (!string.IsNullOrEmpty(LastSelectTabName))
//             {
//                 var lastTab = BindGo.Find(e => e.Comment == LastSelectTabName);
//                 if (lastTab != null)
//                 {
//                     lastTab.ExitActions.ForEach(e => e.Execute());
//                 }
//             }
//
//             //触发当前Tab的进入动作
//             var currentTab = BindGo.Find(e => e.Comment == SelectTabName);
//             if (currentTab != null)
//             {
//                 currentTab.EnterActions.ForEach(e => e.Execute());
//             }
//
//             LastSelectTabName = SelectTabName;
//
//             if (BindGo == null)
//             {
//                 Debug.LogError("BindGo is null.");
//                 return;
//             }
//
//             foreach (var tab in BindGo)
//             {
//                 foreach (var go in tab._gameObjects)
//                 {
//                     go.SetActive(tab.Comment == SelectTabName);
//                 }
//             }
//
//             //确保当前选择的tab是可见的
//             BindGo.Find(e => e.Comment == SelectTabName)._gameObjects.ForEach(e => e.SetActive(true));
//         }
//
//         public TabModule ConvertToTab()
//         {
//             var tab = TabModule.Create();
//             tab.StateParent = GetComponent<RectTransform>();
//             tab.DynammicTabPathDic = DynammicTabPathDic;
//
//             for (int i = 0; i < BindGo.Count; i++)
//             {
//                 var group = BindGo[i];
//                 tab.AddTab(i, BindGo[i]._gameObjects);
//                 if (group.EnterActions.Count > 0)
//                 {
//                     tab.AddSwitchAction(i, () =>
//                     {
//                         for (int j = 0; j < group.EnterActions.Count; j++)
//                         {
//                             group.EnterActions[j].Execute();
//                         }
//                     });
//                 }
//
//                 if (group.ExitActions.Count > 0)
//                 {
//                     tab.AddSwitchAction(i, () =>
//                     {
//                         for (int j = 0; j < group.ExitActions.Count; j++)
//                         {
//                             group.ExitActions[j].Execute();
//                         }
//                     });
//                 }
//             }
//
//             return tab;
//         }
//
//         [Button("反向添加", ButtonHeight = 50)]
//         public void InvertAdd([ValueDropdown("TreeViewAdd")] GameObject go)
//         {
//             if (go == null)
//             {
//                 Debug.LogError("go is null.");
//                 return;
//             }
//
//             foreach (var group in BindGo)
//             {
//                 if (group.Comment != SelectTabName)
//                 {
//                     group._gameObjects.Add(go);
//                 }
//             }
//         }
// #if UNITY_EDITOR
//         public IEnumerable TreeViewAdd()
//         {
//             var root = SelectionHelper.GetRoot(Selection.activeTransform);
//             return root.GetComponentsInChildren<Transform>(true)
//                 .Select(x =>
//                     new ValueDropdownItem(SelectionHelper.GetTransformPath(root, x.transform), x.gameObject));
//         }
// #endif
//
//         #region 生成代码
//
//         [Button("生成Tab枚举", ButtonHeight = 50)]
//         public void GenerateTabClass()
//         {
//             string className = gameObject.name + "_Tab";
//             List<string> tabNames = BindGo.Select(e => e.TitleName).ToList();
//             List<string> commentNames = BindGo.Select(e => e.Comment).ToList();
//             if (string.IsNullOrEmpty(className) || tabNames.Count == 0)
//             {
//                 Debug.LogError("Invalid input parameters.");
//                 return;
//             }
//
//             // Generate the class header
//             string classCode = $"public enum Enum{className}\n{{\n";
//
//             // Generate constants for each tab
//             for (int i = 0; i < tabNames.Count; i++)
//             {
//                 var comment = commentNames[i];
//                 var tabName = tabNames[i];
//                 //生成注释 ///
//                 classCode += $"\t/// <summary>\n\t/// {comment}\n\t/// </summary>\n";
//                 classCode += $"{tabName} = {i},\n";
//             }
//
//             classCode.TrimEnd(',');
//             // Close the class
//             classCode += $"}}";
//             //
//             // StringBuilder builder = new StringBuilder();
//             // Print the generated code
//             GUIUtility.systemCopyBuffer = classCode;
//         }
//
//
//         [Button("创建预制体并挂载ComponetAutoBindTool", ButtonHeight = 50)]
//         public void AddUIItemToSubGroup()
//         {
// #if UNITY_EDITOR
//
//             var uiFormName = TabParent.GetComponentInParent<UIFormLogicExtend>().gameObject.name;
//
//             DynammicTabPathDic ??= new Dictionary<int, string>();
//             DynammicTabRelativePathDic ??= new Dictionary<int, string>();
//             string folderPath = Path.Combine("Assets/Game/UIComponent/", uiFormName + "_SubGroup");
//             //目标没有创建
//             if (!Directory.Exists(folderPath))
//             {
//                 Debug.Log($"创建目录 {folderPath}");
//                 Directory.CreateDirectory(folderPath);
//             }
//
//             for (var i = 0; i < BindGo.Count; i++)
//             {
//                 var group = BindGo[i];
//                 if (group._gameObjects.Count < 1)
//                 {
//                     throw new Exception("请先选择绑定对象");
//                 }
//
//                 var go = group._gameObjects.First();
//                 if (PrefabUtility.IsPartOfAnyPrefab(go))
//                 {
//                     Debug.Log($"Prefab {go.name} is part of a prefab instance. Skipping.");
//                     continue;
//                 }
//                 var componentBindTool = go.GetOrAddComponent<ComponentAutoBindTool>();
//
//                 var className = uiFormName + "_" + group.TitleName;
//
//                 componentBindTool.uiType = ComponentAutoBindTool.UIType.WindowItemOrWindow;
//                 componentBindTool.className = className;
//
//
//                 var itemPath = Path.Combine(folderPath, className + ".prefab").Replace('\\', '/');
//
//                 Debug.Log(itemPath);
//                 if (DynammicTabPathDic.ContainsKey(i))
//                 {
//                     DynammicTabPathDic[i] = itemPath;
//                     var relativePath =
//                         SelectionHelper.GetTransformPath(transform.GetComponentInParent<UIFormLogicExtend>().transform, group._gameObjects.First().transform);
//                     DynammicTabRelativePathDic[i] = relativePath;
//                 }
//                 else
//                 {
//                     DynammicTabPathDic.Add(i, itemPath);
//
//                     var relativePath =
//                         SelectionHelper.GetTransformPath(transform.GetComponentInParent<UIFormLogicExtend>().transform, group._gameObjects.First().transform);
//                     DynammicTabRelativePathDic[i] = relativePath;
//                 }
//
//                 Debug.Log(group._gameObjects.First());
//
//                 PrefabUtility.SaveAsPrefabAssetAndConnect(group._gameObjects.First(), itemPath,
//                     InteractionMode.UserAction);
//
//                 AssetDatabase.Refresh();
//             }
//
// #endif
//         }
//         
//
//         public void DetachSubGroup(string prefabPath, Transform root)
//         {
// #if UNITY_EDITOR
//
//             Debug.Log(prefabPath);
//             GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
//             if (prefabContents == null)
//             {
//                 Debug.LogError("Failed to load prefab contents.");
//                 return;
//             }
//
//             foreach (var (tabIndex, value) in DynammicTabPathDic)
//             {
//                 var group = BindGo[tabIndex];
//                 var go = group._gameObjects.First();
//                 Debug.Log($"移除 {go.name}");
//                 var goPath = DynammicTabRelativePathDic[tabIndex];
//                 prefabContents.transform.Find(TabPath).GetComponent<UISetActiveBindTool>().BindGo[tabIndex]._gameObjects
//                     .RemoveAt(0);
//                 var nestedPrefab = prefabContents.transform.Find(goPath);
//                 if (nestedPrefab == null)
//                 {
//                     Debug.LogError($"Failed to find nested prefab with path: {goPath}");
//                 }
//
//                 DestroyImmediate(nestedPrefab.gameObject);
//                 EditorUtility.SetDirty(TabParent.gameObject);
//             }
//
//             // 保存修改并卸载
//             PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
//             PrefabUtility.UnloadPrefabContents(prefabContents);
//
//             AssetDatabase.Refresh();
// #endif
//         }
//
//
//         [Button("预览", ButtonHeight = 50)]
//         public void AttachSubGroup()
//         {
// #if UNITY_EDITOR
//
//             foreach (var (tabIndex, value) in DynammicTabPathDic)
//             {
//                 var go = AssetDatabase.LoadAssetAtPath<GameObject>(value);
//
//                 PrefabUtility.InstantiatePrefab(go, TabParent);
//                 BindGo[tabIndex]._gameObjects.Insert(0, go);
//             }
//
//             EditorUtility.SetDirty(SelectionHelper.GetRoot(Selection.activeTransform));
//             AssetDatabase.SaveAssets();
// #endif
//         }
//
//         [Button("拷贝所有SubGroup类代码", ButtonHeight = 50)]
//         public void GenerateSubGroupClass()
//         {
// #if UNITY_EDITOR
//             StringBuilder builder = new StringBuilder();
//             foreach (var (tabIndex, value) in DynammicTabPathDic)
//             {
//                 var go = BindGo[tabIndex]._gameObjects.First();
//                 Selection.activeGameObject = go;
//                 var str = go.GetComponent<ComponentAutoBindTool>().GenerateUIBindings();
//                 builder.AppendLine(str);
//             }
//             Selection.activeGameObject = TabParent.gameObject;
//             GUIUtility.systemCopyBuffer = builder.ToString();
// #endif
//         }
//         public void DeletePrefabs()
//         {
// #if UNITY_EDITOR
//             // 选择一个目标预制体
//             string prefabPath = AssetDatabase.GetAssetPath(Selection.activeGameObject);
//             if (string.IsNullOrEmpty(prefabPath))
//             {
//                 Debug.LogError("Please select a prefab asset in the Project window.");
//                 return;
//             }
//
//             // 加载预制体内容到内存
//             GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
//             if (prefabContents == null)
//             {
//                 Debug.LogError("Failed to load prefab contents.");
//                 return;
//             }
//
//             // 查找要删除的嵌套预制体（假设名称为 "NestedPrefab"）
//             Transform nestedPrefab = prefabContents.transform.Find("State/DebugUI_State1");
//             if (nestedPrefab != null)
//             {
//                 // 删除嵌套预制体
//                 Debug.Log($"Removed nested prefab: {nestedPrefab.name}");
//                 Object.DestroyImmediate(nestedPrefab.gameObject);
//             }
//             else
//             {
//                 Debug.LogWarning("No nested prefab found with the specified name.");
//             }
//
//             // 保存修改并卸载
//             PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
//             PrefabUtility.UnloadPrefabContents(prefabContents);
//
//             Debug.Log("Prefab modifications saved successfully.");
// #endif     
//         }
//         #endregion
//
//    
//     }
// }