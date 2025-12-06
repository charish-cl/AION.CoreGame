# XML UI生成器设计规则

## 概述
XML UI生成器是一个将WPF风格的XML结构转换为Unity UI组件的工具。它支持直接生成Canvas上的UI元素，并提供智能的布局和坐标转换功能。

## 0. 语言使用规则（强制要求）

### 0.1 中文优先原则
**所有UI文本内容必须使用中文！**

#### 0.1.1 强制要求
- **按钮文字**：必须使用中文，如"商店"、"皮膚"、"戰鬥"、"國王"、"莊園"
- **标题文本**：必须使用中文，如"商店"、"裝備詳情"、"角色信息"
- **标签文本**：必须使用中文，如"攻擊力"、"生命值"、"等級"、"價格"
- **提示文字**：必须使用中文，如"請輸入姓名"、"確認購買"、"操作成功"
- **错误提示**：必须使用中文，如"金币不足"、"背包已满"、"操作失败"

#### 0.1.2 禁止使用的语言
- ❌ **英文**：除代码名称、技术术语外，禁止使用英文
- ❌ **拼音**：禁止使用拼音代替中文
- ❌ **混合语言**：禁止中英文混合使用

#### 0.1.3 特殊例外情况
- ✅ **游戏ID**：可以使用数字和英文组合（如玩家ID、物品ID）
- ✅ **代码名称**：XML属性名、变量名使用英文（符合编程规范）
- ✅ **专有名词**：已确定的品牌名称或技术术语可保持原文
- ✅ **数值单位**：如"HP"、"MP"、"ATK"等游戏内通用缩写

#### 0.1.4 中文标点符号规则
- **使用全角标点**：逗号（，）、句号（。）、感叹号（！）、问号（？）
- **括号使用**：使用全角括号（（））
- **引号使用**：使用全角引号（""））
- **省略号**：使用全角省略号（……）

#### 0.1.5 示例对比
```xml
<!-- ❌ 错误：使用英文 -->
<Text Name="Title" text="Shop"/>
<Button Name="AttackBtn" Text="Attack"/>
<Text Name="HealthLabel" text="HP: 100/100"/>

<!-- ✅ 正确：使用中文 -->
<Text Name="Title" text="商店"/>
<Button Name="AttackBtn" Text="攻擊"/>
<Text Name="HealthLabel" text="生命值：100/100"/>
```

**AI生成XML时必须遵守此规则，所有显示给用户的文本都必须是中文！**

## 1. 基本结构规则

### 1.0 预制体引用规则（重要！）

#### 1.0.1 ref 属性
XML 支持使用 `ref` 属性引用项目中已有的预制体：

```xml
<!-- 引用预制体，不生成子物体，只设置位置和尺寸 -->
<!-- 顶部金币栏 - 使用 CurrencyItem 预制体 -->
<Panel Name="DiamondPanel" ref="CurrencyItem" X="-100" Y="0" Width="150" Height="40" Anchor="MiddleRight"/>

<!-- 物品列表项 - 使用 CommonGoodsItem 预制体 -->
<Panel Name="Item1" ref="CommonGoodsItem" X="0" Y="0" Width="100" Height="100" Anchor="Center"/>
```

**使用规则：**
- `ref` 属性指定预制体名称（不需要 .prefab 后缀）
- 使用 `ref` 时，**不会生成子物体**，直接实例化预制体
- 只设置位置（X、Y）、尺寸（Width、Height）和锚点（Anchor）
- 预制体的内部结构保持不变

**重要：功能逻辑必须匹配（AI 必须注意！）**
- **不要强行匹配预制体**：只有功能逻辑完全匹配时才使用 `ref`
- **功能不符时不使用 ref**：如果元素的功能与预制体的设计用途不符，应该正常生成子元素，而不是使用预制体
- **判断标准**：
  - ✅ 货币显示（金币、钻石等）→ 使用 CurrencyItem（功能匹配）
  - ✅ 物品/装备显示 → 使用 CommonGoodsItem（功能匹配）
  - ❌ 设施卡片 → 不使用 CommonGoodsItem（功能不匹配，设施卡片有自己的逻辑）
  - ❌ 按钮、面板等通用UI → 不使用预制体（除非有专门的预制体）

**重要：添加注释说明**
- **必须添加注释**说明该元素应该引用哪个预制体
- 注释格式：`<!-- 功能描述 - 使用 预制体名称 预制体 -->`
- 这样在生成 XML 时，可以清楚地知道应该使用哪个预制体
- 示例：
  ```xml
  <!-- 顶部金币显示 - 使用 CurrencyItem 预制体 -->
  <Panel Name="CoinPanel" ref="CurrencyItem" X="50" Y="0" Width="150" Height="40" Anchor="MiddleRight"/>
  
  <!-- 物品槽位 - 使用 CommonGoodsItem 预制体 -->
  <Panel Name="GoodsSlot" ref="CommonGoodsItem" X="0" Y="0" Width="100" Height="100" Anchor="Center"/>
  ```

#### 1.0.2 预制体自动匹配规则
工具在生成 XML 时会自动读取配置文件中的匹配规则，根据元素名称和注释自动添加 `ref` 属性：

