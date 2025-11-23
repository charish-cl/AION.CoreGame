using System;
using AION.CoreFramework;
using GameLogic;
using GameConfig;
using UnityEngine;

namespace GameLogic
{
    public class BaseBuff
    {
        public bool IsExpired = false;
        public string Id;
        public int BuffId; // BuffConfig的ID
        public float Duration;
        public Action OnBuffExpired;
        
        // Buff配置信息
        public ETriggerType TriggerType { get; set; }
        public float TickInterval { get; set; }
        public float Probability { get;  set; } // 概率触发时的概率值
        public int StatusId { get; set; }
        
        // 触发相关
        protected float m_lastTickTime = 0f;
        protected bool m_hasTriggeredImmediate = false;
        protected GameActor m_targetActor; // 目标Actor
        protected NumericComponent m_attackerNumeric; // 攻击者的数值组件（用于伤害计算）
        protected System.Collections.Generic.List<BaseEffect> m_effects; // Buff效果实例列表（支持多个效果）
        protected System.Collections.Generic.List<AttributeModifier> m_modifiers; // 属性修改器列表（用于PropertyMod类型的Effect）
        protected BaseCondition m_condition; // Buff触发条件实例
        protected ETriggerType m_conditionType = ETriggerType.Immediate; // 条件类型
        protected System.Collections.Generic.List<float> m_conditionParams; // 条件参数
        protected BaseTargetSelector m_targetSelector; // Buff目标选择器实例
        
        /// <summary>
        /// 获取目标Actor（用于检查是否已设置）
        /// </summary>
        public GameActor TargetActor => m_targetActor;
        
        public BaseBuff(string id, float duration)
        {
            Id = id;
            Duration = duration;
            m_effects = new System.Collections.Generic.List<BaseEffect>();
            m_modifiers = new System.Collections.Generic.List<AttributeModifier>();
        }
        
        /// <summary>
        /// 添加效果实例
        /// </summary>
        public void AddEffect(BaseEffect effect)
        {
            if (effect != null)
            {
                m_effects.Add(effect);
            }
        }
        
        /// <summary>
        /// 添加属性修改器
        /// </summary>
        public void AddModifier(AttributeModifier modifier)
        {
            if (modifier != null)
            {
                m_modifiers.Add(modifier);
            }
        }
        
        /// <summary>
        /// 获取所有效果
        /// </summary>
        public System.Collections.Generic.List<BaseEffect> GetEffects()
        {
            return m_effects ?? new System.Collections.Generic.List<BaseEffect>();
        }
        
        /// <summary>
        /// 获取所有属性修改器
        /// </summary>
        public System.Collections.Generic.List<AttributeModifier> GetModifiers()
        {
            return m_modifiers ?? new System.Collections.Generic.List<AttributeModifier>();
        }
        
        /// <summary>
        /// 设置触发条件
        /// </summary>
        /// <param name="conditionType">条件类型</param>
        /// <param name="conditionParams">条件参数</param>
        public void SetCondition(ETriggerType conditionType, System.Collections.Generic.List<float> conditionParams = null)
        {
            m_conditionType = conditionType;
            m_conditionParams = conditionParams ?? new System.Collections.Generic.List<float>();
        }
        
        /// <summary>
        /// 设置条件实例
        /// </summary>
        public void SetCondition(BaseCondition condition)
        {
            m_condition = condition;
        }
        
        /// <summary>
        /// 设置目标选择器实例
        /// </summary>
        public void SetTargetSelector(BaseTargetSelector targetSelector)
        {
            m_targetSelector = targetSelector;
        }
        
        /// <summary>
        /// 获取目标列表（使用TargetSelector）
        /// </summary>
        /// <returns>目标Actor列表</returns>
        public System.Collections.Generic.List<GameActor> GetTargets()
        {
            if (m_targetSelector != null)
            {
                return m_targetSelector.SelectTargets();
            }
            
            // 如果没有TargetSelector，返回当前目标Actor（如果存在）
            if (m_targetActor != null)
            {
                return new System.Collections.Generic.List<GameActor> { m_targetActor };
            }
            
            return new System.Collections.Generic.List<GameActor>();
        }
        
