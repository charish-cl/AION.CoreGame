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
    }
}

