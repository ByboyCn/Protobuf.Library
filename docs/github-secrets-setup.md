# GitHub Actions Secrets 配置指南

为了使项目的 CI/CD 流水线正常工作，需要在 GitHub 仓库中配置以下 secrets。

## 必需的 Secrets

### 1. NUGET_API_KEY（可选）
用于发布包到 NuGet.org。

**获取步骤：**
1. 访问 [NuGet.org](https://www.nuget.org/) 并登录
2. 点击右上角头像 → Account Settings
3. 选择 "API Keys" 部分
4. 点击 "Create" 创建新的 API Key
5. 配置以下信息：
   - **Key name**: 任意名称，如 "GitHub Actions"
   - **Glob pattern**: `*` (允许推送所有包)
   - **Expires**: 选择适当的有效期
6. 复制生成的 API Key

**在 GitHub 中配置：**
1. 访问你的 GitHub 仓库
2. 进入 Settings → Secrets and variables → Actions
3. 点击 "New repository secret"
4. Name: `NUGET_API_KEY`
5. Value: 粘贴你的 NuGet API Key
6. 点击 "Add secret"

**注意：** 如果你不打算发布到 NuGet.org，这个 secret 可以不配置。

### 2. GITHUB_TOKEN（自动提供）
用于发布包到 GitHub Packages。

**说明：**
- 这个 secret 由 GitHub Actions 自动提供，无需手动配置
- 在工作流中通过 `${{ secrets.GITHUB_TOKEN }}` 引用
- 需要仓库的 `packages: write` 权限

## 权限要求

### GitHub Packages 发布
工作流需要以下权限：
```yaml
permissions:
  packages: write    # 写入 GitHub Packages
  contents: read     # 读取仓库内容
```

这些权限已在工作流文件中正确配置。

## 工作流触发条件

### 自动运行（每次推送）
- `build` job - 构建和测试

### 仅在版本标签时运行
创建并推送版本标签时触发（如 `v1.0.0`）：
- `publish-github-packages` - 发布到 GitHub Packages
- `publish-nuget-org` - 发布到 NuGet.org（需要 NUGET_API_KEY）
- `create-github-release` - 创建 GitHub Release

**发布新版本的步骤：**
```bash
# 创建版本标签
git tag v1.0.0

# 推送标签到远程
git push origin v1.0.0
```

## 验证配置

### 检查 Secrets 是否正确配置
1. 访问仓库的 Settings → Secrets and variables → Actions
2. 确认 `NUGET_API_KEY` 存在（如果需要发布到 NuGet.org）

### 测试工作流
1. 推送代码到 main 分支触发构建
2. 创建版本标签测试完整发布流程

## 故障排除

### GitHub Packages 发布失败 (404 错误)
**原因：** NuGet 源未正确配置
**解决：** 工作流已修复，会自动配置 GitHub Packages 源

### NuGet.org 发布失败 (认证错误)
**原因：** NUGET_API_KEY 未配置或无效
**解决：** 检查 secret 是否正确配置，API Key 是否有效

### 权限错误
**原因：** 工作流缺少必要的权限
**解决：** 检查工作流中的 `permissions` 配置

## 安全建议

1. **定期轮换 API Keys**：定期更新 NuGet API Key
2. **限制权限**：API Key 只授予必要的权限
3. **监控使用**：定期检查 NuGet.org 和 GitHub Packages 的活动日志
4. **不要泄露**：永远不要在代码中硬编码 API Keys

## 相关链接

- [GitHub Actions Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [NuGet API Keys](https://docs.nuget.org/docs/reference/api-keys)
- [GitHub Packages](https://docs.github.com/en/packages)