        /// <summary>
        /// 检查条件是否满足
        /// </summary>
        /// <returns>true表示条件满足，可以触发buff</returns>
        protected virtual bool CheckCondition()
        {
            // 如果没有条件或条件类型为Always，总是满足
            if (m_conditionType == ETriggerType.Immediate || m_condition == null)
                return true;
            
            // 如果条件实例不存在，尝试创建
            if (m_condition == null && m_targetActor != null)
            {
                m_condition = GameLogic.BuffFactory.CreateCondition(
                    m_conditionType,
                    m_targetActor,
                    m_conditionParams,
                    m_attackerNumeric,
                    m_attackerNumeric?.Actor,
                    StatusId
                );
            }
            
            // 如果条件实例仍然不存在，返回false
            if (m_condition == null)
                return false;
            
            // 检查条件
            return m_condition.Check();
        }

        public virtual void OnStart(GameActor targetActor = null, NumericComponent attackerNumeric = null, GameActor attackerActor = null)
        {
            m_targetActor = targetActor;
            m_attackerNumeric = attackerNumeric;
            m_lastTickTime = 0f;
            m_hasTriggeredImmediate = false;
            
            // 更新所有Effect的targetActor和attackerNumeric
            if (m_effects != null)
            {
                foreach (var effect in m_effects)
                {
                    if (effect != null)
                    {
                        if (targetActor != null)
                        {
                            effect.SetTargetActor(targetActor);
                        }
                        effect.SetAttacker(attackerNumeric, attackerActor);
                    }
                }
            }
            
            // 立即触发类型，在开始时执行一次
            if (TriggerType == ETriggerType.Immediate)
            {
                TriggerEffect();
                m_hasTriggeredImmediate = true;
            }
        }
        
        /// <summary>
        /// 设置攻击者的数值组件（用于伤害计算）
        /// </summary>
        public void SetAttackerNumeric(NumericComponent attackerNumeric)
        {
            m_attackerNumeric = attackerNumeric;
        }

        public virtual void OnUpdate(float deltaTime)
        {
            // 检查是否过期
            if (Duration > 0 && deltaTime >= Duration)
            {
                IsExpired = true;
                OnBuffExpired?.Invoke();
                return;
            }
            
            // 间隔触发
            if (TriggerType == ETriggerType.Interval && TickInterval > 0)
            {
                if (deltaTime - m_lastTickTime >= TickInterval)
                {
                    TriggerEffect();
                    m_lastTickTime = deltaTime;
                }
            }
            // 概率触发（每帧检查，实际使用中可能需要优化）
            else if (TriggerType == ETriggerType.Probability && Probability > 0)
            {
                // 概率触发通常在特定事件时调用，这里不做每帧检查
            }
        }

        public virtual void OnEnd()
        {
            // 恢复所有Status类型的Effect被禁用的组件
            if (m_effects != null)
            {
                foreach (var effect in m_effects)
                {
                    if (effect is StatusEffect statusEffect)
                    {
                        statusEffect.RestoreComponents();
                    }
                }
            }
        }

        public bool CheckExpired()
        {
            return IsExpired;
        }
        
        // 触发效果（使用Effect策略模式）
        protected virtual void TriggerEffect()
        {
            if (m_targetActor == null || m_effects == null || m_effects.Count == 0) return;
            
            // 检查条件是否满足
            if (!CheckCondition())
            {
                Log.Info($"Buff {Id} 条件不满足，跳过触发");
                return;
            }
            
            // 遍历所有Effect并应用
            foreach (var effect in m_effects)
            {
                if (effect != null)
                {
                    effect.Apply();
                }
            }
        }
        
        /// <summary>
        /// 公开的触发效果方法（用于外部调用，如死亡触发）
        /// </summary>
        public void TriggerEffectPublic()
        {
            TriggerEffect();
        }
        
        // 尝试概率触发
        public virtual bool TryProbabilityTrigger()
        {
            if (TriggerType != ETriggerType.Probability) return false;
            if (Probability <= 0) return false;
            
            float random = UnityEngine.Random.Range(0f, 1f);
            if (random <= Probability)
            {
                TriggerEffect();
                return true;
            }
            return false;
        }
    }
}
