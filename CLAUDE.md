# CLAUDE.md

本文件为Claude Code (claude.ai/code)在此代码库中工作时提供指导。

## 项目概述

这是一个基于**Unity 2022.3.57f1c2**的**2D塔防游戏**项目，带有**肉鸽（Roguelike）**属性。项目采用自定义的简单框架架构，基于**AION.CoreFramework**，采用复杂的数据驱动架构。项目实现了Buff系统、技能系统、装备管理和自定义的基于XML的UI生成工具。

## 架构概述

### 核心框架结构
- **入口点**: `GameApp.cs` - 单例模式，支持热更新
- **基础框架**: 自定义简单框架（基于AION.CoreFramework）
- **热更新系统**: 通过AION.CoreFramework支持运行时代码重载
- **配置系统**: 基于Luban的数据驱动配置（Excel → JSON → 运行时）
- **游戏类型**: 2D塔防游戏，带有肉鸽（Roguelike）元素

### 关键目录结构
```
Assets/
├── Editor/                    # 自定义编辑器工具
│   ├── ConfigEditor/         # Excel/配置编辑器工具
│   ├── XMLUIGenerator/      # WPF风格XML到Unity UI转换器
│   ├── ActorDebugInspect.cs # Actor调试工具
│   └── ActorTestTool.cs     # Actor测试工具
├── Game/                     # 游戏资源（模型、纹理、预制体）
│   ├── Config/              # 游戏配置文件
│   ├── UIForm/              # UI预制体
│   └── UIComponent/         # 可复用UI组件
├── GameLogic/               # 核心游戏逻辑（C#脚本）
│   ├── Core/                # 核心系统（GameApp等）
│   ├── GameActor/           # Actor和Buff系统
│   │   ├── Actors/          # 各种Actor类型（塔、敌人、英雄等）
│   │   ├── Buff/            # Buff系统（用于实现技能效果）
│   │   └── Com/             # Actor组件
│   ├── System/              # 游戏系统
│   │   ├── ActorMgr.cs     # Actor管理器
│   │   ├── LevelSystem.cs   # 关卡系统（塔防波次管理）
│   │   └── ConfigSystem.cs  # 配置系统
│   ├── UI/                  # UI控制器
│   ├── GamePlay/            # 游戏玩法相关
│   │   ├── PathFinding/    # 路径寻找系统
│   │   └── BehaviorTree/   # 行为树
│   └── Config/              # 配置数据
├── GameConfig/              # 配置定义（Luban生成）
└── Plugins/                 # 第三方插件（Luban、TextMeshPro等）
```

## 常用开发命令

### Unity编辑器工作流程
由于这是一个Unity项目，大部分开发通过Unity编辑器完成：

1. **打开项目**: 启动Unity编辑器并选择`UnityProject`文件夹
2. **进入播放模式**: 点击Unity编辑器中的播放按钮进行测试
3. **构建**: 使用`File → Build Settings`创建构建版本
4. **运行测试**: 通过`Window → General → Test Runner`使用Unity测试框架

### 配置管理流程
项目使用**Luban**进行配置管理：

1. **编辑Excel文件**: 配置数据存储在Excel文件中
2. **生成配置**: 使用Luban工具将Excel转换为JSON
3. **运行时加载**: 配置通过`ConfigSystem`自动加载

### UI生成工作流程
使用XML UI生成器创建Unity UI：

1. **读取配置**: 始终先读取`Assets/Editor/XMLUIGenerator/XMLUIGeneratorPrefabConfig.json`
2. **分析UI图片**: 识别元素并根据功能匹配预制体
3. **生成XML**: 创建具有正确结构的WPF风格XML
4. **使用工具**: 通过Unity编辑器中的`XMLUIGeneratorWindow`生成Unity UI

## 核心游戏系统

### 1. 关卡系统（塔防核心）
位于`GameLogic/System/LevelSystem.cs`，管理塔防游戏的关卡和波次：

- **关卡阶段**: BuildPhase（建塔阶段）、BattlePhase（战斗阶段）、WaveEnd（波次结束）等
- **波次管理**: 支持多波次敌人生成，每个波次包含多个SpawnGroup
- **敌人生成**: 按配置的延迟和间隔生成敌人
- **阶段切换**: 自动管理建塔阶段和战斗阶段的切换
- **基地管理**: 管理基地生命值，关卡失败条件

