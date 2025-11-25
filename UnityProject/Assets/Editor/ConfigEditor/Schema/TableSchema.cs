#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLogic.Editor.ConfigEditor.Schema
{
    /// <summary>
    /// 表结构描述
    /// </summary>
    [Serializable]
    public class TableSchema
    {
        public string TableName { get; set; }
        public string Description { get; set; }
        
        /// <summary>
        /// 数据开始行（##comment 行下面的行，即 commentRow + 1）
        /// </summary>
        public int DataStartRow { get; set; } = 5;

        public List<FieldSchema> Fields { get; } = new List<FieldSchema>();

        public FieldSchema FindField(string fieldName)
        {
            return Fields.FirstOrDefault(f => f.Name == fieldName);
        }

        public TableSchema Clone()
        {
            var clone = new TableSchema
            {
                TableName = TableName,
                Description = Description
            };

            foreach (var field in Fields)
            {
                clone.Fields.Add(field.Clone());
            }

            if (clone.Fields.Count == 0)
            {
               Debug.LogWarning("TableSchema.Clone: no fields found.");
            }
            return clone;
        }
    }

    /// <summary>
    /// 字段结构描述
    /// </summary>
    [Serializable]
    public class FieldSchema
    {
        public string Name;
        public string DisplayName;
        public string Type;
        public string RawType;
        public string Group;
        public string Comment;
        public string DefaultValue;
        public string Extra;
        public bool IsRequired;

        public FieldSchema Clone()
        {
            return (FieldSchema)MemberwiseClone();
        }
    }
}
#endif

