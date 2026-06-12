using Xunit;
using Protobuf.Core;

namespace Protobuf.Tests;

/// <summary>
/// 基础类型编解码测试
/// </summary>
public class BasicTypesTests
{
    /// <summary>
    /// 辅助方法：写入并读取值
    /// </summary>
    private T WriteAndRead<T>(FieldCodec<T> codec, T value)
    {
        var output = new CodedOutputStream();
        codec.Write(output, value);
        var data = output.ToByteArray();

        var input = new CodedInputStream(data);
        input.ReadTag(); // 读取 tag
        return codec.Read(input); // 读取值
    }
    [Fact]
    public void TestInt32Codec()
    {
        var codec = FieldCodec.Int32(1);
        var expected = 42;
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestInt64Codec()
    {
        var codec = FieldCodec.Int64(1);
        var expected = 1234567890123456789L;
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestUInt32Codec()
    {
        var codec = FieldCodec.UInt32(1);
        var expected = 4294967295u;
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestSInt32Codec_ZigZagEncoding()
    {
        var codec = FieldCodec.SInt32(1);

        // Test negative number
        var expectedNeg = -42;
        var actualNeg = WriteAndRead(codec, expectedNeg);
        Assert.Equal(expectedNeg, actualNeg);

        // Test positive number
        var expectedPos = 42;
        var actualPos = WriteAndRead(codec, expectedPos);
        Assert.Equal(expectedPos, actualPos);
    }

    [Fact]
    public void TestFloatCodec()
    {
        var codec = FieldCodec.Float(1);
        var expected = 3.14159f;
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual, precision: 5);
    }

    [Fact]
    public void TestDoubleCodec()
    {
        var codec = FieldCodec.Double(1);
        var expected = 3.14159265358979;
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual, precision: 10);
    }

    [Fact]
    public void TestBoolCodec()
    {
        var codec = FieldCodec.Bool(1);

        // Test true
        var actualTrue = WriteAndRead(codec, true);
        Assert.True(actualTrue);

        // Test false
        var actualFalse = WriteAndRead(codec, false);
        Assert.False(actualFalse);
    }

    [Fact]
    public void TestStringCodec()
    {
        var codec = FieldCodec.String(1);
        var expected = "Hello, Protobuf!";
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestEmptyString()
    {
        var codec = FieldCodec.String(1);
        var expected = "";
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestBytesCodec()
    {
        var codec = FieldCodec.Bytes(1);
        var expected = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestEmptyBytes()
    {
        var codec = FieldCodec.Bytes(1);
        var expected = Array.Empty<byte>();
        var actual = WriteAndRead(codec, expected);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(16383)]
    [InlineData(16384)]
    [InlineData(2097151)]
    [InlineData(2097152)]
    [InlineData(268435455)]
    [InlineData(268435456)]
    public void TestVarintSize(uint value)
    {
        // Arrange & Act
        var size = SizeCalculator.ComputeUInt32Size(value);

        // Assert
        Assert.True(size > 0);
        Assert.True(size <= 5);
    }

    [Fact]
    public void TestWireFormat_MakeTag()
    {
        // Act
        var tag = WireFormat.MakeTag(1, WireFormat.VarintType);

        // Assert
        Assert.Equal((uint)0x08, tag); // field_number=1, wire_type=0 => tag=0x08
    }

    [Fact]
    public void TestWireFormat_GetTagFieldNumber()
    {
        // Arrange
        var tag = 0x08u; // field_number=1, wire_type=0

        // Act
        var fieldNumber = WireFormat.GetTagFieldNumber(tag);

        // Assert
        Assert.Equal(1, fieldNumber);
    }

    [Fact]
    public void TestWireFormat_GetTagWireType()
    {
        // Arrange
        var tag = 0x08u; // field_number=1, wire_type=0

        // Act
        var wireType = WireFormat.GetTagWireType(tag);

        // Assert
        Assert.Equal(WireFormat.VarintType, wireType);
    }
}
