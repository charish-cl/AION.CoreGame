#if UNITY_EDITOR
using System.Collections.Generic;

namespace GameLogic.Editor.ConfigEditor.Schema
{
    /// <summary>
    /// 表结构解析器接口
    /// </summary>
    public interface ITableSchemaProvider
    {
        /// <summary>
        /// 加载表结构
        /// </summary>
        TableSchema LoadSchema(string tableName);

        /// <summary>
        /// 保存表结构
        /// </summary>
        void SaveSchema(TableSchema schema);

        /// <summary>
        /// 添加字段
        /// </summary>
        void AddField(string tableName, FieldSchema field, int insertIndex = -1);

        /// <summary>
        /// 更新字段
        /// </summary>
        void UpdateField(string tableName, FieldSchema updatedField);

        /// <summary>
        /// 删除字段
        /// </summary>
        void RemoveField(string tableName, string fieldName);

        /// <summary>
        /// 获取所有表名（如果支持）
        /// </summary>
        List<string> GetAllTableNames();
    }
}
#endif

