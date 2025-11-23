using System.Collections.Generic;
using System.Linq;
using GameConfig;
using GameLogic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 所有目标选择器参数
    /// </summary>
    public class AllTargetSelectorParams
    {
        public float SearchRadius = 10f; // 搜索半径（0表示全图）
    }

    /// <summary>
    /// 所有目标选择器
    /// </summary>
    [BuffTargetSelector(ETargetType.All)]
    public class AllTargetSelector : BaseTargetSelector
    {
        public AllTargetSelector(
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

            var param = GetParam<AllTargetSelectorParams>();
            
            Vector2 searchCenter = SourceActor.Position;
            if (AttackerActor != null)
            {
                searchCenter = AttackerActor.Position;
            }

            List<GameActor> targets = new List<GameActor>();
            
            // 获取所有符合条件的Actor
            var candidates = ActorMgr.Instance.Actors
                .Where(actor => 
                    !actor.IsDestroyed &&
                    (param.SearchRadius <= 0 || Vector2.Distance(searchCenter, actor.Position) <= param.SearchRadius))
                .ToList();

            targets.AddRange(candidates);
            
            return targets;
        }
    }
}

