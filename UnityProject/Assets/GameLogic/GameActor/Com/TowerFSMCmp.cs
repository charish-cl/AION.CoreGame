using System.Collections.Generic;
using AION.CoreFramework;
using Cysharp.Threading.Tasks;
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
            
            // 从 TowerComponent 读取攻击范围配置
            var towerComponent = Actor.GetComponent<TowerComponent>();
            if (towerComponent != null && towerComponent.IsConfigValid)
            {
                float configAttackRange = towerComponent.AttackRange;
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
       
            // 塔攻击敌人（ENEMY）
            if (!ActorMgr.Instance.TryGetEnemy(Actor.Position, m_attackRange, out var enemy))
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
            
            if (hasRotateTarget )
            {
                //TODO: 发射子弹
                hasShoot = true;
                ActorMgr.Instance.SpawnBullet(Actor.Position, enemy.Position);
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
            
            // 尝试从 TowerComponent 读取攻击间隔
            var towerComponent = Actor.GetComponent<TowerComponent>();
            if (towerComponent != null && towerComponent.IsConfigValid && 
                towerComponent.AttackIntervals != null && towerComponent.AttackIntervals.Count > 0)
            {
                // 使用第一个攻击间隔（可以根据等级选择）
                coolingTime = towerComponent.AttackIntervals[0];
            }
            else
            {
                // 从数值组件获取
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