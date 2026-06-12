# 📦 NuGet 发布配置完成指南

## ✅ 已完成的配置

### 1. 统一版本号管理 ✅
**文件**: `Directory.Build.props`
- 集中管理所有包的版本号
- 只需在一个地方更新版本
- 支持环境变量覆盖

### 2. 所有项目 NuGet 元数据 ✅
- Protobuf.Core.csproj
- Protobuf.Parser.csproj  
- Protobuf.Generator.csproj
- Protobuf.Json.csproj
- Protobuf.Reflection.csproj

### 3. LICENSE 文件 ✅
**文件**: `LICENSE`
- MIT License
- 符合开源标准

### 4. 图标文件 🎨
**文件**: `icon.svg`
- 已创建 SVG 格式图标
- 需要转换为 PNG 格式

## 🔄 版本号管理说明

### 当前版本号：1.0.0

### 如何更新版本号

#### 方式一：修改 Directory.Build.props（推荐）

```xml
<!-- 在 Directory.Build.props 中 -->
<Version>1.1.0</Version>
```

#### 方式二：使用环境变量

```bash
# Windows
set PACKAGE_VERSION=1.1.0
dotnet build

# Linux/macOS
export PACKAGE_VERSION=1.1.0
dotnet build
```

#### 方式三：直接在 .csproj 中覆盖

```xml
<!-- 在特定项目的 .csproj 中 -->
<PropertyGroup>
  <Version>1.1.0</Version> <!-- 覆盖全局版本 -->
</PropertyGroup>
```

## 🎨 图标处理

### 当前状态
- ✅ 已创建 `icon.svg` 文件
- ⏳ 需要转换为 `icon.png` (256x256 像素)

### 转换方法

#### 方法一：使用在线工具（推荐）

