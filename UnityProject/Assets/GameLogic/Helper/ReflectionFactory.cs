using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 通用反射工厂工具类，用于通过反射扫描基类并动态创建实例
    /// </summary>
    /// <typeparam name="TBaseType">基类类型</typeparam>
    /// <typeparam name="TKey">键类型（如枚举）</typeparam>
    /// <typeparam name="TAttribute">特性类型，必须包含一个返回TKey的属性</typeparam>
    public static class ReflectionFactory<TBaseType, TKey, TAttribute>
        where TBaseType : class
        where TKey : struct, IConvertible
        where TAttribute : Attribute
    {
        /// <summary>
        /// 类型缓存字典
        /// </summary>
        private static Dictionary<TKey, Type> s_typeCache = null;

        /// <summary>
        /// 特性属性获取器（缓存，避免重复反射）
        /// </summary>
        private static Func<TAttribute, TKey> s_attributeKeyGetter = null;

        /// <summary>
        /// 工厂名称（用于日志）
        /// </summary>
        private static string s_factoryName = typeof(ReflectionFactory<TBaseType, TKey, TAttribute>).Name;

        /// <summary>
        /// 初始化类型缓存
        /// </summary>
        /// <param name="attributeKeyGetter">从特性中获取键的函数（如果特性有多个属性，需要指定获取哪个）</param>
        private static void InitializeTypeCache(Func<TAttribute, TKey> attributeKeyGetter = null)
        {
            if (s_typeCache != null)
                return;

            s_typeCache = new Dictionary<TKey, Type>();

            // 如果没有提供获取器，尝试自动查找
            if (attributeKeyGetter == null)
            {
                attributeKeyGetter = CreateDefaultAttributeKeyGetter();
            }

            s_attributeKeyGetter = attributeKeyGetter;

            // 获取当前程序集中所有继承自TBaseType的类
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type baseType = typeof(TBaseType);

            var types = assembly.GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    baseType.IsAssignableFrom(t) &&
                    t.GetCustomAttribute<TAttribute>() != null
                );

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<TAttribute>();
                if (attribute != null && s_attributeKeyGetter != null)
                {
                    TKey key = s_attributeKeyGetter(attribute);
                    if (s_typeCache.ContainsKey(key))
                    {
                        Log.Warning($"{s_factoryName}: 发现重复的键 {key}，类型 {type.Name} 将被忽略");
                        continue;
                    }
                    s_typeCache[key] = type;
                    Log.Info($"{s_factoryName}: 注册类型 {type.Name} -> {key}");
                }
            }

            if (s_typeCache.Count == 0)
            {
                Log.Warning($"{s_factoryName}: 未找到任何带{typeof(TAttribute).Name}的类");
            }
        }

        /// <summary>
        /// 创建默认的特性键获取器（自动查找第一个返回TKey类型的属性）
        /// </summary>
        private static Func<TAttribute, TKey> CreateDefaultAttributeKeyGetter()
        {
            Type attributeType = typeof(TAttribute);
            PropertyInfo[] properties = attributeType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 查找第一个返回TKey类型的属性
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(TKey) && prop.CanRead)
                {
                    return (attr) => (TKey)prop.GetValue(attr);
                }
            }

            Log.Error($"{s_factoryName}: 无法自动找到特性 {attributeType.Name} 中返回 {typeof(TKey).Name} 的属性，请手动提供 attributeKeyGetter");
            return null;
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="constructorArgs">构造函数参数</param>
        /// <returns>创建的实例，如果找不到对应的类型则返回null</returns>
        public static TBaseType Create(TKey key, params object[] constructorArgs)
        {
            // 确保缓存已初始化
            InitializeTypeCache();

            // 从缓存中获取对应的类型
            if (!s_typeCache.TryGetValue(key, out Type targetType))
            {
                Log.Error($"{s_factoryName}: 未找到键 {key} 对应的类型");
                return null;
            }

            // 使用反射创建实例
            try
            {
                if (constructorArgs == null || constructorArgs.Length == 0)
                {
                    // 无参构造函数
                    TBaseType instance = Activator.CreateInstance(targetType) as TBaseType;
                    return instance;
                }
                else
                {
                    // 有参构造函数
                    Type[] paramTypes = constructorArgs.Select(arg => arg?.GetType() ?? typeof(object)).ToArray();
                    ConstructorInfo constructor = targetType.GetConstructor(paramTypes);

                    if (constructor == null)
                    {
                        // 尝试查找匹配的构造函数（考虑继承和接口）
                        constructor = FindMatchingConstructor(targetType, paramTypes);
                    }

                    if (constructor == null)
                    {
                        Log.Error($"{s_factoryName}: 类型 {targetType.Name} 没有找到匹配的构造函数，参数类型: [{string.Join(", ", paramTypes.Select(t => t.Name))}]");
                        return null;
                    }

                    TBaseType instance = constructor.Invoke(constructorArgs) as TBaseType;
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{s_factoryName}: 创建实例失败，键 {key}，类型 {targetType.Name}，错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 查找匹配的构造函数（考虑参数类型的继承关系）
        /// </summary>
        private static ConstructorInfo FindMatchingConstructor(Type targetType, Type[] paramTypes)
        {
            ConstructorInfo[] constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            foreach (var constructor in constructors)
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                if (parameters.Length != paramTypes.Length)
                    continue;

                bool isMatch = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type paramType = parameters[i].ParameterType;
                    Type argType = paramTypes[i];

                    // 检查类型是否匹配（包括继承关系）
                    if (!paramType.IsAssignableFrom(argType))
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    return constructor;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取所有已注册的键
        /// </summary>
        public static IEnumerable<TKey> GetAllKeys()
        {
            InitializeTypeCache();
            return s_typeCache.Keys;
        }

        /// <summary>
        /// 获取所有已注册的类型
        /// </summary>
        public static IEnumerable<Type> GetAllTypes()
        {
            InitializeTypeCache();
            return s_typeCache.Values;
        }

        /// <summary>
        /// 检查是否已注册指定的键
        /// </summary>
        public static bool IsRegistered(TKey key)
        {
            InitializeTypeCache();
            return s_typeCache.ContainsKey(key);
        }

        /// <summary>
        /// 清除缓存（用于测试或重新加载）
        /// </summary>
        public static void ClearCache()
        {
            s_typeCache = null;
            s_attributeKeyGetter = null;
        }
    }
}

