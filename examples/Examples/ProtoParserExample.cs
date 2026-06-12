using System;
using Protobuf.Parser;
using Protobuf.Core;

namespace Examples;

/// <summary>
/// 解析 .proto 文件的示例
/// </summary>
class ProtoParserExample
{
    public static void Run(string[] args)
    {
        Console.WriteLine("=== Protobuf .proto 文件解析示例 ===\n");

        // 示例 1: 简单消息解析
        Console.WriteLine("示例 1: 解析简单的消息定义");
        var simpleProto = @"
syntax = ""proto3"";

message Person {
  string name = 1;
  int32 id = 2;
  string email = 3;
}
";

        var lexer = new Lexer(simpleProto);
        var tokens = lexer.Tokenize();
        var parser = new Protobuf.Parser.Parser(tokens);
        var protoFile = parser.Parse();

        Console.WriteLine($"  Syntax: {protoFile.Syntax}");
        Console.WriteLine($"  消息数量: {protoFile.Messages.Count}");
        Console.WriteLine($"  第一个消息: {protoFile.Messages[0].Name}");
        Console.WriteLine($"  字段数量: {protoFile.Messages[0].Fields.Count}");

        foreach (var field in protoFile.Messages[0].Fields)
        {
            Console.WriteLine($"    - {field.Type} {field.Name} = {field.FieldNumber}");
        }

        // 示例 2: 复杂消息解析（包含 repeated、oneof、map）
        Console.WriteLine("\n示例 2: 解析复杂的消息定义");
        var complexProto = @"
syntax = ""proto3"";

package Example;

message Person {
  string name = 1;
  int32 id = 2;
  repeated string emails = 3;

  enum PhoneType {
    MOBILE = 0;
    HOME = 1;
    WORK = 2;
  }

  message PhoneNumber {
    string number = 1;
    PhoneType type = 2;
  }

  repeated PhoneNumber phones = 4;

  oneof address {
    string home_address = 5;
    string work_address = 6;
  }

  map<string, string> metadata = 7;
}

enum Gender {
  MALE = 0;
  FEMALE = 1;
  OTHER = 2;
}
";

        var lexer2 = new Lexer(complexProto);
        var tokens2 = lexer2.Tokenize();
        var parser2 = new Protobuf.Parser.Parser(tokens2);
        var protoFile2 = parser2.Parse();

        Console.WriteLine($"  Package: {protoFile2.Package}");
        Console.WriteLine($"  顶层消息数量: {protoFile2.Messages.Count}");
        Console.WriteLine($"  顶层枚举数量: {protoFile2.Enums.Count}");

        var person = protoFile2.Messages[0];
        Console.WriteLine($"\n  Person 消息详情:");
        Console.WriteLine($"    普通字段: {person.Fields.Count(f => f.Label == FieldLabel.None && !f.IsOneOf)}");
        Console.WriteLine($"    Repeated 字段: {person.Fields.Count(f => f.Label == FieldLabel.Repeated)}");
        Console.WriteLine($"    OneOf 组数: {person.OneOfs.Count}");
        Console.WriteLine($"    Map 字段: {person.Fields.Count(f => f.IsMap)}");
        Console.WriteLine($"    嵌套消息: {person.NestedMessages.Count}");
        Console.WriteLine($"    嵌套枚举: {person.Enums.Count}");

        // 示例 3: 从文件解析
        Console.WriteLine("\n示例 3: 从文件解析 .proto");
        try
        {
            var protoPath = "person.proto";
            if (File.Exists(protoPath))
            {
                var protoContent = File.ReadAllText(protoPath);
                var lexer3 = new Lexer(protoContent);
                var tokens3 = lexer3.Tokenize();
                var parser3 = new Protobuf.Parser.Parser(tokens3);
                var protoFile3 = parser3.Parse();

                Console.WriteLine($"  成功解析 {protoPath}");
                Console.WriteLine($"  包: {protoFile3.Package}");
                Console.WriteLine($"  消息: {string.Join(", ", protoFile3.Messages.Select(m => m.Name))}");
                Console.WriteLine($"  枚举: {string.Join(", ", protoFile3.Enums.Select(e => e.Name))}");
            }
            else
            {
                Console.WriteLine($"  文件 {protoPath} 不存在，跳过此示例");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  解析文件时出错: {ex.Message}");
        }

        // 示例 4: 编码和解码基础类型
        Console.WriteLine("\n示例 4: Varint 编码和解码");
        var value = 1234567890123456789L;
        Console.WriteLine($"  原始值: {value} (0x{value:X})");

        // 编码
        var output = new CodedOutputStream();
        output.WriteInt64(value);
        var encoded = output.ToByteArray();
        Console.WriteLine($"  编码后: {BitConverter.ToString(encoded)}");
        Console.WriteLine($"  字节长度: {encoded.Length}");

        // 解码
        var input = new CodedInputStream(encoded);
        var decoded = input.ReadInt64();
        Console.WriteLine($"  解码后: {decoded} (0x{decoded:X})");
        Console.WriteLine($"  匹配: {value == decoded}");

        Console.WriteLine("\n=== 示例完成 ===");
    }
}
