using System.Collections.Generic;

namespace GameDevKit.BehaviorTree
{
    //包括前提（Precondition），选择节点（Selector），并行节点（Parallel），序列节点（Sequence）
    public enum NodeState
    {
        Running,
        Success,
        Ready,
        Failed
    }
    public abstract class Node
    {
        public Node parent;
        public NodeState State;
        public abstract NodeState Update();

        public virtual void Reset()
        {
        }
    }

}