### 2. Actor管理系统
关键文件: `GameLogic/System/ActorMgr.cs`, `GameLogic/GameActor/`

- **集中管理**: ActorMgr处理所有游戏Actor（塔、敌人、英雄、子弹等）
- **层级系统**: Actor的父子关系，按类型和配置ID组织
- **唯一ID**: 基于配置的ID生成唯一标识
- **路径寻找**: 与关卡路径寻找系统集成，敌人沿路径移动
- **场景集成**: 与`SceneBehavior`配合处理关卡数据
- **Actor类型**: 支持塔（TowerActor）、敌人（EnemyActor）、英雄（UnitActor）等

### 3. Buff系统
位于`GameLogic/GameActor/Buff/`，这是一个复杂的基于配置的Buff系统：

- **配置驱动**: 使用Luban配置定义Buff
- **Buff类型**: PropertyMod（属性修改）、Heal（治疗）、Damage（伤害）、Status（状态效果）
- **触发类型**: Immediate（立即触发）、Interval（间隔触发）、Probability（概率触发）、OnDeath（死亡触发）等
- **堆叠**: 支持可堆叠Buff，有最大堆叠限制
- **目标筛选**: 友方/敌方目标筛选系统，支持多种目标选择器
- **数值系统**: 与NumericComponent集成，支持属性修改

### 4. 技能系统
**技能系统通过Buff系统实现**，位于`GameLogic/GameActor/Buff/`：

- **技能配置**: 技能效果通过配置Buff来实现
- **技能类型**: 
  - **被动技能**: 通过Buff持续生效（如属性加成）
  - **主动技能**: 通过触发Buff实现（如攻击时触发）
  - **亡语技能**: 通过OnDeath触发类型实现（如死亡时触发效果）
- **技能效果**: 支持伤害、治疗、属性修改、状态效果等
- **目标选择**: 支持多种目标选择器（最近敌人、随机敌人、友方等）
- **技能组合**: 可以通过多个Buff组合实现复杂技能效果

**技能配置示例**（见`GameLogic/GameActor/Buff/亡语技能配置说明.md`）：
- 亡语技能：死亡时概率触发，对多个敌人施加Buff
- 中毒技能：间隔触发，持续造成伤害
- 属性加成：立即触发，永久修改属性

### 5. UI生成系统
位于`Editor/XMLUIGenerator/`:

- **XML到Unity**: 将WPF风格XML转换为Unity UI组件
- **预制体集成**: 自动匹配和实例化预制体
- **布局组件**: 支持Grid、HorizontalLayout、VerticalLayout
- **坐标系统**: WPF到Unity坐标转换
- **设计规则**: 详细规则记录在`XMLUIGeneratorRules.md`中

## 编辑器工具系统

所有编辑器工具位于`Assets/Editor/`目录下：

### 配置编辑工具
- **ExcelEditorWindow**: 在Unity编辑器中编辑Excel配置数据
  - 位置: `Assets/Editor/ExcelEditorWindow.cs`
  - 功能: 可视化编辑Luban配置表
  - 相关: `Assets/Editor/ConfigEditor/`目录下的配置编辑器

### UI生成工具
- **XMLUIGeneratorWindow**: 从XML生成Unity UI
  - 位置: `Assets/Editor/XMLUIGenerator/XMLUIGeneratorWindow.cs`
  - 功能: 将WPF风格XML转换为Unity UI预制体
  - 配置: `Assets/Editor/XMLUIGenerator/XMLUIGeneratorPrefabConfig.json`
  - 规则: `Assets/Editor/XMLUIGenerator/XMLUIGeneratorRules.md`

### 调试工具
- **ActorDebugInspect**: 游戏Actor的运行时调试
  - 位置: `Assets/Editor/ActorDebugInspect.cs`
  - 功能: 在编辑器中查看和调试Actor状态

- **ActorTestTool**: Actor测试工具
  - 位置: `Assets/Editor/ActorTestTool.cs`
  - 功能: 测试Actor创建和功能

