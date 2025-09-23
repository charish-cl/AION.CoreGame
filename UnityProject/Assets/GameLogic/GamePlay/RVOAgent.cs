namespace GameLogic
{
    using System.Collections.Generic;
    using UnityEngine;

    public class RVOAgent : MonoBehaviour
    {
        // 基础参数
        [Header("基础参数")] public float radius = 0.5f; // 智能体半径
        public Vector2 velocity = new Vector2(1, 0); // 当前速度
        public Vector2 preferredSpeed = new Vector2(1, 0); // 期望速度
        public float maxSpeed = 2.0f; // 最大速度

        // RVO参数
        [Header("RVO参数")] public float timeHorizon = 2.0f; // 预测时间
        public float neighborDist = 3.0f; // 邻居检测距离

        private List<RVOAgent> neighbors = new List<RVOAgent>();

        void Update()
        {
            FindNeighbors();
            velocity = ComputeRVO();
            transform.position += (Vector3)velocity * Time.deltaTime;
        }

        // 寻找邻近智能体
        void FindNeighbors()
        {
            neighbors.Clear();
            foreach (var agent in FindObjectsOfType<RVOAgent>())
            {
                if (agent == this) continue;
                float dist = Vector2.Distance(transform.position, agent.transform.position);
                if (dist < neighborDist) neighbors.Add(agent);
            }
        }

        // RVO核心计算
        Vector2 ComputeRVO()
        {
            Vector2 newVelocity = preferredSpeed;
            int count = 0;

            foreach (var neighbor in neighbors)
            {
                Vector2 toAgent = neighbor.transform.position - transform.position;
                float dist = toAgent.magnitude;

                // 超过预测时间或距离则忽略
                if (dist > timeHorizon * maxSpeed || dist > neighborDist) continue;

                // 计算相对速度障碍
                Vector2 relativeVelocity = velocity - neighbor.velocity;
                Vector2 collisionVector = CalculateCollisionVector(toAgent, relativeVelocity);

                if (collisionVector != Vector2.zero)
                {
                    newVelocity += collisionVector;
                    count++;
                }
            }

            // 平均调整速度
            if (count > 0) newVelocity /= count;

            // 限制速度大小
            if (newVelocity.magnitude > maxSpeed)
                newVelocity = newVelocity.normalized * maxSpeed;

            return newVelocity;
        }

        // 计算碰撞规避向量
        Vector2 CalculateCollisionVector(Vector2 toAgent, Vector2 relVel)
        {
            float combinedRad = radius + radius;
            float timeToCollision = (combinedRad - toAgent.magnitude) / relVel.magnitude;

            if (timeToCollision <= 0) return Vector2.zero;

            // 计算规避方向（互惠式调整）
            Vector2 avoidanceDir = (toAgent.normalized + relVel.normalized).normalized;
            return avoidanceDir * (maxSpeed - velocity.magnitude);
        }
    }
}