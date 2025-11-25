#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace GameLogic.Editor.ConfigEditor.Schema
{
    /// <summary>
    /// 基于 EPPlus 的表结构解析器
    /// </summary>
    public class ExcelTableSchemaProvider : ITableSchemaProvider
    {
        private static readonly string[] HeaderMarkers = { "##var", "##type", "##group", "##" };
        private readonly string _excelPath;
        private readonly string _defaultSheetName;

        private static bool _licenseInitialized = false;
        
        static ExcelTableSchemaProvider()
        {
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
        }

        public ExcelTableSchemaProvider(string excelPath, string defaultSheetName = null)
        {
            _excelPath = excelPath ?? throw new ArgumentNullException(nameof(excelPath));
            _defaultSheetName = defaultSheetName;
        }

        public TableSchema LoadSchema(string tableName)
        {
            UnityEngine.Debug.Log($"LoadSchema: 开始加载表结构，表名: {tableName}");
            
            try
            {
                using (var package = OpenPackage())
                {
                    UnityEngine.Debug.Log($"LoadSchema: Excel包已打开");
                    
                    var worksheet = ResolveWorksheet(package.Workbook, tableName);
                    if (worksheet == null)
                    {
                        UnityEngine.Debug.LogError($"LoadSchema: 未找到工作表: {tableName ?? _defaultSheetName}");
                        throw new InvalidOperationException($"未找到工作表: {tableName ?? _defaultSheetName}");
                    }

                    UnityEngine.Debug.Log($"LoadSchema: 找到工作表: {worksheet.Name}, 维度: Start.Row={worksheet.Dimension?.Start.Row}, End.Row={worksheet.Dimension?.End.Row}, Start.Column={worksheet.Dimension?.Start.Column}, End.Column={worksheet.Dimension?.End.Column}");

                    var schema = ParseSheet(worksheet, tableName ?? worksheet.Name);
                    
                    UnityEngine.Debug.Log($"LoadSchema: ParseSheet返回，字段数: {schema?.Fields?.Count ?? 0}");
                    
                    return schema;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"LoadSchema: 加载表结构时发生异常: {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        public void SaveSchema(TableSchema schema)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            using (var package = OpenPackage(createIfMissing: true))
            {
                var worksheet = ResolveWorksheet(package.Workbook, schema.TableName, createIfMissing: true);
                WriteSchema(worksheet, schema);
                package.Save();
            }
        }

        public void AddField(string tableName, FieldSchema field, int insertIndex = -1)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            var schema = LoadSchema(tableName);
            if (insertIndex < 0 || insertIndex > schema.Fields.Count)
            {
                insertIndex = schema.Fields.Count;
            }
            schema.Fields.Insert(insertIndex, field);
            SaveSchema(schema);
        }

        public void UpdateField(string tableName, FieldSchema updatedField)
        {
            if (updatedField == null)
                throw new ArgumentNullException(nameof(updatedField));

            var schema = LoadSchema(tableName);
            var existing = schema.FindField(updatedField.Name);
            if (existing == null)
                throw new InvalidOperationException($"字段 {updatedField.Name} 不存在于 {tableName}");

            var index = schema.Fields.IndexOf(existing);
            schema.Fields[index] = updatedField;
            SaveSchema(schema);
        }

        public void RemoveField(string tableName, string fieldName)
        {
            var schema = LoadSchema(tableName);
            var field = schema.FindField(fieldName);
            if (field == null)
                throw new InvalidOperationException($"字段 {fieldName} 不存在于 {tableName}");

            schema.Fields.Remove(field);
            SaveSchema(schema);
        }

        public List<string> GetAllTableNames()
        {
            using (var package = OpenPackage())
            {
                return package.Workbook.Worksheets.Select(ws => ws.Name).ToList();
            }
        }

        private TableSchema ParseSheet(ExcelWorksheet sheet, string tableName)
        {
            UnityEngine.Debug.Log($"ParseSheet: 开始解析工作表 {sheet.Name}, 表名: {tableName}");
            
            var schema = new TableSchema
            {
                TableName = tableName,
                Description = sheet.Cells[1, 1].GetValue<string>()
            };

            var markerRows = DetectHeaderRows(sheet);
            UnityEngine.Debug.Log($"ParseSheet: 找到 {markerRows.Count} 个标记行: {string.Join(", ", markerRows.Select(kv => $"{kv.Key}={kv.Value}"))}");
            
            if (!markerRows.TryGetValue("var", out var varRow))
            {
                UnityEngine.Debug.LogError($"ParseSheet: 工作表 {sheet.Name} 缺少 ##var 行");
                throw new InvalidOperationException($"工作表 {sheet.Name} 缺少 ##var 行");
            }

            UnityEngine.Debug.Log($"ParseSheet: ##var 行在第 {varRow} 行");

            var typeRow = markerRows.TryGetValue("type", out var tRow) ? tRow : 0;
            var groupRow = markerRows.TryGetValue("group", out var gRow) ? gRow : 0;
            var commentRow = markerRows.TryGetValue("comment", out var cRow) ? cRow : 0;

            UnityEngine.Debug.Log($"ParseSheet: type行={typeRow}, group行={groupRow}, comment行={commentRow}");

            // 计算数据开始行：##comment 行下面的行（commentRow + 1）
            // 如果找不到 comment 行，则使用 varRow + 4（假设标准格式：var, type, group, comment）
            int dataStartRow = commentRow > 0 ? commentRow + 1 : (varRow + 4);
            schema.DataStartRow = dataStartRow;
            UnityEngine.Debug.Log($"ParseSheet: 数据开始行设置为: {dataStartRow} (comment行: {commentRow})");

            // 使用检测到的行号，而不是硬编码
            var names = ReadRowValues(sheet, varRow);
            var types = ReadRowValues(sheet, typeRow);
            var groups = ReadRowValues(sheet, groupRow);
            var comments = ReadRowValues(sheet, commentRow);

            UnityEngine.Debug.Log($"ParseSheet: 读取到 {names.Length} 个字段名: {string.Join(", ", names)}");

            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                if (string.IsNullOrWhiteSpace(name))
                {
                    UnityEngine.Debug.Log($"ParseSheet: 跳过空字段名，索引: {i}");
                    continue;
                }

                var field = new FieldSchema
                {
                    Name = name,
                    RawType = SafeGet(types, i),
                    Type = NormalizeType(SafeGet(types, i)),
                    Group = SafeGet(groups, i),
                    Comment = SafeGet(comments, i),
                    DisplayName = SafeGet(comments, i),
                    Extra = string.Empty
                };

                if (string.IsNullOrEmpty(field.DisplayName))
                {
                    field.DisplayName = name;
                }

                schema.Fields.Add(field);
                UnityEngine.Debug.Log($"ParseSheet: 添加字段 {field.Name}, 类型: {field.RawType ?? field.Type}");
            }

            UnityEngine.Debug.Log($"ParseSheet: 解析完成，共 {schema.Fields.Count} 个字段，数据开始行: {schema.DataStartRow}");
            return schema;
        }

        private void WriteSchema(ExcelWorksheet sheet, TableSchema schema)
        {
            EnsureHeaderRows(sheet);

            WriteRow(sheet, 1, "##var", schema.Fields.Select(f => f.Name));
            WriteRow(sheet, 2, "##type", schema.Fields.Select(f => f.RawType ?? f.Type));
            WriteRow(sheet, 3, "##group", schema.Fields.Select(f => f.Group));
            WriteRow(sheet, 4, "##", schema.Fields.Select(f => f.Comment ?? f.DisplayName ?? f.Name));
        }

        private Dictionary<string, int> DetectHeaderRows(ExcelWorksheet sheet)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int startRow = sheet.Dimension?.Start.Row ?? 1;
            int endRow = sheet.Dimension?.End.Row ?? 1;

            UnityEngine.Debug.Log($"DetectHeaderRows: 开始检测，行范围: {startRow} 到 {endRow}");

            for (int row = startRow; row <= endRow; row++)
            {
                var marker = sheet.Cells[row, 1].GetValue<string>();
                if (string.IsNullOrWhiteSpace(marker) || !marker.StartsWith("##"))
                    continue;

                string key = marker.TrimStart('#').Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(key))
                {
                    key = "comment";
                }

                // var以第一个出现的为准，其他标记行记录最后一个出现的
                if (key == "var")
                {
                    // 只记录第一个出现的 var
                    if (!dict.ContainsKey(key))
                    {
                        dict[key] = row;
                        UnityEngine.Debug.Log($"DetectHeaderRows: 找到第一个 ##var 行在第 {row} 行");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"DetectHeaderRows: 跳过重复的 ##var 行在第 {row} 行（已存在第 {dict[key]} 行）");
                    }
                }
                else
                {
                    // 其他标记行记录最后一个出现的
                    dict[key] = row;
                    UnityEngine.Debug.Log($"DetectHeaderRows: 找到标记行 {key} 在第 {row} 行");
                }
            }

            UnityEngine.Debug.Log($"DetectHeaderRows: 检测完成，找到 {dict.Count} 个标记行");
            return dict;
        }

        private string[] ReadRowValues(ExcelWorksheet sheet, int row)
        {
            if (row <= 0)
            {
                UnityEngine.Debug.Log($"ReadRowValues: 行号无效: {row}");
                return Array.Empty<string>();
            }

            int startColumn = (sheet.Dimension?.Start.Column ?? 1) + 1;
            int endColumn = sheet.Dimension?.End.Column ?? startColumn;
            
            UnityEngine.Debug.Log($"ReadRowValues: 行 {row}, 列范围: {startColumn} 到 {endColumn}");
            
            var values = new List<string>();

            for (int col = startColumn; col <= endColumn; col++)
            {
                var cellValue = sheet.Cells[row, col].Value;
                var text = cellValue?.ToString()?.Trim() ?? string.Empty;
                values.Add(text);
                
                if (col <= 5) // 只记录前5列用于调试
                {
                    UnityEngine.Debug.Log($"ReadRowValues: 第{row}行第{col}列 = '{text}' (原始值: {cellValue?.GetType().Name ?? "null"})");
                }
            }

            UnityEngine.Debug.Log($"ReadRowValues: 行 {row} 读取完成，共 {values.Count} 个值");
            return values.ToArray();
        }

        private void WriteRow(ExcelWorksheet sheet, int row, string marker, IEnumerable<string> values)
        {
            sheet.Cells[row, 1].Value = marker;
            int col = 2;
            foreach (var value in values)
            {
                sheet.Cells[row, col].Value = value;
                col++;
            }

            int endColumn = sheet.Dimension?.End.Column ?? col;
            for (; col <= endColumn; col++)
            {
                sheet.Cells[row, col].Clear();
            }
        }

        private void EnsureHeaderRows(ExcelWorksheet sheet)
        {
            for (int i = 0; i < HeaderMarkers.Length; i++)
            {
                int row = i + 1;
                sheet.Cells[row, 1].Value = HeaderMarkers[i];
            }
        }

        private static string SafeGet(string[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length)
                return string.Empty;
            return array[index];
        }

        private static string NormalizeType(string rawType)
        {
            if (string.IsNullOrWhiteSpace(rawType))
                return string.Empty;

            // 取分号后的最后一段作为类型定义，例如 "(list);item.ItemExchange"
            var segments = rawType.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var last = segments.LastOrDefault()?.Trim();
            return last ?? rawType.Trim();
        }

        private ExcelPackage OpenPackage(bool createIfMissing = false)
        {
            if (!File.Exists(_excelPath))
            {
                if (!createIfMissing)
                {
                    throw new FileNotFoundException($"未找到Excel文件: {_excelPath}");
                }

                var file = new FileInfo(_excelPath);
                var package = new ExcelPackage(file);
                package.Save();
                return package;
            }
            else
            {
                return new ExcelPackage(new FileInfo(_excelPath));
            }
        }

        private ExcelWorksheet ResolveWorksheet(ExcelWorkbook workbook, string tableName, bool createIfMissing = false)
        {
            ExcelWorksheet worksheet = null;

            if (!string.IsNullOrEmpty(tableName))
            {
                worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == tableName);
            }

            if (worksheet == null && !string.IsNullOrEmpty(_defaultSheetName))
            {
                worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == _defaultSheetName);
            }

            if (worksheet == null && createIfMissing)
            {
                var name = tableName ?? _defaultSheetName ?? "Sheet1";
                worksheet = workbook.Worksheets.Add(name);
            }

            return worksheet;
        }
    }
}
#endif

