using System.Collections.Immutable;

namespace Protobuf.Reflection;

/// <summary>
/// 消息描述符（AOT友好）
/// </summary>
public sealed class MessageDescriptor
{
    public string FullName { get; }
    public string Name { get; }
    public ImmutableArray<FieldDescriptor> Fields { get; }
    public ImmutableArray<MessageDescriptor> NestedMessages { get; }
    public ImmutableArray<EnumDescriptor> NestedEnums { get; }

    public MessageDescriptor(
        string fullName,
        string name,
        ImmutableArray<FieldDescriptor> fields,
        ImmutableArray<MessageDescriptor> nestedMessages = default,
        ImmutableArray<EnumDescriptor> nestedEnums = default)
    {
        FullName = fullName;
        Name = name;
        Fields = fields;
        NestedMessages = nestedMessages.IsEmpty ? ImmutableArray<MessageDescriptor>.Empty : nestedMessages;
        NestedEnums = nestedEnums.IsEmpty ? ImmutableArray<EnumDescriptor>.Empty : nestedEnums;
    }

    /// <summary>
    /// 根据字段名查找字段描述符
    /// </summary>
    public FieldDescriptor? FindFieldByName(string name)
    {
        foreach (var field in Fields)
        {
            if (field.Name == name)
                return field;
        }
        return null;
    }

    /// <summary>
    /// 根据字段编号查找字段描述符
    /// </summary>
    public FieldDescriptor? FindFieldByNumber(int number)
    {
        foreach (var field in Fields)
        {
            if (field.FieldNumber == number)
                return field;
        }
        return null;
    }
}

/// <summary>
/// 字段描述符（AOT友好）
/// </summary>
public sealed class FieldDescriptor
{
    public string Name { get; }
    public int FieldNumber { get; }
    public FieldType FieldType { get; }
    public string? TypeName { get; }
    public FieldLabel Label { get; }
    public bool IsMap { get; }
    public bool IsOneOf { get; }
    public string? OneOfName { get; }
    public FieldDescriptor? MapKeyType { get; }
    public FieldDescriptor? MapValueType { get; }

    public FieldDescriptor(
        string name,
        int fieldNumber,
        FieldType fieldType,
        string? typeName = null,
        FieldLabel label = FieldLabel.Optional,
        bool isMap = false,
        bool isOneOf = false,
        string? oneOfName = null,
        FieldDescriptor? mapKeyType = null,
        FieldDescriptor? mapValueType = null)
    {
        Name = name;
        FieldNumber = fieldNumber;
        FieldType = fieldType;
        TypeName = typeName;
        Label = label;
        IsMap = isMap;
        IsOneOf = isOneOf;
        OneOfName = oneOfName;
        MapKeyType = mapKeyType;
        MapValueType = mapValueType;
    }
}

/// <summary>
/// 枚举描述符（AOT友好）
/// </summary>
public sealed class EnumDescriptor
{
    public string FullName { get; }
    public string Name { get; }
    public ImmutableArray<EnumValueDescriptor> Values { get; }

    public EnumDescriptor(
        string fullName,
        string name,
        ImmutableArray<EnumValueDescriptor> values)
    {
        FullName = fullName;
        Name = name;
        Values = values;
    }

    /// <summary>
    /// 根据值查找枚举名称
    /// </summary>
    public string? FindNameByValue(int value)
    {
        foreach (var enumValue in Values)
        {
            if (enumValue.Value == value)
                return enumValue.Name;
        }
        return null;
    }
}

/// <summary>
/// 枚举值描述符（AOT友好）
/// </summary>
public sealed class EnumValueDescriptor
{
    public string Name { get; }
    public int Value { get; }

    public EnumValueDescriptor(string name, int value)
    {
        Name = name;
        Value = value;
    }
}

/// <summary>
/// 字段类型
/// </summary>
public enum FieldType
{
    Double, Float, Int64, UInt64, Int32, Fixed64, Fixed32,
    Bool, String, Group, Message, Bytes, UInt32, Enum,
    SFixed32, SFixed64, SInt32, SInt64
}

/// <summary>
/// 字段标签
/// </summary>
public enum FieldLabel
{
    Optional, Required, Repeated
}
