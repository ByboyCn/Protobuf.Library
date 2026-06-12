using Xunit;
using Protobuf.Core;
using Test;

namespace Protobuf.Tests;

/// <summary>
/// 调试测试 - 逐步验证序列化功能
/// </summary>
public class DebugTests
{
    [Fact]
    public void TestSingleFieldSerialization()
    {
        // 测试单个字段的序列化
        var message = new SimpleMessage();
        message.SetId(12345);

        var output = new CodedOutputStream();
        message.WriteTo(output);
        var data = output.ToByteArray();

        Console.WriteLine($"Single field data: {BitConverter.ToString(data)}");
        // 期望: 08-B9-60 (tag + varint 12345)

        var message2 = SimpleMessage.Parser.ParseFrom(data);
        Assert.Equal(12345, message2.Id);
    }

    [Fact]
    public void TestTwoFieldsSerialization()
    {
        // 测试两个字段的序列化
        var message = new SimpleMessage();
        message.SetId(12345);
        message.SetName("Test");

        var output = new CodedOutputStream();
        message.WriteTo(output);
        var data = output.ToByteArray();

        Console.WriteLine($"Two fields data: {BitConverter.ToString(data)}");
        // 期望: 08-B9-60-12-04-54-65-73-74

        var message2 = SimpleMessage.Parser.ParseFrom(data);
        Assert.Equal(12345, message2.Id);
        Assert.Equal("Test", message2.Name);
    }

    [Fact]
    public void TestBoolFieldSerialization()
    {
        // 测试布尔字段的序列化
        var message = new SimpleMessage();
        message.SetActive(true);

        var output = new CodedOutputStream();
        message.WriteTo(output);
        var data = output.ToByteArray();

        Console.WriteLine($"Bool field data: {BitConverter.ToString(data)}");
        // 期望: 18-01 (tag + true)

        var message2 = SimpleMessage.Parser.ParseFrom(data);
        Assert.True(message2.Active);
    }

    [Fact]
    public void TestManualStreamReading()
    {
        // 手动测试流读取
        // 08-B9-60 = field 1 (int32=12345)
        // 12-04-54-65-73-74 = field 2 (string="Test")
        // 18-01 = field 3 (bool=true)
        var data = new byte[] { 0x08, 0xB9, 0x60, 0x12, 0x04, 0x54, 0x65, 0x73, 0x74, 0x18, 0x01 };
        Console.WriteLine($"Manual test data: {BitConverter.ToString(data)}");

        var input = new CodedInputStream(data);

        // 读取第一个 tag
        uint tag1 = input.ReadTag();
        Console.WriteLine($"Tag 1: 0x{tag1:X} (field: {WireFormat.GetTagFieldNumber(tag1)}, wire: {WireFormat.GetTagWireType(tag1)})");

        // 读取 int32
        int value1 = input.ReadInt32();
        Console.WriteLine($"Value 1: {value1}");

        // 读取第二个 tag
        uint tag2 = input.ReadTag();
        Console.WriteLine($"Tag 2: 0x{tag2:X} (field: {WireFormat.GetTagFieldNumber(tag2)}, wire: {WireFormat.GetTagWireType(tag2)})");

        // 读取长度前缀
        uint length = input.ReadRawVarint32();
        Console.WriteLine($"Length: {length}");

        // 读取字符串数据
        var bytes = input.ReadRawBytes((int)length);
        string str = System.Text.Encoding.UTF8.GetString(bytes);
        Console.WriteLine($"String value: {str}");

        // 读取第三个 tag
        uint tag3 = input.ReadTag();
        Console.WriteLine($"Tag 3: 0x{tag3:X} (field: {WireFormat.GetTagFieldNumber(tag3)}, wire: {WireFormat.GetTagWireType(tag3)})");

        // 读取 bool
        bool value3 = input.ReadBool();
        Console.WriteLine($"Value 3: {value3}");

        // 尝试再读一个 tag（应该返回 0）
        uint tag4 = input.ReadTag();
        Console.WriteLine($"Tag 4: 0x{tag4:X} (should be 0)");

        Assert.Equal((uint)0, tag4);
    }
}
