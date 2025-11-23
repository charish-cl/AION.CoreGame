using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    public class ActorAnimViewCmp : GameActorCmp
    {
        WrapAnimator _wrapAnimator;
        
        MoveLogicCmp moveLogic ;
    
        const string WalkAnimName = "Walking";
        const string IdleAnimName = "Idle";
        const string DamageAnimName = "Damage";
        const string DeadAnimName = "Dead";
        
        
        
        public override void OnInit()
        {
            base.OnInit();
            moveLogic = GetComponent<MoveLogicCmp>();
            if (Actor.Transform != null)
            {
                _wrapAnimator = new WrapAnimator(Actor.Transform.GetComponentInChildren<Animator>());
            }
            
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (CheckIsEnable(moveLogic))
            {
                if (_wrapAnimator == null)
                {
                    return;
                }
                if (moveLogic.IsMoving)
                {
                    _wrapAnimator.PlayAnimation(WalkAnimName);
                }
                else
                {
                    _wrapAnimator.PlayAnimation(IdleAnimName);
                }
            }
        }
    }
}