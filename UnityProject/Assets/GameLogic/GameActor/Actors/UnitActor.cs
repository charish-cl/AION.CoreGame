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
        
        protected override void CreateModel()
        {
            base.CreateModel();
            
            // 从配置获取模型配置并实例化
            var unitConfig = GetConfig<UnitConfig>();
            if (unitConfig != null && unitConfig.ModelId_Ref != null)
            {
                InstantiateModel(unitConfig.ModelId_Ref);
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
            AddComponent<ActorAnimViewCmp>();

            // 如果指定了unitId，添加UnitComponent
            if (UnitId > 0)
            {
                var unitComponent = AddComponent<UnitComponent>();
                unitComponent.Init(UnitId);
            }
        }
        
        protected override void InitializeNumericFromConfig()
        {
            base.InitializeNumericFromConfig();
            
            var numericCmp = NumericComponent;
            if (numericCmp == null)
            {
                return;
            }
            
            // 从UnitConfig初始化数值
            var unitConfig = GetConfig<UnitConfig>();
            if (unitConfig != null)
            {
                numericCmp.Set(NumericType.MaxHpBase, unitConfig.MaxHp);
                numericCmp.Set(NumericType.HpBase, unitConfig.MaxHp);
                numericCmp.Set(NumericType.AttackBase, unitConfig.Attack);
                numericCmp.Set(NumericType.DefenseBase, unitConfig.Defense);
                numericCmp.Set(NumericType.SpeedBase, unitConfig.MoveSpeed);
                numericCmp.Set(NumericType.AttackSpeedBase, unitConfig.AttackInterval);
            }
        }
    }
}

