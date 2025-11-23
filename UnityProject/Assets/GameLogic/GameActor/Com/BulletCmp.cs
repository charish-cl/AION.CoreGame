using System.Collections.Generic;
using UnityEngine;
using AION.CoreFramework;
using AION.Config;
using GameConfig;
using GameConfig.battle;

namespace GameLogic
{
    // 攻击模式
    public enum AttackMode
    {
        Ranged,  // 远程：子弹飞行
        Melee    // 近战：扇形检测
    }
    
    public class BulletCmp : MoveLogicCmp
    {
        public Vector2 m_target;
        
        // 攻击模式
        public AttackMode AttackMode { get; set; } = AttackMode.Ranged;
        
        // 初始位置，用于计算飞行距离
        private Vector2 m_startPosition;
        
        // 生命周期计时器
        private float m_lifetime = 0f;
        
        // 碰撞检测组件引用
        private CollisionDetectCmp m_collisionDetectCmp;
        
        // ========== 近战扇形检测参数 ==========
        // 扇形中心位置（通常是发射者的位置）
        public Vector2 MeleeCenterPosition { get; set; }
        
        // 扇形方向（从中心指向目标的方向）
        public Vector2 MeleeDirection { get; set; }
        
        // 是否已执行近战检测（近战模式只检测一次）
        private bool m_meleeDetected = false;
        
        public void Init(Vector2 target)
        {
            m_target = target;
        }
        
        // 初始化近战模式
        public void InitMelee(Vector2 centerPosition, Vector2 direction, float radius = 2f, float angle = 90f)
        {
            AttackMode = AttackMode.Melee;
            MeleeCenterPosition = centerPosition;
            MeleeDirection = direction.normalized;
            m_target = centerPosition; // 近战模式不需要目标位置
            
            // 使用 NumericComponent 存储近战参数
            if (Actor.NumericComponent != null)
            {
                Actor.NumericComponent.Set(NumericType.BulletMeleeRadius, radius);
                Actor.NumericComponent.Set(NumericType.BulletMeleeAngle, angle);
            }
        }
        
        // 获取属性（从 NumericComponent）
        private float GetBulletProperty(NumericType type, float defaultValue)
        {
            if (Actor.NumericComponent != null)
            {
                return Actor.NumericComponent.Get<float>(type);
            }
            return defaultValue;
        }

        public override void OnInit()
        {
            base.OnInit();
            m_startPosition = Position;
            m_lifetime = 0f;
            m_meleeDetected = false;
            
            // 获取碰撞检测组件（应该在创建 Actor 时就已经添加）
            m_collisionDetectCmp = Actor.GetComponent<CollisionDetectCmp>();
            if (m_collisionDetectCmp == null)
            {
                Log.Warning("BulletCmp: CollisionDetectCmp 组件不存在，请在创建 Actor 时添加");
                return;
            }
            
            // 设置碰撞检测回调
            m_collisionDetectCmp.OnCollisionDetected = OnHitTarget;
            
            // 近战模式：设置位置为中心位置，不需要速度
            if (AttackMode == AttackMode.Melee)
            {
                Position = MeleeCenterPosition;
                Velocity = 0f;
                // 立即执行一次检测
                PerformMeleeDetection();
                // 近战模式检测后立即销毁
                DestroyBullet();
                return;
            }
            
            // 远程模式：从 NumericComponent 获取速度
            if (Actor.NumericComponent != null)
            {
                Velocity = Actor.GetProperty(NumericType.BulletMoveSpeed);
                if (Velocity <= 0)
                {
                    Velocity = 5f; // 默认速度
                }
            }
            else
            {
                Velocity = 5f; // 默认速度
            }
            Log.Info($"BulletCmp: 子弹初始化完成，速度={Velocity}");
        }

        public override void OnUpdate()
        {
            // 近战模式已经在 OnInit 中处理，这里只处理远程模式
            if (AttackMode == AttackMode.Melee)
            {
                return;
            }
            
            // 更新生命周期
            m_lifetime += Time.deltaTime;
            
            // 检查生命周期限制（从 NumericComponent 获取）
            float maxLifetime = GetBulletProperty(NumericType.BulletMaxLifetime, 10f);
            if (maxLifetime > 0 && m_lifetime >= maxLifetime)
            {
                DestroyBullet();
                return;
            }
            
            // 检查最大射程限制（从 NumericComponent 获取）
            float maxRange = GetBulletProperty(NumericType.BulletMaxRange, 0f);
            if (maxRange > 0)
            {
                float distanceTraveled = Vector2.Distance(Position, m_startPosition);
                if (distanceTraveled >= maxRange)
                {
                    DestroyBullet();
                    return;
                }
            }
            
            // 计算到目标的方向
            Vector2 directionToTarget = m_target - Position;
            float distanceToTarget = directionToTarget.magnitude;
            
            // 检查是否到达目标位置（从 NumericComponent 获取阈值）
            float arrivalThreshold = GetBulletProperty(NumericType.BulletArrivalThreshold, 0.1f);
            if (distanceToTarget <= arrivalThreshold)
            {
                DestroyBullet();
                return;
            }
            
            // 移动子弹：先归一化方向，确保匀速移动
            Vector2 normalizedDirection = directionToTarget.normalized;
            Move(normalizedDirection);
            
            // 碰撞检测由CollisionDetectCmp处理，这里不需要手动检测
        }
        
