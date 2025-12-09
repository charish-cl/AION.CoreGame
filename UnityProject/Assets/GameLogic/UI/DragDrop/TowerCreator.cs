using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 塔创建工具类 - 独立的创建方法
    /// </summary>
    public static class TowerCreator
    {
        /// <summary>
        /// 创建塔
        /// </summary>
        public static GameActor CreateTower(int towerId, Vector2 worldPosition, GridHelper gridHelper = null)
        {
            if (ActorMgr.Instance == null)
            {
                Log.Error("TowerCreator: ActorMgr.Instance 为空");
                return null;
            }
            
            return ActorMgr.Instance.CreateTower(towerId, worldPosition);
        }
    }
}

