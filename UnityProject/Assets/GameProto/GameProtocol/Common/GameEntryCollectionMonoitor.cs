using System;
using System.Collections.Generic;
using JetBrains.Annotations;

//适用于皮肤，道具，这种具有全局唯一的实体

namespace GameProto
{
    
    /// <summary>
    /// 通用与服务端协议相交互的字典类，封装了增删改操作 ,这个针对的是唯一的道具ID
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GameEntryCollectionMonoitor<T>  where T : class
    {
        Dictionary<uint, T> dict = new Dictionary<uint, T>();
        
        Func<T, uint> GetIdFunc;
        
  

        List<Action<T>> redActions = new List<Action<T>>();
        
        public GameEntryCollectionMonoitor(Func<T, uint> getIdFunc)
        {
            this.GetIdFunc = getIdFunc;
        }
     

        // HashSet<uint> dirtySet = new HashSet<uint>();
        // bool isDirty = false;
        // bool isTotalDirty = false;
        //
        // public void SetDirty(uint id)
        // {
        //     dirtySet.Add(id);
        // }

        void UpdateItem(T item)
        {
            uint id = GetIdFunc(item);
            if (dict.ContainsKey(id))
            {
                dict[id] = item;
            }
            else
            {
                throw new Exception($"item {id} not found ");
            }
        }
 
        /// <summary>
        /// 封装数据变更操作，让数据更新更加方便，完全解耦了客户端和服务端的协议 ,约定itemList中的数据都是有效的，且非空
        /// </summary>
        /// <param name="op"></param>
        /// <param name="itemList"></param>
        public void Operate(byte op, [NotNull]List<T> itemList)
        {
            uint id = 0;
            switch (op)
            {
                case 1: //  update
                    if (itemList.Count == 0)
                    {
                        itemList.Clear();
                    }
                    foreach (var item in itemList)
                    {
                        UpdateItem(item);
                    }
                    break;
                
                case 2: // add
                    foreach (var item in itemList)
                    {
                        dict.Add(GetIdFunc(item), item);
                    }
                    break;
                case 3: // remove
                    foreach (var item in itemList)
                    {
                        dict.Remove(GetIdFunc(item));
                    }
                    break;
            }

            UpdateAllRedNode();
        }

        private void UpdateAllRedNode()
        {
            
            foreach (var redAction in redActions)
            {
                if (redAction == null)
                {
                    continue;
                }
                foreach (var keyValuePair in dict)
                {
                    redAction.Invoke(keyValuePair.Value);
                }
            }
        }


        public void BindRedAction(Action<T> action)
        {
            redActions.Add(action);
        }
        
        public List<T> GetItemList()
        {
            return new List<T>(dict.Values);
        }
        
        //使用类型约束确保 T 是引用类型
        public T GetItem(uint id)
        {
            T item;
            if (dict.TryGetValue(id, out item))
            {
                return item;
            }
            return null;
        }

        
        
        //有的逻辑并不是客户端收到就拥有了，也有可能是已经解锁了，所以这里给出一个虚方法，让子类自己实现
        public virtual bool IsOwn(uint id)
        {
            return dict.ContainsKey(id);
        }


        //实际上并不完全由这种方式判断
        public virtual int GetNum(uint id)
        {
            return dict.ContainsKey(id)? 1 : 0;
        }
        
        
        public void Dispose()
        {
            dict.Clear();
            redActions.Clear();
            GetIdFunc = null;
        }
    }
}