using System.Runtime.CompilerServices;

namespace Protobuf.Core;

/// <summary>
/// 字段编解码器基类
/// </summary>
/// <typeparam name="T">字段值类型</typeparam>
public sealed class FieldCodec<T>
{
    private readonly uint _tag;
    private readonly WireType _wireType;
    private readonly Func<CodedInputStream, T> _reader;
    private readonly Action<CodedOutputStream, T> _writer;
    private readonly Func<T, int>? _sizeCalculator;

    /// <summary>
    /// 初始化 FieldCodec
    /// </summary>
    public FieldCodec(
        int fieldNumber,
        WireType wireType,
        Func<CodedInputStream, T> reader,
        Action<CodedOutputStream, T> writer,
        Func<T, int>? sizeCalculator = null)
    {
        _tag = WireFormat.MakeTag(fieldNumber, (int)wireType);
        _wireType = wireType;
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _sizeCalculator = sizeCalculator;
    }

    /// <summary>
    /// 读取字段值
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read(CodedInputStream input)
    {
        return _reader(input);
    }

    /// <summary>
    /// 写入字段值
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(CodedOutputStream output, T value)
    {
        output.WriteTag(_tag);
        _writer(output, value);
    }

    /// <summary>
    /// 计算字段值的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CalculateSize(T value)
    {
        if (_sizeCalculator == null)
        {
            // 默认大小计算（包括 tag）
            return SizeCalculator.ComputeUInt32Size(_tag);
        }

        return SizeCalculator.ComputeUInt32Size(_tag) + _sizeCalculator(value);
    }

    /// <summary>
    /// 获取 tag
    /// </summary>
    public uint Tag => _tag;

    /// <summary>
    /// 获取 Wire 类型
    /// </summary>
    public WireType WireType => _wireType;
}

/// <summary>
/// 字段编解码器工具类
/// </summary>
public static class FieldCodec
{
    // ===== 基础类型编解码器 =====

    /// <summary>Int32 编解码器</summary>
    public static FieldCodec<int> Int32(int fieldNumber) =>
        new FieldCodec<int>(fieldNumber, WireType.Varint,
            input => input.ReadInt32(),
            (output, value) => output.WriteInt32(value),
            value => SizeCalculator.ComputeInt32Size(value));

    /// <summary>UInt32 编解码器</summary>
    public static FieldCodec<uint> UInt32(int fieldNumber) =>
        new FieldCodec<uint>(fieldNumber, WireType.Varint,
            input => input.ReadUInt32(),
            (output, value) => output.WriteUInt32(value),
            value => SizeCalculator.ComputeUInt32Size(value));

    /// <summary>Int64 编解码器</summary>
    public static FieldCodec<long> Int64(int fieldNumber) =>
        new FieldCodec<long>(fieldNumber, WireType.Varint,
            input => input.ReadInt64(),
            (output, value) => output.WriteInt64(value),
            value => SizeCalculator.ComputeInt64Size(value));

    /// <summary>UInt64 编解码器</summary>
    public static FieldCodec<ulong> UInt64(int fieldNumber) =>
        new FieldCodec<ulong>(fieldNumber, WireType.Varint,
            input => input.ReadUInt64(),
            (output, value) => output.WriteUInt64(value),
            value => SizeCalculator.ComputeUInt64Size(value));

    /// <summary>SInt32 编解码器</summary>
    public static FieldCodec<int> SInt32(int fieldNumber) =>
        new FieldCodec<int>(fieldNumber, WireType.Varint,
            input => input.ReadSInt32(),
            (output, value) => output.WriteSInt32(value),
            value => SizeCalculator.ComputeSInt32Size(value));

    /// <summary>SInt64 编解码器</summary>
    public static FieldCodec<long> SInt64(int fieldNumber) =>
        new FieldCodec<long>(fieldNumber, WireType.Varint,
            input => input.ReadSInt64(),
            (output, value) => output.WriteSInt64(value),
            value => SizeCalculator.ComputeSInt64Size(value));

