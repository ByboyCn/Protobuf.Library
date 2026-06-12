namespace Protobuf.Core;

/// <summary>
/// Protobuf 消息基接口（非泛型）
/// </summary>
public interface IMessage
{
    /// <summary>
    /// 计算消息的序列化大小（字节）
    /// </summary>
    /// <returns>消息的字节大小</returns>
    int CalculateSize();

    /// <summary>
    /// 检查消息是否已初始化（所有必需字段都有值）
    /// </summary>
    /// <returns>如果消息已初始化则为 true</returns>
    bool IsInitialized();

    /// <summary>
    /// 从编码的输入流反序列化消息
    /// </summary>
    /// <param name="input">编码的输入流</param>
    void MergeFrom(CodedInputStream input);

    /// <summary>
    /// 将消息序列化到编码的输出流
    /// </summary>
    /// <param name="output">编码的输出流</param>
    void WriteTo(CodedOutputStream output);
}

/// <summary>
/// Protobuf 消息接口
/// </summary>
/// <typeparam name="T">消息类型</typeparam>
public interface IMessage<T> : IMessage where T : IMessage<T>
{
    /// <summary>
    /// 从另一个消息合并到此消息
    /// </summary>
    /// <param name="message">源消息</param>
    void MergeFrom(T message);

    /// <summary>
    /// 创建消息的深拷贝
    /// </summary>
    /// <returns>消息的副本</returns>
    T Clone();
}

/// <summary>
/// 消息解析器
/// </summary>
/// <typeparam name="T">消息类型</typeparam>
public sealed class MessageParser<T> where T : IMessage<T>, new()
{
    private readonly Func<T> _factory;

    /// <summary>
    /// 初始化 MessageParser
    /// </summary>
    /// <param name="factory">创建消息实例的工厂函数</param>
    public MessageParser(Func<T> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// 从字节数组解析消息
    /// </summary>
    /// <param name="data">字节数组</param>
    /// <returns>解析的消息</returns>
    public T ParseFrom(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var message = _factory();
        using var stream = new MemoryStream(data);
        var input = new CodedInputStream(stream);
        message.MergeFrom(input);
        return message;
    }

    /// <summary>
    /// 从流解析消息
    /// </summary>
    /// <param name="stream">输入流</param>
    /// <returns>解析的消息</returns>
    public T ParseFrom(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        var message = _factory();
        var input = new CodedInputStream(stream);
        message.MergeFrom(input);
        return message;
    }

    /// <summary>
    /// 从字节数组解析消息（带偏移和长度）
    /// </summary>
    /// <param name="data">字节数组</param>
    /// <param name="offset">偏移量</param>
    /// <param name="length">长度</param>
    /// <returns>解析的消息</returns>
    public T ParseFrom(byte[] data, int offset, int length)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (offset < 0 || offset >= data.Length) throw new ArgumentOutOfRangeException(nameof(offset));
        if (length < 0 || offset + length > data.Length) throw new ArgumentOutOfRangeException(nameof(length));

        var message = _factory();
        using var stream = new MemoryStream(data, offset, length);
        var input = new CodedInputStream(stream);
        message.MergeFrom(input);
        return message;
    }

    /// <summary>
    /// 解析带有长度前缀的消息
    /// </summary>
    /// <param name="data">字节数组</param>
    /// <returns>解析的消息</returns>
    public T ParseFromLengthPrefix(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var message = _factory();
        using var stream = new MemoryStream(data);
        var input = new CodedInputStream(stream);

        // 读取长度前缀
        var length = input.ReadRawVarint32();

        // 读取消息
        message.MergeFrom(input);
        return message;
    }

    /// <summary>
    /// 创建新的空消息实例
    /// </summary>
    /// <returns>新消息实例</returns>
    public T CreateNew()
    {
        return _factory();
    }
}

/// <summary>
/// 字段访问器接口（用于反射）
/// </summary>
public interface IFieldAccessor
{
    /// <summary>
    /// 字段名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 字段编号
    /// </summary>
    int FieldNumber { get; }

    /// <summary>
    /// 字段类型
    /// </summary>
    FieldType FieldType { get; }

    /// <summary>
    /// 检查字段是否有值
    /// </summary>
    /// <param name="message">消息实例</param>
    /// <returns>如果字段有值则为 true</returns>
    bool HasValue(object message);

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <param name="message">消息实例</param>
    /// <returns>字段值</returns>
    object? GetValue(object message);

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="message">消息实例</param>
    /// <param name="value">字段值</param>
    void SetValue(object message, object? value);

    /// <summary>
    /// 清除字段值
    /// </summary>
    /// <param name="message">消息实例</param>
    void Clear(object message);
}
