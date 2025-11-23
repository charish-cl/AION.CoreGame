using System.Collections.Generic;
using AION.CoreFramework;
using GameConfig;
using GameLogic;

namespace GameLogic
{
    /// <summary>
    /// 特定单位目标选择器参数
    /// </summary>
    public class SpecificTargetSelectorParams
    {
        public int UnitId = 0; // 特定单位的ID（从配置中获取）
    }

    /// <summary>
    /// 特定单位目标选择器
    /// </summary>
    [BuffTargetSelector(ETargetType.specific)]
    public class SpecificTargetSelector : BaseTargetSelector
    {
        public SpecificTargetSelector(
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

            var param = GetParam<SpecificTargetSelectorParams>();
            
            if (param.UnitId <= 0)
            {
                Log.Warning("SpecificTargetSelector: UnitId无效");
                return new List<GameActor>();
            }

            // 查找具有指定UnitId的Actor
            List<GameActor> targets = new List<GameActor>();
            
            foreach (var actor in ActorMgr.Instance.Actors)
            {
                if (actor.IsDestroyed)
                    continue;

                var unitComponent = actor.GetComponent<UnitComponent>();
                if (unitComponent != null && unitComponent.IsConfigValid && unitComponent.Config != null)
                {
                    if (unitComponent.Config.Id == param.UnitId)
                    {
                        targets.Add(actor);
                    }
                }
            }
            
            return targets;
        }
    }
}

