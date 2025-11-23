using System.Collections.Generic;
using GameConfig;
using GameLogic;

namespace AION.Config.Buff
{
    /// <summary>
    /// 属性修改效果
    /// 注意：属性修改已经在AttributeModifier中处理，这个Effect主要用于占位
    /// 如果将来需要在触发时执行额外逻辑，可以在这里实现
    /// </summary>
    [BuffEffect(EBuffType.PropertyMod)]
    public class PropertyModEffect : BaseEffect
    {
        public PropertyModEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            int statusId = 0,
            GameConfig.EDamageType damageType = GameConfig.EDamageType.Physical
        ) : base(targetActor, valueParams, attackerNumeric, null, statusId, damageType)
        {
        }
        
        public PropertyModEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0,
            GameConfig.EDamageType damageType = GameConfig.EDamageType.Physical
        ) : base(targetActor, valueParams, attackerNumeric, attackerActor, statusId, damageType)
        {
        }

        public override void Apply()
        {
            // 属性修改已经在Modifier中处理，这里不需要额外操作
            // 如果将来需要在触发时执行额外逻辑（如播放特效、音效等），可以在这里实现
        }
    }
}

