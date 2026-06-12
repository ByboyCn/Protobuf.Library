# AOT 友好的反射 API 使用指南

## 概述

Protobuf.Library 提供了一个完全 AOT 友好的反射 API，允许在 NativeAOT 环境下安全地访问消息类型元数据和字段信息，无需使用传统的反射。

## 特性

- ✅ **AOT 友好**：完全支持 NativeAOT 编译，无需反射
- ✅ **类型安全**：基于代码生成的类型安全访问
- ✅ **高性能**：编译时生成，零运行时开销
- ✅ **零依赖**：不依赖 System.Reflection

## 核心组件

### 1. 消息描述符 (MessageDescriptor)

```csharp
using Protobuf.Reflection;

// 获取消息描述符
var descriptor = MyMessage.Descriptor;

// 访问基本信息
Console.WriteLine($"Message Name: {descriptor.Name}");
Console.WriteLine($"Full Name: {descriptor.FullName}");

// 遍历字段
foreach (var field in descriptor.Fields)
{
    Console.WriteLine($"Field {field.FieldNumber}: {field.Name} ({field.FieldType})");
}

// 查找字段
var fieldByName = descriptor.FindFieldByName("my_field");
var fieldByNumber = descriptor.FindFieldByNumber(1);
```

### 2. 字段访问器 (IFieldAccessor)

```csharp
// 获取字段访问器
var accessors = MyMessage.GetFieldAccessors();

foreach (var accessor in accessors)
{
    var descriptor = accessor.Descriptor;
    
    // 检查字段是否有值
    bool hasValue = accessor.HasValue(message);
    
    // 获取字段值
    object? value = accessor.GetValue(message);
    
    // 设置字段值
    accessor.SetValue(message, newValue);
    
    // 清除字段
    accessor.Clear(message);
}
```

### 3. 类型安全的字段访问

```csharp
// 对于生成的消息，可以使用类型安全的访问器
var message = new MyMessage();
message.SetName("Alice");

// 获取字段访问器
var fieldAccessor = MyMessage._nameAccessor; // 生成的访问器

// 类型安全的方法
string? name = fieldAccessor.GetTypedValue(message);
fieldAccessor.SetTypedValue(message, "Bob");
```

## 生成的代码结构

当使用 Source Generator 生成消息类时，会自动生成以下反射支持：

### 1. 消息描述符

```csharp
public static MessageDescriptor Descriptor => _descriptor;

private static readonly MessageDescriptor _descriptor = CreateDescriptor();

private static MessageDescriptor CreateDescriptor()
{
    var fields = new ImmutableArray.Builder<FieldDescriptor>();
    fields.Add(new FieldDescriptor("name", 1, FieldType.String, 
                  "string", FieldLabel.Optional, false, false, null));
    // ... 更多字段
    
    return new MessageDescriptor(
        "MyNamespace.MyMessage",
        "MyMessage",
        fields.ToImmutable()
    );
}
```

### 2. 字段访问器

```csharp
private static readonly IFieldAccessor<MyMessage, string> _nameAccessor =
    FieldAccessorFactory.Create<MyMessage, string>(
        new FieldDescriptor(...),
        m => m.HasName,           // HasValue
        m => m.Name,              // GetValue
        (m, v) => m.SetName(v!),  // SetValue
        m => m.ClearName()        // Clear
    );
```

### 3. 访问器集合

```csharp
public static IEnumerable<IFieldAccessor> GetFieldAccessors()
{
    yield return _nameAccessor;
    yield return _idAccessor;
    // ... 更多访问器
}
```

## 使用场景

### 1. 动态字段访问

```csharp
// 根据字段名动态设置值
var message = new MyMessage();
var descriptor = MyMessage.Descriptor;

var field = descriptor.FindFieldByName("user_name");
if (field != null)
{
    var accessor = MyMessage.GetFieldAccessors()
        .First(a => a.Descriptor.Name == "user_name");
    
    accessor.SetValue(message, "Alice");
}
```

### 2. 序列化和验证

```csharp
// 验证所有必需字段
public bool ValidateMessage(IMessage message)
{
    var descriptor = message.GetType()
        .GetProperty("Descriptor")?
        .GetValue(null) as MessageDescriptor;
    
    if (descriptor == null) return true;
    
    foreach (var field in descriptor.Fields)
    {
        if (field.Label == FieldLabel.Required)
        {
            // 检查必需字段
        }
    }
    return true;
}
```

