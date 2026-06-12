using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Protobuf.Core;

/// <summary>
/// 用于写入 protobuf 编码数据的输出流
/// </summary>
public sealed class CodedOutputStream : IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private int _position;
    private readonly bool _leaveOpen;

    /// <summary>
    /// 初始化 CodedOutputStream
    /// </summary>
    /// <param name="stream">输出流</param>
    /// <param name="leaveOpen">是否保持流打开</param>
    public CodedOutputStream(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
        _buffer = ArrayPool<byte>.Shared.Rent(4096);
    }

    /// <summary>
    /// 初始化 CodedOutputStream（使用可扩展的内存流）
    /// </summary>
    public CodedOutputStream()
    {
        _stream = new MemoryStream();
        _leaveOpen = true;
        _buffer = ArrayPool<byte>.Shared.Rent(4096);
    }

    /// <summary>
    /// 获取底层流的字节数组（如果底层流是 MemoryStream）
    /// </summary>
    public byte[] ToByteArray()
    {
        Flush();
        if (_stream is MemoryStream memoryStream)
        {
            return memoryStream.ToArray();
        }
        throw new InvalidOperationException("Underlying stream is not a MemoryStream");
    }

    /// <summary>
    /// 写入 tag
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTag(int fieldNumber, WireType wireType)
    {
        WriteTag(WireFormat.MakeTag(fieldNumber, (int)wireType));
    }

    /// <summary>
    /// 写入 tag
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTag(uint tag)
    {
        WriteRawVarint32(tag);
    }

    /// <summary>
    /// 写入 32 位 varint
    /// </summary>
    public void WriteRawVarint32(uint value)
    {
        while (value > 0x7F)
        {
            WriteByte((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
        WriteByte((byte)value);
    }

    /// <summary>
    /// 写入 64 位 varint
    /// </summary>
    public void WriteRawVarint64(ulong value)
    {
        // 循环写入，每次处理7位
        while (value > 0x7F)
        {
            WriteByte((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
        WriteByte((byte)value);
    }

    /// <summary>
    /// 写入 int32
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32(int value)
    {
        WriteRawVarint32((uint)value);
    }

    /// <summary>
    /// 写入 uint32
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32(uint value)
    {
        WriteRawVarint32(value);
    }

    /// <summary>
    /// <summary>
    /// 写入 int64
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64(long value)
    {
        WriteRawVarint64((ulong)value);
    }

    /// <summary>
    /// 写入 uint64
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64(ulong value)
    {
        WriteRawVarint64(value);
    }

    /// <summary>
    /// 写入 sint32（ZigZag 编码）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSInt32(int value)
    {
        WriteRawVarint32(EncodeZigZag32(value));
    }

    /// <summary>
    /// 写入 sint64（ZigZag 编码）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSInt64(long value)
    {
        WriteRawVarint64(EncodeZigZag64(value));
    }

    /// <summary>
    /// 写入 fixed32
    /// </summary>
    public void WriteFixed32(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        WriteRawBytes(bytes, 0, 4);
    }

    /// <summary>
    /// 写入 fixed64
    /// </summary>
    public void WriteFixed64(ulong value)
    {
        var bytes = BitConverter.GetBytes(value);
        WriteRawBytes(bytes, 0, 8);
    }

    /// <summary>
    /// 写入 sfixed32
    /// </summary>
    public void WriteSFixed32(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        WriteRawBytes(bytes, 0, 4);
    }

    /// <summary>
    /// 写入 sfixed64
    /// </summary>
    public void WriteSFixed64(long value)
    {
        var bytes = BitConverter.GetBytes(value);
        WriteRawBytes(bytes, 0, 8);
    }

    /// <summary>
    /// 写入 bool
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool value)
    {
        WriteRawVarint32(value ? 1u : 0u);
    }

    /// <summary>
    /// 写入 enum
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteEnum(int value)
    {
        WriteRawVarint32((uint)value);
    }

    /// <summary>
    /// 写入 float
    /// </summary>
    public void WriteFloat(float value)
    {
        var bytes = BitConverter.GetBytes(value);
        WriteRawBytes(bytes, 0, 4);
    }

    /// <summary>
    /// 写入 double
    /// </summary>
    public void WriteDouble(double value)
    {
        var bytes = BitConverter.GetBytes(value);
        WriteRawBytes(bytes, 0, 8);
    }

    /// <summary>
    /// 写入 string
    /// </summary>
    public void WriteString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            WriteRawVarint32(0);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        WriteRawVarint32((uint)bytes.Length);
        WriteRawBytes(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 写入 bytes
    /// </summary>
    public void WriteBytes(byte[] value)
    {
        if (value == null || value.Length == 0)
        {
            WriteRawVarint32(0);
            return;
        }

        WriteRawVarint32((uint)value.Length);
        WriteRawBytes(value, 0, value.Length);
    }

    /// <summary>
    /// 写入原始字节
    /// </summary>
    public void WriteRawBytes(byte[] value, int offset, int length)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (offset < 0 || offset >= value.Length) throw new ArgumentOutOfRangeException(nameof(offset));
        if (length < 0 || offset + length > value.Length) throw new ArgumentOutOfRangeException(nameof(length));

        FlushIfBufferFull(length);

        int bytesWritten = 0;
        while (bytesWritten < length)
        {
            int toWrite = Math.Min(length - bytesWritten, _buffer.Length - _position);
            Array.Copy(value, offset + bytesWritten, _buffer, _position, toWrite);
            _position += toWrite;
            bytesWritten += toWrite;

            if (_position >= _buffer.Length)
            {
                Flush();
            }
        }
    }

    /// <summary>
    /// 写入长度前缀的消息
    /// </summary>
    public void WriteMessage<T>(T message) where T : IMessage<T>
    {
        // 计算消息大小
        int size = message.CalculateSize();

        // 写入长度前缀
        WriteRawVarint32((uint)size);

        // 写入消息内容
        message.WriteTo(this);
    }

    /// <summary>
    /// 刷新缓冲区
    /// </summary>
    public void Flush()
    {
        if (_position > 0)
        {
            _stream.Write(_buffer, 0, _position);
            _position = 0;
        }
    }

    /// <summary>
    /// 如果缓冲区空间不足，则刷新
    /// </summary>
    private void FlushIfBufferFull(int requiredSize)
    {
        if (requiredSize > _buffer.Length - _position)
        {
            Flush();
        }
    }

    /// <summary>
    /// 写入单个字节
    /// </summary>
    private void WriteByte(byte value)
    {
        if (_position >= _buffer.Length)
        {
            Flush();
        }

        _buffer[_position++] = value;
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

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Flush();

        if (!_leaveOpen)
        {
            _stream?.Dispose();
        }

        if (_buffer != null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
        }
    }
}
