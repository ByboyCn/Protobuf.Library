using System.Runtime.CompilerServices;

namespace Protobuf.Core;

/// <summary>
/// Protobuf Wire Format 工具类
/// </summary>
public static class WireFormat
{
    /// <summary>Varint 类型</summary>
    public const int VarintType = 0;

    /// <summary>64位固定长度类型</summary>
    public const int Fixed64Type = 1;

    /// <summary>长度前缀类型</summary>
    public const int LengthDelimitedType = 2;

    /// <summary>起始组类型（已废弃）</summary>
    public const int StartGroupType = 3;

    /// <summary>结束组类型（已废弃）</summary>
    public const int EndGroupType = 4;

    /// <summary>32位固定长度类型</summary>
    public const int Fixed32Type = 5;

    /// <summary>
    /// 创建 tag
    /// </summary>
    /// <param name="fieldNumber">字段编号</param>
    /// <param name="wireType">Wire 类型</param>
    /// <returns>tag 值</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MakeTag(int fieldNumber, int wireType)
    {
        return (uint)((fieldNumber << 3) | wireType);
    }

    /// <summary>
    /// 从 tag 获取字段编号
    /// </summary>
    /// <param name="tag">tag 值</param>
    /// <returns>字段编号</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetTagFieldNumber(uint tag)
    {
        return (int)(tag >> 3);
    }

    /// <summary>
    /// 从 tag 获取 Wire 类型
    /// </summary>
    /// <param name="tag">tag 值</param>
    /// <returns>Wire 类型</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetTagWireType(uint tag)
    {
        return (int)(tag & 0x7);
    }

    /// <summary>
    /// 获取字段类型的 Wire 类型
    /// </summary>
    /// <param name="fieldType">字段类型</param>
    /// <returns>Wire 类型</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WireType GetWireType(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Int32 or FieldType.Int64 or FieldType.UInt32 or FieldType.UInt64 or
            FieldType.SInt32 or FieldType.SInt64 or FieldType.Bool or FieldType.Enum =>
                WireType.Varint,

            FieldType.Fixed64 or FieldType.SFixed64 or FieldType.Double =>
                WireType.Fixed64,

            FieldType.Fixed32 or FieldType.SFixed32 or FieldType.Float =>
                WireType.Fixed32,

            FieldType.String or FieldType.Bytes or FieldType.Message =>
                WireType.LengthDelimited,

            _ => throw new ArgumentException($"Unknown field type: {fieldType}")
        };
    }
}
