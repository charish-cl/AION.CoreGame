# XML UI 生成器使用说明

## 📋 概述

XML UI 生成器是一个 Unity 编辑器工具，用于将 WPF 风格的 XML 结构转换为 Unity UI 组件。它支持自动识别图片布局、引用项目预制体、智能布局等功能。

## 🚨 重要提示（AI 使用前必读）

### ⚠️ AI 生成 XML 的强制流程

**在使用此工具生成 XML 之前，AI 必须严格遵循以下步骤：**

#### 步骤 1：读取配置文件（必须！）
```bash
读取文件：Assets/Editor/XMLUIGenerator/XMLUIGeneratorPrefabConfig.json
```

**必须了解的信息：**
- ✅ `prefabSearchPaths`：预制体搜索路径
- ✅ `prefabAliases`：预制体别名映射
- ✅ **`prefabMatchRules`：匹配规则（最重要！）**

#### 步骤 2：根据图片识别功能匹配预制体（必须！）
**AI 必须分析图片中的 UI 元素功能，然后根据 JSON 配置中的规则匹配：**

- 🔍 **识别货币显示**（金币、钻石等）→ 匹配规则：`CurrencyItem` 预制体
- 🔍 **识别物品/装备显示** → 匹配规则：`CommonGoodsItem` 预制体
- 🔍 **识别其他元素** → 根据 JSON 中的 `prefabMatchRules` 规则匹配

**重要：功能逻辑必须匹配**
- **不要强行匹配预制体**：只有功能逻辑完全匹配时才使用 `ref`
- **功能不符时不使用 ref**：如果元素的功能与预制体的设计用途不符，应该正常生成子元素
- **判断标准**：
  - ✅ 货币显示 → 使用 CurrencyItem（功能匹配）
  - ✅ 物品/装备显示 → 使用 CommonGoodsItem（功能匹配）
  - ❌ 设施卡片、建筑卡片等 → 不使用预制体（功能不匹配）
  - ❌ 按钮、面板等通用UI → 不使用预制体（除非有专门的预制体）

**匹配逻辑：**
1. 分析图片，识别 UI 元素的功能（如"顶部金币栏"、"物品列表"等）
2. 在 JSON 的 `prefabMatchRules` 中查找匹配的关键词
3. 确定应该使用的预制体名称

#### 步骤 3：生成 XML 时添加注释（必须！）
**每个可能使用预制体的元素都要添加功能注释：**

```xml
<!-- 顶部金币显示 - 工具会自动匹配 CurrencyItem 预制体 -->
<Panel Name="CoinPanel" X="50" Y="0" Width="150" Height="40" Anchor="MiddleRight"/>

<!-- 物品列表项 - 工具会自动匹配 CommonGoodsItem 预制体 -->
<Button Name="Item1" Width="100" Height="100"/>
```

**注释格式：**
- `<!-- 功能描述 -->` - 说明元素的功能
- 工具会根据注释内容和元素名称自动匹配预制体

#### 步骤 4：检查元素层级关系（必须！）
**生成 XML 后，必须检查以下内容：**

1. **视觉位置检查**：
   - 图片中位于某个容器内的元素，是否作为该容器的子元素？
   - 例如：左侧装备槽位内的所有槽位，是否都在 `LeftEquipmentSlots` 内？

2. **功能分组检查**：
   - 功能相关的元素是否都在同一个父容器内？
   - 例如：所有左侧装备槽位是否都在 `LeftEquipmentSlots` 内？

3. **布局顺序检查**：
   - 元素的垂直顺序是否符合图片（从上到下）？
   - 例如：CharacterStats 是否在角色下方、道具网格上方？

4. **区域归属检查**：
   - 元素是否属于正确的区域容器？
   - 例如：CharacterStats 是否在 CharacterArea 内，而不是在 EquipmentManagementArea 内？

## 📁 文件结构

