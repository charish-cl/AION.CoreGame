using AION.CoreFramework;
using GameConfig;
using GameConfig.battle;

namespace GameLogic
{
    /// <summary>
    /// 建筑组件，用于管理建筑的配置数据
    /// </summary>
    public class TowerComponent : GameActorCmp
    {
        /// <summary>
        /// 建筑配置ID
        /// </summary>
        public int TowerId { get; private set; }
        
        /// <summary>
        /// 建筑配置数据
        /// </summary>
        public TowerConfig Config { get; private set; }
        
        /// <summary>
        /// 建筑ID（从配置获取）
        /// </summary>
        public int Id => Config?.Id ?? 0;
        
        /// <summary>
        /// 建筑名称（从配置获取）
        /// </summary>
        public string Name => Config?.Name ?? string.Empty;
        
        /// <summary>
        /// 建筑描述（从配置获取）
        /// </summary>
        public string Desc => Config?.Desc ?? string.Empty;
        
        /// <summary>
        /// 子弹配置（从配置获取）
        /// </summary>
        public BulletConfig BulletConfig => Config?.BulletId_Ref;
        
        /// <summary>
        /// 子弹ID（从配置获取）
        /// </summary>
        public int BulletId => Config?.BulletId ?? 0;
        
        /// <summary>
        /// 攻击间隔列表（从配置获取）
        /// </summary>
        public System.Collections.Generic.List<float> AttackIntervals => Config?.AttackInterals;

        /// <summary>
        /// 攻击范围（从配置获取）
        /// </summary>
        public float AttackRange => Config?.AttackRange ?? 0f;
        
        /// <summary>
        /// 模型配置（从配置获取）
        /// </summary>
        public GameConfig.res.ModelConfig ModelConfig => Config?.ModelId_Ref;
        
        /// <summary>
        /// 初始化建筑组件
        /// </summary>
        /// <param name="towerId">建筑配置ID</param>
        public void Init(int towerId)
        {
            TowerId = towerId;
            LoadConfig();
        }
        
        /// <summary>
        /// 从配置系统加载配置
        /// </summary>
        private void LoadConfig()
        {
            if (ConfigSystem.Instance?.Tables?.TbTower != null)
            {
                Config = ConfigSystem.Instance.Tables.TbTower.GetOrDefault(TowerId);
                if (Config == null)
                {
                    Log.Warning($"TowerComponent: 未找到建筑配置，TowerId = {TowerId}");
                }
            }
            else
            {
                Log.Error("TowerComponent: ConfigSystem 未初始化或 TbTower 为空");
            }
        }
        
        public override void OnInit()
        {
            base.OnInit();
            
            // 如果还没有加载配置，尝试加载
            if (Config == null && TowerId > 0)
            {
                LoadConfig();
            }
        }
        
        /// <summary>
        /// 检查配置是否有效
        /// </summary>
        public bool IsConfigValid => Config != null;
        
        public override string ToString()
        {
            if (Config != null)
            {
                return $"TowerComponent: Id={Id}, Name={Name}, BulletId={BulletId}";
            }
            return $"TowerComponent: TowerId={TowerId} (Config not loaded)";
        }
    }
}

