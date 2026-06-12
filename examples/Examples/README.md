# Protobuf Library 示例

这个目录包含了 Protobuf3 库的使用示例，帮助您快速了解如何使用库的各项功能。

## 示例列表

### 1. ProtoParserExample.cs
演示如何解析 `.proto` 文件并访问其内容。

**功能展示：**
- 解析简单的消息定义
- 解析包含 repeated、oneof、map 的复杂消息
- 从文件读取并解析 `.proto` 文件
- 展示解析后的 AST 结构

**运行方式：**
```bash
cd examples/Examples
dotnet run --project Examples.csproj ProtoParserExample.cs
```

**示例输出：**
```
=== Protobuf .proto 文件解析示例 ===

示例 1: 解析简单的消息定义
  Syntax: proto3
  消息数量: 1
  第一个消息: Person
  字段数量: 3
    - String name = 1
    - Int32 id = 2
    - String email = 3

示例 2: 解析复杂的消息定义
  Package: Example
  顶层消息数量: 1
  顶层枚举数量: 1
  ...
```

### 2. CodecExample.cs
演示如何使用各种 Protobuf 编解码器。

**功能展示：**
- 整数类型编解码（int32, int64, uint32, uint64, bool）
- 浮点类型编解码（float, double）
- 字符串和字节编解码
- Varint 编码原理演示
- ZigZag 编码演示（用于有符号整数）

**运行方式：**
```bash
cd examples/Examples
dotnet run --project Examples.csproj CodecExample.cs
```

**示例输出：**
```
=== Protobuf Codec 使用示例 ===

示例 1: 整数类型编解码
  int32: 42 -> 08-2A -> 42
  int64: 1234567890123456789 -> 08-95-82-A6-EF-C7-9E-84-91-11 (多字节 varint)
  uint32: 4294967295 -> 18-FF-FF-FF-FF-0F (最大值)
  bool: true -> 20-01
  bool: false -> 20-00

示例 2: 浮点类型编解码
  float: 3.14159 -> 25-DB-0F-49-40 (4 字节)
  double: 3.14159265358979 -> 31-18-2D-44-54-FB-21-09-40 (8 字节)
  ...
```

### 3. UsageExample.cs
展示完整的消息使用流程（**需要 Source Generator 支持**）。

**注意：** 此示例需要先实现 Source Generator 才能运行，因为它依赖于从 `.proto` 文件生成的 C# 代码。

**功能展示：**
- 创建消息实例
- 设置字段值
- 使用 has 方法检查字段是否有值
- 操作 repeated 字段
- 使用 oneof 字段
- 使用 map 字段
- 消息序列化/反序列化
- JSON 转换

## 依赖关系

这些示例依赖于以下项目：
- `Protobuf.Core` - 核心运行时库
- `Protobuf.Parser` - Proto 文件解析器

## 运行要求

- .NET 8.0 或更高版本
- 已还原项目依赖：`dotnet restore`

## 学习路径

1. **初学者**：先运行 `CodecExample.cs`，了解 Protobuf 的编码原理
2. **中级**：运行 `ProtoParserExample.cs`，学习如何解析 `.proto` 文件
3. **高级**：等待 Source Generator 实现后，运行 `UsageExample.cs`，学习完整的使用流程

## 常见问题

### Q: 如何运行这些示例？
```bash
# 在项目根目录
dotnet restore

# 运行特定示例
dotnet run --project examples/Examples/Examples.csproj ProtoParserExample.cs
dotnet run --project examples/Examples/Examples.csproj CodecExample.cs
```

### Q: 为什么 UsageExample.cs 不能运行？
该示例需要从 `.proto` 文件生成 C# 代码的功能，这需要 Source Generator 支持。该功能正在开发中。

### Q: 如何修改示例？
您可以自由修改这些示例来实验不同的功能。修改后直接运行即可：
```bash
dotnet run --project examples/Examples/Examples.csproj YourModifiedExample.cs
```

## 下一步

- 查看 `person.proto` 了解示例消息定义
- 阅读源代码了解实现细节
- 等待 Source Generator 实现后体验完整的消息功能

## 相关资源

- [Protobuf 官方文档](https://developers.google.com/protocol-buffers)
- [Protobuf3 语言指南](https://developers.google.com/protocol-buffers/docs/proto3)
- [项目 README](../../README.md)
