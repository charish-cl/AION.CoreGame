using System.Collections.Generic;

namespace AION.CoreFramework
{
    public class LRUCache<K, V>
    {
        private int capacity;
        private Dictionary<K, LinkedListNode<KeyValuePair<K, V>>> cache;
        private LinkedList<KeyValuePair<K, V>> lruList;


        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            cache = new Dictionary<K, LinkedListNode<KeyValuePair<K, V>>>(capacity);
            lruList = new LinkedList<KeyValuePair<K, V>>();
        }

        public V Get(K key)
        {
            if (!cache.TryGetValue(key, out LinkedListNode<KeyValuePair<K, V>> node))
            {
                return default(V);
            }

            lruList.Remove(node);
            lruList.AddLast(node);
            return node.Value.Value;
        }

        public void Set(K key, V value)
        {
            if (!cache.TryGetValue(key, out LinkedListNode<KeyValuePair<K, V>> node))
            {
                // If key doesn't exist, add it to the cache
                if (cache.Count >= capacity)
                {
                    // Remove the least recently used item
                    var oldest = lruList.First;
                    lruList.RemoveFirst();
                    cache.Remove(oldest.Value.Key);
                }

                node = new LinkedListNode<KeyValuePair<K, V>>(new KeyValuePair<K, V>(key, value));
                lruList.AddLast(node);
                cache.Add(key, node);
            }
            else
            {
                // If key exists, update its value and move to the end of the list
                node.Value = new KeyValuePair<K, V>(key, value);
                lruList.Remove(node);
                lruList.AddLast(node);
            }
        }
    }
}