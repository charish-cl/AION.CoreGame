
# 优化方向
二叉树(堆)也即是单调队列 来替换list



```csharp
// 曼哈顿启发式距离
private int ManhattanDistance(AstarNode a, AstarNode b)
{
    int xDistance = Math.Abs(a.x - b.x);
    int yDistance = Math.Abs(a.y - b.y);
    return xDistance + yDistance;
}

// 对角线距离
private int DiagonalDistance(AstarNode a, AstarNode b)
{
    int xDistance = Math.Abs(a.x - b.x);
    int yDistance = Math.Abs(a.y - b.y);
    int remaining = Math.Abs(xDistance - yDistance);
    return MOVE_DIAGONAL_COST * Math.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
}

// 欧几里得距离
private int EuclideanDistance(AstarNode a, AstarNode b)
{
    int xDistance = a.x - b.x;
    int yDistance = a.y - b.y;
    return (int) Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
}
```

不同的距离评估方式对A*算法的结果有很大的影响。

1. 曼哈顿启发式距离：使用曼哈顿启发式距离时，A*算法可以在网格图中找到最短的曼哈顿距离，但是有可能存在一些曲折的路径。
2. 对角线距离：使用对角线距离时，A*算法可以在八向图中寻找最短路径，但是对角线移动的代价比直线移动高，因此它可能会选择一条不够直接的路径。
3. 欧几里得距离：使用欧几里得距离时，A*算法可以在任意维度上寻找最短路径，但是它比其他两种距离评估方式更加复杂，因此计算代价更高。

因此，选择不同的距离评估方式将对A*算法的结果产生重大影响，需要根据具体情况进行选择。
