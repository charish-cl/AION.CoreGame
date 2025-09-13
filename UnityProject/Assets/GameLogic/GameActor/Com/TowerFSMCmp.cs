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
    }

    public class TowerIdleState : BaseState<TowerState>
    {
        bool hasTarget;
        public override void OnEnter()
        {
            base.OnEnter();
            hasTarget = false;
            DisableComponent<OrientationViewCmp>();
        }
        public override TowerState CheckConditions()
        {
          
            return hasTarget? TowerState.Attack : TowerState.Idle;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (SceneMgr.Instance.TryGetMonster(Actor.Position, 15,out var monster))
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
        public override void OnEnter()
        {
            base.OnEnter();
            hasShoot = false;
            enemyHasExit = false;
            orientation = Actor.GetComponent<OrientationViewCmp>();
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
       
            if (!SceneMgr.Instance.TryGetMonster(Actor.Position, 15,out var monster))
            {
                enemyHasExit = true;
                return;
            }
            if (orientation == null)
            {
                return;
            }
            orientation.SetTarget(monster.Position);
            
            bool hasRotateTarget = orientation.CheckHasRotatedToTarget(monster.Position);
            
            if (hasRotateTarget )
            {
                //TODO: 发射子弹
                hasShoot = true;
                SceneMgr.Instance.SpawnBullet(Actor.Position, monster.Position);
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
        float coolingTime = 2;
        public override async void OnEnter()
        {
            base.OnEnter();
            stateTime = 0;
            DisableComponent<OrientationViewCmp>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            stateTime += Time.deltaTime;
            
        }
    }

}