**匹配规则配置**（在 `XMLUIGeneratorPrefabConfig.json` 中）：
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
    },
    {
      "description": "物品、装备等使用 CommonGoodsItem 预制体",
      "matchPatterns": [
        {
          "nameContains": ["Item", "Goods", "Good", "物品", "装备", "道具"],
          "prefabName": "CommonGoodsItem"
        }
      ]
    }
  ]
}
```

**匹配逻辑：**
- 工具会检查元素名称（Name 属性）和注释内容
- 如果包含规则中的关键词，自动添加对应的 `ref` 属性
- 例如：名称包含 "Coin" 或注释包含 "金币"，会自动使用 `CurrencyItem` 预制体

**手动指定优先级更高：**
- 如果 XML 中已经指定了 `ref` 属性，不会进行自动匹配
- 手动指定的 `ref` 优先级高于自动匹配

**AI 必须注意：功能逻辑匹配原则（重要！）**
- **不要强行匹配**：即使名称包含关键词，如果功能逻辑不匹配，也不应该使用预制体
- **判断标准**：
  - ✅ **货币显示**（金币、钻石、星星等）→ 使用 CurrencyItem（功能匹配）
  - ✅ **物品/装备**（背包物品、装备槽位等）→ 使用 CommonGoodsItem（功能匹配）
  - ❌ **设施卡片**（升级卡片、建筑卡片等）→ 不使用预制体（功能不匹配，设施有自己的逻辑）
  - ❌ **按钮、面板等通用UI** → 不使用预制体（除非有专门的预制体）
- **如果不确定**：宁可不用预制体，正常生成子元素，也不要强行匹配
- **示例**：
  ```xml
  <!-- ✅ 正确：货币显示使用 CurrencyItem -->
  <Panel Name="CoinPanel" ref="CurrencyItem" X="50" Y="0" Width="150" Height="40" Anchor="MiddleRight"/>
  
  <!-- ✅ 正确：物品显示使用 CommonGoodsItem -->
  <Button Name="Item1" ref="CommonGoodsItem" Width="100" Height="100"/>
  
  <!-- ❌ 错误：设施卡片不应该使用 CommonGoodsItem -->
  <Button Name="Facility1" ref="CommonGoodsItem" Width="200" Height="180"/>
  
  <!-- ✅ 正确：设施卡片正常生成子元素 -->
  <Button Name="Facility1" Width="200" Height="180">
      <Image Name="Facility1Icon" X="0" Y="30" Width="100" Height="100" Anchor="TopCenter" sprite=""/>
      <Text Name="Facility1Level" X="0" Y="-30" Width="200" Height="30" text="等級5" fontSize="18"/>
  </Button>
  ```

#### 1.0.3 预制体查找机制
工具会按以下顺序查找预制体：

1. **配置文件别名**：`Assets/Editor/XMLUIGeneratorPrefabConfig.json` 中定义的别名
2. **直接路径**：如果 ref 值以 "Assets/" 开头，直接作为路径
3. **搜索路径**：在配置文件中定义的 `prefabSearchPaths` 中搜索
4. **全局搜索**：使用 AssetDatabase 全局搜索同名预制体

#### 1.0.4 配置文件格式
创建 `Assets/Editor/XMLUIGeneratorPrefabConfig.json`：

```json
{
  "prefabSearchPaths": [
    "Assets/Game/UIForm/Common",
    "Assets/Game/UIComponent",
    "Assets/Game/Prefab"
  ],
  "prefabAliases": {
    "CommonGoodsItem": "Assets/Game/UIForm/Common/CommonGoodsItem.prefab",
    "CurrencyItem": "Assets/Game/UIForm/Common/CurrencyItem.prefab"
  }
}
```

**配置说明：**
- `prefabSearchPaths`：预制体搜索路径列表
- `prefabAliases`：预制体别名映射（可选，用于快速引用）

### 1.1 简化结构原则
- **不嵌套外层Panel**：避免不必要的嵌套结构
- **直接挂载**：XML的根元素直接作为Canvas的子元素
- **扁平化设计**：减少不必要的层级嵌套

```xml
<!-- ✅ 推荐：简洁结构 -->
<UI>
    <Panel Name="MainPanel"/>
    <Button Name="ActionButton"/>
    <Text Name="InfoText"/>
</UI>

<!-- ❌ 避免：过度嵌套 -->
<UI>
    <Panel Name="OuterPanel">
        <Panel Name="MiddlePanel">
            <Button Name="ActionButton"/>
        </Panel>
    </Panel>
