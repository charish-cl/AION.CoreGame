using System;
using System.Collections.Generic;
using UnityEngine;

namespace AION.CoreFramework
{
    /// <summary>
    /// 从GameFramework.ObjectPoolModule移植过来的ObjectPoolModule ，一点个人修改
    /// </summary>
    public class ObjectPoolModule:Module
    {
        private IObjectPoolManager _objectPoolManager;
        
        /// <summary>
        /// 对象池字典（类型 -> 对象池）
        /// </summary>
        private Dictionary<Type, object> m_poolMap = new Dictionary<Type, object>();
        
        /// <summary>
        /// 对象池配置字典（类型 -> 配置）
        /// </summary>
        private Dictionary<Type, PoolConfig> m_configMap = new Dictionary<Type, PoolConfig>();
        
        /// <summary>
        /// 工厂函数字典（类型 -> 工厂函数）
        /// </summary>
        private Dictionary<Type, Delegate> m_factoryMap = new Dictionary<Type, Delegate>();
        
        /// <summary>
        /// 对象池配置
        /// </summary>
        public class PoolConfig
        {
            public bool allowMultiSpawn = true;
            public int capacity = 10;
            public float releaseInterval = 10f;
        }
        
        protected override void Awake()
        {
            base.Awake();

            _objectPoolManager = ModuleImpSystem.GetModule<IObjectPoolManager>();
            if (_objectPoolManager == null)
            {
                Log.Fatal("ObjectPoolManager invalid.");
                return;
            }
        }
        
        public ObjectPool<T> CreateObjectPool<T>(bool allowMultiSpawn, int capacity, float releaseInterval) where T : ObjectBase
        {
            return _objectPoolManager.CreateObjectPool<T>(allowMultiSpawn, capacity, releaseInterval);
        }
        
        public ObjectPool<T> CreateObjectPool<T>() where T : ObjectBase
        {
            return _objectPoolManager.CreateObjectPool<T>(true, 10, 10);
        }
        
        /// <summary>
        /// 注册对象池配置（可选，在首次使用前调用）
        /// </summary>
        public void RegisterConfig<T>(PoolConfig config) where T : ObjectBase
        {
            m_configMap[typeof(T)] = config;
        }
        
        /// <summary>
        /// 注册工厂函数（用于创建新对象）
        /// </summary>
        public void RegisterFactory<T>(Func<T> factory) where T : ObjectBase
        {
            m_factoryMap[typeof(T)] = factory;
        }
        
        /// <summary>
        /// 获取对象池（如果不存在则自动创建）
        /// </summary>
        private ObjectPool<T> GetOrCreatePool<T>() where T : ObjectBase
        {
            Type type = typeof(T);
            
            if (!m_poolMap.TryGetValue(type, out var poolObj))
            {
                // 获取配置（如果已注册）
                PoolConfig config = m_configMap.TryGetValue(type, out var cfg) ? cfg : new PoolConfig();
                
                // 创建对象池
                var pool = CreateObjectPool<T>(
                    config.allowMultiSpawn,
                    config.capacity,
                    config.releaseInterval
                );
                
                m_poolMap[type] = pool;
                return pool;
            }
            
            return (ObjectPool<T>)poolObj;
        }
        
        /// <summary>
        /// 从对象池获取对象（如果池中没有则使用工厂创建）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="name">对象名称（用于Spawn，如果为空则使用类型名）</param>
        /// <param name="factory">自定义工厂函数（可选，如果不提供则使用注册的工厂）</param>
        /// <returns>对象实例</returns>
        public T Get<T>(string name = null, Func<T> factory = null) where T : ObjectBase
        {
            var pool = GetOrCreatePool<T>();
            
            // 如果没有提供名称，使用类型名
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }
            
            // 尝试从池中获取
            T obj = pool.Spawn(name);
            
            // 如果池中没有，使用工厂创建
            if (obj == null)
            {
                Func<T> createFactory = factory;
                
                // 如果没有提供工厂，尝试从注册的工厂获取
                if (createFactory == null && m_factoryMap.TryGetValue(typeof(T), out var factoryDelegate))
                {
                    createFactory = (Func<T>)factoryDelegate;
                }
                
                if (createFactory == null)
                {
                    Log.Error($"ObjectPoolModule.Get<{typeof(T).Name}>: 无法创建对象，未提供工厂函数且未注册工厂");
                    return null;
                }
                
                // 创建新对象
                obj = createFactory();
                
                if (obj == null)
                {
                    Log.Error($"ObjectPoolModule.Get<{typeof(T).Name}>: 工厂函数返回 null");
                    return null;
                }
                
                // 设置名称
                if (string.IsNullOrEmpty(obj.Name))
                {
                    obj.Name = name;
                }
                
                // 注册到对象池
                pool.Register(obj);
            }
            
            return obj;
        }
        
        /// <summary>
        /// 释放对象回池
        /// </summary>
        public void Release<T>(T obj) where T : ObjectBase
        {
            if (obj == null)
            {
                Log.Warning($"ObjectPoolModule.Release: 对象为空");
                return;
            }
            
            var pool = GetOrCreatePool<T>();
            pool.UnSpawn(obj);
        }
        
        /// <summary>
        /// 获取对象池（用于访问 objMap 等内部数据，如 Update 逻辑）
        /// </summary>
        public ObjectPool<T> GetPool<T>() where T : ObjectBase
        {
            return GetOrCreatePool<T>();
        }
        
        /// <summary>
        /// 清空指定类型的对象池
        /// </summary>
        public void Clear<T>() where T : ObjectBase
        {
            Type type = typeof(T);
            if (m_poolMap.TryGetValue(type, out var poolObj))
            {
                m_poolMap.Remove(type);
            }
        }
        
        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAll()
        {
            m_poolMap.Clear();
            m_configMap.Clear();
            m_factoryMap.Clear();
        }
    }
    
    /// <summary>
    /// Pool 静态扩展 - 提供便捷的泛型调用接口
    /// </summary>
    public static class Pool
    {
        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="name">对象名称（可选）</param>
        /// <param name="factory">自定义工厂函数（可选，如果不提供则使用注册的工厂）</param>
        /// <returns>对象实例</returns>
        public static T Get<T>(string name = null, Func<T> factory = null) where T : ObjectBase
        {
            return GameModule.ObjectPool.Get(name, factory);
        }
        
        /// <summary>
        /// 释放对象回池
        /// </summary>
        public static void Release<T>(T obj) where T : ObjectBase
        {
            GameModule.ObjectPool.Release(obj);
        }
        
        /// <summary>
        /// 注册工厂函数（用于创建新对象）
        /// </summary>
        public static void RegisterFactory<T>(Func<T> factory) where T : ObjectBase
        {
            GameModule.ObjectPool.RegisterFactory(factory);
        }
        
        /// <summary>
        /// 注册对象池配置
        /// </summary>
        public static void RegisterConfig<T>(ObjectPoolModule.PoolConfig config) where T : ObjectBase
        {
            GameModule.ObjectPool.RegisterConfig<T>(config);
        }
        
        /// <summary>
        /// 获取对象池（用于访问 objMap 等内部数据）
        /// </summary>
        public static ObjectPool<T> GetPool<T>() where T : ObjectBase
        {
            return GameModule.ObjectPool.GetPool<T>();
        }
    }
}