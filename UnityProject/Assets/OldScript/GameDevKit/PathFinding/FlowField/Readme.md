问题

容易被前面的节点卡住

![image-20230211160623851](C:\Users\CodeElk\AppData\Roaming\Typora\typora-user-images\image-20230211160623851.png)





```

public class Grid<T> : IEnumerable<T>
{
private T[,] cells;
public IEnumerator<T> GetEnumerator()
{
return (IEnumerator<T>)cells.GetEnumerator();
}
}为什么这样写有问题,转为泛型枚举器有问题吗
```

