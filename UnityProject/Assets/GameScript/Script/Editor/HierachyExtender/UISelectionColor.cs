// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
//
// namespace GameDevKitEditor
// {
//     /// <summary>
//     /// 这个配合绑定组件工具显示UI的类型
//     /// </summary>
//     public class UISelectionColor
//     {
//         private static Texture2D Bg;
//         private static GUIStyle style;
//
//         [InitializeOnLoadMethod]
//         static void SetHierarchyWindow()
//         {
//             EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
//         }
//
//         private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
//         {
//             var selectObj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
//             if (selectObj == null)
//             {
//                 return;
//             }
//
//             var bindTool = selectObj.GetComponentInParent<ComponentAutoBindTool>();
//             if (bindTool == null)
//             {
//                 return;
//             }
//
//             if (Bg == null)
//             {
//                 Bg = new Texture2D(256, 256);
//                 for (int y = 0; y < Bg.height; y++)
//                 {
//                     for (int x = 0; x < Bg.width; x++)
//                     {
//                         Bg.SetPixel(x, y, new Color(0, 1, 0, 0.1f)); // 透明度为0.1
//                     }
//                 }
//                 Bg.Apply();
//             }
//
//             style ??= new GUIStyle(GUI.skin.label)
//             {
//                 normal = { textColor = Color.red, background = Bg }
//             };
//
//             if (bindTool.HashComponent(selectObj) || Selection.instanceIDs.Contains(instanceID))
//             {
//                 GUI.Box(selectionRect, "", style);
//                 selectionRect.width = 100;
//
//                 var preIndex = bindTool.GetIndex(selectObj);
//                 if (Selection.instanceIDs.Contains(instanceID))
//                 {
//                     GUILayout.BeginArea(new Rect(selectionRect.x + 300, selectionRect.y, 200, selectionRect.height));
//                     var index = EditorGUILayout.Popup(preIndex, uiTypes.ToArray());
//                     if (index > 0 && index != preIndex)
//                     {
//                         bindTool.UpdateData(selectObj, uiTypes[index]);
//                         EditorUtility.SetDirty(bindTool);
//                     }
//                     GUILayout.EndArea();
//
//                     if (bindTool.HashComponent(selectObj))
//                     {
//                         GUILayout.BeginArea(new Rect(selectionRect.x + 520, selectionRect.y, 100, selectionRect.height));
//                         if (GUILayout.Button("移除"))
//                         {
//                             bindTool.RemoveData(selectObj);
//                         }
//                         GUILayout.EndArea();
//                     }
//                 }
//
//                 GUI.Label(new Rect(selectionRect.x + 140, selectionRect.y, 120, selectionRect.height), $"{uiTypes[preIndex]}");
//             }
//         }
//
//         private static readonly List<string> uiTypes = ComponentAutoBindTool.dicWidget;
//     }
// }
