using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Protobuf.CodeGeneration;

namespace Protobuf.Generator;

/// <summary>
/// Protobuf Source Generator
/// 支持两种模式：
/// 1. Source Generator 模式（默认）：在内存中生成代码
/// 2. 文件生成模式（通过 ProtobufEnableFileGeneration=true 启用）：将代码写入 obj 目录
/// </summary>
[Generator]
public sealed class ProtoSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // 注册 .proto 文件作为额外文本
        context.RegisterForPostInitialization(ctx =>
        {
            ctx.AddSource("ProtoSourceGenerator.g.cs", @"
// This is an indicator file for the Protobuf Source Generator
namespace Protobuf.Generator
{
    internal static class GeneratorIndicator
    {
        public const string Version = ""1.1.0"";
        public const string Mode = ""Source Generator + MSBuild Task"";
    }
}
");
        });
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // 检查是否启用了文件生成模式
        var enableFileGeneration = IsFileGenerationEnabled(context);

        // 如果启用了文件生成模式，Source Generator 完全跳过，让 MSBuild 任务处理
        if (enableFileGeneration)
        {
            ReportInfo(context, "", "File Generation Mode is enabled. Source Generator is disabled. MSBuild tasks will generate code to obj/GeneratedProtobuf/ directory.");
            return;
        }

        // 查找所有 .proto 文件
        var protoFiles = context.AdditionalFiles
            .Where(static file => file.Path != null && file.Path.EndsWith(".proto", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (!protoFiles.Any())
        {
            return;
        }

        // 读取所有 proto 文件内容
        var protoFileContents = new Dictionary<string, string>();
        foreach (var protoFile in protoFiles)
        {
            try
            {
                var sourceText = protoFile.GetText(context.CancellationToken);
                if (sourceText == null)
                {
                    continue;
                }

                var protoContent = sourceText.ToString();
                protoFileContents[protoFile.Path!] = protoContent;
            }
            catch (Exception ex)
            {
                ReportError(context, protoFile.Path ?? "", $"Error reading proto file: {ex.Message}");
            }
        }

        // 使用共享的代码生成器
        var generator = new ProtoCodeGenerator();
        var generatedFiles = generator.GenerateCode(protoFileContents);

        // 在 Source Generator 模式下，将生成的代码添加到编译
        foreach (var kvp in generatedFiles)
        {
            var fileName = kvp.Key;
            var code = kvp.Value;
            context.AddSource(fileName, code);
        }

        // 输出诊断信息
        foreach (var diagnostic in generator.Diagnostics)
        {
            switch (diagnostic.Level)
            {
                case IProtoCodeGenerator.DiagnosticLevel.Info:
                    ReportInfo(context, diagnostic.FilePath, diagnostic.Message);
                    break;
                case IProtoCodeGenerator.DiagnosticLevel.Warning:
                    ReportWarning(context, diagnostic.FilePath, diagnostic.Message);
                    break;
                case IProtoCodeGenerator.DiagnosticLevel.Error:
                    ReportError(context, diagnostic.FilePath, diagnostic.Message);
                    break;
            }
        }
    }

    /// <summary>
    /// 检查是否启用了文件生成模式
    /// </summary>
    private bool IsFileGenerationEnabled(GeneratorExecutionContext context)
    {
        // 检查分析器配置
        try
        {
            var analyzerConfigOptions = context.AnalyzerConfigOptions.GlobalOptions;
            if (analyzerConfigOptions.TryGetValue("build_property.ProtobufEnableFileGeneration", out var value))
            {
                return value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // 如果读取配置失败，默认为 false
        }

        return false;
    }

    private void ReportError(GeneratorExecutionContext context, string filePath, string message)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "PROTO001",
                    "Proto File Error",
                    message,
                    "Protobuf.Generator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.Create(filePath, TextSpan.FromBounds(0, 0), new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0)))));
    }

    private void ReportWarning(GeneratorExecutionContext context, string filePath, string message)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "PROTO002",
                    "Proto File Warning",
                    message,
                    "Protobuf.Generator",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true),
                Location.Create(filePath, TextSpan.FromBounds(0, 0), new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0)))));
    }

    private void ReportInfo(GeneratorExecutionContext context, string filePath, string message)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "PROTO003",
                    "Proto File Info",
                    message,
                    "Protobuf.Generator",
                    DiagnosticSeverity.Info,
                    isEnabledByDefault: true),
                Location.Create(filePath, TextSpan.FromBounds(0, 0), new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0)))));
    }
}
