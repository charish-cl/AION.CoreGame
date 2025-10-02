using UnityEngine;

namespace GameDevKit.BehaviorTree
{
    public class WaitNode : ConditionNode
    {
        public float second;
        public float currentTime;
        
        public WaitNode(float second)
        {
            this.second = second;
        }

        public override NodeState Update()
        {
            currentTime += Time.deltaTime;
            State = NodeState.Running;
            if (currentTime > second) State = NodeState.Success;
            return State;
        }

        public override void Reset()
        {
            currentTime = 0;
        }
    }
}