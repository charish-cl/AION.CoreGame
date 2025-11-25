#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using GameLogic.Editor.ConfigEditor.Schema;

namespace GameLogic.Editor
{
    /// <summary>
    /// 视图模式
    /// </summary>
    public enum ViewMode
    {
        List,   // 列表模式
        Object  // 对象模式
    }
    
    /// <summary>
    /// Excel编辑器窗口
    /// </summary>
    public class ExcelEditorWindow : EditorWindow
    {
        private const string EDITOR_PREF_FOLDER_KEY = "ExcelEditor_SelectedFolder";
        private const string EDITOR_PREF_AUTO_SAVE_KEY = "ExcelEditor_AutoSave";
        private const string EDITOR_PREF_VIEW_MODE_KEY = "ExcelEditor_ViewMode";
        
        private string _selectedFolder = "";
        private List<string> _excelFiles = new List<string>();
        private int _selectedExcelIndex = -1;
        private string _currentExcelPath = "";
        
        // 表格数据
        private ExcelWorksheet _currentWorksheet;
        private ExcelPackage _currentPackage;
        private List<string> _availableSheets = new List<string>();
        private int _selectedSheetIndex = -1;
        private Vector2 _scrollPosition;
        private Vector2 _horizontalScrollPosition;
        
        // 视图模式
        private ViewMode _viewMode = ViewMode.List;
        
        // 表格映射和字段定义
        private Dictionary<string, ExcelEditorHelper.TableMapping> _tableMappings = new Dictionary<string, ExcelEditorHelper.TableMapping>();
        private TableSchema _currentTableSchema = null;
        private List<ExcelEditorHelper.FieldSchemaWithIndex> _fieldDefinitions = new List<ExcelEditorHelper.FieldSchemaWithIndex>();
        private Type _currentConfigType = null;
        private ExcelEditorHelper.TableMapping _currentMapping = null;
        
        // 对象模式数据
        private List<object> _objectList = new List<object>();
        private int _selectedObjectIndex = -1;
        private Vector2 _objectScrollPosition;
        
        // 搜索功能
        private string _searchText = "";
        private List<int> _searchResults = new List<int>(); // 存储匹配的对象索引
        private bool _showSearchResults = false;
        private Vector2 _searchResultsScrollPosition;
        
        // 表格显示参数
        private int _visibleStartRow = 1;
        private int _visibleStartCol = 1;
        private int _visibleRowCount = 50;
        private int _visibleColCount = 20;
        private float ROW_HEIGHT = 20f;
        private float COL_WIDTH = 100f;
        private const float HEADER_HEIGHT = 20f;
        private const float HEADER_WIDTH = 50f;
        private const float MIN_ROW_HEIGHT = 15f;
        private const float MIN_COL_WIDTH = 50f;
        private const float RESIZE_HANDLE_WIDTH = 5f;
        
        // 列宽和行高缓存
        private Dictionary<int, float> _columnWidths = new Dictionary<int, float>();
        private Dictionary<int, float> _rowHeights = new Dictionary<int, float>();
        
        // 拖动调整状态
        private bool _isResizingColumn = false;
        private bool _isResizingRow = false;
        private int _resizingColumnIndex = -1;
        private int _resizingRowIndex = -1;
        private float _resizeStartPos = 0f;
        private float _resizeStartSize = 0f;
        
        // 编辑状态
        private Dictionary<string, string> _cellEditValues = new Dictionary<string, string>();
        private string _editingCellKey = "";
        private bool _isDirty = false;
        private bool _autoSave = false;
        
