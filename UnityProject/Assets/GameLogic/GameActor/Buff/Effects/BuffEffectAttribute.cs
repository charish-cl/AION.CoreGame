using System;
using GameConfig;

namespace GameLogic
{
    /// <summary>
    /// Buff效果特性，用于标记Effect对应的BuffType
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BuffEffectAttribute : Attribute
    {
        public EBuffType BuffType { get; private set; }

        public BuffEffectAttribute(EBuffType buffType)
        {
            BuffType = buffType;
        }
    }
}