</UI>
```

### 1.2 根元素命名
- 每个XML文件只能有一个根元素`<UI>`
- 根元素可以有多个子元素
- 所有子元素都直接挂载到Canvas上

### 1.3 元素层级关系规则（重要！避免结构错误）

**AI 必须严格遵循以下规则来判断元素的层级关系：**

#### 1.3.1 判断元素应该属于哪个父容器
1. **视觉位置判断**：
   - 如果元素在图片中明显位于某个容器的视觉范围内，应该作为该容器的子元素
   - 例如：左侧装备槽位内的所有槽位，都应该在 `LeftEquipmentSlots` 容器内

2. **功能分组判断**：
   - 功能相关的元素应该放在同一个父容器内
   - 例如：所有左侧装备槽位（武器、戒指等）应该都在 `LeftEquipmentSlots` 内

3. **布局逻辑判断**：
   - 如果元素需要相对于某个容器定位，应该作为该容器的子元素
   - 例如：戒指槽位相对于左侧装备槽位定位，应该在其内部

#### 1.3.2 常见错误示例
```xml
<!-- ❌ 错误：RingSlot2 应该属于 LeftEquipmentSlots，但放在了 CharacterArea 下 -->
<Panel Name="CharacterArea">
    <Panel Name="LeftEquipmentSlots">
        <Button Name="WeaponSlot"/>
        <Button Name="RingSlot1"/>
    </Panel>
    <Button Name="RingSlot2"/>  <!-- 错误：应该在 LeftEquipmentSlots 内 -->
</Panel>

<!-- ✅ 正确：所有左侧装备槽位都在 LeftEquipmentSlots 内 -->
<Panel Name="CharacterArea">
    <Panel Name="LeftEquipmentSlots">
        <Button Name="WeaponSlot"/>
        <Button Name="RingSlot1"/>
        <Button Name="RingSlot2"/>  <!-- 正确：在 LeftEquipmentSlots 内 -->
    </Panel>
</Panel>
```

#### 1.3.3 布局顺序规则（重要！）
**AI 必须根据图片判断元素的垂直顺序（从上到下）：**

1. **分析图片的垂直布局**：
   - 从上到下识别所有主要区域
   - 确定每个区域的相对位置关系
   - 例如：顶部栏 → 角色区域 → 装备管理区域 → 底部导航栏

2. **判断元素属于哪个区域**：
   - 根据元素在图片中的位置，判断它属于哪个主要区域
   - 例如：CharacterStats（生命值、攻击力）在角色下方、道具网格上方，应该属于 CharacterArea

3. **计算元素之间的相对位置（关键！）**：
   - **紧挨着的元素**：必须计算前一个元素的底部位置，下一个元素紧挨着它
   - **计算步骤**：
     1. 确定第一个元素的位置和高度
     2. 第二个元素的位置 = 第一个元素位置 + 第一个元素高度/2 + 间距
     3. 第三个元素的位置 = 第二个元素位置 + 第二个元素高度/2 + 间距
     4. 以此类推
   - **示例**：
     - 标题在 TopCenter，Y="0"，高度60，底部在 Y="-60"
     - Grid应该在标题下方，Y="-60"（紧挨着）
     - 下一个标题应该在Grid下方，Y="-60-400=-460"（Grid高度400）

4. **检查清单**（生成 XML 后必须检查）：
   - ✅ 所有功能相关的元素是否都在正确的父容器内？
   - ✅ 元素的层级关系是否符合图片中的视觉布局？
   - ✅ 是否有元素被错误地放在了错误的父容器下？
   - ✅ 元素的垂直顺序是否正确（从上到下）？
   - ✅ **紧挨着的元素是否真的紧挨着？**（计算位置是否正确）

## 2. 滚动区域设计

### 2.1 标准滚动结构
当内容超出显示区域时，使用以下结构：

```xml
<ScrollRect Name="ScrollArea" X="0" Y="0" Width="700" Height="400" Anchor="Center">
    <Panel Name="Viewport" X="0" Y="0" Width="700" Height="400" Anchor="Center" color="#FFFFFFFF">
        <Panel Name="Content" Width="700" Height="800" Anchor="TopCenter">
            <!-- 内容元素，高度可以超出视口 -->
            <Text Name="ScrollText1" text="项目1"/>
            <Text Name="ScrollText2" text="项目2"/>
            <Text Name="ScrollText3" text="项目3"/>
        </Panel>
    </Panel>
</ScrollRect>
```

### 2.2 滚动区域特征识别
- **ScrollRect组件**：自动创建滚动功能
- **Viewport面板**：作为滚动视口，通常有背景色
- **Content面板**：内容容器，高度可超出视口

## 3. 布局组件使用规则

### 3.1 网格布局 (Grid)
适用于商品列表、物品网格等需要规则排列的场景：

```xml
<Grid Name="ItemGrid"
      X="0" Y="0" Width="600" Height="400" Anchor="Center"
      cellSizeX="100" cellSizeY="100"
      spacingX="10" spacingY="10"
      constraint="FixedColumnCount" constraintCount="3">
    <Panel Name="Item1">
        <Text Name="Item1Text" text="物品1"/>
    </Panel>
    <Panel Name="Item2">
        <Text Name="Item2Text" text="物品2"/>
    </Panel>
    <Panel Name="Item3">
        <Text Name="Item3Text" text="物品3"/>
    </Panel>
