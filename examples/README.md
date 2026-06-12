# Protobuf.Library Examples

欢迎来到 Protobuf.Library 示例目录！这些示例展示了如何使用轻量级 Protobuf3 库的各种功能。

## 📚 示例导航

### [Examples/](Examples/) - 完整示例集合

包含以下可运行的示例：

#### 1. **[ProtoParserExample.cs](Examples/ProtoParserExample.cs)** - .proto 文件解析
学习如何解析和使用 `.proto` 文件定义
- 解析简单和复杂的消息定义
- 访问 AST（抽象语法树）
- 从文件读取 proto 定义
- 提取消息、字段、枚举等信息

#### 2. **[CodecExample.cs](Examples/CodecExample.cs)** - 编解码器使用
深入了解 Protobuf 的二进制编码
- 所有基础类型的编解码
- Varint 编码原理
- ZigZag 编码（有符号整数）
- 字符串和字节处理
- 浮点数编码

#### 3. **[ManualMessageExample.cs](Examples/ManualMessageExample.cs)** - 手动消息处理
在没有 Source Generator 的情况下手动处理消息
- 手动构建消息
- 字段编码和解析
- 嵌套消息处理
- 未知字段跳过

#### 4. **[RealWorldExample.cs](Examples/RealWorldExample.cs)** - 真实场景
实际应用场景演示
- 配置文件序列化
- 网络协议数据包
- 数据库记录存储
- 性能对比

#### 5. **[UsageExample.cs](Examples/UsageExample.cs)** - 完整使用流程
展示完整的消息操作（需要 Source Generator）
- 消息创建和修改
- has 方法使用
- oneof 和 map 操作
- 序列化/反序列化
- JSON 转换

## 🚀 快速开始

### 前提条件
- .NET 8.0 或更高版本
- 已克隆项目仓库

### 运行示例

```bash
# 在项目根目录
cd Protobuf.Library

# 还原依赖
dotnet restore

# 运行任何示例
dotnet run --project examples/Examples/Examples.csproj ProtoParserExample.cs
dotnet run --project examples/Examples/Examples.csproj CodecExample.cs
dotnet run --project examples/Examples/Examples.csproj ManualMessageExample.cs
dotnet run --project examples/Examples/Examples.csproj RealWorldExample.cs
```

## 📖 学习路径

### 初学者路径
1. **先了解编码原理** → 运行 `CodecExample.cs`
2. **学习解析 proto 文件** → 运行 `ProtoParserExample.cs`
3. **实际应用场景** → 运行 `RealWorldExample.cs`

### 高级路径
1. **手动消息处理** → 运行 `ManualMessageExample.cs`
2. **等待 Source Generator** → 查看 `UsageExample.cs`（待实现）

## 🔧 示例文件说明

### person.proto
示例消息定义文件，包含：
- 基础字段类型
- Repeated 字段
- Oneof 字段
- Map 字段
- 嵌套消息和枚举

### Examples.csproj
示例项目的配置文件，引用了必要的核心项目。

## 💡 提示

- 所有示例都是独立的，可以单独运行
- 示例代码包含详细注释，解释每一步操作
- 鼓励修改示例代码进行实验
- 输出展示了编码/解码的详细过程

## 🎯 关键概念

### Varint 编码
Protobuf 使用可变长度整数编码，小数字占用更少空间：
- 数字 1-127：1 字节
- 数字 128-16383：2 字节
- 大数字：最多 10 字节

### Wire Format
每个字段都有 tag，包含字段编号和类型：
- Tag = (field_number << 3) | wire_type
- 不同类型使用不同的 wire_type

### 字段编号
- 1-15：1 字节编码（推荐用于常用字段）
- 16-2047：2 字节编码

## 📦 相关资源

- [主项目 README](../README.md)
- [核心库文档](../src/Protobuf.Core/README.md)
- [解析器文档](../src/Protobuf.Parser/README.md)
- [Protobuf 官方文档](https://developers.google.com/protocol-buffers)

## 🤝 贡献

欢迎贡献更多示例！如果您有好的用例，请：
1. Fork 项目
2. 创建新示例文件
3. 添加文档说明
4. 提交 Pull Request

## 📝 许可证

与主项目相同，参见 [LICENSE](../LICENSE) 文件。
