using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameConfig;
using GameConfig.battle;
using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// Buff工厂类，根据BuffConfig创建Buff实例
    /// </summary>
    public static class BuffFactory
    {
        /// <summary>
        /// BuffType到Effect类型的映射字典（缓存，避免每次反射查找）
        /// </summary>
        private static Dictionary<EBuffType, Type> s_effectTypeCache = null;
        
        /// <summary>
        /// ConditionType到Condition类型的映射字典（缓存，避免每次反射查找）
        /// </summary>
        private static Dictionary<GameConfig.ETriggerType, Type> s_conditionTypeCache = null;
        
        /// <summary>
        /// TargetType到TargetSelector类型的映射字典（缓存，避免每次反射查找）
        /// </summary>
        private static Dictionary<GameConfig.ETargetType, Type> s_targetSelectorTypeCache = null;
        
        /// <summary>
        /// 初始化Effect类型缓存（使用反射扫描所有带BuffEffectAttribute的类）
        /// </summary>
        private static void InitializeEffectTypeCache()
        {
            if (s_effectTypeCache != null)
                return;
                
            s_effectTypeCache = new Dictionary<EBuffType, Type>();
            
            // 获取当前程序集中所有继承自BaseEffect的类
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type baseEffectType = typeof(BaseEffect);
            
            var effectTypes = assembly.GetTypes()
                .Where(t => 
                    t.IsClass && 
                    !t.IsAbstract && 
                    baseEffectType.IsAssignableFrom(t) &&
                    t.GetCustomAttribute<BuffEffectAttribute>() != null
                );
            
            foreach (var effectType in effectTypes)
            {
                var attribute = effectType.GetCustomAttribute<BuffEffectAttribute>();
                if (attribute != null)
                {
                    if (s_effectTypeCache.ContainsKey(attribute.BuffType))
                    {
                        Log.Warning($"BuffFactory: 发现重复的BuffType {attribute.BuffType}，类型 {effectType.Name} 将被忽略");
                        continue;
                    }
                    s_effectTypeCache[attribute.BuffType] = effectType;
                    Log.Info($"BuffFactory: 注册Effect类型 {effectType.Name} -> {attribute.BuffType}");
                }
            }
            
            if (s_effectTypeCache.Count == 0)
            {
                Log.Warning("BuffFactory: 未找到任何带BuffEffectAttribute的Effect类");
            }
        }
        
        /// <summary>
        /// 初始化Condition类型缓存（使用反射扫描所有带BuffConditionAttribute的类）
        /// </summary>
        private static void InitializETriggerTypeCache()
        {
            if (s_conditionTypeCache != null)
                return;
                
            s_conditionTypeCache = new Dictionary<GameConfig.ETriggerType, Type>();
            
            // 获取当前程序集中所有继承自BaseCondition的类
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type basETriggerType = typeof(BaseCondition);
            
            var conditionTypes = assembly.GetTypes()
                .Where(t => 
                    t.IsClass && 
                    !t.IsAbstract && 
                    basETriggerType.IsAssignableFrom(t) &&
                    t.GetCustomAttribute<BuffConditionAttribute>() != null
                );
            
            foreach (var conditionType in conditionTypes)
            {
                var attribute = conditionType.GetCustomAttribute<BuffConditionAttribute>();
                if (attribute != null)
                {
                    if (s_conditionTypeCache.ContainsKey(attribute.ConditionType))
                    {
                        Log.Warning($"BuffFactory: 发现重复的ConditionType {attribute.ConditionType}，类型 {conditionType.Name} 将被忽略");
                        continue;
                    }
                    s_conditionTypeCache[attribute.ConditionType] = conditionType;
                    Log.Info($"BuffFactory: 注册Condition类型 {conditionType.Name} -> {attribute.ConditionType}");
                }
            }
            
            if (s_conditionTypeCache.Count == 0)
            {
                Log.Warning("BuffFactory: 未找到任何带BuffConditionAttribute的Condition类");
            }
        }
        
        /// <summary>
        /// 初始化TargetSelector类型缓存（使用反射扫描所有带BuffTargetSelectorAttribute的类）
        /// </summary>
        private static void InitializeTargetSelectorTypeCache()
        {
            if (s_targetSelectorTypeCache != null)
                return;
                
            s_targetSelectorTypeCache = new Dictionary<GameConfig.ETargetType, Type>();
            
            // 获取当前程序集中所有继承自BaseTargetSelector的类
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type baseTargetSelectorType = typeof(BaseTargetSelector);
            
            var targetSelectorTypes = assembly.GetTypes()
                .Where(t => 
                    t.IsClass && 
                    !t.IsAbstract && 
                    baseTargetSelectorType.IsAssignableFrom(t) &&
                    t.GetCustomAttribute<BuffTargetSelectorAttribute>() != null
                );
            
            foreach (var targetSelectorType in targetSelectorTypes)
            {
                var attribute = targetSelectorType.GetCustomAttribute<BuffTargetSelectorAttribute>();
                if (attribute != null)
                {
                    if (s_targetSelectorTypeCache.ContainsKey(attribute.TargetType))
                    {
                        Log.Warning($"BuffFactory: 发现重复的TargetType {attribute.TargetType}，类型 {targetSelectorType.Name} 将被忽略");
                        continue;
                    }
                    s_targetSelectorTypeCache[attribute.TargetType] = targetSelectorType;
                    Log.Info($"BuffFactory: 注册TargetSelector类型 {targetSelectorType.Name} -> {attribute.TargetType}");
                }
            }
            
            if (s_targetSelectorTypeCache.Count == 0)
            {
                Log.Warning("BuffFactory: 未找到任何带BuffTargetSelectorAttribute的TargetSelector类");
            }
        }
        /// <summary>
        /// 根据BuffConfig创建Buff实例
        /// </summary>
        /// <param name="config">Buff配置</param>
        /// <param name="targetActor">目标Actor（可选）</param>
        /// <returns>创建的Buff实例</returns>
        public static BaseBuff CreateBuff(BuffConfig config, GameActor targetActor = null)
        {
            if (config == null)
            {
                Log.Error("BuffFactory: BuffConfig is null");
                return null;
            }
            
            // 创建Buff实例
            BaseBuff buff = new BaseBuff(
                config.Id.ToString(), 
                config.Duration
            );
            
            // 设置Buff属性
            buff.BuffId = config.Id;
            buff.TriggerType = config.TriggerType;
            buff.TickInterval = config.TickInterval;
            buff.StatusId = config.StatusId;
            
            // 检查是否有BuffEffects
            if (config.BuffEffects == null || config.BuffEffects.Count == 0)
            {
                Log.Warning($"BuffFactory: BuffConfig {config.Id} 没有BuffEffects");
                return buff;
            }
            
            // 遍历所有BuffEffect，创建对应的Effect和Modifier
            foreach (var buffEffect in config.BuffEffects)
            {
                if (buffEffect == null)
                    continue;
                
                // 创建Effect
                BaseEffect effect = CreateEffect(
                    buffEffect.Type,
                    targetActor,
                    buffEffect.ValueParams != null ? new List<float>(buffEffect.ValueParams) : new List<float>(),
                    null, // attackerNumeric在OnStart时设置
                    config.StatusId,
                    buffEffect.DamageType
                );
                
                if (effect != null)
                {
                    buff.AddEffect(effect);
                    
                    // 如果是PropertyMod类型，创建AttributeModifier
                    if (buffEffect.Type == EBuffType.PropertyMod)
                    {
                        AttributeModifier modifier = CreateAttributeModifier(buffEffect.ValueParams);
                        if (modifier != null)
                        {
                            buff.AddModifier(modifier);
                        }
                    }
                }
            }
            
            // 从第一个Effect的ValueParams中提取概率值（如果是概率触发或OnDeath触发）
            // 注意：概率值应该通过ConditionParams或其他方式设置，这里保留兼容性
            if ((config.TriggerType == ETriggerType.Probability || config.TriggerType == ETriggerType.OnDeath) 
                && config.BuffEffects != null && config.BuffEffects.Count > 0)
            {
                var firstEffect = config.BuffEffects[0];
                if (firstEffect != null && firstEffect.ValueParams != null && firstEffect.ValueParams.Count > 0)
                {
                    // 假设概率值在ValueParams的第一个位置（0-1之间）
                    buff.Probability = Mathf.Clamp01(firstEffect.ValueParams[0]);
                }
            }
            
            // 设置条件（如果有条件参数）
            if (config.ConditionParams != null && config.ConditionParams.Count > 0)
            {
                // ConditionParams格式: [ConditionType, param1, param2, ...]
                // 例如: [1, 0.3, 0.7] 表示生命值百分比在30%-70%之间
                int conditionTypeInt = (int)config.ConditionParams[0];
                ETriggerType conditionType = (ETriggerType)conditionTypeInt;
                
                // 提取条件参数（排除第一个ConditionType参数）
                List<float> conditionParams = new List<float>();
                for (int i = 1; i < config.ConditionParams.Count; i++)
                {
                    conditionParams.Add(config.ConditionParams[i]);
                }
                
                buff.SetCondition(conditionType, conditionParams);
            }
            
            // 创建目标选择器（如果有目标参数）
            if (config.TargetParams != null && config.TargetParams.Count > 0)
            {
                BaseTargetSelector targetSelector = CreateTargetSelector(
                    config.TargetType,
                    targetActor ?? buff.TargetActor,
                    config.TargetParams,
                    null, // attackerNumeric在OnStart时设置
                    null, // attackerActor在OnStart时设置
                    config.StatusId
                );
                
                if (targetSelector != null)
                {
                    buff.SetTargetSelector(targetSelector);
                }
            }
            
            return buff;
        }
        
        /// <summary>
        /// 根据BuffType创建对应的Effect实例
        /// </summary>
        /// <param name="buffType">Buff类型</param>
        /// <param name="targetActor">目标Actor（可为null，后续在OnStart时设置）</param>
        /// <param name="valueParams">数值参数列表</param>
        /// <param name="attackerNumeric">攻击者的数值组件（可为null）</param>
        /// <param name="statusId">状态ID</param>
        /// <param name="damageType">伤害类型</param>
        /// <returns>创建的Effect实例，如果找不到对应的类型则返回null</returns>
        internal static BaseEffect CreateEffect(
            EBuffType buffType,
            GameActor targetActor = null,
            List<float> valueParams = null,
            NumericComponent attackerNumeric = null,
            int statusId = 0,
            GameConfig.EDamageType damageType = GameConfig.EDamageType.Physical)
        {
            // 确保缓存已初始化
            InitializeEffectTypeCache();
            
            // 从缓存中获取对应的Effect类型
            if (!s_effectTypeCache.TryGetValue(buffType, out Type effectType))
            {
                Log.Error($"BuffFactory: 未找到BuffType {buffType} 对应的Effect类型");
                return null;
            }
            
            // 使用反射创建Effect实例
            try
            {
                // 尝试获取攻击者Actor（从attackerNumeric）
                GameActor attackerActor = null;
                if (attackerNumeric != null)
                {
                    attackerActor = attackerNumeric.Actor;
                }
                
                // 优先尝试新的构造函数（包含attackerActor和damageType）
                ConstructorInfo constructor = effectType.GetConstructor(new Type[]
                {
                    typeof(GameActor),
                    typeof(List<float>),
                    typeof(NumericComponent),
                    typeof(GameActor),
                    typeof(int),
                    typeof(GameConfig.EDamageType)
                });
                
                if (constructor != null)
                {
                    // 使用新构造函数
                    BaseEffect effect = (BaseEffect)constructor.Invoke(new object[]
                    {
                        targetActor,
                        valueParams ?? new List<float>(),
                        attackerNumeric,
                        attackerActor,
                        statusId,
                        damageType
                    });
                    return effect;
                }
                
                // 回退到包含attackerActor但不包含damageType的构造函数
                constructor = effectType.GetConstructor(new Type[]
                {
                    typeof(GameActor),
                    typeof(List<float>),
                    typeof(NumericComponent),
                    typeof(GameActor),
                    typeof(int)
                });
                
                if (constructor != null)
                {
                    // 使用旧构造函数（不包含damageType）
                    BaseEffect effect = (BaseEffect)constructor.Invoke(new object[]
                    {
                        targetActor,
                        valueParams ?? new List<float>(),
                        attackerNumeric,
                        attackerActor,
                        statusId
                    });
                    // 手动设置DamageType
                    var damageTypeField = effectType.GetField("DamageType", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (damageTypeField != null)
                    {
                        damageTypeField.SetValue(effect, damageType);
                    }
                    return effect;
                }
                
                // 回退到旧构造函数（不包含attackerActor和damageType）
                constructor = effectType.GetConstructor(new Type[]
                {
                    typeof(GameActor),
                    typeof(List<float>),
                    typeof(NumericComponent),
                    typeof(int)
                });
                
                if (constructor != null)
                {
                    // 使用旧构造函数创建实例
                    BaseEffect effectOld = (BaseEffect)constructor.Invoke(new object[]
                    {
                        targetActor,
                        valueParams ?? new List<float>(),
                        attackerNumeric,
                        statusId
                    });
                    // 手动设置DamageType
                    var damageTypeField = effectType.GetField("DamageType", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (damageTypeField != null)
                    {
                        damageTypeField.SetValue(effectOld, damageType);
                    }
                    return effectOld;
                }
                
                Log.Error($"BuffFactory: Effect类型 {effectType.Name} 没有找到匹配的构造函数");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"BuffFactory: 创建Effect实例失败，类型 {effectType.Name}，错误: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 根据BuffID创建Buff实例
        /// </summary>
        public static BaseBuff CreateBuff(int buffId, GameActor targetActor = null)
        {
            var config = ConfigSystem.Instance.Tables.TbBuff.GetOrDefault(buffId);
            if (config == null)
            {
                Log.Error($"BuffFactory: BuffConfig not found for ID: {buffId}");
                return null;
            }
            
            return CreateBuff(config, targetActor);
        }
        
        /// <summary>
        /// 创建Buff并添加到目标的BuffCmp（处理完整的生命周期）
        /// </summary>
        /// <param name="buffConfig">Buff配置</param>
        /// <param name="targetActor">目标Actor</param>
        /// <param name="attackerNumeric">攻击者的数值组件</param>
        /// <param name="attackerActor">攻击者的Actor</param>
        /// <returns>是否成功添加</returns>
        public static bool CreateAndAddBuff(BuffConfig buffConfig, GameActor targetActor, NumericComponent attackerNumeric = null, GameActor attackerActor = null)
        {
            if (buffConfig == null || targetActor == null || targetActor.IsDestroyed)
            {
                Log.Warning("BuffFactory.CreateAndAddBuff: 参数无效");
                return false;
            }
            
            // 获取目标的Buff组件
            var buffCmp = targetActor.GetComponent<BuffCmp>();
            if (buffCmp == null)
            {
                Log.Warning($"BuffFactory.CreateAndAddBuff: 目标 {targetActor.Tag} 没有BuffCmp组件");
                return false;
            }
            
            // 创建Buff
            var buff = CreateBuff(buffConfig, targetActor);
            if (buff == null)
            {
                return false;
            }
            
            // 设置攻击者信息
            if (attackerNumeric != null)
            {
                buff.SetAttackerNumeric(attackerNumeric);
            }
            
            // 设置目标并启动Buff（OnStart会在AddBuff中调用，但这里先设置好参数）
            buff.OnStart(targetActor, attackerNumeric, attackerActor);
            
            // 添加到BuffCmp（AddBuff会处理生命周期）
            buffCmp.AddBuff(buff);
            
            return true;
        }
        
        /// <summary>
        /// 创建AttributeModifier（用于属性修改类Buff）
        /// </summary>
        private static AttributeModifier CreateAttributeModifier(List<float> valueParams)
        {
            if (valueParams == null || valueParams.Count < 2)
            {
                Log.Warning($"BuffFactory: PropertyMod effect needs at least 2 value params: [NumericType, Value, ModifierType(optional)]");
                return null;
            }
            
            // ValueParams格式: [NumericType, Value, ModifierType(可选，默认PercentAdd)]
            // 例如: [1004, 0.2, 1] 表示攻击力+20%，使用PercentAdd
            int numericTypeInt = (int)valueParams[0];
            float value = valueParams[1];
            ModifierType modType = ModifierType.PercentAdd; // 默认百分比加法
            
            // 如果有第三个参数，使用它作为ModifierType
            if (valueParams.Count >= 3)
            {
                int modTypeInt = (int)valueParams[2];
                modType = (ModifierType)modTypeInt;
            }
                
            NumericType numericType = (NumericType)numericTypeInt;
            
            return new AttributeModifier(modType, numericType, value, null);
        }
        
        /// <summary>
        /// 根据ConditionType创建对应的Condition实例
        /// </summary>
        /// <param name="conditionType">条件类型</param>
        /// <param name="targetActor">目标Actor</param>
        /// <param name="conditionParams">条件参数列表</param>
        /// <param name="attackerNumeric">攻击者的数值组件（可为null）</param>
        /// <param name="attackerActor">攻击者的Actor（可为null）</param>
        /// <param name="statusId">状态ID</param>
        /// <returns>创建的Condition实例，如果找不到对应的类型则返回null</returns>
        internal static BaseCondition CreateCondition(
            ETriggerType conditionType,
            GameActor targetActor,
            List<float> conditionParams = null,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0)
        {
            // 确保缓存已初始化
            InitializETriggerTypeCache();
            
            // 从缓存中获取对应的Condition类型
            if (!s_conditionTypeCache.TryGetValue(conditionType, out Type conditionTypeClass))
            {
                Log.Error($"BuffFactory: 未找到ConditionType {conditionType} 对应的Condition类型");
                return null;
            }
            
            // 使用反射创建Condition实例
            try
            {
                ConstructorInfo constructor = conditionTypeClass.GetConstructor(new Type[]
                {
                    typeof(GameActor),
                    typeof(List<float>),
                    typeof(NumericComponent),
                    typeof(GameActor),
                    typeof(int)
                });
                
                if (constructor == null)
                {
                    Log.Error($"BuffFactory: Condition类型 {conditionTypeClass.Name} 没有找到匹配的构造函数");
                    return null;
                }
                
                BaseCondition condition = (BaseCondition)constructor.Invoke(new object[]
                {
                    targetActor,
                    conditionParams ?? new List<float>(),
                    attackerNumeric,
                    attackerActor,
                    statusId
                });
                
                return condition;
            }
            catch (Exception ex)
            {
                Log.Error($"BuffFactory: 创建Condition实例失败，类型 {conditionTypeClass.Name}，错误: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 根据TargetType创建对应的TargetSelector实例
        /// </summary>
        /// <param name="targetType">目标类型</param>
        /// <param name="sourceActor">源Actor（Buff的拥有者）</param>
        /// <param name="targetParams">目标参数列表</param>
        /// <param name="attackerNumeric">攻击者的数值组件（可为null）</param>
        /// <param name="attackerActor">攻击者的Actor（可为null）</param>
        /// <param name="statusId">状态ID</param>
        /// <returns>创建的TargetSelector实例，如果找不到对应的类型则返回null</returns>
        internal static BaseTargetSelector CreateTargetSelector(
            ETargetType targetType,
            GameActor sourceActor,
            List<float> targetParams = null,
            NumericComponent attackerNumeric = null,
            GameActor attackerActor = null,
            int statusId = 0)
        {
            // 确保缓存已初始化
            InitializeTargetSelectorTypeCache();
            
            // 从缓存中获取对应的TargetSelector类型
            if (!s_targetSelectorTypeCache.TryGetValue(targetType, out Type targetSelectorType))
            {
                Log.Error($"BuffFactory: 未找到TargetType {targetType} 对应的TargetSelector类型");
                return null;
            }
            
            // 使用反射创建TargetSelector实例
            try
            {
                ConstructorInfo constructor = targetSelectorType.GetConstructor(new Type[]
                {
                    typeof(GameActor),
                    typeof(List<float>),
                    typeof(NumericComponent),
                    typeof(GameActor),
                    typeof(int)
                });
                
                if (constructor == null)
                {
                    Log.Error($"BuffFactory: TargetSelector类型 {targetSelectorType.Name} 没有找到匹配的构造函数");
                    return null;
                }
                
                BaseTargetSelector targetSelector = (BaseTargetSelector)constructor.Invoke(new object[]
                {
                    sourceActor,
                    targetParams ?? new List<float>(),
                    attackerNumeric,
                    attackerActor,
                    statusId
                });
                
                return targetSelector;
            }
            catch (Exception ex)
            {
                Log.Error($"BuffFactory: 创建TargetSelector实例失败，类型 {targetSelectorType.Name}，错误: {ex.Message}");
                return null;
            }
        }
    }
}
