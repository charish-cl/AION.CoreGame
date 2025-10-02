using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObstacleAvoidanceBehaviour : SteeringBehaviour
{
    [SerializeField] private float radius = 2f;

    
    [LabelText("碰撞半径")]
    [SerializeField]
    private float agentColliderSize = 0.6f;

    [SerializeField] private bool showGizmo = true;

    //gizmo parameters
    float[] dangersResultTemp = null;

    //找出所有障碍物距离最近的那个障碍物,然后计算出障碍物的权重,然后把障碍物的权重加到danger数组中
    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        foreach (Collider2D obstacleCollider in aiData.obstacles)
        {
            Vector2 directionToObstacle = obstacleCollider.ClosestPoint(transform.position)
                                          - (Vector2)transform.position;
            
            float distanceToObstacle = directionToObstacle.magnitude;
//radius变量在ObstacleAvoidanceBehaviour类中用于根据障碍物与代理之间的距离计算障碍物的权重。
//权重的计算方式如下：如果到障碍物的距离小于或等于agentColliderSize，则权重设置为1。
//否则，权重计算为(radius - distanceToObstacle) / radius。
//然后，这个权重用于确定存储在Directions类中的八个方向中每个方向上障碍物的危险级别。
            //calculate weight based on the distance Enemy<--->Obstacle
            float weight
                = distanceToObstacle <= agentColliderSize
                    ? 1
                    : (radius - distanceToObstacle) / radius;

            Vector2 directionToObstacleNormalized = directionToObstacle.normalized;

            //Add obstacle parameters to the danger array
            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                float result = Vector2.Dot(directionToObstacleNormalized, Directions.eightDirections[i]);

                float valueToPutIn = result * weight;

                //override value only if it is higher than the current one stored in the danger array
                if (valueToPutIn > danger[i])
                {
                    danger[i] = valueToPutIn;
                }
            }
        }

        dangersResultTemp = danger;
        return (danger, interest);
    }

    private void OnDrawGizmos()
    {
        if (showGizmo == false)
            return;

        if (Application.isPlaying && dangersResultTemp != null)
        {
            if (dangersResultTemp != null)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < dangersResultTemp.Length; i++)
                {
                    Gizmos.DrawRay(
                        transform.position,
                        Directions.eightDirections[i] * dangersResultTemp[i] * 2
                    );
                }
            }
        }
    }
}

public static class Directions
{
    public static List<Vector2> eightDirections = new List<Vector2>
    {
        new Vector2(0, 1).normalized,
        new Vector2(1, 1).normalized,
        new Vector2(1, 0).normalized,
        new Vector2(1, -1).normalized,
        new Vector2(0, -1).normalized,
        new Vector2(-1, -1).normalized,
        new Vector2(-1, 0).normalized,
        new Vector2(-1, 1).normalized
    };
}