using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 敌人Actor
    /// </summary>
    public class EnemyActor : UnitActor
    {
        public EnemyActor(int unitId = 0) : base(unitId)
        {
        }
        
        protected override void BindCmp()
        {
            base.BindCmp();
            
            // 敌人特有组件
            AddComponent<SimplePathFindingLogicCmp>();
            AddComponent<HPBarCmp>();
            AddComponent<MonsterFSMCmp>(); // 添加Monster状态机
        }
    }
}

