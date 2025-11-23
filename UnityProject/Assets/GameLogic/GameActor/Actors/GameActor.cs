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
        Base,  // 基地
    }
    public class GameActor
    {
        public LinkedList<GameActorCmp> cmps = new LinkedList<GameActorCmp>();

        private Transform m_transform;
        
        /// <summary>
        /// 配置字典，存储不同类型的配置
        /// </summary>
        private Dictionary<System.Type, object> m_configs = new Dictionary<System.Type, object>();
        
        /// <summary>
        /// Transform 属性，获取时进行判空
        /// </summary>
        public Transform Transform
        {
            get
            {
                if (m_transform == null)
                {
                    if (m_Owner != null)
                    {
                        m_transform = m_Owner.transform;
                    }
                    else
                    {
                        Log.Warning("GameActor.Transform: m_Owner 为空，无法获取 Transform");
                        return null;
                    }
                }
                return m_transform;
            }
        }
        
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

        /// <summary>
        /// 获取单位数值属性（简化方式）
        /// </summary>
        /// <param name="numericType">数值类型</param>
        /// <returns>浮点数值</returns>
        public float GetProperty(NumericType numericType)
        {
            if (NumericComponent != null)
            {
                return NumericComponent.GetAsFloat(numericType);
            }
            return 0f;
        }
        
        /// <summary>
        /// 获取单位数值属性（整数版本）
        /// </summary>
        /// <param name="numericType">数值类型</param>
        /// <returns>整数值</returns>
        public int GetPropertyInt(NumericType numericType)
        {
            if (NumericComponent != null)
            {
                return NumericComponent.GetAsInt(numericType);
            }
            return 0;
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
            m_transform = owner != null ? owner.transform : null;
            
            if (Transform != null)
            {
                SetPosition(Transform.position);
            }
        }
        /// <summary>
        /// 初始化配置（虚方法，子类重写）
        /// </summary>
        protected virtual void InitConfig()
        {
        }
        
        /// <summary>
        /// 设置配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="config">配置对象</param>
        protected void SetConfig<T>(T config)
        {
            m_configs[typeof(T)] = config;
        }
        
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <returns>配置对象，如果不存在则返回null</returns>
        public T GetConfig<T>()
        {
            if (m_configs.TryGetValue(typeof(T), out object config))
            {
                return (T)config;
            }
            return default(T);
        }
        
        protected virtual void BindCmp()
        {
        }
        public  void OnInit()
        {
            // 先初始化配置
            InitConfig();
            
            // 然后添加组件
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