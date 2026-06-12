using Xunit;
using Protobuf.Core;
using Test;

namespace Protobuf.Tests;

/// <summary>
/// 手写消息类的序列化测试
/// </summary>
public class SimpleMessageTests
{
    [Fact]
    public void TestSimpleMessage_Serialization()
    {
        // Arrange
        var message = new SimpleMessage();
        message.SetId(12345);
        message.SetName("Test Message");
        message.SetActive(true);

        // Act - 序列化
        var output = new CodedOutputStream();
        message.WriteTo(output);
        var data = output.ToByteArray();

        // 调试：输出字节内容
        Console.WriteLine($"Serialized data: {BitConverter.ToString(data)}");
        Console.WriteLine($"Data length: {data.Length}");

        // Assert - 验证序列化结果不为空
        Assert.NotNull(data);
        Assert.True(data.Length > 0);

        // Act - 反序列化
        var message2 = SimpleMessage.Parser.ParseFrom(data);

        // Assert - 验证反序列化结果
        Assert.Equal(message.Id, message2.Id);
        Assert.Equal(message.Name, message2.Name);
        Assert.Equal(message.Active, message2.Active);
    }

    [Fact]
    public void TestSimpleMessage_PartialFields()
    {
        // 测试只设置部分字段的情况
        var message = new SimpleMessage();
        message.SetId(999);
        // 不设置 name 和 active

        var output = new CodedOutputStream();
        message.WriteTo(output);
        var data = output.ToByteArray();

        var message2 = SimpleMessage.Parser.ParseFrom(data);

        Assert.Equal(999, message2.Id);
        Assert.Equal("", message2.Name);
        Assert.Equal(false, message2.Active);
    }

    [Fact]
    public void TestSimpleMessage_EmptyMessage()
    {
        // 测试空消息的序列化
        var message = new SimpleMessage();
        // 不设置任何字段

        var output = new CodedOutputStream();
        message.WriteTo(output);
        var data = output.ToByteArray();

        // 空消息应该序列化为空字节数组
        Assert.Equal(0, data.Length);

        var message2 = SimpleMessage.Parser.ParseFrom(data);

        Assert.Equal(0, message2.Id);
        Assert.Equal("", message2.Name);
        Assert.Equal(false, message2.Active);
    }

    [Fact]
    public void TestSimpleMessage_Clone()
    {
        var message1 = new SimpleMessage();
        message1.SetId(111);
        message1.SetName("Clone Test");
        message1.SetActive(false);

        var message2 = message1.Clone();

        // 验证值相同
        Assert.Equal(message1.Id, message2.Id);
        Assert.Equal(message1.Name, message2.Name);
        Assert.Equal(message1.Active, message2.Active);

        // 验证是不同的实例
        Assert.NotSame(message1, message2);

        // 修改 message1 不影响 message2
        message1.SetId(222);
        Assert.Equal(111, message2.Id);
    }

    [Fact]
    public void TestSimpleMessage_MergeFrom()
    {
        var message1 = new SimpleMessage();
        message1.SetId(100);
        message1.SetName("First");

        var message2 = new SimpleMessage();
        message2.SetId(200);
        message2.SetActive(true);

        // 将 message2 合并到 message1
        message1.MergeFrom(message2);

        // message1 应该包含 message2 的所有值
        Assert.Equal(200, message1.Id);
        Assert.Equal("First", message1.Name); // 保持原有值
        Assert.Equal(true, message1.Active);
    }

    [Fact]
    public void TestSimpleMessage_CalculateSize()
    {
        var message = new SimpleMessage();
        message.SetId(12345);
        message.SetName("Hello");
        message.SetActive(true);

        var size = message.CalculateSize();

        Assert.True(size > 0);

        // 序列化后的实际大小应该与计算的大小相等
        var output = new CodedOutputStream();
        message.WriteTo(output);
        var data = output.ToByteArray();

        Assert.Equal(size, data.Length);
    }
}