        /// <summary>
        /// 碰撞检测回调：当检测到碰撞时调用
        /// </summary>
        private void OnHitTarget(GameActor target)
        {
            if (target == null || target.IsDestroyed)
                return;
            
            // 通过Buff来处理伤害逻辑
            ApplyBulletEffect(target);
            
            // 检查是否需要销毁子弹（由碰撞检测组件处理穿透逻辑）
            if (m_collisionDetectCmp != null)
            {
                bool shouldDestroy = !m_collisionDetectCmp.IsPenetrating || 
                    (m_collisionDetectCmp.MaxPenetrationCount > 0 && 
                     m_collisionDetectCmp.PenetrationCount >= m_collisionDetectCmp.MaxPenetrationCount);
                
                if (shouldDestroy)
                {
                    DestroyBullet();
                }
            }
            else
            {
                // 如果没有碰撞检测组件，直接销毁
                DestroyBullet();
            }
        }
        
        /// <summary>
        /// 应用子弹效果（使用CDamageEffect处理伤害，Buff作为额外效果）
        /// </summary>
        private void ApplyBulletEffect(GameActor target)
        {
            // 从 Actor 的配置获取子弹配置
            var bulletConfig = Actor.GetConfig<BulletConfig>();
            if (bulletConfig == null)
            {
                Log.Warning("BulletCmp: 子弹配置为空");
                return;
            }
            
            // 获取真正的攻击者（发射子弹的单位）
            var bulletActor = Actor as BulletActor;
            GameActor realAttacker = bulletActor?.RealAttacker;
            if (realAttacker == null)
            {
                Log.Warning("BulletCmp: 无法获取真正的攻击者");
                return;
            }
            
            // 获取攻击者的数值组件
            var attackerNumeric = realAttacker.GetComponent<NumericComponent>();
            if (attackerNumeric == null)
            {
                Log.Warning("BulletCmp: 攻击者没有NumericComponent");
                return;
            }
            
            // 打印子弹命中日志
            string bulletName = GetActorDisplayNameForLog(Actor);
            string targetName = GetActorDisplayNameForLog(target);
            string attackerName = GetActorDisplayNameForLog(realAttacker);
            Log.Info($"[子弹命中] {bulletName} → 命中 {targetName} (攻击者: {attackerName})");
            
            // 1. 使用 CDamageEffect 处理基础伤害（不走Buff系统）
            if (bulletConfig.Damages != null)
            {
                var healthCmp = target.GetComponent<HealthCmp>();
                if (healthCmp != null)
                {
                    // 直接传递 CDamageEffect，Value 是具体伤害数值
                    healthCmp.TakeDamage(bulletConfig.Damages, realAttacker);
                }
            }
            
            // 2. 如果 Buffs 不为0，额外添加Buff效果
            if (bulletConfig.Buffs_Ref != null && bulletConfig.Buffs > 0)
            {
                BuffFactory.CreateAndAddBuff(bulletConfig.Buffs_Ref, target, attackerNumeric, realAttacker);
            }
            else
            {
                // 否则，打印日志
                Log.Info($"[子弹命中] buffs = 0 → 无额外效果");
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
        
        
        // 执行近战扇形检测
        private void PerformMeleeDetection()
        {
            if (m_meleeDetected)
                return;
                
            m_meleeDetected = true;
            
            // 从 NumericComponent 获取近战参数
            float radius = GetBulletProperty(NumericType.BulletMeleeRadius, 2f);
            float angle = GetBulletProperty(NumericType.BulletMeleeAngle, 90f);
            
            // 获取所有在扇形范围内的敌人（使用 ActorMgr 的方法）
            List<GameActor> hitMonsters = ActorMgr.Instance.GetMonstersInSector(
                MeleeCenterPosition, 
                MeleeDirection, 
                radius, 
                angle
            );
            
            // 对所有命中的敌人应用子弹效果（通过Buff）
            foreach (var monster in hitMonsters)
            {
                ApplyBulletEffect(monster);
            }
        }
        
        private void DestroyBullet()
        {
            Enable = false;
            Actor.Destroy();
        }
    }
}
