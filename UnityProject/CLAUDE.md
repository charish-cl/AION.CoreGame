# Claude 项目上下文同步文档

本文档用于同步项目上下文，帮助 AI 理解项目结构和开发规范。

## 项目概述

- **项目类型**：2D塔防游戏，带有肉鸽（Roguelike）属性
- **框架**：自定义简单框架（基于AION.CoreFramework）
- **配置系统**：数据驱动架构，使用Luban（Excel转JSON）进行配置

## 编程规范

### 1. 命名空间规则（重要！）

#### 1.1 必须引入类型命名空间
- **所有使用的外部类型必须引入对应的命名空间**
- 如果类型找不到，必须检查是否缺少 `using` 语句
- 如果项目中没有该类型，必须告知用户

### 2. 属性访问规则（重要！）

#### 2.1 属性访问权限检查
- **不能直接访问只读属性（只有 getter 没有 setter）**
- 如果属性没有公共的 setter，必须使用对应的设置方法
- 例如：`GameActor.Position` 是只读的，必须使用 `SetPosition()` 方法

### 3. 方法访问权限规则（重要！）

#### 3.1 方法访问权限设计原则
- **public**：供外部类调用的公共方法，必须明确设计意图
- **private**：仅内部使用的私有方法，外部不应访问
- **protected**：供子类继承使用的方法
- **internal**：同一程序集内可访问

#### 3.2 方法访问权限检查清单
在调用方法前，必须检查：
1. **方法是否为 public**：如果不是 public，不能从外部调用
2. **方法签名是否匹配**：参数类型和数量必须匹配
3. **是否需要重载版本**：如果需要无参数版本，必须提供重载

#### 3.3 常见错误和修复
- **错误**：`'ClassName.MethodName' is inaccessible due to its protection level`
- **原因**：尝试调用 private/protected/internal 方法
- **修复**：
  1. 检查方法访问权限
  2. 如果方法应该是 public，修改访问权限
  3. 如果需要外部调用但不想暴露内部实现，提供 public 包装方法

### 4. 泛型集合规则（重要！）

#### 4.1 必须引入泛型命名空间
- **使用泛型集合时必须引入 `using System.Collections.Generic;`**
- 包括：`List<>`, `Dictionary<>`, `HashSet<>`, `Queue<>`, `Stack<>` 等

### 5. Unity MCP工具使用规则

#### 5.1 MCP连接检查
- **Unity MCP 未连接时，必须征询用户意见**
- **不能自作主张创建脚本或修改文件**
- 必须先询问用户是否同意，再执行操作

### 6. 功能实现规则（重要！）

#### 6.1 只实现用户要求的功能
- **严格按照用户需求实现，不要自作主张扩展功能**
- **不要添加用户没有要求的功能或特性**
- **不要添加"可能有用"但用户没有明确要求的功能**
- 如果认为需要扩展功能，必须先询问用户意见

#### 6.2 功能扩展原则
- **如果认为某个功能有用，必须先询问用户**
- **不要假设用户需要某些功能**
- **保持实现简洁，只包含必要的代码**

### 7. 测试方法命名规范（重要！）

#### 7.1 测试方法命名规则
- **所有测试方法必须以 `Test_` 前缀开头**
- **测试方法应该明确标注为测试方法**
- **测试方法可以使用 `[Button]` 特性暴露到Inspector，方便手动测试**

**示例：**
```csharp
// ❌ 错误：测试方法没有 Test_ 前缀
public void GenerateRandomUnplaceableAreas()
{
    // 测试代码...
}

// ✅ 正确：测试方法使用 Test_ 前缀
/// <summary>
/// 测试方法：随机生成不可放置区域
/// </summary>
[Button("测试：生成随机不可放置区域", ButtonSizes.Medium)]
public void Test_GenerateRandomUnplaceableAreas()
{
    // 测试代码...
}
```

#### 7.2 测试方法设计原则
- **明确标识**：方法名和注释都要明确说明这是测试方法
- **可独立运行**：测试方法应该可以独立调用，不依赖特定执行顺序
- **便于调试**：使用 `[Button]` 特性，方便在Inspector中手动触发测试

### 8. MonoBehaviour使用规范（重要！）

#### 8.1 尽量避免使用MonoBehaviour
- **优先使用普通C#类**：除非必须依赖Unity的生命周期或组件系统，否则使用普通类
- **避免过度依赖Unity生命周期**：减少对 `Awake`、`Start`、`Update` 等的依赖

