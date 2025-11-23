using System.Collections.Generic;
using GameConfig;
using GameLogic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 敌人目标选择器参数
    /// </summary>
    public class EnemyTargetSelectorParams
    {
        public int TargetCount = 1;      // 目标数量
        public float SearchRadius = 10f; // 搜索半径
    }

    /// <summary>
    /// 敌人目标选择器
    /// </summary>
    [BuffTargetSelector(ETargetType.Enemy)]
    public class EnemyTargetSelector : BaseTargetSelector
    {
        public EnemyTargetSelector(
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

            var param = GetParam<EnemyTargetSelectorParams>();
            
            Vector2 searchCenter = SourceActor.Position;
            if (AttackerActor != null)
            {
                searchCenter = AttackerActor.Position;
            }

            return BuffHelper.FindTargets(searchCenter, param.SearchRadius, ETargetType.Enemy, param.TargetCount);
        }
    }
}

