using System.Collections.Generic;
using AION.CoreFramework;
using UnityEngine;
using GameConfig;
using GameConfig.battle;

namespace GameLogic
{
    /// <summary>
    /// 子弹Actor
    /// </summary>
    public class BulletActor : GameActor
    {
        /// <summary>
        /// 子弹配置ID
        /// </summary>
        public int BulletId { get; private set; }
        
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector2 TargetPosition { get; private set; }
        
        /// <summary>
        /// 真正的攻击者Actor（发射子弹的单位，不是子弹本身）
        /// </summary>
        public GameActor RealAttacker { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bulletId">子弹配置ID，如果为0则使用默认配置</param>
        /// <param name="targetPosition">目标位置</param>
        public BulletActor(int bulletId = 0, Vector2 targetPosition = default)
        {
            BulletId = bulletId;
            TargetPosition = targetPosition;
        }
        
        protected override void InitConfig()
        {
            base.InitConfig();
            
            // 如果指定了bulletId，加载配置
            if (BulletId > 0 && ConfigSystem.Instance?.Tables?.TbBullet != null)
            {
                var config = ConfigSystem.Instance.Tables.TbBullet.GetOrDefault(BulletId);
                if (config != null)
                {
                    SetConfig<BulletConfig>(config);
                }
                else
                {
                    Log.Warning($"BulletActor: 未找到子弹配置，BulletId = {BulletId}");
                }
            }
        }
        
        protected override void CreateModel()
        {
            base.CreateModel();
            
            // 从配置获取模型配置并实例化
            var bulletConfig = GetConfig<BulletConfig>();
            if (bulletConfig == null)
                return;
            
            // 根据子弹类型决定是否创建模型
            switch (bulletConfig.BulletType)
            {
                case EBulletType.MELEE:
                case EBulletType.SPAWN_UNIT:
                    // 近战和生成单位类型：不创建模型，只创建空的 GameObject
                    Log.Info($"BulletActor: 子弹类型 {bulletConfig.BulletType}，不创建模型预制体");
                    break;
                    
                case EBulletType.PROJECTILE:
                default:
                    // 远程子弹：创建模型
                    if (bulletConfig.ModelId_Ref != null && bulletConfig.ModelId > 0 && !string.IsNullOrEmpty(bulletConfig.ModelId_Ref.Path))
                    {
                        InstantiateModel(bulletConfig.ModelId_Ref);
                    }
                    else
                    {
                        Log.Warning($"BulletActor: 远程子弹 {BulletId} 没有有效的模型配置");
                    }
                    break;
            }
        }
        
        protected override void BindCmp()
        {
            base.BindCmp();
            
            var bulletConfig = GetConfig<BulletConfig>();
            if (bulletConfig == null)
            {
                Log.Warning($"BulletActor: 子弹配置为空，BulletId = {BulletId}");
                return;
            }
   
            
            switch (bulletConfig.BulletType)
            {
                case EBulletType.MELEE:
                    HandleMeleeAttack(bulletConfig);
                    Destroy();
                    break;
                    
                case EBulletType.SPAWN_UNIT:
                    HandleSpawnUnit(bulletConfig);
                    Destroy();
                    break;
                    
                case EBulletType.PROJECTILE:
                default:
                    // 远程子弹由 BulletCmp 处理，不需要在这里处理
                    break;
            }
            
            // 只有远程子弹才需要 BulletCmp
            if (bulletConfig.BulletType == EBulletType.PROJECTILE)
            {
                // 基础组件
                AddComponent<NumericComponent>();
                AddComponent<CollisionDetectCmp>();
                
                // 子弹组件
                var bulletCmp = AddComponent<BulletCmp>();
                bulletCmp.Init(TargetPosition);
                
                // 可视化组件
                AddComponent<MoveViewCmp>();
                var orientationCmp = AddComponent<OrientationViewCmp>();
                orientationCmp.SetTarget(TargetPosition);
            }
            // 近战和生成单位类型不需要 BulletCmp，在 OnInit 中直接处理
        }
        
        protected override void InitializeNumericFromConfig()
        {
            base.InitializeNumericFromConfig();
            
            // 只有远程子弹才需要初始化数值
            var bulletConfig = GetConfig<BulletConfig>();
            if (bulletConfig != null && bulletConfig.BulletType == EBulletType.PROJECTILE)
            {
                if (NumericComponent != null)
                {
                    NumericComponent.Set(NumericType.BulletMoveSpeedBase, bulletConfig.Speed);
                }
            }
        }
        
   
        /// <summary>
        /// 处理近战攻击
        /// </summary>
        private void HandleMeleeAttack(BulletConfig bulletConfig)
        {
            if (RealAttacker == null)
            {
                Log.Warning("BulletActor: 无法获取攻击者，无法执行近战攻击");
                return;
            }
            
            // 计算方向
            Vector2 direction = (TargetPosition - Position).normalized;
            if (direction.magnitude < 0.01f)
            {
                direction = RealAttacker.GetForwardDirection();
            }
            
            // 使用网格系统检测正方向的 Actor
            List<GameActor> hitActors = MeleeGridSystem.Instance.DetectActorsInForwardDirection(
                Position,
                direction,
                bulletConfig.DamageRange,
                (actor) =>
                {
                    // 过滤：根据攻击者类型决定目标
                    if (RealAttacker.Tag == UnitTag.Tower || RealAttacker.Tag == UnitTag.Player)
                    {
                        return actor.Tag == UnitTag.Enemy;
                    }
                    else if (RealAttacker.Tag == UnitTag.Enemy)
                    {
                        return actor.Tag == UnitTag.Player || actor.Tag == UnitTag.Base;
                    }
                    return false;
                }
            );
            
            // 获取攻击者的数值组件
            var attackerNumeric = RealAttacker.GetComponent<NumericComponent>();
            if (attackerNumeric == null)
            {
                Log.Warning("BulletActor: 攻击者没有NumericComponent");
                return;
            }
            
            // 对所有命中的 Actor 应用伤害
            foreach (var hitActor in hitActors)
            {
                ApplyBulletDamage(hitActor, bulletConfig, attackerNumeric);
            }
            
            Log.Info($"BulletActor: 近战攻击完成，中心位置={Position}，方向={direction}，范围={bulletConfig.DamageRange}，命中={hitActors.Count}个目标");
        }
        
        /// <summary>
        /// 处理生成单位
        /// </summary>
        private void HandleSpawnUnit(BulletConfig bulletConfig)
        {
            if (RealAttacker == null)
            {
                Log.Warning("BulletActor: 无法获取攻击者，无法生成单位");
                return;
            }
            
            if (bulletConfig.SpawnUnit_Ref == null)
            {
                Log.Warning($"BulletActor: SpawnUnit_Ref 为空，SpawnUnit = {bulletConfig.SpawnUnit}");
                return;
            }
            
            // 计算生成位置（在攻击者旁边）
            Vector2 forward = RealAttacker.GetForwardDirection();
            if (forward.magnitude < 0.01f)
            {
                forward = Vector2.right;
            }
            Vector2 spawnPosition = RealAttacker.Position + forward * 0.5f;
            
            // 根据攻击者类型决定生成单位的类型
            UnitTag unitTag = UnitTag.Enemy;
            if (RealAttacker.Tag == UnitTag.Tower || RealAttacker.Tag == UnitTag.Player)
            {
                unitTag = UnitTag.Tower; // 塔和玩家生成的是友军单位
            }
            
            // 创建单位（使用 SpawnUnit_Ref 的 ID）
            var unitActor = new UnitActor(bulletConfig.SpawnUnit_Ref.Id);
            ActorMgr.Instance.CreateActorInternal(unitActor, null, unitTag, spawnPosition);
            
            Log.Info($"BulletActor: 生成单位成功，单位ID = {bulletConfig.SpawnUnit_Ref.Id}，位置 = {spawnPosition}，攻击者 = {GetActorDisplayName(RealAttacker)}");
        }
        
        /// <summary>
        /// 应用子弹伤害
        /// </summary>
        private void ApplyBulletDamage(GameActor target, BulletConfig bulletConfig, NumericComponent attackerNumeric)
        {
            if (target == null || target.IsDestroyed)
                return;
            
            // 应用伤害
            if (bulletConfig.Damages != null)
            {
                var healthCmp = target.GetComponent<HealthCmp>();
                if (healthCmp != null)
                {
                    healthCmp.TakeDamage(bulletConfig.Damages, RealAttacker);
                }
            }
            
            // 应用 Buff
            if (bulletConfig.Buffs != null && bulletConfig.Buffs.Count > 0)
            {
                BuffFactory.CreaAndAddBuffs(bulletConfig.Buffs, target, attackerNumeric, RealAttacker);
            }
        }
        
        /// <summary>
        /// 获取Actor的显示名称（用于日志）
        /// </summary>
        private string GetActorDisplayName(GameActor actor)
        {
            if (actor == null)
                return "未知";
            
            string goName = actor.m_Owner != null ? actor.m_Owner.name : "无GameObject";
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
            }
            
            if (string.IsNullOrEmpty(configName))
            {
                configName = actor.Tag.ToString();
            }
            
            return $"{goName}({configName})";
        }
    }
}