### 其他工具
- **ReferencedObjectWindow**: 引用对象查看窗口
- **LubanTools**: Luban配置生成工具（位于`Assets/OldScript/Editor/Window/LubanTools.cs`）

## 关键配置文件

### XML UI生成器配置
**文件**: `Assets/Editor/XMLUIGenerator/XMLUIGeneratorPrefabConfig.json`

**AI关键**: 在生成任何XML UI之前必须读取此文件！

```json
{
  "prefabSearchPaths": ["Assets/Game/UIForm/Common", "Assets/Game/UIComponent"],
  "prefabAliases": {
    "CommonGoodsItem": "Assets/Game/UIForm/Common/CommonGoodsItem.prefab",
    "CurrencyItem": "Assets/Game/UIForm/Common/CurrencyItem.prefab"
  },
  "prefabMatchRules": [
    {
      "description": "金币、钻石等货币显示使用 CurrencyItem 预制体",
      "matchPatterns": [{"nameContains": ["Coin", "Diamond", "Currency"], "prefabName": "CurrencyItem"}]
    }
  ]
}
```

### Luban配置
配置系统使用Excel文件作为源，为运行时生成JSON：

- **位置**: `Assets/Plugins/LubanLib/`
- **使用方法**: 编辑Excel → 生成JSON → 运行时自动加载
- **优势**: 类型安全、验证、热更新支持
- **配置表**: 包括Buff配置、单位配置、关卡配置、波次配置等

## 开发模式

### 编程范式与规范

#### ⚠️ 命名空间规则（必须遵守）
**在编写任何代码时，必须添加所有使用类型的完整命名空间！**

1. **必须添加using指令**: 使用任何类型前，必须添加对应的`using`指令
2. **检查命名空间**: 如果不确定类型的命名空间，必须：
   - 先搜索项目中是否存在该类型
   - 查看现有代码中如何使用该类型
   - 如果项目中不存在，必须告知用户
3. **常见命名空间**:
   - `GameConfig.battle` - 战斗相关配置（TowerConfig、UnitConfig等）
   - `GameConfig.level` - 关卡相关配置（LevelBaseConfig、WaveConfig等）
   - `GameConfig` - 通用配置
   - `GameLogic` - 游戏逻辑
   - `AION.CoreFramework` - 核心框架（包含Log等工具类）
   - `GameDevKit` - 开发工具包
   - **`System.Collections.Generic`** - **泛型集合类型（必须记住！）**
     - `List<T>` → 需要 `using System.Collections.Generic;`
     - `Dictionary<TKey, TValue>` → 需要 `using System.Collections.Generic;`
     - `HashSet<T>` → 需要 `using System.Collections.Generic;`
     - `Queue<T>` → 需要 `using System.Collections.Generic;`
     - `Stack<T>` → 需要 `using System.Collections.Generic;`
4. **泛型集合规则（重要）**:
   - **使用任何泛型集合类型前，必须添加 `using System.Collections.Generic;`**
   - 常见错误：`List<>`、`Dictionary<>` 等找不到类型 → 缺少 `using System.Collections.Generic;`
   - **检查清单**：使用 `List`、`Dictionary`、`HashSet`、`Queue`、`Stack` 等泛型集合时，必须确保文件顶部有 `using System.Collections.Generic;`
5. **禁止**: 绝对不允许因为缺少命名空间导致编译错误
5. **关键检查项**:
   - `Log.Info()` / `Log.Warning()` / `Log.Error()` → 需要`using AION.CoreFramework;`
   - `GameActor` → 需要确保正确的命名空间引用
   - 自定义类型 → 必须验证命名空间存在

#### ⚠️ 变量声明规则（必须遵守）
**在代码中使用任何变量前，必须确保变量已正确声明！**

1. **变量声明检查**: 使用变量前必须确认：
   - 变量已正确声明（类型和名称）
   - 变量在正确的作用域内
   - 变量已正确初始化
2. **常见错误检查**:
   - 拼写错误：`m_gridFilter` vs `m_gridMeshFilter`
   - 作用域错误：局部变量 vs 成员变量
   - 未初始化：使用前必须赋值
3. **类型匹配**: 确保赋值和使用的类型匹配
4. **调试步骤**: 如果遇到"does not exist in the current context"错误：
   - 检查变量声明位置
   - 检查变量拼写
   - 检查是否在正确的方法/类中
   - 检查访问修饰符（private/public等）

