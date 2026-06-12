using Xunit;
using Protobuf.Core;

namespace Protobuf.Tests;

/// <summary>
/// CodedInputStream 基础功能测试
/// </summary>
public class CodedInputStreamTests
{
    [Fact]
    public void TestReadTag_SingleVarint()
    {
        // 测试单个 varint tag 的读取
        var data = new byte[] { 0x08 }; // tag: field 1, wire type 0
        var input = new CodedInputStream(data);

        uint tag = input.ReadTag();
        Assert.Equal((uint)0x08, tag);

        // 再次读取应该返回 0（流结束）
        uint tag2 = input.ReadTag();
        Assert.Equal((uint)0, tag2);
    }

    [Fact]
    public void TestReadVarint_Simple()
    {
        // 测试简单 varint 的读取
        var data = new byte[] { 0x96, 0x01 }; // 150 的 varint 编码
        var input = new CodedInputStream(data);

        uint value = input.ReadRawVarint32();
        Assert.Equal((uint)150, value);
    }

    [Fact]
    public void TestReadTagAndValue()
    {
        // 测试读取 tag 和值
        // 0x08 = tag (field 1, wire type 0)
        // 0x96, 0x01 = varint 150
        var data = new byte[] { 0x08, 0x96, 0x01 };
        var input = new CodedInputStream(data);

        uint tag = input.ReadTag();
        Assert.Equal((uint)0x08, tag);

        uint value = input.ReadRawVarint32();
        Assert.Equal((uint)150, value);

        // 流应该结束
        uint endTag = input.ReadTag();
        Assert.Equal((uint)0, endTag);
    }

    [Fact]
    public void TestReadString()
    {
        // 测试字符串读取
        // 0x12 = tag (field 2, wire type 2)
        // 0x05 = 长度 5
        // 0x48, 0x65, 0x6C, 0x6C, 0x6F = "Hello"
        var data = new byte[] { 0x12, 0x05, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        var input = new CodedInputStream(data);

        uint tag = input.ReadTag();
        Assert.Equal((uint)0x12, tag);

        string value = input.ReadString();
        Assert.Equal("Hello", value);

        // 流应该结束
        uint endTag = input.ReadTag();
        Assert.Equal((uint)0, endTag);
    }
}