#### 8.2 如果必须使用MonoBehaviour，严格控制执行顺序
- **禁止在Awake/Start中自动初始化**：不要在 `Awake()` 或 `Start()` 中自动执行初始化逻辑
- **统一初始化入口**：提供一个 `public Initialize()` 方法，由外部统一调用
- **使用Manager模式**：创建一个总的Manager来统一初始化所有系统，确保执行顺序

**示例：**
```csharp
// ❌ 错误：在Awake中自动初始化，导致执行顺序混乱
public class TowerDefenseGridSystem : MonoBehaviour
{
    private void Awake()
    {
        InitializeGrid(); // 错误：自动初始化，执行顺序不可控
    }
}

// ✅ 正确：提供手动初始化方法，由外部统一调用
public class TowerDefenseGridSystem : MonoBehaviour
{
    /// <summary>
    /// 初始化网格系统（必须手动调用，不在Awake/Start中自动初始化）
    /// </summary>
    public void Initialize()
    {
        InitializeGrid();
        InitializePlaceableArea(null);
    }
    
    private void InitializeGrid()
    {
        // 初始化逻辑...
    }
}

// 使用方式：由Manager统一初始化
public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // 按顺序初始化所有系统
        TowerDefenseGridSystem.Instance.Initialize();
        ActorMgr.Instance.Initialize();
        // ...
    }
}
```

#### 8.3 初始化原则
- **单一入口**：每个系统只有一个初始化入口
- **明确顺序**：初始化顺序由Manager统一控制
- **可重复调用**：初始化方法应该可以安全地重复调用（幂等性）

### 9. 代码简洁性规范（重要！）

#### 9.1 代码长度限制
- **单个类尽量不要超过300行**：超过300行的类应该考虑拆分或重构
- **单个方法尽量不超过50行**：过长的方法应该拆分为多个小方法
- **以简洁实用为主**：避免过度设计，优先考虑可维护性

#### 9.2 代码组织原则
- **避免类过多导致维护困难**：不要为了解耦而创建过多的小类
- **合并相关功能**：如果多个类功能高度相关且简单，考虑合并
- **调试代码分离**：调试相关的Data和方法应该单独暴露在Test类中，而不是分散在各个业务类中

**示例：**
```csharp
// ❌ 错误：创建过多小类，导致维护困难
public class WorldDragDrop { } // 473行
public class WorldDragDropHelper { } // 344行
public class TowerPlacementManager { } // 249行
// 三个类功能相关，但分散在不同文件中，难以维护

// ✅ 正确：合并相关功能，保持简洁
public class TowerPlacementSystem { } // 合并后控制在300行以内
// 调试相关的方法和Data放在 Test 类中
public class TowerPlacementTest { } // 测试和调试代码
```

#### 9.3 重构建议
- **超过300行的类**：考虑拆分或合并相关功能
- **功能重复的类**：合并或提取公共逻辑
- **调试代码**：统一放在Test类中，使用 `Test_` 前缀

### 10. 编程范式与规范

#### 10.1 代码组织
- 保持代码简洁，避免过长的方法
- 将调试代码分离到独立脚本
- 遵循单一职责原则

#### 10.2 错误处理
- 使用 `Log.Info`、`Log.Warning`、`Log.Error` 进行日志记录
- 在关键操作前检查空引用
- 提供有意义的错误信息

## 核心系统

### Actor管理系统
- **ActorMgr.cs**：管理游戏实体（Player, Enemy, Tower, Bullet, Base）
- **GameActor**：基础Actor类，包含位置、属性等

### 技能系统
- 复杂的配置系统，支持各种效果（PropertyMod, Heal, Damage, Status）
- 基于配置的Buff系统

### 编辑器工具系统
- XML UI生成器
- 配置编辑器

### 关卡系统
- 关卡配置和管理

### 网格系统
- **TowerDefenseGridSystem**：塔防网格系统，用于管理塔的放置、碰撞检测
- **GridRenderer**：网格渲染器，使用Shader直接绘制整个网格
- **GridDragView**：网格拖拽视图层，统一管理网格渲染和UI显示
- **WorldDragDrop**：世界拖拽系统，通用的从UI拖拽到世界的系统
- **WorldDragDropHelper**：世界拖拽辅助类，用于将WorldDragDrop与具体业务逻辑解耦

## 架构设计

### 逻辑层与视图层分离
- **逻辑层**（如 `WorldDragDropHelper`）：只负责触发回调，不直接操作渲染
- **视图层**（如 `GridDragView`）：统一管理渲染和UI显示，响应逻辑层的回调
- **渲染组件**（如 `GridRenderer`）：纯渲染组件，依赖由外部注入

### 执行顺序
1. 视图层初始化时，设置渲染组件并注册事件
2. 逻辑层通过视图层更新视图
3. 所有渲染和UI显示由视图层统一管理