#### 属性访问规则
- **只读属性**: 如果属性只有getter，必须使用对应的setter方法
  - 例如：`GameActor.Position`是只读的，必须使用`SetPosition()`方法
- **检查访问权限**: 使用属性前必须检查是否有setter，如果没有则查找对应的setter方法

#### ⚠️ 编译错误预防规则
**在提交代码前必须检查以下常见编译错误！**

1. **命名空间缺失**:
   ```csharp
   // ❌ 错误
   Log.Info("test"); // CS0103: The name 'Log' does not exist

   // ✅ 正确
   using AION.CoreFramework;
   Log.Info("test");
   ```

1.1. **泛型集合命名空间缺失**:
   ```csharp
   // ❌ 错误
   List<int> list = new List<int>(); // CS0246: The type or namespace name 'List<>' could not be found

   // ✅ 正确
   using System.Collections.Generic;
   List<int> list = new List<int>();
   ```

2. **变量名错误**:
   ```csharp
   // ❌ 错误
   m_gridFilter.mesh = mesh; // CS0103: The name 'm_gridFilter' does not exist

   // ✅ 正确
   m_gridMeshFilter.mesh = mesh;
   ```

3. **未声明变量**:
   ```csharp
   // ❌ 错误
   private MeshFilter m_gridFilter; // 声明
   private void Method() {
       m_gridMeshFilter = value; // 使用错误名称
   }

   // ✅ 正确
   private MeshFilter m_gridMeshFilter; // 正确声明
   private void Method() {
       m_gridMeshFilter = value; // 正确使用
   }
   ```

4. **检查清单**:
   - ✅ 所有自定义类型都有正确的using指令
   - ✅ **所有泛型集合类型（List、Dictionary等）都有 `using System.Collections.Generic;`**
   - ✅ 所有变量都已正确声明
   - ✅ 变量名拼写正确
   - ✅ 变量在正确的作用域内
   - ✅ 类型匹配正确
   - ✅ 访问修饰符合适

#### Unity MCP工具使用规则（重要）
**在使用Unity MCP工具前，必须先检查连接状态！**

1. **检查连接**: 使用Unity MCP工具前，必须先尝试调用一个简单的工具（如`read_console`）检查Unity是否连接
2. **未连接时**: 如果Unity MCP未连接，**绝对不要**：
   - 自作主张创建脚本或文件
   - 假设用户想要什么功能
   - 直接创建测试场景或GameObject
3. **正确做法**: 如果Unity MCP未连接，必须：
   - **明确告知用户**Unity MCP未连接
   - **询问用户**是否需要：
     - 等待Unity连接后再操作
     - 提供手动设置步骤
     - 创建编辑器工具脚本（需用户确认）
4. **征询意见**: 任何涉及创建文件、修改场景、添加组件的操作，都必须先询问用户意见
5. **工具脚本**: 如果要创建编辑器工具脚本，必须先说明用途，获得用户同意后再创建

### 代码组织
- **命名空间**: 核心游戏逻辑使用`GameLogic`
- **单例模式**: 管理器使用（GameApp、ActorMgr、LevelSystem）
- **基类**: 游戏系统使用`BaseLogicSys<T>`（来自AION.CoreFramework）
- **组件设计**: 基于组件的Actor架构（FSMComponent、HealthCmp、NumericComponent等）

### UI开发
使用UI生成器时：

1. **始终先读取预制体配置** - 包含匹配规则
2. **按功能匹配，不只按名称** - 货币显示用CurrencyItem，物品用CommonGoodsItem
3. **添加描述性注释** - 帮助工具正确匹配元素
4. **遵循设计规则** - 防止元素超出边界，使用布局组件
5. **检查结构完整性** - 确保正确的父子关系

### 配置开发
- **Excel作为源**: 在Excel文件中编辑游戏数据
- **类型安全**: Luban生成强类型C#类
- **热重载**: 配置更改不需要代码重编译

### 技能开发
- **通过Buff配置**: 技能效果通过配置Buff来实现
- **组合Buff**: 复杂技能可以通过多个Buff组合
- **目标选择**: 使用TargetSelector系统选择技能目标
- **触发时机**: 通过TriggerType控制技能触发时机
- **参考文档**: 查看`GameLogic/GameActor/Buff/`目录下的配置说明文档

