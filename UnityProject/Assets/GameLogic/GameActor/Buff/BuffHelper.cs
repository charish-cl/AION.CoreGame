using System.Collections.Generic;
using System.Linq;
using GameConfig;
using GameLogic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// Buff工具类，提供Buff相关的辅助方法
    /// </summary>
    public static class BuffHelper
    {
        /// <summary>
        /// 查找目标
        /// </summary>
        /// <param name="center">搜索中心位置</param>
        /// <param name="radius">搜索半径</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="count">目标数量</param>
        /// <returns>找到的目标列表</returns>
        public static List<GameActor> FindTargets(Vector2 center, float radius, ETargetType targetType, int count)
        {
            List<GameActor> targets = new List<GameActor>();
            
            if (count <= 0)
                return targets;

            // 根据目标类型查找
            UnitTag targetTag = targetType == ETargetType.Enemy ? UnitTag.Enemy : UnitTag.Player;
            
            // 获取所有符合条件的Actor
            var candidates = ActorMgr.Instance.Actors
                .Where(actor => 
                    actor.Tag == targetTag && 
                    !actor.IsDestroyed &&
                    Vector2.Distance(center, actor.Position) <= radius)
                .OrderBy(actor => Vector2.Distance(center, actor.Position)) // 按距离排序
                .Take(count)
                .ToList();

            targets.AddRange(candidates);
            
            return targets;
        }
        
        /// <summary>
        /// 查找目标（使用Actor作为搜索中心）
        /// </summary>
        /// <param name="centerActor">搜索中心Actor</param>
        /// <param name="radius">搜索半径</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="count">目标数量</param>
        /// <returns>找到的目标列表</returns>
        public static List<GameActor> FindTargets(GameActor centerActor, float radius, ETargetType targetType, int count)
        {
            if (centerActor == null)
                return new List<GameActor>();
            
            return FindTargets(centerActor.Position, radius, targetType, count);
        }
    }
}

