using System;
// using Example;  // 需要代码生成器生成此命名空间

namespace Examples;

// 这是一个使用生成的 Protobuf 代码的示例
// 注意：这个示例需要引用 Protobuf.Generator 项目才能自动生成代码
// 当前此示例暂时注释，等待代码生成器实现

class UsageExample
{
    public static void Run(string[] args)
    {
        Console.WriteLine("Protobuf Library Usage Example");
        Console.WriteLine("==============================");
        Console.WriteLine();
        Console.WriteLine("注意：此示例需要代码生成器支持，当前暂未实现。");
        Console.WriteLine("请参考其他可运行的示例：");
        Console.WriteLine("  - ProtoParserExample: 解析 .proto 文件");
        Console.WriteLine("  - CodecExample: 编解码器使用");
        Console.WriteLine("  - ManualMessageExample: 手动消息处理");
        Console.WriteLine("  - RealWorldExample: 真实场景示例");

        /* 取消注释以下代码，等待代码生成器实现后：

        // 创建一个 Person 消息
        var person = new Person();

        // 设置基本字段
        person.SetName("Alice");
        person.SetId(12345);
        person.SetEmail("alice@example.com");

        // 检查字段是否有值
        Console.WriteLine($"Has Name: {person.HasName}"); // true
        Console.WriteLine($"Has Email: {person.HasEmail}"); // true
        Console.WriteLine($"Has Phones: {person.HasPhones}"); // false（还没有添加电话）

        // 添加电话号码
        // person.AddPhones(new Person.PhoneNumber { ... }); // 需要生成的代码支持

        // 使用 oneof
        person.SetHomeAddress("123 Home St, Home City");
        Console.WriteLine($"Has Home Address: {person.HasHomeAddress}"); // true
        Console.WriteLine($"Home Address: {person.HomeAddress}"); // "123 Home St, Home City"
        Console.WriteLine($"Address Case: {person.AddressCase}"); // HomeAddress

        // 切换到工作地址（会清除家庭地址）
        person.SetWorkAddress("456 Work Ave, Work City");
        Console.WriteLine($"Has Home Address: {person.HasHomeAddress}"); // false
        Console.WriteLine($"Has Work Address: {person.HasWorkAddress}"); // true
        Console.WriteLine($"Address Case: {person.AddressCase}"); // WorkAddress

        // 使用 map
        person.AddMetadata("department", "Engineering");
        person.AddMetadata("title", "Senior Developer");
        Console.WriteLine($"Has Metadata: {person.HasMetadata}"); // true
        Console.WriteLine($"Metadata Count: {person.Metadata.Count}"); // 2

        // 序列化到二进制
        // var bytes = person.Serialize(); // 需要实现序列化

        // 从二进制反序列化
        // var person2 = Person.Parser.ParseFrom(bytes);

        // 序列化到 JSON
        // var json = person.ToJson(); // 需要实现 JSON 序列化

        Console.WriteLine();
        Console.WriteLine("Example completed successfully!");
        */
    }
}
