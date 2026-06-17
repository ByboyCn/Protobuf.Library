using System;
using Protobuf.Core; // 引入核心库

// 注意：生成的代码会在 Test 命名空间中
// 它们会自动可用，因为源代码生成器会生成这些类

namespace ExampleProject;

/// <summary>
/// 示例程序：展示如何使用源代码生成器生成的类
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Protobuf Library - 源代码生成器示例");
        Console.WriteLine("=====================================");
        Console.WriteLine();

        try
        {
            // 测试 simple.proto 生成的代码
            var test = new Test.SimpleTest();
            test.SetId(123);
            test.SetName("测试消息");

            Console.WriteLine("✅ 创建 SimpleTest 消息成功！");
            Console.WriteLine($"   ID: {test.Id}");
            Console.WriteLine($"   Name: {test.Name}");
            Console.WriteLine($"   HasId: {test.HasId}");
            Console.WriteLine($"   HasName: {test.HasName}");
            Console.WriteLine();

            // 序列化测试
            byte[] data;
            using (var ms = new System.IO.MemoryStream())
            {
                var output = new Protobuf.Core.CodedOutputStream(ms);
                test.WriteTo(output);
                output.Flush();
                data = ms.ToArray();

                Console.WriteLine($"✅ 序列化成功！数据长度: {data.Length} 字节");
                Console.WriteLine($"   数据: {BitConverter.ToString(data)}");
                Console.WriteLine();
            }

            // 反序列化测试
            var test2 = new Test.SimpleTest();
            var input = new Protobuf.Core.CodedInputStream(new System.IO.MemoryStream(data));
            test2.MergeFrom(input);
            Console.WriteLine("✅ 反序列化成功！");
            Console.WriteLine($"   ID: {test2.Id}");
            Console.WriteLine($"   Name: {test2.Name}");
            Console.WriteLine();

            Console.WriteLine("=====================================");
            Console.WriteLine("🎉 所有测试通过！源代码生成器工作正常！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 测试失败: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine($"堆栈跟踪:");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
