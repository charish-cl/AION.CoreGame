using System.Collections.Generic;
using GameConfig;
using GameLogic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 治疗效果参数类
    /// </summary>
    public class HealEffectParams
    {
        public float HealAmount = 0f; // 治疗量
    }

    /// <summary>
    /// 治疗效果
    /// </summary>
    [BuffEffect(EBuffType.Heal)]
    public class HealEffect : BaseEffect
    {
        public HealEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            int statusId = 0,
            EDamageType damageType = EDamageType.Physical
        ) : base(targetActor, valueParams, attackerNumeric, null, statusId, damageType)
        {
        }
        
        public HealEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0,
            EDamageType damageType = EDamageType.Physical
        ) : base(targetActor, valueParams, attackerNumeric, attackerActor, statusId, damageType)
        {
        }

        public override void Apply()
        {
            if (TargetActor == null)
                return;

            var healthCmp = TargetActor.GetComponent<HealthCmp>();
            if (healthCmp == null)
                return;

            // 使用GetParam自动填充参数
            var param = GetParam<HealEffectParams>();
            
            if (param.HealAmount > 0)
            {
                int currentHp = healthCmp.HP;
                int maxHp = TargetActor.NumericComponent.GetAsInt(NumericType.MaxHp);
                healthCmp.HP = Mathf.Min(currentHp + (int)param.HealAmount, maxHp);
            }
        }
    }
}

