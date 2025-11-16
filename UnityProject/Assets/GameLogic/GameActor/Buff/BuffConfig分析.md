# BuffConfig 配置分析与建议

## 当前配置字段分析

### 已有字段
1. **Id** - BuffID ✓
2. **Name** - 名称 ✓
3. **Desc** - 描述 ✓
4. **BuffType** - Buff类型（PropertyMod/Heal/Damage/Status）✓
5. **TargetType** - 作用目标（Friendly/Enemy）✓
6. **TriggerType** - 触发类型（Immediate/Interval/Probability）✓
7. **StatusId** - 状态ID（用于眩晕、击退等）✓
8. **ValueParams** - 数值参数列表（分号分隔的float）✓
9. **Duration** - 持续时间 ✓
10. **TickInterval** - 触发间隔 ✓
11. **MaxStacks** - 最大叠加层数 ✓

## 配置使用说明

### 1. 数值加成类 Buff（PropertyMod）

**ValueParams 格式：**
- `[NumericType, Value, ModifierType(可选)]`
- 例如：`1004;0.2;1` 表示攻击力+20%，使用百分比加法
- 例如：`1004;10;0` 表示攻击力+10，使用固定值

**NumericType 常用值：**
- `1004` = Attack（攻击力）
- `1000` = Speed（速度）
- `1003` = AttackSpeed（攻击速度）
- `1005` = Defense（防御）

**ModifierType：**
- `0` = Flat（固定值）
- `1` = PercentAdd（百分比加法）
- `2` = PercentMult（百分比乘法）

### 2. 治疗类 Buff（Heal）

**ValueParams 格式：**
- `[HealAmount]`
- 例如：`50` 表示每次治疗50点生命值

### 3. 伤害类 Buff（Damage）

**ValueParams 格式：**
- `[DamageAmount]`
- 例如：`10` 表示每次造成10点伤害

### 4. 状态类 Buff（Status）

**StatusId 对应：**
- `1` = 眩晕（Stun）
- `4` = 击退（Knockback）
- 其他状态ID需要根据实际定义

**ValueParams 格式：**
- 对于概率触发：`[Probability]`，例如 `0.2` 表示20%概率
- 对于其他触发：可以为空或包含额外参数

## 发现的问题与建议

### ⚠️ 问题1：穿透次数支持不足

**问题描述：**
- 用户需要"增加穿透次数"的Buff
- 当前配置中没有直接支持穿透次数的字段
- BulletCmp 中有 `MaxPenetrationCount` 属性

**解决方案：**
1. **方案A（推荐）**：使用 PropertyMod 类型，通过自定义 NumericType 来支持
   - 在 NumericType 枚举中添加 `PenetrationCount = 1008`
   - ValueParams: `[1008;2;0]` 表示穿透次数+2
   - BulletCmp 在初始化时读取这个数值

2. **方案B**：扩展 BuffConfig，添加 `SpecialEffectType` 字段
   - 但这需要修改 Luban 配置表结构

**建议：** 使用方案A，因为穿透次数本质上也是一种属性修改

### ⚠️ 问题2：概率值不够明确

**问题描述：**
- 概率触发（Probability）时，概率值存储在 ValueParams[0]
- 但 ValueParams 可能被用于其他用途，不够直观

**当前实现：**
- 已实现：概率触发时，从 ValueParams[0] 读取概率值（0-1之间）

**建议：**
- 保持当前实现即可，因为概率值通常只在概率触发时使用
- 如果后续需要更复杂的概率计算，可以考虑扩展

### ⚠️ 问题3：状态效果实现不完整

**问题描述：**
- StatusId 只是一个ID，没有定义状态效果的具体行为
- 眩晕、击退等效果需要在代码中实现

**建议：**
1. 创建 StatusEffectComponent 来处理状态效果
2. 或者扩展 StatuConfig，添加状态效果的具体参数
3. 当前 BaseBuff.ApplyStatus() 方法已预留接口

### ⚠️ 问题4：ValueParams 格式不够规范

**问题描述：**
- 不同 BuffType 使用不同的 ValueParams 格式
- 没有统一的文档说明

**建议：**
- 在配置表注释中说明每种类型的格式
- 或者在代码中添加更详细的验证和错误提示

## 配置示例

### 示例1：攻击力+20%（持续20秒）
```
BuffType: PropertyMod
TriggerType: Immediate
ValueParams: 1004;0.2;1
Duration: 20
MaxStacks: 0
```

### 示例2：持续治疗（每20秒治疗一次，持续50秒）
```
BuffType: Heal
TriggerType: Interval
ValueParams: 50
Duration: 50
TickInterval: 20
MaxStacks: 2
```

### 示例3：持续伤害（每15秒造成10点伤害）
```
BuffType: Damage
TriggerType: Interval
ValueParams: 10
Duration: 0（永久，直到手动移除）
TickInterval: 15
MaxStacks: 1
```

### 示例4：20%概率眩晕（攻击时触发）
```
BuffType: Status
TriggerType: Probability
StatusId: 1
ValueParams: 0.2
Duration: 0（立即效果）
MaxStacks: 0
```

### 示例5：增加穿透次数+2
```
BuffType: PropertyMod
TriggerType: Immediate
ValueParams: 1008;2;0（假设1008是PenetrationCount）
Duration: 0（永久，直到手动移除）
MaxStacks: 0
```

## 总结

### 当前配置基本满足需求 ✓
- 支持数值加成 ✓
- 支持治疗和伤害 ✓
- 支持状态效果（需要实现）✓
- 支持概率触发 ✓

### 需要改进的地方
1. **穿透次数**：建议通过扩展 NumericType 来支持
2. **状态效果**：需要实现具体的状态效果逻辑
3. **文档**：建议在配置表中添加字段说明

### 建议的后续工作
1. 在 NumericType 中添加 `PenetrationCount` 枚举值
2. 实现 StatusEffectComponent 处理眩晕、击退等效果
3. 在配置表注释中添加 ValueParams 格式说明
4. 添加配置验证，确保 ValueParams 格式正确

