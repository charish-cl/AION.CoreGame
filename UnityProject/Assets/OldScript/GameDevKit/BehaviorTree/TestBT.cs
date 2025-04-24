using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.BehaviorTree
{
    public class TestBT:MonoBehaviour
    {
        private void Start()
        {
            SequenceNode node = new SequenceNode(
                new List<Node>()
                {
                    new WaitNode(2),
                    new ActionNode(() =>
                    {
                        Debug.Log("wait2 seconds");
                    })
                }
            );
           tree = new BehaviorTree(node);
        }

        private BehaviorTree tree;
        private void Update()
        {
            tree.Update();
        }
    }
}