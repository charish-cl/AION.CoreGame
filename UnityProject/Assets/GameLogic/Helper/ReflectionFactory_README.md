# ReflectionFactory 通用反射工厂

## 简介

`ReflectionFactory` 是一个通用的反射工厂工具类，用于通过反射扫描基类并动态创建实例，减少重复代码。

## 使用场景

当你需要：
1. 通过反射扫描所有继承自某个基类的类
2. 使用 Attribute 标记类与键值（如枚举）的映射关系
3. 动态创建实例

## 基本用法

### 1. 定义基类和特性

```csharp
// 基类
public abstract class BaseCurrencyData
{
    public CurrencyType CurrencyType { get; protected set; }
    protected BaseCurrencyData(CurrencyType currencyType) { CurrencyType = currencyType; }
}

// 特性（必须包含一个返回键类型的属性）
[AttributeUsage(AttributeTargets.Class)]
public class CurrencyAttribute : Attribute
{
    public CurrencyType CurrencyType { get; private set; }
    public CurrencyAttribute(CurrencyType currencyType) { CurrencyType = currencyType; }
}
```

### 2. 创建具体实现类

```csharp
[Currency(CurrencyType.Coin)]
public class CoinCurrency : BaseCurrencyData
{
    public CoinCurrency(CurrencyType currencyType) : base(currencyType) { }
}
```

### 3. 使用通用工厂

```csharp
// 创建实例（会自动查找匹配的构造函数）
BaseCurrencyData currency = ReflectionFactory<BaseCurrencyData, CurrencyType, CurrencyAttribute>
    .Create(CurrencyType.Coin, CurrencyType.Coin);

// 获取所有已注册的键
IEnumerable<CurrencyType> allTypes = ReflectionFactory<BaseCurrencyData, CurrencyType, CurrencyAttribute>
    .GetAllKeys();

// 检查是否已注册
bool isRegistered = ReflectionFactory<BaseCurrencyData, CurrencyType, CurrencyAttribute>
    .IsRegistered(CurrencyType.Coin);
```

### 4. 封装成专用工厂（推荐）

```csharp
public static class CurrencyFactory
{
    public static BaseCurrencyData CreateCurrency(CurrencyType currencyType)
    {
        return ReflectionFactory<BaseCurrencyData, CurrencyType, CurrencyAttribute>
            .Create(currencyType, currencyType);
    }

    public static IEnumerable<CurrencyType> GetAllCurrencyTypes()
    {
        return ReflectionFactory<BaseCurrencyData, CurrencyType, CurrencyAttribute>
            .GetAllKeys();
    }
}
```

## 特性要求

特性类必须包含一个返回 `TKey` 类型的属性，工厂会自动查找第一个匹配的属性。

如果特性有多个返回 `TKey` 类型的属性，或者需要指定特定的属性，可以手动提供 `attributeKeyGetter`（虽然当前版本暂不支持，但可以通过修改特性设计来避免）。

## 构造函数匹配

工厂会自动匹配构造函数：
1. 首先尝试精确匹配参数类型
2. 如果失败，会尝试匹配可赋值的类型（考虑继承关系）

## 注意事项

1. **键类型约束**：`TKey` 必须是值类型（struct）并实现 `IConvertible`
2. **特性属性**：特性必须包含一个返回 `TKey` 类型的可读属性
3. **构造函数**：确保具体类有匹配的构造函数
4. **命名空间**：工厂只扫描当前程序集（`Assembly.GetExecutingAssembly()`）

## 示例：完整实现

参考 `CurrencyFactory` 的实现：

```csharp
// CurrencyFactory.cs
public static class CurrencyFactory
{
    public static BaseCurrencyData CreateCurrency(CurrencyType currencyType)
    {
        return ReflectionFactory<BaseCurrencyData, CurrencyType, CurrencyAttribute>
            .Create(currencyType, currencyType);
    }
}
```

## 优势

1. **减少重复代码**：不需要为每个工厂类重复编写反射扫描逻辑
2. **类型安全**：使用泛型确保类型安全
3. **自动缓存**：类型映射会自动缓存，提高性能
4. **灵活扩展**：易于添加新的工厂类

