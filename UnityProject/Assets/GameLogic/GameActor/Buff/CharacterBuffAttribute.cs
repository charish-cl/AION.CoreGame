namespace GameLogic
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class CharacterBuffAttribute
    {
        private Dictionary<NumericType, List<AttributeModifier>> _attributeModifiers = new Dictionary<NumericType, List<AttributeModifier>>();
        
        NumericComponent m_numericComponent;
        
        public CharacterBuffAttribute(NumericComponent numericComponent)
        {
            m_numericComponent = numericComponent;
        }
        public void AddModifier(AttributeModifier modifier)
        {
            if (!_attributeModifiers.ContainsKey(modifier.NumericType))
            {
                _attributeModifiers[modifier.NumericType] = new List<AttributeModifier>();
            }

            _attributeModifiers[modifier.NumericType].Add(modifier);
            
            CalculateFinalValue(modifier.NumericType);
        }

        public bool RemoveModifier(AttributeModifier modifier)
        {
            if (_attributeModifiers.ContainsKey(modifier.NumericType))
            {
                 _attributeModifiers[modifier.NumericType].Remove(modifier);
                 CalculateFinalValue(modifier.NumericType);
                 return true;
            }
            return false;
        }
        
        public void CalculateFinalValue(NumericType numericType )
        {
            if (!_attributeModifiers.ContainsKey(numericType))
            {
                return;
            }
            
            int final = (int)numericType;
            int bas = final * 10 + 1;
            int add = final * 10 + 2;
            int pct = final * 10 + 3;
            int finalAdd = final * 10 + 4;
            int finalPct = final * 10 + 5;
            
            
            float addValue = 0;
            float pctValue = 0;
            float finalAddValue = 0;
            float finalPctValue = 0;
            
            List<AttributeModifier> modifiers = _attributeModifiers[numericType];
            foreach (var mod in modifiers)
            {
                // 先处理所有固定加法和减法
                if (mod.Type == ModifierType.Flat)
                {
                    addValue += mod.Value;
                }
                // 再处理所有百分比加法
                else if (mod.Type == ModifierType.PercentAdd)
                {
                    pctValue += (mod.Value);
                }
                // 最后处理独立乘法（不同乘区）
                else if (mod.Type == ModifierType.PercentMult)
                {
                    finalPctValue += (mod.Value);
                }
            }
            m_numericComponent.Set((NumericType)add, addValue);
            m_numericComponent.Set((NumericType)pct, pctValue);
            m_numericComponent.Set((NumericType)finalAdd, finalAddValue);
            m_numericComponent.Set((NumericType)finalPct, finalPctValue);

            m_numericComponent.Update((NumericType)bas);
            
        }
    }

    public enum ModifierType
    {
        Flat, // 固定值，如 +10 攻击力
        PercentAdd, // 百分比加法叠加，如 +10% 攻击力（同类相加）
        PercentMult // 百分比乘法叠加，如 +10% 最终伤害（独立乘区）
    }

    [System.Serializable]
    public class AttributeModifier
    {
        public ModifierType Type;
        public NumericType NumericType;
        public float Value;
        public object Source; // 来源（如装备、技能、Buff），便于管理

        public AttributeModifier(ModifierType type, NumericType numericType, float value, object source = null)
        {
            Type = type;
            Value = value;
            Source = source;
            NumericType = numericType;
        }
    }
}