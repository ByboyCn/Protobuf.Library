# Protobuf Library 项目配置指南

## 在 .csproj 中配置 Proto 文件

### 方式 1：自动包含所有 Proto 文件（推荐）

如果你希望自动包含项目中的所有 `.proto` 文件，只需要添加 Protobuf.Generator 引用：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- 引用核心库 -->
    <ProjectReference Include="Protobuf.Core\Protobuf.Core.csproj" />

    <!-- 引用源代码生成器 -->
    <ProjectReference Include="Protobuf.Generator\Protobuf.Generator.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
  </ItemGroup>

</Project>
```

这样会自动包含项目中所有的 `**/*.proto` 文件。

### 方式 2：显式指定 Proto 文件

如果你想精确控制哪些 proto 文件被处理：

```xml
<ItemGroup>
  <!-- 单个文件 -->
  <ProtoFile Include="Protos\my-message.proto" />

  <!-- 通配符匹配 -->
  <ProtoFile Include="Protos\**\*.proto" />

  <!-- 排除特定文件 -->
  <ProtoFile Include="Protos\**\*.proto" Exclude="Protos\**\*.test.proto" />
</ItemGroup>
```

### 方式 3：使用 AdditionalFiles（兼容性更好）

```xml
<ItemGroup>
  <!-- 带元数据的 AdditionalFiles -->
  <AdditionalFiles Include="Protos\**\*.proto">
    <FileType>proto</FileType>
  </AdditionalFiles>
</ItemGroup>
```

### 方式 4：混合配置

```xml
<ItemGroup>
  <!-- 从不同目录包含 proto 文件 -->
  <ProtoFile Include="SharedProtos\**\*.proto" />
  <ProtoFile Include="LocalProtos\api.proto" />

  <!-- 排除测试文件 -->
  <ProtoFile Include="**\*.proto" Exclude="**\*.test.proto" />
</ItemGroup>
```

## 完整的项目文件示例

### 基础示例

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- 核心库引用 -->
    <ProjectReference Include="..\src\Protobuf.Core\Protobuf.Core.csproj" />

    <!-- 源代码生成器引用 -->
    <ProjectReference Include="..\src\Protobuf.Generator\Protobuf.Generator.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
  </ItemGroup>

  <ItemGroup>
    <!-- Proto 文件配置 -->
    <ProtoFile Include="Protos\**\*.proto" />
  </ItemGroup>

</Project>
```

### 高级示例（带生成设置）

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <!-- 启用扩展分析器规则 -->
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
    <!-- 生成器选项 -->
    <ProtobufGenerateOptions>PascalCase,EnableLogging</ProtobufGenerateOptions>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\src\Protobuf.Core\Protobuf.Core.csproj" />
    <ProjectReference Include="..\src\Protobuf.Generator\Protobuf.Generator.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
  </ItemGroup>

  <ItemGroup>
    <!-- 包含多个目录的 proto 文件 -->
    <ProtoFile Include="Protos\Core\*.proto" ProtoRoot="Protos" />
    <ProtoFile Include="Protos\API\*.proto" ProtoRoot="Protos" />

    <!-- 只包含特定文件 -->
    <ProtoFile Include="Protos\common-types.proto" />
  </ItemGroup>

  <!-- 导入构建 targets（可选，通常自动导入） -->
  <Import Project="..\build\Protobuf.Generator.targets" Condition="Exists('..\build\Protobuf.Generator.targets')" />

</Project>
```

## 构建时行为

### 编译时输出

构建时，你会在输出中看到：

```
Protobuf Generator: Found 3 .proto file(s)
  - Protos\person.proto
  - Protos\api.proto
  - Protos\common-types.proto
```

### 生成的代码

- 生成的代码会自动添加到编译中
- 无需手动维护生成的代码
- 生成的类在 `Generated.Protobuf` 命名空间（或你在 .proto 中定义的 package）

### 故障排除

#### Proto 文件未被识别

1. 确认文件扩展名是 `.proto`
2. 检查文件是否包含在项目中
3. 查看构建输出中的 "Protobuf Generator" 消息

#### 生成的代码不可见

1. 确认引用了 `Protobuf.Core`
2. 检查生成的类的命名空间
3. 查看编译器错误消息

## MSBuild 属性和目标

### 可用的属性

```xml
<PropertyGroup>
  <!-- 启用扩展分析器规则（推荐） -->
  <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>

  <!-- 禁用生成器（调试用） -->
  <ProtobufGeneratorEnabled>false</ProtobufGeneratorEnabled>

  <!-- 设置输出详细程度 -->
  <ProtobufGeneratorVerbosity>detailed</ProtobufGeneratorVerbosity>
</PropertyGroup>
```

### 自定义目标

```xml
<!-- 在生成前运行自定义逻辑 -->
<Target Name="BeforeProtobufGeneration" BeforeTargets="CoreCompile">
  <Message Text="自定义预生成任务..." />
</Target>

<!-- 在生成后运行自定义逻辑 -->
<Target Name="AfterProtobufGeneration" AfterTargets="CoreCompile">
  <Message Text="自定义后生成任务..." />
</Target>
```

## 与 Visual Studio 的集成

### IntelliSense 支持

- 生成的类会立即出现在 IntelliSense 中
- 支持导航到定义（Go to Definition）
- 支持查找所有引用（Find All References）

### 实时错误报告

- .proto 文件语法错误会实时显示
- 类型不匹配会立即提示
- 代码生成错误会显示在错误列表中

## 多项目解决方案

### 共享 Proto 文件

```xml
<!-- 项目 A -->
<ItemGroup>
  <ProtoFile Include="..\SharedProtos\*.proto" />
</ItemGroup>

<!-- 项目 B -->
<ItemGroup>
  <ProtoFile Include="..\SharedProtos\*.proto" />
</ItemGroup>
```

### 依赖项管理

如果多个项目使用相同的 proto 定义，考虑：

1. 创建一个共享的 Proto 项目
2. 在其他项目中引用它
3. 使用导入语句（`import`）组织 proto 文件
