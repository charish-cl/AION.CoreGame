using System.Collections.Generic;
using AION.Config.Buff;
using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    public class BuffCmp:GameActorCmp
    {
        private List<BaseBuff> buffs = new List<BaseBuff>();

        CharacterBuffAttribute _buffAttribute ;
        
        private Dictionary<BaseBuff, float> _buffTimers = new Dictionary<BaseBuff, float>();

        
        public void AddBuff(BaseBuff buff)
        {
            buffs.Add(buff);
            buff.OnStart();
            if (buff.Modifier!= null)
            {
                _buffAttribute.AddModifier(buff.Modifier);
            }
            _buffTimers.Add(buff, Time.realtimeSinceStartup);
            Log.Info("Buff added: " + buff.Id);
        }

        public void RemoveBuff(BaseBuff buff)
        {
            if (buffs.Contains(buff))
            {
                buffs.Remove(buff);
                buff.OnEnd();
            }
            if (buff.Modifier!= null)
            {
                _buffAttribute.RemoveModifier(buff.Modifier);
            }
            _buffTimers.Remove(buff);
            Log.Info("Buff removed: " + buff.Id);
        }

        public void Update()
        {
       
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                BaseBuff buff = buffs[i] as BaseBuff;
                
                buff.OnUpdate(Time.realtimeSinceStartup - _buffTimers[buff]);
                
                if (buff.CheckExpired())
                {
                    RemoveBuff(buff);
                }
            }
        }
        NumericComponent _numericComponent;
        public override void OnInit()
        {
            _numericComponent = GetComponent<NumericComponent>();
            _buffAttribute = new CharacterBuffAttribute(_numericComponent);
            
            
            // 模拟添加一个攻击力Buff：持续30秒，+20%攻击力
            AttributeModifier atkBuff = new AttributeModifier(ModifierType.Flat, NumericType.Speed,1, "Speed");
            BaseBuff testBuff = new BaseBuff("ATK_Buff_01", 1f, atkBuff);
            AddBuff(testBuff);
            
            //
            // // 模拟伤害计算
            // CharacterStats dummyEnemy = new GameObject("Dummy").AddComponent<CharacterStats>();
            // float damage = DamageCalculator.CalculateDamage(Attack, dummyEnemy.Defense, 1.0f, CritRate, CritDamage);
        }

        public override void OnUpdate()
        {
            Update();
        }

        public override void OnDestroy()
        {
       
        }
    }
}