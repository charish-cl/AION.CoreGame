using System;
using System.Collections.Generic;
using AION.CoreFramework;

namespace GameLogic.GamePlay
{
    
    // 进入该状态时会执行一次的逻辑
    // 处于该状态时会不断执行的逻辑
    // 退出该状态（转移到其它状态）时会执行一次的逻辑
    public interface IFSMState
    {
        public virtual void OnEnter()
        {

        }
        public virtual void OnUpdate()
        {       
        }
        public virtual void OnExit()
        {

        }
    }
//如何少引入一些条件变量？ 如何少引入一些不必要的变量？，如何传递参数？
    public class StateMachine<T> where T : IFSMState
    {
        public Dictionary<string, T> states = new Dictionary<string, T>();
        public T CurrentState;

        public void ChangeState(string stateName)
        {
            if (states.ContainsKey(stateName))
            {
                CurrentState.OnExit();
                CurrentState = states[stateName];
                CurrentState.OnEnter();
            }
            else
            {
                throw new GameFrameworkException($"{stateName} State not found");
            }
        }

        public void OnUpdate(float elapseSeconds)
        {
            CurrentState.OnUpdate();
        }
        
        public void AddState(string stateName, T state)
        {
            if (states.ContainsKey(stateName))
            {
                throw new GameFrameworkException($"{stateName} State already exists");
            }
            states.Add(stateName, state);
        }
    }
}