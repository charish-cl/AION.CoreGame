using System.Collections.Generic;
using UnityEngine;
using AION.CoreFramework;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLogic
{
    /// <summary>
    /// LocalSettings 管理器，提供统一的本地设置加载接口
    /// 编辑器模式下使用 AssetDatabase 加载，运行时使用 Resources.Load
    /// </summary>
    public static class LS
    {
        private static readonly Dictionary<System.Type, GameLocalSetting> _cache =
            new Dictionary<System.Type, GameLocalSetting>();

        private const string configPath = "Assets/Game/Config/LocalSettings/";

        /// <summary>
        /// 获取指定类型的本地设置
        /// </summary>
        /// <typeparam name="T">继承自 GameLocalSetting 的类型</typeparam>
        /// <returns>本地设置实例，如果未找到则返回 null</returns>
        public static T Get<T>() where T : GameLocalSetting
        {
            System.Type type = typeof(T);

            // 先从缓存中查找
            if (_cache.TryGetValue(type, out GameLocalSetting cached))
            {
                return cached as T;
            }

            T setting = LoadSetting<T>();

            if (setting != null)
            {
                _cache[type] = setting;
            }

            return setting;
        }

        private static T RunTimeLoad<T>(string fileName) where T : Object
        {
           return GameModule.Resource.LoadAsset<T>($"{configPath}{fileName}.asset");
        }
        /// <summary>
        /// 加载本地设置
        /// </summary>
        private static T LoadSetting<T>() where T : GameLocalSetting
        {
            string fileName = typeof(T).Name;
            T setting = null;
            
#if UNITY_EDITOR
            
            if (Application.isPlaying )
            {
                setting = RunTimeLoad<T>(fileName);
            }
            else
            {
                string directPath = $"{configPath}{fileName}.asset";
                setting = AssetDatabase.LoadAssetAtPath<T>(directPath);
            }
#else
            setting = RunTimeLoad<T>(fileName);

#endif

            if (setting == null)
            {
                Debug.LogWarning($"未找到本地设置: {fileName}，请确保文件存在于正确的位置");
            }

            return setting;
        }

        /// <summary>
        /// 清除缓存（用于重新加载设置）
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 清除指定类型的缓存
        /// </summary>
        public static void ClearCache<T>() where T : GameLocalSetting
        {
            _cache.Remove(typeof(T));
        }
    }
}