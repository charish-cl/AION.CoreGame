#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;
using UnityEngine;
using GameLogic.Editor.ConfigEditor.Schema;

namespace GameLogic.Editor
{
    /// <summary>
    /// Excel编辑器辅助类
    /// </summary>
    public static class ExcelEditorHelper
    {
        /// <summary>
        /// 表格映射信息
        /// </summary>
        public class TableMapping
        {
            public string FullName { get; set; }  // 例如: item.TbItem
            public string ValueType { get; set; }  // 例如: ItemConfig
            public string Input { get; set; }      // Excel文件路径
            public bool ReadSchemaFromFile { get; set; }
        }
        
        /// <summary>
        /// 解析__tables__.xlsx文件，获取表格映射关系
        /// </summary>
        public static Dictionary<string, TableMapping> ParseTablesMapping(string tablesFilePath)
        {
            var mappings = new Dictionary<string, TableMapping>();
            
            if (!File.Exists(tablesFilePath))
            {
                Debug.LogWarning($"未找到__tables__.xlsx文件: {tablesFilePath}");
                return mappings;
            }
            
            Debug.Log($"开始解析__tables__.xlsx: {tablesFilePath}");
            
            try
            {
                using (var package = new ExcelPackage(new FileInfo(tablesFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        Debug.LogWarning("__tables__.xlsx中没有找到工作表");
                        return mappings;
                    }
                    
                    Debug.Log($"找到工作表: {worksheet.Name}");
                    
                    var dimension = worksheet.Dimension;
                    if (dimension == null)
                    {
                        Debug.LogWarning("工作表维度为null");
                        return mappings;
                    }
                    
                    Debug.Log($"工作表维度: Start.Row={dimension.Start.Row}, End.Row={dimension.End.Row}, Start.Column={dimension.Start.Column}, End.Column={dimension.End.Column}");
                    
                    // 查找表头行
                    int headerRow = -1;
                    int fullNameCol = -1, valueTypeCol = -1, inputCol = -1, readSchemaCol = -1;
                    
                    for (int row = 1; row <= Math.Min(10, dimension.End.Row); row++)
                    {
                        for (int col = 1; col <= dimension.End.Column; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? "";
                            if (cellValue == "full_name")
                            {
                                headerRow = row;
                                fullNameCol = col;
                                Debug.Log($"找到full_name列: 行{row}, 列{col}");
                            }
                            else if (cellValue == "value_type")
                            {
                                valueTypeCol = col;
                                Debug.Log($"找到value_type列: 行{row}, 列{col}");
                            }
                            else if (cellValue == "input")
                            {
                                inputCol = col;
                                Debug.Log($"找到input列: 行{row}, 列{col}");
                            }
                            else if (cellValue == "read_schema_from_file")
                            {
                                readSchemaCol = col;
                                Debug.Log($"找到read_schema_from_file列: 行{row}, 列{col}");
                            }
                        }
                        
                        if (headerRow > 0 && fullNameCol > 0 && valueTypeCol > 0 && inputCol > 0)
                        {
                            Debug.Log($"找到完整表头，行: {headerRow}");
                            break;
                        }
                    }
                    
                    if (headerRow <= 0)
                    {
                        Debug.LogWarning("无法找到__tables__.xlsx的表头行");
                        return mappings;
                    }
                    
                    // 读取数据行
                    int mappingCount = 0;
                    for (int row = headerRow + 1; row <= dimension.End.Row; row++)
                    {
                        var fullName = worksheet.Cells[row, fullNameCol].Value?.ToString() ?? "";
                        if (string.IsNullOrEmpty(fullName))
                        {
                            continue;
                        }
                        
                        var mapping = new TableMapping
                        {
                            FullName = fullName,
                            ValueType = worksheet.Cells[row, valueTypeCol].Value?.ToString() ?? "",
                            Input = worksheet.Cells[row, inputCol].Value?.ToString() ?? "",
                            ReadSchemaFromFile = GetBoolValue(worksheet.Cells[row, readSchemaCol].Value)
                        };
                        
                        Debug.Log($"解析映射: FullName={mapping.FullName}, ValueType={mapping.ValueType}, Input={mapping.Input}");
                        
                        // 提取表名（例如从 item.TbItem 提取 TbItem）
                        var tableName = fullName.Split('.').LastOrDefault();
                        if (!string.IsNullOrEmpty(tableName))
                        {
                            mappings[tableName] = mapping;
                            mappingCount++;
                        }
                        
                        // 也可以使用完整名称作为key
                        mappings[fullName] = mapping;
                    }
                    
                    Debug.Log($"解析完成，共找到 {mappingCount} 个映射");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"解析__tables__.xlsx失败: {e.Message}\n{e.StackTrace}");
            }
            
            return mappings;
        }
        
        /// <summary>
        /// 解析Excel文件的前4行，获取字段定义（使用ExcelTableSchemaProvider）
        /// </summary>
        public static TableSchema ParseTableSchema(string excelPath, string sheetName = null)
        {
            if (string.IsNullOrEmpty(excelPath) || !File.Exists(excelPath))
            {
                Debug.LogWarning($"ParseTableSchema: Excel文件不存在: {excelPath}");
                return null;
            }
            
            Debug.Log($"ParseTableSchema: 开始解析，文件: {excelPath}, 工作表: {sheetName ?? "默认"}");
            
            try
            {
                var provider = new ExcelTableSchemaProvider(excelPath, sheetName);
                var allTableNames = provider.GetAllTableNames();
                Debug.Log($"ParseTableSchema: 找到 {allTableNames.Count} 个工作表: {string.Join(", ", allTableNames)}");
                
                // 如果指定了sheetName，优先使用它
                string actualSheetName = null;
                if (!string.IsNullOrEmpty(sheetName))
                {
                    if (allTableNames.Contains(sheetName))
                    {
                        actualSheetName = sheetName;
                        Debug.Log($"ParseTableSchema: 使用指定的工作表: {actualSheetName}");
                    }
                    else
                    {
                        Debug.LogWarning($"ParseTableSchema: 指定的工作表 '{sheetName}' 不存在，可用工作表: {string.Join(", ", allTableNames)}");
                        actualSheetName = allTableNames.FirstOrDefault();
                    }
                }
                else
                {
                    actualSheetName = allTableNames.FirstOrDefault();
                }
                
                if (string.IsNullOrEmpty(actualSheetName))
                {
                    Debug.LogWarning("ParseTableSchema: 没有找到可用的工作表");
                    return null;
                }
                
                Debug.Log($"ParseTableSchema: 最终使用工作表: {actualSheetName}");
                
                TableSchema schema = null;
                try
                {
                    schema = provider.LoadSchema(actualSheetName);
                }
                catch (Exception loadEx)
                {
                    Debug.LogError($"ParseTableSchema: LoadSchema抛出异常: {loadEx.Message}\n{loadEx.StackTrace}");
                    // 尝试直接解析工作表
                    try
                    {
                        using (var package = new ExcelPackage(new FileInfo(excelPath)))
                        {
                            var worksheet = package.Workbook.Worksheets[actualSheetName];
                            if (worksheet != null)
                            {
                                Debug.Log($"ParseTableSchema: 直接解析工作表，维度: Start.Row={worksheet.Dimension?.Start.Row}, End.Row={worksheet.Dimension?.End.Row}, Start.Column={worksheet.Dimension?.Start.Column}, End.Column={worksheet.Dimension?.End.Column}");
                                
                                // 检查前几行的内容
                                for (int row = 1; row <= Math.Min(5, worksheet.Dimension?.End.Row ?? 5); row++)
                                {
                                    var rowContent = new List<string>();
                                    for (int col = 1; col <= Math.Min(10, worksheet.Dimension?.End.Column ?? 10); col++)
                                    {
                                        var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? "";
                                        rowContent.Add(cellValue);
                                    }
                                    Debug.Log($"  第{row}行内容: {string.Join(" | ", rowContent)}");
                                }
                            }
                        }
                    }
                    catch (Exception directEx)
                    {
                        Debug.LogError($"ParseTableSchema: 直接解析也失败: {directEx.Message}");
                    }
                    return null;
                }
                
                if (schema != null)
                {
                    Debug.Log($"ParseTableSchema: 解析成功，表名: {schema.TableName}, 字段数: {schema.Fields?.Count ?? 0}");
                    if (schema.Fields == null || schema.Fields.Count == 0)
                    {
                        Debug.LogWarning($"ParseTableSchema: 警告！表结构解析成功但字段列表为空或null");
                        
                        // 尝试直接检查工作表内容
                        try
                        {
                            using (var package = new ExcelPackage(new FileInfo(excelPath)))
                            {
                                var worksheet = package.Workbook.Worksheets[actualSheetName];
                                if (worksheet != null && worksheet.Dimension != null)
                                {
                                    Debug.Log($"直接检查工作表内容:");
                                    Debug.Log($"  维度: Start.Row={worksheet.Dimension.Start.Row}, End.Row={worksheet.Dimension.End.Row}, Start.Column={worksheet.Dimension.Start.Column}, End.Column={worksheet.Dimension.End.Column}");
                                    
                                    // 检查##var行
                                    for (int row = 1; row <= Math.Min(10, worksheet.Dimension.End.Row); row++)
                                    {
                                        var marker = worksheet.Cells[row, 1].Value?.ToString() ?? "";
                                        if (marker.StartsWith("##"))
                                        {
                                            Debug.Log($"  找到标记行: 第{row}行, 标记: {marker}");
                                            
                                            // 读取这一行的所有列
                                            var rowValues = new List<string>();
                                            for (int col = 2; col <= worksheet.Dimension.End.Column; col++)
                                            {
                                                var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? "";
                                                rowValues.Add(cellValue);
                                            }
                                            Debug.Log($"    列值: {string.Join(", ", rowValues)}");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception checkEx)
                        {
                            Debug.LogError($"检查工作表内容失败: {checkEx.Message}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("ParseTableSchema: LoadSchema返回null");
                }
                
                return schema;
            }
            catch (Exception e)
            {
                Debug.LogError($"ParseTableSchema: 解析表结构失败: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }
        
        /// <summary>
        /// 从TableSchema转换为带列索引的字段列表（用于对象模式）
        /// </summary>
        public static List<FieldSchemaWithIndex> ConvertSchemaToFieldList(TableSchema schema)
        {
            var fields = new List<FieldSchemaWithIndex>();
            
            if (schema == null)
            {
                Debug.LogWarning("ConvertSchemaToFieldList: schema为null");
                return fields;
            }
            
            if (schema.Fields == null)
            {
                Debug.LogWarning($"ConvertSchemaToFieldList: schema.Fields为null，表名: {schema.TableName}");
                return fields;
            }
            
            if (schema.Fields.Count == 0)
            {
                Debug.LogWarning($"ConvertSchemaToFieldList: schema.Fields为空，表名: {schema.TableName}");
                return fields;
            }
            Debug.Log($"ConvertSchemaToFieldList: 开始转换，字段数: {schema.Fields.Count}");
            
            // 列索引从2开始（第1列是##标记）
            int columnIndex = 2;
            foreach (var field in schema.Fields)
            {
                if (field == null)
                {
                    Debug.LogWarning($"ConvertSchemaToFieldList: 发现null字段，跳过");
                    continue;
                }
                
                fields.Add(new FieldSchemaWithIndex
                {
                    Field = field,
                    ColumnIndex = columnIndex++
                });
                
                Debug.Log($"ConvertSchemaToFieldList: 添加字段 {field.Name}, 列索引: {columnIndex - 1}");
            }
            
            Debug.Log($"ConvertSchemaToFieldList: 转换完成，共 {fields.Count} 个字段");
            return fields;
        }
        
        /// <summary>
        /// 带列索引的字段结构
        /// </summary>
        public class FieldSchemaWithIndex
        {
            public FieldSchema Field { get; set; }
            public int ColumnIndex { get; set; }
        }
        
        /// <summary>
        /// 通过反射获取类的字段信息
        /// </summary>
        public static List<FieldInfo> GetClassFields(Type classType)
        {
            var fields = new List<FieldInfo>();
            
            if (classType == null)
            {
                return fields;
            }
            
            // 获取所有public readonly字段（Luban生成的类通常使用readonly字段）
            var allFields = classType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in allFields)
            {
                // 跳过_Ref字段和内部字段
                if (field.Name.EndsWith("_Ref") || field.Name.StartsWith("__"))
                {
                    continue;
                }
                
                fields.Add(field);
            }
            
            return fields;
        }
        
        /// <summary>
        /// 获取字段的注释（从XML文档注释中提取）
        /// </summary>
        public static string GetFieldComment(FieldInfo field)
        {
            // 尝试从XML注释中获取
            // 这里简化处理，实际可以使用XML文档解析
            var summaryAttr = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            if (summaryAttr != null)
            {
                return summaryAttr.Description;
            }
            
            // 如果没有Description特性，返回字段名
            return field.Name;
        }
        
        /// <summary>
        /// 根据类型名称查找对应的Type（优先使用从__tables__.xlsx解析的命名空间）
        /// </summary>
        public static Type FindTypeByName(string typeName, string namespaceHint = null)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogWarning("FindTypeByName: typeName为空");
                return null;
            }
            
            // 如果提供了命名空间提示（从__tables__.xlsx解析），优先使用
            if (!string.IsNullOrEmpty(namespaceHint))
            {
                var fullTypeName = $"{namespaceHint}.{typeName}";
                Debug.Log($"FindTypeByName: 使用从__tables__.xlsx解析的命名空间: {fullTypeName}");
                
                // 先尝试直接获取
                var type = Type.GetType(fullTypeName);
                if (type != null)
                {
                    Debug.Log($"  直接找到类型: {type.FullName}");
                    return type;
                }
                
                // 在所有程序集中查找
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        type = assembly.GetType(fullTypeName);
                        if (type != null)
                        {
                            Debug.Log($"  在程序集 {assembly.FullName} 中找到类型: {type.FullName}");
                            return type;
                        }
                    }
                    catch
                    {
                        // 忽略错误
                    }
                }
            }
            
            // 如果从__tables__.xlsx解析的命名空间找不到，尝试默认命名空间
            Debug.Log($"FindTypeByName: 尝试默认命名空间 GameConfig.{typeName}");
            var defaultTypeName = $"GameConfig.{typeName}";
            var defaultType = Type.GetType(defaultTypeName);
            if (defaultType != null)
            {
                Debug.Log($"  在默认命名空间找到类型: {defaultType.FullName}");
                return defaultType;
            }
            
            // 最后在所有程序集中搜索
            Debug.Log("  在所有程序集中搜索...");
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // 先尝试默认命名空间
                    var type = assembly.GetType(defaultTypeName);
                    if (type != null)
                    {
                        Debug.Log($"  在程序集 {assembly.FullName} 中找到类型: {type.FullName}");
                        return type;
                    }
                    
                    // 搜索所有以GameConfig开头的命名空间
                    var allTypes = assembly.GetTypes();
                    foreach (var t in allTypes)
                    {
                        if (t.Name == typeName && t.Namespace != null && t.Namespace.StartsWith("GameConfig"))
                        {
                            Debug.Log($"  在程序集 {assembly.FullName} 中搜索到类型: {t.FullName}");
                            return t;
                        }
                    }
                }
                catch (Exception e)
                {
                    // 忽略错误
                    Debug.LogWarning($"  程序集 {assembly.FullName} 查找时出错: {e.Message}");
                }
            }
            
            Debug.LogWarning($"FindTypeByName: 未找到类型 {typeName}");
            return null;
        }
        
        private static bool GetBoolValue(object value)
        {
            if (value == null)
            {
                return false;
            }
            
            if (value is bool)
            {
                return (bool)value;
            }
            
            var str = value.ToString().ToUpper();
            return str == "TRUE" || str == "1" || str == "YES";
        }
    }
}
#endif

