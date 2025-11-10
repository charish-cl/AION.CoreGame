using System;
using System.Linq;

namespace GameLogic
{
    using UnityEngine;
    using System.Collections.Generic;

    public class QuadTreeCollisionSystem : MonoBehaviour
    {
        [Header("四叉树参数")] public Rect bounds = new Rect(0, 0, 100, 100);
        public int maxObjectsPerNode = 4;
        public int maxDepth = 5;

        [Header("调试绘制")] public bool drawTree = true;
        public bool drawColliders = true;

        private QuadTree tree;
        private List<FCollider> allColliders = new List<FCollider>();

        public void Build(Grid grid)
        {
            
        }

        void Start()
        {
            allColliders = new List<FCollider>();
            
            
            //随机生成100个碰撞器
            // for (int i = 0; i < 100; i++)
            // {
            //     var collider = new GameObject("Collider").AddComponent<FBoxCollider>(); 
            //     collider.transform.position = new Vector3(
            //         UnityEngine.Random.Range(0, 100),
            //         UnityEngine.Random.Range(0, 100),
            //         0
            //     );
            //     var size = UnityEngine.Random.Range(5, 10);
            //     collider.size = size * Vector2.one;
            //     collider.transform.parent = transform;
            //     allColliders.Add(collider);  
            // }
            //
            for (int i = 0; i < 10; i++)
            {
                var collider = new GameObject("Collider").AddComponent<FBoxCollider>(); 
                collider.transform.position = new Vector3(
                    UnityEngine.Random.Range(0, 10),
                    UnityEngine.Random.Range(0, 10),
                    0
                );
                var size = UnityEngine.Random.Range(1, 4);
                collider.size = size * Vector2.one;
                collider.transform.parent = transform;
                allColliders.Add(collider);  
            }

            foreach (var collider in FindObjectsOfType<FCollider>())
            {
                allColliders.Add(collider);
            }

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
                
                bool hasCollided = false;
                foreach (var other in candidates)
                {
                    if (other != allColliders[i] && CheckCollision(allColliders[i], other))
                    {
                        hasCollided = true;
                        other.isColliding = true;
                        // 碰撞事件处理
                        Debug.Log($"{allColliders[i].name} 与 {other.name} 发生碰撞");
                    }
                }
                allColliders[i].isColliding = hasCollided;
            }
        }

        private void FixedUpdate()
        {
            return;
            // 物理模拟随机运动
            foreach (var collider in allColliders)
            {
                if (collider.isActiveAndEnabled)
                {
                    collider.transform.position += new Vector3(
                        UnityEngine.Random.Range(-1, 2),
                        UnityEngine.Random.Range(-1, 2),
                        0
                    ) * (Time.deltaTime * 1);
                }
            }   
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (tree == null)
            {
                return;
            }
            if (drawTree) tree.DrawGizmos();

            if (drawColliders)
            {
                foreach (var collider in allColliders)
                {
                    if (collider.isActiveAndEnabled)
                    {
                        Gizmos.color = collider.isColliding ? Color.red : Color.green;
                        if (collider is FCircleollider circle)
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
            if (a is FCircleollider circleA)
            {
                if (b is FCircleollider circleB)
                    return CircleCircle(circleA, circleB);
                else if (b is FBoxCollider boxB)
                    return CircleBox(circleA, boxB);
            }
            else if (a is FBoxCollider boxA)
            {
                if (b is FCircleollider circleB)
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

        private bool CircleCircle(FCircleollider a, FCircleollider b)
        {
            float distance = Vector2.Distance(a.center, b.center);
            return distance < (a.radius + b.radius);
        }

        private bool CircleBox(FCircleollider circleF, FBoxCollider fBox)
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
            private Vector2 center => bounds.center;
            private bool HasSplit => nodes[0] != null;
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
                if (HasSplit)
                {  
                    // 获取物体可能属于的所有子节点索引
                    List<int> indices = GetIndices(fCollider.GetAABB());
                    
                    foreach (int index in indices)
                    {
                        if (index != -1)
                        {
                            // 如果能放入子节点，则插入并返回
                            nodes[index].Insert(fCollider);
                            return;
                        }
                    }
                }

                objects.Add(fCollider);
                //大于指定数量时，分裂四叉树
                if (objects.Count > maxObjects && level < maxLevels)
                {
                    if (nodes[0] == null) Split();

                    // 分裂后，需要将当前节点的物体重新分配到子节点中（尝试下放）
                    for (int i = objects.Count - 1; i >= 0; i--)
                    {
                        FCollider obj = objects[i];
                        List<int> objIndices = GetIndices(obj.GetAABB());
                        foreach (int idx in objIndices)
                        {
                            if (idx != -1)
                            {
                                nodes[idx].Insert(obj);
                            }
                        }
                       
                    }
                    objects.Clear();
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
                if (HasSplit)
                {
                    List<int> indices = GetIndices(rect);
                    foreach (int index in indices)
                    {
                        if (index != -1)
                        {
                            nodes[index].GetPotentialColliders(rect, ref result);
                        }
                    }
                }   
                
                result.AddRange(objects);
            }

            /// <summary>
            /// 获取一个矩形区域所属的所有子节点索引列表
            /// </summary>
            /// <param name="rect">要检查的矩形区域</param>
            /// <returns>子节点索引列表，如果不属于任何子节点或属于多个，返回包含多个索引的列表；如果完全不属于任何子节点（应在父节点），返回空列表。</returns>
            private List<int> GetIndices(Rect rect)
            {
                HashSet<int> indices = new HashSet<int>();
                
                //获取矩形四个点的象限索引
                int topLeft = GetPointIndex(new Vector2(rect.xMin, rect.yMax), center);
                int topRight = GetPointIndex(new Vector2(rect.xMax, rect.yMax), center);
                int bottomLeft = GetPointIndex(new Vector2(rect.xMin, rect.yMin), center);
                int bottomRight = GetPointIndex(new Vector2(rect.xMax, rect.yMin), center);
                
                indices.Add(topLeft);
                indices.Add(topRight);
                indices.Add(bottomLeft);                
                indices.Add(bottomRight);
                

                return indices.ToList();
            }

            //判断一个点在第几象限，右上角是0，逆时针
            private int GetPointIndex(Vector2 point, Vector2 center)
            {
                float x = point.x - center.x;
                float y = point.y - center.y;
                if (x >= 0 && y >= 0)
                {
                    return 0;
                }
                else if (x < 0 && y >= 0)
                {
                    return 1;
                }
                else if (x < 0 && y < 0)
                {
                    return 2;
                }
                else if (x >= 0 && y < 0)
                {
                    return 3;
                }
                else
                {
                    return -1;
                }
            }
            

            private void Split()
            {
                float subWidth = bounds.width / 2;
                float subHeight = bounds.height / 2;
                float x = bounds.x;
                float y = bounds.y;

                //左下角为中心 右上 左上 左下 右下 逆时针
                nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight),
                    maxObjects, maxLevels);
                nodes[1] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight), maxObjects,
                    maxLevels);
                nodes[2] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight), maxObjects, maxLevels);
                
                nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight), maxObjects,
                    maxLevels);
         
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
    }
}