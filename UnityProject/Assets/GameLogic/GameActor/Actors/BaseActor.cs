using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 基地Actor
    /// </summary>
    public class BaseActor : GameActor
    {
        protected override void BindCmp()
        {
            base.BindCmp();
            
            // 基础组件
            AddComponent<NumericComponent>();
            AddComponent<BuffCmp>();
            AddComponent<HealthCmp>();
            AddComponent<CampComponent>(); // 添加基地组件
        }
        
        protected override void InitializeNumericFromConfig()
        {
            base.InitializeNumericFromConfig();
            
            var numericCmp = NumericComponent;
            if (numericCmp == null)
            {
                return;
            }
            
            // 基地有更高的生命值
            numericCmp.Set(NumericType.MaxHpBase, 1000);
            numericCmp.Set(NumericType.HpBase, 1000);
        }
    }
}

