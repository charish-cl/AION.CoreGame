# Pool 使用示例

## 改造说明

1. **使用工厂函数注册**：每个对象类型需要注册工厂函数来创建对象
2. **支持根据 key 加载不同资源**：工厂函数可以根据参数动态加载不同的资源
3. **Pool 迁移到 ObjectPoolModule**：提供静态扩展方法，使用更方便

## 使用示例

### 1. 定义 ObjectBase 子类

```csharp
public class HPBarLogic : ObjectBase
{
    public GameObject HPBarPrefab;
    public Transform HeroTransform;
    private Slider m_Slider;
    
    public HPBarLogic(GameObject go = null, Transform heroTransform = null)
    {
        HPBarPrefab = go;
        HeroTransform = heroTransform;
        if (go)
        {
            m_Slider = go.GetComponent<Slider>();
        }
    }
    
    public void SetParent(Transform parent)
    {
        HeroTransform = parent;
    }
    
    public override void OnSpawn()
    {
        if (HPBarPrefab != null)
        {
            HPBarPrefab.gameObject.SetActive(true);
        }
    }

    public override void OnUnspawn()
    {
        if (HPBarPrefab != null)
        {
            HPBarPrefab.gameObject.SetActive(false);
        }
    }
    
    public void Init(float HP)
    {
        if (m_Slider != null)
        {
            m_Slider.maxValue = HP;
            m_Slider.value = HP;
        }
    }
    
    public void SetHp(float currentHp)
    {
        if (m_Slider != null)
        {
            m_Slider.value = currentHp;
        }
    }
}
```

### 2. 注册工厂函数（在初始化时）

```csharp
// 在 HPBarLogicSystem.OnInit 中注册
public override bool OnInit()
{
    HPBarParent = ActorMgr.Instance.SceneBehavior.HPBarCanvas.transform;
    
    // 注册工厂函数
    Pool.RegisterFactory<HPBarLogic>(() =>
    {
        GameObject hpBarPrefab = GameModule.Resource.LoadAsset<GameObject>("Assets/Game/UIComponent/HPBar.prefab");
        GameObject hpBarGameObject = GameObject.Instantiate(hpBarPrefab, HPBarParent);
        hpBarGameObject.name = "HPBar";
        return new HPBarLogic(hpBarGameObject, null);
    });
    
    return base.OnInit();
}
```

### 3. 使用 Pool.Get<T>() 和 Pool.Release<T>()

```csharp
// 在 HPBarCmp 中使用
public class HPBarCmp : GameActorCmp
{
    HPBarLogic hpBarLogic;
    
    public override void OnInit()
    {
        base.OnInit();
        
        // 使用 Pool.Get<T>() 获取对象（自动使用注册的工厂）
        hpBarLogic = Pool.Get<HPBarLogic>("HPBar");
        
        // 设置父节点
        hpBarLogic.SetParent(Actor.Transform);
        
        // 初始化
        var numericComponent = Actor.GetComponent<NumericComponent>();
        hpBarLogic.Init(numericComponent.GetAsInt(NumericType.Hp));
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        // 使用 Pool.Release<T>() 释放对象
        Pool.Release(hpBarLogic);
    }
}
```

### 4. 根据 key 加载不同资源（示例：Bullet）

```csharp
// 定义 BulletLogic
public class BulletLogic : ObjectBase
{
    public GameObject BulletPrefab;
    public int BulletId;
    
    public BulletLogic(GameObject go, int bulletId)
    {
        BulletPrefab = go;
        BulletId = bulletId;
    }
}

// 注册工厂函数（支持根据 key 加载不同资源）
public void RegisterBulletFactory()
{
    // 方式1：使用闭包捕获 key
    for (int bulletId = 1; bulletId <= 10; bulletId++)
    {
        int id = bulletId; // 避免闭包问题
        Pool.RegisterFactory<BulletLogic>(() =>
        {
            // 根据 bulletId 加载不同的资源
            string path = $"Assets/Game/Bullets/Bullet_{id}.prefab";
            GameObject prefab = GameModule.Resource.LoadAsset<GameObject>(path);
            GameObject bullet = GameObject.Instantiate(prefab);
            return new BulletLogic(bullet, id);
        });
    }
    
    // 方式2：在 Get 时传入工厂函数（更灵活）
    // 见下面的使用示例
}

// 使用方式2：在 Get 时传入工厂函数
public BulletLogic GetBullet(int bulletId)
{
    return Pool.Get<BulletLogic>($"Bullet_{bulletId}", () =>
    {
        // 根据 bulletId 动态加载资源
        string path = $"Assets/Game/Bullets/Bullet_{bulletId}.prefab";
        GameObject prefab = GameModule.Resource.LoadAsset<GameObject>(path);
        GameObject bullet = GameObject.Instantiate(prefab);
        return new BulletLogic(bullet, bulletId);
    });
}
```

### 5. 简化 HPBarLogicSystem（仅保留 Update 逻辑）

```csharp
public class HPBarLogicSystem : BaseLogicSys<HPBarLogicSystem>
{
    public Transform HPBarParent;
    
    public override bool OnInit()
    {
        HPBarParent = ActorMgr.Instance.SceneBehavior.HPBarCanvas.transform;
        
        // 注册工厂函数
        Pool.RegisterFactory<HPBarLogic>(() =>
        {
            GameObject hpBarPrefab = GameModule.Resource.LoadAsset<GameObject>("Assets/Game/UIComponent/HPBar.prefab");
            GameObject hpBarGameObject = GameObject.Instantiate(hpBarPrefab, HPBarParent);
            hpBarGameObject.name = "HPBar";
            return new HPBarLogic(hpBarGameObject, null);
        });
        
        return base.OnInit();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        
        // 使用 Pool.GetPool<T>() 访问对象池内部数据
        var pool = Pool.GetPool<HPBarLogic>();
        foreach (var keyValuePair in pool.objMap)
        {
            var hpBar = keyValuePair.Value;
            if (hpBar != null && hpBar.m_Object != null)
            {
                hpBar.m_Object.SynPos();
            }
        }
    }
}
```

## 优势

1. **代码更简洁**：无需为每个对象类型创建 System 类
2. **灵活的资源加载**：可以根据 key 动态加载不同的资源
3. **统一接口**：所有对象池使用统一的 `Pool.Get<T>()` 和 `Pool.Release<T>()`
4. **灵活配置**：支持自定义工厂和对象池配置

## 注意事项

1. **必须注册工厂函数**：在使用 `Pool.Get<T>()` 前，必须先注册工厂函数（或在 Get 时传入）
2. **工厂函数职责**：工厂函数负责加载资源和创建对象实例
3. **根据 key 加载**：如果同一个类型需要加载不同资源，可以在 Get 时传入工厂函数，或使用不同的 name
