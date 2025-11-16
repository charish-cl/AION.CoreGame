using System;
using GameLogic;
using GameConfig;
using UnityEngine;

namespace AION.Config.Buff
{
    public class BaseBuff
    {
        public bool IsExpired = false;
        public string Id;
        public int BuffId; // BuffConfig的ID
        public float Duration;
        public AttributeModifier Modifier;
        public Action OnBuffExpired;
        
        // Buff配置信息
        public EBuffType BuffType { get; set; }
        public ETriggerType TriggerType { get; set; }
        public float TickInterval { get; set; }
        public float Probability { get;  set; } // 概率触发时的概率值
        public System.Collections.Generic.List<float> ValueParams { get; set; }
        public int StatusId { get; set; }
        
        // 触发相关
        protected float m_lastTickTime = 0f;
        protected bool m_hasTriggeredImmediate = false;
        protected GameActor m_targetActor; // 目标Actor
        protected NumericComponent m_attackerNumeric; // 攻击者的数值组件（用于伤害计算）
        
        /// <summary>
        /// 获取目标Actor（用于检查是否已设置）
        /// </summary>
        public GameActor TargetActor => m_targetActor;
        
        public BaseBuff(string id, float duration, AttributeModifier modifier)
        {
            Id = id;
            Duration = duration;
            Modifier = modifier;
            ValueParams = new System.Collections.Generic.List<float>();
        }

        public virtual void OnStart(GameActor targetActor = null, NumericComponent attackerNumeric = null)
        {
            m_targetActor = targetActor;
            m_attackerNumeric = attackerNumeric;
            m_lastTickTime = 0f;
            m_hasTriggeredImmediate = false;
            
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
        }

        public bool CheckExpired()
        {
            return IsExpired;
        }
        
        // 触发效果（根据BuffType执行不同效果）
        protected virtual void TriggerEffect()
        {
            if (m_targetActor == null) return;
            
            switch (BuffType)
            {
                case EBuffType.PropertyMod:
                    // 属性修改已经在Modifier中处理，这里不需要额外操作
                    break;
                    
                case EBuffType.Heal:
                    ApplyHeal();
                    break;
                    
                case EBuffType.Damage:
                    ApplyDamage();
                    break;
                    
                case EBuffType.Status:
                    ApplyStatus();
                    break;
            }
        }
        
        // 应用治疗
        protected virtual void ApplyHeal()
        {
            if (ValueParams == null || ValueParams.Count == 0) return;
            
            var healthCmp = m_targetActor.GetComponent<HealthCmp>();
            if (healthCmp != null)
            {
                float healAmount = ValueParams[0];
                int currentHp = healthCmp.HP;
                int maxHp = m_targetActor.NumericComponent.GetAsInt(NumericType.MaxHp);
                healthCmp.HP = Mathf.Min(currentHp + (int)healAmount, maxHp);
            }
        }
        
        // 应用伤害
        protected virtual void ApplyDamage()
        {
            var healthCmp = m_targetActor.GetComponent<HealthCmp>();
            if (healthCmp == null) return;
            
            // 如果有攻击者的数值组件，使用HealthCmp的TakeDamage方法（会计算防御等）
            if (m_attackerNumeric != null)
            {
                // 使用HealthCmp的TakeDamage方法，它会自动计算伤害（考虑攻击力和防御力）
                healthCmp.TakeDamage(m_attackerNumeric);
            }
            else if (ValueParams != null && ValueParams.Count > 0)
            {
                // 如果没有攻击者信息，直接使用ValueParams中的伤害值
                float damageAmount = ValueParams[0];
                healthCmp.HP -= (int)damageAmount;
                
                // 显示伤害数字
                if (healthCmp.numberPrefab != null)
                {
                    healthCmp.numberPrefab.Spawn(m_targetActor.Position + new Vector2(0, 0.5f), (int)damageAmount);
                }
            }
        }
        
        // 应用状态效果（眩晕、击退等）
        protected virtual void ApplyStatus()
        {
            // 状态效果的具体实现需要根据StatusId来判断
            // 这里先留空，后续可以根据需要扩展
            // 例如：眩晕可以禁用移动，击退可以施加位移等
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