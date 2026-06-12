using System.Collections.Immutable;
using Protobuf.Core;

namespace Protobuf.Reflection;

/// <summary>
/// 字段访问器（AOT友好）
/// </summary>
public interface IFieldAccessor
{
    /// <summary>
    /// 字段描述符
    /// </summary>
    FieldDescriptor Descriptor { get; }

    /// <summary>
    /// 检查字段是否有值
    /// </summary>
    bool HasValue(IMessage message);

    /// <summary>
    /// 获取字段值
    /// </summary>
    object? GetValue(IMessage message);

    /// <summary>
    /// 设置字段值
    /// </summary>
    void SetValue(IMessage message, object? value);

    /// <summary>
    /// 清除字段值
    /// </summary>
    void Clear(IMessage message);
}

/// <summary>
/// 类型化字段访问器（AOT友好）
/// </summary>
public interface IFieldAccessor<TMessage, TValue> : IFieldAccessor
    where TMessage : IMessage<TMessage>
{
    /// <summary>
    /// 获取字段值（类型安全）
    /// </summary>
    TValue? GetTypedValue(TMessage message);

    /// <summary>
    /// 设置字段值（类型安全）
    /// </summary>
    void SetTypedValue(TMessage message, TValue? value);
}

/// <summary>
/// 字段访问器工厂（AOT友好）
/// </summary>
public static class FieldAccessorFactory
{
    /// <summary>
    /// 创建字段访问器（用于生成的代码）
    /// </summary>
    public static IFieldAccessor Create<TMessage, TValue>(
        FieldDescriptor descriptor,
        Func<TMessage, bool> hasValue,
        Func<TMessage, TValue?> getter,
        Action<TMessage, TValue?> setter,
        Action<TMessage> clearer)
        where TMessage : IMessage<TMessage>
    {
        return new FieldAccessorImpl<TMessage, TValue>(descriptor, hasValue, getter, setter, clearer);
    }
}

/// <summary>
/// 字段访问器实现（内部使用）
/// </summary>
internal sealed class FieldAccessorImpl<TMessage, TValue> : IFieldAccessor<TMessage, TValue>
    where TMessage : IMessage<TMessage>
{
    private readonly Func<TMessage, bool> _hasValue;
    private readonly Func<TMessage, TValue?> _getter;
    private readonly Action<TMessage, TValue?> _setter;
    private readonly Action<TMessage> _clearer;

    public FieldDescriptor Descriptor { get; }

    public FieldAccessorImpl(
        FieldDescriptor descriptor,
        Func<TMessage, bool> hasValue,
        Func<TMessage, TValue?> getter,
        Action<TMessage, TValue?> setter,
        Action<TMessage> clearer)
    {
        Descriptor = descriptor;
        _hasValue = hasValue;
        _getter = getter;
        _setter = setter;
        _clearer = clearer;
    }

    public bool HasValue(IMessage message) => _hasValue((TMessage)message);

    public object? GetValue(IMessage message) => _getter((TMessage)message)!;

    public void SetValue(IMessage message, object? value) => _setter((TMessage)message, (TValue?)value);

    public void Clear(IMessage message) => _clearer((TMessage)message);

    public TValue? GetTypedValue(TMessage message) => _getter(message);

    public void SetTypedValue(TMessage message, TValue? value) => _setter(message, value);
}

/// <summary>
/// 反射服务（AOT友好）
/// </summary>
public sealed class ReflectionService
{
    private static readonly ImmutableDictionary<string, MessageDescriptor> _descriptors =
        ImmutableDictionary<string, MessageDescriptor>.Empty;

    /// <summary>
    /// 注册消息描述符（由生成代码调用）
    /// </summary>
    public static void RegisterDescriptor(MessageDescriptor descriptor)
    {
        // 在实际实现中，这里应该使用线程安全的注册机制
        // 对于AOT友好，可以在编译时生成注册代码
    }

    /// <summary>
    /// 查找消息描述符
    /// </summary>
    public static MessageDescriptor? FindDescriptor(string fullName)
    {
        // 在实际实现中，这里应该查找已注册的描述符
        return null;
    }

    /// <summary>
    /// 获取所有已注册的消息描述符
    /// </summary>
    public static IEnumerable<MessageDescriptor> GetAllDescriptors()
    {
        return Enumerable.Empty<MessageDescriptor>();
    }
}
