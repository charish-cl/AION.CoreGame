using System;
using System.Collections.Generic;

namespace AION.CoreFramework
{
    /// <summary>
    /// 游戏框架优先队列类，支持 DequeMax 和 DequeMin 操作。
    /// </summary>
    /// <typeparam name="TValue">指定优先队列中元素的类型。</typeparam>
    public class GameFrameworkPriorityQueue<TValue>
    {
        private readonly SortedDictionary<int, Queue<TValue>> _queueDictionary;

        /// <summary>
        /// 初始化优先队列的新实例。
        /// </summary>
        public GameFrameworkPriorityQueue()
        {
            _queueDictionary = new SortedDictionary<int, Queue<TValue>>();
        }

        /// <summary>
        /// 获取优先队列中的元素数量。
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 将元素按照指定的优先级入队。
        /// </summary>
        /// <param name="priority">优先级。</param>
        /// <param name="item">要入队的元素。</param>
        public void Enqueue(int priority, TValue item)
        {
            if (!_queueDictionary.ContainsKey(priority))
            {
                _queueDictionary[priority] = new Queue<TValue>();
            }

            _queueDictionary[priority].Enqueue(item);
            Count++;
        }
        

        /// <summary>
        /// 查看优先级最高的元素但不出队。
        /// </summary>
        /// <returns>优先级最高的元素。</returns>
        public TValue Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            var firstKey = FirstKey();
            return _queueDictionary[firstKey].Peek();
        }

        /// <summary>
        /// 清空优先队列。
        /// </summary>
        public void Clear()
        {
            _queueDictionary.Clear();
            Count = 0;
        }

        /// <summary>
        /// 将优先级最低的元素出队。
        /// </summary>
        /// <returns>优先级最低的元素。</returns>
        public TValue DequeMin()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            var firstKey = FirstKey();
            var queue = _queueDictionary[firstKey];
            var item = queue.Dequeue();
            Count--;

            if (queue.Count == 0)
            {
                _queueDictionary.Remove(firstKey);
            }

            return item;
        }

        /// <summary>
        /// 将优先级最高的元素出队。
        /// </summary>
        /// <returns>优先级最高的元素。</returns>
        public TValue DequeMax()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            var lastKey = LastKey();
            var queue = _queueDictionary[lastKey];
            var item = queue.Dequeue();
            Count--;

            if (queue.Count == 0)
            {
                _queueDictionary.Remove(lastKey);
            }

            return item;
        }

        /// <summary>
        /// 获取优先队列中的最小优先级。
        /// </summary>
        /// <returns>最小优先级。</returns>
        private int FirstKey()
        {
            foreach (var key in _queueDictionary.Keys)
            {
                return key;
            }

            throw new InvalidOperationException("The priority queue is in an invalid state.");
        }

        /// <summary>
        /// 获取优先队列中的最大优先级。
        /// </summary>
        /// <returns>最大优先级。</returns>
        private int LastKey()
        {
            int lastKey = default;
            foreach (var key in _queueDictionary.Keys)
            {
                lastKey = key;
            }

            return lastKey;
        }
    }
}
