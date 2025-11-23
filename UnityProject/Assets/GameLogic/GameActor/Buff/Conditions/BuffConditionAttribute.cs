using System;
using GameLogic;
using GameConfig;

namespace GameLogic
{
    /// <summary>
    /// Buff条件特性，用于标记Condition对应的条件类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BuffConditionAttribute : Attribute
    {
        public ETriggerType ConditionType { get; private set; }

        public BuffConditionAttribute( ETriggerType conditionType)
        {
            ConditionType = conditionType;
        }
    }
}
