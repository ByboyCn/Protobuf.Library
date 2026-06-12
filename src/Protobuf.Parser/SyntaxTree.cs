using Protobuf.Core;

namespace Protobuf.Parser;

/// <summary>
/// Proto 文件 AST
/// </summary>
public sealed class ProtoFile
{
    public string? Syntax { get; set; }
    public string? Package { get; set; }
    public List<Import> Imports { get; } = new();
    public List<MessageDeclaration> Messages { get; } = new();
    public List<EnumDeclaration> Enums { get; } = new();
    public List<OptionDeclaration> Options { get; } = new();
    public string? FilePath { get; set; }

    public ProtoFile(string? filePath = null)
    {
        FilePath = filePath;
    }
}

/// <summary>
/// Import 声明
/// </summary>
public sealed class Import
{
    public string Path { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsWeak { get; set; }
}

/// <summary>
/// Option 声明
/// </summary>
public sealed class OptionDeclaration
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public bool IsString { get; set; }
}

/// <summary>
/// Message 声明
/// </summary>
public sealed class MessageDeclaration
{
    public string Name { get; set; } = string.Empty;
    public List<FieldDeclaration> Fields { get; } = new();
    public List<EnumDeclaration> Enums { get; } = new();
    public List<MessageDeclaration> NestedMessages { get; } = new();
    public List<OneOfDeclaration> OneOfs { get; } = new();
    public List<OptionDeclaration> Options { get; } = new();
    public List<ReservedDeclaration> Reserveds { get; } = new();
}

/// <summary>
/// 字段声明
/// </summary>
public sealed class FieldDeclaration
{
    public string Name { get; set; } = string.Empty;
    public FieldType Type { get; set; }
    public string? TypeName { get; set; } // 自定义类型名称
    public int FieldNumber { get; set; }
    public FieldLabel Label { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsPacked { get; set; }
    public bool IsMap { get; set; }
    public bool IsOneOf { get; set; }
    public string? OneOfName { get; set; }
    public FieldDeclaration? MapKeyType { get; set; }
    public FieldDeclaration? MapValueType { get; set; }

    // JSON 映射选项
    public string? JsonName { get; set; }
}

/// <summary>
/// OneOf 声明
/// </summary>
public sealed class OneOfDeclaration
{
    public string Name { get; set; } = string.Empty;
    public List<FieldDeclaration> Fields { get; } = new();
}

/// <summary>
/// Reserved 声明
/// </summary>
public sealed class ReservedDeclaration
{
    public List<int>? Ranges { get; set; }
    public List<string>? FieldNames { get; set; }
}

/// <summary>
/// Enum 声明
/// </summary>
public sealed class EnumDeclaration
{
    public string Name { get; set; } = string.Empty;
    public List<EnumValueDeclaration> Values { get; } = new();
    public List<OptionDeclaration> Options { get; } = new();
}

/// <summary>
/// Enum 值声明
/// </summary>
public sealed class EnumValueDeclaration
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

/// <summary>
/// Service 声明（可选，用于 gRPC）
/// </summary>
public sealed class ServiceDeclaration
{
    public string Name { get; set; } = string.Empty;
    public List<RpcDeclaration> Rpcs { get; } = new();
    public List<OptionDeclaration> Options { get; } = new();
}

/// <summary>
/// RPC 声明
/// </summary>
public sealed class RpcDeclaration
{
    public string Name { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public bool RequestStream { get; set; }
    public bool ResponseStream { get; set; }
    public List<OptionDeclaration> Options { get; } = new();
}

/// <summary>
/// 扩展声明
/// </summary>
public sealed class ExtendDeclaration
{
    public string TypeName { get; set; } = string.Empty;
    public List<FieldDeclaration> Fields { get; } = new();
}
