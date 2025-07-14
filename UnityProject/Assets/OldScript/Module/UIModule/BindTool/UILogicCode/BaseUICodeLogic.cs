using System;
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
        public GameObject Parent;
        
        protected UIDataSource DataSource;
        
        public abstract string FieldName { get; } 
        
        public abstract string FieldCode { get; }
        /// <summary>
        /// 方法的代码
        /// </summary>
        public virtual string MethodCode { get;  } = "";
        
        /// <summary>
        /// 初始化代码
        /// </summary>
        public virtual string InitalizeCode { get; } = "";
        
        /// <summary>
        /// show代码
        /// </summary>
        public virtual void Refresh()
        {
            
        }
        public void InitParent( GameObject parent)
        {
            Parent = parent;
        }
    }
}