using System;
using Protobuf.Core;

namespace Examples;

/// <summary>
/// 手动消息构建和编码示例（简化版）
/// 只展示编码功能，避免解析的复杂性
/// </summary>
class ManualMessageExample
{
    public static void Run(string[] args)
    {
        Console.WriteLine("=== 手动消息构建和编码示例 ===\n");

        // 示例 1: 手动构建简单的消息
        Console.WriteLine("示例 1: 手动构建 Person 消息");
        var personData = BuildSimplePerson();
        Console.WriteLine($"  编码后的数据: {BitConverter.ToString(personData)}");
        Console.WriteLine($"  数据长度: {personData.Length} 字节");

        // 示例 2: 编码包含嵌套消息的数据
        Console.WriteLine("\n示例 2: 编码复杂数据");
        var complexData = BuildComplexMessage();
        Console.WriteLine($"  编码后的数据: {BitConverter.ToString(complexData)}");
        Console.WriteLine($"  数据长度: {complexData.Length} 字节");

        // 示例 3: 编码包含未知字段的数据
        Console.WriteLine("\n示例 3: 字段独立性演示");
        DemonstrateFieldIndependence();

        // 示例 4: 解码简单数据（手动）
        Console.WriteLine("\n示例 4: 手动解码演示");
        ManualDecodeExample();

        Console.WriteLine("\n=== 示例完成 ===");
    }

    /// <summary>
    /// 手动构建一个简单的 Person 消息
    /// </summary>
    static byte[] BuildSimplePerson()
    {
        var output = new CodedOutputStream();

        // 字段 1: string name = 1;
        var nameCodec = FieldCodec.String(1);
        nameCodec.Write(output, "Alice");

        // 字段 2: int32 id = 2;
        var idCodec = FieldCodec.Int32(2);
        idCodec.Write(output, 12345);

        // 字段 3: string email = 3;
        var emailCodec = FieldCodec.String(3);
        emailCodec.Write(output, "alice@example.com");

        Console.WriteLine("  构建了以下字段:");
        Console.WriteLine("    name: \"Alice\"");
        Console.WriteLine("    id: 12345");
        Console.WriteLine("    email: \"alice@example.com\"");

        return output.ToByteArray();
    }

    /// <summary>
    /// 构建包含嵌套消息的复杂数据
    /// </summary>
    static byte[] BuildComplexMessage()
    {
        var output = new CodedOutputStream();

        // 字段 1: string name = 1;
        var nameCodec = FieldCodec.String(1);
        nameCodec.Write(output, "Bob");

        // 字段 2: int32 id = 2;
        var idCodec = FieldCodec.Int32(2);
        idCodec.Write(output, 67890);

        // 字段 3: repeated string phones = 3;
        var phonesCodec = FieldCodec.String(3);
        phonesCodec.Write(output, "555-1234;MOBILE");
        phonesCodec.Write(output, "555-5678;HOME");

        Console.WriteLine("  构建了以下字段:");
        Console.WriteLine("    name: \"Bob\"");
        Console.WriteLine("    id: 67890");
        Console.WriteLine("    phones: [\"555-1234;MOBILE\", \"555-5678;HOME\"] (repeated)");

        return output.ToByteArray();
    }

    /// <summary>
    /// 演示字段独立性
    /// </summary>
    static void DemonstrateFieldIndependence()
    {
        var output = new CodedOutputStream();

        // 只编码字段 1 和 3（跳过字段 2）
        var nameCodec = FieldCodec.String(1);
        nameCodec.Write(output, "Charlie");

        var emailCodec = FieldCodec.String(3);
        emailCodec.Write(output, "charlie@example.com");

        var data = output.ToByteArray();
        Console.WriteLine($"  只编码字段 1 和 3 的数据: {BitConverter.ToString(data)}");
        Console.WriteLine("  注意: 字段可以独立编码，无需按顺序");
    }

    /// <summary>
    /// 手动解码演示
    /// </summary>
    static void ManualDecodeExample()
    {
        Console.WriteLine("  手动解码 Person 消息:");

        // 手动编码
        var output = new CodedOutputStream();
        output.WriteTag(WireFormat.MakeTag(1, WireFormat.LengthDelimitedType));
        output.WriteString("Test");

        output.WriteTag(WireFormat.MakeTag(2, WireFormat.VarintType));
        output.WriteInt32(42);

        var data = output.ToByteArray();
        Console.WriteLine($"    编码数据: {BitConverter.ToString(data)}");

        // 手动解码
        var input = new CodedInputStream(data);

        // 读取字段 1
        var tag1 = input.ReadTag();
        var fieldNumber1 = WireFormat.GetTagFieldNumber(tag1);
        var wireType1 = WireFormat.GetTagWireType(tag1);
        Console.WriteLine($"    读取 tag: 0x{tag1:X} (field {fieldNumber1}, type {wireType1})");

        if (fieldNumber1 == 1 && wireType1 == WireFormat.LengthDelimitedType)
        {
            var name = input.ReadString();
            Console.WriteLine($"    解码得到 name: \"{name}\"");
        }

        // 读取字段 2
        var tag2 = input.ReadTag();
        var fieldNumber2 = WireFormat.GetTagFieldNumber(tag2);
        var wireType2 = WireFormat.GetTagWireType(tag2);
        Console.WriteLine($"    读取 tag: 0x{tag2:X} (field {fieldNumber2}, type {wireType2})");

        if (fieldNumber2 == 2 && wireType2 == WireFormat.VarintType)
        {
            var id = input.ReadInt32();
            Console.WriteLine($"    解码得到 id: {id}");
        }

        Console.WriteLine("  ✅ 手动解码成功！");
    }
}
