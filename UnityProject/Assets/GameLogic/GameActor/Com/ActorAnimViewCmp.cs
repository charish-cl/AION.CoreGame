using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    public class ActorAnimViewCmp : GameActorCmp
    {
        AnimatorComponent animator;
        
        MoveLogicCmp moveLogic ;
    
        const string WalkAnimName = "Walking";
        const string IdleAnimName = "Idle";
        const string DamageAnimName = "Damage";
        const string DeadAnimName = "Dead";
        
        
        
        public override void OnInit()
        {
            base.OnInit();
            moveLogic = GetComponent<MoveLogicCmp>();
            animator = new AnimatorComponent(Actor.m_transform.GetComponentInChildren<Animator>());
            
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (CheckIsEnable(moveLogic))
            {
                if (moveLogic.IsMoving)
                {
                    animator.PlayAnimation(WalkAnimName);
                }
                else
                {
                    animator.PlayAnimation(IdleAnimName);
                }
            }
        }
    }
}