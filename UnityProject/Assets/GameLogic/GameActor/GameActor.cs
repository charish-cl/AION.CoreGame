using System.Collections.Generic;
using UnityEngine;

namespace AION.CoreFramework
{
    public class GameActor
    {
        public LinkedList<GameActorCmp> cmps = new LinkedList<GameActorCmp>();

        public Transform m_transform;
        
        public GameObject m_Owner;
        //事件


        #region 事件

        public ActorEventDispatcher EventDispatcher;

        #endregion

        #region 生命周期

        protected virtual void BindCmp()
        {
            
        }
        protected virtual void OnInit()
        {
            EventDispatcher = MemoryPool.Acquire<ActorEventDispatcher>();
            //这里添加组件
            BindCmp();
        }
        
        protected virtual void OnUpdate()
        {
            foreach (var gameActorCmp in cmps)
            {
                gameActorCmp.OnUpdate();
            }
        }


        protected virtual void OnDestroy()
        {
            
            foreach (var gameActorCmp in cmps)
            {
                gameActorCmp.OnDestroy();
            }
            
            MemoryPool.Release(EventDispatcher);
            
            Object.Destroy(m_Owner);
            
        }


        #endregion
     
        /// <summary>
        /// 简单的依赖注入，使用关联性很强的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSevice<T>() where T:class
        {
            //这里要等一帧，再执行，等所有的组件收集完
            foreach (var gameActorCmp in cmps)
            {
                if (gameActorCmp is T t)
                {
                    return t;
                }
            }
            return null;
        }

        public T AddComponent<T>() where T : GameActorCmp, new()
        {
            T newCmp = null;
            if (!HasComponent<T>())
            {
                newCmp = new T();
                cmps.AddLast(newCmp);
            }
            return newCmp;
        }

        public T GetComponet<T>() where T : GameActorCmp, new()
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

        public bool RemoveComponent<T>() where T : GameActorCmp, new()
        {
            var gameActorCmp = GetComponet<T>();
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