using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// Buff目标选择器基类，使用策略模式实现不同类型的目标选择
    /// </summary>
    public abstract class BaseTargetSelector
    {
        /// <summary>
        /// 源Actor（Buff的拥有者）
        /// </summary>
        protected GameActor SourceActor { get; private set; }
        
        /// <summary>
        /// 目标参数列表
        /// </summary>
        protected List<float> TargetParams { get; private set; }
        
        /// <summary>
        /// 攻击者的数值组件（用于目标选择判断）
        /// </summary>
        protected NumericComponent AttackerNumeric { get; private set; }
        
        /// <summary>
        /// 攻击者的Actor（用于目标选择判断）
        /// </summary>
        protected GameActor AttackerActor { get; private set; }
        
        /// <summary>
        /// 状态ID（用于状态相关目标选择）
        /// </summary>
        protected int StatusId { get; private set; }

        /// <summary>
        /// 构造函数，初始化参数
        /// </summary>
        protected BaseTargetSelector(
            GameActor sourceActor,
            List<float> targetParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0
        )
        {
            SourceActor = sourceActor;
            TargetParams = targetParams ?? new List<float>();
            AttackerNumeric = attackerNumeric;
            AttackerActor = attackerActor;
            StatusId = statusId;
        }

        /// <summary>
        /// 获取参数：根据类的字段/属性自动从TargetParams填充
        /// 按照字段/属性的声明顺序依次填充
        /// </summary>
        /// <typeparam name="T">参数类型（必须有无参构造函数）</typeparam>
        /// <returns>填充后的参数对象</returns>
        protected T GetParam<T>() where T : new()
        {
            T result = new T();
            
            if (TargetParams == null || TargetParams.Count == 0)
            {
                return result;
            }

            Type type = typeof(T);
            int paramIndex = 0;

            // 获取所有公共字段和属性，按声明顺序
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            // 先处理字段
            foreach (var field in fields)
            {
                if (paramIndex >= TargetParams.Count)
                    break;

                if (TrySetValue(field.FieldType, TargetParams[paramIndex], field, result))
                {
                    paramIndex++;
                }
            }

            // 再处理属性
            foreach (var prop in properties)
            {
                if (paramIndex >= TargetParams.Count)
                    break;

                if (prop.CanWrite && TrySetValue(prop.PropertyType, TargetParams[paramIndex], prop, result))
                {
                    paramIndex++;
                }
            }

            return result;
        }

        /// <summary>
        /// 尝试设置值
        /// </summary>
        private bool TrySetValue(Type targetType, float value, MemberInfo member, object target)
        {
            try
            {
                object convertedValue = null;

                // 根据目标类型转换值
                if (targetType == typeof(float))
                {
                    convertedValue = value;
                }
                else if (targetType == typeof(int))
                {
                    convertedValue = (int)value;
                }
                else if (targetType == typeof(double))
                {
                    convertedValue = (double)value;
                }
                else if (targetType == typeof(bool))
                {
                    convertedValue = value != 0f;
                }
                else
                {
                    // 不支持的类型，跳过
                    return false;
                }

                // 设置值
                if (member is FieldInfo field)
                {
                    field.SetValue(target, convertedValue);
                }
                else if (member is PropertyInfo prop)
                {
                    prop.SetValue(target, convertedValue);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"BaseTargetSelector.GetParam: 设置 {member.Name} 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 选择目标
        /// </summary>
        /// <returns>选中的目标Actor列表</returns>
        public abstract List<GameActor> SelectTargets();
    }
}

