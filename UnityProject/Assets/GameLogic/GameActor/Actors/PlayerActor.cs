using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 玩家Actor（英雄）
    /// </summary>
    public class PlayerActor : UnitActor
    {
        public PlayerActor(int unitId = 0) : base(unitId)
        {
        }
        
        protected override void BindCmp()
        {
            base.BindCmp();
            
            // 玩家特有组件
            AddComponent<MoveLogicCmp>();
            AddComponent<InputLogicCmp>();
            AddComponent<ActorAnimViewCmp>();
            AddComponent<UnitFSMCmp>(); // 添加Hero状态机
        }
    }
}

