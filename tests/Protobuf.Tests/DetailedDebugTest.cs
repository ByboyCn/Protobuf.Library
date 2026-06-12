using Xunit;
using Protobuf.Core;

namespace Protobuf.Tests;

/// <summary>
/// 详细的调试测试
/// </summary>
public class DetailedDebugTest
{
    [Fact]
    public void TestDetailedStreamReading()
    {
        var data = new byte[] { 0x08 };
        Console.WriteLine($"Input data: {BitConverter.ToString(data)}");

        var input = new CodedInputStream(data);

        // 第一次读取
        Console.WriteLine("First ReadTag():");
        uint tag1 = input.ReadTag();
        Console.WriteLine($"  Result: 0x{tag1:X}");
        Console.WriteLine($"  IsAtEnd: {input.IsAtEnd}");

        // 第二次读取
        Console.WriteLine("Second ReadTag():");
        uint tag2 = input.ReadTag();
        Console.WriteLine($"  Result: 0x{tag2:X}");
        Console.WriteLine($"  IsAtEnd: {input.IsAtEnd}");

        Assert.Equal((uint)0x08, tag1);
        Assert.Equal((uint)0, tag2);
    }
}
