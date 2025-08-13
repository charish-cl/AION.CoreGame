using System;
using System.Collections.Generic;
using AION.CoreFramework;

namespace GameLogic
{
    public class ResDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

        public void Init(Func<TKey, TValue> initFunc)
        {
            Load(initFunc);
        }

        
        public void Load(Func<TKey, TValue> initFunc)
        {
            string Name = typeof(TKey).Name;
            
            
            // GameModule.Resource.LoadAsset<>("Assets/Game/Config/")
        }
    }
}