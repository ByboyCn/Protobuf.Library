using System;
using Protobuf.Core;

namespace Examples;

/// <summary>
/// Protobuf 编解码器使用示例
/// </summary>
class CodecExample
{
    public static void Run(string[] args)
    {
        Console.WriteLine("=== Protobuf Codec 使用示例 ===\n");

        // 示例 1: 整数类型编解码
        Console.WriteLine("示例 1: 整数类型编解码");
        TestIntegerCodecs();

        // 示例 2: 浮点类型编解码
        Console.WriteLine("\n示例 2: 浮点类型编解码");
        TestFloatCodecs();

        // 示例 3: 字符串和字节编解码
        Console.WriteLine("\n示例 3: 字符串和字节编解码");
        TestStringAndBytesCodecs();

        // 示例 4: Varint 编码原理
        Console.WriteLine("\n示例 4: Varint 编码原理演示");
        DemonstrateVarintEncoding();

        // 示例 5: ZigZag 编码（用于有符号整数）
        Console.WriteLine("\n示例 5: ZigZag 编码");
        DemonstrateZigZagEncoding();

        Console.WriteLine("\n=== 示例完成 ===");
    }

    /// <summary>
    /// 测试整数类型编解码
    /// </summary>
    static void TestIntegerCodecs()
    {
        // int32
        var int32Codec = FieldCodec.Int32(1);
        var int32Value = 42;
        var int32Encoded = Encode(int32Codec, int32Value);
        var int32Decoded = Decode(int32Codec, int32Encoded);
        Console.WriteLine($"  int32: {int32Value} -> {BitConverter.ToString(int32Encoded)} -> {int32Decoded} (匹配: {int32Value == int32Decoded})");

        // int64 (大数值)
        var int64Codec = FieldCodec.Int64(2);
        var int64Value = 1234567890123456789L;
        var int64Encoded = Encode(int64Codec, int64Value);
        var int64Decoded = Decode(int64Codec, int64Encoded);
        Console.WriteLine($"  int64: {int64Value} -> {BitConverter.ToString(int64Encoded)} (多字节 varint, 匹配: {int64Value == int64Decoded})");

        // uint32
        var uint32Codec = FieldCodec.UInt32(3);
        var uint32Value = 4294967295u;
        var uint32Encoded = Encode(uint32Codec, uint32Value);
        var uint32Decoded = Decode(uint32Codec, uint32Encoded);
        Console.WriteLine($"  uint32: {uint32Value} -> {BitConverter.ToString(uint32Encoded)} (最大值, 匹配: {uint32Value == uint32Decoded})");

        // bool
        var boolCodec = FieldCodec.Bool(4);
        Console.WriteLine($"  bool: true -> {BitConverter.ToString(Encode(boolCodec, true))}");
        Console.WriteLine($"  bool: false -> {BitConverter.ToString(Encode(boolCodec, false))}");
    }

    /// <summary>
    /// 测试浮点类型编解码
    /// </summary>
    static void TestFloatCodecs()
    {
        // float
        var floatCodec = FieldCodec.Float(1);
        var floatValue = 3.14159f;
        var floatEncoded = Encode(floatCodec, floatValue);
        var floatDecoded = Decode(floatCodec, floatEncoded);
        Console.WriteLine($"  float: {floatValue} -> {BitConverter.ToString(floatEncoded)} (4 字节, 匹配: {floatValue == floatDecoded})");

        // double
        var doubleCodec = FieldCodec.Double(2);
        var doubleValue = Math.PI;
        var doubleEncoded = Encode(doubleCodec, doubleValue);
        var doubleDecoded = Decode(doubleCodec, doubleEncoded);
        Console.WriteLine($"  double: {doubleValue} -> {BitConverter.ToString(doubleEncoded)} (8 字节, 匹配: {doubleValue == doubleDecoded})");
    }

    /// <summary>
    /// 测试字符串和字节编解码
    /// </summary>
    static void TestStringAndBytesCodecs()
    {
        // string
        var stringCodec = FieldCodec.String(1);
        var stringValue = "Hello, Protobuf!";
        var stringEncoded = Encode(stringCodec, stringValue);
        var stringDecoded = Decode(stringCodec, stringEncoded);
        Console.WriteLine($"  string: \"{stringValue}\" -> {BitConverter.ToString(stringEncoded)} (匹配: {stringValue == stringDecoded})");

        // bytes
        var bytesCodec = FieldCodec.Bytes(2);
        var bytesValue = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var bytesEncoded = Encode(bytesCodec, bytesValue);
        var bytesDecoded = Decode(bytesCodec, bytesEncoded);
        Console.WriteLine($"  bytes: [{string.Join(", ", bytesValue)}] -> {BitConverter.ToString(bytesEncoded)} (匹配: {bytesValue.SequenceEqual(bytesDecoded)})");

        // 空字符串
        var emptyString = "";
        var emptyStringEncoded = Encode(stringCodec, emptyString);
        var emptyStringDecoded = Decode(stringCodec, emptyStringEncoded);
        Console.WriteLine($"  空字符串: \"\" -> {BitConverter.ToString(emptyStringEncoded)} (匹配: {emptyString == emptyStringDecoded})");
    }

    /// <summary>
    /// 演示 Varint 编码原理
    /// </summary>
    static void DemonstrateVarintEncoding()
    {
        var values = new ulong[]
        {
            0,          // 0 字节（除了 tag）
            127,        // 0x7F - 单字节
            128,        // 0x80 0x01 - 双字节
            300,        // 0xAC 0x02 - 双字节
            16383,      // 三字节
            16384,      // 四字节
            1234567890123456789  // 多字节
        };

        foreach (var value in values)
        {
            var codec = FieldCodec.UInt64(1);
            var data = Encode(codec, value);
            var valueBytes = data.Skip(1).ToArray(); // 跳过 tag

            Console.WriteLine($"  值 {value,20} (0x{value:X}):");
            Console.WriteLine($"    -> {BitConverter.ToString(valueBytes)}");
            Console.WriteLine($"    -> {valueBytes.Length} 字节");
        }
    }

    /// <summary>
    /// 演示 ZigZag 编码
    /// </summary>
    static void DemonstrateZigZagEncoding()
    {
        var values = new long[] { 0, 1, -1, 2, -2, 127, -128, 128, -129 };

        foreach (var value in values)
        {
            var codec = FieldCodec.SInt64(1);
            var data = Encode(codec, (long)value);
            var valueBytes = data.Skip(1).ToArray();

            // 计算 ZigZag 编码值
            var zigzag = (ulong)((value >> 1) ^ -(value & 1));

            Console.WriteLine($"  {value,4} -> ZigZag: {zigzag,4} (0x{zigzag:X}):");
            Console.WriteLine($"    -> {BitConverter.ToString(valueBytes)} ({valueBytes.Length} 字节)");
        }
    }

    /// <summary>
    /// 辅助方法：只编码，返回字节数组
    /// </summary>
    static byte[] Encode<T>(FieldCodec<T> codec, T value)
    {
        var output = new CodedOutputStream();
        codec.Write(output, value);
        return output.ToByteArray();
    }

    /// <summary>
    /// 辅助方法：解码字节数组
    /// </summary>
    static T Decode<T>(FieldCodec<T> codec, byte[] data)
    {
        var input = new CodedInputStream(data);
        input.ReadTag(); // 跳过 tag
        return codec.Read(input);
    }
}
