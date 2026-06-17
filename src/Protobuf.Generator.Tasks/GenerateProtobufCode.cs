using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Protobuf.Generator.Tasks;

/// <summary>
/// MSBuild 任务：从 .proto 文件生成 C# 代码并写入到文件
/// </summary>
public class GenerateProtobufCode : Task
{
    /// <summary>
    /// 要处理的 proto 文件列表
    /// </summary>
    [Required]
    public ITaskItem[] ProtoFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// 输出目录
    /// </summary>
    [Required]
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 生成的文件列表（输出）
    /// </summary>
    [Output]
    public ITaskItem[] GeneratedFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// 诊断消息列表（输出）
    /// </summary>
    [Output]
    public ITaskItem[] Diagnostics { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// 执行任务
    /// </summary>
    public override bool Execute()
    {
        try
        {
            // 检查是否有 proto 文件
            if (ProtoFiles.Length == 0)
            {
                Log.LogMessage(MessageImportance.Low, "No .proto files found.");
                GeneratedFiles = Array.Empty<ITaskItem>();
                Diagnostics = Array.Empty<ITaskItem>();
                return true;
            }

            Log.LogMessage(MessageImportance.High, $"Generating code from {ProtoFiles.Length} .proto file(s)...");

            // 创建代码生成器
            var generator = new CodeGeneration.ProtoCodeGenerator();

            // 读取所有 proto 文件内容
            var protoFileContents = new Dictionary<string, string>();
            foreach (var protoFile in ProtoFiles)
            {
                var filePath = protoFile.ItemSpec;
                if (!File.Exists(filePath))
                {
                    Log.LogWarning($"Proto file not found: {filePath}");
                    continue;
                }

                try
                {
                    var content = File.ReadAllText(filePath);
                    protoFileContents[filePath] = content;
                    Log.LogMessage(MessageImportance.Low, $"  Loaded: {filePath}");
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error reading proto file {filePath}: {ex.Message}");
                }
            }

            if (protoFileContents.Count == 0)
            {
                Log.LogWarning("No valid proto files to process.");
                GeneratedFiles = Array.Empty<ITaskItem>();
                Diagnostics = Array.Empty<ITaskItem>();
                return true;
            }

            // 生成代码
            var generatedFiles = generator.GenerateCode(protoFileContents);

            // 确保输出目录存在
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            // 写入生成的文件
            var generatedFileList = new List<ITaskItem>();
            foreach (var kvp in generatedFiles)
            {
                var fileName = kvp.Key;
                var code = kvp.Value;
                var outputPath = Path.Combine(OutputDirectory, fileName);

                try
                {
                    // 只在内容改变时写入文件（避免不必要的文件修改时间更新）
                    var shouldWrite = true;
                    if (File.Exists(outputPath))
                    {
                        var existingContent = File.ReadAllText(outputPath);
                        if (existingContent == code)
                        {
                            shouldWrite = false;
                            Log.LogMessage(MessageImportance.Low, $"  Skipping (unchanged): {fileName}");
                        }
                    }

                    if (shouldWrite)
                    {
                        File.WriteAllText(outputPath, code);
                        Log.LogMessage(MessageImportance.High, $"  Generated: {fileName}");
                    }

                    generatedFileList.Add(new TaskItem(outputPath));
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error writing generated file {fileName}: {ex.Message}");
                }
            }

            GeneratedFiles = generatedFileList.ToArray();

            // 转换诊断消息
            var diagnostics = generator.Diagnostics.Select(d => CreateDiagnosticItem(d)).ToArray();
            Diagnostics = diagnostics;

            // 记录诊断消息
            foreach (var diagnostic in generator.Diagnostics)
            {
                switch (diagnostic.Level)
                {
                    case CodeGeneration.IProtoCodeGenerator.DiagnosticLevel.Info:
                        Log.LogMessage(MessageImportance.Normal, $"[{diagnostic.FilePath}] {diagnostic.Message}");
                        break;
                    case CodeGeneration.IProtoCodeGenerator.DiagnosticLevel.Warning:
                        Log.LogWarning($"[{diagnostic.FilePath}] {diagnostic.Message}");
                        break;
                    case CodeGeneration.IProtoCodeGenerator.DiagnosticLevel.Error:
                        Log.LogError($"[{diagnostic.FilePath}] {diagnostic.Message}");
                        break;
                }
            }

            return !Log.HasLoggedErrors;
        }
        catch (Exception ex)
        {
            Log.LogError($"Error in GenerateProtobufCode task: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 创建诊断消息项
    /// </summary>
    private ITaskItem CreateDiagnosticItem(CodeGeneration.IProtoCodeGenerator.Diagnostic diagnostic)
    {
        var item = new TaskItem(diagnostic.FilePath);
        item.SetMetadata("Level", diagnostic.Level.ToString());
        item.SetMetadata("Message", diagnostic.Message);
        return item;
    }
}
