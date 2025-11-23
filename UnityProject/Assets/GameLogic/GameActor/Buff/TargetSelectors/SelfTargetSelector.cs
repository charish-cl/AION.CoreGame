using System.Collections.Generic;
using GameConfig;
using GameLogic;

namespace GameLogic
{
    /// <summary>
    /// 自己目标选择器
    /// </summary>
    [BuffTargetSelector(ETargetType.Self)]
    public class SelfTargetSelector : BaseTargetSelector
    {
        public SelfTargetSelector(
            GameActor sourceActor,
            List<float> targetParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0
        ) : base(sourceActor, targetParams, attackerNumeric, attackerActor, statusId)
        {
        }

        public override List<GameActor> SelectTargets()
        {
            if (SourceActor == null)
                return new List<GameActor>();

            return new List<GameActor> { SourceActor };
        }
    }
}

