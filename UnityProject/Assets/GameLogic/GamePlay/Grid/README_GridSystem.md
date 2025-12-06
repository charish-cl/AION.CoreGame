# 塔防网格系统使用指南

本指南介绍如何使用塔防网格系统、世界拖拽系统和攻击范围高亮系统。

## 系统概述

### 1. TowerDefenseGridSystem（塔防网格系统）
负责管理塔的放置、碰撞检测和区域选择。

### 2. WorldDragDrop（世界拖拽系统）
实现从UI拖拽到世界生成塔的功能。

### 3. AttackRangeHighlighter（攻击范围高亮系统）
点击塔时显示攻击范围。

### 4. TowerSelectionManager（塔选择管理器）
管理塔的选择和攻击范围显示。

## 快速开始

### 步骤1：设置网格系统

1. 在场景中创建一个空GameObject，命名为`TowerDefenseGridSystem`
2. 添加`TowerDefenseGridSystem`组件
3. 配置参数：
   - **Cell Size**: 网格单元大小（例如 1x1）
   - **Grid Origin**: 网格原点（世界坐标）
   - **Grid Size**: 网格尺寸（单元数量，例如 50x50）
   - **Show Grid Lines**: 是否在Scene视图中显示网格线

### 步骤2：设置塔选择管理器

1. 在场景中创建一个空GameObject，命名为`TowerSelectionManager`
2. 添加`TowerSelectionManager`组件
3. （可选）创建攻击范围高亮预制体，并赋值给`Attack Range Highlight Prefab`

### 步骤3：设置UI拖拽

1. 在UI中创建一个按钮或图标，用于拖拽塔
2. 添加`WorldDragDrop`组件
3. 配置参数：
   - **Tower Id**: 塔配置ID（从配置表读取）
   - **Preview Prefab**: 拖拽时显示的预览预制体（可选）
   - **Snap To Grid**: 是否对齐到网格（推荐开启）
   - **Tower Size**: 塔占用的网格大小（1表示占用1x1网格）

## 详细使用

### 网格系统API

```csharp
// 获取单例
var gridSystem = TowerDefenseGridSystem.Instance;

// 世界坐标转网格坐标
Vector2Int gridPos = gridSystem.WorldToGrid(worldPosition);

// 网格坐标转世界坐标
Vector2 worldPos = gridSystem.GridToWorld(gridPos);

// 检查是否可以放置塔
bool canPlace = gridSystem.CanPlaceTower(worldPos, towerSize);

// 放置塔
bool success = gridSystem.PlaceTower(worldPos, tower, towerSize);

// 移除塔
gridSystem.RemoveTower(tower);

// 获取鼠标位置的网格坐标
Vector2Int mouseGrid = gridSystem.GetMouseGridPosition();

// 获取鼠标位置的世界坐标（对齐到网格）
Vector2 mouseWorld = gridSystem.GetMouseWorldPositionAligned();
```

### 世界拖拽系统

#### 基本使用

1. 在UI元素上添加`WorldDragDrop`组件
2. 设置`Tower Id`（塔配置ID）
3. 设置`Tower Size`（塔占用的网格大小）
4. 运行游戏，拖拽UI元素到世界即可生成塔

#### 自定义预览

1. 创建一个预览预制体（可以是塔的简化版本）
2. 在`WorldDragDrop`组件中设置`Preview Prefab`
3. 拖拽时会显示预览，绿色表示可放置，红色表示不可放置

#### 事件监听

```csharp
var worldDragDrop = GetComponent<WorldDragDrop>();
worldDragDrop.OnTowerPlaced += (towerId, worldPos) => {
    Debug.Log($"塔 {towerId} 放置在 {worldPos}");
};
```

### 攻击范围高亮

#### 自动高亮（推荐）

1. 确保场景中有`TowerSelectionManager`
2. 点击塔即可自动显示攻击范围

#### 手动控制

```csharp
// 选择塔
TowerSelectionManager.Instance.SelectTower(tower);

// 取消选择
TowerSelectionManager.Instance.DeselectTower();

// 获取选中的塔
var selected = TowerSelectionManager.Instance.GetSelectedTower();
```

#### 自定义高亮外观

1. 创建攻击范围高亮预制体
2. 添加`AttackRangeHighlighter`组件
3. 配置参数：
   - **Highlight Color**: 高亮颜色
   - **Edge Color**: 边缘颜色
   - **Edge Width**: 边缘宽度
   - **Use Shader Highlight**: 是否使用Shader高亮（推荐）

### Shader高亮（推荐）

1. 使用提供的`AttackRangeHighlight.shader`
2. 在`AttackRangeHighlighter`组件中设置`Use Shader Highlight = true`
3. Shader会自动创建圆形高亮，带有边缘效果

## 高级功能

### 设置不可放置区域

```csharp
// 设置单个网格单元不可放置
gridSystem.SetPlaceable(new Vector2Int(10, 10), false);

// 设置矩形区域不可放置
Rect worldRect = new Rect(0, 0, 10, 10);
gridSystem.SetPlaceableArea(worldRect, false);
```

### 监听网格事件

```csharp
// 监听网格单元悬停
gridSystem.OnCellHovered += (gridPos) => {
    Debug.Log($"悬停在网格 {gridPos}");
};

// 监听塔选择
gridSystem.OnTowerSelected += (tower) => {
    if (tower != null) {
        Debug.Log($"选中塔: {tower.name}");
    }
};
```

### 自定义网格可视化

网格系统使用`OnDrawGizmos`在Scene视图中绘制网格线。可以在`TowerDefenseGridSystem`组件中调整：
- **Grid Line Color**: 网格线颜色
- **Valid Place Color**: 可放置区域颜色
- **Invalid Place Color**: 不可放置区域颜色

## 性能优化建议

1. **网格大小**: 根据游戏地图大小合理设置网格尺寸，避免过大
2. **网格单元大小**: 根据塔的大小设置合适的单元大小（通常1-2个单位）
3. **高亮对象**: 使用对象池管理高亮对象，避免频繁创建销毁
4. **Shader**: 使用Shader高亮比Sprite高亮性能更好

## 常见问题

### Q: 拖拽时看不到预览？
A: 检查`Preview Prefab`是否设置，或者检查相机设置。

### Q: 点击塔没有显示攻击范围？
A: 确保场景中有`TowerSelectionManager`组件，并且塔有`Collider2D`组件。

### Q: 网格对齐不准确？
A: 检查`Grid Origin`和`Cell Size`设置，确保与游戏世界坐标匹配。

### Q: 如何自定义攻击范围显示？
A: 创建自定义预制体，添加`AttackRangeHighlighter`组件，并设置`Use Shader Highlight = false`，然后自定义Sprite。

## 示例场景

完整的示例场景包含：
1. 网格系统设置
2. UI拖拽按钮
3. 塔选择管理器
4. 测试用的塔预制体

参考场景：`Assets/GameLogic/GamePlay/Grid/Test/GridTestScene.unity`（需要创建）