    /// <summary>Fixed32 编解码器</summary>
    public static FieldCodec<uint> Fixed32(int fieldNumber) =>
        new FieldCodec<uint>(fieldNumber, WireType.Fixed32,
            input => input.ReadFixed32(),
            (output, value) => output.WriteFixed32(value),
            value => 4);

    /// <summary>Fixed64 编解码器</summary>
    public static FieldCodec<ulong> Fixed64(int fieldNumber) =>
        new FieldCodec<ulong>(fieldNumber, WireType.Fixed64,
            input => input.ReadFixed64(),
            (output, value) => output.WriteFixed64(value),
            value => 8);

    /// <summary>SFixed32 编解码器</summary>
    public static FieldCodec<int> SFixed32(int fieldNumber) =>
        new FieldCodec<int>(fieldNumber, WireType.Fixed32,
            input => input.ReadSFixed32(),
            (output, value) => output.WriteSFixed32(value),
            value => 4);

    /// <summary>SFixed64 编解码器</summary>
    public static FieldCodec<long> SFixed64(int fieldNumber) =>
        new FieldCodec<long>(fieldNumber, WireType.Fixed64,
            input => input.ReadSFixed64(),
            (output, value) => output.WriteSFixed64(value),
            value => 8);

    /// <summary>Bool 编解码器</summary>
    public static FieldCodec<bool> Bool(int fieldNumber) =>
        new FieldCodec<bool>(fieldNumber, WireType.Varint,
            input => input.ReadBool(),
            (output, value) => output.WriteBool(value),
            value => 1);

    /// <summary>Float 编解码器</summary>
    public static FieldCodec<float> Float(int fieldNumber) =>
        new FieldCodec<float>(fieldNumber, WireType.Fixed32,
            input => input.ReadFloat(),
            (output, value) => output.WriteFloat(value),
            value => 4);

    /// <summary>Double 编解码器</summary>
    public static FieldCodec<double> Double(int fieldNumber) =>
        new FieldCodec<double>(fieldNumber, WireType.Fixed64,
            input => input.ReadDouble(),
            (output, value) => output.WriteDouble(value),
            value => 8);

    /// <summary>String 编解码器</summary>
    public static FieldCodec<string> String(int fieldNumber) =>
        new FieldCodec<string>(fieldNumber, WireType.LengthDelimited,
            input => input.ReadString(),
            (output, value) => output.WriteString(value),
            value => SizeCalculator.ComputeStringSize(value));

    /// <summary>Bytes 编解码器</summary>
    public static FieldCodec<byte[]> Bytes(int fieldNumber) =>
        new FieldCodec<byte[]>(fieldNumber, WireType.LengthDelimited,
            input => input.ReadBytes(),
            (output, value) => output.WriteBytes(value),
            value => SizeCalculator.ComputeBytesSize(value));

    /// <summary>Enum 编解码器</summary>
    public static FieldCodec<int> Enum(int fieldNumber) =>
        new FieldCodec<int>(fieldNumber, WireType.Varint,
            input => input.ReadEnum(),
            (output, value) => output.WriteEnum(value),
            value => SizeCalculator.ComputeEnumSize(value));

    /// <summary>消息编解码器</summary>
    public static FieldCodec<TMessage> Message<TMessage>(
        int fieldNumber,
        Func<TMessage> factory) where TMessage : IMessage<TMessage> =>
        new FieldCodec<TMessage>(fieldNumber, WireType.LengthDelimited,
            input => ReadMessage(input, factory),
            (output, value) => output.WriteMessage(value),
            value => SizeCalculator.ComputeMessageSize(value));

