# 测试场景设置指南

本指南说明如何设置测试场景来测试网格系统和拖拽系统。

## 场景设置步骤

### 1. 创建或打开测试场景

- 创建新场景：`File → New Scene`
- 或使用现有场景：`Assets/TopDownEngine/Demos/Koala2D/KoalaHealth.unity`

### 2. 创建网格系统

1. 创建空GameObject，命名为 `TowerDefenseGridSystem`
2. 添加组件：`TowerDefenseGridSystem` (位于 `GameLogic` 命名空间)
3. 配置参数：
   - **Cell Size**: `(1, 1)` - 根据实际网格大小调整
   - **Grid Origin**: `(0, 0)` - 网格原点（世界坐标）
   - **Grid Size**: `(50, 50)` - 网格尺寸（单元数量）
   - **Show Grid Lines**: `true` - 在Scene视图中显示网格线
   - **Grid Line Color**: 白色半透明
   - **Valid Place Color**: 绿色半透明
   - **Invalid Place Color**: 红色半透明

### 3. 创建塔选择管理器

1. 创建空GameObject，命名为 `TowerSelectionManager`
2. 添加组件：`TowerSelectionManager` (位于 `GameLogic` 命名空间)
3. 配置参数：
   - **Auto Create Highlight**: `true`
   - **Attack Range Highlight Prefab**: （可选）如果有预制体可以赋值

### 4. 创建测试脚本

1. 创建空GameObject，命名为 `WorldDragDropTest`
2. 添加组件：`WorldDragDropTest` (位于 `GameLogic` 命名空间)
3. 配置参数：
   - **Grid System**: 拖入 `TowerDefenseGridSystem` GameObject
   - **World Camera**: 拖入主相机（Main Camera）
   - **Test UI Elements**: 拖入UI中的塔图标（可以稍后添加）
   - **Test Tower Ids**: 设置塔ID数组，例如 `[1, 2, 3]`
   - **Tower Size**: `1` - 塔占用的网格大小
   - **Grid Cell Size**: `(1, 1)` - 必须与TowerDefenseGridSystem一致
   - **Preview Prefab**: （可选）预览预制体

### 5. 设置UI元素（底部拖拽区域）

#### 方式1：使用现有UI
1. 在底部UI区域找到塔图标
2. 为每个塔图标添加 `WorldDragDrop` 组件
3. 配置 `WorldDragDrop`：
   - **Drag Item Id**: 设置对应的塔ID
   - **World Camera**: 拖入主相机
   - **Snap To Grid**: `true`
   - **Grid Cell Size**: `(1, 1)`
   - **Preview Prefab**: （可选）

#### 方式2：使用测试脚本自动创建
1. 在 `WorldDragDropTest` 组件上点击 **"创建测试UI按钮"** 按钮
2. 会自动创建测试按钮
3. 点击 **"初始化测试"** 按钮

### 6. 确保场景有必要的系统

- **Camera**: 主相机（用于世界坐标转换）
- **Canvas**: UI Canvas（用于UI拖拽）
- **EventSystem**: 事件系统（Unity UI需要）

## 快速测试流程

1. **运行游戏**
2. **点击"初始化测试"按钮**（在WorldDragDropTest组件上）
3. **从底部UI拖拽塔图标**
4. **拖拽到上方网格区域**
5. **观察预览**（绿色=可放置，红色=不可放置）
6. **松开鼠标**，如果位置有效，会创建塔
7. **点击创建的塔**，应该显示攻击范围

## 常见问题

### Q: 拖拽时看不到预览？
A: 检查：
- World Camera是否正确设置
- Preview Prefab是否设置（或UI元素是否有Image组件）
- 世界坐标转换是否正确

### Q: 预览位置不对？
A: 检查：
- World Camera是否正确（不是UI相机）
- Grid Cell Size是否与TowerDefenseGridSystem一致
- World Z Depth设置是否正确

### Q: 无法放置塔？
A: 检查：
- Grid System是否正确初始化
- 网格位置是否可放置（检查网格系统配置）
- ActorMgr是否已初始化

### Q: 点击塔没有显示攻击范围？
A: 检查：
- TowerSelectionManager是否存在
- 塔是否有Collider2D组件
- 塔是否正确创建

## 调试技巧

1. **查看Console日志**：所有操作都会输出日志
2. **使用Scene视图**：开启Show Grid Lines查看网格
3. **使用Gizmos**：在Scene视图中查看网格线和悬停单元
4. **Inspector调试**：查看WorldDragDropTest的调试信息

## 测试场景完整结构

```
Scene
├── Camera (MainCamera)
├── EventSystem
├── Canvas (UI)
│   └── [UI元素，包含WorldDragDrop组件]
├── TowerDefenseGridSystem
├── TowerSelectionManager
└── WorldDragDropTest
```

## 下一步

设置完成后，运行游戏并测试拖拽功能。如果遇到问题，查看Console日志和调试信息。

