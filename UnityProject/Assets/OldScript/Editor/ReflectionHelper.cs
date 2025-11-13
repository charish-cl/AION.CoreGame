using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 通用反射工具类 - 支持动态调用方法、获取成员、创建实例等操作
/// </summary>
public static class ReflectionHelper
{

    public static Type GetTypeFromAssembly(string assemblyName, string typeName)
    {
        Assembly assembly = LoadAssembly(assemblyName);
        if (assembly == null)
        {
            Debug.LogError(assemblyName + " could not be loaded");
            return null;
        }
        return assembly.GetType(typeName);
    }
    #region 核心方法 - 动态调用方法
    
    /// <summary>
    /// 动态调用方法（最通用版本）
    /// </summary>
    /// <param name="assemblyName">程序集名称（不含.dll）</param>
    /// <param name="typeName">完整类型名（含命名空间）</param>
    /// <param name="methodName">方法名</param>
    /// <param name="parameters">参数值数组</param>
    /// <param name="isStatic">是否静态方法</param>
    /// <returns>方法返回值</returns>
    public static object InvokeMethod(string assemblyName, string typeName, string methodName, 
        object[] parameters = null, bool isStatic = false, object instance = null)
    {
        try
        {
            // 1. 加载程序集
            Assembly assembly = LoadAssembly(assemblyName);
            if (assembly == null)
                throw new Exception($"程序集 {assemblyName} 加载失败");
            
            // 2. 获取类型
            Type targetType = GetTypeFromAssembly(assembly, typeName);
            if (targetType == null)
                throw new Exception($"类型 {typeName} 在程序集 {assemblyName} 中未找到");
            
            // 3. 准备参数类型数组（用于方法重载识别）
            Type[] paramTypes = parameters != null ? new Type[parameters.Length] : Type.EmptyTypes;
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    paramTypes[i] = parameters[i]?.GetType() ?? typeof(object);
                }
            }
            
            // 4. 获取方法信息
            MethodInfo method = targetType.GetMethod(methodName, 
                BindingFlags.Public | BindingFlags.NonPublic | 
                (isStatic ? BindingFlags.Static : BindingFlags.Instance), 
                null, paramTypes, null);
                
            if (method == null)
                throw new Exception($"方法 {methodName} 在类型 {typeName} 中未找到");
            
            // 5. 处理实例（静态方法不需要实例）
            object methodInstance = isStatic ? null : instance;
            if (!isStatic && methodInstance == null)
            {
                methodInstance = CreateInstance(assemblyName, typeName);
            }
            