    private static TMessage ReadMessage<TMessage>(
        CodedInputStream input,
        Func<TMessage> factory) where TMessage : IMessage<TMessage>
    {
        var length = (int)input.ReadRawVarint32();
        var oldLimit = input.SetRecursionLimit(length);
        try
        {
            var message = factory();
            message.MergeFrom(input);
            if (length != 0)
            {
                input.CheckLastTagWas(0);
            }
            return message;
        }
        finally
        {
            input.ResetRecursionLimit(oldLimit);
        }
    }
}

/// <summary>
/// Protobuf 大小计算工具类
/// </summary>
public static class SizeCalculator
{
    /// <summary>
    /// 计算 int32 的 varint 大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeInt32Size(int value)
    {
        return ComputeUInt32Size((uint)value);
    }

    /// <summary>
    /// 计算 uint32 的 varint 大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeUInt32Size(uint value)
    {
        if (value < 0x80)
        {
            return 1;
        }

        if (value < 0x4000)
        {
            return 2;
        }

        if (value < 0x200000)
        {
            return 3;
        }

        if (value < 0x10000000)
        {
            return 4;
        }

        return 5;
    }

    /// <summary>
    /// 计算 int64 的 varint 大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeInt64Size(long value)
    {
        return ComputeUInt64Size((ulong)value);
    }

    /// <summary>
    /// 计算 uint64 的 varint 大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeUInt64Size(ulong value)
    {
        if (value < 0x80)
        {
            return 1;
        }

        if (value < 0x4000)
        {
            return 2;
        }

        if (value < 0x200000)
        {
            return 3;
        }

        if (value < 0x10000000)
        {
            return 4;
        }

        if (value < 0x800000000)
        {
            return 5;
        }

        if (value < 0x40000000000)
        {
            return 6;
        }

        if (value < 0x2000000000000)
        {
            return 7;
        }

        if (value < 0x100000000000000)
        {
            return 8;
        }

        if (value < 0x8000000000000000)
        {
            return 9;
        }

        return 10;
    }

    /// <summary>
    /// 计算 sint32 的 varint 大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeSInt32Size(int value)
    {
        return ComputeUInt32Size(EncodeZigZag32(value));
    }

    /// <summary>
    /// 计算 sint64 的 varint 大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeSInt64Size(long value)
    {
        return ComputeUInt64Size(EncodeZigZag64(value));
    }

    /// <summary>
    /// 计算 float 的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeFloatSize(float value)
    {
        return 4;
    }

    /// <summary>
    /// 计算 double 的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeDoubleSize(double value)
    {
        return 8;
    }

    /// <summary>
    /// 计算 bool 的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeBoolSize(bool value)
    {
        return 1;
    }

    /// <summary>
    /// 计算 enum 的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeEnumSize(int value)
    {
        return ComputeUInt32Size((uint)value);
    }

    /// <summary>
    /// 计算 tag 的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeTagSize(int fieldNumber)
    {
        return ComputeUInt32Size(WireFormat.MakeTag(fieldNumber, 0));
    }

    /// <summary>
    /// 计算 string 的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeStringSize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ComputeUInt32Size(0);
        }

        var byteCount = System.Text.Encoding.UTF8.GetByteCount(value);
        return ComputeUInt32Size((uint)byteCount) + byteCount;
    }

    /// <summary>
    /// 计算 bytes 的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeBytesSize(byte[] value)
    {
        if (value == null || value.Length == 0)
        {
            return ComputeUInt32Size(0);
        }

        return ComputeUInt32Size((uint)value.Length) + value.Length;
    }

    /// <summary>
    /// 计算消息的大小
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeMessageSize<T>(T message) where T : IMessage<T>
    {
        var size = message.CalculateSize();
        return ComputeUInt32Size((uint)size) + size;
    }

    /// <summary>
    /// 编码 ZigZag32
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncodeZigZag32(int n)
    {
        return (uint)((n << 1) ^ (n >> 31));
    }

    /// <summary>
    /// 编码 ZigZag64
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong EncodeZigZag64(long n)
    {
        return (ulong)((n << 1) ^ (n >> 63));
    }
}
