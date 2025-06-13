using System;
using Sirenix.OdinInspector;

namespace AION.Config
{
    [LabelText("伤害类型")]
    [Flags]
    public enum EnumDamageType
    {
        [LabelText("物理伤害")]
        Physical    = 1,
        [LabelText("魔法伤害")]
        Magical     = 2,
        
        [LabelText("所有伤害")]
        All = Physical | Magical,
    }


    [LabelText("魔法伤害类型")]
    public enum EnumMagicalDamageType
    {
        [LabelText("火")]
        Fire,
        [LabelText("冰")] 
        Ice,
        [LabelText("电")]
        Electric,
        [LabelText("毒")]
        Poison,
    }

}