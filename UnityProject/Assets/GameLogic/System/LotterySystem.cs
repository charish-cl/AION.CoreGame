using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 抽奖项数据（带评分）
    /// </summary>
    [System.Serializable]
    public class LotteryItem<T>
    {
        /// <summary>
        /// 抽奖项ID
        /// </summary>
        public int id;
        
        /// <summary>
        /// 抽奖项数据
        /// </summary>
        public T data;
        
        /// <summary>
        /// 稀有度品质评分（越高越稀有，抽中概率越低）
        /// </summary>
        public float rarityScore;
        
        /// <summary>
        /// 权重（根据rarityScore计算，用于抽奖）
        /// </summary>
        public float weight;
        
        public LotteryItem(int id, T data, float rarityScore)
        {
            this.id = id;
            this.data = data;
            this.rarityScore = rarityScore;
            this.weight = 0f; // 由LotterySystem计算
        }
    }
    
    /// <summary>
    /// 抽奖结果
    /// </summary>
    [System.Serializable]
    public class LotteryResult<T>
    {
        /// <summary>
        /// 抽中的项
        /// </summary>
        public LotteryItem<T> item;
        
        /// <summary>
        /// 抽奖时间戳
        /// </summary>
        public float timestamp;
        
        public LotteryResult(LotteryItem<T> item, float timestamp)
        {
            this.item = item;
            this.timestamp = timestamp;
        }
    }
    
    /// <summary>
    /// 通用抽奖系统 - 使用正态分布和稀有度评分，满足玩家期望
    /// </summary>
    public class LotterySystem<T>
    {
        /// <summary>
        /// 抽奖项列表
        /// </summary>
        private List<LotteryItem<T>> m_items = new List<LotteryItem<T>>();
        
        /// <summary>
        /// 分布影响参数（0-1，越大越容易抽到高稀有度）
        /// </summary>
        private float m_distributionFactor = 0.5f;
        
        /// <summary>
        /// 期望值（用于正态分布，控制整体稀有度期望）
        /// </summary>
        private float m_expectedValue = 0.5f;
        
        /// <summary>
        /// 标准差（用于正态分布，控制分布的集中程度）
        /// </summary>
        private float m_standardDeviation = 0.2f;
        
        /// <summary>
        /// 历史抽奖记录（用于平衡，避免一直高或一直低）
        /// </summary>
        private List<float> m_drawHistory = new List<float>();
        
        /// <summary>
        /// 历史记录最大长度（用于平衡算法）
        /// </summary>
        private int m_maxHistoryLength = 20;
        
        /// <summary>
        /// 设置分布影响参数（0-1，越大越容易抽到高稀有度）
        /// </summary>
        public void SetDistributionFactor(float factor)
        {
            m_distributionFactor = Mathf.Clamp01(factor);
        }
        
        /// <summary>
        /// 设置期望值和标准差（用于正态分布）
        /// </summary>
        public void SetDistributionParams(float expectedValue, float standardDeviation)
        {
            m_expectedValue = expectedValue;
            m_standardDeviation = Mathf.Max(0.01f, standardDeviation);
        }
        
        /// <summary>
        /// 添加抽奖项
        /// </summary>
        public void AddItem(int id, T data, float rarityScore)
        {
            var item = new LotteryItem<T>(id, data, rarityScore);
            m_items.Add(item);
            UpdateWeights();
        }
        
        /// <summary>
        /// 批量添加抽奖项
        /// </summary>
        public void AddItems(List<LotteryItem<T>> items)
        {
            m_items.AddRange(items);
            UpdateWeights();
        }
        
        /// <summary>
        /// 清空所有抽奖项
        /// </summary>
        public void ClearItems()
        {
            m_items.Clear();
            m_drawHistory.Clear();
        }
        
        /// <summary>
        /// 更新权重（根据稀有度评分和分布参数）
        /// </summary>
        private void UpdateWeights()
        {
            if (m_items.Count == 0) return;
            
            // 计算平均稀有度（用于归一化）
            float avgRarity = m_items.Average(item => item.rarityScore);
            float maxRarity = m_items.Max(item => item.rarityScore);
            float minRarity = m_items.Min(item => item.rarityScore);
            float rarityRange = maxRarity - minRarity;
            
            if (rarityRange <= 0.001f)
            {
                // 如果所有稀有度相同，使用均匀分布
                float uniformWeight = 1f / m_items.Count;
                foreach (var item in m_items)
                {
                    item.weight = uniformWeight;
                }
                return;
            }
            
            // 计算历史平均稀有度（用于平衡）
            float historyAvgRarity = 0f;
            if (m_drawHistory.Count > 0)
            {
                historyAvgRarity = m_drawHistory.Average();
            }
            else
            {
                historyAvgRarity = avgRarity;
            }
            
            // 计算每个项的权重
            float totalWeight = 0f;
            foreach (var item in m_items)
            {
                // 归一化稀有度（0-1）
                float normalizedRarity = (item.rarityScore - minRarity) / rarityRange;
                
                // 使用正态分布计算基础权重
                float baseWeight = CalculateNormalDistributionWeight(normalizedRarity);
                
                // 根据分布影响参数调整权重
                // distributionFactor越大，高稀有度权重越高
                float adjustedWeight = baseWeight * (1f + m_distributionFactor * (normalizedRarity - 0.5f));
                
                // 平衡调整：如果历史平均稀有度偏低，增加高稀有度权重；反之亦然
                float balanceAdjustment = 1f;
                if (m_drawHistory.Count > 5)
                {
                    float targetRarity = m_expectedValue;
                    float currentAvg = historyAvgRarity;
                    float deviation = targetRarity - currentAvg;
                    
                    // 如果当前平均偏低，增加高稀有度权重
                    if (deviation > 0.05f && normalizedRarity > 0.6f)
                    {
                        balanceAdjustment = 1f + deviation * 2f;
                    }
                    // 如果当前平均偏高，增加低稀有度权重
                    else if (deviation < -0.05f && normalizedRarity < 0.4f)
                    {
                        balanceAdjustment = 1f - deviation * 2f;
                    }
                }
                
                item.weight = adjustedWeight * balanceAdjustment;
                totalWeight += item.weight;
            }
            
            // 归一化权重
            if (totalWeight > 0.001f)
            {
                foreach (var item in m_items)
                {
                    item.weight /= totalWeight;
                }
            }
        }
        
        /// <summary>
        /// 计算正态分布权重
        /// </summary>
        private float CalculateNormalDistributionWeight(float normalizedRarity)
        {
            // 使用正态分布公式：f(x) = (1 / (σ * sqrt(2π))) * exp(-0.5 * ((x - μ) / σ)^2)
            float x = normalizedRarity;
            float mu = m_expectedValue;
            float sigma = m_standardDeviation;
            
            float exponent = -0.5f * Mathf.Pow((x - mu) / sigma, 2f);
            float weight = Mathf.Exp(exponent) / (sigma * Mathf.Sqrt(2f * Mathf.PI));
            
            return weight;
        }
        
        /// <summary>
        /// 抽奖（返回单个结果）
        /// </summary>
        public LotteryResult<T> Draw()
        {
            if (m_items.Count == 0)
            {
                Log.Warning("LotterySystem: 抽奖项为空，无法抽奖");
                return null;
            }
            
            // 更新权重（考虑历史平衡）
            UpdateWeights();
            
            // 使用加权随机抽取
            float random = UnityEngine.Random.Range(0f, 1f);
            float cumulativeWeight = 0f;
            
            LotteryItem<T> selectedItem = null;
            foreach (var item in m_items)
            {
                cumulativeWeight += item.weight;
                if (random <= cumulativeWeight)
                {
                    selectedItem = item;
                    break;
                }
            }
            
            // 如果由于浮点误差没有选中，选择最后一个
            if (selectedItem == null)
            {
                selectedItem = m_items[m_items.Count - 1];
            }
            
            // 记录历史（用于平衡）
            float normalizedRarity = (selectedItem.rarityScore - m_items.Min(i => i.rarityScore)) / 
                                     (m_items.Max(i => i.rarityScore) - m_items.Min(i => i.rarityScore));
            m_drawHistory.Add(normalizedRarity);
            if (m_drawHistory.Count > m_maxHistoryLength)
            {
                m_drawHistory.RemoveAt(0);
            }
            
            return new LotteryResult<T>(selectedItem, Time.time);
        }
        
        /// <summary>
        /// 抽奖（返回多个结果，不重复）
        /// </summary>
        public List<LotteryResult<T>> DrawMultiple(int count)
        {
            if (count <= 0 || m_items.Count == 0)
            {
                return new List<LotteryResult<T>>();
            }
            
            List<LotteryResult<T>> results = new List<LotteryResult<T>>();
            List<LotteryItem<T>> availableItems = new List<LotteryItem<T>>(m_items);
            
            // 每次抽取后，临时移除已抽取的项（避免重复）
            for (int i = 0; i < count && availableItems.Count > 0; i++)
            {
                // 创建临时抽奖系统
                var tempLottery = new LotterySystem<T>();
                tempLottery.SetDistributionFactor(m_distributionFactor);
                tempLottery.SetDistributionParams(m_expectedValue, m_standardDeviation);
                tempLottery.AddItems(availableItems);
                
                var result = tempLottery.Draw();
                if (result != null)
                {
                    results.Add(result);
                    availableItems.Remove(result.item);
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 获取所有抽奖项（只读）
        /// </summary>
        public IReadOnlyList<LotteryItem<T>> Items => m_items;
        
        /// <summary>
        /// 获取历史平均稀有度（只读）
        /// </summary>
        public float HistoryAverageRarity
        {
            get
            {
                if (m_drawHistory.Count == 0) return 0f;
                return m_drawHistory.Average();
            }
        }
    }
}

