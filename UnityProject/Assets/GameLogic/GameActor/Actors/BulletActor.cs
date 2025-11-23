using AION.CoreFramework;
using UnityEngine;
using GameConfig;
using GameConfig.battle;

namespace GameLogic
{
    /// <summary>
    /// 子弹Actor
    /// </summary>
    public class BulletActor : GameActor
    {
        /// <summary>
        /// 子弹配置ID
        /// </summary>
        public int BulletId { get; private set; }
        
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector2 TargetPosition { get; private set; }
        
        /// <summary>
        /// 真正的攻击者Actor（发射子弹的单位，不是子弹本身）
        /// </summary>
        public GameActor RealAttacker { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bulletId">子弹配置ID，如果为0则使用默认配置</param>
        /// <param name="targetPosition">目标位置</param>
        public BulletActor(int bulletId = 0, Vector2 targetPosition = default)
        {
            BulletId = bulletId;
            TargetPosition = targetPosition;
        }
        
        protected override void InitConfig()
        {
            base.InitConfig();
            
            // 如果指定了bulletId，加载配置
            if (BulletId > 0 && ConfigSystem.Instance?.Tables?.TbBullet != null)
            {
                var config = ConfigSystem.Instance.Tables.TbBullet.GetOrDefault(BulletId);
                if (config != null)
                {
                    SetConfig<BulletConfig>(config);
                }
                else
                {
                    Log.Warning($"BulletActor: 未找到子弹配置，BulletId = {BulletId}");
                }
            }
        }
        
        protected override void CreateModel()
        {
            base.CreateModel();
            
            // 从配置获取模型配置并实例化
            var bulletConfig = GetConfig<BulletConfig>();
            if (bulletConfig != null && bulletConfig.ModelId_Ref != null)
            {
                // 检查是否为近战子弹（ModelId 为 0 或 ModelId_Ref 为空表示近战，不需要预制体）
                if (bulletConfig.ModelId > 0 && !string.IsNullOrEmpty(bulletConfig.ModelId_Ref?.Path))
                {
                    InstantiateModel(bulletConfig.ModelId_Ref);
                }
                else
                {
                    // 近战子弹：不创建模型，只创建空的 GameObject
                    Log.Info($"BulletActor: 近战子弹 {BulletId}，不创建模型预制体");
                }
            }
        }
        
        protected override void BindCmp()
        {
            base.BindCmp();
            
            // 基础组件
            AddComponent<NumericComponent>();
            AddComponent<CollisionDetectCmp>(); // 预先添加碰撞检测组件，避免在 OnInit 时添加导致集合修改异常
            
            // 子弹组件
            var bulletCmp = AddComponent<BulletCmp>();
            var bulletConfig = GetConfig<BulletConfig>();
            
            // 检查是否为近战子弹（ModelId 为 0 或 ModelId_Ref 为空表示近战）
            if (bulletConfig != null && (bulletConfig.ModelId == 0 || bulletConfig.ModelId_Ref == null || string.IsNullOrEmpty(bulletConfig.ModelId_Ref.Path)))
            {
                // 近战子弹：设置攻击模式为近战，不添加可视化组件
                bulletCmp.AttackMode = AttackMode.Melee;
                // 计算方向（从攻击者位置指向目标位置）
                Vector2 direction = (TargetPosition - Position).normalized;
                bulletCmp.InitMelee(Position, direction);
                Log.Info($"BulletActor: 子弹 {BulletId} 设置为近战模式（逻辑子弹，无预制体）");
            }
            else
            {
                // 远程子弹：正常初始化，添加可视化组件
                AddComponent<MoveViewCmp>();
                bulletCmp.Init(TargetPosition);
                var orientationCmp = AddComponent<OrientationViewCmp>();
                orientationCmp.SetTarget(TargetPosition);
            }
        }
        
        protected override void InitializeNumericFromConfig()
        {
            base.InitializeNumericFromConfig();
            
            
            // BulletActor 通常不需要从配置初始化数值，或者可以在这里设置子弹特有的属性
            // 如果需要，可以从 BulletConfig 读取相关属性
            var bulletConfig = GetConfig<BulletConfig>();
            if (bulletConfig != null)
            {
                NumericComponent.Set(NumericType.BulletMoveSpeedBase, bulletConfig.Speed);
            }
        }
    }
}