```
Assets/Editor/XMLUIGenerator/
├── README.md                          # 本说明文档
├── XMLUIGeneratorWindow.cs            # 核心生成器代码
├── XMLUIGeneratorPrefabConfig.json    # 预制体配置和匹配规则（重要！）
└── XMLUIGeneratorRules.md             # 详细设计规则文档
```

## 🔧 使用流程（AI 生成 XML 时）

### 步骤 1：读取配置文件
```bash
读取文件：Assets/Editor/XMLUIGenerator/XMLUIGeneratorPrefabConfig.json
```

**关键信息：**
- `prefabSearchPaths`：预制体搜索路径
- `prefabAliases`：预制体别名映射
- `prefabMatchRules`：**匹配规则（最重要！）**

### 步骤 2：分析图片并匹配规则
根据图片识别到的 UI 元素：
- 货币显示（金币、钻石等）→ 匹配规则：`CurrencyItem`
- 物品/装备显示 → 匹配规则：`CommonGoodsItem`
- 其他元素根据 JSON 规则匹配

### 步骤 3：生成 XML
在生成的 XML 中：
1. **添加功能注释**：说明每个元素的功能
2. **使用 ref 属性**（可选）：如果自动匹配失败，手动指定
3. **遵循设计规则**：参考 `XMLUIGeneratorRules.md`

### 示例 XML 生成
```xml
<!-- 顶部金币显示 - 工具会自动匹配 CurrencyItem 预制体 -->
<Panel Name="CoinPanel" X="50" Y="0" Width="150" Height="40" Anchor="MiddleRight"/>

<!-- 物品列表项 - 工具会自动匹配 CommonGoodsItem 预制体 -->
<Button Name="Item1" Width="100" Height="100"/>
```

## 📖 配置文件说明

### XMLUIGeneratorPrefabConfig.json

**prefabMatchRules**：匹配规则数组
- `description`：规则描述
- `matchPatterns`：匹配模式数组
  - `nameContains`：关键词列表（元素名称或注释包含这些词时匹配）
  - `prefabName`：匹配到的预制体名称

**示例规则：**
```json
{
  "prefabMatchRules": [
    {
      "description": "金币、钻石等货币显示使用 CurrencyItem 预制体",
      "matchPatterns": [
        {
          "nameContains": ["Coin", "Diamond", "Currency", "金币", "钻石", "货币"],
          "prefabName": "CurrencyItem"
        }
      ]
    }
  ]
}
```

## 🎯 匹配规则工作原理

1. **工具读取 XML 时**：
   - 检查元素是否有 `ref` 属性（手动指定）
   - 如果没有，检查元素名称和注释
   - 根据 `prefabMatchRules` 中的规则匹配
   - 匹配成功则自动添加 `ref` 属性

2. **匹配优先级**：
   - 手动指定的 `ref` > 自动匹配
   - 多个规则匹配时，使用第一个匹配的规则

## 📝 设计规则文档

详细的设计规则和最佳实践请参考：`XMLUIGeneratorRules.md`

包括：
- 基本结构规则
- 布局规则（Grid、HorizontalLayout、VerticalLayout）
- 坐标系统规则
- 超框检测规则
- 预制体引用规则

## ⚠️ 注意事项

1. **AI 必须在使用前读取 JSON 配置文件**
2. **根据图片功能识别，匹配对应的预制体规则**
3. **生成的 XML 必须包含功能注释**，便于工具自动匹配
4. **遵循设计规则**，确保生成的 UI 不超框、布局合理

## 🔄 更新配置

如果需要添加新的预制体或匹配规则：
1. 编辑 `XMLUIGeneratorPrefabConfig.json`
2. 在 `prefabMatchRules` 中添加新规则
3. 或在 `prefabAliases` 中添加别名映射

## 📞 问题排查

如果预制体匹配失败：
1. 检查 JSON 配置文件格式是否正确
2. 检查元素名称或注释是否包含匹配关键词
3. 检查预制体是否存在于搜索路径中
4. 查看 Unity Console 的日志输出