</Grid>
```

**Grid属性说明：**
- `cellSizeX/cellSizeY`：每个单元格的尺寸
- `spacingX/spacingY`：单元格之间的间距
- `constraint`：布局约束类型
- `constraintCount`：约束数量

### 3.2 水平布局 (HorizontalLayoutGroup)
适用于导航栏、按钮组等水平排列的元素：

**使用规则：**
- **超过3个重复元素**：应该使用 HorizontalLayoutGroup
- **底部/顶部按钮组**：优先使用 HorizontalLayoutGroup（如底部导航栏、操作按钮组）
- **中间区域**：视情况而定，如果元素数量少（如6个装备槽）可以不使用

**Layout 子元素坐标规则（重要！）：**
- **贴边情况**：如果子元素需要贴父容器的边，可以不设置 X、Y 坐标，Layout 会自动处理
- **不贴边情况**：如果子元素与父容器边缘有间距，必须明确设置 X、Y 坐标
- **判断方法**：根据图片判断元素实际位置
  - 如果元素紧贴父容器边缘 → 贴边，可以不设置坐标
  - 如果元素与父容器边缘有间距 → 不贴边，必须设置坐标
- **示例**：
  - 底部导航栏：`BottomCenter + Y="0"`（贴屏幕底部），子元素可以不设置 Y
  - 操作按钮组：父容器贴底部，但按钮在导航栏上方，需要设置 `Y="130"`（不贴边）

```xml
<!-- 底部导航栏使用HorizontalLayoutGroup -->
<HorizontalLayout Name="BottomNav" X="0" Y="0" Width="750" Height="100" Anchor="BottomCenter"
                  spacing="30" paddingLeft="60" paddingRight="60">
    <Button Name="ShopButton" Width="120" Height="80" Text="商店"/>
    <Button Name="SkinButton" Width="120" Height="80" Text="皮膚"/>
    <Button Name="BattleButton" Width="120" Height="80" Text="戰鬥"/>
    <Button Name="KingButton" Width="120" Height="80" Text="國王"/>
    <Button Name="ManorButton" Width="120" Height="80" Text="莊園"/>
</HorizontalLayout>

<!-- 操作按钮组使用HorizontalLayoutGroup -->
<HorizontalLayout Name="ActionButtons" X="0" Y="0" Width="700" Height="60" Anchor="BottomCenter"
                  spacing="100" childAlignment="MiddleCenter">
    <Button Name="SortButton" Width="200" Height="50" Text="按品質排序"/>
    <Button Name="SynthesizeButton" Width="200" Height="50" Text="合成"/>
</HorizontalLayout>
```

**HorizontalLayoutGroup 属性：**
- `spacing`：元素之间的间距
- `paddingLeft/paddingRight/paddingTop/paddingBottom`：内边距
- `childAlignment`：子元素对齐方式（UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight）
- `childControlWidth/Height`：是否控制子元素尺寸（**默认 false**，一般不控制尺寸）
- `childForceExpandWidth/Height`：是否强制扩展子元素（**默认 true for Width**，水平布局通常强制扩展宽度）

**重要说明：**
- HorizontalLayout 的默认行为：不控制子元素尺寸，但强制扩展宽度以填充空间
- 这样可以实现按钮等元素自动平均分配空间
- 如果子元素需要固定尺寸，可以设置 `childControlWidth="true" childForceExpandWidth="false"`

**Layout 组件与子元素坐标规则（AI 必须注意）：**
- **父物体有 Layout 组件时，AI 在生成 XML 时必须考虑 padding 和 spacing**
- **子元素的坐标应该相对于可用区域（已减去 padding）**
- **AI 在生成 XML 时需要注意：**
  - 如果父物体使用了 Layout 组件，子元素的 X、Y 坐标应该相对于可用区域（已减去 padding）
  - 例如：父物体 `paddingLeft="60" paddingRight="60"`，子元素使用 `Anchor="MiddleLeft"` 时，X=0 实际位置会在 padding 之后（距离左边缘 60 像素）
  - 例如：父物体 `paddingTop="20" paddingBottom="20"`，子元素使用 `Anchor="TopCenter"` 时，Y=0 实际位置会在 padding 之后（距离顶部 20 像素）
  - **AI 必须手动计算这些偏移，确保子元素不超出可用区域**

### 3.3 垂直布局 (VerticalLayoutGroup)
适用于列表项、菜单项等垂直排列的元素：

```xml
<!-- 菜单列表使用VerticalLayoutGroup -->
<VerticalLayout Name="MenuList" X="0" Y="-100" Width="300" Height="200" Anchor="Center"
                spacing="10" paddingTop="20" paddingBottom="20">
    <Button Name="MenuItem1" Width="280" Height="50" Text="菜单项1"/>
    <Button Name="MenuItem2" Width="280" Height="50" Text="菜单项2"/>
    <Button Name="MenuItem3" Width="280" Height="50" Text="菜单项3"/>
</VerticalLayout>
```

**VerticalLayoutGroup 属性：**
- `spacing`：元素之间的间距
- `paddingLeft/paddingRight/paddingTop/paddingBottom`：内边距
- `childAlignment`：子元素对齐方式
- `childControlWidth/Height`：是否控制子元素尺寸（**默认 false**）
- `childForceExpandWidth/Height`：是否强制扩展子元素（**默认 true for Height**）

**Layout 组件通用规则（AI 必须注意）：**
- **父物体有 Layout 组件时，AI 必须手动考虑 padding 和 spacing**
- **AI 必须确保子元素不超出父容器的可用区域（已减去 padding）**
- **AI 在生成 XML 时必须手动计算：**
  - 如果父物体使用了 Layout 组件，子元素的 X、Y 坐标应该相对于可用区域（已减去 padding）
  - 例如：父物体 `paddingLeft="60" paddingRight="60"`，子元素使用 `Anchor="MiddleLeft"` 时，X=0 实际位置会在 padding 之后（距离左边缘 60 像素）
  - **AI 必须手动计算这些偏移，确保子元素不超框**

## 4. 坐标系统规则

### 4.1 WPF风格坐标
XML使用WPF风格的坐标系统：

```xml
<!-- 顶部中心：向下偏移50像素 -->
<Panel Name="TopBar" X="0" Y="-50" Anchor="TopCenter"/>

