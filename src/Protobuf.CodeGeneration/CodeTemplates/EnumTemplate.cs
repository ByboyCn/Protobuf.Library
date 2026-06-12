using Protobuf.Parser;

namespace Protobuf.CodeGeneration.CodeTemplates;

/// <summary>
/// Enum 代码生成模板
/// </summary>
internal sealed class EnumTemplate : TemplateBase
{
    private readonly EnumDeclaration _enum;
    private readonly string? _namespace;

    public EnumTemplate(EnumDeclaration enumDecl, string? namespaze)
    {
        _enum = enumDecl;
        _namespace = namespaze;
    }

    public override string Generate()
    {
        // 生成命名空间
        if (!string.IsNullOrEmpty(_namespace))
        {
            AddLineNoIndent($"namespace {_namespace}");
            AddLineNoIndent("{");
            Indent();
        }

        // 生成 XML 文档注释
        AddLine("/// <summary>");
        AddLine($"/// {_enum.Name} enum");
        AddLine("/// </summary>");

        // 生成 enum 声明
        AddLine($"public enum {_enum.Name}");
        AddLineNoIndent("{");
        Indent();

        // 生成 enum 值
        for (int i = 0; i < _enum.Values.Count; i++)
        {
            var value = _enum.Values[i];
            AddLine($"/// <summary>");
            AddLine($"/// {value.Name} = {value.Value}");
            AddLine($"/// </summary>");
            AddLine($"{value.Name} = {value.Value}");

            if (i < _enum.Values.Count - 1)
            {
                AddLine(",");
            }
            else
            {
                AddLine();
            }
        }

        Outdent();
        AddLineNoIndent("}");

        if (!string.IsNullOrEmpty(_namespace))
        {
            Outdent();
            AddLineNoIndent("}");
        }

        return _sb.ToString();
    }
}