1. 访问 [SVG to PNG Converter](https://svgtopng.com/)
2. 上传 `icon.svg` 文件
3. 设置尺寸为 256x256
4. 下载 `icon.png`
5. 放到项目根目录

#### 方法二：使用 PowerShell（Windows）

```powershell
# 需要安装 ImageMagick
magick -background none icon.svg icon.png
```

#### 方法三：使用在线编辑器

1. 访问 [Figma](https://www.figma.com/) 或 [Canva](https://www.canva.com/)
2. 创建 256x256 像素图标
3. 导出为 PNG 格式
4. 保存为 `icon.png`

### 临时解决方案

如果暂时没有 PNG 图标，可以先从配置中移除图标要求：

```xml
<!-- 在 Directory.Build.props 中临时注释 -->
<!-- <PackageIcon>icon.png</PackageIcon> -->
```

## 🔧 GitHub 配置步骤

### 第一步：GitHub Packages 权限

1. 访问你的 GitHub 仓库
2. **Settings** → **Actions** → **General**
3. 滚动到 "Workflow permissions"
4. 选择 **Read and write permissions**
5. 保存更改

### 第二步：添加 NuGet.org Secret（可选）

如果要同时发布到 NuGet.org：

1. 在 GitHub 仓库中
2. **Settings** → **Secrets and variables** → **Actions**
3. **New repository secret**
4. Name: `NUGET_USER`
5. Value: 你的 nuget.org 用户名（不是邮箱）

### 第三步：NuGet.org Trusted Publishing（推荐）

1. 登录 [nuget.org](https://www.nuget.org/)
2. 点击用户名 → **"受信任的发布"**
3. 添加新策略：

```
Repository Owner: ByboyCn
Repository: Protobuf.Library
Workflow file: build-and-publish.yml
Environment: (留空)
```

## 🚀 发布流程

### 首次发布（推荐先测试）

```bash
# 1. 确保工作流文件是最新的
git pull origin main

# 2. 创建预发布版本
# 在 Directory.Build.props 中设置：
# <Version>1.0.0-beta</Version>

git add .
git commit -m "Prepare v1.0.0-beta release"
git tag v1.0.0-beta
git push origin v1.0.0-beta
```

### 正式发布

```bash
# 1. 更新版本到正式版本
# 在 Directory.Build.props 中设置：
# <Version>1.0.0</Version>

git add .
git commit -m "Release v1.0.0"
git tag v1.0.0
git push origin v1.0.0
```

## 📋 发布检查清单

### 发布前检查
- [ ] 所有 .csproj 文件配置正确
- [ ] 版本号已更新
- [ ] LICENSE 文件存在
- [ ] icon.png 文件存在
- [ ] GitHub Actions 工作流已配置
- [ ] GitHub Packages 权限已设置

### 测试发布
- [ ] 创建 v1.0.0-beta 标签测试
- [ ] 验证 GitHub Actions 构建成功
- [ ] 检查 GitHub Packages 中的包
- [ ] 测试安装包

### 正式发布
- [ ] 创建 v1.0.0 标签
- [ ] 验证所有包成功发布
- [ ] 检查 GitHub Release 创建成功
- [ ] 更新文档中的版本说明

## 📦 包结构

发布后会有以下包：

### GitHub Packages
- `Protobuf.Core` - 核心运行时
- `Protobuf.Parser` - 解析器
- `Protobuf.Generator` - Source Generator
- `Protobuf.Json` - JSON 支持
- `Protobuf.Reflection` - 反射 API

### 每个包包含
- `.nupkg` - 主包
- `.snupkg` - 符号包（用于调试）

## 🔍 验证发布

### 检查 GitHub Packages

```
1. 访问 GitHub 仓库
2. 点击右侧的 "Packages" 标签（如果没有，可能在 Settings 中）
3. 查看所有 5 个包
4. 点击任意包查看详情
```

### 测试安装

```bash
# 配置 NuGet 源
dotnet nuget add source --name "GitHub" "https://nuget.pkg.github.com/ByboyCn/Protobuf.Library/index.json"

# 创建测试项目
dotnet new console -o TestApp
cd TestApp

# 添加包引用
dotnet add package Protobuf.Core --source "GitHub"
dotnet add package Protobuf.Generator --source "GitHub"

# 验证安装
dotnet build
```

## 💡 配置特点

### 🎯 统一管理
- **版本号集中**：`Directory.Build.props` 一个文件管理所有版本
- **通用信息共享**：Authors、Company、License 等在配置文件中
- **项目特定**：每个项目只定义自己特有的信息

### 🔧 灵活性
- **环境变量**：可以通过 CI/CD 设置不同版本
- **项目覆盖**：单个项目可以覆盖全局版本号
- **条件配置**：可以为不同环境设置不同配置

### 🚀 自动化
- **GitHub Actions**：自动构建、测试、发布
- **标签触发**：推送标签自动开始发布流程
- **多目标**：同时发布到 GitHub Packages 和 NuGet.org

## 🎊 下一步

### 立即可以做

1. **转换图标**：将 `icon.svg` 转换为 `icon.png`
2. **测试构建**：`dotnet build --configuration Release`
3. **本地打包**：`dotnet pack --configuration Release`

### 准备发布

1. **创建测试标签**：`git tag v1.0.0-beta`
2. **验证流程**：测试 GitHub Actions 工作流
3. **正式发布**：`git tag v1.0.0`

### 配置完成度

- ✅ 项目配置文件：100% 完成
- ✅ 版本号管理：✅ 集中管理
- ✅ LICENSE 文件：✅ MIT License
- ⏳ icon.png：需要 SVG→PNG 转换
- ✅ GitHub Actions：✅ 已配置
- ✅ 文档完整：✅ 包含详细指南

## 🎉 配置状态

所有核心配置已完成！你现在拥有：

- ✅ **完善的 NuGet 配置**：所有项目都有完整的元数据
- ✅ **统一的版本管理**：Directory.BuildProps 集中管理
- ✅ **GitHub Actions 工作流**：自动构建和发布
- ✅ **MIT License**：开源友好
- ✅ **详细文档**：完整的发布指南

**只需要将 icon.svg 转换为 icon.png，就可以开始发布了！** 🚀

需要我帮你测试构建或解决其他问题吗？