# 对象池改造建议

## 问题分析

当前对象池系统存在以下问题：
1. **代码重复**：每次创建新对象类型都需要：
   - 创建 Logic 类（继承 ObjectBase）
   - 创建 System 类（管理对象池）
   - 写 CreateXXX 和 ReleaseXXX 方法
2. **使用不便**：不能直接使用 `Pool.Get<T>()` 和 `Pool.Release<T>()` 这样的泛型调用

## 解决方案

### 1. 创建通用的 `Pool` 静态类

已创建 `Assets/OldScript/Module/ObjectModule/Pool.cs`，提供：
- `Pool.Get<T>(string name, Func<T> factory = null)` - 自动获取或创建对象
- `Pool.Release<T>(T obj)` - 释放对象
- `Pool.RegisterFactory<T>(Func<T> factory)` - 注册工厂函数
- `Pool.RegisterConfig<T>(PoolConfig config)` - 注册对象池配置

### 2. 使用方式对比

#### 旧方式（需要 System 类）：
```csharp
// 需要创建 HPBarLogicSystem
public class HPBarLogicSystem : BaseLogicSys<HPBarLogicSystem>
{
    private ObjectPool<HPBarLogic> HPBarPool;
    
    public HPBarLogic CreateHPBar(Transform heroTransform)
    {
        var hpBar = HPBarPool.Spawn("HPBar");
        if (hpBar == null)
        {
            // 创建逻辑...
            hpBar = new HPBarLogic(hpBarGameObject, heroTransform);
            HPBarPool.Register(hpBar);
        }
        return hpBar;
    }
    
    public void ReleaseHPBar(HPBarLogic hpBarLogic)
    {
        HPBarPool.UnSpawn(hpBarLogic);
    }
}

// 使用
hpBarLogic = HPBarLogicSystem.Instance.CreateHPBar(Actor.Transform);
HPBarLogicSystem.Instance.ReleaseHPBar(hpBarLogic);
```

#### 新方式（直接使用 Pool）：
```csharp
// 1. 初始化时注册工厂（可选，只需一次）
Pool.RegisterFactory<HPBarLogic>(() =>
{
    GameObject hpBarPrefab = GameModule.Resource.LoadAsset<GameObject>("Assets/Game/UIComponent/HPBar.prefab");
    Transform hpBarParent = ActorMgr.Instance.SceneBehavior.HPBarCanvas.transform;
    GameObject hpBarGameObject = GameObject.Instantiate(hpBarPrefab, hpBarParent);
    hpBarGameObject.name = "HPBar";
    return new HPBarLogic(hpBarGameObject, null);
});

// 2. 使用（无需 System 类）
hpBarLogic = Pool.Get<HPBarLogic>("HPBar");
hpBarLogic.SetParent(Actor.Transform);
Pool.Release(hpBarLogic);
```

### 3. 改造步骤

#### 步骤1：保留 HPBarLogicSystem（仅用于 Update 逻辑）

如果 `HPBarLogicSystem` 有 `OnUpdate` 逻辑（如 `SynPos`），可以保留但简化：

```csharp
public class HPBarLogicSystem : BaseLogicSys<HPBarLogicSystem>
{
    public override bool OnInit()
    {
        // 注册工厂
        Pool.RegisterFactory<HPBarLogic>(() =>
        {
            GameObject hpBarPrefab = GameModule.Resource.LoadAsset<GameObject>("Assets/Game/UIComponent/HPBar.prefab");
            Transform hpBarParent = ActorMgr.Instance.SceneBehavior.HPBarCanvas.transform;
            GameObject hpBarGameObject = GameObject.Instantiate(hpBarPrefab, hpBarParent);
            hpBarGameObject.name = "HPBar";
            return new HPBarLogic(hpBarGameObject, null);
        });
        return base.OnInit();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        // 更新所有 HPBar 位置（如果需要）
        var pool = Pool.GetOrCreatePool<HPBarLogic>(); // 需要添加这个方法
        foreach (var keyValuePair in pool.objMap)
        {
            var hpBar = keyValuePair.Value;
            if (hpBar != null)
            {
                hpBar.m_Object.SynPos();
            }
        }
    }
}
```

#### 步骤2：修改 HPBarCmp 使用新的 Pool

```csharp
public class HPBarCmp : GameActorCmp
{
    HPBarLogic hpBarLogic;
    
    public override void OnInit()
    {
        base.OnInit();
        Actor.EventDispatcher.AddEventListener<NumericType, float, float>(IActorEvent_Event.NumbericChange, OnNumbericChange, this);
        
        // 使用新的 Pool.Get<T>()
        hpBarLogic = Pool.Get<HPBarLogic>("HPBar");
        hpBarLogic.SetParent(Actor.Transform);
        
        var numericComponent = Actor.GetComponent<NumericComponent>();
        hpBarLogic.Init(numericComponent.GetAsInt(NumericType.Hp));
    }

    private void OnNumbericChange(NumericType arg1, float oldValue, float newValue)
    {
        if (arg1 == NumericType.Hp)
        {
            hpBarLogic.SetHp(newValue);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // 使用新的 Pool.Release<T>()
        Pool.Release(hpBarLogic);
    }
}
```

### 4. 优势

1. **代码简洁**：无需为每个对象类型创建 System 类
2. **统一接口**：所有对象池使用统一的 `Pool.Get<T>()` 和 `Pool.Release<T>()`
3. **自动管理**：对象池自动创建和管理
4. **灵活配置**：支持工厂模式和配置注册

### 5. 注意事项

1. **工厂函数**：如果对象创建需要参数，可以在 `Pool.Get<T>()` 时提供工厂函数
2. **Update 逻辑**：如果对象需要每帧更新，可以保留 System 类但简化其职责
3. **向后兼容**：可以逐步迁移，新旧方式可以共存

### 6. 需要添加的方法

为了支持 `HPBarLogicSystem.OnUpdate` 中的逻辑，需要在 `Pool` 类中添加：

```csharp
/// <summary>
/// 获取对象池（用于访问 objMap 等内部数据）
/// </summary>
public static ObjectPool<T> GetPool<T>() where T : ObjectBase
{
    return GetOrCreatePool<T>();
}
```

