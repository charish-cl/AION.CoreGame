using System;
using System.Collections.Generic;

namespace GameProto
{
    //参考属性包装器wrapper，这个是用来监听Item变化的，也可以用来监听货币变化
    public class GameEntryMonoitor<T>
    {
        private List<Action<T>> OnValueChange { get; set; } = new List<Action<T>>();
        
        private T Entry { get; set; }

        public long Value { get;private set; }
        
        public Func<T, long> GetValueFunc { get; set; }
        
        
        
        public GameEntryMonoitor(T entry, Func<T, long> getValueFunc)
        {
            Entry = entry;
            GetValueFunc = getValueFunc;
        }
        //可以用来绑定红点/ 界面 显示 货币显示
        public void BindAction(Action<T> action)
        {
            OnValueChange.Add(action);
        }

        public void SetValue(T entry)
        {
            Value = GetValueFunc(Entry);
            foreach (var action in OnValueChange)
            {
                action(Entry);
            }
        }
    }
}