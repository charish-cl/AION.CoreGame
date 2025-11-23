using AION.CoreFramework;
using AION.Config;
using DamageNumbersPro;
using UnityEngine;
using System.Collections.Generic;

namespace GameLogic
{
    public class HealthCmp : GameActorCmp
    {
        public int HP
        {
            get
            {
                return this.GetComponent<NumericComponent>().GetAsInt(NumericType.Hp);
            }
            set
            {
                this.GetComponent<NumericComponent>().Set(NumericType.HpBase, value);
            }
        }
        
        public int MaxHP
        {
            get
            {
                return this.GetComponent<NumericComponent>().GetAsInt(NumericType.HpBase);
            }
            set
            {
                this.GetComponent<NumericComponent>().Set(NumericType.HpBase, value);
            }
        }
        public DamageNumber numberPrefab;

        public override void OnInit()
        {
            base.OnInit();
            numberPrefab = ActorMgr.Instance.SceneBehavior.numberPrefab;
        }
        
        /// <summary>
        /// 元素伤害信息
        /// </summary>
        public struct ElementDamageInfo
        {
            public float FireDamage;
            public float WaterDamage;
            public float EarthDamage;
            public float WindDamage;
            public float LightningDamage;
            public float TotalElementDamage => FireDamage + WaterDamage + EarthDamage + WindDamage + LightningDamage;
        }
        
        public static float CalculateDamage(float attackerAtk, float defenderDef, float skillMultiplier, 
            float attackerCritRate, float attackerCritDmg, 
            float baseCritChance = 0.05f, float baseCritDmg = 1.5f)
        {
            
            //此处还可以计算miss效果
            
            // 1. 计算基础伤害 (攻击 - 防御)
            float baseDamage = Mathf.Max(0, attackerAtk - defenderDef);
            baseDamage *= skillMultiplier;

            // 2. 判断暴击
            float randomValue = Random.Range(0f, 1f);
            float totalCritChance = baseCritChance + attackerCritRate;
            totalCritChance = Mathf.Clamp01(totalCritChance); // 暴击率钳制在0-1之间
            bool isCrit = randomValue <= totalCritChance;
            float critMultiplier = isCrit ? (baseCritDmg + attackerCritDmg) : 1f;
            

            // 3. 计算最终伤害
            float finalDamage = baseDamage * critMultiplier;

            // 4. 可以继续叠加其他乘区（元素伤害、易伤等）...
            // finalDamage *= (1 + elementDmgBonus); 

            
            Log.Info($"Damage: {finalDamage}, IsCrit: {isCrit}");
            
            return finalDamage;
        }
        
        /// <summary>
        /// 计算物理伤害
        /// </summary>
        /// <param name="attackerNumeric">攻击者的数值组件</param>
        /// <param name="defenderNumeric">防御者的数值组件</param>
        /// <param name="skillMultiplier">技能倍率</param>
        /// <returns>物理伤害值</returns>
        public static float CalculatePhysicalDamage(NumericComponent attackerNumeric, NumericComponent defenderNumeric, float skillMultiplier = 1f)
        {
            if (attackerNumeric == null || defenderNumeric == null)
                return 0f;
            
            float attackerAtk = attackerNumeric[NumericType.Attack];
            float defenderDef = defenderNumeric[NumericType.Defense];
            float attackerCritRate = attackerNumeric.GetAsFloat(NumericType.CritRate);
            float attackerCritDmg = attackerNumeric.GetAsFloat(NumericType.CritDmg);
            
            return CalculateDamage(attackerAtk, defenderDef, skillMultiplier, attackerCritRate, attackerCritDmg);
        }
        
        /// <summary>
        /// 计算法术伤害
        /// </summary>
        /// <param name="attackerNumeric">攻击者的数值组件</param>
        /// <param name="defenderNumeric">防御者的数值组件</param>
        /// <param name="skillMultiplier">技能倍率</param>
        /// <returns>法术伤害值</returns>
        public static float CalculateMagicalDamage(NumericComponent attackerNumeric, NumericComponent defenderNumeric, float skillMultiplier = 1f)
        {
            if (attackerNumeric == null || defenderNumeric == null)
                return 0f;
            
            float attackerMagicAtk = attackerNumeric.GetAsFloat(NumericType.MagicAttack);
            float defenderMagicDef = defenderNumeric.GetAsFloat(NumericType.MagicDefense);
            float attackerCritRate = attackerNumeric.GetAsFloat(NumericType.CritRate);
            float attackerCritDmg = attackerNumeric.GetAsFloat(NumericType.CritDmg);
            
            return CalculateDamage(attackerMagicAtk, defenderMagicDef, skillMultiplier, attackerCritRate, attackerCritDmg);
        }
        
