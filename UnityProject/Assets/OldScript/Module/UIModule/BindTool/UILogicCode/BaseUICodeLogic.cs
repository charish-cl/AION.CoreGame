using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AION.CoreFramework
{
    public enum CodeArea
    {
        OnCreate,
        RefreshUI,
        RegisterEvent,
        RefreshData
    }
    /// <summary>
    /// ui生成代码的基类
    /// </summary>
    [Serializable]
    public abstract class BaseUICodeLogic
    {
        [HideInInspector]
        public UIDataSourceBindData Parent;

        protected UIDataSource DataSource
        {
            get
            {
                if (Parent!=null)
                {
                    return Parent.UIDataSource;
                }
                return null;
            }
        }
        
        public abstract string FieldName { get; } 
        
        public abstract string FieldType { get; }
        
        public string FieldCode => $"{FieldType} {FieldName};";
        /// <summary>
        /// 方法的代码
        /// </summary>
        public virtual string MethodCode { get;  } = "";
        
        /// <summary>
        /// 初始化代码
        /// </summary>
        public virtual string RefreshCode { get; } = "";
        
        /// <summary>
        /// show代码
        /// </summary>
        public virtual void Refresh()
        {
            
        }
        public void InitParent( UIDataSourceBindData parent)
        {
            Parent = parent;
        }
        //获取小写第一个字母的名字
        public string GetLowerFirstLetterName(string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }

        public Type GetTypeByAssemblies(string typeName)
        {
            Assembly assembly = Assembly.Load("GameLogic");
            var type = assembly.GetType("GameLogic." + typeName);
            return type;
        }
        public Type GetTypeMByAssemblies(string typeName)
        {
            Assembly assembly = Assembly.Load("GameLogic");
            var type = assembly.GetType("GameLogic." + typeName);
            return type;
        }
  
        
    }
}