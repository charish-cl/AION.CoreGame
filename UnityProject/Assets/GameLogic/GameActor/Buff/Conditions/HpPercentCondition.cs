using System.Collections.Generic;
using GameConfig;
using GameLogic;

namespace GameLogic
{
    /// <summary>
    /// 生命值百分比条件参数类
    /// </summary>
    public class HpPercentConditionParams
    {
        public float MinHpPercent = 0f;  // 最小生命值百分比
        public float MaxHpPercent = 1f;  // 最大生命值百分比
        public bool UseCurrentHp = true; // 是否使用当前生命值（true）或最大生命值（false）
    }

    /// <summary>
    /// 生命值百分比条件
    /// </summary>
    [BuffCondition(ETriggerType.Probability)]
    public class HpPercentCondition : BaseCondition
    {
        public HpPercentCondition(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0
        ) : base(targetActor, valueParams, attackerNumeric, attackerActor, statusId)
        {
        }

        public override bool Check()
        {
            if (TargetActor == null)
                return false;

            var healthCmp = TargetActor.GetComponent<HealthCmp>();
            if (healthCmp == null)
                return false;

            // 使用GetParam自动填充参数
            var param = GetParam<HpPercentConditionParams>();

            float currentHp = healthCmp.HP;
            float maxHp = healthCmp.MaxHP;
            
            if (maxHp <= 0)
                return false;

            float hpPercent = currentHp / maxHp;

            // 检查生命值百分比是否在指定范围内
            return hpPercent >= param.MinHpPercent && hpPercent <= param.MaxHpPercent;
        }
    }
}
