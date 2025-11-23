using System.Collections.Generic;
using GameConfig;
using GameLogic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 友方目标选择器参数
    /// </summary>
    public class FriendlyTargetSelectorParams
    {
        public int TargetCount = 1;      // 目标数量
        public float SearchRadius = 10f; // 搜索半径
    }

    /// <summary>
    /// 友方目标选择器
    /// </summary>
    [BuffTargetSelector(ETargetType.Friendly)]
    public class FriendlyTargetSelector : BaseTargetSelector
    {
        public FriendlyTargetSelector(
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

            var param = GetParam<FriendlyTargetSelectorParams>();
            
            Vector2 searchCenter = SourceActor.Position;
            if (AttackerActor != null)
            {
                searchCenter = AttackerActor.Position;
            }

            return BuffHelper.FindTargets(searchCenter, param.SearchRadius, ETargetType.Friendly, param.TargetCount);
        }
    }
}

