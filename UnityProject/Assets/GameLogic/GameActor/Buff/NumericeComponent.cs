using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    using System.Collections.Generic;

    public enum NumericType
    {
        Max = 10000,

        Speed = 1000,
        SpeedBase = Speed * 10 + 1,
        SpeedAdd = Speed * 10 + 2,
        SpeedPct = Speed * 10 + 3,
        SpeedFinalAdd = Speed * 10 + 4,
        SpeedFinalPct = Speed * 10 + 5,

        Hp = 1001,
        HpBase = Hp * 10 + 1,

        MaxHp = 1002,
        MaxHpBase = MaxHp * 10 + 1,
        MaxHpAdd = MaxHp * 10 + 2,
        MaxHpPct = MaxHp * 10 + 3,
        MaxHpFinalAdd = MaxHp * 10 + 4,
        MaxHpFinalPct = MaxHp * 10 + 5,
        
        
        AttackSpeed = 1003,
        AttackSpeedBase = AttackSpeed * 10 + 1,
        AttackSpeedAdd = AttackSpeed * 10 + 2,
        AttackSpeedPct = AttackSpeed * 10 + 3,
        AttackSpeedFinalAdd = AttackSpeed * 10 + 4,
        AttackSpeedFinalPct = AttackSpeed * 10 + 5,
        
        Attack = 1004,
        AttackBase = Attack * 10 + 1,
        AttackAdd = Attack * 10 + 2,
        AttackPct = Attack * 10 + 3,
        AttackFinalAdd = Attack * 10 + 4,
        AttackFinalPct = Attack * 10 + 5,
        
        Defense = 1005,
        DefenseBase = Defense * 10 + 1,
        DefenseAdd = Defense * 10 + 2,
        DefensePct = Defense * 10 + 3,
        DefenseFinalAdd = Defense * 10 + 4,
        DefenseFinalPct = Defense * 10 + 5,
        
        //法术攻击
        MagicAttack = 1019,
        MagicAttackBase = MagicAttack * 10 + 1,
        MagicAttackAdd = MagicAttack * 10 + 2,
        MagicAttackPct = MagicAttack * 10 + 3,
        MagicAttackFinalAdd = MagicAttack * 10 + 4,
        MagicAttackFinalPct = MagicAttack * 10 + 5,
        
        //法术防御
        MagicDefense = 1020,
        MagicDefenseBase = MagicDefense * 10 + 1,
        MagicDefenseAdd = MagicDefense * 10 + 2,
        MagicDefensePct = MagicDefense * 10 + 3,
        MagicDefenseFinalAdd = MagicDefense * 10 + 4,
        MagicDefenseFinalPct = MagicDefense * 10 + 5,
        
        //护甲
        Armor = 1006,
        
        //暴击
        Critical = 1007,
        CriticalBase = Critical * 10 + 1,
        CriticalAdd = Critical * 10 + 2,
        CriticalPct = Critical * 10 + 3,
        CriticalFinalAdd = Critical * 10 + 4,
        CriticalFinalPct = Critical * 10 + 5,
        
        //暴击率（0-1之间，表示暴击概率）
        CritRate = 1021,
        CritRateBase = CritRate * 10 + 1,
        CritRateAdd = CritRate * 10 + 2,
        CritRatePct = CritRate * 10 + 3,
        CritRateFinalAdd = CritRate * 10 + 4,
        CritRateFinalPct = CritRate * 10 + 5,
        
        //暴击伤害（暴击时的伤害倍率，如1.5表示150%伤害）
        CritDmg = 1022,
        CritDmgBase = CritDmg * 10 + 1,
        CritDmgAdd = CritDmg * 10 + 2,
        CritDmgPct = CritDmg * 10 + 3,
        CritDmgFinalAdd = CritDmg * 10 + 4,
        CritDmgFinalPct = CritDmg * 10 + 5,
        
        //反伤比例（0-1之间，表示反弹所受伤害的比例）
        ReflectDamage = 1008,
        ReflectDamageBase = ReflectDamage * 10 + 1,
        ReflectDamageAdd = ReflectDamage * 10 + 2,
        ReflectDamagePct = ReflectDamage * 10 + 3,
        
        // ========== 五大元素伤害 ==========
        // 火元素伤害
        FireDamage = 1009,
        FireDamageBase = FireDamage * 10 + 1,
        FireDamageAdd = FireDamage * 10 + 2,
        FireDamagePct = FireDamage * 10 + 3,
        
        // 水元素伤害
        WaterDamage = 1010,
        WaterDamageBase = WaterDamage * 10 + 1,
        WaterDamageAdd = WaterDamage * 10 + 2,
        WaterDamagePct = WaterDamage * 10 + 3,
        
        // 土元素伤害
        EarthDamage = 1011,
        EarthDamageBase = EarthDamage * 10 + 1,
        EarthDamageAdd = EarthDamage * 10 + 2,
        EarthDamagePct = EarthDamage * 10 + 3,
        
        // 风元素伤害
        WindDamage = 1012,
        WindDamageBase = WindDamage * 10 + 1,
        WindDamageAdd = WindDamage * 10 + 2,
        WindDamagePct = WindDamage * 10 + 3,
        
        // 雷元素伤害
        LightningDamage = 1013,
        LightningDamageBase = LightningDamage * 10 + 1,
        LightningDamageAdd = LightningDamage * 10 + 2,
        LightningDamagePct = LightningDamage * 10 + 3,
        
        // ========== 五大元素抗性（0-1之间，表示减免比例） ==========
        // 火元素抗性
        FireResistance = 1014,
        FireResistanceBase = FireResistance * 10 + 1,
        FireResistanceAdd = FireResistance * 10 + 2,
        FireResistancePct = FireResistance * 10 + 3,
        
        // 水元素抗性
        WaterResistance = 1015,
        WaterResistanceBase = WaterResistance * 10 + 1,
        WaterResistanceAdd = WaterResistance * 10 + 2,
        WaterResistancePct = WaterResistance * 10 + 3,
        
        // 土元素抗性
        EarthResistance = 1016,
        EarthResistanceBase = EarthResistance * 10 + 1,
        EarthResistanceAdd = EarthResistance * 10 + 2,
        EarthResistancePct = EarthResistance * 10 + 3,
        
        // 风元素抗性
        WindResistance = 1017,
        WindResistanceBase = WindResistance * 10 + 1,
        WindResistanceAdd = WindResistance * 10 + 2,
        WindResistancePct = WindResistance * 10 + 3,
        
        // 雷元素抗性
        LightningResistance = 1018,
        LightningResistanceBase = LightningResistance * 10 + 1,
        LightningResistanceAdd = LightningResistance * 10 + 2,
        LightningResistancePct = LightningResistance * 10 + 3,
    }

    public class NumericComponent : GameActorCmp
    {
        public readonly Dictionary<int, int> NumericDic = new Dictionary<int, int>();

        //这里初始化基础数值
        public override void OnInit()
        {
            base.OnInit();
        }
        
        public float GetAsFloat(NumericType numericType)
        {
            return (float)GetByKey((int)numericType) / 10000;
        }

        public void Set(NumericType nt, float value)
        {
            this[nt] = (int)(value * 10000); // 通过这种方式模拟float
        }
    
        
        public int GetAsInt(NumericType numericType)
        {
            return GetByKey((int)numericType);
        }
        public void Set(NumericType nt, int value)
        {
            this[nt] = value;
        }

        public int this[NumericType numericType]
        {
            get { return this.GetByKey((int)numericType); }
            private set
            {
                int v = this.GetByKey((int)numericType);
                if (v == value)
                {
                    return;
                }

                NumericDic[(int)numericType] = value;

                //这里应该更新
                Update(numericType);
            }
        }

 

        private int GetByKey(int key)
        {
            int value = 0;
            this.NumericDic.TryGetValue(key, out value);
            return value;
        }

        //每次数值改变的时候调用下这个函数
        public void Update(NumericType numericType)
        {
            //不能直接传Speed 1000 这种值，要传SpeedBase 10001 这种值
            if (numericType < NumericType.Max)
            {
                Log.Error($"NumericType error {numericType} 不能小于 {NumericType.Max}");
            	return;
            }
            // eg : 10021/10 = 1002  这里传进的偏移值最终都会定位到1002这个数值
            int final = (int)numericType / 10;
            int bas = final * 10 + 1;
            int add = final * 10 + 2;
            int pct = final * 10 + 3;
            int finalAdd = final * 10 + 4;
            int finalPct = final * 10 + 5;

            float preValue = GetByKey(final);
            
            // 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
            // final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
            this.NumericDic[final] =
                ((this.GetByKey(bas) + this.GetByKey(add)) * (100 + this.GetByKey(pct)) / 100 +
                 this.GetByKey(finalAdd)) * (100 + this.GetByKey(finalPct)) / 100;


            if (!Mathf.Approximately(preValue, GetAsInt((NumericType)final)))
            {
                Log.Info("NumericComponent Update {0} {1} {2}", (NumericType)final, preValue, GetAsInt(numericType));
            }
            //这个不行ActorEvent没有Get方法，后面看看能不能封装个接口出来
            // GameEvent.Get<IActorEvent_Gen>().NumbericChange((NumericType)final, preValue,  (float)GetAsInt(numericType));
            Actor.EventDispatcher.SendEvent(IActorEvent_Event.NumbericChange, (NumericType)final, preValue,  (float)GetAsInt(numericType));
            // GameEvent.Get<INumeric>().NumericChange(this.Entity.Id, (NumericType)final, preValue, GetAsInt(numericType));
            // Game.EventSystem.Run(EventIdType.NumbericChange, this.Entity.Id, numericType, final);
        }
    }
}
