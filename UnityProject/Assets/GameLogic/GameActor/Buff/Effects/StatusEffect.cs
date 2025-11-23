using System.Collections.Generic;
using GameConfig;
using GameConfig.battle;
using GameLogic;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 状态效果（眩晕、击退等）
    /// 根据StatuConfig配置来禁用/启用相应的组件
    /// </summary>
    [BuffEffect(EBuffType.Status)]
    public class StatusEffect : BaseEffect
    {
        // 记录被禁用的组件，用于恢复
        private List<GameActorCmp> m_disabledComponents = new List<GameActorCmp>();
        
        public StatusEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            int statusId = 0,
            EDamageType damageType = EDamageType.Physical
        ) : base(targetActor, valueParams, attackerNumeric, null, statusId, damageType)
        {
        }
        
        public StatusEffect(
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

            // 从配置中读取状态信息
            if (StatusId <= 0)
            {
                Log.Warning("StatusEffect: StatusId无效");
                return;
            }

            var statusConfig = ConfigSystem.Instance.Tables.TbStatus.GetOrDefault(StatusId);
            if (statusConfig == null)
            {
                Log.Warning($"StatusEffect: 未找到StatusId {StatusId} 对应的配置");
                return;
            }

            // 根据配置禁用/启用组件
            ApplyStatusConfig(statusConfig);
        }
        
        /// <summary>
        /// 应用状态配置
        /// </summary>
        private void ApplyStatusConfig(StatuConfig statusConfig)
        {
            // 禁用移动相关组件
            if (!statusConfig.CanMove)
            {
                DisableComponent<MoveLogicCmp>();
                DisableComponent<SimplePathFindingLogicCmp>();
                DisableComponent<InputLogicCmp>();
                DisableComponent<MoveViewCmp>();
                DisableComponent<DirectionViewCmp>();
                DisableComponent<OrientationViewCmp>();
            }

            // 禁用攻击相关组件（通过禁用状态机来禁用攻击）
            if (!statusConfig.CanAttack)
            {
                DisableComponent<UnitFSMCmp>();
                DisableComponent<MonsterFSMCmp>();
                DisableComponent<TowerFSMCmp>();
                DisableComponent<OrientationViewCmp>();
            }
        }
        
        /// <summary>
        /// 禁用组件
        /// </summary>
        private void DisableComponent<T>() where T : GameActorCmp, new()
        {
            if (TargetActor.TryGetComponent<T>(out var cmp))
            {
                if (cmp.Enable)
                {
                    cmp.Enable = false;
                    m_disabledComponents.Add(cmp);
                    Log.Info($"StatusEffect: 禁用组件 {typeof(T).Name}");
                }
            }
        }
        
        /// <summary>
        /// 恢复所有被禁用的组件（在Buff结束时调用）
        /// </summary>
        public void RestoreComponents()
        {
            foreach (var cmp in m_disabledComponents)
            {
                if (cmp != null)
                {
                    cmp.Enable = true;
                    Log.Info($"StatusEffect: 恢复组件 {cmp.GetType().Name}");
                }
            }
            m_disabledComponents.Clear();
        }
    }
}

