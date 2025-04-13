// using System.Collections.Generic;
// using System.IO;
// using GameDevKitEditor;
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using TMPro;
// using UnityEditor;
// using UnityEngine;
// using Directory = System.IO.Directory;
//
// [TreeWindow("资源工具/替换字体工具")]
// public class FontToolWindow : OdinEditorWindow
// {
//     [FolderPath]
//     [LabelText("选中目录")]
//     public string DirectoryPath;
//
//     [LabelText("替换的字体")]
//     public TMP_FontAsset UsingTMPFontAsset;
//     
//     [Button("替换字体",ButtonHeight = 50)]
//     public void ReplaceFont()
//     {
//         var prefabs = GetAllGameObjectPrefab(DirectoryPath);
//         foreach (var prefab in prefabs)
//         {
//             var texts = prefab.GetComponentsInChildren<TextMeshProUGUI>();
//             foreach (var text in texts)
//             {
//                 text.font = UsingTMPFontAsset;
//             }
//
//             PrefabUtility.SavePrefabAsset(prefab);
//         }
//         
//     }
//     
//     /// <summary>
//     /// 获取目录下所有的预制体
//     /// </summary>
//     /// <param name="dicPath"></param>
//     /// <returns></returns>
//     private List<GameObject> GetAllGameObjectPrefab(string dicPath)
//     {
//         List<GameObject> prefabs = new List<GameObject>();
//         string[] files=Directory.GetFiles(dicPath, "*.prefab", SearchOption.AllDirectories);
//
//         foreach (var file in files)    
//         {
//             GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file);
//             if(prefab!=null) prefabs.Add(prefab);
//         }
//
//         return prefabs;
//     }
//     
//         [Button("清除字体，剥离子物体",ButtonHeight = 50)]
//         public static void Do()
//         {
//             if (Selection.activeObject is TMP_FontAsset TMPFontAsset)
//             { 
//                 AssetDatabase.RemoveObjectFromAsset(TMPFontAsset.atlasTexture);
//                 AssetDatabase.SaveAssets();
//                 Debug.Log("清除字体成功");
//             }
//         }
//         
//         
//         public Material[] TextMaterials;//所有FontAsset的材质球
//         public Texture2D TextTexture2D;//复制出的图集
//         [Button("替换字体贴图",ButtonHeight = 50)]
//
//         public void ReplaceTextTexture()
//         {
//             //创建用于压缩的png
//             byte[] bytes = TextTexture2D.EncodeToPNG();
//             System.IO.File.WriteAllBytes("Assets/Game/Font/LocalResources/font.png", bytes);
//             var TextPng = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Font/LocalResources/font.png");
//             //批量赋值，或者只用上面的代码创建png，手动赋值
//             foreach (var material in TextMaterials)
//             {
//                 material.SetTexture("_MainTex",TextPng);
//                 EditorUtility.SetDirty(material);
//             }
//             AssetDatabase.Refresh();
//         }
//
// }