<!-- 左上角：向右偏移100像素，向下偏移50像素 -->
<Panel Name="TopLeftPanel" X="100" Y="-50" Anchor="TopLeft"/>

<!-- 底部中心：向上偏移50像素 -->
<Panel Name="BottomBar" X="0" Y="50" Anchor="BottomCenter"/>
```

### 4.2 坐标转换规则
生成器会根据Anchor类型自动调整坐标：

- **Top系列 (Y=1.0)**：Y=-50 表示从顶部向下50像素
- **Bottom系列 (Y=0.0)**：Y=50 表示从底部向上50像素
- **Center系列 (Y=0.5f)**：Y值直接作为相对中心的偏移

### 4.3 超框检测规则（重要！）
写XML时必须严格遵守以下规则：

#### 4.3.1 屏幕边界限制
- **设计分辨率**：750 x 1334
- **所有元素**（包括子元素）的实际坐标必须在 (0, 0) 到 (750, 1334) 范围内
- **不能超出屏幕边界**，即使部分超出也不允许

#### 4.3.2 坐标计算规则（重要！）
Unity 坐标系统需要根据子物体、父物体、anchor、pivot 综合计算：

**基本公式：**
```
子元素实际位置 = 父元素锚点位置 + 子元素anchoredPosition + 子元素尺寸 * pivot偏移
```

**详细计算：**
1. **父元素锚点位置**：
   - 对于点锚点（anchorMin == anchorMax）：锚点位置 = 父元素rect位置 + 父元素rect尺寸 * anchor
   - 对于拉伸锚点（anchorMin != anchorMax）：需要考虑 offsetMin 和 offsetMax

2. **子元素anchoredPosition**：
   - 这是相对于子元素锚点的偏移量
   - 如果子元素是点锚点，anchoredPosition 直接是偏移
   - 如果子元素是拉伸锚点，需要考虑 offsetMin/offsetMax

3. **Pivot 影响**：
   - Pivot 影响元素的实际位置
   - 实际位置 = 锚点位置 + anchoredPosition - (sizeDelta * pivot)

**对于不同锚点类型：**
- **Top系列 (Y=1.0)**：实际Y = 父元素顶部Y + anchoredPosition.y（Y为负值表示向下）
- **Bottom系列 (Y=0.0)**：实际Y = 父元素底部Y + anchoredPosition.y（Y为正值表示向上）
- **Center系列 (Y=0.5f)**：实际Y = 父元素中心Y + anchoredPosition.y
- **Left系列 (X=0.0)**：实际X = 父元素左边X + anchoredPosition.x（X为正值表示向右）
- **Right系列 (X=1.0)**：实际X = 父元素右边X + anchoredPosition.x（X为负值表示向左）

**父物体有 Layout 组件时的特殊规则（AI 必须注意）：**
- **AI 必须手动考虑 padding**：在生成 XML 时，AI 需要手动计算 padding 对坐标的影响
  - Left 系列 anchor：X 坐标需要加上 `padding.left`
  - Right 系列 anchor：X 坐标需要减去 `padding.right`
  - Bottom 系列 anchor：Y 坐标需要加上 `padding.bottom`
  - Top 系列 anchor：Y 坐标需要减去 `padding.top`
  - Center 系列：padding 对称，通常不需要特殊处理

- **AI 必须确保不超框**：
  - AI 在生成 XML 时必须计算子元素是否超出父容器的可用区域（已减去 padding）
  - 可用区域 = 父容器尺寸 - padding
  - 子元素的坐标 + 尺寸不能超出可用区域

- **AI 生成 XML 时的计算示例**：
  - 父物体：`Width="750" paddingLeft="60" paddingRight="60"`
  - 可用宽度 = 750 - 60 - 60 = 630
  - 子元素使用 `Anchor="MiddleLeft"`，X=0 时实际位置在距离左边缘 60 像素处
  - 如果子元素 `Width="100"`，需要确保 X + Width/2 ≤ 630/2，即 X ≤ 265
  - **AI 必须进行这些计算，确保不超框**

**超框检测建议：**
- 如果坐标计算复杂，可以使用**绝对布局**（直接设置 anchoredPosition，不考虑复杂计算）
- 使用 Layout 组件可以避免复杂的坐标计算
- 对于贴边元素，使用 X="0" 或 Y="0" 配合合适的 Anchor
- **父物体有 Layout 时，工具会自动处理 padding 和超框检测**

#### 4.3.3 贴边与不贴边元素规则（重要！）

**贴边元素的定义：**
- **贴边**：元素紧贴屏幕或父容器的边缘，没有间距
- **不贴边**：元素与屏幕或父容器边缘有间距，需要明确设置坐标

**贴边元素的规则：**
- **贴左边**：使用 `Anchor="MiddleLeft"` 或 `Anchor="TopLeft"` 或 `Anchor="BottomLeft"` + `X="0"`
- **贴右边**：使用 `Anchor="MiddleRight"` 或 `Anchor="TopRight"` 或 `Anchor="BottomRight"` + `X="0"`
- **贴顶部**：使用 `Anchor="TopCenter"` 或 `Anchor="TopLeft"` 或 `Anchor="TopRight"` + `Y="0"` 或 `Y="负值"`（从顶部向下）
- **贴底部**：使用 `Anchor="BottomCenter"` 或 `Anchor="BottomLeft"` 或 `Anchor="BottomRight"` + `Y="0"` 或 `Y="正值"`（从底部向上）

**不贴边元素的规则：**
- **不贴边**：元素需要与边缘保持距离，必须明确设置 X、Y 坐标
- **即使父容器贴边，子元素也可能不贴边**：需要根据实际布局设置坐标
- **示例**：
  - 父容器：`BottomCenter + Y="0"`（贴底部）
  - 子元素：`BottomCenter + Y="130"`（不贴边，距离底部 130 像素）

**Layout 组件与贴边的关系（AI 必须注意）：**
- **父容器贴边 ≠ 子元素贴边**：
  - 如果父容器使用 Layout 且贴边（如 `BottomCenter + Y="0"`）
  - 子元素仍然需要根据实际位置设置 Y 坐标
  - 例如：底部导航栏上方有操作按钮，即使父容器贴底部，操作按钮也需要设置 `Y="130"` 来定位到导航栏上方

- **使用 Layout 时子元素的坐标规则**：
  - **如果子元素需要贴父容器的边**：可以不设置 X、Y，Layout 会自动处理
  - **如果子元素不贴父容器的边**：必须明确设置 X、Y 坐标来定位
  - **判断标准**：看图片中元素的实际位置，如果与父容器边缘有间距，就是不贴边，需要设置坐标

**重要说明：**
- **只有贴边元素需要调整 Anchor**：不贴边的元素不需要特殊处理 Anchor
- **父元素贴边时，子元素坐标要特别注意**：如果父元素已经贴边（如 BottomCenter + Y="0"），子元素使用 BottomCenter + Y="-200" 会超出父容器
- **AI 必须根据图片判断**：元素是贴边还是不贴边，然后决定是否需要设置坐标

#### 4.3.4 超框检测检查清单
在生成XML前，必须检查：
1. ✅ 每个元素的 X + Width/2 是否 ≤ 750
2. ✅ 每个元素的 X - Width/2 是否 ≥ 0
3. ✅ 每个元素的 Y + Height/2 是否 ≤ 1334（对于Top系列）
4. ✅ 每个元素的 Y - Height/2 是否 ≥ 0（对于Bottom系列）
5. ✅ 所有子元素也要进行相同的检查
6. ✅ 考虑父元素的位置，计算子元素的绝对坐标

#### 4.3.5 结构完整性检查清单（重要！避免层级错误）

**在生成 XML 后，AI 必须进行以下检查：**

1. **视觉位置检查**：
   - ✅ 图片中位于某个容器内的元素，是否作为该容器的子元素？
   - ✅ 例如：左侧装备槽位内的所有槽位，是否都在 `LeftEquipmentSlots` 内？
   - ✅ 检查方法：在图片中画一条线，如果元素明显在某个容器的视觉范围内，应该作为子元素

2. **功能分组检查**：
   - ✅ 功能相关的元素是否都在同一个父容器内？
   - ✅ 例如：所有左侧装备槽位（武器、戒指等）是否都在 `LeftEquipmentSlots` 内？
   - ✅ 检查方法：如果多个元素功能相同或相关，应该放在同一个容器内

3. **布局顺序检查**：
   - ✅ 元素的垂直顺序是否符合图片（从上到下）？
   - ✅ 例如：CharacterStats 是否在角色下方、道具网格上方？
   - ✅ 检查方法：按照图片从上到下的顺序，列出所有主要元素，检查 XML 中的顺序是否一致

4. **区域归属检查**：
   - ✅ 元素是否属于正确的区域容器？
   - ✅ 例如：CharacterStats 是否在 CharacterArea 内，而不是在 EquipmentManagementArea 内？
   - ✅ 检查方法：根据元素的功能和位置，判断它应该属于哪个主要区域

5. **常见错误检查**：
   - ✅ 是否有元素被错误地放在了根元素下，而不是在对应的父容器内？
   - ✅ 是否有元素被放在了错误的父容器下？
   - ✅ 是否有元素的位置与其父容器不匹配？

**检查示例：**
```xml
<!-- ❌ 错误示例：RingSlot2 应该属于 LeftEquipmentSlots -->
<Panel Name="CharacterArea">
    <Panel Name="LeftEquipmentSlots">
        <Button Name="WeaponSlot"/>
        <Button Name="RingSlot1"/>
    </Panel>
    <Button Name="RingSlot2"/>  <!-- 错误：应该在 LeftEquipmentSlots 内 -->
