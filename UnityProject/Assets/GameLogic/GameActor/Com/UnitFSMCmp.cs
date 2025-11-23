using System.Collections.Generic;
using AION.CoreFramework;
using GameConfig;
using GameConfig.battle;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameLogic
{
    public enum HeroState
    {
        Move,      // 移动状态：往上走寻找敌人
        Attack,    // 攻击状态：攻击敌人
        Cooling,   // 冷却状态：攻击间隔
    }
    
    public class UnitFSMCmp : FSMComponent<HeroState>
    {
        public override Dictionary<HeroState, BaseState<HeroState>> States { get; set; } = new()
        {
            {HeroState.Move, new HeroMoveState()},
            {HeroState.Attack, new HeroAttackState()},
            {HeroState.Cooling, new HeroCoolingState()},
        };

        public override HeroState CurrentState { get; set; } = HeroState.Move;
        
        /// <summary>
        /// 攻击范围（从配置获取）
        /// </summary>
        public float AttackRange { get; set; } = 3f;
        
        public override void OnInit()
        {
            base.OnInit();
            
            // 从 UnitComponent 读取配置
            var unitComponent = Actor.GetComponent<UnitComponent>();
            if (unitComponent != null && unitComponent.IsConfigValid && unitComponent.Config != null)
            {
                AttackRange = unitComponent.Config.AttackRange;
            }
        }
    }

    public class HeroMoveState : BaseState<HeroState>
    {
        private MoveLogicCmp m_moveLogic;
        private float m_attackRange;
        private Vector2 m_moveDirection = Vector2.up; // 往上走
        
        public override void OnEnter()
        {
            base.OnEnter();
            m_moveLogic = Actor.GetComponent<MoveLogicCmp>();
            m_attackRange = Actor.GetComponent<UnitFSMCmp>()?.AttackRange ?? 3f;
            
            // 启用移动组件
            if (m_moveLogic != null)
            {
                EnableComponent<MoveLogicCmp>();
            }
        }
        
        public override HeroState CheckConditions()
        {
            // 根据 UnitType 查找目标
            var unitComponent = Actor.GetComponent<UnitComponent>();
            // 使用 CombatHelper 查找目标
            var target = CombatHelper.FindAttackTarget(Actor, m_attackRange);
            if (target != null)
            {
                return HeroState.Attack;
            }
            
            return HeroState.Move;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            // 往上移动：通过InputLogicCmp设置输入方向
            if (m_moveLogic != null)
            {
                var inputCmp = Actor.GetComponent<InputLogicCmp>();
                if (inputCmp != null)
                {
                    inputCmp.SetInput(m_moveDirection);
                }
            }
        }
        
        private void EnableComponent<T>() where T : GameActorCmp, new()
        {
            if (Actor.TryGetComponent<T>(out var cmp))
            {
                cmp.Enable = true;
            }
        }
    }

    public class HeroAttackState : BaseState<HeroState>
    {
        private OrientationViewCmp m_orientation;
        private GameActor m_target;
        private bool m_hasAttacked;
        private float m_attackRange;
        
        public override void OnEnter()
        {
            base.OnEnter();
            m_hasAttacked = false;
            m_orientation = Actor.GetComponent<OrientationViewCmp>();
            if (m_orientation == null)
            {
                m_orientation = Actor.AddComponent<OrientationViewCmp>();
            }
            m_attackRange = Actor.GetComponent<UnitFSMCmp>()?.AttackRange ?? 3f;
            
            // 禁用移动组件
            DisableComponent<MoveLogicCmp>();
            
            // 使用 CombatHelper 查找目标
            m_target = CombatHelper.FindAttackTarget(Actor, m_attackRange);
        }
        
        public override HeroState CheckConditions()
        {
            if (m_hasAttacked)
            {
                return HeroState.Cooling;
            }
            
            // 检查目标是否还在攻击范围内
            if (m_target != null && !m_target.IsDestroyed)
            {
                float distance = Vector2.Distance(Actor.Position, m_target.Position);
                if (distance > m_attackRange)
                {
                    return HeroState.Move;
                }
            }
            else
            {
                // 目标已消失，返回移动状态
                return HeroState.Move;
            }
            
            return HeroState.Attack;
        }
        
        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (m_target == null || m_target.IsDestroyed)
            {
                return;
            }
            
            // 如果正在攻击（动画未播完），不能再次攻击
            if (CombatHelper.IsAttacking(Actor))
            {
                return;
            }
            
            // 朝向目标
            if (m_orientation != null)
            {
                m_orientation.SetTarget(m_target.Position);
                
                // 检查是否已经转向目标
                if (m_orientation.CheckHasRotatedToTarget(m_target.Position))
                {
                    // 执行攻击（带前摇后摇）
                    if (!m_hasAttacked)
                    {
                        m_hasAttacked = true;
                        // 使用异步方法执行攻击，不等待完成（让状态机继续运行）
                        CombatHelper.PerformAttackWithCastTime(Actor, m_target).Forget();
                    }
                }
            }
        }
    }

    public class HeroCoolingState : BaseState<HeroState>
    {
        private float m_stateTime;
        private float m_coolingTime;
        
        public override void OnEnter()
        {
            base.OnEnter();
            m_stateTime = 0f;
            
            // 计算冷却时间（攻击间隔）
            m_coolingTime = 1f / Actor.GetProperty(NumericType.AttackSpeed);
            
            DisableComponent<OrientationViewCmp>();
        }
        
        public override HeroState CheckConditions()
        {
            if (m_stateTime >= m_coolingTime)
            {
                return HeroState.Attack;
            }
            return HeroState.Cooling;
        }
        
        public override void OnUpdate()
        {
            base.OnUpdate();
            m_stateTime += Time.deltaTime;
        }
    }
}

