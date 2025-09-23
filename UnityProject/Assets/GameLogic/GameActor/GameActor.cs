using System.Collections.Generic;
using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
   
    public enum BuffState
    {
        None,
        
    }
    public enum UnitTag
    {
        Player,
        Enemy,
        Tower,
        Bullet,
    }
    public class GameActor
    {
        public LinkedList<GameActorCmp> cmps = new LinkedList<GameActorCmp>();

        public Transform m_transform;
        
        public GameObject m_Owner;
        //事件

        public UnitTag Tag { get; private set; }
        public Vector2 Position { get;private set; }

        public NumericComponent NumericComponent
        {
            get
            {
                return GetComponent<NumericComponent>();   
            }
        }

        public bool IsDestroyed { get;private set; }
        
        public void Destroy()
        {
            IsDestroyed = true;
        }
        public void SetTag(UnitTag tag)
        {
            Tag = tag;
        }
        public void SetPosition(Vector2 pos)
        {
            Position = pos;
        }
        #region 事件

        private ActorEventDispatcher m_EventDispatcher;

        public ActorEventDispatcher EventDispatcher
        {
            get
            {
                if (m_EventDispatcher == null)
                {
                    m_EventDispatcher = MemoryPool.Acquire<ActorEventDispatcher>();   
                }
                return m_EventDispatcher;
            }
        }

        #endregion

        #region 生命周期

        public void BindGo(GameObject owner)
        {
            m_Owner = owner;
            m_transform = owner.transform;
            
            SetPosition(m_transform.position);
        }
        protected virtual void BindCmp()
        {
        }
        public  void OnInit()
        {
            //这里添加组件
            BindCmp();
            
            
            foreach (var gameActorCmp in cmps)
            {
                gameActorCmp.OnInit();
            }
        }
        
        public void OnUpdate()
        {
            foreach (var gameActorCmp in cmps)
            {
                if (gameActorCmp.Enable)
                {
                    gameActorCmp.OnUpdate();
                }
            }
        }


        public void OnDestroy()
        {
            foreach (var gameActorCmp in cmps)
            {
                gameActorCmp.OnDestroy();
            }
            
            MemoryPool.Release(EventDispatcher);
            
            Object.Destroy(m_Owner);
        }


        #endregion
        
        public T AddComponent<T>() where T : GameActorCmp, new()
        {
            T newCmp = null;
            if (!HasComponent<T>())
            {
                newCmp = new T();
                newCmp.Actor = this;
                cmps.AddLast(newCmp);
            }
            return newCmp;
        }

        public T GetComponent<T>() where T : GameActorCmp, new()
        {
            T newCmp = null;

            foreach (var gameActorCmp in cmps)
            {
                if (gameActorCmp is T t)
                {
                    return gameActorCmp as T;
                }
            }

            return newCmp;
        }
        public bool TryGetComponent<T>(out T cmp) where T : GameActorCmp, new()
        {
            cmp = GetComponent<T>();
            return cmp!= null;
        }

        public bool RemoveComponent<T>() where T : GameActorCmp, new()
        {
            var gameActorCmp = GetComponent<T>();
            if (gameActorCmp!=null)
            {
                cmps.Remove(gameActorCmp);
                return true;
            }
            return false;
        }

        public bool HasComponent<T>() where T : GameActorCmp
        {
            bool hasCmp = false;
            //不包含就添加
            foreach (var gameActorCmp in cmps)
            {
                if (gameActorCmp is T t)
                {
                    hasCmp = true;
                    break;
                }
            }

            return hasCmp;
        }
    }
}