### 3. 元数据查询

```csharp
// 获取消息结构信息
var descriptor = MyMessage.Descriptor;

Console.WriteLine($"Message: {descriptor.Name}");
Console.WriteLine($"Fields: {descriptor.Fields.Length}");

foreach (var field in descriptor.Fields)
{
    var label = field.Label switch
    {
        FieldLabel.Required => "required",
        FieldLabel.Optional => "optional",
        FieldLabel.Repeated => "repeated",
        _ => "unknown"
    };
    
    Console.WriteLine($"  {field.Name}: {field.FieldType} [{label}] = {field.FieldNumber}");
}
```

## AOT 兼容性

### 为什么传统反射不适用于 AOT？

传统反射在 NativeAOT 编译时会遇到问题：
- 运行时类型信息可能被裁剪
- 反射 API 需要动态元数据
- 性能开销大，无法优化

### 我们的解决方案

1. **代码生成**：在编译时生成所有必要的元数据
2. **零反射**：完全避免使用 `System.Reflection`
3. **静态分发**：使用编译时确定的类型和方法
4. **Immutable 数据**：使用不可变数据结构，避免运行时修改

## 示例：完整的 AOT 友好使用

```csharp
using Protobuf.Reflection;

public class MessageProcessor
{
    // AOT 友好的消息处理
    public void ProcessMessage(IMessage message)
    {
        // 1. 获取描述符（无反射）
        var descriptor = GetMessageDescriptor(message);
        if (descriptor == null) return;
        
        // 2. 遍历字段
        foreach (var field in descriptor.Fields)
        {
            if (ShouldProcessField(field))
            {
                ProcessField(message, field);
            }
        }
    }
    
    // AOT 友好的字段处理
    private void ProcessField(IMessage message, FieldDescriptor field)
    {
        // 获取访问器（无反射）
        var accessor = GetFieldAccessor(message, field);
        if (accessor == null) return;
        
        // 获取和设置值
        var value = accessor.GetValue(message);
        var processedValue = TransformValue(value);
        accessor.SetValue(message, processedValue);
    }
    
    // 使用生成的静态方法
    private static MessageDescriptor? GetMessageDescriptor(IMessage message)
    {
        // 生成代码中会包含静态的 Descriptor 属性
        return message switch
        {
            MyMessage msg => MyMessage.Descriptor,
            OtherMessage msg => OtherMessage.Descriptor,
            _ => null
        };
    }
    
    private static IFieldAccessor? GetFieldAccessor(IMessage message, FieldDescriptor field)
    {
        return message switch
        {
            MyMessage msg => MyMessage.GetFieldAccessors()
                .First(a => a.Descriptor.FieldNumber == field.FieldNumber),
            OtherMessage msg => OtherMessage.GetFieldAccessors()
                .First(a => a.Descriptor.FieldNumber == field.FieldNumber),
            _ => null
        };
    }
}
```

## 性能对比

| 方法 | AOT 兼容 | 性能 | 内存开销 |
|------|----------|------|----------|
| 传统反射 | ❌ | 慢 | 高 |
| 表达式树 | ❌ | 中 | 中 |
| **我们的方案** | ✅ | **快** | **低** |

## 限制和注意事项

1. **仅支持生成的消息**：反射 API 仅适用于通过 Source Generator 生成的消息类
2. **静态元数据**：描述符在编译时生成，运行时不可修改
3. **类型转换**：使用 `GetValue()` 时需要进行类型转换

## 最佳实践

1. **优先使用生成的访问器**：直接使用生成的静态访问器而非查找
2. **缓存描述符**：描述符是静态的，可以安全缓存
3. **类型安全优先**：优先使用类型安全的方法而非泛型方法
4. **避免频繁查找**：如果需要多次访问同一字段，缓存访问器

## 总结

Protobuf.Library 的 AOT 友好反射 API 提供了：
- 🚀 **高性能**：编译时生成，零运行时开销
- 🔒 **类型安全**：基于代码生成的类型安全访问
- 📦 **AOT 兼容**：完全支持 NativeAOT 编译
- 💪 **零依赖**：不依赖传统的反射 API

这使得 Protobuf.Library 成为了真正现代、高性能的 Protobuf 解决方案！
