using Xunit;
using Xunit.Abstractions;
using Protobuf.Core;

namespace Protobuf.Tests;

public class SimpleDebug
{
    private readonly ITestOutputHelper _output;

    public SimpleDebug(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DebugInt64()
    {
        var codec = FieldCodec.Int64(1);
        var expected = 1234567890123456789L;

        _output.WriteLine($"=== TestInt64Codec ===");
        _output.WriteLine($"Codec tag: {codec.Tag} (0x{codec.Tag:X})");

        var output = new CodedOutputStream();
        codec.Write(output, expected);
        var data = output.ToByteArray();

        _output.WriteLine($"Expected: {expected}");
        _output.WriteLine($"Expected (hex): 0x{expected:X}");
        _output.WriteLine($"Data: {BitConverter.ToString(data)}");
        _output.WriteLine($"Data length: {data.Length}");

        var input = new CodedInputStream(data);
        var tag = input.ReadTag();
        _output.WriteLine($"Tag read: {tag} (0x{tag:X})");
        _output.WriteLine($"Tag matches codec tag: {tag == codec.Tag}");

        var actual = codec.Read(input);
        _output.WriteLine($"Actual: {actual}");
        _output.WriteLine($"Actual (hex): 0x{actual:X}");
        _output.WriteLine($"Match: {expected == actual}");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DebugInt32()
    {
        var codec = FieldCodec.Int32(1);
        var expected = 42;

        var output = new CodedOutputStream();
        codec.Write(output, expected);
        var data = output.ToByteArray();

        _output.WriteLine($"Expected: {expected}");
        _output.WriteLine($"Expected (hex): 0x{expected:X}");
        _output.WriteLine($"Data: {BitConverter.ToString(data)}");
        _output.WriteLine($"Data length: {data.Length}");

        var input = new CodedInputStream(data);
        var tag = input.ReadTag();
        _output.WriteLine($"Tag: {tag} (0x{tag:X})");

        var actual = codec.Read(input);
        _output.WriteLine($"Actual: {actual}");
        _output.WriteLine($"Actual (hex): 0x{actual:X}");
        _output.WriteLine($"Match: {expected == actual}");
    }
}
