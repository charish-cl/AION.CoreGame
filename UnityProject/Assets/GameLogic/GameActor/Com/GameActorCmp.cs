using System;
using AION.CoreFramework;

namespace GameLogic
{
    //组件
    public class GameActorCmp
    {
        public GameActor Actor;

        public bool Enable = true;
        
        public T GetComponent<T>() where T : GameActorCmp, new()
        {
            return Actor.GetComponent<T>(); 
        }
        public bool CheckIsEnable(GameActorCmp cmp)
        {
            return cmp!= null && cmp.Enable;
        }
        public void AddEvent<T>(int eventId, Action<T> eventCallback)
        {
            if (Actor.EventDispatcher == null)
            {
                Log.Error("组件没有事件注册者！");
                return;
            }
            Actor.EventDispatcher.AddEventListener<T>(eventId, eventCallback,Actor);
        }
        public void AddEvent(int eventId, Action eventCallback)
        {
            if (Actor.EventDispatcher == null)
            {
                Log.Error("组件没有事件注册者！");
                return;
            }
            Actor.EventDispatcher.AddEventListener(eventId, eventCallback,Actor);
        }
        public void SendEvent<T>(int eventId)
        {
            Actor.EventDispatcher.SendEvent(eventId);
                
        }
        public void SendEvent<T>(int eventId, Action<T> eventCallback)
        {
            Actor.EventDispatcher.SendEvent(eventId, eventCallback);
        }
        
        public virtual void OnInit()
        {
            
        }

        public virtual void OnUpdate()
        {
            
        }
        

        public virtual void OnDestroy()
        {
            
        }
      
        
    }
}