        /// <summary>
        /// 计算元素伤害（考虑抗性）
        /// </summary>
        /// <param name="attackerNumeric">攻击者的数值组件</param>
        /// <param name="defenderNumeric">防御者的数值组件</param>
        /// <returns>元素伤害信息</returns>
        public static ElementDamageInfo CalculateElementDamage(NumericComponent attackerNumeric, NumericComponent defenderNumeric)
        {
            ElementDamageInfo elementDamage = new ElementDamageInfo();
            
            if (attackerNumeric == null || defenderNumeric == null)
                return elementDamage;
            
            // 获取攻击者的元素伤害
            float attackerFireDamage = attackerNumeric.GetAsFloat(NumericType.FireDamage);
            float attackerWaterDamage = attackerNumeric.GetAsFloat(NumericType.WaterDamage);
            float attackerEarthDamage = attackerNumeric.GetAsFloat(NumericType.EarthDamage);
            float attackerWindDamage = attackerNumeric.GetAsFloat(NumericType.WindDamage);
            float attackerLightningDamage = attackerNumeric.GetAsFloat(NumericType.LightningDamage);
            
            // 获取防御者的元素抗性
            float defenderFireResistance = Mathf.Clamp01(defenderNumeric.GetAsFloat(NumericType.FireResistance));
            float defenderWaterResistance = Mathf.Clamp01(defenderNumeric.GetAsFloat(NumericType.WaterResistance));
            float defenderEarthResistance = Mathf.Clamp01(defenderNumeric.GetAsFloat(NumericType.EarthResistance));
            float defenderWindResistance = Mathf.Clamp01(defenderNumeric.GetAsFloat(NumericType.WindResistance));
            float defenderLightningResistance = Mathf.Clamp01(defenderNumeric.GetAsFloat(NumericType.LightningResistance));
            
            // 计算实际元素伤害（考虑抗性）
            // 实际伤害 = 元素伤害 * (1 - 抗性)
            elementDamage.FireDamage = attackerFireDamage * (1f - defenderFireResistance);
            elementDamage.WaterDamage = attackerWaterDamage * (1f - defenderWaterResistance);
            elementDamage.EarthDamage = attackerEarthDamage * (1f - defenderEarthResistance);
            elementDamage.WindDamage = attackerWindDamage * (1f - defenderWindResistance);
            elementDamage.LightningDamage = attackerLightningDamage * (1f - defenderLightningResistance);
            
            return elementDamage;
        }
        /// <summary>
        /// 存储最后一次攻击者的Actor引用（用于反伤）
        /// </summary>
        private GameActor m_lastAttacker = null;

     

