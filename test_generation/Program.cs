using System;
using System.Collections.Generic;
using System.IO;
using Protobuf.CodeGeneration;
using Protobuf.Parser;

// 简单的代码生成测试
var generator = new ProtoCodeGenerator();
var protoContent = @"
syntax = ""proto3"";
package Test;

message SimpleTest {
  int32 id = 1;
  string name = 2;
}
";

var protoFiles = new Dictionary<string, string>
{
    { "test.proto", protoContent }
};

var generated = generator.GenerateCode(protoFiles);

Console.WriteLine($"Generated {generated.Count} files:");
foreach (var file in generated)
{
    Console.WriteLine($"\n=== {file.Key} ===");
    Console.WriteLine(file.Value.Substring(0, Math.Min(500, file.Value.Length)));
    if (file.Value.Length > 500)
    {
        Console.WriteLine("...(truncated)");
    }
}

Console.WriteLine($"\n\nDiagnostics:");
foreach (var diagnostic in generator.Diagnostics)
{
    Console.WriteLine($"  [{diagnostic.Level}] {diagnostic.FilePath}: {diagnostic.Message}");
}
