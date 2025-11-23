using System.Collections.Generic;
using AION.CoreFramework;
using GameConfig;
using GameConfig.battle;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameLogic
{
    public enum MonsterState
    {
        Move,      // 移动状态：往下走攻击基地
        Attack,    // 攻击状态：攻击基地或敌人
        Cooling,   // 冷却状态：攻击间隔
    }
    
    public class MonsterFSMCmp : FSMComponent<MonsterState>
    {
        public override Dictionary<MonsterState, BaseState<MonsterState>> States { get; set; } = new()
        {
            {MonsterState.Move, new MonsterMoveState()},
            {MonsterState.Attack, new MonsterAttackState()},
            {MonsterState.Cooling, new MonsterCoolingState()},
        };

        public override MonsterState CurrentState { get; set; } = MonsterState.Move;
        
        /// <summary>
        /// 攻击范围（从配置获取）
        /// </summary>
        public float AttackRange { get; set; } = 2f;
        
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

    public class MonsterMoveState : BaseState<MonsterState>
    {
        private SimplePathFindingLogicCmp m_pathFinding;
        private GameActor m_targetBase;
        private float m_attackRange;
        
        public override void OnEnter()
        {
            base.OnEnter();
            m_pathFinding = Actor.GetComponent<SimplePathFindingLogicCmp>();
            m_attackRange = Actor.GetComponent<MonsterFSMCmp>()?.AttackRange ?? 2f;
            
            // 启用寻路组件
            if (m_pathFinding != null)
            {
                EnableComponent<SimplePathFindingLogicCmp>();
            }
        }
        
        public override MonsterState CheckConditions()
        {
            // 根据 UnitType 查找目标
            var unitComponent = Actor.GetComponent<UnitComponent>();
            // 使用 CombatHelper 查找目标
            var target = CombatHelper.FindAttackTarget(Actor, m_attackRange);
            if (target != null)
            {
                return MonsterState.Attack;
            }
            
            return MonsterState.Move;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            // 移动逻辑由SimplePathFindingLogicCmp处理
        }
        
        private void EnableComponent<T>() where T : GameActorCmp, new()
        {
            if (Actor.TryGetComponent<T>(out var cmp))
            {
                cmp.Enable = true;
            }
        }
    }

    public class MonsterAttackState : BaseState<MonsterState>
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
            m_attackRange = Actor.GetComponent<MonsterFSMCmp>()?.AttackRange ?? 2f;
            
            // 禁用移动组件
            DisableComponent<SimplePathFindingLogicCmp>();
            
            // 使用 CombatHelper 查找目标
            m_target = CombatHelper.FindAttackTarget(Actor, m_attackRange);
        }
        
        public override MonsterState CheckConditions()
        {
            if (m_hasAttacked)
            {
                return MonsterState.Cooling;
            }
            
            // 检查目标是否还在攻击范围内
            if (m_target != null && !m_target.IsDestroyed)
            {
                float distance = Vector2.Distance(Actor.Position, m_target.Position);
                if (distance > m_attackRange)
                {
                    return MonsterState.Move;
                }
            }
            else
            {
                // 目标已消失，返回移动状态
                return MonsterState.Move;
            }
            
            return MonsterState.Attack;
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

    public class MonsterCoolingState : BaseState<MonsterState>
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
        
        public override MonsterState CheckConditions()
        {
            if (m_stateTime >= m_coolingTime)
            {
                return MonsterState.Attack;
            }
            return MonsterState.Cooling;
        }
        
        public override void OnUpdate()
        {
            base.OnUpdate();
            m_stateTime += Time.deltaTime;
        }
    }
}