        public void TakeDamage(NumericComponent attackerNumeric)
        {
            TakeDamage(attackerNumeric, null);
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="attackerNumeric">攻击者的数值组件</param>
        /// <param name="attackerActor">攻击者的Actor（用于反伤，可为null）</param>
        /// <param name="damageType">伤害类型（物理/法术/混合）</param>
        /// <param name="skillMultiplier">技能倍率</param>
        public void TakeDamage(NumericComponent attackerNumeric, GameActor attackerActor = null, 
            EnumDamageType damageType = EnumDamageType.Physical, float skillMultiplier = 1f)
        {
            NumericComponent self = this.GetComponent<NumericComponent>();
            
            // 1. 计算物理伤害
            int physicalDamage = 0;
            if (damageType.HasFlag(EnumDamageType.Physical))
            {
                physicalDamage = (int)CalculatePhysicalDamage(attackerNumeric, self, skillMultiplier);
            }
            
            // 2. 计算法术伤害
            int magicalDamage = 0;
            if (damageType.HasFlag(EnumDamageType.Magical))
            {
                magicalDamage = (int)CalculateMagicalDamage(attackerNumeric, self, skillMultiplier);
            }
            
            // 3. 计算元素伤害（考虑抗性）
            ElementDamageInfo elementDamage = CalculateElementDamage(attackerNumeric, self);
            int totalElementDamage = (int)elementDamage.TotalElementDamage;
            
            // 4. 计算总伤害 = 物理伤害 + 法术伤害 + 元素伤害
            int totalDamage = physicalDamage + magicalDamage + totalElementDamage;
            
            // 5. 应用伤害
            HP -= totalDamage;
            
            // 保存攻击者引用（用于反伤）
            if (attackerActor != null)
            {
                m_lastAttacker = attackerActor;
            }
            
            if (HP <= 0)
            {
                //Destroy the actor.
                Actor.Destroy();
            }
            
            // 6. 显示伤害数字和详细信息
            DisplayDamageInfo(physicalDamage, magicalDamage, elementDamage, totalDamage);
            
            // 7. 处理反伤（基于总伤害）
            ProcessReflectDamage(totalDamage, attackerNumeric, attackerActor);
        }
        
        /// <summary>
        /// 显示伤害信息
        /// </summary>
        private void DisplayDamageInfo(int physicalDamage, int magicalDamage, ElementDamageInfo elementDamage, int totalDamage)
        {
            // 显示伤害数字
            numberPrefab.Spawn(Actor.Position + new Vector2(0, 0.5f), totalDamage);
            
            // 构建伤害详情日志
            string damageDetails = $"伤害: ";
            List<string> damageParts = new List<string>();
            
            if (physicalDamage > 0)
                damageParts.Add($"物理={physicalDamage}");
            
            if (magicalDamage > 0)
                damageParts.Add($"法术={magicalDamage}");
            
            if (elementDamage.TotalElementDamage > 0)
            {
                damageParts.Add($"元素={(int)elementDamage.TotalElementDamage}");
                damageDetails += $" (火={elementDamage.FireDamage:F1}, 水={elementDamage.WaterDamage:F1}, 土={elementDamage.EarthDamage:F1}, 风={elementDamage.WindDamage:F1}, 雷={elementDamage.LightningDamage:F1})";
            }
            
            damageDetails = string.Join(", ", damageParts) + damageDetails + $", 总计={totalDamage}";
            Log.Info(damageDetails);
        }
        
        /// <summary>
        /// 处理反伤逻辑
        /// </summary>
        private void ProcessReflectDamage(int receivedDamage, NumericComponent attackerNumeric, GameActor attackerActor)
        {
            NumericComponent self = this.GetComponent<NumericComponent>();
            
            // 获取反伤比例（0-1之间）
            float reflectRatio = self.GetAsFloat(NumericType.ReflectDamage);
            
            if (reflectRatio > 0 && attackerActor != null && !attackerActor.IsDestroyed)
            {
                // 计算反弹的伤害 = 实际受到的伤害 * 反伤比例
                int reflectDamage = (int)(receivedDamage * reflectRatio);
                
                if (reflectDamage > 0)
                {
                    // 对攻击者造成反弹伤害
                    var attackerHealthCmp = attackerActor.GetComponent<HealthCmp>();
                    if (attackerHealthCmp != null)
                    {
                        // 使用自己的数值组件作为"攻击者"（反伤不计算防御，直接造成固定伤害）
                        // 或者可以创建一个临时的NumericComponent来传递反弹伤害
                        attackerHealthCmp.HP -= reflectDamage;
                        
                        // 显示反弹伤害数字
                        if (attackerHealthCmp.numberPrefab != null)
                        {
                            attackerHealthCmp.numberPrefab.Spawn(attackerActor.Position + new Vector2(0, 0.5f), reflectDamage);
                        }
                        
                        Log.Info($"反伤: {Actor.Tag} 反弹 {reflectDamage} 点伤害给 {attackerActor.Tag} (反伤比例: {reflectRatio * 100}%)");
                        
                        // 检查攻击者是否死亡
                        if (attackerHealthCmp.HP <= 0)
                        {
                            attackerActor.Destroy();
                        }
                    }
                }
            }
        }
    }
}
