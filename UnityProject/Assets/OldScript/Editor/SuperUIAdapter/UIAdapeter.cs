// using System;
// using UnityEngine;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using System.Collections.Generic;
// using System.Reflection;
// using System.Text;
// using System.Linq;
//
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using UnityEditor.Build;
// using UnityEngine.UI;
//
// public class UIChangeMonitor : OdinEditorWindow
// {
//     public GameObject currentPrefab;
//     public GameObject selectedObject;
//
//     public Dictionary<Component, Dictionary<string, PropTypeInfo>> snapshot =
//         new Dictionary<Component, Dictionary<string, PropTypeInfo>>();
//
//     public List<PropertyChange> changedProperties = new List<PropertyChange>();
//
//     public struct PropTypeInfo
//     {
//         public Type fieldType;
//         public string name;
//         public object value;
//         public Type MemberType;
//         public bool IsEnum;
//
//         public object newValue;
//
//         public Component component;
//         public PropTypeInfo(Type fieldType, string name, object value,Component component,object newValue=null)
//         {
//             this.fieldType = fieldType;
//             this.name = name;
//             this.value = value;
//           
//             MemberType = value.GetType();
//             IsEnum = MemberType.IsEnum;
//             this.component = component;
//             this.newValue = newValue;
//         }
//
//         public void GetNewValue()
//         {
//             var newValue = ReflectionUtil.GetComponentSnapshot(component)[name].value;
//             this.newValue = newValue;
//         }
//         public bool IsValueChanged()
//         {
//             return !ReflectionUtil.ValueEquals(value,newValue);
//         }
//
//     }
//
//     public struct PropertyChange
//     {
//         public Component component;
//         public string propertyName;
//         public object oldValue;
//         public object newValue;
//     }
//
//     private void OnEnable()
//     {
//         snapshot.Clear();
//         changedProperties.Clear();
//     }
//
//     [MenuItem("Tools/UI Change Monitor")]
//     public static void ShowWindow()
//     {
//         GetWindow<UIChangeMonitor>("UI Monitor");
//     }
//
//     Vector2 scollPos;
//
//
//     [Button]
//     void OnClickGenerateCode()
//     {
//         //检查变动属性
//         CheckForChanges();
//
//         //生成适配代码
//         GenerateAdaptationCode();
//     }
//
//
//
//     /// <summary>
//     /// 是否是挂载在UI上的Item，不是Clone的
//     /// </summary>
//     /// <returns></returns>
//     public bool IsItemInUIWindow()
//     {
//         if (currentPrefab == null)
//         {
//             return false;
//         }
//         var parentName = currentPrefab.transform.name;
//
//         return parentName.StartsWith("m_item")&& !parentName.Contains("(Clone)");
//     }
//     public GameObject GetUIParent(Transform currentSelected)
//     {
//         //遇到m_item开头，或者Page结尾的，或者UI结尾的，返回
//         while (currentSelected.parent != null)
//         {
//             var parentName = currentSelected.parent.name;
//             //正常情况下，选中m_item开头，说明是要改对应的页面上对应的item
//             //选中m_item开头，并且有(Clone) 说明是要改对应的item
//             if (parentName.StartsWith("m_item") || parentName.EndsWith("Page") || parentName.EndsWith("UI"))
//             {
//                 return currentSelected.parent.gameObject;
//             }
//          
//             currentSelected = currentSelected.parent;
//         }
//     
//         return null;
//     }
//
//     public GameObject FindRootUI(Transform currentSelected)
//     {
//         while (currentSelected.parent != null)
//         {
//             var parentName = currentSelected.parent.name;
//             //正常情况下，选中m_item开头，说明是要改对应的页面上对应的item
//             //选中m_item开头，并且有(Clone) 说明是要改对应的item
//             if (parentName.EndsWith("Page") || parentName.EndsWith("UI"))
//             {
//                 return currentSelected.parent.gameObject;
//             }
//             currentSelected = currentSelected.parent;
//         }
//         return null;
//     }
//     void Update()
//     {
//         GameObject currentSelectedParent = null;
//         var currentSelected = Selection.activeGameObject;
//
//         if (currentSelected == null)
//         {
//             return;
//         }
//         currentSelectedParent = GetUIParent(currentSelected.transform);
//
//         if (currentSelectedParent != currentPrefab)
//         {
//             changedProperties.Clear();
//             snapshot.Clear();
//             Debug.Log("Select Root Changed");
//         }
//         
//         currentPrefab = currentSelectedParent;
//         
//         
//         if (currentSelected != selectedObject)
//         {
//             selectedObject = currentSelected;
//             
//             //清楚没有变动的属性
//             CheckClearNoChangeProperty();
//             changedProperties.Clear();
//             CaptureComponentSnapshots();
//         }
//     }
//
//     void CheckClearNoChangeProperty()
//     {
//         //清楚没有变动的属性
//         snapshot = snapshot.Where(e =>
//         {
//             var comp = e.Key;
//             var snapshotDict = e.Value;
//             foreach (var kv in snapshotDict)
//             {
//                 if (!ReflectionUtil.ValueEquals(kv.Value.value,
//                         ReflectionUtil.GetComponentSnapshot(comp)[kv.Key].value))
//                 {
//                     return true;
//                 }
//             }
//             return false;
//         }).ToDictionary(k => k.Key, v => v.Value);
//
//     }
//     void CaptureComponentSnapshots()
//     {
//         if (selectedObject == null) return;
//
//         foreach (var comp in selectedObject.GetComponents<Component>())
//         {
//             //如果已经有了快照，则不再重新生成
//             var snapshotDict = ReflectionUtil.GetComponentSnapshot(comp);
//             if (!snapshot.ContainsKey(comp))
//             {
//                 snapshot.Add(comp, snapshotDict);
//             }
//             else
//             {
//                 //只添加多的部分
//                 foreach (var kv in snapshotDict)
//                 {
//                     if (!snapshot[comp].ContainsKey(kv.Key))
//                     {
//                         snapshot[comp].Add(kv.Key, kv.Value);
//                     }
//                 }
//             }
//         }
//     }
//
//     void CheckForChanges()
//     {
//         changedProperties.Clear();
//         if (selectedObject == null) return;
//         foreach (var keyValuePair in snapshot)
//         {
//             var comp = keyValuePair.Key;
//             var snapshotDict = keyValuePair.Value;
//             foreach (var kv in snapshotDict)
//             {
//                 if (!ReflectionUtil.ValueEquals(kv.Value.value, ReflectionUtil.GetComponentSnapshot(comp)[kv.Key].value))
//                 {
//                     changedProperties.Add(new PropertyChange
//                     {
//                         component = comp,
//                         propertyName = kv.Key,
//                         oldValue = snapshotDict[kv.Key].value,
//                         newValue = ReflectionUtil.GetComponentSnapshot(comp)[kv.Key].value
//                     });
//                 }
//             }
//         }
//     }
//
//     void GenerateAdaptationCode()
//     {
//         if (changedProperties.Count == 0)
//         {
//             Debug.Log("No changes detected!");
//             return;
//         }
//
//         var sb = new StringBuilder();
//         sb.AppendLine("if (BaseConfigInfo.LocalAreaType == LocalAreaType.THA)");
//         sb.AppendLine("{");
//
//         
//         CheckClearNoChangeProperty();
//         
//         foreach (var (key, value) in snapshot)
//         {
//             var comp = key;
//             Debug.Log($"{comp.GetType().Name} Changes");
//             GeneratePropertyCode(comp.gameObject,comp,value, sb);
//         }
//         
//         sb.AppendLine("}");
//         sb.Replace("dodtext", "text");
//         GUIUtility.systemCopyBuffer = sb.ToString();
//         Debug.Log("Adaptation code copied to clipboard!");
//     }
//
//     void GeneratePropertyCode(GameObject selectGameObjectName,Component comp,
//         Dictionary<string, PropTypeInfo> propTypeInfos, StringBuilder sb)
//     {
//         var dicWidget = ScriptGenerator.dicWidget;
//         bool IsHasWidget = false;
//         string NameType = "";
//         bool IsStarM = selectGameObjectName.name.StartsWith("m_");
//         if (IsStarM)
//         {
//             foreach (var keyValuePair in dicWidget)
//             {
//                 if (selectGameObjectName.name.Contains(keyValuePair.Key))
//                 {
//                     IsHasWidget = true;
//                     NameType = keyValuePair.Value;
//                     break;
//                 }
//             }
//         }
//
//         var componentTypeName = comp.GetType().Name;
//         
//         Dictionary<string,string> dic = new Dictionary<string, string>()
//         {
//             {"DodText","Text"},
//             {"DodTextMeshProUGUI","TextMeshProUGUI"},
//             {"DodImage","Image"},
//         };
//         if (dic.TryGetValue(componentTypeName, out var value))
//         {
//             componentTypeName = value;
//         }
//         bool IsSameType = componentTypeName == NameType;
//         //改成驼峰命名
//         string varName =  componentTypeName.Substring(0, 1).ToLower() + componentTypeName.Substring(1);
//         Transform parent = GetUIParent(comp.transform).transform;
//  
//
//         string accessPath = "";
//
//         bool NeedCheckNull = true;
//         if (IsItemInUIWindow())
//         {
//             parent =  FindRootUI(comp.transform)?.transform;
//             accessPath = $"FindChildComponent<{comp.GetType().Name}>(\"{GetHierarchyPath(comp.transform, parent)}\")";
//             IsSameType = false;
//         }
//         else if (componentTypeName == "RectTransform"&& (NameType == "Text" || NameType == "Image"|| NameType == "DodImage"))
//         {
//             accessPath = $"{ comp.gameObject.name }.rectTransform";
//             NeedCheckNull = false;
//         }
//         else if (IsStarM)
//         {
//             accessPath = $"{ comp.gameObject.name }.GetComponent<{componentTypeName}>()";
//         }
//         else
//         {
//             accessPath = IsSameType
//                 ? comp.gameObject.name
//                 : $"FindChildComponent<{comp.GetType().Name}>(\"{GetHierarchyPath(comp.transform, parent)}\")";
//         }
//          
//         // 生成组件获取代码
//         if (!IsSameType)
//         {
//             sb.AppendLine($"    var {varName} = {accessPath};");
//             if (NeedCheckNull)
//             {
//                 sb.AppendLine($"    if ({varName} != null)");
//                 sb.AppendLine("    {");
//             }
//         }
//         else
//         {
//             varName = selectGameObjectName.name;
//         }
//         // 修改propTypeInfos的newValue值
//         var list = propTypeInfos.Keys.ToList();
//         for (int i = 0; i < list.Count; i++)
//         {
//             var key = list[i];
//             var propTypeInfo = propTypeInfos[key];
//             propTypeInfo.GetNewValue();
//             propTypeInfos[key] = propTypeInfo;
//         }
//         foreach (var keyValuePair in propTypeInfos)
//         {
//             if (propTypeInfos[keyValuePair.Key].IsValueChanged())
//             {
//                 GenerateMemberCode(varName, keyValuePair.Value, sb);
//             }
//         }
//
//         if (!IsSameType&& NeedCheckNull)
//             sb.AppendLine("    }");
//     }
//
//     string GetHierarchyPath(Transform tr, Transform root)
//     {
//         var path = new List<string>();
//         while (tr != null && tr != root)
//         {
//             path.Add(tr.name);
//             tr = tr.parent;
//         }
//
//         path.Reverse();
//         return string.Join("/", path);
//     }
//     // 判断组件是否处于原生尺寸，支持 Image 和 RawImage
//     public static bool IsNativeSize(Image image, float tolerance = 0.1f)
//     {
//
//         Vector2 calculatedNativeSize = Vector2.zero;
//
//         // 处理 Image 组件
//         
//         if (image.sprite == null) return false;
//         
//      
//         calculatedNativeSize = new Vector2(
//             image.sprite.rect.width ,
//             image.sprite.rect.height
//         );
//         
//       
//         // 获取当前 RectTransform 的实际尺寸
//         RectTransform rectTransform = image.rectTransform;
//         Vector2 currentSize = rectTransform.sizeDelta;
//         // 允许微小误差（默认 0.1 单位）
//         return Mathf.Approximately(currentSize.x, calculatedNativeSize.x) && 
//                Mathf.Approximately(currentSize.y, calculatedNativeSize.y);
//     }
//     
//     void GenerateMemberCode(string accessor, PropTypeInfo propTypeInfo, StringBuilder sb)
//     {
//         var fieldType = propTypeInfo.MemberType;
//         var value = propTypeInfo.newValue;
//         
//         if (propTypeInfo.IsEnum)
//         {
//             sb.AppendLine($"    {accessor}.{propTypeInfo.name} = {propTypeInfo.MemberType.Name}.{value};");
//         }
//         else if(
//             propTypeInfo.component.gameObject.name.StartsWith("m_img")&&
//                 propTypeInfo.name == "sizeDelta"
//                 &&propTypeInfo.component.TryGetComponent(out Image image)&&
//                 IsNativeSize(image))
//         {
//             sb.AppendLine($"    {propTypeInfo.component.gameObject.name}.SetNativeSize();");
//         }
//         else if (fieldType == typeof(Vector2))
//         {
//             var vec = (Vector2)value;
//             //如果小数部分是0，则不显示小数部分
//             sb.AppendLine($"    {accessor}.{propTypeInfo.name} = new Vector2({FormatFloat(vec.x)}, {FormatFloat(vec.y)});");
//         }
//         else if (fieldType == typeof(float))
//         {
//             //保留两位小数
//             float f = (float)value;
//             sb.AppendLine($"    {accessor}.{propTypeInfo.name} = {FormatFloat(f)};");
//         }
//         else if (fieldType == typeof(RectOffset))
//         {
//             var rectOffset = (RectOffset)value;
//             sb.AppendLine(
//                 $"    {accessor}.{propTypeInfo.name} = new RectOffset({rectOffset.left}, {rectOffset.right}, {rectOffset.top}, {rectOffset.bottom});");
//         }
//         else
//         {
//             sb.AppendLine($"    {accessor}.{propTypeInfo.name} = {FormatValue(value)};");
//         }
//     } 
//     string FormatFloat(float number)
//     {
//         // 分离整数部分和小数部分
//         int integerPart = (int)number;
//         float decimalPart = number - integerPart;
//
//         // 判断小数部分是否小于0.1
//         if (decimalPart < 0.1f)
//         {
//             return integerPart.ToString();
//         }
//         else
//         {
//             // 保留两位小数
//             return number.ToString("0.00")+"f";
//         }
//     }
//     string FormatValue(object value)
//     {
//         if (value is string) return $"\"{value}\"";
//         if (value is bool) return value.ToString().ToLower();
//         return value.ToString();
//     }
// }
//
// public static class ReflectionUtil
// {
//     // 需要监听的属性白名单（类型名 + 属性名）
//     private static readonly HashSet<string> monitoredProperties = new HashSet<string>
//     {
//         // RectTransform
//         "RectTransform.anchoredPosition",
//         // "RectTransform.offsetMin",
//         // "RectTransform.offsetMax",
//         "RectTransform.anchorMin",
//         "RectTransform.anchorMax",
//         "RectTransform.sizeDelta",
//         "RectTransform.pivot",
//         //scale
//         "RectTransform.localScale",
//
//         // Text
//         "Text.fontSize",
//         "Text.lineSpacing",
//         "Text.alignment",
//         "Text.resizeTextForBestFit",
//         "Text.horizontalOverflow",
//         "Text.verticalOverflow",
//         "Text.alignment",
//
//         "DodText.fontSize",
//         "DodText.lineSpacing",
//         "DodText.alignment",
//         "DodText.resizeTextForBestFit",
//         "DodText.horizontalOverflow",
//         "DodText.verticalOverflow",
//         "DodText.alignment",
//
//
//         //HorizontalLayoutGroup
//         "HorizontalLayoutGroup.spacing",
//         "HorizontalLayoutGroup.padding",
//         "HorizontalLayoutGroup.childControlWidth",
//         "HorizontalLayoutGroup.childControlHeight",
//         "HorizontalLayoutGroup.childForceExpandWidth",
//         "HorizontalLayoutGroup.childForceExpandHeight",
//         "HorizontalLayoutGroup.childAlignment",
//
//
//         //VerticalLayoutGroup
//         "VerticalLayoutGroup.spacing",
//         "VerticalLayoutGroup.padding",
//         "VerticalLayoutGroup.childControlWidth",
//         "VerticalLayoutGroup.childControlHeight",
//         "VerticalLayoutGroup.childForceExpandWidth",
//         "VerticalLayoutGroup.childForceExpandHeight",
//         "VerticalLayoutGroup.childAlignment",
//
//         //GridLayoutGroup
//         "GridLayoutGroup.cellSize",
//         "GridLayoutGroup.padding",
//         "GridLayoutGroup.spacing",
//         "GridLayoutGroup.constraint",
//         "GridLayoutGroup.constraintCount",
//         "GridLayoutGroup.startCorner",
//         "GridLayoutGroup.startAxis",
//         "GridLayoutGroup.childAlignment",
//     };
//
//     public static Dictionary<string, UIChangeMonitor.PropTypeInfo> GetComponentSnapshot(Component comp)
//     {
//         var snapshot = new Dictionary<string, UIChangeMonitor.PropTypeInfo>();
//         var type = comp.GetType();
//
//         var componentTypeName = type.Name;
//         UIChangeMonitor.PropTypeInfo propTypeInfo;
//         foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
//         {
//             if (!monitoredProperties.Contains(componentTypeName + "." + field.Name))
//             {
//                 continue;
//             }
//
//             if (field.IsDefined(typeof(ObsoleteAttribute))) continue;
//             propTypeInfo = new UIChangeMonitor.PropTypeInfo(field.FieldType, field.Name, field.GetValue(comp),comp);
//             snapshot[field.Name] = propTypeInfo;
//         }
//
//         foreach (var field in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
//         {
//             if (!monitoredProperties.Contains(componentTypeName + "." + field.Name))
//             {
//                 continue;
//             }
//
//             if (field.IsDefined(typeof(ObsoleteAttribute))) continue;
//             propTypeInfo = new UIChangeMonitor.PropTypeInfo(field.PropertyType, field.Name, field.GetValue(comp),comp);
//             snapshot[field.Name] = propTypeInfo;
//         }
//
//
//         return snapshot;
//     }
//
//
//     public static bool ValueEquals(object a, object b)
//     {
//         
//         if (a == null || b == null) return a == b;
//        
//         //如果是float ,要看浮点差异
//         if (a is float fA && b is float fB) return Math.Abs(fA - fB) < 0.01f;
//
//         if (a is Vector2 vecA && b is Vector2 vecB)
//         {
//             return Math.Abs(vecA.x - vecB.x) < 0.01f && Math.Abs(vecA.y - vecB.y) < 0.01f;
//         }
//
//         return a.Equals(b);
//     }
// }