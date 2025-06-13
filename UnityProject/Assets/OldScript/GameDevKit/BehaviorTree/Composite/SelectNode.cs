using System.Collections.Generic;

namespace GameDevKit.BehaviorTree
{
    public class SelectNode : CompositNode
    {
        public SelectNode(List<Node> childeren) : base(childeren)
        {
        }

        public override NodeState Update()
        {
           
            foreach (var btNode in childeren)
            {
                var state = btNode.Update();
                if (state != NodeState.Failed) return state;
            }

            return NodeState.Failed;
        }
    }

}