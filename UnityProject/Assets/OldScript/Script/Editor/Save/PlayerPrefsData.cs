using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameDevKitEditor
{
    public class PlayerPrefPair
    {
        public string Key;
        public object Value;
    }
    // PlayerPrefs 数据类，用于管理每个 PlayerPrefs 键值
    public class PlayerPrefsData
    {
        public string Key;
        public string Value;

        public bool IsSelected;

         Action<string, string> OnSave;
        
         Action<string> OnDelete;
        // 构造函数
        public PlayerPrefsData(string key, string value,Action<string, string> onSave,Action<string> onDelete)
        {
            Key = key;
            Value = value;
            OnSave = onSave;
            OnDelete = onDelete;
        }

        // 保存修改后的 PlayerPrefs 数据
        [Button("保存修改")]
        private void Save()
        {
           OnSave?.Invoke(Key, Value);
        }

        // 删除当前 PlayerPrefs 数据
        [Button("删除")]
        private void Delete()
        {
            OnDelete?.Invoke(Key);
        }
    }
}