            // 6. 调用方法
            return method.Invoke(methodInstance, parameters);
        }
        catch (Exception ex)
        {
            Debug.LogError($"反射调用方法失败: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 简化版方法调用 - 自动创建实例
    /// </summary>
    public static object InvokeInstanceMethod(string assemblyName, string typeName, string methodName, 
        params object[] parameters)
    {
        return InvokeMethod(assemblyName, typeName, methodName, parameters, false, null);
    }
    
    /// <summary>
    /// 简化版静态方法调用
    /// </summary>
    public static object InvokeStaticMethod(string assemblyName, string typeName, string methodName, 
        params object[] parameters)
    {
        return InvokeMethod(assemblyName, typeName, methodName, parameters, true, null);
    }
    
    /// <summary>
    /// 泛型版本方法调用 - 指定返回类型
    /// </summary>
    public static T InvokeMethod<T>(string assemblyName, string typeName, string methodName, 
        object[] parameters = null, bool isStatic = false, object instance = null)
    {
        object result = InvokeMethod(assemblyName, typeName, methodName, parameters, isStatic, instance);
        return result is T typedResult ? typedResult : default(T);
    }
    
    #endregion
    
    #region 泛型方法支持
    
    /// <summary>
    /// 调用泛型方法
    /// </summary>
    public static object InvokeGenericMethod(string assemblyName, string typeName, string methodName, 
        Type[] genericTypes, object[] parameters = null, object instance = null)
    {
        try
        {
            Assembly assembly = LoadAssembly(assemblyName);
            Type targetType = GetTypeFromAssembly(assembly, typeName);
            
            // 获取非泛型方法定义
            MethodInfo genericMethodDefinition = targetType.GetMethod(methodName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            
            if (genericMethodDefinition == null)
                throw new Exception($"泛型方法 {methodName} 未找到");
            
            // 创建具体泛型方法
            MethodInfo concreteMethod = genericMethodDefinition.MakeGenericMethod(genericTypes);
            
            object methodInstance = instance ?? CreateInstance(assemblyName, typeName);
            return concreteMethod.Invoke(methodInstance, parameters);
        }
        catch (Exception ex)
        {
            Debug.LogError($"调用泛型方法失败: {ex.Message}");
            return null;
        }
    }
    
    #endregion
    
    #region 成员访问相关
    
    /// <summary>
    /// 获取字段值
    /// </summary>
    public static object GetFieldValue(string assemblyName, string typeName, string fieldName, 
        object instance = null)
    {
        try
        {
            Assembly assembly = LoadAssembly(assemblyName);
            Type targetType = GetTypeFromAssembly(assembly, typeName);
            
            FieldInfo field = targetType.GetField(fieldName, 
                BindingFlags.Public | BindingFlags.NonPublic | 
                (instance == null ? BindingFlags.Static : BindingFlags.Instance));
            
            if (field == null)
                throw new Exception($"字段 {fieldName} 未找到");
            
            object fieldInstance = instance ?? CreateInstance(assemblyName, typeName);
            return field.GetValue(fieldInstance);
        }
        catch (Exception ex)
        {
            Debug.LogError($"获取字段值失败: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 设置字段值
    /// </summary>
    public static void SetFieldValue(string assemblyName, string typeName, string fieldName, 
        object value, object instance = null)
    {
        try
        {
            Assembly assembly = LoadAssembly(assemblyName);
            Type targetType = GetTypeFromAssembly(assembly, typeName);
            
            FieldInfo field = targetType.GetField(fieldName, 
                BindingFlags.Public | BindingFlags.NonPublic | 
                (instance == null ? BindingFlags.Static : BindingFlags.Instance));
            
            if (field == null)
                throw new Exception($"字段 {fieldName} 未找到");
            
            object fieldInstance = instance ?? CreateInstance(assemblyName, typeName);
            field.SetValue(fieldInstance, value);
        }
        catch (Exception ex)
        {
            Debug.LogError($"设置字段值失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 获取属性值
    /// </summary>
    public static object GetPropertyValue(string assemblyName, string typeName, string propertyName, 
        object instance = null)
    {
        try
        {
            Assembly assembly = LoadAssembly(assemblyName);
            Type targetType = GetTypeFromAssembly(assembly, typeName);
            
            PropertyInfo property = targetType.GetProperty(propertyName, 
                BindingFlags.Public | BindingFlags.NonPublic | 
                (instance == null ? BindingFlags.Static : BindingFlags.Instance));
            
            if (property == null)
                throw new Exception($"属性 {propertyName} 未找到");
            
            object propertyInstance = instance ?? CreateInstance(assemblyName, typeName);
            return property.GetValue(propertyInstance);
        }
        catch (Exception ex)
        {
            Debug.LogError($"获取属性值失败: {ex.Message}");
            return null;
        }
    }
    
    #endregion
    
    #region 实例创建相关
    
    /// <summary>
    /// 创建类型实例
    /// </summary>
    public static object CreateInstance(string assemblyName, string typeName, params object[] constructorArgs)
    {
        try
        {
            Assembly assembly = LoadAssembly(assemblyName);
            Type targetType = GetTypeFromAssembly(assembly, typeName);
            
            if (constructorArgs == null || constructorArgs.Length == 0)
            {
                return Activator.CreateInstance(targetType);
            }
            else
            {
                // 获取匹配的构造函数
                Type[] paramTypes = new Type[constructorArgs.Length];
                for (int i = 0; i < constructorArgs.Length; i++)
                {
                    paramTypes[i] = constructorArgs[i]?.GetType() ?? typeof(object);
                }
                
                ConstructorInfo constructor = targetType.GetConstructor(paramTypes);
                if (constructor == null)
                    throw new Exception($"未找到匹配的构造函数");
                
                return constructor.Invoke(constructorArgs);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"创建实例失败: {ex.Message}");
            return null;
        }
    }
    
    #endregion
    
    #region 底层辅助方法
    
    /// <summary>
    /// 加载程序集
    /// </summary>
    private static Assembly LoadAssembly(string assemblyName)
    {
        try
        {
            // 尝试从已加载的程序集中查找
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in loadedAssemblies)
            {
                if (assembly.GetName().Name == assemblyName)
                    return assembly;
            }
            
            // 动态加载程序集
            return Assembly.Load(assemblyName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"加载程序集 {assemblyName} 失败: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 从程序集中获取类型
    /// </summary>
    private static Type GetTypeFromAssembly(Assembly assembly, string typeName)
    {
        if (assembly == null) return null;
        
        Type type = assembly.GetType(typeName);
        if (type == null)
        {
            // 尝试搜索所有类型（包括嵌套类型）
            Type[] allTypes = assembly.GetTypes();
            foreach (Type t in allTypes)
            {
                if (t.FullName == typeName || t.Name == typeName)
                    return t;
            }
        }
        
        return type;
    }
    
    /// <summary>
    /// 获取类型的所有方法名（用于调试）
    /// </summary>
    public static List<string> GetAllMethodNames(string assemblyName, string typeName)
    {
        var methodNames = new List<string>();
        try
        {
            Assembly assembly = LoadAssembly(assemblyName);
            Type targetType = GetTypeFromAssembly(assembly, typeName);
            
            MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                methodNames.Add(method.Name);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"获取方法列表失败: {ex.Message}");
        }
        
        return methodNames;
    }
    
    #endregion
}