using System.Collections.Generic;
using Protobuf.Parser;

namespace Protobuf.CodeGeneration;

/// <summary>
/// Protobuf 代码生成器接口
/// </summary>
public interface IProtoCodeGenerator
{
    /// <summary>
    /// 从 proto 文件生成 C# 代码
    /// </summary>
    /// <param name="protoFiles">proto 文件路径到内容的映射</param>
    /// <returns>生成的文件集合（文件名 -> 代码内容）</returns>
    Dictionary<string, string> GenerateCode(Dictionary<string, string> protoFiles);

    /// <summary>
    /// 诊断消息级别
    /// </summary>
    public enum DiagnosticLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// 诊断消息
    /// </summary>
    public class Diagnostic
    {
        public string FilePath { get; set; } = string.Empty;
        public DiagnosticLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 获取生成的诊断消息
    /// </summary>
    List<Diagnostic> Diagnostics { get; }
}
