#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using GameLogic.Editor.ConfigEditor.Schema;

namespace GameLogic.Editor
{
    /// <summary>
    /// 引用对象窗口：显示被引用的Excel数据行
    /// </summary>
    public class ReferencedObjectWindow : EditorWindow
    {
        private int _refId;
        private string _refTableName;
        private string _selectedFolder;
        private Dictionary<string, ExcelEditorHelper.TableMapping> _tableMappings;
        private string _sourceExcelPath;
        
        private ExcelPackage _currentPackage;
        private ExcelWorksheet _currentWorksheet;
        private string _currentExcelPath;
        private TableSchema _currentTableSchema;
        private List<ExcelEditorHelper.FieldSchemaWithIndex> _fieldDefinitions;
        private Dictionary<string, object> _referencedObject;
        private int _referencedRow;
        
        private Vector2 _scrollPosition;
        private bool _isDirty = false;
        
        public static void OpenWindow(int refId, string refTableName, string selectedFolder, 
            Dictionary<string, ExcelEditorHelper.TableMapping> tableMappings, string sourceExcelPath)
        {
            var window = GetWindow<ReferencedObjectWindow>(true, $"引用对象: {refTableName}#{refId}", true);
            window._refId = refId;
            window._refTableName = refTableName;
            window._selectedFolder = selectedFolder;
            window._tableMappings = tableMappings;
            window._sourceExcelPath = sourceExcelPath;
            window.LoadReferencedObject();
        }
        
        private void OnEnable()
        {
            // 设置EPPlus许可证
            try
            {
                ExcelPackage.License.SetNonCommercialOrganization("AION.CoreGame");
            }
            catch
            {
                // 旧版本EPPlus可能不支持，忽略
            }
        }
        
        private void OnDisable()
        {
            CloseCurrentExcel();
        }
        
        private void OnGUI()
        {
            if (_referencedObject == null)
            {
                EditorGUILayout.HelpBox($"未找到引用对象: {_refTableName}#{_refId}", MessageType.Warning);
                if (GUILayout.Button("关闭"))
                {
                    Close();
                }
                return;
            }
            
            EditorGUILayout.BeginVertical();
            
            // 工具栏
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"引用对象: {_refTableName}#{_refId}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            
            if (_currentPackage != null)
            {
                if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    SaveReferencedObject();
                }
                
                if (_isDirty)
                {
                    EditorGUILayout.LabelField("*", GUILayout.Width(10));
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 显示对象数据
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.LabelField("对象数据", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            foreach (var fieldDefWithIndex in _fieldDefinitions)
            {
                var fieldDef = fieldDefWithIndex.Field;
                if (!_referencedObject.ContainsKey(fieldDef.Name))
                {
                    continue;
                }
                
                var value = _referencedObject[fieldDef.Name];
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(fieldDef.DisplayName ?? fieldDef.Name, EditorStyles.boldLabel);
                if (!string.IsNullOrEmpty(fieldDef.Comment))
                {
                    EditorGUILayout.LabelField(fieldDef.Comment, EditorStyles.miniLabel);
                }
                
                DrawFieldValue(fieldDef, value);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawFieldValue(FieldSchema fieldDef, object value)
        {
            string typeStr = fieldDef.RawType ?? fieldDef.Type ?? "";
            bool isList = typeStr.Contains("list");
            bool isPath = (!string.IsNullOrEmpty(fieldDef.Comment) && fieldDef.Comment.Contains("路径")) ||
                          (!string.IsNullOrEmpty(fieldDef.DisplayName) && fieldDef.DisplayName.Contains("路径"));
            
            if (value is List<int> list)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"列表 ({list.Count} 项)", EditorStyles.miniLabel);
                
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    list.Add(0);
                    _isDirty = true;
                }
                
                EditorGUILayout.EndHorizontal();
                
                for (int i = 0; i < list.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));
                    int newVal = EditorGUILayout.IntField(list[i]);
                    if (newVal != list[i])
                    {
                        list[i] = newVal;
                        _isDirty = true;
                    }
                    
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        list.RemoveAt(i);
                        _isDirty = true;
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            else if (value is int intVal)
            {
                int newVal = EditorGUILayout.IntField(intVal);
                if (newVal != intVal)
                {
                    _referencedObject[fieldDef.Name] = newVal;
                    _isDirty = true;
                }
            }
            else if (value is float floatVal)
            {
                float newVal = EditorGUILayout.FloatField(floatVal);
                if (newVal != floatVal)
                {
                    _referencedObject[fieldDef.Name] = newVal;
                    _isDirty = true;
                }
            }
            else if (value is bool boolVal)
            {
                bool newVal = EditorGUILayout.Toggle(boolVal);
                if (newVal != boolVal)
                {
                    _referencedObject[fieldDef.Name] = newVal;
                    _isDirty = true;
                }
            }
            else
            {
                string strVal = value?.ToString() ?? "";
                EditorGUILayout.BeginHorizontal();
                string newVal = EditorGUILayout.TextField(strVal);
                if (newVal != strVal)
                {
                    _referencedObject[fieldDef.Name] = newVal;
                    _isDirty = true;
                }
                
                if (isPath && !string.IsNullOrEmpty(newVal))
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(60)))
                    {
                        PingAsset(newVal);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void PingAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }
            
            if (!assetPath.StartsWith("Assets/"))
            {
                assetPath = "Assets/" + assetPath.TrimStart('/');
            }
            
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
                Debug.Log($"已 Ping 资源: {assetPath}");
            }
            else
            {
                EditorUtility.DisplayDialog("提示", $"未找到资源: {assetPath}", "确定");
                Debug.LogWarning($"未找到资源: {assetPath}");
            }
        }
        