## 拖拽系统开发

### 拖拽系统架构
项目使用自定义的拖拽系统，支持从UI拖拽到3D世界放置塔防单位：

- **WorldDragDrop**: 核心拖拽组件，处理UI拖拽逻辑
- **WorldDragDropHelper**: 辅助类，连接拖拽与业务逻辑
- **GridCellHighlighter**: 网格高亮器，显示可放置区域
- **DragItemData**: 拖拽数据管理，处理数量和UI状态
- **TowerPlacementManager**: 塔放置管理器，统一处理点击和拖拽模式

### 拖拽系统常见问题修复

#### 1. 拖拽时UI消失问题
**问题**: 拖拽开始时UI元素被隐藏，用户看不到拖拽的物品
**位置**: `DragItemData.cs:114-120`
**修复**: 改为设置半透明而不是隐藏UI
```csharp
// 获取CanvasGroup用于透明度控制
var canvasGroup = uiElement.GetComponent<CanvasGroup>();
if (canvasGroup == null)
{
    canvasGroup = uiElement.AddComponent<CanvasGroup>();
}
// 设置为半透明而不是隐藏
canvasGroup.alpha = 0.3f;
canvasGroup.blocksRaycasts = false;
```

#### 2. 拖拽预览不跟随鼠标问题
**问题**: 拖拽时预览对象位置不正确，不跟随鼠标移动
**位置**: `WorldDragDrop.cs:177-193`
**修复**: 正确使用RectTransformUtility进行坐标转换
```csharp
RectTransform canvasRect = m_canvas.transform as RectTransform;
Vector2 localPoint;

if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
    canvasRect,
    eventData.position,
    m_canvas.worldCamera ?? Camera.main,
    out localPoint))
{
    m_rectTransform.position = canvasRect.TransformPoint(localPoint);
}
```

#### 3. 网格高亮不显示问题
**问题**: 拖拽时网格没有高亮显示可放置/不可放置区域
**位置**: `GridCellHighlighter.cs:45-58`
**修复**: 改进材质创建逻辑，多级备用方案
```csharp
// 首先尝试加载项目材质
var projectMaterial = Resources.Load<Material>("Custom_GridCellHighlight");
if (projectMaterial != null)
{
    highlightMaterial = new Material(projectMaterial);
}
else
{
    // 查找自定义Shader，然后是默认Shader，最后是备用Shader
    Shader shader = Shader.Find("Custom/GridCellHighlight") ??
                    Shader.Find("Sprites/Default") ??
                    Shader.Find("Unlit/Transparent");
    if (shader != null)
        highlightMaterial = new Material(shader);
}
```

### 拖拽系统开发注意事项
- **材质依赖**: 确保项目中存在`Assets/Shaders/GridCellHighlight.shader`和对应材质
- **Canvas设置**: UI元素需要有CanvasGroup组件用于透明度控制
- **相机配置**: 正确设置worldCamera用于坐标转换
- **网格系统**: 确保TowerDefenseGridSystem实例存在并正确初始化
- **事件绑定**: WorldDragDropHelper需要正确绑定所有拖拽事件回调

## 网格系统开发

### 网格区域颜色绘制
项目为TowerDefenseGridSystem添加了网格区域颜色绘制功能，可以实时显示网格状态：

- **可放置区域**: 绿色半透明
- **不可放置区域**: 红色半透明
- **已占用区域**: 灰色半透明
- **动态更新**: 网格状态变化时自动更新颜色

#### 配置选项
```csharp
// 网格区域颜色绘制设置
enableGridAreaRendering = true;        // 启用网格区域绘制
useMeshRendering = true;               // 使用Mesh渲染（高性能）
showGridBackground = true;             // 显示网格背景
gridBackgroundAlpha = 0.1f;            // 背景透明度
gridUpdateFrequency = 0.1f;            // 更新频率（秒）
```

