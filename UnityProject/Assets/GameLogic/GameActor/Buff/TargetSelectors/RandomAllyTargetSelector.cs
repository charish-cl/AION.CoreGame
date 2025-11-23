using System.Collections.Generic;
using System.Linq;
using GameConfig;
using GameLogic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 随机友方目标选择器参数
    /// </summary>
    public class RandomAllyTargetSelectorParams
    {
        public int TargetCount = 1;      // 目标数量
        public float SearchRadius = 10f; // 搜索半径
    }

    /// <summary>
    /// 随机友方目标选择器
    /// </summary>
    [BuffTargetSelector(ETargetType.RandomAlly)]
    public class RandomAllyTargetSelector : BaseTargetSelector
    {
        public RandomAllyTargetSelector(
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

            var param = GetParam<RandomAllyTargetSelectorParams>();
            
            Vector2 searchCenter = SourceActor.Position;
            if (AttackerActor != null)
            {
                searchCenter = AttackerActor.Position;
            }

            // 获取所有符合条件的友方
            var candidates = ActorMgr.Instance.Actors
                .Where(actor => 
                    actor.Tag == UnitTag.Player && 
                    !actor.IsDestroyed &&
                    (param.SearchRadius <= 0 || Vector2.Distance(searchCenter, actor.Position) <= param.SearchRadius))
                .ToList();

            // 随机选择
            var selected = candidates.OrderBy(x => Random.Range(0f, 1f)).Take(param.TargetCount).ToList();
            
            return selected;
        }
    }
}

