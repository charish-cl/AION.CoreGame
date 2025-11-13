using System;
using System.Collections.Generic;
using System.Linq;
using AION.CoreFramework;

namespace GameLogic
{
    public class ResDictionaryList<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> _dict = new();
        private readonly List<TKey> _sortedKeys = new();
        private readonly Func<TValue, TKey> _getKeyFunc;

        public ResDictionaryList(List<TValue> source, Func<TValue, TKey> getKeyFunc)
        {
            _getKeyFunc = getKeyFunc ?? throw new ArgumentNullException(nameof(getKeyFunc));

            if (source == null || source.Count == 0)
            {
                Log.Info($"[ResDictionaryList] Source list is null or empty. Type={typeof(TValue)}");
                return;
            }

            foreach (var item in source)
            {
                if (item == null)
                {
                    Log.Info($"[ResDictionaryList] Null item in list. Type={typeof(TValue)}");
                    continue;
                }

                TKey key = _getKeyFunc(item);
                if (!_dict.TryGetValue(key, out var list))
                {
                    list = new List<TValue>();
                    _dict[key] = list;
                }

                list.Add(item);
            }

            if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
            {
                _sortedKeys = _dict.Keys.OrderBy(k => k).ToList();
            }
            else
            {
                _sortedKeys = _dict.Keys.ToList();
            }
        }

        /// <summary>根据 key 获取配置列表</summary>
        public List<TValue> Get(TKey key)
        {
            if (key == null)
            {
                Log.Info($"[ResDictionaryList] Get with null key. Type={typeof(TValue)}");
                return null;
            }

            if (_dict.TryGetValue(key, out var list))
                return list;

            Log.Info($"[ResDictionaryList] Get failed. Key={key}, Type={typeof(TValue)}");
            return null;
        }

        /// <summary>是否为最大 Key（需要 TKey 可比较）</summary>
        public bool CheckIsMax(TKey key)
        {
            if (_sortedKeys.Count == 0)
                return true;

            if (key is not IComparable<TKey> comparableKey)
            {
                Log.Info($"[ResDictionaryList] TKey must be IComparable for CheckIsMax. Key={key}");
                return false;
            }

            var lastKey = _sortedKeys.Last();
            return comparableKey.CompareTo(lastKey) >= 0;
        }

        /// <summary>获取下一个 Key 对应的配置列表（需要 TKey 可比较）</summary>
        public List<TValue> GetNext(TKey currentKey)
        {
            if (_sortedKeys.Count == 0)
            {
                Log.Info($"[ResDictionaryList] Empty when GetNext. Type={typeof(TValue)}");
                return null;
            }

            if (currentKey == null)
            {
                Log.Info($"[ResDictionaryList] GetNext with null key.");
                return null;
            }

            for (int i = 0; i < _sortedKeys.Count; i++)
            {
                if (EqualityComparer<TKey>.Default.Equals(_sortedKeys[i], currentKey))
                {
                    if (i + 1 < _sortedKeys.Count)
                    {
                        var nextKey = _sortedKeys[i + 1];
                        return _dict[nextKey];
                    }

                    Log.Info($"[ResDictionaryList] Already max element. Key={currentKey}");
                    return null;
                }
            }

            Log.Info($"[ResDictionaryList] Key not found when GetNext. Key={currentKey}");
            return null;
        }

        public int Count => _dict.Count;

        /// <summary>获取所有数据字典</summary>
        public IReadOnlyDictionary<TKey, List<TValue>> GetAll() => _dict;
    }
}