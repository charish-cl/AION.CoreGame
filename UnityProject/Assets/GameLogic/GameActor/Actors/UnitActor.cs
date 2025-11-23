using AION.CoreFramework;
using UnityEngine;
using GameConfig;
using GameConfig.battle;

namespace GameLogic
{
    /// <summary>
    /// 单位Actor，用于英雄和敌人
    /// </summary>
    public class UnitActor : GameActor
    {
        /// <summary>
        /// 单位配置ID
        /// </summary>
        public int UnitId { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitId">单位配置ID，如果为0则使用默认配置</param>
        public UnitActor(int unitId = 0)
        {
            UnitId = unitId;
        }
        
        protected override void InitConfig()
        {
            base.InitConfig();
            
            // 如果指定了unitId，加载配置
            if (UnitId > 0 && ConfigSystem.Instance?.Tables?.TbUnit != null)
            {
                var config = ConfigSystem.Instance.Tables.TbUnit.GetOrDefault(UnitId);
                if (config != null)
                {
                    SetConfig<UnitConfig>(config);
                }
                else
                {
                    Log.Warning($"UnitActor: 未找到单位配置，UnitId = {UnitId}");
                }
            }
            else
            {
                Log.Warning("UnitActor: 未指定单位配置");
            }
        }
        
        protected override void BindCmp()
        {
            base.BindCmp();
            
            // 基础组件
            AddComponent<NumericComponent>();
            AddComponent<BuffCmp>();
            AddComponent<HealthCmp>();
            AddComponent<MoveViewCmp>();
            AddComponent<DirectionViewCmp>();
            AddComponent<OrientationViewCmp>();
            
            // 如果指定了unitId，添加UnitComponent
            if (UnitId > 0)
            {
                var unitComponent = AddComponent<UnitComponent>();
                unitComponent.Init(UnitId);
            }
            
            // 添加ModelComponent，它会从Actor获取配置
            AddComponent<ModelComponent>();
        }
    }
}

