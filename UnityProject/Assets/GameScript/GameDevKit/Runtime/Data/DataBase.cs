using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDevKit
{
    /// <summary>
    /// 这个是参考在深渊项目里的思路吧，一个配置父类，搭配一堆子文件
    /// </summary>
    public abstract class BaseDataBase:SerializedScriptableObject
    {
    }
    //数据商店,只读
    public  class DataBase<T> :BaseDataBase where T : SerializedScriptableObject
    {
        [ListDrawerSettings(Expanded = true,CustomAddFunction = "Add",CustomRemoveIndexFunction = "Remove")]
        public List<T> Items = new List<T>();

        readonly Dictionary<Guid, T> _Dictionary = new Dictionary<Guid, T>();
        
        //Add a SerializedScriptableObject to this object,it's type is T,Use Unity AssetDatabase.AddObjectToAsset(other,obj);
        private void Add()
        {
            var item = ScriptableObject.CreateInstance<T>();
            item.name = typeof(T).Name;
            this.AddObjectToObject(item);
            Items.Add(item);
            // var data = item as BaseData;
            // _Dictionary.Add(data.Guid,item);
        }
      
        //Remove
        private void Remove(int index)
        {
            Debug.Log("移除");
            var item = Items[index];
            AssetHelper.RemoveObjectFromAsset(item);
            Items.RemoveAt(index);
            
        }
        public T Get(Guid guid)
        {
            if (_Dictionary.ContainsKey(guid))
            {
                return _Dictionary[guid];
            }
            return null;
        }
    }
}