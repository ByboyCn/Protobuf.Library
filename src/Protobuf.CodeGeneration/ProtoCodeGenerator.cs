using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Protobuf.Parser;

namespace Protobuf.CodeGeneration;

/// <summary>
/// 共享的 Protobuf 代码生成器实现
/// 可被 Source Generator 和 MSBuild 任务共同使用
/// </summary>
public sealed class ProtoCodeGenerator : IProtoCodeGenerator
{
    // 存储所有解析的 proto 文件 AST
    private readonly Dictionary<string, ProtoFile> _allProtoFiles = new();
    // 存储文件之间的依赖关系
    private readonly Dictionary<string, List<string>> _fileDependencies = new();
    // 存储所有 proto 文件路径到其内容的映射
    private readonly Dictionary<string, string> _protoFileContents = new();

    /// <summary>
    /// 诊断消息列表
    /// </summary>
    public List<IProtoCodeGenerator.Diagnostic> Diagnostics { get; } = new();

    /// <summary>
    /// 从 proto 文件生成 C# 代码
    /// </summary>
    public Dictionary<string, string> GenerateCode(Dictionary<string, string> protoFiles)
    {
        // 清理状态
        _allProtoFiles.Clear();
        _fileDependencies.Clear();
        _protoFileContents.Clear();
        Diagnostics.Clear();

        var generatedFiles = new Dictionary<string, string>();

        if (protoFiles.Count == 0)
        {
            return generatedFiles;
        }

        // 第一步：读取所有 proto 文件内容
        foreach (var kvp in protoFiles)
        {
            _protoFileContents[kvp.Key] = kvp.Value;
        }

        // 第二步：解析所有 proto 文件 AST
        foreach (var kvp in _protoFileContents)
        {
            try
            {
                ParseProtoFile(kvp.Key, kvp.Value);
            }
            catch (Exception ex)
            {
                Diagnostics.Add(new IProtoCodeGenerator.Diagnostic
                {
                    FilePath = kvp.Key,
                    Level = IProtoCodeGenerator.DiagnosticLevel.Error,
                    Message = $"Error parsing proto file: {ex.Message}"
                });
            }
        }

        // 第三步：解析导入文件并建立依赖关系
        foreach (var kvp in _allProtoFiles)
        {
            try
            {
                ResolveImports(kvp.Value);
            }
            catch (Exception ex)
            {
                Diagnostics.Add(new IProtoCodeGenerator.Diagnostic
                {
                    FilePath = kvp.Key,
                    Level = IProtoCodeGenerator.DiagnosticLevel.Error,
                    Message = $"Error resolving imports: {ex.Message}"
                });
            }
        }

        // 第四步：按拓扑排序生成代码
        var sortedFiles = TopologicalSort();

        foreach (var filePath in sortedFiles)
        {
            if (_allProtoFiles.TryGetValue(filePath, out var protoFileAst))
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var code = GenerateCode(protoFileAst, fileName);
                    generatedFiles[$"{fileName}.g.cs"] = code;

                    Diagnostics.Add(new IProtoCodeGenerator.Diagnostic
                    {
                        FilePath = filePath,
                        Level = IProtoCodeGenerator.DiagnosticLevel.Info,
                        Message = $"Generated {fileName}.g.cs with {protoFileAst.Enums.Count} enum(s) and {protoFileAst.Messages.Count} message(s)"
                    });
                }
                catch (Exception ex)
                {
                    Diagnostics.Add(new IProtoCodeGenerator.Diagnostic
                    {
                        FilePath = filePath,
                        Level = IProtoCodeGenerator.DiagnosticLevel.Error,
                        Message = $"Error generating code: {ex.Message}"
                    });
                }
            }
        }

        return generatedFiles;
    }

    /// <summary>
    /// 解析单个 proto 文件
    /// </summary>
    private void ParseProtoFile(string filePath, string content)
    {
        var lexer = new Lexer(content);
        var tokens = lexer.Tokenize();

        var protoParser = new Parser.Parser(tokens);
        var protoFileAst = protoParser.Parse();

        protoFileAst.FilePath = filePath;
        _allProtoFiles[filePath] = protoFileAst;
    }

    /// <summary>
    /// 解析导入文件并建立依赖关系
    /// </summary>
    private void ResolveImports(ProtoFile protoFile)
    {
        if (!_fileDependencies.ContainsKey(protoFile.FilePath!))
        {
            _fileDependencies[protoFile.FilePath!] = new List<string>();
        }

        foreach (var import in protoFile.Imports)
        {
            var importPath = ResolveImportPath(import.Path, protoFile.FilePath!);

            if (string.IsNullOrEmpty(importPath))
            {
                if (!import.IsWeak)
                {
                    Diagnostics.Add(new IProtoCodeGenerator.Diagnostic
                    {
                        FilePath = protoFile.FilePath!,
                        Level = IProtoCodeGenerator.DiagnosticLevel.Warning,
                        Message = $"Could not resolve import: {import.Path}"
                    });
                }
                continue;
            }

            // 添加依赖关系
            if (!_fileDependencies[protoFile.FilePath!].Contains(importPath))
            {
                _fileDependencies[protoFile.FilePath!].Add(importPath);
            }

            // 如果导入的文件还没有被解析，尝试解析它
            if (!_allProtoFiles.ContainsKey(importPath))
            {
                if (_protoFileContents.TryGetValue(importPath, out var content))
                {
                    try
                    {
                        ParseProtoFile(importPath, content);
                        // 递归解析导入文件的导入
                        ResolveImports(_allProtoFiles[importPath]);
                    }
                    catch (Exception ex)
                    {
                        if (!import.IsWeak)
                        {
                            Diagnostics.Add(new IProtoCodeGenerator.Diagnostic
                            {
                                FilePath = protoFile.FilePath!,
                                Level = IProtoCodeGenerator.DiagnosticLevel.Error,
                                Message = $"Error parsing imported file {importPath}: {ex.Message}"
                            });
                        }
                    }
                }
                else if (!import.IsWeak)
                {
                    Diagnostics.Add(new IProtoCodeGenerator.Diagnostic
                    {
                        FilePath = protoFile.FilePath!,
                        Level = IProtoCodeGenerator.DiagnosticLevel.Warning,
                        Message = $"Imported file not found: {import.Path}"
                    });
                }
            }
        }
    }

    /// <summary>
    /// 解析导入路径
    /// </summary>
    private string? ResolveImportPath(string importPath, string currentFilePath)
    {
        // 尝试相对于当前文件目录解析
        var currentDirectory = Path.GetDirectoryName(currentFilePath);
        var relativePath = Path.Combine(currentDirectory ?? "", importPath);

        // 规范化路径
        relativePath = Path.GetFullPath(relativePath);

        // 检查文件是否存在
        if (_protoFileContents.ContainsKey(relativePath))
        {
            return relativePath;
        }

        // 尝试直接使用导入路径（可能是绝对路径或相对于项目根目录的路径）
        if (_protoFileContents.ContainsKey(importPath))
        {
            return importPath;
        }

        // 尝试在常见位置查找
        var projectRoot = Path.GetDirectoryName(currentFilePath);
        while (projectRoot != null)
        {
            var testPath = Path.Combine(projectRoot, importPath);
            if (_protoFileContents.ContainsKey(testPath))
            {
                return testPath;
            }

            // 检查 proto 子目录
            testPath = Path.Combine(projectRoot, "proto", importPath);
            if (_protoFileContents.ContainsKey(testPath))
            {
                return testPath;
            }

            projectRoot = Path.GetDirectoryName(projectRoot);
        }

        return null;
    }

    /// <summary>
    /// 拓扑排序以确保依赖项在使用之前生成
    /// </summary>
    private List<string> TopologicalSort()
    {
        var sorted = new List<string>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        foreach (var filePath in _allProtoFiles.Keys)
        {
            if (!visited.Contains(filePath))
            {
                Visit(filePath, visited, visiting, sorted);
            }
        }

        return sorted;
    }

    /// <summary>
    /// 深度优先搜索访问节点
    /// </summary>
    private void Visit(string filePath, HashSet<string> visited, HashSet<string> visiting, List<string> sorted)
    {
        if (visiting.Contains(filePath))
        {
            // 循环依赖，但仍需处理
            return;
        }

        if (visited.Contains(filePath))
        {
            return;
        }

        visiting.Add(filePath);

        if (_fileDependencies.TryGetValue(filePath, out var dependencies))
        {
            foreach (var dependency in dependencies)
            {
                if (_allProtoFiles.ContainsKey(dependency))
                {
                    Visit(dependency, visited, visiting, sorted);
                }
            }
        }

        visiting.Remove(filePath);
        visited.Add(filePath);
        sorted.Add(filePath);
    }

    /// <summary>
    /// 生成单个 proto 文件的代码
    /// </summary>
    private string GenerateCode(ProtoFile protoFile, string fileName)
    {
        var codeBuilder = new System.Text.StringBuilder();

        // 生成文件头
        codeBuilder.AppendLine("// <auto-generated>");
        codeBuilder.AppendLine("//     Generated by Protobuf Source Generator");
        codeBuilder.AppendLine("// </auto-generated>");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member");
        codeBuilder.AppendLine();

        // 确定命名空间
        var namespaceName = protoFile.Package;
        if (string.IsNullOrEmpty(namespaceName))
        {
            namespaceName = "Generated.Protobuf";
        }

        // 生成所有枚举
        foreach (var enumDecl in protoFile.Enums)
        {
            var enumTemplate = new CodeTemplates.EnumTemplate(enumDecl, namespaceName);
            codeBuilder.AppendLine(enumTemplate.Generate());
            codeBuilder.AppendLine();
        }

        // 生成所有消息
        foreach (var messageDecl in protoFile.Messages)
        {
            var messageTemplate = new CodeTemplates.MessageTemplate(messageDecl, namespaceName, protoFile, _allProtoFiles);
            codeBuilder.AppendLine(messageTemplate.Generate());
            codeBuilder.AppendLine();
        }

        return codeBuilder.ToString();
    }
}
