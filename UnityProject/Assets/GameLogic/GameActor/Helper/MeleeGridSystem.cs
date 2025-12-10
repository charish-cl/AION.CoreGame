using System.Collections.Generic;
using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 近战网格系统 - 用于快速检测正方向的 Actor
    /// 每个网格可以包含多个 Actor，只检测正方向的网格
    /// </summary>
    public class MeleeGridSystem
    {
        private static MeleeGridSystem s_instance;
        public static MeleeGridSystem Instance => s_instance ??= new MeleeGridSystem();

        /// <summary>
        /// 网格大小（每个网格的边长）
        /// </summary>
        private float m_cellSize = 1f;

        /// <summary>
        /// 网格字典：网格坐标 -> Actor 列表
        /// </summary>
        private Dictionary<Vector2Int, List<GameActor>> m_gridDict = new Dictionary<Vector2Int, List<GameActor>>();

        /// <summary>
        /// Actor 到网格坐标的映射（用于快速移除）
        /// </summary>
        private Dictionary<GameActor, Vector2Int> m_actorToGrid = new Dictionary<GameActor, Vector2Int>();

        /// <summary>
        /// 初始化网格系统
        /// </summary>
        /// <param name="cellSize">网格大小</param>
        public void Initialize(float cellSize = 1f)
        {
            m_cellSize = cellSize;
            Clear();
        }

        /// <summary>
        /// 将世界坐标转换为网格坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / m_cellSize);
            int y = Mathf.FloorToInt(worldPos.y / m_cellSize);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 将网格坐标转换为世界坐标（网格中心）
        /// </summary>
        public Vector2 GridToWorld(Vector2Int gridPos)
        {
            return new Vector2(
                gridPos.x * m_cellSize + m_cellSize * 0.5f,
                gridPos.y * m_cellSize + m_cellSize * 0.5f
            );
        }

        /// <summary>
        /// 注册 Actor 到网格系统
        /// </summary>
        public void RegisterActor(GameActor actor)
        {
            if (actor == null || actor.IsDestroyed)
                return;

            // 移除旧的注册（如果存在）
            UnregisterActor(actor);

            Vector2Int gridPos = WorldToGrid(actor.Position);
            
            if (!m_gridDict.TryGetValue(gridPos, out var actorList))
            {
                actorList = new List<GameActor>();
                m_gridDict[gridPos] = actorList;
            }

            actorList.Add(actor);
            m_actorToGrid[actor] = gridPos;
        }

        /// <summary>
        /// 从网格系统移除 Actor
        /// </summary>
        public void UnregisterActor(GameActor actor)
        {
            if (actor == null || !m_actorToGrid.TryGetValue(actor, out var gridPos))
                return;

            if (m_gridDict.TryGetValue(gridPos, out var actorList))
            {
                actorList.Remove(actor);
                if (actorList.Count == 0)
                {
                    m_gridDict.Remove(gridPos);
                }
            }

            m_actorToGrid.Remove(actor);
        }

        /// <summary>
        /// 更新 Actor 的位置（如果位置变化，更新网格）
        /// </summary>
        public void UpdateActorPosition(GameActor actor)
        {
            if (actor == null || actor.IsDestroyed)
                return;

            Vector2Int newGridPos = WorldToGrid(actor.Position);
            
            if (m_actorToGrid.TryGetValue(actor, out var oldGridPos))
            {
                // 如果网格坐标没变，不需要更新
                if (oldGridPos == newGridPos)
                    return;

                // 从旧网格移除
                UnregisterActor(actor);
            }

            // 添加到新网格
            RegisterActor(actor);
        }

        /// <summary>
        /// 检测正方向的网格中的 Actor
        /// </summary>
        /// <param name="centerPos">检测中心位置</param>
        /// <param name="direction">正方向（归一化）</param>
        /// <param name="range">检测范围（网格数量）</param>
        /// <param name="filter">过滤函数，返回 true 表示包含该 Actor</param>
        /// <returns>检测到的 Actor 列表</returns>
        public List<GameActor> DetectActorsInForwardDirection(
            Vector2 centerPos,
            Vector2 direction,
            float range,
            System.Func<GameActor, bool> filter = null)
        {
            List<GameActor> result = new List<GameActor>();
            
            if (direction.magnitude < 0.01f)
            {
                Log.Warning("MeleeGridSystem: 方向向量太小，无法检测");
                return result;
            }

            direction = direction.normalized;
            Vector2Int centerGrid = WorldToGrid(centerPos);
            
            // 计算需要检测的网格范围（基于 range）
            int gridRange = Mathf.CeilToInt(range / m_cellSize);
            
            // 获取正方向的主要方向（8方向）
            Vector2Int mainDirection = GetMainDirection(direction);
            
            // 检测正方向及其相邻方向的网格
            HashSet<Vector2Int> checkedGrids = new HashSet<Vector2Int>();
            
            // 检测正方向的网格
            for (int i = 1; i <= gridRange; i++)
            {
                Vector2Int gridPos = centerGrid + mainDirection * i;
                
                // 检查相邻网格（形成扇形）
                CheckGridAndNeighbors(gridPos, centerPos, direction, range, result, checkedGrids, filter);
            }

            return result;
        }

        /// <summary>
        /// 获取主要方向（8方向）
        /// </summary>
        private Vector2Int GetMainDirection(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // 将角度转换为 0-360
            if (angle < 0) angle += 360;
            
            // 8方向：右、右上、上、左上、左、左下、下、右下
            if (angle >= 337.5f || angle < 22.5f) return new Vector2Int(1, 0);   // 右
            if (angle >= 22.5f && angle < 67.5f) return new Vector2Int(1, 1);    // 右上
            if (angle >= 67.5f && angle < 112.5f) return new Vector2Int(0, 1);   // 上
            if (angle >= 112.5f && angle < 157.5f) return new Vector2Int(-1, 1); // 左上
            if (angle >= 157.5f && angle < 202.5f) return new Vector2Int(-1, 0); // 左
            if (angle >= 202.5f && angle < 247.5f) return new Vector2Int(-1, -1); // 左下
            if (angle >= 247.5f && angle < 292.5f) return new Vector2Int(0, -1); // 下
            return new Vector2Int(1, -1); // 右下
        }

        /// <summary>
        /// 检查网格及其相邻网格
        /// </summary>
        private void CheckGridAndNeighbors(
            Vector2Int gridPos,
            Vector2 centerPos,
            Vector2 direction,
            float range,
            List<GameActor> result,
            HashSet<Vector2Int> checkedGrids,
            System.Func<GameActor, bool> filter)
        {
            // 检查主网格
            CheckSingleGrid(gridPos, centerPos, direction, range, result, checkedGrids, filter);
            
            // 检查相邻网格（形成扇形）
            Vector2Int[] neighbors = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // 上
                new Vector2Int(0, -1),  // 下
                new Vector2Int(1, 0),   // 右
                new Vector2Int(-1, 0),  // 左
            };
            
            foreach (var neighbor in neighbors)
            {
                Vector2Int neighborGrid = gridPos + neighbor;
                CheckSingleGrid(neighborGrid, centerPos, direction, range, result, checkedGrids, filter);
            }
        }

        /// <summary>
        /// 检查单个网格
        /// </summary>
        private void CheckSingleGrid(
            Vector2Int gridPos,
            Vector2 centerPos,
            Vector2 direction,
            float range,
            List<GameActor> result,
            HashSet<Vector2Int> checkedGrids,
            System.Func<GameActor, bool> filter)
        {
            if (checkedGrids.Contains(gridPos))
                return;
            
            checkedGrids.Add(gridPos);
            
            if (!m_gridDict.TryGetValue(gridPos, out var actorList))
                return;
            
            foreach (var actor in actorList)
            {
                if (actor == null || actor.IsDestroyed)
                    continue;
                
                // 检查距离
                float distance = Vector2.Distance(centerPos, actor.Position);
                if (distance > range)
                    continue;
                
                // 检查是否在正方向（使用点积判断）
                Vector2 toActor = (actor.Position - centerPos).normalized;
                float dot = Vector2.Dot(direction, toActor);
                if (dot < 0.5f) // 角度约 60 度内
                    continue;
                
                // 应用过滤
                if (filter != null && !filter(actor))
                    continue;
                
                result.Add(actor);
            }
        }

        /// <summary>
        /// 清空所有网格
        /// </summary>
        public void Clear()
        {
            m_gridDict.Clear();
            m_actorToGrid.Clear();
        }

        /// <summary>
        /// 获取所有注册的 Actor 数量
        /// </summary>
        public int GetActorCount()
        {
            return m_actorToGrid.Count;
        }
    }
}

