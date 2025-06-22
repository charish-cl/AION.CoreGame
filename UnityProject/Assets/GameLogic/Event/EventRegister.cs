using System;
using AION.CoreFramework;

namespace GameLogic
{
    public class EventRegister
    {
        
        public EventRegister()
        {
            var eventDispatcher = GameEvent.EventMgr.Dispatcher;
            
            Register<ICommonUI_Gen>(eventDispatcher);
            Register<IBattleEvent_Gen>(eventDispatcher);
        }
        
        void Register<T>(EventDispatcher eventDispatcher)
        {
            Activator.CreateInstance(typeof(T), eventDispatcher);
        }

        public static void Init()
        {
            new EventRegister();
        }
    }
}