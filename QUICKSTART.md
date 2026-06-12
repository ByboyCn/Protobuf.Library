# 快速开始 - Protobuf Library

## 🎯 一分钟上手

### 安装和配置

```bash
# 1. 添加项目引用
dotnet add reference ../Protobuf.Core/Protobuf.Core.csproj
dotnet add reference ../Protobuf.Generator/Protobuf.Generator.csproj
dotnet add reference ../Protobuf.Json/Protobuf.Json.csproj
dotnet add reference ../Protobuf.Reflection/Protobuf.Reflection.csproj
```

### 定义 .proto 文件

创建 `Protos/Person.proto`:

```protobuf
syntax = "proto3";

package Example;

message Person {
  string name = 1;
  int32 id = 2;
  string email = 3;
  
  repeated string phone_numbers = 4;
  
  oneof address {
    string home_address = 5;
    string work_address = 6;
  }
  
  map<string, string> metadata = 7;
}

enum PhoneType {
  MOBILE = 0;
  HOME = 1;
  WORK = 2;
}
```

### 配置项目文件

```xml
<ItemGroup>
  <ProjectReference Include="Protobuf.Core\Protobuf.Core.csproj" />
  <ProjectReference Include="Protobuf.Generator\Protobuf.Generator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
  <ProjectReference Include="Protobuf.Json\Protobuf.Json.csproj" />
  <ProjectReference Include="Protobuf.Reflection\Protobuf.Reflection.csproj" />
  <ProtoFile Include="Protos\**\*.proto" />
</ItemGroup>
```

### 构建项目

```bash
dotnet build
```

✅ Source Generator 会自动生成 `Person.g.cs` 文件！

### 使用生成的代码

```csharp
using Example;

// 1. 创建消息
var person = new Person();

// 2. 设置字段
person.SetName("Alice");
person.SetId(12345);
person.SetEmail("alice@example.com");

// 3. 使用 Repeated 字段
person.AddPhoneNumbers("555-1234");
person.AddPhoneNumbers("555-5678");

// 4. 使用 Oneof
person.SetHomeAddress("123 Home St");

// 5. 使用 Map
person.AddMetadata("department", "Engineering");
person.AddMetadata("title", "Developer");

// 6. 序列化
byte[] data = person.Serialize();
Console.WriteLine($"Serialized {data.Length} bytes");

// 7. 反序列化
var person2 = Person.Parser.ParseFrom(data);
Console.WriteLine($"Name: {person2.Name}");
Console.WriteLine($"ID: {person2.Id}");

// 8. JSON 支持
string json = person.ToJson();
Console.WriteLine($"JSON: {json}");

// 9. 反射 API
var descriptor = Person.Descriptor;
Console.WriteLine($"Message: {descriptor.Name}");
Console.WriteLine($"Fields: {descriptor.Fields.Length}");
```

## 🚀 常用操作快速参考

### 基础字段

```csharp
// 设置
person.SetName("Alice");
person.SetId(12345);

// 获取
string name = person.Name;
int id = person.Id;

// 检查
if (person.HasName) { }

// 清除
person.ClearName();
```

### Repeated 字段

```csharp
// 添加
person.AddPhoneNumbers("555-1234");

// 获取
IReadOnlyList<string> phones = person.PhoneNumbers;

// 清除
person.ClearPhoneNumbers();
```

### Oneof 字段

```csharp
// 设置
person.SetHomeAddress("123 Home St");

// 检查
if (person.HasHomeAddress) { }

// 获取 Case
switch (person.AddressCase) {
    case Person.AddressCase.HomeAddress:
        Console.WriteLine($"Home: {person.HomeAddress}");
        break;
    case Person.AddressCase.WorkAddress:
        Console.WriteLine($"Work: {person.WorkAddress}");
        break;
}

// 清除
person.ClearAddress();
```

### Map 字段

```csharp
// 添加
person.AddMetadata("key", "value");

// 获取
IReadOnlyDictionary<string, string> metadata = person.Metadata;

// 清除
person.ClearMetadata();
```

### 序列化/反序列化

```csharp
// 序列化
byte[] data = person.Serialize();

// 反序列化
var person2 = Person.Parser.ParseFrom(data);

// JSON
string json = person.ToJson();
```

## 🎓 学习路径

### 1. 新手（5分钟）
- 阅读 [README.md](README.md) 了解项目概览
- 按照上面的步骤创建第一个 .proto 文件
- 运行示例代码

### 2. 进阶（15分钟）
- 阅读 [USAGE.md](USAGE.md) 了解完整 API
- 学习嵌套消息和高级类型
- 了解序列化和反序列化的细节

### 3. 高级（30分钟）
- 阅读 [reflection-api-guide.md](docs/reflection-api-guide.md) 了解反射 API
- 学习 [build-system-guide.md](docs/build-system-guide.md) 了解构建系统
- 运行 `examples/Examples` 中的示例

## 💡 核心特性

### ✅ AOT 友好
完全支持 NativeAOT 编译，零反射依赖：

