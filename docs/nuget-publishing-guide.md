# 🚀 NuGet 发布完整指南

## 📋 发布前准备清单

### 1. 项目配置文件
- ✅ 每个 `.csproj` 文件需要完整的 NuGet 元数据
- ✅ 版本号管理
- ✅ 许可证信息
- ✅ 项目图标和 README

### 2. GitHub 配置
- ✅ GitHub Actions 工作流
- ✅ Repository secrets（如果发布到 NuGet.org）
- ✅ GitHub Packages 权限

### 3. 发布环境
- ✅ GitHub Packages（主要）
- ✅ NuGet.org（可选）

## 🔧 第一步：完善项目配置

### 更新所有 `.csproj` 文件

#### 1. Protobuf.Core.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <LangVersion>latest</LangVersion>
    <Aot>true</Aot>
  </PropertyGroup>

  <PropertyGroup>
    <!-- NuGet 包元数据 -->
    <PackageId>Protobuf.Core</PackageId>
    <Version>1.0.0</Version>
    <Authors>Protobuf.Library Contributors</Authors>
    <Company>Protobuf.Library</Company>
    <Product>Protobuf.Library</Product>
    <Description>Lightweight Protobuf3 Runtime Library with AOT Support - Core runtime with zero dependencies</Description>
    <PackageTags>protobuf;serialization;aot;nativeaot;protobuf3;google-protobuf;performance</PackageTags>
    <PackageProjectUrl>https://github.com/yourusername/Protobuf.Library</PackageProjectUrl>
    <RepositoryUrl>https://github.com/yourusername/Protobuf.Library</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageReleaseNotes>
      v1.0.0 - Initial release
      - Complete Protobuf3 support
      - AOT-friendly reflection API
      - JSON serialization support
      - Zero external dependencies
    </PackageReleaseNotes>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RequireLicenseAcceptance>true</RequireLicenseAcceptance>
    
    <!-- 发布配置 -->
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    
    <!-- 生成 PDB -->
    <DebugType>portable</DebugType>
    <DebugSymbols>true</DebugSymbols>
  </PropertyGroup>

  <ItemGroup>
    <!-- README 文件 -->
    <None Include="..\..\README.md" Pack="true" PackagePath="\"/>
    <!-- 图标文件 -->
    <None Include="..\..\icon.png" Pack="true" PackagePath="\"/>
  </ItemGroup>

</Project>
```

#### 2. Protobuf.Parser.csproj

```xml
<PropertyGroup>
  <PackageId>Protobuf.Parser</PackageId>
  <Version>1.0.0</Version>
  <Description>Protobuf3 .proto file parser with full AST support</Description>
  <PackageTags>protobuf;parser;proto;ast;protobuf3</PackageTags>
  <!-- ... 其他元数据同上 ... -->
</PropertyGroup>
```

#### 3. Protobuf.Generator.csproj

```xml
<PropertyGroup>
  <PackageId>Protobuf.Generator</PackageId>
  <Version>1.0.0</Version>
  <Description>Source Generator for Protobuf3 - Auto-generates C# code from .proto files</Description>
  <PackageTags>protobuf;generator;source-generator;roslyn;analyzers</PackageTags>
  <DevelopmentDependency>true</DevelopmentDependency>
  <!-- ... 其他元数据同上 ... -->
</PropertyGroup>
```

#### 4. Protobuf.Json.csproj

```xml
<PropertyGroup>
  <PackageId>Protobuf.Json</PackageId>
  <Version>1.0.0</Version>
  <Description>JSON serialization support for Protobuf3 messages (PascalCase)</Description>
  <PackageTags>protobuf;json;serialization;protobuf3</PackageTags>
  <!-- ... 其他元数据同上 ... -->
</PropertyGroup>
```

#### 5. Protobuf.Reflection.csproj

```xml
<PropertyGroup>
  <PackageId>Protobuf.Reflection</PackageId>
  <Version>1.0.0</Version>
  <Description>AOT-friendly reflection API for Protobuf3 messages</Description>
  <PackageTags>protobuf;reflection;aot;metadata</PackageTags>
  <!-- ... 其他元数据同上 ... -->
