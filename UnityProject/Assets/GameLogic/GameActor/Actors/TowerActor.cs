using AION.CoreFramework;
using GameConfig;
using GameConfig.battle;

namespace GameLogic
{
    /// <summary>
    /// 塔Actor
    /// </summary>
    public class TowerActor : GameActor
    {
        /// <summary>
        /// 塔配置ID
        /// </summary>
        public int TowerId { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="towerId">塔配置ID，如果为0则使用默认配置</param>
        public TowerActor(int towerId = 0)
        {
            TowerId = towerId;
        }
        
        protected override void InitConfig()
        {
            base.InitConfig();
            
            // 如果指定了towerId，加载配置
            if (TowerId > 0 && ConfigSystem.Instance?.Tables?.TbTower != null)
            {
                var config = ConfigSystem.Instance.Tables.TbTower.GetOrDefault(TowerId);
                if (config != null)
                {
                    SetConfig<TowerConfig>(config);
                }
                else
                {
                    Log.Warning($"TowerActor: 未找到塔配置，TowerId = {TowerId}");
                }
            }
            else
            {
                Log.Warning("TowerActor: 未指定单位配置");
            }
            
        }
        
        protected override void BindCmp()
        {
            base.BindCmp();
            
            // 基础组件
            AddComponent<NumericComponent>();
            AddComponent<TowerFSMCmp>();
            AddComponent<OrientationViewCmp>();
            
            // 如果指定了towerId，添加TowerComponent
            if (TowerId > 0)
            {
                var towerComponent = AddComponent<TowerComponent>();
                towerComponent.Init(TowerId);
            }
            
            // 添加ModelComponent，它会从Actor获取配置
            AddComponent<ModelComponent>();
        }
    }
}

