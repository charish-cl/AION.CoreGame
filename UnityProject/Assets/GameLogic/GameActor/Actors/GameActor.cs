using System.Collections.Generic;
using AION.CoreFramework;
using UnityEngine;
using Object = UnityEngine.Object;
using GameConfig.res;
using GameConfig.battle;

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
        /// 模型实例（由 CreateModel 创建）
        /// </summary>
        protected GameObject m_ModelInstance { get; private set; }
        
        /// <summary>
        /// Prefab 名字（从模型配置中提取，用于命名 GameObject）
        /// </summary>
        public string PrefabName { get; private set; }
        
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
            
            // 如果已经设置了位置（Position 不为零），将 Transform 的位置同步到设置的位置
            // 否则使用 Transform 的位置
            if (Transform != null)
            {
                if (Position != Vector2.zero)
                {
                    // 如果已经设置了位置，将 Transform 的位置同步到设置的位置
                    Transform.position = Position;
                }
                else
                {
                    // 如果没有设置位置，使用 Transform 的位置
                    SetPosition(Transform.position);
                }
            }
        }
        /// <summary>
        /// 初始化配置（虚方法，子类重写）
        /// </summary>
        protected virtual void InitConfig()
        {
        }
        
        /// <summary>
        /// 创建模型（虚方法，子类重写）
        /// 在 InitConfig 之后、BindCmp 之前调用，确保组件初始化时模型已准备好
        /// </summary>
        protected virtual void CreateModel()
        {
            // 默认不创建模型，子类可以重写
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
        
        /// <summary>
        /// 实例化模型（内部方法，供子类调用）
        /// </summary>
        /// <param name="modelConfig">模型配置</param>
        protected void InstantiateModel(ModelConfig modelConfig)
        {
            if (modelConfig == null || string.IsNullOrEmpty(modelConfig.Path))
            {
                Log.Warning($"GameActor.InstantiateModel: 模型配置无效或路径为空");
                return;
            }
            
            // 加载模型资源
            GameObject prefab = GameModule.Resource.LoadAsset<GameObject>(modelConfig.Path);
            if (prefab == null)
            {
                Log.Error($"GameActor.InstantiateModel: 加载模型资源失败，路径 = {modelConfig.Path}");
                return;
            }
            
            // 从路径中提取 Prefab 名字（去掉路径和扩展名）
            string prefabName = ExtractPrefabName(modelConfig.Path);
            PrefabName = prefabName;
            
            // 保存当前设置的位置（如果已设置）
            Vector2 savedPosition = Position;
            
            // 实例化模型
            m_ModelInstance = Object.Instantiate(prefab);
            if (m_ModelInstance == null)
            {
                Log.Error($"GameActor.InstantiateModel: 实例化模型失败，路径 = {modelConfig.Path}");
                return;
            }
            
            // 绑定到 GameActor（BindGo 会处理位置同步）
            BindGo(m_ModelInstance);
            
            // 如果之前设置了位置，确保 Transform 使用该位置
            if (savedPosition != Vector2.zero && Transform != null)
            {
                Transform.position = savedPosition;
                Position = savedPosition;
            }
            
            // 应用模型偏移（ModelConfig.Offset）
            if ( Transform != null)
            {
                Vector2 offset = new Vector2(modelConfig.Offset.X, modelConfig.Offset.Y);
                if (offset != Vector2.zero)
                {
                    Transform.position += new Vector3(offset.x, offset.y, 0);
                    Position = Transform.position;
                    Log.Info($"GameActor.InstantiateModel: 应用模型偏移 ({offset.x}, {offset.y})");
                }

                if (modelConfig.Scale > 0)
                {
                    Transform.localScale =  modelConfig.Scale * Vector3.one;
                }
                else
                {
                    Log.Info($"GameActor.InstantiateModel: 模型尺寸未设置，使用默认值");
                }
            }
            
            Log.Info($"GameActor.InstantiateModel: 成功实例化模型，路径 = {modelConfig.Path}, Prefab名称 = {prefabName}");
        }
        
        /// <summary>
        /// 从路径中提取 Prefab 名字
        /// </summary>
        /// <param name="path">资源路径，例如 "Assets/Game/Prefab/Player.prefab"</param>
        /// <returns>Prefab 名字，例如 "Player"</returns>
        private string ExtractPrefabName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "Unknown";
            }
            
            // 去掉路径，只保留文件名
            string fileName = System.IO.Path.GetFileName(path);
            
            // 去掉扩展名
            if (fileName.EndsWith(".prefab"))
            {
                fileName = fileName.Substring(0, fileName.Length - 7);
            }
            
            return fileName;
        }
        
        protected virtual void BindCmp()
        {
        }
        public  void OnInit()
        {
            // 先初始化配置
            InitConfig();
            
            // 然后创建模型（在组件初始化之前，确保组件可以访问 Transform）
            CreateModel();
            
            // 然后添加组件
            BindCmp();
            
            // 在组件初始化之前，从配置初始化数值属性（子类重写）
            InitializeNumericFromConfig();
            
            // 最后初始化所有组件
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
            
            // 销毁模型实例
            if (m_ModelInstance != null)
            {
                Object.Destroy(m_ModelInstance);
                m_ModelInstance = null;
            }
            
            // 如果 m_Owner 不是模型实例，也要销毁
            if (m_Owner != null && m_Owner != m_ModelInstance)
            {
                Object.Destroy(m_Owner);
            }
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
        
        /// <summary>
        /// 从配置初始化数值组件（虚方法，子类重写）
        /// 在组件初始化之前调用，用于设置基础属性
        /// </summary>
        protected virtual void InitializeNumericFromConfig()
        {
            // 默认不初始化，子类重写
        }
    }
}