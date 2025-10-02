namespace GameDevKit.BehaviorTree
{
    public class BehaviorTree
    {
        private Node root;
        private Node runingNode;
        public NodeState State;

        public BehaviorTree(Node root)
        {
            this.root = root;
            State = NodeState.Ready;
        }

        public NodeState Update()
        {
            State = root.Update();
            if(State == NodeState.Success) Reset();
            return State;
        }

        public void Reset()
        {
            root.Reset();
        }
    }
}