        [MenuItem("Tools/Excel Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<ExcelEditorWindow>();
            window.titleContent = new GUIContent("Excel编辑器");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        private static bool _licenseInitialized = false;
        
        private void OnEnable()
        {
            // EPPlus 8+ 版本需要使用新的许可证设置方式
            if (!_licenseInitialized)
            {
                try
                {
                    // EPPlus 8+ 新版本API
                    ExcelPackage.License.SetNonCommercialOrganization("AION.CoreGame");
                    _licenseInitialized = true;
                }
                catch
                {
                    // 如果新API不存在，尝试旧版本API（向后兼容）
                    try
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        _licenseInitialized = true;
                    }
                    catch
                    {
                        // 忽略错误，某些版本可能不需要设置
                        _licenseInitialized = true;
                    }
                }
            }
            LoadSavedFolder();
            LoadAutoSaveSetting();
            LoadViewModeSetting();
            LoadTablesMapping();
        }
        
        private void OnDisable()
        {
            CloseCurrentExcel();
        }
        
        private void OnGUI()
        {
            DrawToolbar();
            
            EditorGUILayout.BeginHorizontal();
            
            // 左侧：文件夹和文件列表
            DrawLeftPanel();
            
            // 右侧：表格视图
            DrawRightPanel();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("选择文件夹", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                SelectFolder();
            }
            
            EditorGUILayout.LabelField("文件夹:", GUILayout.Width(50));
            EditorGUILayout.LabelField(_selectedFolder, EditorStyles.miniLabel);
            
            GUILayout.FlexibleSpace();
            
            // 视图模式切换
            EditorGUI.BeginChangeCheck();
            _viewMode = (ViewMode)EditorGUILayout.EnumPopup(_viewMode, EditorStyles.toolbarPopup, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
            {
                SaveViewModeSetting();
                RefreshCurrentData();
            }
            
            // 自动保存开关
            EditorGUI.BeginChangeCheck();
            _autoSave = GUILayout.Toggle(_autoSave, "自动保存", EditorStyles.toolbarButton, GUILayout.Width(80));
            if (EditorGUI.EndChangeCheck())
            {
                SaveAutoSaveSetting();
            }
            
            if (_currentPackage != null)
            {
                if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    SaveCurrentExcel();
                }
                
                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    ReloadCurrentExcel();
                }
                
                if (_isDirty)
                {
                    EditorGUILayout.LabelField("*", GUILayout.Width(10));
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            
            EditorGUILayout.LabelField("Excel文件列表", EditorStyles.boldLabel);
            
            if (string.IsNullOrEmpty(_selectedFolder))
            {
                EditorGUILayout.HelpBox("请先选择包含Excel文件的文件夹", MessageType.Info);
            }
            else
            {
                if (_excelFiles.Count == 0)
                {
                    EditorGUILayout.HelpBox("该文件夹中没有找到Excel文件", MessageType.Warning);
                }
                else
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                    
                    for (int i = 0; i < _excelFiles.Count; i++)
                    {
                        var fileName = Path.GetFileName(_excelFiles[i]);
                        var isSelected = i == _selectedExcelIndex;
                        
                        if (GUILayout.Toggle(isSelected, fileName, EditorStyles.miniButton, GUILayout.Height(25)))
                        {
                            if (!isSelected)
                            {
                                SelectExcel(i);
                            }
                        }
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
            }
            
            // 显示工作表列表
            if (_currentPackage != null && _availableSheets.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("工作表列表", EditorStyles.boldLabel);
                
                for (int i = 0; i < _availableSheets.Count; i++)
                {
                    var isSelected = i == _selectedSheetIndex;
                    var sheetName = _availableSheets[i];
                    
                    if (GUILayout.Toggle(isSelected, sheetName, EditorStyles.miniButton, GUILayout.Height(25)))
                    {
                        if (!isSelected)
                        {
                            SelectSheet(i);
                        }
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();
            
            if (_currentWorksheet == null)
            {
                EditorGUILayout.HelpBox("请从左侧选择一个Excel文件", MessageType.Info);
            }
            else
            {
                if (_viewMode == ViewMode.List)
                {
                    // 列表模式：直接显示表格，不需要映射关系
                    DrawExcelTable();
                }
                else
                {
                    // 对象模式：需要映射关系和字段定义
                    DrawObjectView();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawExcelTable()
        {
            var dimension = _currentWorksheet.Dimension;
            if (dimension == null)
            {
                EditorGUILayout.HelpBox("该工作表为空", MessageType.Info);
                return;
            }
            
            int maxRow = dimension.End.Row;
            int maxCol = dimension.End.Column;
            
            // 工具栏
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("添加行", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                AddRow();
            }
            
            if (GUILayout.Button("删除行", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                DeleteRow();
            }
            
            if (GUILayout.Button("添加列", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                AddColumn();
            }
            
            if (GUILayout.Button("删除列", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                DeleteColumn();
            }
            
            EditorGUILayout.LabelField($"行: {maxRow}, 列: {maxCol}", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
            
            // 计算表格总高度
            float tableHeight = HEADER_HEIGHT;
            int visibleRowCount = Math.Min(_visibleRowCount, maxRow - _visibleStartRow + 1);
            for (int i = 0; i < visibleRowCount; i++)
            {
                int row = _visibleStartRow + i;
                tableHeight += GetRowHeight(row);
            }
            
            // 表格视图
            Rect tableRect = EditorGUILayout.GetControlRect(false, tableHeight);
            
            // 绘制表头
            DrawTableHeader(tableRect, maxCol);
            
            // 绘制表格内容
            DrawTableContent(tableRect, maxRow, maxCol);
        }
        
        private void DrawTableHeader(Rect tableRect, int maxCol)
        {
            Rect headerRect = new Rect(tableRect.x, tableRect.y, HEADER_WIDTH, HEADER_HEIGHT);
            GUI.Box(headerRect, "", EditorStyles.toolbar);
            
            float currentX = tableRect.x + HEADER_WIDTH;
            
            // 列号
            for (int col = _visibleStartCol; col <= Math.Min(_visibleStartCol + _visibleColCount - 1, maxCol); col++)
            {
                float colWidth = GetColumnWidth(col);
                
                headerRect = new Rect(
                    currentX,
                    tableRect.y,
                    colWidth,
                    HEADER_HEIGHT
                );
                
                GUI.Box(headerRect, GetColumnName(col), EditorStyles.toolbar);
                
                // 绘制列宽调整手柄
                Rect resizeHandle = new Rect(
                    currentX + colWidth - RESIZE_HANDLE_WIDTH / 2,
                    tableRect.y,
                    RESIZE_HANDLE_WIDTH,
                    HEADER_HEIGHT
                );
                
                EditorGUIUtility.AddCursorRect(resizeHandle, MouseCursor.ResizeHorizontal);
                
                if (Event.current.type == EventType.MouseDown && resizeHandle.Contains(Event.current.mousePosition))
                {
                    _isResizingColumn = true;
                    _resizingColumnIndex = col;
                    _resizeStartPos = Event.current.mousePosition.x;
                    _resizeStartSize = colWidth;
                    Event.current.Use();
                }
                
                currentX += colWidth;
            }
            
            // 处理列宽调整
            if (_isResizingColumn && _resizingColumnIndex >= 0)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    float delta = Event.current.mousePosition.x - _resizeStartPos;
                    float newWidth = Mathf.Max(MIN_COL_WIDTH, _resizeStartSize + delta);
                    _columnWidths[_resizingColumnIndex] = newWidth;
                    Event.current.Use();
                    Repaint();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    _isResizingColumn = false;
                    _resizingColumnIndex = -1;
                    Event.current.Use();
                }
            }
        }
        
        private void DrawTableContent(Rect tableRect, int maxRow, int maxCol)
        {
            float currentY = tableRect.y + HEADER_HEIGHT;
            
            // 行号列
            for (int row = _visibleStartRow; row <= Math.Min(_visibleStartRow + _visibleRowCount - 1, maxRow); row++)
            {
                float rowHeight = GetRowHeight(row);
                
                Rect rowHeaderRect = new Rect(
                    tableRect.x,
                    currentY,
                    HEADER_WIDTH,
                    rowHeight
                );
                
                GUI.Box(rowHeaderRect, row.ToString(), EditorStyles.miniButton);
                
                // 绘制行高调整手柄
                Rect resizeHandle = new Rect(
                    tableRect.x,
                    currentY + rowHeight - RESIZE_HANDLE_WIDTH / 2,
                    HEADER_WIDTH,
                    RESIZE_HANDLE_WIDTH
                );
                
                EditorGUIUtility.AddCursorRect(resizeHandle, MouseCursor.ResizeVertical);
                
                if (Event.current.type == EventType.MouseDown && resizeHandle.Contains(Event.current.mousePosition))
                {
                    _isResizingRow = true;
                    _resizingRowIndex = row;
                    _resizeStartPos = Event.current.mousePosition.y;
                    _resizeStartSize = rowHeight;
                    Event.current.Use();
                }
                
                currentY += rowHeight;
            }
            
            // 处理行高调整
            if (_isResizingRow && _resizingRowIndex >= 0)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    float delta = Event.current.mousePosition.y - _resizeStartPos;
                    float newHeight = Mathf.Max(MIN_ROW_HEIGHT, _resizeStartSize + delta);
                    _rowHeights[_resizingRowIndex] = newHeight;
                    Event.current.Use();
                    Repaint();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    _isResizingRow = false;
                    _resizingRowIndex = -1;
                    Event.current.Use();
                }
            }
            
            // 数据单元格
            currentY = tableRect.y + HEADER_HEIGHT;
            for (int row = _visibleStartRow; row <= Math.Min(_visibleStartRow + _visibleRowCount - 1, maxRow); row++)
            {
                float rowHeight = GetRowHeight(row);
                float currentX = tableRect.x + HEADER_WIDTH;
                
                for (int col = _visibleStartCol; col <= Math.Min(_visibleStartCol + _visibleColCount - 1, maxCol); col++)
                {
                    float colWidth = GetColumnWidth(col);
                    
                    Rect cellRect = new Rect(
                        currentX,
                        currentY,
                        colWidth,
                        rowHeight
                    );
                    
                    DrawCell(cellRect, row, col);
                    
                    currentX += colWidth;
                }
                
                currentY += rowHeight;
            }
            
            // 滚动条
            if (maxRow > _visibleRowCount)
            {
                int newStartRow = EditorGUILayout.IntSlider("行滚动", _visibleStartRow, 1, maxRow - _visibleRowCount + 1);
                if (newStartRow != _visibleStartRow)
                {
                    _visibleStartRow = newStartRow;
                }
            }
            
            if (maxCol > _visibleColCount)
            {
                int newStartCol = EditorGUILayout.IntSlider("列滚动", _visibleStartCol, 1, maxCol - _visibleColCount + 1);
                if (newStartCol != _visibleStartCol)
                {
                    _visibleStartCol = newStartCol;
                }
            }
        }
        
        private float GetColumnWidth(int colIndex)
        {
            if (_columnWidths.ContainsKey(colIndex))
            {
                return _columnWidths[colIndex];
            }
            return COL_WIDTH;
        }
        
        private float GetRowHeight(int rowIndex)
        {
            if (_rowHeights.ContainsKey(rowIndex))
            {
                return _rowHeights[rowIndex];
            }
            return ROW_HEIGHT;
        }
        
        private void DrawCell(Rect cellRect, int row, int col)
        {
            string cellKey = $"{row}_{col}";
            string cellValue = "";
            
            if (_editingCellKey == cellKey)
            {
                // 编辑模式
                if (!_cellEditValues.ContainsKey(cellKey))
                {
                    var cell = _currentWorksheet.Cells[row, col];
                    cellValue = cell.Value?.ToString() ?? "";
                    _cellEditValues[cellKey] = cellValue;
                }
                else
                {
                    cellValue = _cellEditValues[cellKey];
                }
                
                GUI.SetNextControlName(cellKey);
                EditorGUI.BeginChangeCheck();
                string newValue = EditorGUI.TextField(cellRect, cellValue);
                
                if (EditorGUI.EndChangeCheck())
                {
                    _cellEditValues[cellKey] = newValue;
                    _isDirty = true;
                }
                
                // 处理键盘事件
                if (Event.current.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == cellKey)
                {
                    if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                    {
                        // 确保使用最新的值
                        if (_cellEditValues.ContainsKey(cellKey))
                        {
                            ApplyCellEdit(cellKey, row, col);
                            // 如果开启了自动保存，立即保存
                            if (_autoSave)
                            {
                                SaveCurrentExcel(true); // 静默保存，不显示日志
                            }
                        }
                        _editingCellKey = "";
                        GUI.FocusControl(null);
                        Event.current.Use();
                        Repaint();
                    }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    {
                        _cellEditValues.Remove(cellKey);
                        _editingCellKey = "";
                        GUI.FocusControl(null);
                        Event.current.Use();
                        Repaint();
                    }
                }
                
                // 检查是否失去焦点（点击其他地方）
                if (Event.current.type == EventType.MouseDown && GUI.GetNameOfFocusedControl() == cellKey)
                {
                    if (!cellRect.Contains(Event.current.mousePosition))
                    {
                        // 失去焦点，应用编辑
                        if (_cellEditValues.ContainsKey(cellKey))
                        {
                            ApplyCellEdit(cellKey, row, col);
                            if (_autoSave)
                            {
                                SaveCurrentExcel(true);
                            }
                        }
                        _editingCellKey = "";
                        GUI.FocusControl(null);
                        Repaint();
                    }
                }
            }
            else
            {
                // 显示模式
                var cell = _currentWorksheet.Cells[row, col];
                cellValue = cell.Value?.ToString() ?? "";
                
                // 绘制单元格背景
                GUI.Box(cellRect, "", EditorStyles.textField);
                
                // 绘制文本
                Rect textRect = new Rect(cellRect.x + 2, cellRect.y, cellRect.width - 4, cellRect.height);
                GUI.Label(textRect, cellValue, EditorStyles.label);
                
                // 处理点击事件 - 使用MouseDown而不是Button，确保单击即可
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && cellRect.Contains(Event.current.mousePosition))
                {
                    _editingCellKey = cellKey;
                    _cellEditValues[cellKey] = cellValue;
                    GUI.FocusControl(cellKey);
                    Event.current.Use();
                    Repaint();
                }
            }
        }
        
        private void ApplyCellEdit(string cellKey, int row, int col)
        {
            if (_cellEditValues.ContainsKey(cellKey))
            {
                string newValue = _cellEditValues[cellKey];
                _currentWorksheet.Cells[row, col].Value = newValue;
                // 不立即移除，保留在字典中以便下次编辑时使用
                _isDirty = true;
            }
        }
        
        private string GetColumnName(int colIndex)
        {
            string result = "";
            while (colIndex > 0)
            {
                colIndex--;
                result = (char)('A' + colIndex % 26) + result;
                colIndex /= 26;
            }
            return result;
        }
        
        private void SelectFolder()
        {
            string defaultPath = _selectedFolder;
            if (string.IsNullOrEmpty(defaultPath))
            {
                defaultPath = Application.dataPath;
            }
            
            string path = EditorUtility.OpenFolderPanel("选择Excel文件夹", defaultPath, "");
            
            if (!string.IsNullOrEmpty(path))
            {
                _selectedFolder = path;
                SaveFolderToPrefs();
                RefreshExcelList();
            }
        }
        
        private void RefreshExcelList()
        {
            _excelFiles.Clear();
            _selectedExcelIndex = -1;
            CloseCurrentExcel();
            
            if (string.IsNullOrEmpty(_selectedFolder) || !Directory.Exists(_selectedFolder))
            {
                return;
            }
            
            var files = Directory.GetFiles(_selectedFolder, "*.xlsx", SearchOption.TopDirectoryOnly);
            _excelFiles.AddRange(files.OrderBy(f => f));
            
            // 重新加载映射表
            LoadTablesMapping();
        }
        
        private void SelectExcel(int index)
        {
            if (index < 0 || index >= _excelFiles.Count)
            {
                return;
            }
            
            if (_isDirty)
            {
                int result = EditorUtility.DisplayDialogComplex("未保存的更改", "当前文件有未保存的更改，是否保存？", "保存", "不保存", "取消");
                if (result == 2) // 取消
                {
                    return;
                }
                else if (result == 0) // 保存
                {
                    SaveCurrentExcel();
                }
                // result == 1 表示不保存，直接继续
            }
            
            CloseCurrentExcel();
            
            _selectedExcelIndex = index;
            _currentExcelPath = _excelFiles[index];
            
            try
            {
                var fileInfo = new FileInfo(_currentExcelPath);
                _currentPackage = new ExcelPackage(fileInfo);
                
                // 获取所有工作表
                _availableSheets.Clear();
                foreach (var sheet in _currentPackage.Workbook.Worksheets)
                {
                    _availableSheets.Add(sheet.Name);
                }
                
                Debug.Log($"Excel文件包含 {_availableSheets.Count} 个工作表: {string.Join(", ", _availableSheets)}");
                
                // 尝试从映射中获取应该使用的sheet名称
                string targetSheetName = GetTargetSheetName();
                
                if (!string.IsNullOrEmpty(targetSheetName) && _availableSheets.Contains(targetSheetName))
                {
                    _currentWorksheet = _currentPackage.Workbook.Worksheets[targetSheetName];
                    _selectedSheetIndex = _availableSheets.IndexOf(targetSheetName);
                    Debug.Log($"根据映射选择工作表: {targetSheetName}");
                }
                else
                {
                    // 使用第一个工作表
                    _currentWorksheet = _currentPackage.Workbook.Worksheets.FirstOrDefault();
                    if (_currentWorksheet == null)
                    {
                        _currentWorksheet = _currentPackage.Workbook.Worksheets.Add("Sheet1");
                        _availableSheets.Add("Sheet1");
                    }
                    _selectedSheetIndex = 0;
                    Debug.Log($"使用默认工作表: {_currentWorksheet.Name}");
                }
                
                _visibleStartRow = 1;
                _visibleStartCol = 1;
                _isDirty = false;
                _columnWidths.Clear();
                _rowHeights.Clear();
                
                // 刷新数据（解析字段定义和映射）
                RefreshCurrentData();
                
                Debug.Log($"已打开Excel文件: {Path.GetFileName(_currentExcelPath)}");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"无法打开Excel文件: {e.Message}", "确定");
                Debug.LogError(e);
            }
        }
        
        private string GetTargetSheetName()
        {
            if (_currentMapping == null || string.IsNullOrEmpty(_currentMapping.Input))
            {
                return null;
            }
            
            // 解析 Input 字段，格式可能是 "基础状态表@Buff.xlsx" 或 "Buff.xlsx"
            var input = _currentMapping.Input;
            
            // 检查是否包含 @ 符号
            if (input.Contains("@"))
            {
                var parts = input.Split('@');
                if (parts.Length >= 1)
                {
                    var sheetName = parts[0].Trim();
                    Debug.Log($"从映射Input中提取工作表名: {sheetName}");
                    return sheetName;
                }
            }
            
            return null;
        }
        
        private void SelectSheet(int index)
        {
            if (index < 0 || index >= _availableSheets.Count || _currentPackage == null)
            {
                return;
            }
            
            if (_isDirty)
            {
                int result = EditorUtility.DisplayDialogComplex("未保存的更改", "当前工作表有未保存的更改，是否保存？", "保存", "不保存", "取消");
                if (result == 2) // 取消
                {
                    return;
                }
                else if (result == 0) // 保存
                {
                    SaveCurrentExcel(true);
                }
            }
            
            try
            {
                var sheetName = _availableSheets[index];
                _currentWorksheet = _currentPackage.Workbook.Worksheets[sheetName];
                _selectedSheetIndex = index;
                
                _visibleStartRow = 1;
                _visibleStartCol = 1;
                _isDirty = false;
                _columnWidths.Clear();
                _rowHeights.Clear();
                _cellEditValues.Clear();
                _editingCellKey = "";
                
                // 刷新数据（包括重新解析表结构和映射）
                RefreshCurrentData();
                
                Debug.Log($"已切换到工作表: {sheetName}");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"无法切换到工作表: {e.Message}", "确定");
                Debug.LogError(e);
            }
        }
        
        private void SaveCurrentExcel(bool silent = false)
        {
            if (_currentPackage == null)
            {
                Debug.LogWarning("SaveCurrentExcel: _currentPackage 为 null");
                return;
            }
            
            if (string.IsNullOrEmpty(_currentExcelPath))
            {
                Debug.LogWarning("SaveCurrentExcel: _currentExcelPath 为空");
                return;
            }
            
            try
            {
                // 检查文件是否被占用
                var fileInfo = new FileInfo(_currentExcelPath);
                if (!fileInfo.Exists)
                {
                    Debug.LogWarning($"SaveCurrentExcel: 文件不存在: {_currentExcelPath}");
                }
                
                // 检查文件是否只读
                if (fileInfo.Exists && fileInfo.IsReadOnly)
                {
                    EditorUtility.DisplayDialog("错误", $"文件是只读的，无法保存: {_currentExcelPath}", "确定");
                    Debug.LogError($"文件是只读的: {_currentExcelPath}");
                    return;
                }
                
                if (_viewMode == ViewMode.List)
                {
                    // 列表模式：保存单元格编辑
                    foreach (var kvp in _cellEditValues)
                    {
                        var parts = kvp.Key.Split('_');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int row) && int.TryParse(parts[1], out int col))
                        {
                            _currentWorksheet.Cells[row, col].Value = kvp.Value;
                        }
                    }
                    _cellEditValues.Clear();
                    _editingCellKey = "";
                }
                else
                {
                    // 对象模式：保存对象数据到Excel
                    SaveObjectDataToExcel();
                }
                
                Debug.Log($"SaveCurrentExcel: 开始保存文件: {_currentExcelPath}");
                
                // 使用 FileInfo 保存
                _currentPackage.SaveAs(fileInfo);
                
                _isDirty = false;
                
                if (!silent)
                {
                    Debug.Log($"已保存Excel文件: {Path.GetFileName(_currentExcelPath)}");
                }
            }
            catch (System.IO.IOException ioEx)
            {
                string errorMsg = $"文件可能被其他程序占用（如Excel）: {ioEx.Message}";
                EditorUtility.DisplayDialog("错误", $"保存失败: {errorMsg}\n\n请关闭Excel或其他打开此文件的程序后重试。", "确定");
                Debug.LogError($"保存失败 (IO异常): {errorMsg}\n{ioEx.StackTrace}");
            }
            catch (UnauthorizedAccessException authEx)
            {
                string errorMsg = $"没有权限保存文件: {authEx.Message}";
                EditorUtility.DisplayDialog("错误", $"保存失败: {errorMsg}\n\n请检查文件权限。", "确定");
                Debug.LogError($"保存失败 (权限异常): {errorMsg}\n{authEx.StackTrace}");
            }
            catch (Exception e)
            {
                string errorMsg = $"保存失败: {e.Message}";
                EditorUtility.DisplayDialog("错误", $"{errorMsg}\n\n文件路径: {_currentExcelPath}\n\n详细错误请查看Console。", "确定");
                Debug.LogError($"保存失败: {errorMsg}\n异常类型: {e.GetType().Name}\n堆栈跟踪:\n{e.StackTrace}");
            }
        }
        
        private void SaveObjectDataToExcel()
        {
            if (_currentWorksheet == null || _objectList.Count == 0)
            {
                return;
            }
            
            var dimension = _currentWorksheet.Dimension;
            if (dimension == null)
            {
                return;
            }
            
            // 使用 TableSchema 中的 DataStartRow（##comment 行下面的行）
            int dataStartRow = _currentTableSchema?.DataStartRow ?? 5;
            
            for (int i = 0; i < _objectList.Count; i++)
            {
                int row = dataStartRow + i;
                var obj = _objectList[i];
                
                if (obj is Dictionary<string, object> dict)
                {
                    foreach (var fieldDefWithIndex in _fieldDefinitions)
                    {
                        var fieldDef = fieldDefWithIndex.Field;
                        if (dict.ContainsKey(fieldDef.Name))
                        {
                            var value = dict[fieldDef.Name];
                            string strValue = "";
                            
                            if (value is List<int> list)
                            {
                                // 列表转换为字符串
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
                            
                            _currentWorksheet.Cells[row, fieldDefWithIndex.ColumnIndex].Value = strValue;
                        }
                    }
                }
            }
        }
        
        private void ReloadCurrentExcel()
        {
            if (string.IsNullOrEmpty(_currentExcelPath))
            {
                return;
            }
            
            if (_isDirty)
            {
                int result = EditorUtility.DisplayDialogComplex("未保存的更改", "当前文件有未保存的更改，是否保存？", "保存", "不保存", "取消");
                if (result == 2) // 取消
                {
                    return;
                }
                else if (result == 0) // 保存
                {
                    SaveCurrentExcel();
                }
                // result == 1 表示不保存，直接继续
            }
            
            CloseCurrentExcel();
            SelectExcel(_selectedExcelIndex);
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
            _cellEditValues.Clear();
            _editingCellKey = "";
            _isDirty = false;
        }
        
        private void AddRow()
        {
            if (_currentWorksheet == null)
            {
                return;
            }
            
            var dimension = _currentWorksheet.Dimension;
            int insertRow = dimension != null ? dimension.End.Row + 1 : 1;
            
            _currentWorksheet.InsertRow(insertRow, 1);
            _isDirty = true;
            
            Debug.Log($"已添加新行: {insertRow}");
        }
        
        private void DeleteRow()
        {
            if (_currentWorksheet == null)
            {
                return;
            }
            
            var dimension = _currentWorksheet.Dimension;
            if (dimension == null)
            {
                return;
            }
            
            int deleteRow = _visibleStartRow;
            if (deleteRow < 1 || deleteRow > dimension.End.Row)
            {
                EditorUtility.DisplayDialog("提示", "请选择要删除的行", "确定");
                return;
            }
            
            if (EditorUtility.DisplayDialog("确认删除", $"确定要删除第{deleteRow}行吗？", "确定", "取消"))
            {
                _currentWorksheet.DeleteRow(deleteRow);
                _isDirty = true;
                Debug.Log($"已删除行: {deleteRow}");
            }
        }
        
        private void AddColumn()
        {
            if (_currentWorksheet == null)
            {
                return;
            }
            
            var dimension = _currentWorksheet.Dimension;
            int insertCol = dimension != null ? dimension.End.Column + 1 : 1;
            
            _currentWorksheet.InsertColumn(insertCol, 1);
            _isDirty = true;
            
            Debug.Log($"已添加新列: {GetColumnName(insertCol)}");
        }
        
        private void DeleteColumn()
        {
            if (_currentWorksheet == null)
            {
                return;
            }
            
            var dimension = _currentWorksheet.Dimension;
            if (dimension == null)
            {
                return;
            }
            
            int deleteCol = _visibleStartCol;
            if (deleteCol < 1 || deleteCol > dimension.End.Column)
            {
                EditorUtility.DisplayDialog("提示", "请选择要删除的列", "确定");
                return;
            }
            
            if (EditorUtility.DisplayDialog("确认删除", $"确定要删除第{GetColumnName(deleteCol)}列吗？", "确定", "取消"))
            {
                _currentWorksheet.DeleteColumn(deleteCol);
                _isDirty = true;
                Debug.Log($"已删除列: {GetColumnName(deleteCol)}");
            }
        }
        
        private void LoadSavedFolder()
        {
            _selectedFolder = EditorPrefs.GetString(EDITOR_PREF_FOLDER_KEY, "");
            if (!string.IsNullOrEmpty(_selectedFolder) && Directory.Exists(_selectedFolder))
            {
                RefreshExcelList();
            }
        }
        
        private void SaveFolderToPrefs()
        {
            EditorPrefs.SetString(EDITOR_PREF_FOLDER_KEY, _selectedFolder);
        }
        
        private void LoadAutoSaveSetting()
        {
            _autoSave = EditorPrefs.GetBool(EDITOR_PREF_AUTO_SAVE_KEY, false);
        }
        
        private void SaveAutoSaveSetting()
        {
            EditorPrefs.SetBool(EDITOR_PREF_AUTO_SAVE_KEY, _autoSave);
        }
        
        private void LoadViewModeSetting()
        {
            _viewMode = (ViewMode)EditorPrefs.GetInt(EDITOR_PREF_VIEW_MODE_KEY, (int)ViewMode.List);
        }
        
        private void SaveViewModeSetting()
        {
            EditorPrefs.SetInt(EDITOR_PREF_VIEW_MODE_KEY, (int)_viewMode);
        }
        
        private void LoadTablesMapping()
        {
            if (string.IsNullOrEmpty(_selectedFolder) || !Directory.Exists(_selectedFolder))
            {
                Debug.LogWarning($"LoadTablesMapping: 文件夹无效: {_selectedFolder}");
                return;
            }
            
            string tablesPath = Path.Combine(_selectedFolder, "__tables__.xlsx");
            Debug.Log($"LoadTablesMapping: 查找映射文件: {tablesPath}");
            
            _tableMappings = ExcelEditorHelper.ParseTablesMapping(tablesPath);
            Debug.Log($"LoadTablesMapping: 解析完成，找到 {_tableMappings.Count} 个映射");
            
            foreach (var mapping in _tableMappings)
            {
                Debug.Log($"  映射: Key={mapping.Key}, FullName={mapping.Value.FullName}, ValueType={mapping.Value.ValueType}, Input={mapping.Value.Input}");
            }
        }
        
        private void RefreshCurrentData()
        {
            if (_currentWorksheet == null)
            {
                Debug.LogWarning("RefreshCurrentData: _currentWorksheet 为 null");
                return;
            }
            
            Debug.Log($"开始刷新数据，Excel文件: {Path.GetFileName(_currentExcelPath)}, 工作表: {_currentWorksheet.Name}");
            
            // 先查找对应的映射，获取正确的表名和类型信息
            var fileName = Path.GetFileName(_currentExcelPath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(_currentExcelPath);
            var currentSheetName = _currentWorksheet?.Name ?? "";
            
            Debug.Log($"查找映射，文件名: {fileName}, 工作表: {currentSheetName}, 映射表数量: {_tableMappings.Count}");
            
            _currentMapping = null;
            _currentConfigType = null;
            
            // 优先查找精确匹配的映射（带@的）
            List<ExcelEditorHelper.TableMapping> exactMatches = new List<ExcelEditorHelper.TableMapping>();
            List<ExcelEditorHelper.TableMapping> fileMatches = new List<ExcelEditorHelper.TableMapping>();
            
            foreach (var mapping in _tableMappings.Values)
            {
                Debug.Log($"  检查映射: FullName={mapping.FullName}, ValueType={mapping.ValueType}, Input={mapping.Input}");
                
                // 如果Input包含@，需要同时匹配文件名和工作表名（精确匹配）
                if (mapping.Input.Contains("@"))
                {
                    var parts = mapping.Input.Split('@');
                    if (parts.Length >= 2)
                    {
                        var sheetName = parts[0].Trim();
                        var filePart = parts[1].Trim();
                        
                        // 检查文件名和工作表名是否都匹配
                        if ((filePart.Contains(fileName) || filePart.Contains(fileNameWithoutExt)) && sheetName == currentSheetName)
                        {
                            exactMatches.Add(mapping);
                            Debug.Log($"  精确匹配（带@）: 工作表={sheetName}, 文件={filePart}");
                        }
                    }
                }
                else
                {
                    // 只检查文件名（模糊匹配，用于没有@的情况）
                    if (mapping.Input.Contains(fileName) || mapping.Input.Contains(fileNameWithoutExt))
                    {
                        fileMatches.Add(mapping);
                        Debug.Log($"  文件匹配（无@）: Input包含文件名");
                    }
                }
            }
            
            // 优先使用精确匹配（带@的）
            if (exactMatches.Count > 0)
            {
                _currentMapping = exactMatches[0];
                Debug.Log($"  使用精确匹配的映射: {_currentMapping.FullName}, ValueType: {_currentMapping.ValueType}");
            }
            else if (fileMatches.Count > 0)
            {
                // 如果文件有多个sheet，且没有精确匹配，可能需要更智能的选择
                // 暂时使用第一个匹配的
                if (_availableSheets.Count > 1)
                {
                    Debug.LogWarning($"  文件有多个sheet但未找到精确匹配，使用第一个文件匹配（可能不正确）");
                }
                _currentMapping = fileMatches[0];
                Debug.Log($"  使用文件匹配的映射: {_currentMapping.FullName}, ValueType: {_currentMapping.ValueType}");
            }
            
            if (_currentMapping != null)
            {
                // 从FullName中提取命名空间信息（直接从__tables__.xlsx解析）
                // 例如: battle.TbTower -> GameConfig.battle
                string namespaceHint = null;
                if (_currentMapping.FullName.Contains("."))
                {
                    var parts = _currentMapping.FullName.Split('.');
                    if (parts.Length >= 2)
                    {
                        // 取除了最后一部分的所有部分作为命名空间
                        namespaceHint = "GameConfig." + string.Join(".", parts.Take(parts.Length - 1));
                        Debug.Log($"  从FullName提取命名空间: {namespaceHint}");
                    }
                }
                
                // 直接使用从__tables__.xlsx解析出的命名空间
                _currentConfigType = ExcelEditorHelper.FindTypeByName(_currentMapping.ValueType, namespaceHint);
                if (_currentConfigType != null)
                {
                    Debug.Log($"  成功找到类型: {_currentConfigType.FullName}");
                }
                else
                {
                    Debug.LogWarning($"  无法找到类型: {_currentMapping.ValueType}，使用的命名空间: {namespaceHint ?? "GameConfig"}");
                }
            }
            else
            {
                Debug.LogWarning($"未找到匹配的映射，文件名: {fileName}, 工作表: {currentSheetName}");
            }
            
            // 使用ExcelTableSchemaProvider解析表结构
            // 注意：LoadSchema需要的是工作表名（用于查找工作表），但TableName应该从映射中获取
            try
            {
                // 从映射中提取表名（FullName的最后一部分，例如 battle.TbTower -> TbTower）
                string tableNameFromMapping = null;
                if (_currentMapping != null && _currentMapping.FullName.Contains("."))
                {
                    tableNameFromMapping = _currentMapping.FullName.Split('.').LastOrDefault();
                    Debug.Log($"从映射FullName提取表名: {tableNameFromMapping}");
                }
                
                // 使用工作表名来解析（LoadSchema内部会使用工作表名来查找工作表）
                _currentTableSchema = ExcelEditorHelper.ParseTableSchema(_currentExcelPath, _currentWorksheet.Name);
                if (_currentTableSchema != null)
                {
                    // 如果找到了映射，使用映射中的表名（而不是工作表名）
                    if (!string.IsNullOrEmpty(tableNameFromMapping))
                    {
                        _currentTableSchema.TableName = tableNameFromMapping;
                        Debug.Log($"使用映射中的表名: {tableNameFromMapping}（工作表名: {_currentWorksheet.Name}）");
                    }
                    
                    _fieldDefinitions = ExcelEditorHelper.ConvertSchemaToFieldList(_currentTableSchema);
                    Debug.Log($"解析表结构成功，表名: {_currentTableSchema.TableName}, 字段数量: {_fieldDefinitions.Count}");
                    if (_fieldDefinitions.Count > 0)
                    {
                        foreach (var field in _fieldDefinitions)
                        {
                            Debug.Log($"  字段: {field.Field.Name}, 类型: {field.Field.RawType ?? field.Field.Type}, 列索引: {field.ColumnIndex}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"表结构解析成功但字段列表为空，表名: {_currentTableSchema.TableName}, Fields.Count: {_currentTableSchema.Fields.Count}");
                    }
                }
                else
                {
                    _fieldDefinitions.Clear();
                    Debug.LogWarning($"ParseTableSchema返回null，文件: {_currentExcelPath}, 工作表: {_currentWorksheet.Name}");
                    
                    // 尝试直接使用ExcelTableSchemaProvider解析
                    try
                    {
                        var provider = new ExcelTableSchemaProvider(_currentExcelPath, _currentWorksheet.Name);
                        var schema = provider.LoadSchema(_currentWorksheet.Name);
                        if (schema != null)
                        {
                            if (!string.IsNullOrEmpty(tableNameFromMapping))
                            {
                                schema.TableName = tableNameFromMapping;
                            }
                            Debug.Log($"直接使用provider解析成功，字段数: {schema.Fields.Count}");
                            _currentTableSchema = schema;
                            _fieldDefinitions = ExcelEditorHelper.ConvertSchemaToFieldList(_currentTableSchema);
                            Debug.Log($"转换后字段数: {_fieldDefinitions.Count}");
                        }
                    }
                    catch (Exception e2)
                    {
                        Debug.LogError($"直接使用provider解析失败: {e2.Message}\n{e2.StackTrace}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"解析表结构时发生异常: {e.Message}\n{e.StackTrace}");
                _fieldDefinitions.Clear();
            }
            
            // 如果是对象模式，加载对象列表
            if (_viewMode == ViewMode.Object)
            {
                Debug.Log($"对象模式，开始加载对象列表，ConfigType: {(_currentConfigType != null ? _currentConfigType.FullName : "null")}, FieldDefinitions: {_fieldDefinitions.Count}");
                LoadObjectList();
            }
            else
            {
                Debug.Log("列表模式，不需要加载对象列表，直接显示表格");
                // 列表模式不需要特殊处理，直接显示表格即可
            }
        }
        
        private void LoadObjectList()
        {
            _objectList.Clear();
            _selectedObjectIndex = -1;
            
            Debug.Log($"LoadObjectList: _currentWorksheet={(_currentWorksheet != null ? _currentWorksheet.Name : "null")}, _fieldDefinitions.Count={_fieldDefinitions.Count}");
            
            if (_currentWorksheet == null)
            {
                Debug.LogWarning("LoadObjectList: _currentWorksheet 为 null");
                return;
            }
            
            if (_fieldDefinitions.Count == 0)
            {
                Debug.LogWarning("LoadObjectList: _fieldDefinitions 为空，无法加载对象列表");
                return;
            }
            
            var dimension = _currentWorksheet.Dimension;
            if (dimension == null)
            {
                Debug.LogWarning("LoadObjectList: 工作表维度为 null，可能是空表");
                return;
            }
            
            Debug.Log($"工作表维度: Start.Row={dimension.Start.Row}, End.Row={dimension.End.Row}, Start.Column={dimension.Start.Column}, End.Column={dimension.End.Column}");
            
            // 找到数据开始行：使用 TableSchema 中的 DataStartRow（##comment 行下面的行）
            int dataStartRow = _currentTableSchema?.DataStartRow ?? 5;
            Debug.Log($"数据开始行: {dataStartRow} (来自TableSchema: {_currentTableSchema?.DataStartRow ?? 0}), 数据结束行: {dimension.End.Row}");
            
            int loadedCount = 0;
            // 遍历数据行
            for (int row = dataStartRow; row <= dimension.End.Row; row++)
            {
                try
                {
                    var obj = CreateObjectFromRow(row);
                    if (obj != null)
                    {
                        _objectList.Add(obj);
                        loadedCount++;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"加载第{row}行数据失败: {e.Message}\n{e.StackTrace}");
                }
            }
            
            Debug.Log($"LoadObjectList 完成，成功加载 {loadedCount} 个对象");
        }
        
        private object CreateObjectFromRow(int row)
        {
            if (_fieldDefinitions.Count == 0)
            {
                Debug.LogWarning($"CreateObjectFromRow: 第{row}行，字段定义为空");
                return null;
            }
            
            // 使用反射创建对象（这里简化处理，实际Luban类需要特殊处理）
            // 由于Luban类使用readonly字段，我们需要通过其他方式创建
            // 这里先返回一个字典来存储数据
            var data = new Dictionary<string, object>();
            
            foreach (var fieldDefWithIndex in _fieldDefinitions)
            {
                var fieldDef = fieldDefWithIndex.Field;
                try
                {
                    var cell = _currentWorksheet.Cells[row, fieldDefWithIndex.ColumnIndex];
                    var value = cell.Value;
                    
                    // 根据类型转换值
                    object convertedValue = ConvertValue(value, fieldDef.RawType ?? fieldDef.Type);
                    data[fieldDef.Name] = convertedValue;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"CreateObjectFromRow: 第{row}行，列{fieldDefWithIndex.ColumnIndex}，字段{fieldDef.Name}处理失败: {e.Message}");
                }
            }
            
            if (data.Count == 0)
            {
                Debug.LogWarning($"CreateObjectFromRow: 第{row}行，没有成功解析任何字段");
                return null;
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
            
            // 处理列表类型
            if (typeName.Contains("list"))
            {
                var str = value?.ToString() ?? "";
                if (string.IsNullOrEmpty(str))
                {
                    return new List<int>();
                }
                
                // 解析分隔符（例如 #sep=;）
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
            
            // 处理基本类型
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
        
        private void DrawObjectView()
        {
            if (_objectList.Count == 0)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.HelpBox("没有找到数据，请检查Excel文件格式", MessageType.Info);
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("调试信息:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"工作表: {(_currentWorksheet != null ? _currentWorksheet.Name : "null")}");
                EditorGUILayout.LabelField($"字段定义数量: {_fieldDefinitions.Count}");
                EditorGUILayout.LabelField($"映射信息: {(_currentMapping != null ? _currentMapping.FullName : "未找到")}");
                EditorGUILayout.LabelField($"配置类型: {(_currentConfigType != null ? _currentConfigType.FullName : "未找到")}");
                
                if (GUILayout.Button("重新加载", GUILayout.Height(30)))
                {
                    RefreshCurrentData();
                }
                
                EditorGUILayout.EndVertical();
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            // 左侧：对象列表和搜索
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            EditorGUILayout.LabelField("对象列表", EditorStyles.boldLabel);
            
            // 搜索框
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
            {
                PerformSearch();
            }
            
            if (!string.IsNullOrEmpty(_searchText))
            {
                if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    _searchText = "";
                    _searchResults.Clear();
                    _showSearchResults = false;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // 显示搜索结果数量
            if (!string.IsNullOrEmpty(_searchText) && _searchResults.Count > 0)
            {
                EditorGUILayout.LabelField($"找到 {_searchResults.Count} 个结果", EditorStyles.miniLabel);
            }
            else if (!string.IsNullOrEmpty(_searchText))
            {
                EditorGUILayout.LabelField("未找到匹配项", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.Space(5);
            
            // 对象列表或搜索结果
            _objectScrollPosition = EditorGUILayout.BeginScrollView(_objectScrollPosition);
            
            var displayList = string.IsNullOrEmpty(_searchText) ? 
                Enumerable.Range(0, _objectList.Count).ToList() : 
                _searchResults;
            
            foreach (int i in displayList)
            {
                if (i < 0 || i >= _objectList.Count)
                {
                    continue;
                }
                
                var isSelected = i == _selectedObjectIndex;
                var obj = _objectList[i];
                
                // 获取显示名称
                string displayName = GetObjectDisplayName(obj, i);
                
                // 高亮搜索关键词
                if (!string.IsNullOrEmpty(_searchText) && displayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                {
                    // 可以在这里添加高亮显示
                }
                
                if (GUILayout.Toggle(isSelected, displayName, EditorStyles.miniButton, GUILayout.Height(25)))
                {
                    if (!isSelected)
                    {
                        _selectedObjectIndex = i;
                        _showSearchResults = false; // 选择后关闭搜索结果
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            
            // 右侧：对象详情
            EditorGUILayout.BeginVertical();
            
            if (_selectedObjectIndex >= 0 && _selectedObjectIndex < _objectList.Count)
            {
                DrawObjectDetails(_objectList[_selectedObjectIndex]);
            }
            else
            {
                EditorGUILayout.HelpBox("请从左侧选择一个对象", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private string GetObjectDisplayName(object obj, int index)
        {
            string displayName = $"对象 {index + 1}";
            
            if (obj is Dictionary<string, object> dict)
            {
                // 优先显示ID
                if (dict.ContainsKey("id"))
                {
                    var id = dict["id"];
                    displayName = $"ID: {id}";
                    
                    // 如果有name或desc字段，也显示
                    if (dict.ContainsKey("name"))
                    {
                        displayName += $" - {dict["name"]}";
                    }
                    else if (dict.ContainsKey("desc"))
                    {
                        displayName += $" - {dict["desc"]}";
                    }
                }
                else if (dict.ContainsKey("Id"))
                {
                    var id = dict["Id"];
                    displayName = $"ID: {id}";
                    
                    if (dict.ContainsKey("Name"))
                    {
                        displayName += $" - {dict["Name"]}";
                    }
                    else if (dict.ContainsKey("Desc"))
                    {
                        displayName += $" - {dict["Desc"]}";
                    }
                }
                else if (dict.ContainsKey("name"))
                {
                    displayName = $"{dict["name"]}";
                }
                else if (dict.ContainsKey("Name"))
                {
                    displayName = $"{dict["Name"]}";
                }
            }
            
            return displayName;
        }
        
        private void PerformSearch()
        {
            _searchResults.Clear();
            
            if (string.IsNullOrEmpty(_searchText))
            {
                _showSearchResults = false;
                return;
            }
            
            string searchLower = _searchText.ToLower();
            
            for (int i = 0; i < _objectList.Count; i++)
            {
                var obj = _objectList[i];
                
                if (obj is Dictionary<string, object> dict)
                {
                    bool matched = false;
                    
                    // 遍历所有字段进行模糊匹配
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value != null)
                        {
                            string valueStr = kvp.Value.ToString().ToLower();
                            if (valueStr.Contains(searchLower))
                            {
                                matched = true;
                                break;
                            }
                        }
                    }
                    
                    // 也检查显示名称
                    if (!matched)
                    {
                        string displayName = GetObjectDisplayName(obj, i).ToLower();
                        if (displayName.Contains(searchLower))
                        {
                            matched = true;
                        }
                    }
                    
                    if (matched)
                    {
                        _searchResults.Add(i);
                    }
                }
            }
            
            _showSearchResults = _searchResults.Count > 0;
            
            // 如果有搜索结果，自动选择第一个
            if (_searchResults.Count > 0)
            {
                _selectedObjectIndex = _searchResults[0];
            }
        }
        
        private void DrawObjectDetails(object obj)
        {
            if (obj == null)
            {
                return;
            }
            
            EditorGUILayout.LabelField("对象详情", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (obj is Dictionary<string, object> dict)
            {
                foreach (var fieldDefWithIndex in _fieldDefinitions)
                {
                    var fieldDef = fieldDefWithIndex.Field;
                    var value = dict.ContainsKey(fieldDef.Name) ? dict[fieldDef.Name] : null;
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    // 显示字段名和注释
                    string label = fieldDef.Name;
                    if (!string.IsNullOrEmpty(fieldDef.Comment))
                    {
                        label = $"{fieldDef.Comment} ({fieldDef.Name})";
                    }
                    else if (!string.IsNullOrEmpty(fieldDef.DisplayName))
                    {
                        label = $"{fieldDef.DisplayName} ({fieldDef.Name})";
                    }
                    
                    EditorGUILayout.LabelField(label, GUILayout.Width(200));
                    
                    // 显示和编辑值
                    DrawFieldValue(fieldDef, value, dict);
                    
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.Space(2);
                }
            }
        }
        
        private void DrawFieldValue(FieldSchema fieldDef, object value, Dictionary<string, object> dataDict)
        {
            string typeStr = fieldDef.RawType ?? fieldDef.Type ?? "";
            bool isList = typeStr.Contains("list");
            bool isRef = typeStr.Contains("ref") || fieldDef.Name.EndsWith("_Ref") || fieldDef.Name.EndsWith("Id");
            bool isPath = (!string.IsNullOrEmpty(fieldDef.Comment) && fieldDef.Comment.Contains("路径")) ||
                          (!string.IsNullOrEmpty(fieldDef.DisplayName) && fieldDef.DisplayName.Contains("路径"));
            
            EditorGUILayout.BeginHorizontal();
            
            if (value is List<int> list)
            {
                // 列表类型：显示列表项，带 + 和 - 按钮
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"列表 ({list.Count} 项)", EditorStyles.miniLabel);
                
                // + 按钮：添加新元素
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
                    
                    // - 按钮：删除当前元素
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        list.RemoveAt(i);
                        _isDirty = true;
                        break; // 退出循环，因为列表已改变
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            else if (value is int intVal)
            {
                EditorGUILayout.BeginVertical();
                int newVal = EditorGUILayout.IntField(intVal);
                if (newVal != intVal)
                {
                    dataDict[fieldDef.Name] = newVal;
                    _isDirty = true;
                }
                
                // Ref 类型：添加打开引用对象按钮
                if (isRef && intVal > 0)
                {
                    if (GUILayout.Button("打开引用", GUILayout.Width(80)))
                    {
                        OpenReferencedObject(intVal, fieldDef);
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
            else if (value is float floatVal)
            {
                float newVal = EditorGUILayout.FloatField(floatVal);
                if (newVal != floatVal)
                {
                    dataDict[fieldDef.Name] = newVal;
                    _isDirty = true;
                }
            }
            else if (value is bool boolVal)
            {
                bool newVal = EditorGUILayout.Toggle(boolVal);
                if (newVal != boolVal)
                {
                    dataDict[fieldDef.Name] = newVal;
                    _isDirty = true;
                }
            }
            else
            {
                string strVal = value?.ToString() ?? "";
                EditorGUILayout.BeginVertical();
                string newVal = EditorGUILayout.TextField(strVal);
                if (newVal != strVal)
                {
                    dataDict[fieldDef.Name] = newVal;
                    _isDirty = true;
                }
                
                // 路径字段：添加 Ping 按钮
                if (isPath && !string.IsNullOrEmpty(newVal))
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(60)))
                    {
                        PingAsset(newVal);
                    }
                }
                
                // Ref 类型（字符串ID）：添加打开引用对象按钮
                if (isRef && !string.IsNullOrEmpty(newVal) && int.TryParse(newVal, out int refId) && refId > 0)
                {
                    if (GUILayout.Button("打开引用", GUILayout.Width(80)))
                    {
                        OpenReferencedObject(refId, fieldDef);
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void PingAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }
            
            // 确保路径以 Assets/ 开头
            if (!assetPath.StartsWith("Assets/"))
            {
                assetPath = "Assets/" + assetPath.TrimStart('/');
            }
            
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
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
        
        private void OpenReferencedObject(int refId, FieldSchema fieldDef)
        {
            // 查找引用对象所在的表
            // 通常 ref 字段名会包含引用的表名，例如 "itemId" -> "item" 表
            // 或者从字段的 Comment/DisplayName 中提取表名
            
            string refTableName = null;
            string fieldName = fieldDef.Name.ToLower();
            
            // 尝试从字段名提取表名（例如 "itemId" -> "item"）
            if (fieldName.EndsWith("id"))
            {
                refTableName = fieldName.Substring(0, fieldName.Length - 2);
            }
            else if (fieldName.Contains("_ref"))
            {
                refTableName = fieldName.Replace("_ref", "");
            }
            
            // 如果找不到，尝试从映射中查找
            if (string.IsNullOrEmpty(refTableName))
            {
                // 查找所有可能的表
                foreach (var mapping in _tableMappings.Values)
                {
                    string tableKey = mapping.FullName.Split('.').LastOrDefault()?.ToLower();
                    if (!string.IsNullOrEmpty(tableKey) && fieldName.Contains(tableKey))
                    {
                        refTableName = tableKey;
                        break;
                    }
                }
            }
            
            if (string.IsNullOrEmpty(refTableName))
            {
                EditorUtility.DisplayDialog("提示", $"无法确定引用表名，字段: {fieldDef.Name}", "确定");
                Debug.LogWarning($"无法确定引用表名，字段: {fieldDef.Name}");
                return;
            }
            
            // 打开引用对象窗口
            ReferencedObjectWindow.OpenWindow(refId, refTableName, _selectedFolder, _tableMappings, _currentExcelPath);
        }
    }
}
#endif

