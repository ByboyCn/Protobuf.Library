@echo off
REM 修复 Visual Studio 中源代码生成器无法工作的脚本
REM
REM 问题：Visual Studio 清理后无法生成代码
REM 原因：编译器服务器缓存了旧的 DLL 或依赖项未正确复制
REM
REM 使用方法：在解决方案根目录运行此脚本

echo ========================================
echo 修复 Visual Studio 源代码生成器
echo ========================================
echo.

REM 步骤 1: 关闭 Visual Studio
echo [1/5] 请先关闭 Visual Studio！
echo 按 Ctrl+C 取消，或按任意键继续...
pause > nul

REM 步骤 2: 停止编译器服务器进程
echo.
echo [2/5] 停止编译器服务器进程...
taskkill /F /IM VBCSCompiler.exe 2>nul
taskkill /F /IM MSBuild.exe 2>nul
taskkill /F /IM devenv.exe 2>nul
echo 编译器服务器已停止

REM 步骤 3: 清理所有项目
echo.
echo [3/5] 清理所有项目...
dotnet clean
echo 项目已清理

REM 步骤 4: 重新构建生成器项目
echo.
echo [4/5] 重新构建生成器项目...
dotnet build src/Protobuf.Core/Protobuf.Core.csproj --no-incremental
dotnet build src/Protobuf.Parser/Protobuf.Parser.csproj --no-incremental
dotnet build src/Protobuf.Generator/Protobuf.Generator.csproj --no-incremental
echo 生成器项目已重新构建

REM 步骤 5: 验证依赖项
echo.
echo [5/5] 验证生成器依赖项...
if exist "src\Protobuf.Generator\bin\Debug\net8.0\Protobuf.Parser.dll" (
    echo [√] Protobuf.Parser.dll 存在
) else (
    echo [×] Protobuf.Parser.dll 缺失！
)
if exist "src\Protobuf.Generator\bin\Debug\net8.0\Protobuf.Core.dll" (
    echo [√] Protobuf.Core.dll 存在
) else (
    echo [×] Protobuf.Core.dll 缺失！
)

echo.
echo ========================================
echo 修复完成！
echo ========================================
echo.
echo 现在请：
echo 1. 打开 Visual Studio
echo 2. 选择 "生成" → "重新生成解决方案"
echo 3. 如果还是不行，关闭 VS，重新运行此脚本
echo.
pause
