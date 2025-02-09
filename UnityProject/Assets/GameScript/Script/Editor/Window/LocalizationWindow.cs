// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text;
// using System.Text.RegularExpressions;
// using Editor;
// using GameScripts.Editor;
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using Sirenix.Utilities;
// using TMPro;
// using TMPro.EditorUtilities;
// using UnityEditor;
// using UnityEngine;
//
// namespace GameDevKitEditor
// {
//     [TreeWindow("UI工具/本地化工具")]
//     public class LocalizationWindow : OdinEditorWindow
//     {
//         [TableList] public List<TextMeshData> textMeshList;
//
//         [ButtonGroup("Localization")]
//         [Button("打开本地化Localization表", ButtonSizes.Large)]
//         public void OpenLocalizationTable()
//         {
//             EditorUtility.OpenWithDefaultApp(System.Environment.CurrentDirectory + @"\Luban\Datas\Localization.xlsx");
//         }
//      
//         [ButtonGroup("Localization")]
//         [Button("打开本地化Localization_CodeElk表", ButtonSizes.Large)]
//         public void OpenLocalization_CodeElk()
//         {
//             EditorUtility.OpenWithDefaultApp(System.Environment.CurrentDirectory + @"\Luban\Datas\Localization_CodeElk.xlsx");
//         }
//
//         [ButtonGroup("Localization")]
//         [Button("刷新本地化表", ButtonSizes.Large)]
//         public void RefreshLocalizationTable()
//         {
//             LubanTools.BuildLubanExcel();
//         }
//
//         [ButtonGroup("Font")]
//         [Button("获取所有字体", ButtonSizes.Large)]
//         public void GetAllTextMesh()
//         {
//             // Initialize list
//             textMeshList = new List<TextMeshData>();
//             // Find all TextMesh components under the Canvas object
//             var selection = Selection.activeGameObject;
//             if (selection != null)
//             {
//                 TextMeshProUGUI[] textMeshes = selection.GetComponentsInChildren<TextMeshProUGUI>(true);
//                 foreach (TextMeshProUGUI textMesh in textMeshes)
//                 {
//                     textMeshList.Add(new TextMeshData(textMesh));
//                 }
//             }
//         }
//         
//         [ButtonGroup("Font")]
//         [Button("替换Text", ButtonSizes.Large)]
//         public void ReplaceText()
//         {
//             StringBuilder sb = new StringBuilder();
//
//             var hashSet = new HashSet<string>();
//             // Find all TextMeshProUGUI components under the Canvas object
//             foreach (TextMeshData textMesh in textMeshList)
//             {
//                
//                 //Debug.Log($"{textMesh.text}  {Regex.IsMatch(textMesh.text, @"[\u4e00-\u9fa5]")}");
//                 // 这个正则表达式[\u4e00-\u9fa5](?![^{]*})会匹配所有的汉字，但不包含大括号{或}。
//                 if (Regex.IsMatch(textMesh.text, @"[\u4e00-\u9fa5]"))
//                 {
//                     if (textMesh.text.Contains("{"))
//                     {
//                         continue;
//                     }
//
//                     string replaceText = textMesh.text;
//                     // If the text contains numbers, replace them with {0}, {1}, ...
//                     if (Regex.IsMatch(textMesh.text, @"\d"))
//                     {
//                         int count = 0;
//                         replaceText = Regex.Replace(textMesh.text, @"\d+", m => "{" + (count++) + "}");
//                        
//                     }
//                  
//                     string key = Selection.activeGameObject.name + "." + textMesh.name.Split('_').Last();
//                     // Add the replaced text to the StringBuilder
//                     textMesh.text = key;
//                         
//                     if (hashSet.Contains(key))
//                     {
//                         continue;
//                     }
//                     //replaceText去除换行
//                     
//                     replaceText =replaceText.Replace("\n", "");
//                     sb.AppendLine($"{key}\t{replaceText}");
//                     hashSet.Add(key);
//                 }
//                 
//             }
//
//             // Copy the StringBuilder to the clipboard
//             GUIUtility.systemCopyBuffer = sb.ToString();
//             Save();
//         }
//         // [ButtonGroup("Font")]
//         // [Button("保存", ButtonSizes.Large)]
//         public void Save()
//         {
//             foreach (TextMeshData data in textMeshList)
//             {
//                 data.textMesh.text = data.text;
//                 EditorUtility.SetDirty(data.textMesh.gameObject);
//             }
//         }
//         [ButtonGroup("Font")]
//         [Button("设置TextMeshProUGUI排列方式", ButtonSizes.Large)]
//         public void SetTextMeshProUGUIAlignment()
//         {
//             foreach (TextMeshData data in textMeshList)
//             {
//                 SetTextMeshProUGUIAlignment(data.textMesh);
//             }
//         }  
//         [ButtonGroup("Font")]
//         [Button("设置字体宽度", ButtonSizes.Large)]
//         public void SetTextWidth()
//         {
//             foreach (TextMeshData data in textMeshList)
//             {
//                 var size = data.textMesh.GetComponent<RectTransform>().sizeDelta;
//                 data.textMesh.GetComponent<RectTransform>().sizeDelta = new Vector2(500, size.y);
//                 EditorUtility.SetDirty(data.textMesh.gameObject);
//             }
//         }
//
//         private void SetTextMeshProUGUIAlignment(TextMeshProUGUI textMesh)
//         {
//             if (textMesh != null)
//             {
//                 textMesh.alignment = TextAlignmentOptions.Left;
//                 textMesh.enableAutoSizing = true;
//                 textMesh.enableWordWrapping = true;
//                 textMesh.overflowMode = TextOverflowModes.Overflow;
//                 // textMesh.verti = VerticalWrapMode.Overflow;
//                 // textMesh.horizontalOverflow = HorizontalWrapMode.Wrap;
//                 textMesh.lineSpacing = 1f; // 设置行间距
//             }
//         }
//
//         [ButtonGroup("Font")]
//         [Button("获取TextMeshProUGUI全局设置", ButtonSizes.Large)]
//         public void GetGlobalTextMeshSettings()
//         {
//             TMP_Settings settings = TMP_Settings.GetSettings();
//             EditorGUIUtility.PingObject(settings);
//             // 其他全局属性...
//         }
//         [ButtonGroup("Font")]
//         [Button("Ping图集文件")]
//         public void PingSpriteAtlas()
//         {
//             // Find all SpriteAtlas objects in the project
//             var folders = AssetDatabase.LoadAssetAtPath<Object>("Assets/Plugins/TextMesh Pro/Resources/Sprite Assets");
//
//             EditorGUIUtility.PingObject(folders);
//         }
//         [ButtonGroup("Font")]
//         [Button("创建SpriteAsset")]
//         public void CreateSpriteAsset()
//         {
//             var directory = IOUtility.GetCurrentAssetDirectory();
//             var guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { directory });
//             var spritePath = "Assets/Plugins/TextMesh Pro/Resources/Sprite Assets";
//             foreach (var s in guids)
//             {
//                 var path = AssetDatabase.GUIDToAssetPath(s);
//                 var sprite = AssetDatabase.LoadAssetAtPath<Texture>(path);
//                 Selection.activeObject = sprite;
//                 TMP_SpriteAssetMenu.CreateSpriteAsset();
//                 var spritePathWithoutExtension = Path.GetFileNameWithoutExtension(path);
//                 AssetDatabase.MoveAsset(Path.Combine(spritePathWithoutExtension, ".asset"), spritePath + "/" + sprite.name + ".asset");
//
//             }
//         }
//
//         [ButtonGroup("Font")]
//         [Button("移动SpriteAsset")]
//         public void MoveSpriteAsset()
//         {
//             var directory = IOUtility.GetCurrentAssetDirectory();
//             var guids = AssetDatabase.FindAssets("t:TMP_SpriteAsset", new string[] { directory });
//             var spritePath = "Assets/Plugins/TextMesh Pro/Resources/Sprite Assets";
//             foreach (var s in guids)
//             {
//                 var path = AssetDatabase.GUIDToAssetPath(s);
//                 var sprite = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(path);
//                 AssetDatabase.MoveAsset(path, spritePath + "/" + sprite.name + ".asset");
//             }
//         }
//         
//         // Custom class to store TextMeshProUGUI data
//         public class TextMeshData
//         {
//             // [TableColumnWidth(250)] public readonly string path;
//             [OnValueChanged("Save")] [TableColumnWidth(150)]
//             public string name;
//
//             [OnValueChanged("Save")] [TableColumnWidth(200)]
//             public string text;
//
//             [OnValueChanged("Save")]
//             // [ValueDropdown("GetColor", AppendNextDrawer = true, IsUniqueList = true)]
//             [ColorPalette("MonoPoly")]
//             [TableColumnWidth(200)]
//             public Color TextColor;
//
//             // [OnValueChanged("Save")]
//             [OnValueChanged("Save")] [ValueDropdown("GetMaterialPresets")] [TableColumnWidth(200)]
//             public string MaterialPreset;
//
//             [TableColumnWidth(50)] public TextMeshProUGUI textMesh;
//
//             private List<string> materialPresets;
//             
//             void Save()
//             {
//                 textMesh.gameObject.name = name;
//                 textMesh.text = text;
//                 textMesh.color = TextColor;
//                 textMesh.fontSharedMaterial = TMP_EditorUtility.FindMaterialReferences(textMesh.font)
//                     .FirstOrDefault(x => x.name == MaterialPreset);
//                 EditorUtility.SetDirty(textMesh.gameObject);
//             }
//
//             public TextMeshData(TextMeshProUGUI textMesh)
//             {
//                 this.textMesh = textMesh;
//                 this.text = textMesh.text;
//                 this.name = textMesh.gameObject.name;
//                 // this.path = GetPath(textMesh.gameObject);
//                 this.TextColor = textMesh.color;
//                 materialPresets = GetMaterialPresets();
//                 MaterialPreset = textMesh.fontSharedMaterial.name;
//             }
//
//             private string GetPath(GameObject obj)
//             {
//                 string path = obj.name;
//                 while (obj.transform.parent != null)
//                 {
//                     obj = obj.transform.parent.gameObject;
//                     path = obj.name + "/" + path;
//                 }
//
//                 return path;
//             }
//
//             protected List<string> GetMaterialPresets()
//             {
//                 TMP_FontAsset fontAsset = textMesh.font;
//                 if (fontAsset == null) return null;
//
//                 var materialReferences = TMP_EditorUtility.FindMaterialReferences(fontAsset);
//
//                 return materialReferences.Select(x => x.name).ToList();
//             }
//         }
//
//         // Automatically select the TextMeshProUGUI object in the hierarchy when its text is changed
//         // [OnValueChanged(nameof(SelectTextMesh))]
//         // public string SelectedText;
//         //
//         // private void SelectTextMesh()
//         // {
//         //     foreach (TextMeshData data in textMeshList)
//         //     {
//         //         if (data.text == SelectedText)
//         //         {
//         //             Selection.activeGameObject = data.textMesh.gameObject;
//         //             break;
//         //         }
//         //     }
//         // }
//     }
// }