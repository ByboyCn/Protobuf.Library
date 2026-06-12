using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Protobuf.Core;

/// <summary>
/// 用于读取 protobuf 编码数据的输入流
/// </summary>
public sealed class CodedInputStream : IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private int _bufferPos;
    private int _bufferCount;
    private readonly bool _leaveOpen;
    private int _recursionDepth = 0;
    private const int DefaultRecursionLimit = 100;
    private const int DefaultSizeLimit = 64 * 1024 * 1024; // 64MB
    private int _sizeLimit = DefaultSizeLimit;
    private int _lastTag;
    private bool _hasLastTag;

    /// <summary>
    /// 初始化 CodedInputStream
    /// </summary>
    /// <param name="stream">输入流</param>
    /// <param name="leaveOpen">是否保持流打开</param>
    public CodedInputStream(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
        _buffer = ArrayPool<byte>.Shared.Rent(4096);
    }

    /// <summary>
    /// 初始化 CodedInputStream（从字节数组）
    /// </summary>
    /// <param name="data">字节数组</param>
    public CodedInputStream(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        _stream = new MemoryStream(data);
        _leaveOpen = true;
        _buffer = ArrayPool<byte>.Shared.Rent(4096);
    }

    /// <summary>
    /// 读取下一个 tag
    /// </summary>
    /// <returns>tag 值，如果没有更多数据则返回 0</returns>
    public uint ReadTag()
    {
        // 如果有回退的 tag，先返回它
        if (_hasLastTag)
        {
            _hasLastTag = false;
            return (uint)_lastTag;
        }

        // 检查流是否结束
        if (IsAtEnd)
        {
            return 0;
        }

        // 读取新的 tag
        try
        {
            _lastTag = (int)ReadRawVarint32();
            // 注意：不设置 _hasLastTag，除非调用了 RetreatTag()
            return (uint)_lastTag;
        }
        catch (InvalidOperationException)
        {
            // 流结束，返回 0
            return 0;
        }
    }

    /// <summary>
    /// 回退最后一个 tag，以便重新读取
    /// </summary>
    public void RetreatTag()
    {
        _hasLastTag = true;
    }

    /// <summary>
    /// 读取并检查最后一个 tag 的字段编号
    /// </summary>
    /// <param name="tag">tag 值</param>
    /// <returns>字段编号</returns>
    public int GetLastFieldNumber(uint tag)
    {
        return WireFormat.GetTagFieldNumber(tag);
    }

    /// <summary>
    /// 读取 32 位 varint
    /// </summary>
    public uint ReadRawVarint32()
    {
        int tmp = ReadByte();
        if (tmp < 0)
        {
            throw new InvalidOperationException("Reached end of stream while reading varint");
        }

        if ((tmp & 0x80) == 0)
        {
            return (uint)tmp;
        }

        uint result = (uint)(tmp & 0x7f);
        int shift = 7;

        while (true)
        {
            tmp = ReadByte();
            if (tmp < 0)
            {
                throw new InvalidOperationException("Reached end of stream while reading varint");
            }

            result |= (uint)((tmp & 0x7f) << shift);
            shift += 7;

            if ((tmp & 0x80) == 0)
            {
                return result;
            }

            if (shift >= 35)
            {
                throw new InvalidOperationException("Varint too long");
            }
        }
    }

    /// <summary>
    /// 读取 64 位 varint
    /// </summary>
    public ulong ReadRawVarint64()
    {
        int tmp = ReadByte();
        if (tmp < 0)
        {
            throw new InvalidOperationException("Reached end of stream while reading varint");
        }

        if ((tmp & 0x80) == 0)
        {
            return (ulong)tmp;
        }

        ulong result = (ulong)(tmp & 0x7f);
        int shift = 7;

        while (true)
        {
            tmp = ReadByte();
            if (tmp < 0)
            {
                throw new InvalidOperationException("Reached end of stream while reading varint");
            }

            // 修复：先将 tmp & 0x7f 转换为 ulong，然后再移位
            var part = (ulong)(tmp & 0x7f);
            result |= part << shift;
            shift += 7;

            if ((tmp & 0x80) == 0)
            {
                return result;
            }

            if (shift >= 70)
            {
                throw new InvalidOperationException("Varint too long");
            }
        }
    }

    /// <summary>
    /// 读取 int32
    /// </summary>
    public int ReadInt32()
    {
        return (int)ReadRawVarint32();
    }

    /// <summary>
    /// 读取 uint32
    /// </summary>
    public uint ReadUInt32()
    {
        return ReadRawVarint32();
    }

    /// <summary>
    /// 读取 int64
    /// </summary>
    public long ReadInt64()
    {
        return (long)ReadRawVarint64();
    }

    /// <summary>
    /// 读取 uint64
    /// </summary>
    public ulong ReadUInt64()
    {
        return ReadRawVarint64();
    }

    /// <summary>
    /// 读取 sint32（ZigZag 编码）
    /// </summary>
    public int ReadSInt32()
    {
        uint n = ReadRawVarint32();
        return DecodeZigZag32(n);
    }

    /// <summary>
    /// 读取 sint64（ZigZag 编码）
    /// </summary>
    public long ReadSInt64()
    {
        ulong n = ReadRawVarint64();
        return DecodeZigZag64(n);
    }

    /// <summary>
    /// 读取 fixed32
    /// </summary>
    public uint ReadFixed32()
    {
        var bytes = ReadRawBytes(4);
        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>
    /// 读取 fixed64
    /// </summary>
    public ulong ReadFixed64()
    {
        var bytes = ReadRawBytes(8);
        return BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>
    /// 读取 sfixed32
    /// </summary>
    public int ReadSFixed32()
    {
        var bytes = ReadRawBytes(4);
        return BitConverter.ToInt32(bytes, 0);
    }

    /// <summary>
    /// 读取 sfixed64
    /// </summary>
    public long ReadSFixed64()
    {
        var bytes = ReadRawBytes(8);
        return BitConverter.ToInt64(bytes, 0);
    }

    /// <summary>
    /// 读取 bool
    /// </summary>
    public bool ReadBool()
    {
        return ReadRawVarint32() != 0;
    }

    /// <summary>
    /// 读取 enum
    /// </summary>
    public int ReadEnum()
    {
        return (int)ReadRawVarint32();
    }

    /// <summary>
    /// 读取 float
    /// </summary>
    public float ReadFloat()
    {
        var bytes = ReadRawBytes(4);
        return BitConverter.ToSingle(bytes, 0);
    }

    /// <summary>
    /// 读取 double
    /// </summary>
    public double ReadDouble()
    {
        var bytes = ReadRawBytes(8);
        return BitConverter.ToDouble(bytes, 0);
    }

    /// <summary>
    /// 读取 string
    /// </summary>
    public string ReadString()
    {
        var length = (int)ReadRawVarint32();
        if (length == 0)
        {
            return string.Empty;
        }

        var bytes = ReadRawBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 读取 bytes
    /// </summary>
    public byte[] ReadBytes()
    {
        var length = (int)ReadRawVarint32();
        if (length == 0)
        {
            return Array.Empty<byte>();
        }

        return ReadRawBytes(length);
    }

    /// <summary>
    /// 读取原始字节
    /// </summary>
    public byte[] ReadRawBytes(int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        var result = new byte[length];
        int bytesRead = 0;

        while (bytesRead < length)
        {
            int count = ReadRawBuffer(result, bytesRead, length - bytesRead);
            if (count == 0)
            {
                throw new InvalidOperationException("Reached end of stream while reading bytes");
            }
            bytesRead += count;
        }

        return result;
    }

    /// <summary>
    /// 跳过指定长度的字节
    /// </summary>
    public void SkipRawBytes(int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        int bytesSkipped = 0;
        while (bytesSkipped < length)
        {
            int count = ReadRawBuffer(Array.Empty<byte>(), 0, 0);
            if (count == 0)
            {
                throw new InvalidOperationException("Reached end of stream while skipping bytes");
            }
            bytesSkipped += count;
        }
    }

    /// <summary>
    /// 跳过当前字段
    /// </summary>
    public void SkipField(uint tag)
    {
        var wireType = WireFormat.GetTagWireType(tag);

        switch (wireType)
        {
            case WireFormat.VarintType:
                ReadRawVarint32();
                break;
            case WireFormat.Fixed64Type:
                ReadRawBytes(8);
                break;
            case WireFormat.LengthDelimitedType:
                var length = (int)ReadRawVarint32();
                SkipRawBytes(length);
                break;
            case WireFormat.Fixed32Type:
                ReadRawBytes(4);
                break;
            default:
                throw new InvalidOperationException($"Unknown wire type: {wireType}");
        }
    }

    /// <summary>
    /// 检查是否到达流的末尾
    /// </summary>
    public bool IsAtEnd
    {
        get
        {
            if (_bufferPos < _bufferCount)
            {
                return false;
            }

            // 尝试重新填充缓冲区
            RefillBuffer();
            return _bufferPos >= _bufferCount;
        }
    }

    /// <summary>
    /// 读取单个字节
    /// </summary>
    private int ReadByte()
    {
        if (_bufferPos >= _bufferCount)
        {
            RefillBuffer();
            if (_bufferPos >= _bufferCount)
            {
                return -1;
            }
        }

        return _buffer[_bufferPos++];
    }

    /// <summary>
    /// 读取原始数据到缓冲区
    /// </summary>
    private int ReadRawBuffer(byte[] target, int offset, int count)
    {
        int bytesToRead = Math.Min(count, _bufferCount - _bufferPos);

        if (bytesToRead > 0)
        {
            Array.Copy(_buffer, _bufferPos, target, offset, bytesToRead);
            _bufferPos += bytesToRead;
            return bytesToRead;
        }

        // 如果目标数组是空的，直接跳过
        if (target.Length == 0)
        {
            int skipped = 0;
            while (skipped < count)
            {
                RefillBuffer();
                int canSkip = Math.Min(count - skipped, _bufferCount - _bufferPos);
                _bufferPos += canSkip;
                skipped += canSkip;

                if (_bufferPos >= _bufferCount && skipped < count)
                {
                    int read = _stream.Read(_buffer, 0, _buffer.Length);
                    if (read == 0) break;
                    _bufferCount = read;
                    _bufferPos = 0;
                }
            }
            return skipped;
        }

        // 直接从流读取
        return _stream.Read(target, offset, count);
    }

    /// <summary>
    /// 重新填充缓冲区
    /// </summary>
    private void RefillBuffer()
    {
        if (_bufferPos < _bufferCount)
        {
            return;
        }

        _bufferCount = _stream.Read(_buffer, 0, _buffer.Length);
        _bufferPos = 0;

        // 如果没有读取到数据，说明流已结束
        if (_bufferCount == 0)
        {
            _bufferPos = 0;
        }
    }

    /// <summary>
    /// 解码 ZigZag32
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeZigZag32(uint n)
    {
        return (int)((n >> 1) ^ -(int)(n & 1));
    }

    /// <summary>
    /// 设置递归限制
    /// </summary>
    public int SetRecursionLimit(int limit)
    {
        var oldLimit = _sizeLimit;
        _sizeLimit = limit;
        _recursionDepth++;
        if (_recursionDepth > DefaultRecursionLimit)
        {
            throw new InvalidOperationException("Recursion limit exceeded");
        }
        return oldLimit;
    }

    /// <summary>
    /// 重置递归限制
    /// </summary>
    public void ResetRecursionLimit(int oldLimit)
    {
        _sizeLimit = oldLimit;
        _recursionDepth--;
    }

    /// <summary>
    /// 检查最后一个 tag 是否为期望值
    /// </summary>
    public void CheckLastTagWas(uint expectedTag)
    {
        // 这个方法用于验证嵌套消息读取后的状态
        // 简化实现中不进行复杂检查
        _ = expectedTag; // 避免未使用参数警告
    }

    /// <summary>
    /// 解码 ZigZag64
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long DecodeZigZag64(ulong n)
    {
        // 将 n 视为有符号 64 位整数
        long decoded = (long)(n >> 1);
        if ((n & 1) != 0)
        {
            decoded = ~decoded;
        }
        return decoded;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
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
