namespace GameDevKit.PathFinding.FlowField
{
    using System.Collections.Generic;
    using System.Text;
    using Sirenix.OdinInspector;
    using UnityEngine;

    class FlowField : SerializedMonoBehaviour
    {
        public enum EnumDirection
        {
            Left,
            Right,
            Up,
            Down,
            None
        }

        Vector2[] directions = new Vector2[4]
        {
            new Vector2(0, -1),
            new Vector2(0, 1),
            new Vector2(-1, 0),
            new Vector2(1, 0),
        };


        int[] reverseDirectionIndex = new int[4]
        {
            1,
            0,
            3,
            2
        };


        public class Node
        {
            //从哪个节点开始的
            public int ParentStartIndex = -1;

            public Node prev;

            public EnumDirection direction = EnumDirection.None;

            public int NodeState;
            public int x;
            public int y;

            public bool IsTower;

            public Node()
            {
            }

            public Node(int x, int y, int nodeState)
            {
                NodeState = nodeState;
                this.x = x;
                this.y = y;
            }
        }


        public const int NODE_TOWER = 3;
        public const int NODE_EMPTY = 0;
        public const int NODE_BLOCK = 1;

        Node[][] GridsNodes;
        int xNUm;
        int yNum;

        public int XNum
        {
            get { return xNUm; }
        }

        public int YNum
        {
            get { return yNum; }
        }

        public Node this[int x, int y]
        {
            get { return GridsNodes[x][y]; }
        }

        public void Init(int x = 10, int y = 10, int step = -1)
        {
            xNUm = x;
            yNum = y;

            ResetGrid();

            TowersNodes.Clear();
        }

        
        

        // TODO : 塔边缘相交处有问题，明天看看

        #region 建筑相关

        //建筑点
        List<Node> TowersNodes = new List<Node>();

        //塔边缘区域
        Dictionary<int, List<Node>> TowerBoundNodes = new Dictionary<int, List<Node>>();

        public void AddTowerBoundNodes(int towerIndex, Node nodes)
        {
            if (!TowerBoundNodes.ContainsKey(towerIndex))
            {
                TowerBoundNodes[towerIndex] = new List<Node>();
            }

            TowerBoundNodes[towerIndex].Add(nodes);
        }

        //随机建筑点
        public void RandomTowerPoint(int cnt)
        {
            TowersNodes.Clear();
            for (int i = 0; i < cnt; i++)
            {
                int randX = Random.Range(0, xNUm);
                int randY = Random.Range(0, yNum);
                CreatedTower(randX, randY);
            }
        }

        public void CreatedTower(int x, int y)
        {
            GridsNodes[x][y].NodeState = NODE_TOWER;
            GridsNodes[x][y].ParentStartIndex = TowersNodes.Count;
            GridsNodes[x][y].x = x;
            GridsNodes[x][y].y = y;
            GridsNodes[x][y].IsTower = true;
            TowersNodes.Add(GridsNodes[x][y]);
        }

        public void CreateTowers(Vector2Int[] points)
        {
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                CreatedTower(point.x, point.y);
            }
        }

        public void MultiBfs(int step = -1)
        {
            MultiBFS(TowersNodes,step);
        }

        
        [Button]
        //移除建筑点
        public void RemoveTower(int towerIndex)
        {
            Node towerNode = TowersNodes[towerIndex];
            //重新对塔边缘区域进行BFS
            TowersNodes.Remove(towerNode);

            List<Node> towerBoundNodes = TowerBoundNodes[towerIndex];

            if (towerBoundNodes != null && towerBoundNodes.Count > 0)
            {
                MultiBFS(TowersNodes);
            }

            TowerBoundNodes.Remove(towerIndex);
        }

        #endregion

        public bool IsCanWalk(int x, int y)
        {
            return GridsNodes[x][y].NodeState == NODE_EMPTY;
        }

        public bool IsOutBound(int x, int y)
        {
            return x < 0 || x >= xNUm || y < 0 || y >= yNum;
        }

        public void MultiBFS(List<Node> towerNodes, int step = -1)
        {
            Queue<Vector2>[] queues = new Queue<Vector2>[towerNodes.Count];
            for (int i = 0; i < towerNodes.Count; i++)
            {
                queues[i] = new Queue<Vector2>();
                queues[i].Enqueue(new Vector2(towerNodes[i].x, towerNodes[i].y));
                GridsNodes[towerNodes[i].x][towerNodes[i].y].NodeState = NODE_BLOCK;
            }

            bool IsAllQueueEmpty()
            {
                for (int i = 0; i < towerNodes.Count; i++)
                {
                    if (queues[i].Count > 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            bool HasStep()
            {
                if (step < 0)
                {
                    return true;
                }

                return step-- > 0;
            }

            while (!IsAllQueueEmpty() && HasStep())
            {
                for (int i = 0; i < towerNodes.Count; i++)
                {
                    if (queues[i].Count > 0)
                    {
                        Vector2 current = queues[i].Dequeue();
                        int currentX = (int)current.x;
                        int currentY = (int)current.y;
                        for (int j = 0; j < 4; j++)
                        {
                            int newX = currentX + (int)directions[j].x;
                            int newY = currentY + (int)directions[j].y;
                            if (IsOutBound(newX, newY))
                            {
                                continue;
                            }

                            Node newNode = GridsNodes[newX][newY];
                            if (!IsCanWalk(newX, newY))
                            {
                                if (newNode.ParentStartIndex != -1 && newNode.ParentStartIndex !=
                                    GridsNodes[currentX][currentY].ParentStartIndex)
                                {
                                    //塔边缘区域
                                    AddTowerBoundNodes(newNode.ParentStartIndex, newNode);
                                }

                                continue;
                            }

                            if (newNode.NodeState == NODE_EMPTY)
                            {
                                newNode.NodeState = NODE_BLOCK;
                                //反方向
                                newNode.direction = (EnumDirection)reverseDirectionIndex[j];
                                queues[i].Enqueue(new Vector2(newX, newY));
                                towerNodes[i].ParentStartIndex = newNode.ParentStartIndex;
                            }
                        }
                    }
                }
            }
        }


        public void ResetGrid()
        {
            GridsNodes = new Node[xNUm][];
            //全部设置为0
            for (int i = 0; i < xNUm; i++)
            {
                GridsNodes[i] = new Node[yNum];
                for (int j = 0; j < yNum; j++)
                {
                    GridsNodes[i][j] = new Node(i, j, NODE_EMPTY);
                }
            }
        }

       
    }
}