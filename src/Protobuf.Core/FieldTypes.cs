namespace Protobuf.Core;

/// <summary>
/// Protobuf 字段数据类型
/// </summary>
public enum FieldType
{
    /// <summary>浮点数类型（32位）</summary>
    Float = 1,

    /// <summary>双精度浮点数类型（64位）</summary>
    Double = 2,

    /// <summary>整数类型（32位）</summary>
    Int32 = 3,

    /// <summary>长整数类型（64位）</summary>
    Int64 = 4,

    /// <summary>无符号整数类型（32位）</summary>
    UInt32 = 5,

    /// <summary>无符号长整数类型（64位）</summary>
    UInt64 = 6,

    /// <summary>有符号整数类型（32位）</summary>
    SInt32 = 7,

    /// <summary>有符号长整数类型（64位）</summary>
    SInt64 = 8,

    /// <summary>32位固定长度整数</summary>
    Fixed32 = 9,

    /// <summary>64位固定长度整数</summary>
    Fixed64 = 10,

    /// <summary>有符号32位固定长度整数</summary>
    SFixed32 = 11,

    /// <summary>有符号64位固定长度整数</summary>
    SFixed64 = 12,

    /// <summary>布尔类型</summary>
    Bool = 13,

    /// <summary>字符串类型</summary>
    String = 14,

    /// <summary>字节数组类型</summary>
    Bytes = 15,

    /// <summary>消息类型</summary>
    Message = 16,

    /// <summary>枚举类型</summary>
    Enum = 17
}

/// <summary>
/// Protobuf Wire Format 类型
/// </summary>
public enum WireType
{
    /// <summary>Varint 编码</summary>
    Varint = 0,

    /// <summary>64位固定长度（用于 double, fixed64, sfixed64）</summary>
    Fixed64 = 1,

    /// <summary>长度前缀（用于 string, bytes, 嵌套消息, packed repeated）</summary>
    LengthDelimited = 2,

    /// <summary>起始组（已废弃，protobuf3 中不使用）</summary>
    StartGroup = 3,

    /// <summary>结束组（已废弃，protobuf3 中不使用）</summary>
    EndGroup = 4,

    /// <summary>32位固定长度（用于 float, fixed32, sfixed32）</summary>
    Fixed32 = 5
}

/// <summary>
/// 字段标签选项
/// </summary>
[Flags]
public enum FieldLabel
{
    /// <summary>无标签（proto3 中的可选字段）</summary>
    None = 0,

    /// <summary>必需字段（proto2 中使用）</summary>
    Required = 1,

    /// <summary>可选字段（proto2 中使用）</summary>
    Optional = 2,

    /// <summary>重复字段</summary>
    Repeated = 4,

    /// <summary>Oneof 字段</summary>
    OneOf = 8
}