#### 使用方法
```csharp
// 获取网格系统
TowerDefenseGridSystem gridSystem = TowerDefenseGridSystem.Instance;

// 动态控制网格显示
gridSystem.ToggleGridAreaRendering(true);     // 启用网格区域绘制
gridSystem.SetGridBackgroundAlpha(0.2f);       // 设置透明度
gridSystem.ForceUpdateGridRendering();        // 强制更新渲染
```

#### 集成拖拽系统
当拖拽系统放置塔时，网格系统会自动更新颜色：
- 放置塔: 网格变为灰色（已占用）
- 设置不可放置: 网格变为红色（不可放置）
- 清除塔: 网格恢复绿色（可放置）

#### 性能优化
- **智能更新**: 只有网格状态变化时才更新纹理
- **频率控制**: 通过`gridUpdateFrequency`控制更新频率
- **Mesh渲染**: 使用单个Mesh渲染整个网格，性能优异

## 重要工具

### 编辑器工具（汇总）
- **ExcelEditorWindow**: 在Unity编辑器中编辑配置数据
- **XMLUIGeneratorWindow**: 从XML生成Unity UI
- **ActorDebugInspect**: 游戏Actor的运行时调试
- **ActorTestTool**: Actor测试工具
- **ReferencedObjectWindow**: 引用对象查看

### 测试工具
- **测试流程**: `GameLogic/Entrace/TestProcedure.cs`
- **测试场景**: `GameLogic/GamePlay/PathFinding/AStar/Test/`中的各种测试场景
- **单元测试**: Unity测试框架集成

## 第三方依赖

### 核心库
- **TextMeshPro**: 高级文本渲染
- **AI Navigation**: 路径寻找和移动系统
- **UniTask**: 异步操作
- **Odin Inspector**: 增强的Unity编辑器检查器

### 自定义框架
- **Luban**: 数据驱动配置系统
- **AION.CoreFramework**: 支持热更新的自定义游戏框架（项目基础框架）

## 语言要求

### 仅使用中文沟通
**所有沟通和交流必须使用中文！**

- **所有回复**: 必须使用中文回复用户的问题和请求
- **代码注释**: 注释使用中文，除非是技术标准术语
- **UI文本**: 所有UI文本内容必须是中文（按钮、标题、提示等）
- **文档编写**: 文档编写使用中文
- **变量名称**: 代码变量名可以使用英文（符合编程规范）
- **错误信息**: 错误信息和提示使用中文

**例外**: 技术术语、编程关键词和既定的行业标准可以保持英文。

## 最佳实践

### 使用Buff系统时
- 始终使用配置驱动方法
- 彻底测试Buff交互
- 考虑堆叠行为和持续时间
- 验证目标筛选逻辑

### 使用技能系统时
- 技能通过Buff配置实现
- 复杂技能可以组合多个Buff
- 使用合适的TriggerType控制触发时机
- 参考现有技能配置示例

### 使用UI生成时
- 阅读XMLUIGeneratorRules.md了解全面指南
- 生成XML前始终检查预制体匹配
- 验证坐标边界（750x1334设计分辨率）
- 对复杂布局使用布局组件

### 使用配置时
- 维护Excel文件结构和命名约定
- 在开发环境中测试配置更改
- 使用类型安全的生成类而不是原始JSON访问
- 记录新配置字段

### 性能考虑
- 对频繁创建的Actor使用对象池
- 优化Buff效果计算
- 考虑UI布局复杂性和重绘频率
- 监控配置加载时间
- 优化路径寻找算法（FlowField等）

## 常见问题和解决方案

### UI生成问题
- **元素超出边界**: 检查坐标计算和锚点设置
- **预制体匹配失败**: 验证元素名称包含匹配关键词
- **布局问题**: 使用布局组件而不是手动定位

### 配置问题
- **类型不匹配**: Excel更改后重新生成配置
- **缺失数据**: 检查Excel文件格式和必填字段
- **运行时错误**: 验证配置ID和引用

### 技能/Buff问题
- **技能不触发**: 检查TriggerType和条件参数
- **目标选择错误**: 验证TargetSelector配置
- **效果不正确**: 检查ValueParams格式和参数顺序

### 性能问题
- **Buff系统**: 优化Buff更新频率和效果计算
- **UI性能**: 减少不必要的布局重计算
- **内存泄漏**: 正确清理Actor和事件监听器
- **路径寻找**: 使用FlowField等高效算法