</PropertyGroup>
```

## 🎨 第二步：添加包资源

### 1. 创建包图标

创建 `icon.png` (256x256 像素):

```bash
# 在项目根目录创建一个简单的图标文件
# 可以使用任何图像编辑工具或在线生成器
```

### 2. 添加 LICENSE 文件

创建 `LICENSE` 文件:

```
MIT License

Copyright (c) 2024 Protobuf.Library Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 🔐 第三步：GitHub 配置

### 1. 创建 GitHub Actions 工作流

工作流文件已创建：`.github/workflows/build-and-publish.yml`

### 2. 配置 GitHub Packages 权限

1. 进入 GitHub 仓库设置
2. Settings → Actions → General
3. 在 "Workflow permissions" 中选择 "Read and write permissions"
4. 确保 "Allow GitHub Actions to create and approve pull requests" 已启用

### 3. （可选）配置 NuGet.org API Key

如果同时要发布到 NuGet.org：

1. 在 [NuGet.org](https://www.nuget.org/) 创建账户
2. 创建 API Key：Account → API Keys → Create
3. 在 GitHub 仓库中添加 Secret：
   - Settings → Secrets and variables → Actions
   - New repository secret
   - Name: `NUGET_API_KEY`
   - Value: 你的 NuGet API Key

## 📦 第四步：版本管理

### 版本策略

```bash
# 主要版本 (不兼容的 API 变更)
v2.0.0

# 次要版本 (向后兼容的功能新增)
v1.1.0

# 修订版本 (向后兼容的问题修正)
v1.0.1

# 预发布版本
v1.0.0-beta.1
v1.0.0-rc.1
```

### 创建版本标签

```bash
# 创建版本标签
git tag v1.0.0
git push origin v1.0.0

# 或者创建带注释的标签
git tag -a v1.0.0 -m "Release v1.0.0: Initial stable release"
git push origin v1.0.0
```

## 🚀 第五步：发布流程

### 自动发布（推荐）

```bash
# 1. 更新版本号
# 修改所有 .csproj 文件中的 <Version>1.0.0</Version>

# 2. 提交更改
git add .
git commit -m "Release v1.0.0"

# 3. 创建标签并推送
git tag v1.0.0
git push origin main --tags

# 4. GitHub Actions 自动构建并发布
# 📦 检查 Actions 标签页查看构建进度
```

### 手动测试

```bash
# 1. 本地测试打包
dotnet pack --configuration Release

# 2. 检查生成的包
ls bin/Release/*.nupkg

# 3. 测试安装
dotnet new console -o TestApp
cd TestApp
dotnet add package Protobuf.Core --source https://nuget.pkg.github.com/yourusername/Protobuf.Library
```

## 📊 第六步：验证发布

### 1. 检查 GitHub Packages

```
1. 访问 GitHub 仓库
2. 点击 "Packages" 标签（如果看不到，在右侧边栏）
3. 查看发布的包版本
```

### 2. 测试安装包

```bash
# 从 GitHub Packages 安装
dotnet add package Protobuf.Core --source https://nuget.pkg.github.com/yourusername/Protobuf.Library

# 或者配置 NuGet 源
dotnet nuget add source --name "github" "https://nuget.pkg.github.com/yourusername/Protobuf.Library"
dotnet add package Protobuf.Core
```

### 3. 检查 GitHub Release

```
1. 访问 GitHub 仓库
2. 点击 "Releases" 标签
3. 查看 v1.0.0 release
4. 下载 .nupkg 文件验证
```

## 🔧 配置 NuGet 源（客户端）

### 方式一：临时使用

```bash
dotnet add package Protobuf.Core --source https://nuget.pkg.github.com/yourusername/index.json
```

### 方式二：永久配置

```bash
# 添加 GitHub Packages 源
dotnet nuget add source --name "GitHubProtobuf" "https://nuget.pkg.github.com/yourusername/index.json"

# 使用时直接引用
dotnet add package Protobuf.Core
```

### 方式三：全局配置

创建 `nuget.config` 文件：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/yourusername/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

## 📋 发布检查清单

### 发布前检查

- [ ] 所有 .csproj 文件包含完整元数据
- [ ] 版本号已更新
- [ ] README.md 文件已包含
- [ ] icon.png 文件已创建
- [ ] LICENSE 文件已创建
- [ ] 所有测试通过
- [ ] GitHub Actions 工作流已配置
- [ ] 版本标签已创建

### 发布后检查

- [ ] GitHub Actions 构建成功
- [ ] 包出现在 GitHub Packages 中
- [ ] GitHub Release 已创建
- [ ] 可以成功安装包
- [ ] 包中的 README 正确显示

## 🔄 持续发布流程

### 日常开发

```bash
# 1. 开发新功能
git checkout -b feature/new-feature
# ... 开发代码 ...

# 2. 提交和推送
git add .
git commit -m "Add new feature"
git push origin feature/new-feature

# 3. 创建 Pull Request
# GitHub Actions 会自动运行测试
```

### 发布新版本

```bash
# 1. 更新版本号
# 将所有 .csproj 中的版本号从 1.0.0 改为 1.1.0

# 2. 提交版本更新
git add .
git commit -m "Bump version to 1.1.0"

# 3. 创建标签
git tag v1.1.0
git push origin main --tags

# 4. 自动发布
# GitHub Actions 会自动构建并发布新版本
```

## 🎯 使用示例

### 用户如何使用你的包

```bash
# 1. 配置 NuGet 源
dotnet nuget add source --name "ProtobufLib" "https://nuget.pkg.github.com/yourusername/index.json"

# 2. 创建新项目
dotnet new console -o MyApp
cd MyApp

# 3. 添加包引用
dotnet add package Protobuf.Core
dotnet add package Protobuf.Generator
dotnet add package Protobuf.Json
dotnet add package Protobuf.Reflection

# 4. 开始使用
# 参考 README.md 中的使用指南
```

## 💡 最佳实践

### 版本管理
- 使用语义化版本（Semantic Versioning）
- 在 Git 标签中明确版本号
- 在 PackageReleaseNotes 中记录变更

### 发布策略
- 先发布到 GitHub Packages 测试
- 稳定后再发布到 NuGet.org（可选）
- 使用预发布版本进行 beta 测试

### 安全性
- 不要在代码中硬编码 API Keys
- 使用 GitHub Secrets 存储敏感信息
- 定期轮换 API Keys

### 文档
- 保持 README.md 与发布版本同步
- 在 PackageReleaseNotes 中详细记录变更
- 提供完整的使用示例

## 🆘 故障排除

### 常见问题

#### 1. 发布失败：401 Unauthorized
**原因**: GitHub Token 权限不足
**解决**: 检查 Repository Secrets 和 Workflow Permissions

#### 2. 包没有出现在 GitHub Packages
**原因**: 标签格式错误或权限问题
**解决**: 确保标签格式为 `v1.0.0`，检查 Packages 权限

#### 3. 无法从 GitHub Packages 安装
**原因**: NuGet 源配置错误
**解决**: 检查 NuGet 源 URL 格式和认证信息

#### 4. 包的 README 没有显示
**原因**: README.md 文件路径不正确
**解决**: 确保 `<PackageReadmeFile>` 和 `<None>` 元素正确配置

## 📚 相关资源

- [GitHub Packages 文档](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
- [NuGet.org 文档](https://docs.microsoft.com/nuget/)
- [.NET Standard 版本控制](https://docs.microsoft.com/nuget/concepts/package-versioning)

## 🎉 完成！

现在你的 Protobuf.Library 已经可以发布到 GitHub Packages 了！

**下一步**：
1. 完善项目配置文件
2. 创建 icon.png 和 LICENSE 文件
3. 创建版本标签
4. 推送到 GitHub
5. 查看自动构建和发布过程

祝你发布顺利！🚀
