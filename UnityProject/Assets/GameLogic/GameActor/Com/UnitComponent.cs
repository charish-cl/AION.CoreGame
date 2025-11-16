using AION.CoreFramework;
using GameConfig;
using GameConfig.battle;

namespace GameLogic
{
    /// <summary>
    /// 单位组件，用于管理单位的配置数据
    /// </summary>
    public class UnitComponent : GameActorCmp
    {
        /// <summary>
        /// 单位配置ID
        /// </summary>
        public int UnitId { get; private set; }
        
        /// <summary>
        /// 单位配置数据
        /// </summary>
        public UnitConfig Config { get; private set; }
        
        /// <summary>
        /// 单位ID（从配置获取）
        /// </summary>
        public int Id => Config?.Id ?? 0;
        
        /// <summary>
        /// 单位名称（从配置获取）
        /// </summary>
        public string Name => Config?.Name ?? string.Empty;
        
        /// <summary>
        /// 单位类型（从配置获取）
        /// </summary>
        public EUnitType? UnitType => Config?.UnitType;
        
        /// <summary>
        /// 最大生命值（从配置获取）
        /// </summary>
        public int MaxHp => Config?.MaxHp ?? 0;
        
        /// <summary>
        /// 模型配置（从配置获取）
        /// </summary>
        public GameConfig.res.ModelConfig ModelConfig => Config?.ModelId_Ref;
        
        /// <summary>
        /// 初始化单位组件
        /// </summary>
        /// <param name="unitId">单位配置ID</param>
        public void Init(int unitId)
        {
            UnitId = unitId;
            LoadConfig();
        }
        
        /// <summary>
        /// 从配置系统加载配置
        /// </summary>
        private void LoadConfig()
        {
            if (ConfigSystem.Instance?.Tables?.TbUnit != null)
            {
                Config = ConfigSystem.Instance.Tables.TbUnit.GetOrDefault(UnitId);
                if (Config == null)
                {
                    Log.Warning($"UnitComponent: 未找到单位配置，UnitId = {UnitId}");
                }
            }
            else
            {
                Log.Error("UnitComponent: ConfigSystem 未初始化或 TbUnit 为空");
            }
        }
        
        public override void OnInit()
        {
            base.OnInit();
            
            // 如果还没有加载配置，尝试加载
            if (Config == null && UnitId > 0)
            {
                LoadConfig();
            }
            
            // 如果配置加载成功，初始化数值组件
            if (Config != null)
            {
                InitializeFromConfig();
            }
        }
        
        /// <summary>
        /// 根据配置初始化数值组件
        /// </summary>
        private void InitializeFromConfig()
        {
            var numericCmp = GetComponent<NumericComponent>();
            if (numericCmp != null && MaxHp > 0)
            {
                // 设置最大生命值
                numericCmp.Set(NumericType.MaxHpBase, MaxHp);
                // 设置当前生命值为最大生命值
                numericCmp.Set(NumericType.HpBase, MaxHp);
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
                return $"UnitComponent: Id={Id}, Name={Name}, UnitType={UnitType}, MaxHp={MaxHp}";
            }
            return $"UnitComponent: UnitId={UnitId} (Config not loaded)";
        }
    }
}

