
<h1>Mathf.Clamp01</h1>

Mathf.Clamp01是Unity中的一个数学函数，用于将一个浮点数限制在0到1之间。如果输入值小于0，则返回0，如果输入值大于1，则返回1，否则返回输入值本身。

下面是一个使用Mathf.Clamp01函数的示例代码：

csharp
float value = 1.5f;
float clampedValue = Mathf.Clamp01(value);

Debug.Log(clampedValue); //输出为1
在上面的示例中，我们将一个浮点数value赋值为1.5，但由于Mathf.Clamp01函数的作用，它被限制在了0到1之间，所以输出结果为1。

<h1>操作</h1>

给敌人挂上EnemyAI组件即可，这个组件是用来控制敌人的AI的，里面有很多参数，可以自己去看看，这里就不一一介绍了。

AIData是用来存储敌人的AI数据的,被Detect更新数据


    //SeekBehaviour, FleeBehaviour, WanderBehaviour, ObstacleAvoidanceBehaviour
            
