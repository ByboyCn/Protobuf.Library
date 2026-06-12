# Protobuf Library - 轻量级 Protobuf3 库

一个完全独立的、支持 AOT 的 Protobuf3 库，无需依赖 Google.Protobuf 或 protobuf-net。

## 特性

- ✅ **零依赖**：不依赖任何外部 Protobuf 库
- ✅ **AOT 友好**：完全支持 NativeAOT 编译
- ✅ **Protobuf3 支持**：完整的 protobuf3 标准支持
- ✅ **Source Generator**：编译时自动生成代码
- ✅ **CSProj 集成**：支持通配符匹配，`**/*.proto`
- ✅ **hasXxx() 方法**：检查字段是否有值
- ✅ **完整类型支持**：基础类型、repeated、oneof、map
- ✅ **构建系统集成**：无缝集成到 MSBuild

## 支持的类型

### 基础类型

- `double`, `float`
- `int32`, `int64`, `uint32`, `uint64`
- `sint32`, `sint64`（ZigZag 编码）
- `fixed32`, `fixed64`, `sfixed32`, `sfixed64`
- `bool`, `string`, `bytes`

### 高级类型

- `repeated` - 列表字段
- `oneof` - 互斥字段
- `map<K,V>` - 字典字段
- 嵌套消息
- 枚举

## 快速开始

### 1. 定义 .proto 文件

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
```

### 2. 配置项目文件

```xml
<ItemGroup>
  <!-- 引用核心库 -->
  <ProjectReference Include="Protobuf.Core\Protobuf.Core.csproj" />

  <!-- 引用源代码生成器 -->
  <ProjectReference Include="Protobuf.Generator\Protobuf.Generator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />

  <!-- 包含 proto 文件（支持通配符） -->
  <ProtoFile Include="Protos\**\*.proto" />
</ItemGroup>
```

### 3. 使用生成的代码

```csharp
var person = new Person();

// 设置字段
person.SetName("Alice");
person.SetId(12345);

// 检查字段是否有值
if (person.HasName)
{
    Console.WriteLine($"Name: {person.Name}");
}

// 使用 repeated 字段
person.AddPhoneNumbers("555-1234");
person.AddPhoneNumbers("555-5678");

// 使用 oneof
person.SetHomeAddress("123 Home St");

// 切换到另一个 oneof 选项（会自动清除前一个）
person.SetWorkAddress("456 Work Ave");

// 使用 map
person.AddMetadata("department", "Engineering");
```

## API 参考

### hasXxx() 方法

每个字段都有一个对应的 `HasXxx()` 方法，用于检查字段是否有值：

```csharp
person.HasName      // bool
person.HasId        // bool
person.HasEmail     // bool
person.HasPhoneNumbers  // bool
```

### Setter 方法

使用 `SetXxx()` 方法设置字段值：

```csharp
person.SetName("Alice");
person.SetId(12345);
person.SetEmail("alice@example.com");
```

### Getter 属性

使用 `Xxx` 属性获取字段值：

```csharp
string name = person.Name;
int id = person.Id;
```

### Repeated 字段

对于 `repeated` 字段，使用 `AddXxx()` 方法：

```csharp
person.AddPhoneNumbers("555-1234");

// 获取只读列表
IReadOnlyList<string> phones = person.PhoneNumbers;
```

### Oneof 字段

```csharp
person.SetHomeAddress("123 Home St");

// 检查哪个 oneof 字段被设置
if (person.HasHomeAddress)
{
    Console.WriteLine($"Home: {person.HomeAddress}");
}

// 获取 oneof case
Person.AddressCase addrCase = person.AddressCase;

// 清除 oneof
person.ClearAddress();
```

### Map 字段

```csharp
person.AddMetadata("key", "value");

// 获取只读字典
IReadOnlyDictionary<string, string> metadata = person.Metadata;
```

### 序列化

```csharp
// 序列化到字节数组
byte[] data = person.Serialize();

// 从字节数组反序列化
Person person2 = Person.Parser.ParseFrom(data);
```

### JSON 支持

```csharp
// 序列化到 JSON（PascalCase）
string json = person.ToJson();
// {"Name":"Alice","Id":12345,...}

// 从 JSON 反序列化
Person person3 = Person.Parser.ParseJson(json);
```

## 项目结构

```
Protobuf.Library/
├── src/
│   ├── Protobuf.Core/          # 核心运行时库
│   ├── Protobuf.Parser/        # .proto 文件解析器
│   ├── Protobuf.Generator/     # Source Generator
│   ├── Protobuf.Json/           # JSON 支持
│   └── Protobuf.Reflection/     # 反射 API
├── tests/
│   └── Protobuf.Tests/
└── examples/
    └── Examples/
```

## 🚀 如何使用

### 快速开始

查看 **[QUICKSTART.md](QUICKSTART.md)** 了解一分钟上手指南

### 详细指南

查看 **[USAGE.md](USAGE.md)** 获取完整使用文档

### 运行示例

```bash
cd examples/Examples
dotnet run -- parser    # Proto 文件解析示例
dotnet run -- codec     # 编解码器使用示例
dotnet run -- manual    # 手动消息处理示例
dotnet run -- realworld # 真实应用场景示例
```

### 当前使用方式

**阶段一：手动方式（当前可用）**

- ✅ 解析 .proto 文件了解结构
- ✅ 使用运行时 API 手动编码/解码
- ⏳ 等待 Source Generator

**阶段二：自动生成（即将推出）**

- ⏳ 定义 .proto 文件
- ⏳ Source Generator 自动生成 C# 代码
- ⏳ 开箱即用，无需手写代码

## 与 Google.Protobuf 和 protobuf-net 的区别

### Google.Protobuf

- ❌ 需要额外的 protoc 工具生成代码
- ❌ 生成的代码冗长且难以阅读
- ❌ 大型依赖

### protobuf-net

- ❌ 主要依赖运行时反射
- ❌ AOT 不支持
- ❌ 配置复杂

### 本库

- ✅ Source Generator 自动生成代码
- ✅ 生成的代码简洁易读
- ✅ 完全支持 AOT
- ✅ 零外部依赖
- ✅ 单一库，开箱即用

## 开发状态

- [X]  核心运行时（Wire Format、编解码器）
- [X]  Proto 文件解析器
- [X]  代码生成器（Source Generator）
- [X]  基础类型支持
- [X]  Repeated 字段支持
- [X]  OneOf 字段支持
- [X]  Map 字段支持（完整序列化/反序列化）
- [X]  嵌套消息类型支持
- [X]  JSON 序列化/反序列化（PascalCase）
- [X]  AOT 友好的反射 API
- [X]  所有项目成功构建
- [X]  完整的序列化/反序列化实现
- [X]  单元测试（54个测试全部通过）
- [X]  文档完善

## 许可证

待定

## 贡献

欢迎贡献！
