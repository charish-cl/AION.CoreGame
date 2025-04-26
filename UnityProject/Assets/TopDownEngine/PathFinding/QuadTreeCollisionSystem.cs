namespace TopDownEngine.PathFinding
{
   using UnityEngine;
using System.Collections.Generic;

public class QuadTreeCollisionSystem : MonoBehaviour
{
    [Header("四叉树参数")]
    public Rect bounds = new Rect(0, 0, 100, 100);
    public int maxObjectsPerNode = 4;
    public int maxDepth = 5;

    [Header("调试绘制")]
    public bool drawTree = true;
    public bool drawColliders = true;

    private QuadTree tree;
    private List<FCollider> allColliders = new List<FCollider>();

    void Start()
    {
        tree = new QuadTree(0, bounds, maxObjectsPerNode, maxDepth);
    }

    void Update()
    {
        // 重建四叉树（动态物体需要每帧更新）
        tree.Clear();
        foreach (var collider in allColliders)
        {
            if (collider.isActiveAndEnabled)
                tree.Insert(collider);
        }

        // 检测所有碰撞
        for (int i = 0; i < allColliders.Count; i++)
        {
            if (!allColliders[i].isActiveAndEnabled) continue;

            var candidates = tree.GetPotentialColliders(allColliders[i]);
            foreach (var other in candidates)
            {
                if (other != allColliders[i] && CheckCollision(allColliders[i], other))
                {
                    // 碰撞事件处理
                    Debug.Log($"{allColliders[i].name} 与 {other.name} 发生碰撞");
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (drawTree) tree.DrawGizmos();

        if (drawColliders)
        {
            foreach (var collider in allColliders)
            {
                if (collider.isActiveAndEnabled)
                {
                    Gizmos.color = collider.isColliding ? Color.red : Color.green;
                    if (collider is FCCircleollider circle)
                    {
                        Gizmos.DrawWireSphere(circle.center, circle.radius);
                    }
                    else if (collider is FBoxCollider box)
                    {
                        Gizmos.DrawWireCube(box.center, box.size);
                    }
                }
            }
        }
    }

    #region 碰撞检测核心
    private bool CheckCollision(FCollider a, FCollider b)
    {
        // AABB快速排除
        if (!RectOverlap(a.GetAABB(), b.GetAABB())) return false;

        // 精确检测
        if (a is FCCircleollider circleA)
        {
            if (b is FCCircleollider circleB)
                return CircleCircle(circleA, circleB);
            else if (b is FBoxCollider boxB)
                return CircleBox(circleA, boxB);
        }
        else if (a is FBoxCollider boxA)
        {
            if (b is FCCircleollider circleB)
                return CircleBox(circleB, boxA);
            else if (b is FBoxCollider boxB)
                return BoxBox(boxA, boxB);
        }

        return false;
    }

    private bool RectOverlap(Rect a, Rect b)
    {
        return !(a.xMax < b.xMin || a.xMin > b.xMax || a.yMax < b.yMin || a.yMin > b.yMax);
    }

    private bool CircleCircle(FCCircleollider a, FCCircleollider b)
    {
        float distance = Vector2.Distance(a.center, b.center);
        return distance < (a.radius + b.radius);
    }

    private bool CircleBox(FCCircleollider circleF, FBoxCollider fBox)
    {
        Vector2 closest = new Vector2(
            Mathf.Clamp(circleF.center.x, fBox.min.x, fBox.max.x),
            Mathf.Clamp(circleF.center.y, fBox.min.y, fBox.max.y)
        );
        float distance = Vector2.Distance(circleF.center, closest);
        return distance < circleF.radius;
    }

    private bool BoxBox(FBoxCollider a, FBoxCollider b)
    {
        return RectOverlap(a.GetAABB(), b.GetAABB());
    }
    #endregion

    #region 四叉树实现
    private class QuadTree
    {
        private int level;
        private Rect bounds;
        private int maxObjects;
        private int maxLevels;
        private List<FCollider> objects;
        private QuadTree[] nodes;

        public QuadTree(int level, Rect bounds, int maxObjects, int maxLevels)
        {
            this.level = level;
            this.bounds = bounds;
            this.maxObjects = maxObjects;
            this.maxLevels = maxLevels;
            objects = new List<FCollider>();
            nodes = new QuadTree[4];
        }

        public void Clear()
        {
            objects.Clear();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                {
                    nodes[i].Clear();
                    nodes[i] = null;
                }
            }
        }

        public void Insert(FCollider fCollider)
        {
            if (nodes[0] != null)
            {
                int index = GetIndex(fCollider.GetAABB());
                if (index != -1)
                {
                    nodes[index].Insert(fCollider);
                    return;
                }
            }

            objects.Add(fCollider);

            if (objects.Count > maxObjects && level < maxLevels)
            {
                if (nodes[0] == null) Split();

                for (int i = objects.Count - 1; i >= 0; i--)
                {
                    int index = GetIndex(objects[i].GetAABB());
                    if (index != -1)
                    {
                        nodes[index].Insert(objects[i]);
                        objects.RemoveAt(i);
                    }
                }
            }
        }

        public List<FCollider> GetPotentialColliders(FCollider fCollider)
        {
            List<FCollider> result = new List<FCollider>();
            GetPotentialColliders(fCollider.GetAABB(), ref result);
            return result;
        }

        private void GetPotentialColliders(Rect rect, ref List<FCollider> result)
        {
            int index = GetIndex(rect);
            if (index != -1 && nodes[0] != null)
            {
                nodes[index].GetPotentialColliders(rect, ref result);
            }

            result.AddRange(objects);
        }

        private int GetIndex(Rect rect)
        {
            int index = -1;
            float verticalMidpoint = bounds.x + bounds.width / 2;
            float horizontalMidpoint = bounds.y + bounds.height / 2;

            bool topQuadrant = rect.y > horizontalMidpoint;
            bool bottomQuadrant = rect.y < horizontalMidpoint && rect.y + rect.height < horizontalMidpoint;

            if (rect.x > verticalMidpoint && rect.x + rect.width < bounds.x + bounds.width)
            {
                if (topQuadrant) index = 1;
                else if (bottomQuadrant) index = 2;
            }
            else if (rect.x < verticalMidpoint && rect.x + rect.width < verticalMidpoint)
            {
                if (topQuadrant) index = 0;
                else if (bottomQuadrant) index = 3;
            }

            return index;
        }

        private void Split()
        {
            float subWidth = bounds.width / 2;
            float subHeight = bounds.height / 2;
            float x = bounds.x;
            float y = bounds.y;

            nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight), maxObjects, maxLevels);
            nodes[1] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight), maxObjects, maxLevels);
            nodes[2] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight), maxObjects, maxLevels);
            nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight), maxObjects, maxLevels);
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(bounds.center.x, bounds.center.y, 0), 
                               new Vector3(bounds.width, bounds.height, 1));

            if (nodes[0] != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i].DrawGizmos();
                }
            }
        }
    }
    #endregion

    #region 碰撞体基类
    public abstract class FCollider : MonoBehaviour
    {
        [HideInInspector] public bool isColliding;
        public abstract Rect GetAABB();
    }

    public class FCCircleollider : FCollider
    {
        public float radius = 1;
        public Vector2 center => transform.position;

        public override Rect GetAABB()
        {
            return new Rect(
                center.x - radius,
                center.y - radius,
                radius * 2,
                radius * 2
            );
        }
    }

    public class FBoxCollider : FCollider
    {
        public Vector2 size = Vector2.one;
        public Vector2 center => transform.position;
        public Vector2 min => center - size / 2;
        public Vector2 max => center + size / 2;

        public override Rect GetAABB()
        {
            return new Rect(min.x, min.y, size.x, size.y);
        }
    }
    #endregion
}
}