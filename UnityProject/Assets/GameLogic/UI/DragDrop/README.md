# 拖拽系统架构说明

## 概述

拖拽系统采用三层架构设计，职责清晰，易于维护和扩展。

## 架构层次

### 1. 事件接收层 - `DragDropEventHandler`
**职责**：只负责接收Unity拖拽事件，转换为通用事件

- 实现 `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`
- 处理UI拖拽的视觉效果（透明度、位置等）
- 发出通用事件：`OnBeginDragEvent`, `OnDragEvent`, `OnEndDragEvent`
- **不关心业务逻辑，不关心视图更新**

### 2. 视图绑定层 - `DragDropViewBinder` (实现 `IDragDropView`)
**职责**：实现视图接口，只负责视图更新

- 实现 `IDragDropView` 接口
- 管理网格显示/隐藏
- 管理高亮显示
- 接口方法：
  - `OnDragBegin` - 开始拖拽
  - `OnDragUpdate` - 拖拽中（更新高亮）
  - `OnDragEnd` - 拖拽结束
  - `OnPlaceSuccess` - 放置成功
  - `OnPlaceFailed` - 放置失败
- **不关心业务逻辑，只负责视图渲染**

### 3. 逻辑处理层 - `DragDropLogicHandler`
**职责**：将Logic层与Action绑定，只负责业务逻辑

- 持有 `IDragDropView` 接口引用（不直接依赖具体实现）
- 处理拖拽项注册
- 计算高亮位置
- 检查是否可以放置
- 执行放置逻辑
- 通过接口调用视图层方法（完全解耦）
- 提供逻辑层Action供外部自定义：
  - `OnCalculateHighlight` - 计算高亮（可自定义）
  - `OnCheckCanPlace` - 检查可放置性（可自定义）
  - `OnPlaceItem` - 放置物品（可自定义）
- **不关心视图更新，只负责业务逻辑，通过接口与视图层交互**

## 使用流程

### 步骤1：初始化网格辅助类

```csharp
// 初始化网格辅助类（单例，从 GridSetting 读取配置）
GridHelper gridHelper = GridHelper.Instance;
gridHelper.Initialize(); // 自动从 LS.Get<GridSetting>() 读取配置
```

### 步骤2：设置视图绑定器

```csharp
// 创建视图绑定器（普通类，不需要MonoBehaviour）
DragDropViewBinder viewBinder = new DragDropViewBinder();
viewBinder.Initialize(gridRenderer, showGridOnDrag: true, hideGridOnDragEnd: true);
```

### 步骤3：设置逻辑处理器

```csharp
// 创建逻辑处理器（普通类，不需要MonoBehaviour）
DragDropLogicHandler logicHandler = new DragDropLogicHandler();
logicHandler.Initialize(viewBinder); // 注入视图接口

// （可选）自定义逻辑层Action
logicHandler.OnPlaceItem = (itemData, worldPos) => {
    // 自定义放置逻辑
    var gridHelper = GridHelper.Instance;
    return TowerCreator.CreateTower(itemData.itemId, worldPos, gridHelper);
};
```

### 步骤3：设置UI拖拽项（最简单的方式）

```csharp
// 创建 DragItemData
DragItemData itemData = new DragItemData(towerId, 1, new List<Vector2Int> { Vector2Int.zero });

// 一键绑定（自动添加组件并绑定）
DragDropBinder.CreateAndBind(uiButton, itemData, logicHandler, worldCamera);
```

## 完整示例

```csharp
public class DragDropSetup : MonoBehaviour
{
    public GridHelper gridHelper;
    public GridRenderer gridRenderer;
    public Camera worldCamera;
    
    private DragDropViewBinder viewBinder;
    private DragDropLogicHandler logicHandler;
    
    void Start()
    {
        // 1. 初始化网格辅助类
        GridHelper gridHelper = GridHelper.Instance;
        gridHelper.Initialize(); // 从 GridSetting 读取配置
        
        // 2. 创建视图绑定器（普通类）
        viewBinder = new DragDropViewBinder();
        viewBinder.Initialize(gridRenderer);
        
        // 3. 创建逻辑处理器（普通类）
        logicHandler = new DragDropLogicHandler();
        logicHandler.Initialize(viewBinder); // 注入视图接口
        
        // 3. 设置UI拖拽项
        SetupDragItems();
    }
    
    void SetupDragItems()
    {
        // 假设有3个塔按钮
        for (int i = 1; i <= 3; i++)
        {
            GameObject button = CreateTowerButton(i);
            
            // 创建 DragItemData
            DragItemData itemData = new DragItemData(i, 1, new List<Vector2Int> { Vector2Int.zero });
            
            // 一键绑定（自动添加组件并绑定）
            DragDropBinder.CreateAndBind(button, itemData, logicHandler, worldCamera);
        }
    }
    
    GameObject CreateTowerButton(int towerId)
    {
        // 创建UI按钮的代码...
        return null;
    }
}
```

## 架构优势

1. **职责清晰**：每个类只负责一个层次，易于理解和维护
2. **易于扩展**：可以自定义逻辑层Action，实现不同的放置逻辑
3. **解耦设计**：视图层和逻辑层完全分离，互不依赖
4. **简单明了**：相比之前的复杂代码，现在只需要3个简单的类

## 与旧代码对比

### 旧架构问题
- `WorldDragDrop` 混合了事件接收、视图更新、业务逻辑
- `TowerPlacementManager` 混合了视图绑定和业务逻辑
- 代码复杂，难以维护

### 新架构优势
- `DragDropEventHandler` 只负责事件接收
- `DragDropViewBinder` 只负责视图更新
- `DragDropLogicHandler` 只负责业务逻辑
- 代码清晰，易于维护和扩展

