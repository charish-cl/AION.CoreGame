using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace OldScript.Editor.SuperTableBuilder
{
    public class ExcelHelper
    {
        /// <summary>
        /// 从EXCEL 导入数据到DataTable ，只支持.xlsx 不支持.xls
        /// </summary>
        /// <param name="filePath">EXCEL文件路径</param>
        /// <returns></returns>
        public static DataTable ReadExcel(string filePath)
        {
            string sExt = System.IO.Path.GetExtension(filePath);
            sExt = sExt.ToUpper();

            if (sExt == ".XLSX")
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var workbook = package.Workbook;
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    var startRow = worksheet.Dimension.Start.Row;
                    var endRow = worksheet.Dimension.End.Row;
                    var startColumn = worksheet.Dimension.Start.Column;
                    var endColumn = worksheet.Dimension.End.Column;
                    var table = new DataTable();
                    for (int i = startColumn; i <= endColumn; i++)
                    {
                        table.Columns.Add(worksheet.Cells[startRow, i].Value.ToString());
                    }

                    for (int row = startRow + 1; row <= endRow; row++)
                    {
                        var dataRow = table.NewRow();

                        for (int col = startColumn; col <= endColumn; col++)
                        {
                            dataRow[col - 1] = worksheet.Cells[row, col].Value;
                        }

                        table.Rows.Add(dataRow);
                    }

                    return table;
                }
            }
            else
            {
                throw new Exception("文件格式有误,只能使用.xlsx");
            }
        }

        public static void ExcelToSO<T>(string excelPath, string soAssetPath) where T : ScriptableObject
        {
            // 1. 加载SO实例
            T so = AssetDatabase.LoadAssetAtPath<T>(soAssetPath);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(so, soAssetPath);
            }

            DataTable table = ReadExcel(excelPath);
            
            
            // 5. 保存修改
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            Debug.Log($"Excel数据成功导入到 {soAssetPath}");
        }

        public static void SOToExcel<T>(T so, string excelPath) where T : ScriptableObject
        {
            FileInfo file = new FileInfo(excelPath);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // 创建工作表
                ExcelWorksheet sheet = package.Workbook.Worksheets.Count > 0
                    ? package.Workbook.Worksheets[0]
                    : package.Workbook.Worksheets.Add("Data");

                // 写入表头
                FieldInfo[] fields = typeof(T).GetFields(
                    BindingFlags.Public | BindingFlags.Instance);

                for (int i = 0; i < fields.Length; i++)
                {
                    sheet.Cells[1, i + 1].Value = fields[i].Name;
                }

                // 写入数据
                for (int i = 0; i < fields.Length; i++)
                {
                    object value = fields[i].GetValue(so);
                    sheet.Cells[2, i + 1].Value = value?.ToString() ?? "";
                }

                package.Save();
            }

            Debug.Log($"{AssetDatabase.GetAssetPath(so)} 已导出到 {excelPath}");
        }

        private static void SetFieldValue(object target, FieldInfo field, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            try
            {
                System.Type type = field.FieldType;
                if (type == typeof(int)) field.SetValue(target, int.Parse(value));
                else if (type == typeof(float)) field.SetValue(target, float.Parse(value));
                else if (type == typeof(bool)) field.SetValue(target, bool.Parse(value));
                else if (type == typeof(string)) field.SetValue(target, value);
                else if (type.IsEnum) field.SetValue(target, System.Enum.Parse(type, value));
                // 添加其他类型支持...
            }
            catch (System.Exception e)
            {
                Debug.LogError($"字段 {field.Name} 转换失败: {e.Message}");
            }
        }
    }
}