</Panel>

<!-- ✅ 正确示例：所有左侧装备槽位都在 LeftEquipmentSlots 内 -->
<Panel Name="CharacterArea">
    <Panel Name="LeftEquipmentSlots">
        <Button Name="WeaponSlot"/>
        <Button Name="RingSlot1"/>
        <Button Name="RingSlot2"/>  <!-- 正确：在 LeftEquipmentSlots 内 -->
    </Panel>
</Panel>
```

#### 4.3.6 常见错误示例
```xml
<!-- ❌ 错误：元素超出右边界 -->
<Panel Name="BadPanel" X="700" Y="0" Width="100" Anchor="MiddleLeft"/>
<!-- 实际右边界 = 700 + 100/2 = 750，刚好在边界，但如果padding会超 -->

<!-- ❌ 错误：子元素超出父容器 -->
<Panel Name="Parent" X="0" Y="0" Width="200" Height="200" Anchor="TopLeft">
    <Panel Name="Child" X="150" Y="0" Width="100" Anchor="MiddleLeft"/>
    <!-- 子元素右边界 = 150 + 100/2 = 200，刚好在父容器内，但如果父容器有padding会超 -->
</Panel>

<!-- ✅ 正确：确保有足够边距 -->
<Panel Name="GoodPanel" X="0" Y="0" Width="200" Height="200" Anchor="TopLeft">
    <Panel Name="Child" X="50" Y="0" Width="100" Anchor="MiddleLeft"/>
    <!-- 子元素右边界 = 50 + 100/2 = 100，距离父容器右边界还有100像素 -->
