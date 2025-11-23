using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// Buff效果基类，使用策略模式实现不同类型的Buff效果
    /// </summary>
    public abstract class BaseEffect
    {
        /// <summary>
        /// 目标Actor
        /// </summary>
        protected GameActor TargetActor { get; private set; }
        
        /// <summary>
        /// 数值参数列表
        /// </summary>
        protected List<float> ValueParams { get; private set; }
        
        /// <summary>
        /// 攻击者的数值组件（用于伤害计算等）
        /// </summary>
        protected NumericComponent AttackerNumeric { get; private set; }
        
        /// <summary>
        /// 攻击者的Actor（用于反伤等需要攻击者引用的场景）
        /// </summary>
        protected GameActor AttackerActor { get; private set; }
        
        /// <summary>
        /// 设置攻击者信息（用于在OnStart时更新）
        /// </summary>
        public void SetAttacker(NumericComponent attackerNumeric, GameActor attackerActor = null)
        {
            AttackerNumeric = attackerNumeric;
            AttackerActor = attackerActor ?? (attackerNumeric?.Actor);
        }
        
        /// <summary>
        /// 设置目标Actor（用于在OnStart时更新）
        /// </summary>
        public void SetTargetActor(GameActor targetActor)
        {
            TargetActor = targetActor;
        }
        
        /// <summary>
        /// 状态ID（用于状态效果）
        /// </summary>
        protected int StatusId { get; private set; }

        /// <summary>
        /// 构造函数，初始化参数
        /// </summary>
        protected BaseEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0
        )
        {
            TargetActor = targetActor;
            ValueParams = valueParams ?? new List<float>();
            AttackerNumeric = attackerNumeric;
            AttackerActor = attackerActor;
            StatusId = statusId;
        }

        /// <summary>
        /// 获取参数：根据类的字段/属性自动从ValueParams填充
        /// 按照字段/属性的声明顺序依次填充
        /// </summary>
        /// <typeparam name="T">参数类型（必须有无参构造函数）</typeparam>
        /// <returns>填充后的参数对象</returns>
        protected T GetParam<T>() where T : new()
        {
            T result = new T();
            
            if (ValueParams == null || ValueParams.Count == 0)
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
                if (paramIndex >= ValueParams.Count)
                    break;

                if (TrySetValue(field.FieldType, ValueParams[paramIndex], field, result))
                {
                    paramIndex++;
                }
            }

            // 再处理属性
            foreach (var prop in properties)
            {
                if (paramIndex >= ValueParams.Count)
                    break;

                if (prop.CanWrite && TrySetValue(prop.PropertyType, ValueParams[paramIndex], prop, result))
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
                Log.Warning($"BaseEffect.GetParam: 设置 {member.Name} 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 应用效果
        /// </summary>
        public abstract void Apply();
    }
}
