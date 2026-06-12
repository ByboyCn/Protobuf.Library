# GitHub Actions Secrets 配置指南

为了使项目的 CI/CD 流水线正常工作，需要配置 GitHub Actions 工作流。

## 认证方式

### 1. NuGet.org 可信发布（推荐）
本项目使用 **NuGet.org 可信发布**（Trusted Publishing）功能，这是基于 OIDC 的无密码认证方式，更加安全。

**优势：**
- 无需管理 API Keys
- 自动化的令牌管理
- 更高的安全性
- 符合现代安全最佳实践

**配置步骤：**

#### 第一步：在 NuGet.org 创建可信发布策略
1. 访问 [NuGet.org](https://www.nuget.org/) 并登录
2. 进入你的账户设置
3. 选择 "Trusted Publishing" 或 "可信发布"
4. 点击 "Create new policy" 或 "创建新策略"
5. 填写以下信息：
   - **Policy name**: 任意名称，如 "GitHub Actions"
   - **Repository**: `ByboyCn/Protobuf.Library`（你的 GitHub 仓库）
   - **Workflow**: `.github/workflows/build-and-publish.yml`
   - **Environment**: 可以选择特定环境或留空
   - **Branch pattern**: `main` 或 `master`
6. 保存策略

#### 第二步：无需配置 Secrets
可信发布不需要任何 GitHub Secrets！GitHub Actions 会自动处理 OIDC 认证。

#### 第三步：发布新版本
```bash
# 创建版本标签
git tag v1.0.0

# 推送标签到远程
git push origin v1.0.0
```

**注意：** 首次使用可信发布时，策略可能处于 "pending full activation" 状态，需要等待几分钟让系统验证配置。

### 2. GitHub Packages（自动配置）
用于发布包到 GitHub Packages。

**说明：**
- 使用 GitHub 自动提供的 `GITHUB_TOKEN`
- 无需手动配置任何 secrets
- 需要仓库的 `packages: write` 权限

## 权限要求

### GitHub Packages 发布
```yaml
permissions:
  packages: write    # 写入 GitHub Packages
  contents: read     # 读取仓库内容
```

### NuGet.org 可信发布
```yaml
permissions:
  contents: read     # 读取仓库内容
  id-token: write    # OIDC 认证必需
```

这些权限已在工作流文件中正确配置。

## 工作流触发条件

### 自动运行（每次推送）
- `build` job - 构建和测试

### 仅在版本标签时运行
创建并推送版本标签时触发（如 `v1.0.0`）：
- `publish-github-packages` - 发布到 GitHub Packages
- `publish-nuget-org` - 发布到 NuGet.org（使用可信发布）
- `create-github-release` - 创建 GitHub Release

## 可信发布故障排除

### 策略处于 "pending" 状态
**原因：** 新创建的可信发布策略需要几分钟来激活
**解决：** 等待 5-10 分钟，然后重试推送

### 认证失败
**原因：** 可信发布策略配置不正确
**解决：**
1. 检查仓库名称是否正确
2. 确认工作流文件路径正确
3. 验证分支模式匹配

### OIDC 令牌错误
**原因：** 缺少 `id-token: write` 权限
**解决：** 确保工作流包含 `permissions: id-token: write`

### GitHub Packages 发布失败 (404 错误)
**原因：** NuGet 源未正确配置
**解决：** 工作流已修复，会自动配置 GitHub Packages 源

## 可信发布与传统 API Key 对比

| 特性 | 可信发布 | API Key |
|------|----------|---------|
| 安全性 | 高（OIDC） | 中（静态密钥） |
| 维护 | 自动 | 手动轮换 |
| 配置复杂度 | 简单 | 中等 |
| 有效期 | 动态令牌 | 永久或定期 |
| 推荐度 | ✅ 推荐 | 备选方案 |

## 安全建议

1. **使用可信发布**：优先使用 OIDC 认证而非静态 API Keys
2. **限制权限**：只授予工作流必要的权限
3. **监控活动**：定期检查 NuGet.org 和 GitHub Packages 的活动日志
4. **定期审查**：检查可信发布策略的配置

## 相关链接

- [NuGet.org 可信发布](https://learn.microsoft.com/zh-cn/nuget/nuget-org/trusted-publishing)
- [GitHub Actions OIDC](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-nuget)
- [GitHub Actions Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [GitHub Packages](https://docs.github.com/en/packages)