```csharp
// AOT 安全的反射 API
var descriptor = Person.Descriptor;
var accessors = Person.GetFieldAccessors();
```

### ✅ 自动代码生成
Source Generator 自动生成简洁的代码：

```csharp
public partial class Person : IMessage<Person>
{
    private string _name = "";
    public bool HasName => _name != "";
    public string Name => _name;
    public void SetName(string value) { _name = value ?? ""; }
    public void ClearName() { _name = ""; }
    // ... 更多生成的代码
}
```

### ✅ 类型安全
编译时类型检查，IDE 完整支持：

```csharp
person.SetName("Alice");         // ✅ 类型安全
person.SetName(123);            // ❌ 编译错误
Console.WriteLine(person.Name); // ✅ 智能提示
```

### ✅ 完整 Protobuf3 支持
- ✅ 所有基础类型
- ✅ Repeated 字段
- ✅ Oneof 字段
- ✅ Map 字段
- ✅ 嵌套消息
- ✅ 枚举
- ✅ JSON 序列化
- ✅ 反射 API

## 📦 项目结构

```
Protobuf.Library/
├── src/
│   ├── Protobuf.Core/          # 核心运行时
│   ├── Protobuf.Parser/        # 解析器
│   ├── Protobuf.Generator/     # Source Generator
│   ├── Protobuf.Json/           # JSON 支持
│   └── Protobuf.Reflection/     # 反射 API
├── docs/                       # 详细文档
│   ├── reflection-api-guide.md
│   ├── build-system-guide.md
│   └── quick-start-guide.md
├── examples/                   # 示例代码
│   └── Examples/
└── tests/                      # 测试
    └── Protobuf.Tests/
```

## 🔧 运行示例

```bash
cd examples/Examples

# 解析器示例
dotnet run -- parser

# 编解码器示例
dotnet run -- codec

# 手动消息处理示例
dotnet run -- manual

# 真实应用场景示例
dotnet run -- realworld
```

## ⚡ 性能特点

- **零运行时开销**：编译时生成代码
- **AOT 优化**：完全支持 NativeAOT
- **内存高效**：智能缓冲区管理
- **类型安全**：编译时检查，避免运行时错误

## 🆚 与其他库对比

| 特性 | Google.Protobuf | protobuf-net | 我们 |
|------|----------------|--------------|------|
| protoc 工具 | ✅ 需要 | ❌ 不需要 | ❌ 不需要 |
| AOT 支持 | ❌ 有限 | ❌ 有限 | ✅ 完全 |
| Source Generator | ❌ 否 | ❌ 否 | ✅ 是 |
| 反射依赖 | ✅ 是 | ✅ 是 | ❌ 否 |
| 代码简洁性 | ❌ 冗长 | ✅ 简洁 | ✅ 简洁 |

## 🎯 下一步

1. **创建你的第一个 .proto 文件**
2. **配置项目并构建**
3. **使用生成的代码**
4. **探索高级功能（反射 API、JSON 等）**

## 💻 完整示例

```csharp
using System;
using Example;

class Program
{
    static void Main()
    {
        // 创建消息
        var person = new Person();
        
        // 设置字段
        person.SetName("Alice");
        person.SetId(12345);
        person.SetEmail("alice@example.com");
        
        // 添加电话号码
        person.AddPhoneNumbers("555-1234");
        person.AddPhoneNumbers("555-5678");
        
        // 设置地址
        person.SetHomeAddress("123 Home St");
        
        // 添加元数据
        person.AddMetadata("department", "Engineering");
        
        // 序列化
        byte[] data = person.Serialize();
        Console.WriteLine($"✅ Serialized {data.Length} bytes");
        
        // 反序列化
        var person2 = Person.Parser.ParseFrom(data);
        Console.WriteLine($"✅ Name: {person2.Name}");
        Console.WriteLine($"✅ ID: {person2.Id}");
        Console.WriteLine($"✅ Phones: {person2.PhoneNumbers.Count}");
        
        // JSON
        string json = person2.ToJson();
        Console.WriteLine($"✅ JSON: {json}");
        
        // 反射
        var descriptor = Person.Descriptor;
        Console.WriteLine($"✅ Message: {descriptor.Name}");
        Console.WriteLine($"✅ Fields: {descriptor.Fields.Length}");
    }
}
```

## 📚 更多文档

- [README.md](README.md) - 项目概览
- [USAGE.md](USAGE.md) - 完整使用指南
- [reflection-api-guide.md](docs/reflection-api-guide.md) - 反射 API 指南
- [build-system-guide.md](docs/build-system-guide.md) - 构建系统指南

## ❓ 遇到问题？

1. 检查 [USAGE.md](USAGE.md) 中的详细说明
2. 查看 `examples/Examples/` 中的示例代码
3. 运行 `dotnet build` 查看编译错误信息
4. 确保所有项目引用正确

---

**就这么简单！** 🎉 开始使用 Protobuf.Library 吧！
