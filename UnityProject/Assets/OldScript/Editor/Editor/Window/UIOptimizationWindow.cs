// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using GameDevKitEditor;
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using Sirenix.Utilities;
// using UnityEditor;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// namespace AION.CoreFramework
// {
//  
//
//     [TreeWindow("UI工具")]
//     public class UIOptimizationWindow : OdinEditorWindow
//     {
//         // 提取选中对象并判空的通用逻辑
//         private GameObject GetSelectedGameObject()
//         {
//             GameObject selectedObject = Selection.activeGameObject;
//             if (selectedObject == null)
//             {
//                 Debug.LogWarning("未选中任何游戏对象，请先选择一个游戏对象。");
//             }
//             return selectedObject;
//         }
//
//         [Button("移除图片和文字不必要的RayCasterTarge", ButtonHeight = 50)]
//         public void RemoveRayCasterTarget()
//         {
//             var go = GetSelectedGameObject();
//             if (go == null) return;
//
//             // 获取所有 Button 下的 Graphic 组件
//             var excludeGraphic = new HashSet<Graphic>();
//
//             var buttons = go.GetComponentsInChildren<Button>(true).Select(e => e.targetGraphic);
//
//
//             // 获取所有 ScrollRect 中 Viewport 的 Graphic 组件
//             var scrollRects = go.GetComponentsInChildren<ScrollRect>(true).Select(e => e.viewport.GetComponent<Graphic>());
//
//
//             excludeGraphic.AddRange(buttons);
//             excludeGraphic.AddRange(scrollRects);
//
//             // 获取选中 GameObject 下所有的 Graphic 组件
//             var allGraphics = go.GetComponentsInChildren<Graphic>(true);
//             foreach (var graphic in allGraphics)
//             {
//                 if (!excludeGraphic.Contains(graphic))
//                 {
//                     graphic.raycastTarget = false;
//                     Debug.Log($"禁用raycastTarget: {graphic.gameObject.name}");
//                     EditorUtility.SetDirty(graphic);
//                 }
//             }
//         }
//
//         [Button("移除图片和文字不必要的Mask", ButtonHeight = 50)]
//         public void RemoveMask()
//         {
//             var go = GetSelectedGameObject();
//             if (go == null) return;
//
//             foreach (var componentsInChild in go.GetComponentsInChildren<Image>())
//             {
//                 if (componentsInChild.GetComponent<Button>() != null)
//                 {
//                     continue;
//                 }
//
//                 componentsInChild.maskable = false;
//             }
//             foreach (var componentsInChild in go.GetComponentsInChildren<TextMeshProUGUI>())
//             {
//                 if (componentsInChild.GetComponent<Button>() != null)
//                 {
//                     continue;
//                 }
//
//                 componentsInChild.maskable = false;
//             }
//         }
//
//         [Button("关闭富文本", ButtonHeight = 50)]
//         public void CloseRichTex()
//         {
//             var go = GetSelectedGameObject();
//             if (go == null) return;
//
//             // 获取 Button 下所有 Graphic 组件
//             var graphics = go.GetComponentsInChildren<TextMeshProUGUI>(true);
//
//
//             foreach (var graphic in graphics)
//             {
//                 graphic.richText = false;
//                 Debug.Log($"禁用富文本{graphic.gameObject.name}");
//                 EditorUtility.SetDirty(graphic);
//             }
//         }
//
//         [Button("设置z轴为0", ButtonHeight = 50)]
//         public void SetUIZAxisToZero()
//         {
//             var selectedObject = GetSelectedGameObject();
//             if (selectedObject == null) return;
//
//             var rectTransforms = selectedObject.GetComponentsInChildren<RectTransform>();
//             foreach (var rectTransform in rectTransforms)
//             {
//                 Vector3 position = rectTransform.localPosition;
//                 if (!Mathf.Approximately(position.z, 0))
//                 {
//                     position.z = 0;
//                     rectTransform.localPosition = position;
//                     Debug.Log($"{rectTransform.gameObject.name} 的 Z 轴已设置为 0");
//                     EditorUtility.SetDirty(rectTransform);
//                 }
//             }
//         }
//
//         [Button("去掉预制体上面的黑色遮罩", ButtonHeight = 50)]
//         public void RemoveBlackMask()
//         {
//             if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
//             {
//                 Debug.LogWarning("未选中任何预制体资源，请先选择一个预制体资源。");
//                 return;
//             }
//
//             string selectPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
//             string[] paths = Directory.GetFiles(selectPath, "*.prefab");
//             foreach (var path in paths)
//             {
//                 GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
//                 if (prefab.TryGetComponent<Image>(out var Image))
//                 {
//                     GameObject.DestroyImmediate(Image, true);
//
//                     if (!prefab.TryGetComponent(out UIConfigBindTool uiConfigBindTool))
//                     {
//                         uiConfigBindTool = prefab.AddComponent<UIConfigBindTool>();
//                         uiConfigBindTool.UseDefaultAnimation = false;
//                     }
//
//                     uiConfigBindTool.UIType = UIConfig.UITypes.UIPopWindow;
//                     uiConfigBindTool.UIGroupName = "PopWindow";
//                     uiConfigBindTool.Init();
//                 }
//
//                 PrefabUtility.SavePrefabAsset(prefab);
//             }
//         }
//
//         [Button("设置按钮disable不变色", ButtonHeight = 50)]
//         public void SetButtonDisableColor()
//         {
//             var selectedObject = GetSelectedGameObject();
//             if (selectedObject == null) return;
//
//             var buttons = selectedObject.GetComponentsInChildren<Button>(true);
//             foreach (var button in buttons)
//             {
//                 var colors = button.colors;
//                 colors.disabledColor = button.colors.normalColor;
//                 button.colors = colors;
//                 Debug.Log($"{button.gameObject.name} 的 disable 颜色已设置为 normal 颜色");
//                 EditorUtility.SetDirty(button);
//             }
//         }
//
//         [Button("查找所有的Canvas组件", ButtonHeight = 50)]
//         public void FindCanvas()
//         {
//             var selectedObject = GetSelectedGameObject();
//             if (selectedObject == null) return;
//
//             var canvases = selectedObject.GetComponentsInChildren<Canvas>(true);
//             foreach (var canvas in canvases)
//             {
//                 Debug.Log(canvas.gameObject.name);
//                 EditorGUIUtility.PingObject(canvas.gameObject);
//             }
//         }
//
//         // 替换Mask组件为RectMask2D组件的方法
//         public void ReplaceMasks()
//         {
//             var go = GetSelectedGameObject();
//             if (go == null) return;
//
//             Mask[] masks = go.GetComponentsInChildren<Mask>();
//             foreach (Mask mask in masks)
//             {
//                 RectTransform rectTransform = mask.GetComponent<RectTransform>();
//
//                 // 创建一个新的RectMask2D组件
//                 RectMask2D rectMask2D = mask.gameObject.AddComponent<RectMask2D>();
//
//                 // 移除旧的Mask组件
//                 DestroyImmediate(mask);
//             }
//         }
//
//         // 替换ScrollView中View的Mask组件为RectMask2D组件的方法
//         public void ReplaceScrollViewMasks()
//         {
//             var go = GetSelectedGameObject();
//             if (go == null) return;
//
//             ScrollRect[] scrollRects = go.GetComponentsInChildren<ScrollRect>();
//             foreach (ScrollRect scrollRect in scrollRects)
//             {
//                 Transform viewport = scrollRect.viewport;
//                 if (viewport != null)
//                 {
//                     Mask mask = viewport.GetComponent<Mask>();
//                     if (mask != null)
//                     {
//                         RectTransform rectTransform = mask.GetComponent<RectTransform>();
//
//                         // 创建一个新的RectMask2D组件
//                         RectMask2D rectMask2D = mask.gameObject.AddComponent<RectMask2D>();
//
//                         // 移除旧的Mask组件
//                         DestroyImmediate(mask);
//                     }
//                 }
//             }
//         }
//     }
// }