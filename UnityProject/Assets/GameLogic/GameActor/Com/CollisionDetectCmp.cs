using System.Collections.Generic;
using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 碰撞检测组件，用于处理子弹与敌人的碰撞检测
    /// </summary>
    public class CollisionDetectCmp : GameActorCmp
    {
        /// <summary>
        /// 碰撞检测半径
        /// </summary>
        public float CollisionRadius { get; set; } = 1f;
        
        /// <summary>
        /// 碰撞回调：当检测到碰撞时调用
        /// </summary>
        public System.Action<GameActor> OnCollisionDetected;
        
        /// <summary>
        /// 已碰撞的敌人列表（用于穿透检测）
        /// </summary>
        private HashSet<GameActor> m_hitActors = new HashSet<GameActor>();
        
        /// <summary>
        /// 是否穿透（穿透多个敌人）
        /// </summary>
        public bool IsPenetrating { get; set; } = false;
        
        /// <summary>
        /// 最大穿透数量（如果为0或负数则不限制）
        /// </summary>
        public int MaxPenetrationCount { get; set; } = 0;
        
        /// <summary>
        /// 当前穿透计数
        /// </summary>
        public int PenetrationCount { get; set; } = 0;
        
        public override void OnInit()
        {
            base.OnInit();
            m_hitActors.Clear();
            PenetrationCount = 0;
        }
        
        public override void OnUpdate()
        {
            base.OnUpdate();
            
            // 检测碰撞
            if (ActorMgr.Instance.TryGetMonster(Actor.Position, CollisionRadius, out var monster))
            {
                // 检查是否已经碰撞过（用于穿透检测）
                if (m_hitActors.Contains(monster))
                {
                    return; // 已经碰撞过，跳过
                }
                
                // 检查穿透限制
                if (IsPenetrating)
                {
                    if (MaxPenetrationCount > 0 && PenetrationCount >= MaxPenetrationCount)
                    {
                        return; // 达到最大穿透次数
                    }
                    PenetrationCount++;
                }
                
                // 记录已碰撞的敌人
                m_hitActors.Add(monster);
                
                // 触发碰撞回调
                OnCollisionDetected?.Invoke(monster);
            }
        }
        
        /// <summary>
        /// 重置碰撞检测（用于穿透子弹）
        /// </summary>
        public void ResetCollision()
        {
            m_hitActors.Clear();
            PenetrationCount = 0;
        }
        
        /// <summary>
        /// 检查是否已经碰撞过指定敌人
        /// </summary>
        public bool HasHitActor(GameActor actor)
        {
            return m_hitActors.Contains(actor);
        }
    }
}

