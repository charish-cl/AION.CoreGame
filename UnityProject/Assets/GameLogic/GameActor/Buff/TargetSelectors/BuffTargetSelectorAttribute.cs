using System;
using GameConfig;

namespace GameLogic
{
    /// <summary>
    /// Buff目标选择器特性，用于标记TargetSelector对应的TargetType
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BuffTargetSelectorAttribute : Attribute
    {
        public ETargetType TargetType { get; private set; }

        public BuffTargetSelectorAttribute(ETargetType targetType)
        {
            TargetType = targetType;
        }
    }
}

