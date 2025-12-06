# 塔放置系统使用说明

## 概述

新的塔放置系统支持以下功能：
1. **不规则形状的塔**：支持任意形状的塔（如L型、T型等），不仅仅是矩形
2. **两种放置模式**：
   - **点击模式**：点击tower图标后，点击网格放置
   - **拖拽模式**：拖拽tower图标到网格放置
3. **高亮显示**：可放置区域显示绿色，不可放置区域显示红色
4. **网格区域管理**：可以初始化可放置/不可放置区域，放置后自动更新

## 核心组件

### 1. TowerDefenseGridSystem（网格系统）

**新增功能**：
- 支持不规则形状的塔（通过坐标列表定义）
- 初始化可放置/不可放置区域
- 放置后自动标记为不可放置

**关键方法**：
```csharp
// 检查是否可以放置（使用不规则形状）
bool CanPlaceTower(Vector2Int gridPos, List<Vector2Int> towerFootprint)

// 放置塔（使用不规则形状）
bool PlaceTower(Vector2Int gridPos, GameActor tower, List<Vector2Int> towerFootprint)

// 初始化可放置区域
void InitializePlaceableArea(List<Vector2Int> placeableCells = null)

// 获取塔的占用形状（用于高亮显示）
List<Vector2Int> GetTowerFootprint(Vector2Int gridPos, List<Vector2Int> towerFootprint)
```

**塔形状定义示例**：
```csharp
// 1x1 塔
List<Vector2Int> footprint1x1 = new List<Vector2Int> { new Vector2Int(0, 0) };

// 2x2 塔
List<Vector2Int> footprint2x2 = new List<Vector2Int> 
{ 
    new Vector2Int(0, 0), new Vector2Int(1, 0),
    new Vector2Int(0, 1), new Vector2Int(1, 1)
};

// L型塔（用户示例：(0,1)(1,1)(0,1) 应该是 (0,0)(1,0)(0,1)）
List<Vector2Int> footprintL = new List<Vector2Int> 
{ 
    new Vector2Int(0, 0), 
    new Vector2Int(1, 0), 
    new Vector2Int(0, 1) 
};
```

### 2. GridCellHighlighter（高亮器）

**功能**：
- 高亮显示可放置区域（绿色）
- 高亮显示不可放置区域（红色）
- 使用自定义shader实现边缘效果

**使用方法**：
```csharp
// 高亮单个网格
highlighter.HighlightCell(gridPos, isValid);

// 高亮多个网格（用于显示塔的占用形状）
highlighter.HighlightCells(footprintCells, isValid);

// 清除所有高亮
highlighter.ClearAllHighlights();
```

### 3. TowerPlacementManager（放置管理器）

**功能**：
- 管理两种放置模式（点击模式和拖拽模式）
- 处理塔的创建和放置逻辑
- 实时更新高亮显示

**放置模式**：
- `PlacementMode.Click`：点击模式
- `PlacementMode.Drag`：拖拽模式

**关键方法**：
```csharp
// 选择塔（进入放置模式）
void SelectTower(int towerId, List<Vector2Int> towerFootprint = null)

// 取消放置
void CancelPlacement()

// 创建塔
GameActor CreateTower(int towerId, Vector2 worldPosition)
```

### 4. TowerSelectionUI（塔选择UI）

**功能**：
- 显示tower图标
- 点击后进入放置模式
- 显示选中状态

**使用方法**：
1. 在UI中创建Image，添加`TowerSelectionUI`组件
2. 设置`towerId`和`towerFootprint`
3. 点击后自动进入放置模式

### 5. WorldDragDrop（拖拽系统）

**新增功能**：
- 拖拽结束添加日志
- 支持与`TowerPlacementManager`配合使用

## 使用流程

### 点击模式流程

1. **设置UI**：
   - 在Canvas中创建Image，添加`TowerSelectionUI`组件
   - 设置`towerId`和`towerFootprint`
   - 设置`placementManager`引用

2. **点击tower图标**：
   - 用户点击UI中的tower图标
   - `TowerSelectionUI`调用`TowerPlacementManager.SelectTower()`
   - 进入放置模式

3. **选择位置**：
   - 鼠标移动时，实时高亮显示塔的占用形状
   - 可放置：绿色
   - 不可放置：红色

4. **放置塔**：
   - 点击鼠标左键放置
   - 按ESC取消放置

### 拖拽模式流程

1. **设置UI**：
   - 在UI元素上添加`WorldDragDrop`组件
   - 设置`dragItemId`和`towerFootprint`（通过`WorldDragDropHelper`）

2. **拖拽**：
   - 用户拖拽UI元素
   - 实时显示预览和高亮

3. **放置**：
   - 拖拽结束时，如果位置可放置，则创建塔
   - 日志记录拖拽结束信息

## 初始化可放置区域

```csharp
// 方式1：全部可放置（默认）
gridSystem.InitializePlaceableArea();

// 方式2：指定可放置区域
List<Vector2Int> placeableCells = new List<Vector2Int>
{
    new Vector2Int(10, 10),
    new Vector2Int(11, 10),
    new Vector2Int(10, 11),
    // ... 更多坐标
};
gridSystem.InitializePlaceableArea(placeableCells);
```

## 塔形状配置示例

```csharp
// 在TowerSelectionUI或配置中设置
public class TowerConfig
{
    public int towerId = 1;
    
    // 1x1 塔
    public List<Vector2Int> footprint1x1 = new List<Vector2Int> 
    { 
        new Vector2Int(0, 0) 
    };
    
    // 2x3 矩形塔
    public List<Vector2Int> footprint2x3 = new List<Vector2Int> 
    { 
        new Vector2Int(0, 0), new Vector2Int(1, 0),
        new Vector2Int(0, 1), new Vector2Int(1, 1),
        new Vector2Int(0, 2), new Vector2Int(1, 2)
    };
    
    // L型塔（用户示例：(0,1)(1,1)(0,1) 应该是 (0,0)(1,0)(0,1)）
    public List<Vector2Int> footprintL = new List<Vector2Int> 
    { 
        new Vector2Int(0, 0),  // 左下
        new Vector2Int(1, 0),  // 右下
        new Vector2Int(0, 1)   // 左上
    };
}
```

## 注意事项

1. **坐标系统**：塔的形状坐标是相对于锚点的偏移，锚点通常是塔的中心或某个特定点
2. **放置后更新**：放置塔后，占用的网格会自动标记为`isOccupied = true`和`isPlaceable = false`
3. **高亮性能**：高亮系统使用对象池，但大量高亮可能影响性能，建议限制同时高亮的数量
4. **日志**：拖拽结束时会自动记录日志，格式：`WorldDragDrop: 拖拽结束 - ItemId={id}, WorldPos={pos}, CanPlace={canPlace}`

## Shader说明

`GridCellHighlight.shader`用于高亮显示网格单元，支持：
- 自定义颜色（可放置/不可放置）
- 边缘效果
- 透明度控制

如果shader未找到，系统会自动使用默认材质。