</Panel>
```

## 5. 图片处理规则

### 5.1 Sprite Pivot处理
当加载的Sprite pivot不是(0.5, 0.5)时：
- 自动调整RectTransform的pivot
- 保持视觉位置不变
- 计算并调整anchoredPosition

### 5.2 图片加载方式
支持多种路径格式：

```xml
<!-- Resources路径 -->
<Image Name="Icon" sprite="Icons/shop_icon"/>

<!-- AssetDatabase路径 -->
<Image Name="Background" sprite="Assets/Textures/bg_main"/>

<!-- 文件名搜索 -->
<Image Name="PlayerAvatar" sprite="player_avatar"/>
```

## 6. 组件创建规则

### 6.1 基础组件

#### Panel (面板)
```xml
<Panel Name="Container" X="0" Y="0" Width="200" Height="100"
        Anchor="Center" color="#FFFFFFFF"/>
```

#### Image (图片)
```xml
<Image Name="Background" X="0" Y="0" Width="200" Height="100"
        Anchor="Center" sprite="bg_texture"/>
```

#### Text (文本)
```xml
<Text Name="Title" X="0" Y="0" Width="200" Height="50"
      text="标题" fontSize="24" color="#000000"
      alignment="MiddleCenter"/>
```

#### Button (按钮)
```xml
<Button Name="ActionBtn" X="0" Y="0" Width="120" Height="60"
        Anchor="Center" Text="按钮" FontSize="18"/>
```

### 6.2 复合组件

#### InputField (输入框)
```xml
<InputField Name="NameInput" X="0" Y="0" Width="200" Height="50"
            placeholder="请输入姓名" fontSize="16"/>
```

#### Slider (滑块)
```xml
<Slider Name="VolumeSlider" X="0" Y="0" Width="200" Height="20"
        minValue="0" maxValue="100" value="50"/>
```

#### Dropdown (下拉菜单)
```xml
<Dropdown Name="OptionSelect" X="0" Y="0" Width="200" Height="40"
            options="选项1,选项2,选项3"/>
```

## 7. 属性设置规则

### 7.1 基础属性
```xml
<!-- 尺寸和位置 -->
<Panel Name="Sample" Width="200" Height="100" X="50" Y="50"/>

<!-- 颜色 -->
<Image Name="ColoredImage" color="#FF0000FF"/>

<!-- 字体属性 -->
<Text Name="StyledText" fontSize="18" color="#FFFFFF"/>

<!-- 锚点 -->
<Panel Name="AnchoredPanel" Anchor="TopRight"/>
```

### 7.2 特殊属性
```xml
<!-- TextMeshPro特定属性 -->
<Text Name="TMProText" alignment="TopLeft" font="Arial"/>

<!-- Unity枚举类型 -->
<Image Name="ButtonImage" raycastTarget="true"/>

<!-- 自定义组件属性 -->
<CustomComponent Name="CustomCtrl" customProperty="value"/>
```

## 8. 命名规则

### 8.1 自动命名转换
- **Button自动前缀**：XML中的 "ButtonName" 会自动转换为 "m_btn_ButtonName"
- **其他组件保持原名**：Panel、Text、Image等保持XML中的名称

### 8.2 命名约定
```xml
<!-- 推荐：有意义的名称 -->
<Button Name="m_btn_Login" Text="登录"/>
<Panel Name="PlayerInfo" X="0" Y="0" Width="300" Height="200"/>
<Text Name="ScoreDisplay" text="得分：0"/>

