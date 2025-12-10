using System.Collections.Generic;
using System.Linq;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 阵营辅助类，用于判断敌对关系和查找敌对单位
    /// </summary>
    public static class FactionHelper
    {
        /// <summary>
        /// 判断两个单位是否是敌对关系
        /// </summary>
        /// <param name="actor1">单位1</param>
        /// <param name="actor2">单位2</param>
        /// <returns>true 表示敌对，false 表示非敌对</returns>
        public static bool IsEnemy(GameActor actor1, GameActor actor2)
        {
            if (actor1 == null || actor2 == null || actor1.IsDestroyed || actor2.IsDestroyed)
                return false;
            
            return IsEnemy(actor1.Tag, actor2.Tag);
        }
        
        /// <summary>
        /// 判断两个标签是否是敌对关系
        /// </summary>
        /// <param name="tag1">标签1</param>
        /// <param name="tag2">标签2</param>
        /// <returns>true 表示敌对，false 表示非敌对</returns>
        public static bool IsEnemy(UnitTag tag1, UnitTag tag2)
        {
            // 相同标签不是敌对
            if (tag1 == tag2)
                return false;
            
            // 子弹不是敌对关系判断的对象
            if (tag1 == UnitTag.Bullet || tag2 == UnitTag.Bullet)
                return false;
            
            // 友方阵营：Player, Tower, Base
            bool isFaction1 = IsFriendlyFaction(tag1);
            bool isFaction2 = IsFriendlyFaction(tag2);
            
            // 如果一个是友方，一个是敌方，则是敌对
            return isFaction1 != isFaction2;
        }
        
        /// <summary>
        /// 判断标签是否属于友方阵营（Player, Tower, Base）
        /// </summary>
        /// <param name="tag">单位标签</param>
        /// <returns>true 表示友方，false 表示敌方或其他</returns>
        public static bool IsFriendlyFaction(UnitTag tag)
        {
            return tag == UnitTag.Player || tag == UnitTag.Tower || tag == UnitTag.Base;
        }
        
        /// <summary>
        /// 判断标签是否属于敌方阵营（Enemy）
        /// </summary>
        /// <param name="tag">单位标签</param>
        /// <returns>true 表示敌方，false 表示友方或其他</returns>
        public static bool IsEnemyFaction(UnitTag tag)
        {
            return tag == UnitTag.Enemy;
        }
        
        /// <summary>
        /// 获取指定单位的敌对标签列表
        /// </summary>
        /// <param name="tag">单位标签</param>
        /// <returns>敌对标签列表</returns>
        public static List<UnitTag> GetEnemyTags(UnitTag tag)
        {
            List<UnitTag> enemyTags = new List<UnitTag>();
            
            if (IsFriendlyFaction(tag))
            {
                // 友方（Player, Tower, Base）的敌对是 Enemy
                enemyTags.Add(UnitTag.Enemy);
            }
            else if (tag == UnitTag.Enemy)
            {
                // Enemy 的敌对是 Player 和 Base
                enemyTags.Add(UnitTag.Player);
                enemyTags.Add(UnitTag.Base);
            }
            
            return enemyTags;
        }
        
        /// <summary>
        /// 判断目标是否是攻击者的敌对单位
        /// </summary>
        /// <param name="attacker">攻击者</param>
        /// <param name="target">目标</param>
        /// <returns>true 表示是敌对单位</returns>
        public static bool IsEnemyTarget(GameActor attacker, GameActor target)
        {
            if (attacker == null || target == null || attacker.IsDestroyed || target.IsDestroyed)
                return false;
            
            return IsEnemy(attacker.Tag, target.Tag);
        }
        
        /// <summary>
        /// 过滤出指定单位的敌对单位列表
        /// </summary>
        /// <param name="actor">参考单位</param>
        /// <param name="candidates">候选单位列表</param>
        /// <returns>敌对单位列表</returns>
        public static List<GameActor> FilterEnemies(GameActor actor, IEnumerable<GameActor> candidates)
        {
            if (actor == null || candidates == null)
                return new List<GameActor>();
            
            return candidates
                .Where(candidate => candidate != null && !candidate.IsDestroyed && IsEnemy(actor, candidate))
                .ToList();
        }
        
        /// <summary>
        /// 过滤出指定标签的敌对单位列表
        /// </summary>
        /// <param name="tag">参考标签</param>
        /// <param name="candidates">候选单位列表</param>
        /// <returns>敌对单位列表</returns>
        public static List<GameActor> FilterEnemies(UnitTag tag, IEnumerable<GameActor> candidates)
        {
            if (candidates == null)
                return new List<GameActor>();
            
            List<UnitTag> enemyTags = GetEnemyTags(tag);
            if (enemyTags.Count == 0)
                return new List<GameActor>();
            
            return candidates
                .Where(candidate => candidate != null && !candidate.IsDestroyed && enemyTags.Contains(candidate.Tag))
                .ToList();
        }
    }
}

