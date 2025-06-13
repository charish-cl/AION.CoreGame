using System;

namespace AION.CoreFramework
{
    //组件
    public class GameActorCmp
    {
        public GameActor Actor;


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
        public T GetService<T>() where T : class
        {
            return Actor.GetSevice<T>();
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