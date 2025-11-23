using System.Collections.Generic;
using AION.CoreFramework;
using GameConfig;
using GameLogic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 伤害效果参数类
    /// </summary>
    public class DamageEffectParams
    {
        public float DamageAmount = 0f;        // 固定伤害值（如果UseAttackerDamage为false时使用）
        public bool UseAttackerDamage = false;  // 是否使用攻击者的伤害（默认true）
        public float AttackPercent = 1f;      // 攻击力百分比（0-1之间，例如0.2表示20%）
    }

    /// <summary>
    /// 伤害效果
    /// </summary>
    [BuffEffect(EBuffType.Damage)]
    public class DamageEffect : BaseEffect
    {
        public DamageEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            int statusId = 0,
            EDamageType damageType = EDamageType.Physical
        ) : base(targetActor, valueParams, attackerNumeric, null, statusId, damageType)
        {
        }
        
        public DamageEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0,
            EDamageType damageType = EDamageType.Physical
        ) : base(targetActor, valueParams, attackerNumeric, attackerActor, statusId, damageType)
        {
        }

        public override void Apply()
        {
            if (TargetActor == null)
                return;

            var healthCmp = TargetActor.GetComponent<HealthCmp>();
            if (healthCmp == null)
                return;

            // 使用GetParam自动填充参数
            var param = GetParam<DamageEffectParams>();

            // 如果有攻击者的数值组件且使用攻击者伤害
            if (param.UseAttackerDamage && AttackerNumeric != null)
            {
                // 如果指定了攻击力百分比，创建临时NumericComponent来计算伤害
                if (param.AttackPercent != 1f && param.AttackPercent > 0f)
                {
                    // 计算基于攻击力百分比的伤害
                    int attackerAttack = AttackerNumeric.GetAsInt(NumericType.Attack);
                    int damageAmount = (int)(attackerAttack * param.AttackPercent);
                    
                    // 直接造成固定伤害（不计算防御，因为是百分比伤害）
                    healthCmp.HP -= damageAmount;
                    
                    // 显示伤害数字
                    if (healthCmp.numberPrefab != null)
                    {
                        healthCmp.numberPrefab.Spawn(TargetActor.Position + new Vector2(0, 0.5f), damageAmount);
                    }
                    
                    Log.Info($"DamageEffect: 造成攻击力{param.AttackPercent * 100}%的伤害 = {damageAmount} (攻击者攻击力: {attackerAttack})");
                }
                else
                {
                    Log.Info($"DamageEffect: 使用攻击者的伤害");
                    
                    // 使用HealthCmp的TakeDamage方法，传递攻击者Actor以支持反伤（会计算防御等）
                    GameActor attacker = AttackerActor;
                    if (attacker == null && AttackerNumeric != null)
                    {
                        attacker = AttackerNumeric.Actor;
                    }
                    healthCmp.TakeDamage(AttackerNumeric, DamageType, attacker);
                }
            }
            else if (param.DamageAmount > 0)
            {
                healthCmp.TakeDamage(param.DamageAmount, DamageType, AttackerActor);
            }
        }
    }
}

