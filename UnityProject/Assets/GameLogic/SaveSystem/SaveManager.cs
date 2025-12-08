using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSaveSystem
{
    public class SaveManager
    {
        private static SaveManager s_instance;
        public static SaveManager Instance => s_instance ??= new SaveManager();

        private Dictionary<Type, object> m_cache = new Dictionary<Type, object>();

        /// <summary>获取存档数据（自动创建或加载）</summary>
        public T Get<T>() where T : class, new()
        {
            Type type = typeof(T);
            if (m_cache.TryGetValue(type, out var cached)) return cached as T;

            T data = new T();
            Load(data);
            m_cache[type] = data;
            return data;
        }

        /// <summary>保存数据到磁盘</summary>
        public void Save<T>(T data) where T : class
        {
            string key = GetKey<T>();
            string json = JsonSerializer.Serialize(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            Debug.Log($"[Save] {key}: {json}");
        }

        /// <summary>从磁盘加载数据到对象</summary>
        public void Load<T>(T data) where T : class
        {
            string key = GetKey<T>();
            if (PlayerPrefs.HasKey(key))
            {
                JsonSerializer.Deserialize(PlayerPrefs.GetString(key), data);
            }
        }

        /// <summary>删除特定类型的存档</summary>
        public void Delete<T>()
        {
            string key = GetKey<T>();
            PlayerPrefs.DeleteKey(key);
            m_cache.Remove(typeof(T));
        }

        /// <summary>清空内存缓存（不删文件）</summary>
        public void ClearCache() => m_cache.Clear();

        /// <summary>删除所有存档（慎用）</summary>
        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            ClearCache();
        }

        private string GetKey<T>() => $"SAVE_{typeof(T).Name}";
    }
}