<!-- 避免：模糊的名称 -->
<Button Name="Button1" Text="按钮1"/>
<Panel Name="Panel2"/>
<Text Name="Text3"/>
```

## 9. 错误处理规则

### 9.1 XML解析错误
- **清晰提示**：指出具体的行号和问题类型
- **常见错误**：
  - 标签不匹配
  - 属性值格式错误
  - 特殊字符未转义

### 9.2 属性设置错误
- **Warning级别**：不影响整体生成
- **自动跳过**：无法识别的属性会被忽略
- **详细日志**：记录具体的错误信息

### 9.3 组件创建失败
- **提供上下文**：显示创建失败的组件和原因
- **继续执行**：不会影响其他组件的创建

## 10. 最佳实践

### 10.1 结构设计
1. **保持扁平化**：避免过度嵌套
2. **合理分组**：相关元素放在一起
3. **使用布局组件**：简化复杂的定位逻辑

### 10.2 坐标规划
1. **相对定位**：优先使用相对Anchor
2. **避免硬编码**：使用百分比或布局组件
3. **考虑响应式**：考虑不同屏幕尺寸

### 10.3 性能优化
1. **避免深层嵌套**：减少Transform层级
2. **合理使用布局组件**：减少手动计算
3. **图片优化**：使用适当的压缩和尺寸

### 10.4 可维护性
1. **清晰命名**：使用有意义的名称
2. **合理结构**：逻辑分组
3. **注释说明**：复杂逻辑需要注释

## 11. 示例模板

### 11.1 基础UI模板
```xml
<?xml version="1.0" encoding="utf-8"?>
<UI>
    <Panel Name="MainPanel" X="0" Y="0" Width="750" Height="1334" Anchor="Center" color="#FFFFFFFF">
        <!-- 标题栏 -->
        <Text Name="Title" X="0" Y="-600" Width="750" Height="60" Anchor="TopCenter"
              text="应用标题" fontSize="32" color="#000000" alignment="MiddleCenter"/>

        <!-- 内容区域 -->
        <Panel Name="ContentArea" X="0" Y="0" Width="700" Height="1000" Anchor="Center">
            <Text Name="ContentText" text="内容区域" fontSize="24" color="#000000"/>
        </Panel>

        <!-- 底部按钮 -->
        <Button Name="ActionButton" X="0" Y="400" Width="200" Height="60"
                Anchor="BottomCenter" Text="确认" FontSize="18"/>
    </Panel>
</UI>
```

### 11.2 商店UI模板
```xml
<?xml version="1.0" encoding="utf-8"?>
<UI>
    <!-- 商店主面板 -->
    <Panel Name="ShopMainPanel" X="0" Y="0" Width="750" Height="1334" Anchor="Center" color="#FFE8E8E8">

        <!-- 顶部栏 -->
        <Panel Name="TopBar" X="0" Y="-550" Width="750" Height="100" Anchor="TopCenter" color="#FF2C2C2C">
            <Text Name="ShopTitle" X="0" Y="0" Width="200" Height="60"
                  text="商店" fontSize="28" color="#FFFFFFFF" alignment="MiddleCenter"/>
        </Panel>

        <!-- 商品滚动区域 -->
        <ScrollRect Name="ProductsScrollArea" X="0" Y="-50" Width="700" Height="800" Anchor="Center">
            <Panel Name="ScrollViewport" X="0" Y="0" Width="700" Height="800" Anchor="Center" color="#FFFFFFFF">
                <Grid Name="ProductsContent" Width="700" Height="1600" Anchor="TopCenter"
                      cellSizeX="330" cellSizeY="450"
                      spacingX="20" spacingY="20"
                      constraint="FixedColumnCount" constraintCount="2">
                    <Panel Name="Product1">
                        <Image Name="Product1Image" X="0" Y="-100" Width="300" Height="280" Anchor="TopCenter"/>
                        <Text Name="Product1Name" X="0" Y="50" Width="300" Height="30"
                              text="商品名称" fontSize="22" color="#333333" alignment="MiddleCenter"/>
                    </Panel>
                    <!-- 更多商品... -->
                </Grid>
            </Panel>
        </ScrollRect>
    </Panel>
</UI>
```

## 12. 常见问题解答

### 12.1 Q: 为什么我的UI元素位置不正确？
A: 检查以下几点：
- Anchor设置是否正确
- 坐标值是否超出了父容器范围
- 是否使用了合适的布局组件

### 12.2 Q: 如何创建滚动效果？
A: 使用ScrollRect组件结构，确保Content面板高度大于Viewport高度。

### 12.3 Q: 图片显示位置不对怎么办？
A: 检查Sprite的pivot设置，生成器会自动处理pivot调整。

### 12.4 Q: 为什么按钮没有响应点击？
A: 确保Button组件正确创建，并添加了必要的GraphicRaycaster组件。

### 12.5 Q: 如何优化UI性能？
A:
- 减少不必要的嵌套层级
- 使用布局组件替代手动定位
- 合理设置图片尺寸和格式

## 13. 更新日志

### v1.0
- 基础XML到Unity转换功能
- 支持常用UI组件
- 坐标系统转换

### v1.1
- 添加ScrollRect支持
- 优化坐标转换逻辑
- 改进Sprite pivot处理

### v1.2
- 移除自动检测功能，专注于XML结构设计
- 完善布局组件支持
- 优化超框检测和修复
- 添加详细规则文档

---

**注意**：本规则文档随XML生成器更新而更新，请确保使用最新版本。