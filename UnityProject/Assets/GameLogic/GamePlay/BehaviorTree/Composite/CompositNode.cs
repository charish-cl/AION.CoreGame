using System.Collections.Generic;

namespace GameDevKit.BehaviorTree
{
    public abstract class CompositNode : Node
    {
        protected List<Node> childeren;

        protected CompositNode(List<Node> childeren)
        {
            this.childeren = childeren ?? new List<Node>();
        }

        public virtual CompositNode AddNode(Node node)
        {
            childeren.Add(node);
            return this;
        }
    }
}