        private void LoadReferencedObject()
        {
            // 查找引用对象所在的Excel文件
            string excelPath = FindReferencedExcel();
            if (string.IsNullOrEmpty(excelPath))
            {
                Debug.LogError($"未找到引用表 {_refTableName} 对应的Excel文件");
                return;
            }
            
            try
            {
                var fileInfo = new FileInfo(excelPath);
                _currentPackage = new ExcelPackage(fileInfo);
                _currentExcelPath = excelPath;
                
                // 查找工作表
                string sheetName = FindSheetName();
                if (string.IsNullOrEmpty(sheetName))
                {
                    _currentWorksheet = _currentPackage.Workbook.Worksheets.FirstOrDefault();
                }
                else
                {
                    _currentWorksheet = _currentPackage.Workbook.Worksheets[sheetName];
                }
                
                if (_currentWorksheet == null)
                {
                    Debug.LogError($"未找到工作表: {sheetName ?? "默认"}");
                    return;
                }
                
                // 解析表结构
                _currentTableSchema = ExcelEditorHelper.ParseTableSchema(excelPath, _currentWorksheet.Name);
                if (_currentTableSchema == null)
                {
                    Debug.LogError($"无法解析表结构: {excelPath}");
                    return;
                }
                
                _fieldDefinitions = ExcelEditorHelper.ConvertSchemaToFieldList(_currentTableSchema);
                
                // 查找引用ID对应的行
                _referencedRow = FindRowById(_refId);
                if (_referencedRow <= 0)
                {
                    Debug.LogWarning($"未找到ID为 {_refId} 的行");
                    return;
                }
                
                // 加载对象数据
                _referencedObject = LoadObjectFromRow(_referencedRow);
                
                Debug.Log($"已加载引用对象: {_refTableName}#{_refId}, 行: {_referencedRow}");
            }
            catch (Exception e)
            {
                Debug.LogError($"加载引用对象失败: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private string FindReferencedExcel()
        {
            // 查找映射中对应的Excel文件
            foreach (var mapping in _tableMappings.Values)
            {
                string tableKey = mapping.FullName.Split('.').LastOrDefault()?.ToLower();
                if (tableKey == _refTableName.ToLower())
                {
                    // 解析Input字段获取Excel文件路径
                    string input = mapping.Input;
                    if (input.Contains("@"))
                    {
                        var parts = input.Split('@');
                        if (parts.Length >= 2)
                        {
                            string fileName = parts[1].Trim();
                            return Path.Combine(_selectedFolder, fileName);
                        }
                    }
                    else
                    {
                        // 直接是文件名
                        return Path.Combine(_selectedFolder, input);
                    }
                }
            }
            
            return null;
        }
        
        private string FindSheetName()
        {
            foreach (var mapping in _tableMappings.Values)
            {
                string tableKey = mapping.FullName.Split('.').LastOrDefault()?.ToLower();
                if (tableKey == _refTableName.ToLower())
                {
                    string input = mapping.Input;
                    if (input.Contains("@"))
                    {
                        var parts = input.Split('@');
                        if (parts.Length >= 2)
                        {
                            return parts[0].Trim();
                        }
                    }
                }
            }
            
            return null;
        }
        
        private int FindRowById(int id)
        {
            if (_currentWorksheet == null || _fieldDefinitions == null || _fieldDefinitions.Count == 0)
            {
                return 0;
            }
            
            // 假设第一列是ID列（通常是第一个字段）
            var idField = _fieldDefinitions.FirstOrDefault();
            if (idField == null)
            {
                return 0;
            }
            
            int dataStartRow = _currentTableSchema?.DataStartRow ?? 5;
            var dimension = _currentWorksheet.Dimension;
            if (dimension == null)
            {
                return 0;
            }
            
            for (int row = dataStartRow; row <= dimension.End.Row; row++)
            {
                var cellValue = _currentWorksheet.Cells[row, idField.ColumnIndex].Value;
                if (cellValue != null)
                {
                    if (int.TryParse(cellValue.ToString(), out int rowId) && rowId == id)
                    {
                        return row;
                    }
                }
            }
            
            return 0;
        }
        
        private Dictionary<string, object> LoadObjectFromRow(int row)
        {
            var data = new Dictionary<string, object>();
            
            if (_fieldDefinitions == null || _fieldDefinitions.Count == 0)
            {
                return data;
            }
            
            foreach (var fieldDefWithIndex in _fieldDefinitions)
            {
                var fieldDef = fieldDefWithIndex.Field;
                var cellValue = _currentWorksheet.Cells[row, fieldDefWithIndex.ColumnIndex].Value;
                
                string typeStr = fieldDef.RawType ?? fieldDef.Type ?? "";
                var convertedValue = ConvertValue(cellValue, typeStr);
                data[fieldDef.Name] = convertedValue;
            }
            
            return data;
        }
        
        private object ConvertValue(object value, string typeName)
        {
            if (value == null)
            {
                return null;
            }
            
            if (string.IsNullOrEmpty(typeName))
            {
                return value?.ToString() ?? "";
            }
            
            if (typeName.Contains("list"))
            {
                var str = value?.ToString() ?? "";
                if (string.IsNullOrEmpty(str))
                {
                    return new List<int>();
                }
                
                char separator = ';';
                if (typeName.Contains("#sep="))
                {
                    var sepIndex = typeName.IndexOf("#sep=");
                    var sepStr = typeName.Substring(sepIndex + 5, 1);
                    separator = sepStr[0];
                }
                
                var parts = str.Split(separator);
                var list = new List<int>();
                foreach (var part in parts)
                {
                    if (int.TryParse(part.Trim(), out int intVal))
                    {
                        list.Add(intVal);
                    }
                }
                return list;
            }
            
            if (typeName.Contains("int"))
            {
                if (int.TryParse(value.ToString(), out int intVal))
                {
                    return intVal;
                }
            }
            else if (typeName.Contains("string"))
            {
                return value?.ToString() ?? "";
            }
            else if (typeName.Contains("bool"))
            {
                var str = value?.ToString()?.ToUpper() ?? "";
                return str == "TRUE" || str == "1" || str == "YES";
            }
            else if (typeName.Contains("float"))
            {
                if (float.TryParse(value.ToString(), out float floatVal))
                {
                    return floatVal;
                }
            }
            
            return value?.ToString() ?? "";
        }
        
        private void SaveReferencedObject()
        {
            if (_currentPackage == null || _referencedObject == null || _referencedRow <= 0)
            {
                return;
            }
            
            try
            {
                // 保存对象数据到Excel
                foreach (var fieldDefWithIndex in _fieldDefinitions)
                {
                    var fieldDef = fieldDefWithIndex.Field;
                    if (!_referencedObject.ContainsKey(fieldDef.Name))
                    {
                        continue;
                    }
                    
                    var value = _referencedObject[fieldDef.Name];
                    string strValue = "";
                    
                    if (value is List<int> list)
                    {
                        char separator = ';';
                        string typeStr = fieldDef.RawType ?? fieldDef.Type;
                        if (typeStr.Contains("#sep="))
                        {
                            var sepIndex = typeStr.IndexOf("#sep=");
                            var sepStr = typeStr.Substring(sepIndex + 5, 1);
                            separator = sepStr[0];
                        }
                        strValue = string.Join(separator.ToString(), list);
                    }
                    else
                    {
                        strValue = value?.ToString() ?? "";
                    }
                    
                    _currentWorksheet.Cells[_referencedRow, fieldDefWithIndex.ColumnIndex].Value = strValue;
                }
                
                var fileInfo = new FileInfo(_currentExcelPath);
                _currentPackage.SaveAs(fileInfo);
                _isDirty = false;
                
                Debug.Log($"已保存引用对象: {_refTableName}#{_refId}");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"保存失败: {e.Message}", "确定");
                Debug.LogError($"保存引用对象失败: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void CloseCurrentExcel()
        {
            if (_currentPackage != null)
            {
                _currentPackage.Dispose();
                _currentPackage = null;
            }
            
            _currentWorksheet = null;
            _currentExcelPath = "";
        }
    }
}
#endif

