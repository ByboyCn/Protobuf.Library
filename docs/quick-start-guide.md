# 🚀 Protobuf Library 5分钟快速入门

## 步骤 1：创建项目

```bash
# 创建新项目
dotnet new console -n MyProtobufApp
cd MyProtobufApp
```

## 步骤 2：添加引用

### 方式 A：使用本地路径（开发时）

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\Protobuf.Library\src\Protobuf.Core\Protobuf.Core.csproj" />
    <ProjectReference Include="..\Protobuf.Library\src\Protobuf.Generator\Protobuf.Generator.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
  </ItemGroup>
</Project>
```

### 方式 B：使用 NuGet 包（生产环境）

```bash
dotnet add package Protobuf.Core
dotnet add package Protobuf.Generator
```

## 步骤 3：创建 Proto 文件

创建 `Protos/person.proto`:

```protobuf
syntax = "proto3";

package MyApp;

message Person {
  string name = 1;
  int32 age = 2;
  repeated string hobbies = 3;
}
```

## 步骤 4：配置项目文件

```xml
<ItemGroup>
  <!-- 包含 proto 文件 -->
  <ProtoFile Include="Protos\**\*.proto" />
</ItemGroup>
```

## 步骤 5：编写代码

```csharp
using Protobuf.Core;
using MyApp; // 自动生成

// 创建消息
var person = new Person();
person.SetName("张三");
person.SetAge(25);
person.AddHobbies("编程");
person.AddHobbies("阅读");

// 序列化
byte[] data = person.Serialize();
Console.WriteLine($"序列化: {data.Length} 字节");

// 反序列化
var person2 = Person.Parser.ParseFrom(data);
Console.WriteLine($"姓名: {person2.Name}");
Console.WriteLine($"年龄: {person2.Age}");
Console.WriteLine($"爱好: {string.Join(", ", person2.Hobbies)}");
```

## 步骤 6：构建运行

```bash
dotnet build
dotnet run
```

## 🎉 完成！

你现在已经：
- ✅ 定义了 Protobuf 消息
- ✅ 配置了自动代码生成
- ✅ 实现了序列化/反序列化
- ✅ 运行了第一个 Protobuf 程序

## 📚 下一步

- 查看 [完整配置指南](build-system-guide.md)
- 了解 [高级特性](csproj-configuration.md)
- 浏览 [示例代码](../examples/)

## 💡 常见问题

### Q: 如何添加多个 proto 文件？

```xml
<ItemGroup>
  <ProtoFile Include="Protos\**\*.proto" />
</ItemGroup>
```
使用 `**\*.proto` 自动包含所有子目录。

### Q: 生成的代码在哪里？

生成代码是**虚拟的**，直接在 IDE 中可用：
- ✅ IntelliSense 自动完成
- ✅ Go to Definition 可导航
- ✅ 不需要手动管理

### Q: 如何调试生成的代码？

查看编译输出中的生成文件，或者在 IDE 中使用"转到定义"查看生成的类。

### Q: 支持哪些类型？

所有 Protobuf3 类型：
- 基础类型：`int32`, `string`, `bool` 等
- 复合类型：`repeated`, `oneof`, `map`
- 嵌套消息和枚举
