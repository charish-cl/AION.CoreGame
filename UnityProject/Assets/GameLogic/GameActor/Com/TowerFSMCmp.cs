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
        public float AttackRange { get; set; } = 0;
        
        public override void OnInit()
        {
            base.OnInit();
            
            // 从 TowerConfig 读取攻击范围配置
            var towerConfig = Actor.GetConfig<TowerConfig>();
            if (towerConfig != null)
            {
                float configAttackRange = towerConfig.AttackRange;
                AttackRange = configAttackRange;
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
            m_attackRange = towerFSM.AttackRange;
            DisableComponent<OrientationViewCmp>();
        }
        public override TowerState CheckConditions()
        {
          
            return hasTarget? TowerState.Attack : TowerState.Idle;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (m_attackRange <= 0f)
            {
                hasTarget = true;
                return;
            }
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
            
            // 获取子弹配置，判断是否是生成单位类型且攻击范围为0
            int bulletId = GetBulletId();
            // 如果攻击范围为0，不需要找目标，因为这不是一个真正的攻击塔，直接生成子弹
            if (m_attackRange <= 0f)
            {
                if (!hasShoot)
                {
                    hasShoot = true;
                    SpawnBulletDirectly(bulletId);
                }
                return;
            }
            
            // 其他情况需要找目标
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
        
        /// <summary>
        /// 直接生成子弹（不需要目标，用于生成单位类型且攻击范围为0的情况）
        /// </summary>
        private void SpawnBulletDirectly(int bulletId)
        {
            var attackerNumeric = Actor.GetComponent<NumericComponent>();
            if (attackerNumeric == null)
            {
                Log.Warning($"TowerAttackState: 塔没有NumericComponent");
                return;
            }
            
            // 生成子弹，目标位置使用攻击者位置（生成单位会在攻击者旁边生成）
            ActorMgr.Instance.SpawnBullet(Actor.Position, Actor.Position, bulletId, attackerNumeric, Actor, null);
        }
        
        /// <summary>
        /// 获取子弹ID
        /// </summary>
        private int GetBulletId()
        {
            var towerConfig = Actor.GetConfig<TowerConfig>();
            return towerConfig?.BulletId ?? 0;
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