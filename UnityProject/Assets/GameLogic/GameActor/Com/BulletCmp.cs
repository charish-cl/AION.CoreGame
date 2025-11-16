using System.Collections.Generic;
using UnityEngine;
using AION.CoreFramework;
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
        
        // 最大射程（如果为0或负数则不限制）
        public float MaxRange { get; set; } = 0f;
        
        // 最大生命周期（秒），超过后自动销毁（如果为0或负数则不限制）
        public float MaxLifetime { get; set; } = 10f;
        
        // 到达目标的距离阈值
        public float ArrivalThreshold { get; set; } = 0.1f;
        
        // 生命周期计时器
        private float m_lifetime = 0f;
        
        // 碰撞检测组件引用
        private CollisionDetectCmp m_collisionDetectCmp;
        
        // ========== 近战扇形检测参数 ==========
        // 扇形中心位置（通常是发射者的位置）
        public Vector2 MeleeCenterPosition { get; set; }
        
        // 扇形方向（从中心指向目标的方向）
        public Vector2 MeleeDirection { get; set; }
        
        // 扇形半径
        public float MeleeRadius { get; set; } = 2f;
        
        // 扇形角度（度）
        public float MeleeAngle { get; set; } = 90f;
        
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
            MeleeRadius = radius;
            MeleeAngle = angle;
            m_target = centerPosition; // 近战模式不需要目标位置
        }

        public override void OnInit()
        {
            base.OnInit();
            m_startPosition = Position;
            m_lifetime = 0f;
            m_meleeDetected = false;
            
            // 获取或添加碰撞检测组件
            m_collisionDetectCmp = Actor.GetComponent<CollisionDetectCmp>();
            if (m_collisionDetectCmp == null)
            {
                m_collisionDetectCmp = Actor.AddComponent<CollisionDetectCmp>();
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
                Velocity = Actor.GetProperty(NumericType.Speed);
                if (Velocity <= 0)
                {
                    Velocity = 5f; // 默认速度
                }
            }
            else
            {
                Velocity = 5f; // 默认速度
            }
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
            
            // 检查生命周期限制
            if (MaxLifetime > 0 && m_lifetime >= MaxLifetime)
            {
                DestroyBullet();
                return;
            }
            
            // 检查最大射程限制
            if (MaxRange > 0)
            {
                float distanceTraveled = Vector2.Distance(Position, m_startPosition);
                if (distanceTraveled >= MaxRange)
                {
                    DestroyBullet();
                    return;
                }
            }
            
            // 计算到目标的方向
            Vector2 directionToTarget = m_target - Position;
            float distanceToTarget = directionToTarget.magnitude;
            
            // 检查是否到达目标位置
            if (distanceToTarget <= ArrivalThreshold)
            {
                DestroyBullet();
                return;
            }
            
            // 移动子弹
            Move(directionToTarget);
            
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
        /// 应用子弹效果（通过Buff）
        /// </summary>
        private void ApplyBulletEffect(GameActor target)
        {
            // 获取子弹配置
            var bulletComponent = Actor.GetComponent<BulletComponent>();
            if (bulletComponent == null || !bulletComponent.IsConfigValid)
            {
                Log.Warning("BulletCmp: 子弹配置无效，无法应用效果");
                return;
            }
            
            var bulletConfig = bulletComponent.Config;
            if (bulletConfig == null || bulletConfig.Buffs_Ref == null)
            {
                Log.Warning("BulletCmp: 子弹配置或Buff配置为空");
                return;
            }
            
            // 根据子弹类型处理不同的效果
            switch (bulletConfig.BulletType)
            {
                case EBulletType.PROJECTILE:
                    // 投射物类型：对目标施加Buff
                    ApplyBuffToTarget(target, bulletConfig.Buffs_Ref);
                    break;
                
                case EBulletType.INSTANT_AURA:
                    // 即时光环类型：立即应用Buff效果(包含近战攻击,生成单位等)
                    ApplyBuffToTarget(target, bulletConfig.Buffs_Ref);
                    break;
            }
        }
        
        /// <summary>
        /// 对目标施加Buff
        /// </summary>
        private void ApplyBuffToTarget(GameActor target, BuffConfig buffConfig)
        {
            if (target == null || buffConfig == null)
                return;
            
            // 获取目标的Buff组件
            var buffCmp = target.GetComponent<BuffCmp>();
            if (buffCmp == null)
            {
                Log.Warning($"BulletCmp: 目标 {target} 没有BuffCmp组件");
                return;
            }
            
            // 创建并添加Buff
            var buff = BuffFactory.CreateBuff(buffConfig, target);
            if (buff != null)
            {
                // 传递攻击者的数值组件（用于伤害计算）
                // 子弹的NumericComponent应该包含攻击者的攻击力等信息
                var attackerNumeric = Actor.GetComponent<NumericComponent>();
                if (attackerNumeric != null)
                {
                    buff.SetAttackerNumeric(attackerNumeric);
                }
                
                // 设置目标并启动Buff
                buff.OnStart(target, attackerNumeric);
                buffCmp.AddBuff(buff);
            }
        }
        
        // 执行近战扇形检测
        private void PerformMeleeDetection()
        {
            if (m_meleeDetected)
                return;
                
            m_meleeDetected = true;
            
            // 获取所有在扇形范围内的敌人
            List<GameActor> hitMonsters = GetMonstersInSector(
                MeleeCenterPosition, 
                MeleeDirection, 
                MeleeRadius, 
                MeleeAngle
            );
            
            // 对所有命中的敌人应用子弹效果（通过Buff）
            foreach (var monster in hitMonsters)
            {
                ApplyBulletEffect(monster);
            }
        }
        
        // 获取扇形范围内的所有敌人
        private List<GameActor> GetMonstersInSector(Vector2 center, Vector2 direction, float radius, float angle)
        {
            List<GameActor> result = new List<GameActor>();
            
            // 计算扇形的半角（度转弧度）
            float halfAngle = angle * 0.5f * Mathf.Deg2Rad;
            
            // 归一化方向向量
            Vector2 normalizedDir = direction.normalized;
            
            // 遍历所有敌人
            foreach (var actor in SceneMgr.Instance.Actors)
            {
                if (actor.Tag != UnitTag.Enemy || actor.IsDestroyed)
                    continue;
                
                Vector2 toEnemy = actor.Position - center;
                float distance = toEnemy.magnitude;
                
                // 检查距离
                if (distance > radius || distance < 0.01f)
                    continue;
                
                // 归一化到敌人的方向
                Vector2 toEnemyNormalized = toEnemy.normalized;
                
                // 计算方向向量与到敌人向量的点积（用于计算角度）
                float dot = Vector2.Dot(normalizedDir, toEnemyNormalized);
                
                // 使用点积计算角度（acos返回0到π之间的角度）
                float angleToEnemy = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f));
                
                // 检查是否在扇形角度范围内
                if (angleToEnemy <= halfAngle)
                {
                    result.Add(actor);
                }
            }
            
            return result;
        }
        
        private void DestroyBullet()
        {
            Enable = false;
            Actor.Destroy();
        }
    }
}