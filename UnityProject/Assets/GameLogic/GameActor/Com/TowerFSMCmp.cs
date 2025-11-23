using System.Collections.Generic;
using AION.CoreFramework;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameConfig.battle;
using UnityEngine;

namespace GameLogic
{
    public enum TowerState
    {
        Idle,
        //攻击
        Attack,
        //冷却
        Cooling,
    } 
    public class TowerFSMCmp : FSMComponent<TowerState>
    {
        public override Dictionary<TowerState, BaseState<TowerState>> States { get; set; } = new()
        {
            {TowerState.Idle, new TowerIdleState()},
            {TowerState.Attack, new TowerAttackState()},
            {TowerState.Cooling, new TowerCoolingState()},
        };

        public override TowerState CurrentState { get; set; } = TowerState.Idle;
        
        /// <summary>
        /// 攻击范围（从配置获取，如果没有配置则使用默认值）
        /// </summary>
        public float AttackRange { get; set; } = 15f;
        
        public override void OnInit()
        {
            base.OnInit();
            
            // 从 TowerConfig 读取攻击范围配置
            var towerConfig = Actor.GetConfig<TowerConfig>();
            if (towerConfig != null)
            {
                float configAttackRange = towerConfig.AttackRange;
                if (configAttackRange > 0f)
                {
                    AttackRange = configAttackRange;
                }
            }
        }
    }

    public class TowerIdleState : BaseState<TowerState>
    {
        bool hasTarget;
        private float m_attackRange;
        
        public override void OnEnter()
        {
            base.OnEnter();
            hasTarget = false;
            var towerFSM = Actor.GetComponent<TowerFSMCmp>();
            m_attackRange = towerFSM?.AttackRange ?? 15f;
            DisableComponent<OrientationViewCmp>();
        }
        public override TowerState CheckConditions()
        {
          
            return hasTarget? TowerState.Attack : TowerState.Idle;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            // 塔攻击敌人（ENEMY）
            if (ActorMgr.Instance.TryGetEnemy(Actor.Position, m_attackRange, out var enemy))
            {
                hasTarget = true;
                return;
            }
        }
    }

   
    public class TowerAttackState : BaseState<TowerState>
    {
        OrientationViewCmp orientation;
        bool hasShoot;
        bool enemyHasExit;
        private float m_attackRange;
        
        public override void OnEnter()
        {
            base.OnEnter();
            hasShoot = false;
            enemyHasExit = false;
            orientation = Actor.GetComponent<OrientationViewCmp>();
            var towerFSM = Actor.GetComponent<TowerFSMCmp>();
            m_attackRange = towerFSM?.AttackRange ?? 15f;
        }
        
        public override TowerState CheckConditions()
        {
            if (hasShoot)
            {
                return TowerState.Cooling;
            }
            if (enemyHasExit)
            {
                return TowerState.Idle;
            }
            return TowerState.Attack;
        }
        
        public override void OnUpdate()
        {
            // 如果正在攻击（动画未播完），不能再次攻击
            if (CombatHelper.IsAttacking(Actor))
            {
                return;
            }
            
            // 查找攻击目标
            var enemy = CombatHelper.FindAttackTarget(Actor, m_attackRange);
            if (enemy == null)
            {
                enemyHasExit = true;
                return;
            }
            
            if (orientation == null)
            {
                return;
            }
            orientation.SetTarget(enemy.Position);
            
            bool hasRotateTarget = orientation.CheckHasRotatedToTarget(enemy.Position);
            
            if (hasRotateTarget)
            {
                // 执行攻击（带前摇后摇）
                if (!hasShoot)
                {
                    hasShoot = true;
                    // 使用异步方法执行攻击，不等待完成（让状态机继续运行）
                    CombatHelper.PerformAttackWithCastTime(Actor, enemy).Forget();
                }
            }
        }
    }

    public class TowerCoolingState : BaseState<TowerState>
    {
        public override TowerState CheckConditions()
        {
            if (stateTime >= coolingTime)
            {
                return TowerState.Idle;
            }
            return TowerState.Cooling;
        }

        float stateTime;
        float coolingTime;
        
        public override async void OnEnter()
        {
            base.OnEnter();
            stateTime = 0;
            
            // 尝试从 TowerConfig 读取攻击间隔
            var towerConfig = Actor.GetConfig<TowerConfig>();
            if (towerConfig != null && 
                towerConfig.AttackInterals != null && towerConfig.AttackInterals.Count > 0)
            {
                coolingTime = towerConfig.AttackInterals[0];
            }
            else
            {
                coolingTime = 1f / Actor.GetProperty(NumericType.AttackSpeed);
            }
            
            DisableComponent<OrientationViewCmp>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            stateTime += Time.deltaTime;
        }
    }

}