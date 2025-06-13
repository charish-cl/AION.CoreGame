using System;

namespace GameDevKit.BehaviorTree
{
    public class ConditionNode : Node
    {
        private Func<bool> condition;
        
        public override NodeState Update()
        {
            if (condition == null) return NodeState.Failed;
            return condition.Invoke() ? NodeState.Success : NodeState.Failed;
        }
    }
}