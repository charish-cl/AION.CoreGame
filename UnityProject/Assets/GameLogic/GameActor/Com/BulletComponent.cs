using AION.CoreFramework;
using GameConfig;
using GameConfig.battle;

namespace GameLogic
{
    /// <summary>
    /// 子弹组件，用于管理子弹的配置数据
    /// </summary>
    public class BulletComponent : GameActorCmp
    {
        /// <summary>
        /// 子弹配置ID
        /// </summary>
        public int BulletId { get; private set; }
        
        /// <summary>
        /// 子弹配置数据
        /// </summary>
        public BulletConfig Config { get; private set; }
        
        /// <summary>
        /// 子弹ID（从配置获取）
        /// </summary>
        public int Id => Config?.Id ?? 0;
        
        /// <summary>
        /// 子弹名称（从配置获取）
        /// </summary>
        public string Name => Config?.Name ?? string.Empty;
        
        /// <summary>
        /// 子弹类型（从配置获取）
        /// </summary>
        public EBulletType? BulletType => Config?.BulletType;
        
        /// <summary>
        /// Buff配置（从配置获取）
        /// </summary>
        public BuffConfig BuffConfig => Config?.Buffs_Ref;
        
        /// <summary>
        /// Buff ID（从配置获取）
        /// </summary>
        public int BuffId => Config?.Buffs ?? 0;
        
        /// <summary>
        /// 模型配置（从配置获取）
        /// </summary>
        public GameConfig.res.ModelConfig ModelConfig => Config?.ModelId_Ref;
        
        /// <summary>
        /// 初始化子弹组件
        /// </summary>
        /// <param name="bulletId">子弹配置ID</param>
        public void Init(int bulletId)
        {
            BulletId = bulletId;
            LoadConfig();
        }
        
        /// <summary>
        /// 从配置系统加载配置
        /// </summary>
        private void LoadConfig()
        {
            if (ConfigSystem.Instance?.Tables?.TbBullet != null)
            {
                Config = ConfigSystem.Instance.Tables.TbBullet.GetOrDefault(BulletId);
                if (Config == null)
                {
                    Log.Warning($"BulletComponent: 未找到子弹配置，BulletId = {BulletId}");
                }
            }
            else
            {
                Log.Error("BulletComponent: ConfigSystem 未初始化或 TbBullet 为空");
            }
        }
        
        public override void OnInit()
        {
            base.OnInit();
            
            // 如果还没有加载配置，尝试加载
            if (Config == null && BulletId > 0)
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
                return $"BulletComponent: Id={Id}, Name={Name}, BulletType={BulletType}, BuffId={BuffId}";
            }
            return $"BulletComponent: BulletId={BulletId} (Config not loaded)";
        }
    }
}

