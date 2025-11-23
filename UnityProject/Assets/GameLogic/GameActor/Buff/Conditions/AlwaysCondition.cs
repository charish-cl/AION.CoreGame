using System.Collections.Generic;
using GameConfig;
using GameLogic;

namespace GameLogic
{
    /// <summary>
    /// 无条件条件参数类
    /// </summary>
    public class AlwaysConditionParams
    {
        // 无条件条件没有参数
    }

    /// <summary>
    /// 无条件条件（总是满足）
    /// </summary>
    [BuffCondition(ETriggerType.Immediate)]
    public class AlwaysCondition : BaseCondition
    {
        public AlwaysCondition(
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
            // 无条件总是满足
            return true;
        }
    }
}
