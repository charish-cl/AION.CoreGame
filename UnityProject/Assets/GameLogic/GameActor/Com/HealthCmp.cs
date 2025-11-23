using AION.CoreFramework;
using DamageNumbersPro;
using UnityEngine;
using System.Collections.Generic;
using GameConfig;
using GameConfig.battle;

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
                var numericCmp = this.GetComponent<NumericComponent>();
                if (numericCmp != null)
                {
                    // 获取旧值（用于确保触发事件）
                    int oldHp = numericCmp.GetAsInt(NumericType.Hp);
                    
                    // 设置新值（这会触发 Update(NumericType.HpBase)，重新计算 Hp）
                    numericCmp.Set(NumericType.HpBase, value);
                    
                    // 由于 Hp 没有 Add/Pct 等派生类型，Hp = HpBase
                    // 所以 Update(NumericType.HpBase) 会计算 Hp，但可能第一次 Hp 还没有初始化
                    // 确保 Hp 被重新计算并触发事件
                    int newHp = numericCmp.GetAsInt(NumericType.Hp);
                    
                    // 如果值确实变化了，或者第一次（oldHp == 0 且 newHp != 0），确保触发事件
                    if (oldHp != newHp || (oldHp == 0 && newHp > 0))
                    {
                        // 值已经变化，Update 已经触发了事件，不需要额外操作
                    }
                    else
                    {
                        // 如果值没有变化但需要刷新（第一次），手动触发一次事件
                        // 但这种情况应该不会发生，因为 Set 会调用 Update
                    }
                }
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
            float damageBeforeMultiplier = baseDamage;
            baseDamage *= skillMultiplier;
            
            float defenseReduction = attackerAtk - baseDamage; // 防御削减的伤害

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

            // 详细日志（在调用处打印，这里只返回结果）
            
            return finalDamage;
        }
        
        /// <summary>
        /// 计算物理伤害（带详细日志）
        /// </summary>
        /// <param name="attackerNumeric">攻击者的数值组件</param>
        /// <param name="defenderNumeric">防御者的数值组件</param>
        /// <param name="isCrit">是否暴击（输出参数）</param>
        /// <param name="skillMultiplier">技能倍率</param>
        /// <returns>物理伤害值</returns>
        public static float CalculatePhysicalDamage(NumericComponent attackerNumeric, NumericComponent defenderNumeric, out bool isCrit, float skillMultiplier = 1f)
        {
            isCrit = false;
            if (attackerNumeric == null || defenderNumeric == null)
                return 0f;
            
            float attackerAtk = attackerNumeric[NumericType.Attack];
            float defenderDef = defenderNumeric[NumericType.Defense];
            float attackerCritRate = attackerNumeric.GetAsFloat(NumericType.CritRate);
            float attackerCritDmg = attackerNumeric.GetAsFloat(NumericType.CritDmg);
            
            // 计算基础伤害
            float baseDamage = Mathf.Max(0, attackerAtk - defenderDef);
            float damageAfterMultiplier = baseDamage * skillMultiplier;
            float defenseReduction = attackerAtk - baseDamage;
            
            // 判断暴击
            float randomValue = Random.Range(0f, 1f);
            float totalCritChance = Mathf.Clamp01(0.05f + attackerCritRate);
            isCrit = randomValue <= totalCritChance;
            float critMultiplier = isCrit ? (1.5f + attackerCritDmg) : 1f;
            
            // 计算最终伤害
            float finalDamage = damageAfterMultiplier * critMultiplier;
            
            // 打印详细日志
            Log.Info($"[物理伤害计算] 初始攻击力: {attackerAtk}, 目标护甲: {defenderDef}, 防御削减: {defenseReduction:F1}, " +
                    $"基础伤害: {baseDamage:F1}, 技能倍率: {skillMultiplier}, 倍率后伤害: {damageAfterMultiplier:F1}, " +
                    $"暴击: {(isCrit ? "是" : "否")}, 最终伤害: {finalDamage:F1}");
            
            return finalDamage;
        }
        
        /// <summary>
        /// 计算物理伤害（兼容旧接口）
        /// </summary>
        public static float CalculatePhysicalDamage(NumericComponent attackerNumeric, NumericComponent defenderNumeric, float skillMultiplier = 1f)
        {
            bool isCrit;
            return CalculatePhysicalDamage(attackerNumeric, defenderNumeric, out isCrit, skillMultiplier);
        }
        
        /// <summary>
        /// 计算法术伤害（带详细日志）
        /// </summary>
        /// <param name="attackerNumeric">攻击者的数值组件</param>
        /// <param name="defenderNumeric">防御者的数值组件</param>
        /// <param name="isCrit">是否暴击（输出参数）</param>
        /// <param name="skillMultiplier">技能倍率</param>
        /// <returns>法术伤害值</returns>
        public static float CalculateMagicalDamage(NumericComponent attackerNumeric, NumericComponent defenderNumeric, out bool isCrit, float skillMultiplier = 1f)
        {
            isCrit = false;
            if (attackerNumeric == null || defenderNumeric == null)
                return 0f;
            
            float attackerMagicAtk = attackerNumeric.GetAsFloat(NumericType.MagicAttack);
            float defenderMagicDef = defenderNumeric.GetAsFloat(NumericType.MagicDefense);
            float attackerCritRate = attackerNumeric.GetAsFloat(NumericType.CritRate);
            float attackerCritDmg = attackerNumeric.GetAsFloat(NumericType.CritDmg);
            
            // 计算基础伤害
            float baseDamage = Mathf.Max(0, attackerMagicAtk - defenderMagicDef);
            float damageAfterMultiplier = baseDamage * skillMultiplier;
            float defenseReduction = attackerMagicAtk - baseDamage;
            
            // 判断暴击
            float randomValue = Random.Range(0f, 1f);
            float totalCritChance = Mathf.Clamp01(0.05f + attackerCritRate);
            isCrit = randomValue <= totalCritChance;
            float critMultiplier = isCrit ? (1.5f + attackerCritDmg) : 1f;
            
            // 计算最终伤害
            float finalDamage = damageAfterMultiplier * critMultiplier;
            
            // 打印详细日志
            Log.Info($"[法术伤害计算] 初始法攻: {attackerMagicAtk}, 目标法防: {defenderMagicDef}, 防御削减: {defenseReduction:F1}, " +
                    $"基础伤害: {baseDamage:F1}, 技能倍率: {skillMultiplier}, 倍率后伤害: {damageAfterMultiplier:F1}, " +
                    $"暴击: {(isCrit ? "是" : "否")}, 最终伤害: {finalDamage:F1}");
            
            return finalDamage;
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

     
        
        /// <summary>
        /// 受到伤害（根据伤害类型分别计算，不计算总伤害）
        /// </summary>
        /// <param name="attackerNumeric">攻击者的数值组件</param>
        /// <param name="damageType">伤害类型</param>
        /// <param name="attackerActor">攻击者的Actor（用于反伤，可为null）</param>
        /// <param name="skillMultiplier">技能倍率</param>
        public void TakeDamage(NumericComponent attackerNumeric, GameConfig.EDamageType damageType, 
            GameActor attackerActor = null, float skillMultiplier = 1f)
        {
            if (attackerNumeric == null)
            {
                Log.Warning("HealthCmp.TakeDamage: attackerNumeric 为空");
                return;
            }
            
            NumericComponent self = this.GetComponent<NumericComponent>();
            
            // 获取攻击者和目标的显示名称
            string attackerName = GetActorDisplayNameForLog(attackerActor);
            string defenderName = GetActorDisplayNameForLog(Actor);
            
            // 获取防御者的护甲信息
            int defenderDef = self != null ? self.Get<int>(NumericType.Defense) : 0;
            float defenderMagicDef = self != null ? self.Get<float>(NumericType.MagicDefense) : 0f;
            
            // 根据伤害类型分别计算（不计算总伤害，分别处理）
            int physicalDamage = 0;
            int magicalDamage = 0;
            bool isPhysicalCrit = false;
            bool isMagicalCrit = false;
            
            // 1. 计算物理伤害（检查是否暴击）
            if (damageType == GameConfig.EDamageType.Physical)
            {
                physicalDamage = (int)CalculatePhysicalDamage(attackerNumeric, self, out isPhysicalCrit, skillMultiplier);
            }
            
            // 2. 计算法术伤害（检查是否暴击）
            else if (damageType == GameConfig.EDamageType.Magical)
            {
                magicalDamage = (int)CalculateMagicalDamage(attackerNumeric, self, out isMagicalCrit, skillMultiplier);
            }
            
            // 3. 计算元素伤害（考虑抗性）- 元素伤害独立于物理/法术伤害类型
            ElementDamageInfo elementDamage = CalculateElementDamage(attackerNumeric, self);
            int totalElementDamage = (int)elementDamage.TotalElementDamage;
            
            // 打印伤害总结日志（分别显示）
            string damageLog = $"[造成伤害] {attackerName} → {defenderName} | ";
            if (physicalDamage > 0)
            {
                damageLog += $"物理: {physicalDamage} (护甲: {defenderDef}, 暴击: {(isPhysicalCrit ? "是" : "否")})";
            }
            if (magicalDamage > 0)
            {
                if (physicalDamage > 0) damageLog += ", ";
                damageLog += $"法术: {magicalDamage} (法防: {defenderMagicDef:F1}, 暴击: {(isMagicalCrit ? "是" : "否")})";
            }
            if (totalElementDamage > 0)
            {
                if (physicalDamage > 0 || magicalDamage > 0) damageLog += ", ";
                damageLog += $"元素: {totalElementDamage}";
            }
            int totalDamage = physicalDamage + magicalDamage + totalElementDamage;
            damageLog += $" | 目标剩余HP: {HP - totalDamage}";
            Log.Info(damageLog);
            
            // 4. 分别应用伤害（先应用物理，再应用法术，最后应用元素）
            int oldHp = HP;
            if (physicalDamage > 0)
            {
                HP -= physicalDamage;
            }
            if (magicalDamage > 0)
            {
                HP -= magicalDamage;
            }
            if (totalElementDamage > 0)
            {
                HP -= totalElementDamage;
            }
            
            // 注意：暴击事件应该在攻击起手时由攻击者触发（在 CombatHelper.PerformAttack 中）
            // 这里不再发送暴击事件，因为动画应该在攻击时播放，而不是受击时
            
            // 保存攻击者引用（用于反伤）
            if (attackerActor != null)
            {
                m_lastAttacker = attackerActor;
            }
            
            if (HP <= 0)
            {
                Log.Info($"[单位死亡] {defenderName} 被 {attackerName} 击杀");
                // 发送死亡事件
                if (Actor.EventDispatcher != null)
                {
                    Actor.EventDispatcher.SendEvent(IActorEvent_Event.OnDeath);
                }
                //Destroy the actor.
                Actor.Destroy();
            }
            
            // 5. 显示伤害数字和详细信息（分别显示不同伤害类型）
            DisplayDamageInfo(physicalDamage, magicalDamage, elementDamage, isPhysicalCrit, isMagicalCrit);
            
            // 6. 处理反伤（基于总伤害）
            ProcessReflectDamage(totalDamage, attackerNumeric, attackerActor);
        }
        
        /// <summary>
        /// 受到伤害（使用CDamageEffect配置）
        /// </summary>
        /// <param name="damageEffect">伤害效果配置</param>
        /// <param name="attackerActor">攻击者的Actor（用于反伤，可为null）</param>
        public void TakeDamage(GameConfig.CDamageEffect damageEffect, GameActor attackerActor = null)
        {
            if (damageEffect == null)
            {
                Log.Warning("HealthCmp.TakeDamage: damageEffect 为空");
                return;
            }
            
            // CDamageEffect.Value 是具体伤害数值，直接使用
            TakeDamage(damageEffect.Value, damageEffect.Type, attackerActor);
        }
        
        /// <summary>
        /// 受到固定伤害（不计算防御、暴击等，直接造成伤害）
        /// </summary>
        /// <param name="fixedDamage">固定伤害值</param>
        /// <param name="damageType">伤害类型</param>
        /// <param name="attackerActor">攻击者的Actor（用于反伤，可为null）</param>
        public void TakeDamage(float fixedDamage, GameConfig.EDamageType damageType = GameConfig.EDamageType.Physical, GameActor attackerActor = null)
        {
            if (fixedDamage <= 0)
            {
                return;
            }
            
            NumericComponent self = this.GetComponent<NumericComponent>();
            int totalDamage = Mathf.RoundToInt(fixedDamage);
            
            // 根据伤害类型应用不同的防御减免（固定伤害也可以考虑防御）
            if (self != null)
            {
                if (damageType == GameConfig.EDamageType.Physical)
                {
                    // 物理伤害：考虑护甲减免
                    int defenderDef = self.Get<int>(NumericType.Defense);
                    totalDamage = Mathf.Max(1, totalDamage - defenderDef); // 至少造成1点伤害
                }
                else if (damageType == GameConfig.EDamageType.Magical)
                {
                    // 法术伤害：考虑法防减免
                    float defenderMagicDef = self.Get<float>(NumericType.MagicDefense);
                    totalDamage = Mathf.Max(1, totalDamage - Mathf.RoundToInt(defenderMagicDef));
                }
            }
            
            // 获取攻击者和目标的显示名称
            string attackerName = GetActorDisplayNameForLog(attackerActor);
            string defenderName = GetActorDisplayNameForLog(Actor);
            
            // 打印伤害日志
            string damageTypeStr = damageType == GameConfig.EDamageType.Physical ? "物理" 
                : (damageType == GameConfig.EDamageType.Magical ? "法术" : damageType.ToString());
            Log.Info($"[造成固定伤害] {attackerName} → {defenderName} | {damageTypeStr}固定伤害: {totalDamage} | 目标剩余HP: {HP - totalDamage}");
            
            // 应用伤害
            int oldHp = HP;
            HP -= totalDamage;
            
            // 保存攻击者引用（用于反伤）
            if (attackerActor != null)
            {
                m_lastAttacker = attackerActor;
            }
            
            if (HP <= 0)
            {
                Log.Info($"[单位死亡] {defenderName} 被 {attackerName} 击杀");
                // 发送死亡事件
                if (Actor.EventDispatcher != null)
                {
                    Actor.EventDispatcher.SendEvent(IActorEvent_Event.OnDeath);
                }
                //Destroy the actor.
                Actor.Destroy();
            }
            
            // 显示伤害数字（根据伤害类型分别显示，固定伤害不暴击）
            int physicalDamage = damageType == GameConfig.EDamageType.Physical ? totalDamage : 0;
            int magicalDamage = damageType == GameConfig.EDamageType.Magical ? totalDamage : 0;
            DisplayDamageInfo(physicalDamage, magicalDamage, new ElementDamageInfo(), false, false);
            
            // 处理反伤（基于总伤害）
            if (attackerActor != null)
            {
                ProcessReflectDamage(totalDamage, attackerActor.GetComponent<NumericComponent>(), attackerActor);
            }
        }
        
        
        /// <summary>
        /// 获取Actor的显示名称（用于日志）
        /// </summary>
        private string GetActorDisplayNameForLog(GameActor actor)
        {
            if (actor == null)
            {
                return "未知";
            }
            
            string goName = actor.m_Owner != null ? actor.m_Owner.name : "无GameObject";
            
            // 尝试获取配置名字
            string configName = "";
            var unitConfig = actor.GetConfig<UnitConfig>();
            if (unitConfig != null)
            {
                configName = unitConfig.Name;
            }
            else
            {
                var towerConfig = actor.GetConfig<TowerConfig>();
                if (towerConfig != null)
                {
                    configName = towerConfig.Name;
                }
                else
                {
                    var bulletConfig = actor.GetConfig<BulletConfig>();
                    if (bulletConfig != null)
                    {
                        configName = bulletConfig.Name;
                    }
                }
            }
            
            if (string.IsNullOrEmpty(configName))
            {
                configName = actor.Tag.ToString();
            }
            
            return $"{goName}({configName})";
        }
        
        // 伤害飘字颜色常量（参考LoL等游戏）
        private const string COLOR_PHYSICAL = "#FFB6C1";    // 浅红色（普通物理伤害）
        private const string COLOR_PHYSICAL_CRIT = "#DC143C"; // 深红色（暴击物理伤害）
        private const string COLOR_MAGICAL = "#87CEEB";    // 浅蓝色（普通法术伤害）
        private const string COLOR_MAGICAL_CRIT = "#1E90FF";  // 深蓝色（暴击法术伤害）
        private const string COLOR_FIRE = "#FF6B35";      // 橙红色（火焰伤害）
        private const string COLOR_WATER = "#4A90E2";     // 蓝色（水伤害）
        private const string COLOR_EARTH = "#8B4513";     // 棕色（土伤害）
        private const string COLOR_WIND = "#87CEEB";      // 天蓝色（风伤害）
        private const string COLOR_LIGHTNING = "#FFD700";  // 金色（雷电伤害）
        private const string COLOR_REFLECT = "#FFFFFF";    // 白色（反伤）
        
        /// <summary>
        /// 将十六进制颜色字符串转换为 Color
        /// </summary>
        private Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length == 6)
            {
                int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color(r / 255f, g / 255f, b / 255f, 1f);
            }
            return Color.white;
        }
        
        /// <summary>
        /// 显示伤害信息（分别显示不同伤害类型的飘字）
        /// </summary>
        private void DisplayDamageInfo(int physicalDamage, int magicalDamage, ElementDamageInfo elementDamage, bool isPhysicalCrit, bool isMagicalCrit)
        {
            Vector2 basePosition = Actor.Position + new Vector2(0, 0.5f);
            float offsetX = 0f;
            
            // 分别显示不同伤害类型的飘字
            if (physicalDamage > 0)
            {
                var dn = numberPrefab.Spawn(basePosition + new Vector2(offsetX, 0), physicalDamage);
                // 根据是否暴击设置颜色
                string color = isPhysicalCrit ? COLOR_PHYSICAL_CRIT : COLOR_PHYSICAL;
                dn.SetColor(HexToColor(color));
                offsetX += 0.3f; // 偏移位置，避免重叠
            }
            
            if (magicalDamage > 0)
            {
                var dn = numberPrefab.Spawn(basePosition + new Vector2(offsetX, 0), magicalDamage);
                // 根据是否暴击设置颜色
                string color = isMagicalCrit ? COLOR_MAGICAL_CRIT : COLOR_MAGICAL;
                dn.SetColor(HexToColor(color));
                offsetX += 0.3f;
            }
            
            // 元素伤害分别显示（字体小一点，位置靠下一点）
            Vector2 elementBasePosition = basePosition + new Vector2(0, -0.2f); // 靠下一点
            float elementOffsetX = 0f;
            
            if (elementDamage.FireDamage > 0)
            {
                var dn = numberPrefab.Spawn(elementBasePosition + new Vector2(elementOffsetX, 0), elementDamage.FireDamage);
                dn.SetColor(HexToColor(COLOR_FIRE));
                dn.SetScale(0.8f); // 字体小一点
                elementOffsetX += 0.3f;
            }
            
            if (elementDamage.WaterDamage > 0)
            {
                var dn = numberPrefab.Spawn(elementBasePosition + new Vector2(elementOffsetX, 0), elementDamage.WaterDamage);
                dn.SetColor(HexToColor(COLOR_WATER));
                dn.SetScale(0.8f); // 字体小一点
                elementOffsetX += 0.3f;
            }
            
            if (elementDamage.EarthDamage > 0)
            {
                var dn = numberPrefab.Spawn(elementBasePosition + new Vector2(elementOffsetX, 0), elementDamage.EarthDamage);
                dn.SetColor(HexToColor(COLOR_EARTH));
                dn.SetScale(0.8f); // 字体小一点
                elementOffsetX += 0.3f;
            }
            
            if (elementDamage.WindDamage > 0)
            {
                var dn = numberPrefab.Spawn(elementBasePosition + new Vector2(elementOffsetX, 0), elementDamage.WindDamage);
                dn.SetColor(HexToColor(COLOR_WIND));
                dn.SetScale(0.8f); // 字体小一点
                elementOffsetX += 0.3f;
            }
            
            if (elementDamage.LightningDamage > 0)
            {
                var dn = numberPrefab.Spawn(elementBasePosition + new Vector2(elementOffsetX, 0), elementDamage.LightningDamage);
                dn.SetColor(HexToColor(COLOR_LIGHTNING));
                dn.SetScale(0.8f); // 字体小一点
            }
            
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
            
            damageDetails = string.Join(", ", damageParts) + damageDetails;
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
                        
                    // 显示反弹伤害数字（白色）
                    if (attackerHealthCmp.numberPrefab != null)
                    {
                        var dn = attackerHealthCmp.numberPrefab.Spawn(attackerActor.Position + new Vector2(0, 0.5f), reflectDamage);
                        dn.SetColor(HexToColor(COLOR_REFLECT));
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
