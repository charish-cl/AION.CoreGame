using System;
using System.Collections.Generic;
using AION.CoreFramework;

namespace GameLogic
{
    public enum ActorActionState
    {
        NoTransition,
        Idle,
        Move,
        Attack,
        Dead,
    }

    public abstract class FSMComponent <T> : GameActorCmp  where T : Enum
    {
      
        
        public virtual T CurrentState { get; set; }
        
        T PreviousState { get; set; } 
        public abstract Dictionary<T, BaseState <T>> States { get; set; }


        public override void OnInit()
        {
            base.OnInit();
            foreach (var keyValuePair in States)
            {
                keyValuePair.Value.SetActor(this.Actor);
            }
            ChangeState(CurrentState);
        }
        
        public void ChangeState(T newState)
        {
            CurrentState = newState;
            
            if (States.ContainsKey(PreviousState))
            {
                States[PreviousState].OnExit();
            }
            
            if (States.ContainsKey(CurrentState))
            {
                States[CurrentState].OnEnter();
            }
            if (Actor.Transform != null)
            {
                Log.Info($"{Actor.Transform.name} FSMComponent: {PreviousState} changed state to {CurrentState}");
            }
            else
            {
                Log.Info($"FSMComponent: {PreviousState} changed state to {CurrentState}");
            }
            
            PreviousState = CurrentState;
        }        
        
        public override void OnUpdate()
        {
            if (States.ContainsKey(CurrentState))
            {
                var nextState = States[CurrentState].CheckConditions();
                if (!nextState.Equals(CurrentState))
                {
                    ChangeState(nextState);
                    return;
                }
                States[CurrentState].OnUpdate();
            }
        }
    }

    public abstract class BaseState<T>
    {
        List<GameActorCmp> DisableComponents { get; set; } = new List<GameActorCmp>();
        
        protected GameActor Actor { get; set; }
        
        public void SetActor(GameActor actor)
        {
            Actor = actor;
        }
        protected void DisableComponent<T>() where T : GameActorCmp, new()
        {
            if (Actor.TryGetComponent<T>(out var cmp))
            {
                DisableComponents.Add(cmp);
                cmp.Enable = false;
            }
         
        }
   
        public abstract T CheckConditions();
        
        public virtual void OnEnter()
        {
            
        }

        public virtual void OnUpdate()
        {
            
        }
        
        public virtual void OnExit()
        {
            foreach (var component in DisableComponents)
            {
                component.Enable = true;
            }
        }
        
    }
}