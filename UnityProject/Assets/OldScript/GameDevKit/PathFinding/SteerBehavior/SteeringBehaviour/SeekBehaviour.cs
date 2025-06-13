using System.Linq;
using UnityEngine;

public class SeekBehaviour : SteeringBehaviour
{
    [SerializeField]
    private float targetRechedThreshold = 0.5f;

    [SerializeField]
    private bool showGizmo = true;

    bool reachedLastTarget = true;

    //gizmo parameters
    private Vector2 targetPositionCached;
    private float[] interestsTemp;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        //if we don't have a target stop seeking
        //else set a new target
        if (reachedLastTarget)
        {
            if (aiData.targets == null || aiData.targets.Count <= 0)
            {
                aiData.currentTarget = null;
                return (danger, interest);
            }
            else
            {
                reachedLastTarget = false;
                aiData.currentTarget = aiData.targets.OrderBy
                    (target => Vector2.Distance(target.position, transform.position)).FirstOrDefault();
            }

        }

        //cache the last position only if we still see the target (if the targets collection is not empty)
        if (aiData.currentTarget != null && aiData.targets != null && aiData.targets.Contains(aiData.currentTarget))
            targetPositionCached = aiData.currentTarget.position;

        //First check if we have reached the target
        if (Vector2.Distance(transform.position, targetPositionCached) < targetRechedThreshold)
        {
            reachedLastTarget = true;
            aiData.currentTarget = null;
            return (danger, interest);
        }

        //通过将目标位置和当前位置的向量相减，可以得到指向目标方向的向量。
        //然后，使用 Unity 引擎中的 Vector2.Dot() 方法计算该向量与八个方向向量之间的夹角余弦值，从而确定其在该方向上的兴趣度。
        //只有夹角小于 90 度的方向才会被接受，并且如果该方向的兴趣度高于之前记录的值，
        //则更新该方向的兴趣度。最终，返回的 interest 数组表示每个方向的兴趣度，可以用于制定行动计划、决策等。
        //If we havent yet reached the target do the main logic of finding the interest directions
        Vector2 directionToTarget = (targetPositionCached - (Vector2)transform.position);
        for (int i = 0; i < interest.Length; i++)
        {
            float result = Vector2.Dot(directionToTarget.normalized, Directions.eightDirections[i]);

            //accept only directions at the less than 90 degrees to the target direction
            if (result > 0)
            {
                float valueToPutIn = result;
                if (valueToPutIn > interest[i])
                {
                    interest[i] = valueToPutIn;
                }

            }
        }
        interestsTemp = interest;
        return (danger, interest);
    }

    private void OnDrawGizmos()
    {

        if (showGizmo == false)
            return;
        Gizmos.DrawSphere(targetPositionCached, 0.2f);

        if (Application.isPlaying && interestsTemp != null)
        {
            if (interestsTemp != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < interestsTemp.Length; i++)
                {
                    Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestsTemp[i]*2);
                }
                if (reachedLastTarget == false)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(targetPositionCached, 0.1f);
                }
            }
        }
    }
}
