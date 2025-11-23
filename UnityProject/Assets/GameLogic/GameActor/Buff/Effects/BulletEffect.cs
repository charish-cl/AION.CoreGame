using System.Collections.Generic;
using AION.CoreFramework;
using GameLogic;
using GameConfig;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 子弹效果参数类
    /// </summary>
    public class BulletEffectParams
    {
        public int PenetrationCount = 0;      // 穿透次数
        public float ExplosionChance = 0f;     // 爆炸概率（0-1）
        public float ExplosionRadius = 0f;     // 爆炸范围
        public float ExplosionDamageRatio = 1f; // 爆炸伤害比例
        public int SplitCount = 0;             // 分裂数量
        public float SplitDamageRatio = 1f;   // 分裂伤害比例
    }

    /// <summary>
    /// 子弹效果：用于修改子弹属性（穿透、爆炸、分裂等）
    /// 注意：这个Effect作用于子弹本身，而不是目标
    /// </summary>
    [BuffEffect(EBuffType.Status)] // 暂时使用Status类型，后续可以扩展EBuffType添加Bullet类型
    public class BulletEffect : BaseEffect
    {
        public BulletEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            int statusId = 0
        ) : base(targetActor, valueParams, attackerNumeric, null, statusId)
        {
        }
        
        public BulletEffect(
            GameActor targetActor,
            List<float> valueParams,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0
        ) : base(targetActor, valueParams, attackerNumeric, attackerActor, statusId)
        {
        }

        public override void Apply()
        {
            if (TargetActor == null)
                return;

            // 获取子弹组件
            var bulletCmp = TargetActor.GetComponent<BulletCmp>();
            var collisionCmp = TargetActor.GetComponent<CollisionDetectCmp>();
            var numericCmp = TargetActor.GetComponent<NumericComponent>();
            
            if (bulletCmp == null && collisionCmp == null)
            {
                Log.Warning("BulletEffect: 目标Actor没有BulletCmp或CollisionDetectCmp组件");
                return;
            }

            // 使用GetParam自动填充参数
            var param = GetParam<BulletEffectParams>();

            // 应用穿透次数
            if (param.PenetrationCount > 0 && collisionCmp != null)
            {
                collisionCmp.IsPenetrating = true;
                collisionCmp.MaxPenetrationCount = param.PenetrationCount;
                Log.Info($"BulletEffect: 设置穿透次数 = {param.PenetrationCount}");
            }
            
            // 应用爆炸相关属性
            if (numericCmp != null)
            {
                if (param.ExplosionChance > 0)
                {
                    // 爆炸概率可以通过NumericComponent存储，或者直接在BulletComponent中处理
                    // 这里先记录到NumericComponent，后续BulletCmp可以读取
                    numericCmp.Set(NumericType.Critical, (int)(param.ExplosionChance * 10000)); // 临时使用Critical存储
                    Log.Info($"BulletEffect: 设置爆炸概率 = {param.ExplosionChance}");
                }
                
                if (param.ExplosionRadius > 0)
                {
                    // 爆炸范围可以通过NumericComponent存储
                    numericCmp.Set(NumericType.Defense, (int)(param.ExplosionRadius * 10000)); // 临时使用Defense存储
                    Log.Info($"BulletEffect: 设置爆炸范围 = {param.ExplosionRadius}");
                }
            }
            
            // 分裂相关属性可以类似处理
            if (param.SplitCount > 0)
            {
                Log.Info($"BulletEffect: 设置分裂数量 = {param.SplitCount}, 伤害比例 = {param.SplitDamageRatio}");
                // 分裂逻辑需要在BulletCmp中实现
            }
        }
    }
}

