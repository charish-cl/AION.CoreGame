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
        
        protected override void CreateModel()
        {
            base.CreateModel();
            
            // 从配置获取模型配置并实例化
            var towerConfig = GetConfig<TowerConfig>();
            if (towerConfig != null && towerConfig.ModelId_Ref != null)
            {
                InstantiateModel(towerConfig.ModelId_Ref);
            }
        }
        
        protected override void BindCmp()
        {
            base.BindCmp();
            
            // 基础组件
            AddComponent<NumericComponent>();
            AddComponent<TowerFSMCmp>();
            AddComponent<ActorAnimViewCmp>();
            
            var towerConfig = GetConfig<TowerConfig>();

            //只有攻击塔才需要朝向组件
            if (towerConfig.AttackRange>0)
            {
                AddComponent<OrientationViewCmp>();
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
            
            // 从TowerConfig初始化数值
            var towerConfig = GetConfig<TowerConfig>();
            if (towerConfig != null)
            {
                // TowerConfig 没有生命值、攻击力等，只设置攻击范围
                // 如果有攻击间隔列表，使用第一个值作为基础攻击速度
                if (towerConfig.AttackInterals != null && towerConfig.AttackInterals.Count > 0)
                {
                    numericCmp.Set(NumericType.AttackSpeedBase, towerConfig.AttackInterals[0]);
                }
                // 攻击范围可能需要通过其他方式设置，这里暂时不设置
            }
        }
    }
}

