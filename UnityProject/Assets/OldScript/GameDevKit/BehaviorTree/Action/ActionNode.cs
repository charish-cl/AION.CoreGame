using System;

namespace GameDevKit.BehaviorTree
{
    public class ActionNode : Node
    {
        private Action _action;

        public ActionNode(Action action)
        {
            _action = action;
        }

        public override NodeState Update()
        {
            _action?.Invoke();
            return NodeState.Success;
        }
    }
}