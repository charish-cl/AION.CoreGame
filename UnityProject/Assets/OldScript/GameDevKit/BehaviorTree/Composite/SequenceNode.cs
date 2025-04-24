using System.Collections.Generic;

namespace GameDevKit.BehaviorTree
{
    public class SequenceNode : CompositNode
    {
        public SequenceNode(List<Node> childeren) : base(childeren)
        {
        }

        public override NodeState Update()
        {
            foreach (var btNode in childeren)
            {
                var state = btNode.Update();
                if (state != NodeState.Success) return state;
            }

            return NodeState.Success;
        }

        public override void Reset()
        {
            foreach (var btNode in childeren)
            {
                btNode.Reset();
            }
        }
    }
}