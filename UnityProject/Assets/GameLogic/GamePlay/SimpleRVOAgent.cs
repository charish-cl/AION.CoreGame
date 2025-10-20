using UnityEngine;
using System.Collections.Generic;

public class SimpleRVOAgent : MonoBehaviour
{
    [Header("RVO Parameters")]
     float radius = 0.7f;
    float maxSpeed = 1.0f;
    float neighborDist = 2f;
    public float timeHorizon = 2.0f;
    
    private Vector2 _currentVelocity;
    private Vector2 _preferredVelocity;
    
    // 存储场景中所有Agent的引用
    private static List<SimpleRVOAgent> _allAgents = new List<SimpleRVOAgent>();
    
    void Start()
    {
        // 所有Agent都向右移动作为首选速度
        _preferredVelocity = Vector2.right * maxSpeed;
        _currentVelocity = _preferredVelocity;
        
        // 注册到全局列表
        _allAgents.Add(this);
    }
    
    void Update()
    {
        Vector2 direction = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position;
        
        _preferredVelocity = direction.normalized * maxSpeed;
        
        // 计算新的安全速度
        Vector2 newVelocity = ComputeRVOVelocity();
        _currentVelocity = newVelocity;
        
        // 更新位置
        Vector3 newPosition = transform.position + (Vector3)newVelocity * Time.deltaTime;
        transform.position = newPosition;
        
        
    }
    
    private Vector2 ComputeRVOVelocity()
    {
        // 获取邻近的Agent
        List<SimpleRVOAgent> neighbors = GetNeighbors();
        
        if (neighbors.Count == 0)
        {
            // 没有邻近Agent，直接使用首选速度
            return _preferredVelocity;
        }
        
        // 构建速度障碍区域并寻找最佳速度
        Vector2 bestVelocity = FindBestVelocity(neighbors);
        return bestVelocity;
    }
    
    private List<SimpleRVOAgent> GetNeighbors()
    {
        List<SimpleRVOAgent> result = new List<SimpleRVOAgent>();
        
        foreach (var agent in _allAgents)
        {
            if (agent == this) continue;
            
            float distance = Vector2.Distance(transform.position, agent.transform.position);
            if (distance <= neighborDist)
            {
                result.Add(agent);
            }
        }
        
        return result;
    }
    
    private Vector2 FindBestVelocity(List<SimpleRVOAgent> neighbors)
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
    
        Vector2 bestVelocity = _preferredVelocity; // 初始值设为首选速度
        float bestScore = float.MaxValue;
    
        int sampleCount = 50; // 可以减少采样数，但提高质量
        // 1. 首先评估当前速度。如果当前速度表现良好，尽量保持，可以增强稳定性。
        float currentScore = EvaluateVelocity(_currentVelocity, currentPos, neighbors);
        if (currentScore < bestScore)
        {
            bestScore = currentScore;
            bestVelocity = _currentVelocity;
        }

        // 2. 大部分样本围绕“理想方向”采样（结合首选速度和当前速度）
        for (int i = 0; i < sampleCount; i++)
        {
            Vector2 baseDirection;
            float baseSpeed;

            // 大部分样本围绕首选速度或当前速度方向采样
            if (i < sampleCount * 0.6f) // 60% 样本围绕首选速度
            {
                baseDirection = _preferredVelocity.normalized;
                baseSpeed = _preferredVelocity.magnitude;
            }
            else if (i < sampleCount * 0.9f) // 30% 样本围绕当前速度
            {
                baseDirection = _currentVelocity.normalized;
                baseSpeed = _currentVelocity.magnitude;
            }
            else // 10% 样本完全随机，用于跳出可能的局部最优
            {
                baseDirection = Random.insideUnitCircle.normalized;
                baseSpeed = Random.Range(0f, maxSpeed);
            }

            // 在一个有限的角度范围内（例如±90度）进行随机偏移，而不是360度
            float randomAngle = Random.Range(-90f, 90f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                baseDirection.x * Mathf.Cos(randomAngle) - baseDirection.y * Mathf.Sin(randomAngle),
                baseDirection.x * Mathf.Sin(randomAngle) + baseDirection.y * Mathf.Cos(randomAngle)
            ).normalized;

            // 速度大小也可以在基础速度附近随机
            float speedVariation = Random.Range(0.5f, 1.5f);
            float speed = Mathf.Clamp(baseSpeed * speedVariation, 0, maxSpeed);

            Vector2 candidateVelocity = dir * speed;
            float score = EvaluateVelocity(candidateVelocity, currentPos, neighbors);

            if (score < bestScore)
            {
                bestScore = score;
                bestVelocity = candidateVelocity;
            }
        }
        return bestVelocity;
    }
    private float EvaluateVelocity(Vector2 candidateVelocity, Vector2 currentPos, List<SimpleRVOAgent> neighbors)
    {
        float score = 0f;
        
        // 1. 首选速度偏离惩罚（希望尽量保持向右移动）
        float preferencePenalty = Vector2.Distance(candidateVelocity, _preferredVelocity);
        score += preferencePenalty * 2f; // 权重可以调整
        
        // 2. 碰撞风险惩罚
        foreach (var neighbor in neighbors)
        {
            Vector2 neighborPos = new Vector2(neighbor.transform.position.x, neighbor.transform.position.y);
            Vector2 neighborVelocity = neighbor.GetCurrentVelocity();
            
            float collisionPenalty = CalculateCollisionPenalty(candidateVelocity, currentPos, 
                                                             neighborVelocity, neighborPos, 
                                                             neighbor.radius);
            score += collisionPenalty;
        }
        
        return score;
    }
    
    private float CalculateCollisionPenalty(Vector2 candidateVelocity, Vector2 currentPos,
        Vector2 neighborVelocity, Vector2 neighborPos,
        float neighborRadius)
    {
        // RVO核心：相互避让 - 双方各承担一半责任
        // 计算一个“共同”的速度，用于预测邻居的未来位置，模拟相互避让
        Vector2 perceivedNeighborVelocity = 0.5f * (candidateVelocity + neighborVelocity);

        float combinedRadius = radius + neighborRadius;
        float collisionPenalty = 0f;

        int steps = 10;
        for (int i = 1; i <= steps; i++)
        {
            float t = (timeHorizon / steps) * i;
            // 使用candidateVelocity预测自身未来位置
            Vector2 futurePos1 = currentPos + candidateVelocity * t;
            // 使用 perceivedNeighborVelocity（共同速度）来预测邻居的未来位置，体现相互避让
            Vector2 futurePos2 = neighborPos + perceivedNeighborVelocity * t;
            float distance = Vector2.Distance(futurePos1, futurePos2);

            // 如果预测会碰撞，则根据穿透深度施加惩罚
            if (distance < combinedRadius)
            {
                float penetrationDepth = combinedRadius - distance;
                // 惩罚系数可以调整，确保碰撞惩罚远大于速度偏好惩罚
                collisionPenalty += 1000f * (penetrationDepth / combinedRadius) * (timeHorizon / t);
            }
        }
        return collisionPenalty;
    }
    
    public Vector2 GetCurrentVelocity()
    {
        return _currentVelocity;
    }
    
    void OnDestroy()
    {
        _allAgents.Remove(this);
    }
    
    // 可视化调试
    void OnDrawGizmos()
    {
        // 绘制Agent半径
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
        
        // 绘制当前速度方向
        Gizmos.color = Color.red;
        Vector3 endPoint = transform.position + (Vector3)_currentVelocity * 0.5f;
        Gizmos.DrawLine(transform.position, endPoint);
        
        // 绘制感知范围
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawWireSphere(transform.position, neighborDist);
    }
}