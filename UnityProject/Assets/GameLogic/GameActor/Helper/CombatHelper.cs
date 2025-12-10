using System.Collections.Generic;
using GameConfig;
using GameConfig.battle;
using UnityEngine;
using AION.CoreFramework;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace GameLogic
{
    /// <summary>
    /// 战斗工具类，提供查找目标和执行攻击的辅助方法
    /// </summary>
    public static class CombatHelper
    {
        // 攻击状态字典：记录每个 Actor 是否正在攻击
        private static Dictionary<GameActor, bool> s_attackingActors = new Dictionary<GameActor, bool>();
        
        // 攻击任务字典：记录每个 Actor 的当前攻击任务
        private static Dictionary<GameActor, CancellationTokenSource> s_attackCancellationTokens = new Dictionary<GameActor, CancellationTokenSource>();
        /// <summary>
        /// 根据攻击者类型自动查找攻击目标
        /// </summary>
        /// <param name="attacker">攻击者</param>
        /// <param name="attackRange">攻击范围</param>
        /// <returns>找到的目标，如果没找到则返回null</returns>
        public static GameActor FindAttackTarget(GameActor attacker, float attackRange)
        {
            if (attacker == null || attacker.IsDestroyed)
                return null;
            
            // 获取敌对标签列表
            List<UnitTag> enemyTags = FactionHelper.GetEnemyTags(attacker.Tag);
            if (enemyTags.Count == 0)
                return null;
            
            // 敌人优先攻击基地，如果不在范围内则攻击英雄
            if (attacker.Tag == UnitTag.Enemy)
            {
                // 优先查找基地
                if (enemyTags.Contains(UnitTag.Base) && ActorMgr.Instance.TryGetBase(out var baseActor))
                {
                    float distanceToBase = Vector2.Distance(attacker.Position, baseActor.Position);
                    if (distanceToBase <= attackRange)
                        return baseActor;
                }
                
                // 如果基地不在范围内，查找玩家
                if (enemyTags.Contains(UnitTag.Player) && ActorMgr.Instance.TryGetPlayer(attacker.Position, attackRange, out var player))
                    return player;
            }
            else
            {
                // 友方单位（Player, Tower, Base）查找敌人
                if (enemyTags.Contains(UnitTag.Enemy) && ActorMgr.Instance.TryGetEnemy(attacker.Position, attackRange, out var enemy))
                    return enemy;
            }
            
            return null;
        }
        
        /// <summary>
        /// 检查是否正在攻击（动画未播完）
        /// </summary>
        public static bool IsAttacking(GameActor attacker)
        {
            if (attacker == null || attacker.IsDestroyed)
            {
                return false;
            }
            
            return s_attackingActors.ContainsKey(attacker) && s_attackingActors[attacker];
        }
        
        /// <summary>
        /// 检查攻击动画是否正在播放（动画未播完）
        /// </summary>
        private static bool IsAttackAnimationPlaying(GameActor attacker)
        {
            if (attacker == null || attacker.IsDestroyed || attacker.Transform == null)
            {
                return false;
            }
            
            var animator = attacker.Transform.GetComponentInChildren<UnityEngine.Animator>();
            if (animator == null || animator.runtimeAnimatorController == null || animator.layerCount <= 0)
            {
                return false;
            }
            
            var wrapAnimator = new WrapAnimator(animator);
            bool isPlayingAttack = wrapAnimator.IsPlayingAnimation("Attack");
            bool isPlayingCritical = wrapAnimator.IsPlayingAnimation("Critical");
            
            if (isPlayingAttack || isPlayingCritical)
            {
                // 检查动画是否已经播放完成（归一化时间 >= 1.0）
                float normalizedTime = wrapAnimator.GetCurrentAnimationNormalizedTime();
                // 如果归一化时间 < 1.0，说明动画还在播放
                return normalizedTime < 1.0f;
            }
            
            return false;
        }
        
        /// <summary>
        /// 执行攻击（带前摇后摇）- 异步方法
        /// </summary>
        /// <param name="attacker">攻击者</param>
        /// <param name="target">目标</param>
        /// <returns>是否成功执行攻击</returns>
        public static async UniTask<bool> PerformAttackWithCastTime(GameActor attacker, GameActor target)
        {
            if (attacker == null || target == null || attacker.IsDestroyed || target.IsDestroyed)
            {
                return false;
            }
            
            // 如果正在攻击（动画未播完），不能再次攻击
            if (IsAttacking(attacker) || IsAttackAnimationPlaying(attacker))
            {
                return false;
            }
            
            // 取消之前的攻击任务（如果存在）
            if (s_attackCancellationTokens.ContainsKey(attacker))
            {
                s_attackCancellationTokens[attacker]?.Cancel();
                s_attackCancellationTokens[attacker]?.Dispose();
            }
            
            // 创建新的取消令牌
            var cts = new CancellationTokenSource();
            s_attackCancellationTokens[attacker] = cts;
            
            // 标记为正在攻击
            s_attackingActors[attacker] = true;
            
            try
            {
                // 获取子弹配置
                int bulletId = GetBulletId(attacker);
                if (bulletId == 0)
                {
                    Log.Warning($"CombatHelper.PerformAttackWithCastTime: 攻击者 {GetActorDisplayName(attacker)} 没有有效的子弹配置");
                    return false;
                }
                
                var bulletConfig = ConfigSystem.Instance?.Tables?.TbBullet?.GetOrDefault(bulletId);
                
                // 计算前摇时间
                float preCastTime = 0f;
                if (bulletConfig != null && bulletConfig.PreCastAnimTime > 0f)
                {
                    preCastTime = CalculatePreCastTime(attacker, bulletConfig.PreCastAnimTime);
                }
                else
                {
                    // 尝试从动画获取前摇时间
                    preCastTime = GetPreCastTimeFromAnimation(attacker);
                }
                
                // 前摇阶段：播放攻击动画并等待前摇时间
                if (preCastTime > 0f)
                {
                    // 发送攻击事件，播放攻击动画
                    if (attacker.EventDispatcher != null)
                    {
                        attacker.EventDispatcher.SendEvent(IActorEvent_Event.OnAttack);
                    }
                    
                    // 等待前摇时间
                    await UniTask.Delay((int)(preCastTime * 1000), cancellationToken: cts.Token);
                }
                
                // 检查是否被取消或目标已消失
                if (cts.Token.IsCancellationRequested || attacker.IsDestroyed || target.IsDestroyed)
                {
                    return false;
                }
                
                // 执行实际攻击（发射子弹）
                bool attackSuccess = PerformAttackInternal(attacker, target, bulletId);
                
                if (!attackSuccess)
                {
                    return false;
                }
                
                // 计算后摇时间
                float postCastTime = 0f;
                if (bulletConfig != null && bulletConfig.PostCastAnimTime > 0f)
                {
                    postCastTime = CalculatePostCastTime(attacker, bulletConfig.PostCastAnimTime);
                }
                else
                {
                    // 尝试从动画获取后摇时间
                    postCastTime = GetPostCastTimeFromAnimation(attacker);
                }
                
                // 后摇阶段：等待后摇时间
                if (postCastTime > 0f)
                {
                    await UniTask.Delay((int)(postCastTime * 1000), cancellationToken: cts.Token);
                }
                
                return true;
            }
            finally
            {
                // 清除攻击状态
                if (s_attackingActors.ContainsKey(attacker))
                {
                    s_attackingActors[attacker] = false;
                }
                
                // 清理取消令牌
                if (s_attackCancellationTokens.ContainsKey(attacker))
                {
                    s_attackCancellationTokens[attacker]?.Dispose();
                    s_attackCancellationTokens.Remove(attacker);
                }
            }
        }
        
        /// <summary>
        /// 从动画获取前摇时间
        /// </summary>
        private static float GetPreCastTimeFromAnimation(GameActor attacker)
        {
            float animLength = GetAttackAnimationLength(attacker);
            if (animLength > 0f)
            {
                // 使用动画时长的前30%作为前摇（参考 LoL）
                return CalculatePreCastTime(attacker, animLength * 0.3f);
            }
            return 0.2f; // 默认值
        }
        
        /// <summary>
        /// 从动画获取后摇时间
        /// </summary>
        private static float GetPostCastTimeFromAnimation(GameActor attacker)
        {
            float animLength = GetAttackAnimationLength(attacker);
            if (animLength > 0f)
            {
                // 使用动画时长的后30%作为后摇（参考 LoL）
                return CalculatePostCastTime(attacker, animLength * 0.3f);
            }
            return 0.2f; // 默认值
        }
        
        /// <summary>
        /// 获取攻击动画时长
        /// </summary>
        private static float GetAttackAnimationLength(GameActor attacker)
        {
            if (attacker == null || attacker.IsDestroyed || attacker.Transform == null)
                return 0f;
            
            var animator = attacker.Transform.GetComponentInChildren<UnityEngine.Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
                return 0f;
            
            var wrapAnimator = new WrapAnimator(animator);
            return wrapAnimator.GetAnimationLength("Attack");
        }
        
        /// <summary>
        /// 获取子弹ID
        /// </summary>
        private static int GetBulletId(GameActor attacker)
        {
            var unitConfig = attacker.GetConfig<UnitConfig>();
            if (unitConfig != null && unitConfig.BulletId > 0)
                return unitConfig.BulletId;
            
            var towerConfig = attacker.GetConfig<TowerConfig>();
            if (towerConfig != null && towerConfig.BulletId > 0)
                return towerConfig.BulletId;
            
            return 0;
        }
        
        /// <summary>
        /// 执行攻击（发射子弹）- 内部方法，不考虑前摇后摇
        /// </summary>
        private static bool PerformAttackInternal(GameActor attacker, GameActor target, int bulletId)
        {
            if (attacker == null || target == null || attacker.IsDestroyed || target.IsDestroyed)
                return false;
            
            if (ActorMgr.Instance == null)
            {
                Log.Warning("CombatHelper.PerformAttackInternal: ActorMgr未初始化");
                return false;
            }
            
            if (bulletId == 0)
            {
                Log.Warning($"CombatHelper.PerformAttackInternal: 攻击者 {GetActorDisplayName(attacker)} 没有有效的子弹配置");
                return false;
            }
            
            var attackerNumeric = attacker.GetComponent<NumericComponent>();
            if (attackerNumeric == null)
            {
                Log.Warning($"CombatHelper.PerformAttackInternal: 攻击者 {GetActorDisplayName(attacker)} 没有NumericComponent");
                return false;
            }
            
            // 判断是否暴击（在攻击起手时判断，用于播放暴击动画）
            CheckAndTriggerCrit(attacker, attackerNumeric, bulletId);
            
            // 生成子弹
            ActorMgr.Instance.SpawnBullet(attacker.Position, target.Position, bulletId, attackerNumeric, attacker, target);
            
            return true;
        }
        
        /// <summary>
        /// 检查并触发暴击事件
        /// </summary>
        private static void CheckAndTriggerCrit(GameActor attacker, NumericComponent attackerNumeric, int bulletId)
        {
            if (attacker.EventDispatcher == null || ConfigSystem.Instance?.Tables?.TbBullet == null)
                return;
            
            var bulletConfig = ConfigSystem.Instance.Tables.TbBullet.GetOrDefault(bulletId);
            if (bulletConfig?.Damages == null)
                return;
            
            float attackerCritRate = attackerNumeric.Get<float>(NumericType.CritRate);
            float randomValue = Random.Range(0f, 1f);
            float totalCritChance = Mathf.Clamp01(0.05f + attackerCritRate);
            
            if (randomValue <= totalCritChance)
            {
                attacker.EventDispatcher.SendEvent(IActorEvent_Event.OnCriticalHit);
            }
        }
        
        /// <summary>
        /// 执行攻击（发射子弹）- 立即执行，不考虑前摇（保留用于兼容）
        /// </summary>
        /// <param name="attacker">攻击者</param>
        /// <param name="target">目标</param>
        /// <returns>是否成功执行攻击</returns>
        public static UniTask<bool> PerformAttack(GameActor attacker, GameActor target)
        {
            if (attacker == null || target == null || attacker.IsDestroyed || target.IsDestroyed)
                return UniTask.FromResult(false);
            
            int bulletId = GetBulletId(attacker);
            if (bulletId == 0)
            {
                Log.Warning($"CombatHelper.PerformAttack: 攻击者 {GetActorDisplayName(attacker)} 没有有效的子弹配置");
                return UniTask.FromResult(false);
            }
            
            return PerformAttackWithCastTime(attacker, target);
        }

        /// <summary>
        /// 计算受攻速影响的前摇时间
        /// </summary>
        public static float CalculatePreCastTime(GameActor attacker, float basePreCastTime)
        {
            if (attacker == null || basePreCastTime <= 0f)
            {
                return 0f;
            }
            
            var attackerNumeric = attacker.GetComponent<NumericComponent>();
            if (attackerNumeric == null)
            {
                return basePreCastTime;
            }
            
            // 获取攻速（攻击速度，例如 1.0 表示每秒攻击1次）
            float attackSpeed = attackerNumeric.Get<float>(NumericType.AttackSpeed);
            if (attackSpeed <= 0f)
            {
                return basePreCastTime;
            }
            
            // 攻速越高，前后摇时间越短
            // 公式：实际前摇时间 = 基础前摇时间 / 攻速
            // 例如：基础前摇 0.5s，攻速 2.0，则实际前摇 = 0.5 / 2.0 = 0.25s
            float actualPreCastTime = basePreCastTime / attackSpeed;
            
            // 设置最小值，避免攻速过高导致前摇时间过短
            return Mathf.Max(actualPreCastTime, 0.05f);
        }

        /// <summary>
        /// 计算受攻速影响的后摇时间
        /// </summary>
        public static float CalculatePostCastTime(GameActor attacker, float basePostCastTime)
        {
            if (attacker == null || basePostCastTime <= 0f)
            {
                return 0f;
            }
            
            var attackerNumeric = attacker.GetComponent<NumericComponent>();
            if (attackerNumeric == null)
            {
                return basePostCastTime;
            }
            
            // 获取攻速（攻击速度，例如 1.0 表示每秒攻击1次）
            float attackSpeed = attackerNumeric.Get<float>(NumericType.AttackSpeed);
            if (attackSpeed <= 0f)
            {
                return basePostCastTime;
            }
            
            // 攻速越高，前后摇时间越短
            // 公式：实际后摇时间 = 基础后摇时间 / 攻速
            float actualPostCastTime = basePostCastTime / attackSpeed;
            
            // 设置最小值，避免攻速过高导致后摇时间过短
            return Mathf.Max(actualPostCastTime, 0.05f);
        }
        
   
        
        /// <summary>
        /// 获取Actor的显示名称（用于日志）
        /// </summary>
        private static string GetActorDisplayName(GameActor actor)
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
    }
}

