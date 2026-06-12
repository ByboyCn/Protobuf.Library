using System;
using System.Text;
using Protobuf.Core;
using Protobuf.Parser;

namespace Protobuf.Generator.CodeTemplates;

/// <summary>
/// 代码生成模板基类
/// </summary>
internal abstract class TemplateBase
{
    protected readonly StringBuilder _sb = new();
    protected int _indentLevel = 0;
    protected const string IndentString = "    ";

    /// <summary>
    /// 生成代码
    /// </summary>
    public abstract string Generate();

    /// <summary>
    /// 添加缩进
    /// </summary>
    protected void AddIndent()
    {
        for (int i = 0; i < _indentLevel; i++)
        {
            _sb.Append(IndentString);
        }
    }

    /// <summary>
    /// 增加缩进级别
    /// </summary>
    protected void Indent()
    {
        _indentLevel++;
    }

    /// <summary>
    /// 减少缩进级别
    /// </summary>
    protected void Outdent()
    {
        if (_indentLevel > 0)
        {
            _indentLevel--;
        }
    }

    /// <summary>
    /// 添加行
    /// </summary>
    protected void AddLine(string? line = null)
    {
        AddIndent();
        if (line != null)
        {
            _sb.AppendLine(line);
        }
        else
        {
            _sb.AppendLine();
        }
    }

    /// <summary>
    /// 添加行（无缩进）
    /// </summary>
    protected void AddLineNoIndent(string line)
    {
        _sb.AppendLine(line);
    }

    /// <summary>
    /// 转换 PascalCase
    /// </summary>
    protected string ToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
        {
            return snakeCase;
        }

        var words = snakeCase.Split('_', StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                sb.Append(char.ToUpper(word[0]));
                if (word.Length > 1)
                {
                    sb.Append(word.Substring(1));
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 转换 camelCase
    /// </summary>
    protected string ToCamelCase(string snakeCase)
    {
        var pascalCase = ToPascalCase(snakeCase);
        if (string.IsNullOrEmpty(pascalCase))
        {
            return pascalCase;
        }

        return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
    }

    /// <summary>
    /// 获取 C# 类型名称
    /// </summary>
    protected string GetCSharpType(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Double => "double",
            FieldType.Float => "float",
            FieldType.Int32 => "int",
            FieldType.Int64 => "long",
            FieldType.UInt32 => "uint",
            FieldType.UInt64 => "ulong",
            FieldType.SInt32 => "int",
            FieldType.SInt64 => "long",
            FieldType.Fixed32 => "uint",
            FieldType.Fixed64 => "ulong",
            FieldType.SFixed32 => "int",
            FieldType.SFixed64 => "long",
            FieldType.Bool => "bool",
            FieldType.String => "string",
            FieldType.Bytes => "byte[]",
            FieldType.Message => "IMessage<T>", // 将在具体实现中处理
            FieldType.Enum => "int",
            _ => "object"
        };
    }

    /// <summary>
    /// 获取默认值
    /// </summary>
    protected string GetDefaultValue(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Double => "0.0",
            FieldType.Float => "0.0f",
            FieldType.Int32 => "0",
            FieldType.Int64 => "0L",
            FieldType.UInt32 => "0u",
            FieldType.UInt64 => "0ul",
            FieldType.SInt32 => "0",
            FieldType.SInt64 => "0L",
            FieldType.Fixed32 => "0u",
            FieldType.Fixed64 => "0ul",
            FieldType.SFixed32 => "0",
            FieldType.SFixed64 => "0L",
            FieldType.Bool => "false",
            FieldType.String => "\"\"",
            FieldType.Bytes => "Array.Empty<byte>()",
            _ => "default"
        };
    }
}
