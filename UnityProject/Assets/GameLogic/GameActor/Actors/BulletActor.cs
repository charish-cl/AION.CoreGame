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
        
        protected override void BindCmp()
        {
            base.BindCmp();
            
            // 基础组件
            AddComponent<NumericComponent>();
            AddComponent<CollisionDetectCmp>(); // 预先添加碰撞检测组件，避免在 OnInit 时添加导致集合修改异常
            AddComponent<MoveViewCmp>();
            
            // 子弹组件
            var bulletCmp = AddComponent<BulletCmp>();
            bulletCmp.Init(TargetPosition);
            
            var orientationCmp = AddComponent<OrientationViewCmp>();
            orientationCmp.SetTarget(TargetPosition);
            
            // 如果指定了bulletId，添加BulletComponent
            if (BulletId > 0)
            {
                var bulletComponent = AddComponent<BulletComponent>();
                bulletComponent.Init(BulletId);
            }
            
            // 添加ModelComponent，它会从Actor获取配置
            AddComponent<ModelComponent>();
        }
    }
}

