using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// Grid 设置，继承自 GameLocalSetting
    /// 用于保存网格相关的配置
    /// </summary>
    [CreateAssetMenu(fileName = "GridSetting", menuName = "GameLogic/LocalSettings/GridSetting")]
    public class GridSetting : GameLocalSetting
    {
        [Header("网格系统设置")]
        [Tooltip("网格单元大小")]
        public Vector2 cellSize = new Vector2(1f, 1f);
        
        [Tooltip("网格原点（世界坐标）")]
        public Vector2 gridOrigin = Vector2.zero;
        
        [Tooltip("网格尺寸（单元数量）")]
        public Vector2Int gridSize = new Vector2Int(50, 50);

    }
}

