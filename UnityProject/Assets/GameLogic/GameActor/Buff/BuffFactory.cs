using System.Collections.Generic;
using GameConfig;
using GameConfig.battle;
using AION.Config.Buff;
using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// Buff工厂类，根据BuffConfig创建Buff实例
    /// </summary>
    public static class BuffFactory
    {
        /// <summary>
        /// 根据BuffConfig创建Buff实例
        /// </summary>
        /// <param name="config">Buff配置</param>
        /// <param name="targetActor">目标Actor（可选）</param>
        /// <returns>创建的Buff实例</returns>
        public static BaseBuff CreateBuff(BuffConfig config, GameActor targetActor = null)
        {
            if (config == null)
            {
                Log.Error("BuffFactory: BuffConfig is null");
                return null;
            }
            
            BaseBuff buff = null;
            AttributeModifier modifier = null;
            
            // 根据BuffType创建不同类型的Buff
            switch (config.BuffType)
            {
                case EBuffType.PropertyMod:
                    // 属性修改类Buff，需要创建AttributeModifier
                    modifier = CreateAttributeModifier(config);
                    break;
                    
                case EBuffType.Heal:
                case EBuffType.Damage:
                case EBuffType.Status:
                    // 这些类型不需要Modifier，但可以创建空的
                    modifier = null;
                    break;
            }
            
            // 创建Buff实例
            buff = new BaseBuff(
                config.Id.ToString(), 
                config.Duration, 
                modifier
            );
            
            // 设置Buff属性
            buff.BuffId = config.Id;
            buff.BuffType = config.BuffType;
            buff.TriggerType = config.TriggerType;
            buff.TickInterval = config.TickInterval;
            buff.StatusId = config.StatusId;
            buff.ValueParams = config.ValueParams != null 
                ? new List<float>(config.ValueParams) 
                : new List<float>();
            
            // 从ValueParams中提取概率值（如果是概率触发）
            if (config.TriggerType == ETriggerType.Probability && buff.ValueParams.Count > 0)
            {
                // 假设概率值在ValueParams的第一个位置（0-1之间）
                buff.Probability = Mathf.Clamp01(buff.ValueParams[0]);
            }
            
            return buff;
        }
        
        /// <summary>
        /// 根据BuffID创建Buff实例
        /// </summary>
        public static BaseBuff CreateBuff(int buffId, GameActor targetActor = null)
        {
            var config = ConfigSystem.Instance.Tables.TbBuff.GetOrDefault(buffId);
            if (config == null)
            {
                Log.Error($"BuffFactory: BuffConfig not found for ID: {buffId}");
                return null;
            }
            
            return CreateBuff(config, targetActor);
        }
        
        /// <summary>
        /// 创建AttributeModifier（用于属性修改类Buff）
        /// </summary>
        private static AttributeModifier CreateAttributeModifier(BuffConfig config)
        {
            if (config.ValueParams == null || config.ValueParams.Count < 2)
            {
                Log.Warning($"BuffFactory: PropertyMod buff {config.Id} needs at least 2 value params: [NumericType, Value, ModifierType(optional)]");
                return null;
            }
            
            // ValueParams格式: [NumericType, Value, ModifierType(可选，默认PercentAdd)]
            // 例如: [1004, 0.2, 1] 表示攻击力+20%，使用PercentAdd
            int numericTypeInt = (int)config.ValueParams[0];
            float value = config.ValueParams[1];
            ModifierType modType = ModifierType.PercentAdd; // 默认百分比加法
            
            // 如果有第三个参数，使用它作为ModifierType
            if (config.ValueParams.Count >= 3)
            {
                int modTypeInt = (int)config.ValueParams[2];
                modType = (ModifierType)modTypeInt;
            }
                
            NumericType numericType = (NumericType)numericTypeInt;
            
            return new AttributeModifier(modType, numericType, value, config);
        }
    }
}

