using System.Linq;
using System.Collections.Generic;
using Protobuf.Core;
using Protobuf.Parser;

namespace Protobuf.CodeGeneration.CodeTemplates;

/// <summary>
/// Message 代码生成模板
/// </summary>
internal sealed class MessageTemplate : TemplateBase
{
    private readonly MessageDeclaration _message;
    private readonly string? _namespace;
    private readonly ProtoFile _protoFile;
    private readonly Dictionary<string, ProtoFile> _allProtoFiles;

    public MessageTemplate(MessageDeclaration message, string? namespaze, ProtoFile protoFile, Dictionary<string, ProtoFile> allProtoFiles)
    {
        _message = message;
        _namespace = namespaze;
        _protoFile = protoFile;
        _allProtoFiles = allProtoFiles ?? new Dictionary<string, ProtoFile>();
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
        AddLine($"/// {_message.Name} message");
        AddLine("/// </summary>");

        // 生成 partial class 声明
        AddLine($"public partial class {_message.Name} : Protobuf.Core.IMessage<{_message.Name}>");
        AddLineNoIndent("{");
        Indent();

        // 生成私有字段
        GeneratePrivateFields();

        // 生成属性（带 get; set;）
        GeneratePropertiesWithSetter();

        GenerateHasMethods();
        GenerateSetters();
        GenerateClearMethods();
        GenerateOneOfCode();

        // 生成构造函数
        GenerateConstructors();

        // 生成 IMessage 接口实现
        GenerateInterfaceImplementations();

        // 生成序列化方法
        GenerateSerializationMethods();

        // 生成嵌套消息类型的读取辅助方法
        GenerateNestedMessageReaders();

        // 生成 JSON 支持
        GenerateJsonSupport();

        Outdent();
        AddLineNoIndent("}");

        // 生成嵌套的 enum（在类内部）
        foreach (var enumDecl in _message.Enums)
        {
            var enumTemplate = new EnumTemplate(enumDecl, null);
            var enumCode = enumTemplate.Generate();
            AddLineNoIndent(enumCode);
        }

        // 生成嵌套的 message（在类内部）
        foreach (var nestedMessage in _message.NestedMessages)
        {
            var messageTemplate = new MessageTemplate(nestedMessage, null, _protoFile, _allProtoFiles);
            var messageCode = messageTemplate.Generate();
            AddLineNoIndent(messageCode);
        }

        if (!string.IsNullOrEmpty(_namespace))
        {
            Outdent();
            AddLineNoIndent("}");
        }

        return _sb.ToString();
    }

    /// <summary>
    /// 生成私有字段
    /// </summary>
    private void GeneratePrivateFields()
    {
        AddLine("#region Private Fields");
        AddLine();

        foreach (var field in _message.Fields)
        {
            if (field.IsMap)
            {
                var keyType = GetMapKeyType(field);
                var valueType = GetMapValueType(field);
                AddLine($"private Dictionary<{keyType}, {valueType}> _{GetFieldName(field)} = new();");
            }
            else if (field.Label == FieldLabel.Repeated)
            {
                var elementType = GetElementType(field);
                AddLine($"private List<{elementType}> _{GetFieldName(field)} = new();");
            }
            else
            {
                var csharpType = GetCSharpTypeForField(field);
                var defaultValue = GetDefaultValueForField(field);
                AddLine($"private {csharpType} _{GetFieldName(field)} = {defaultValue};");
            }
        }

        AddLine();
        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成属性（带 get; set;）
    /// </summary>
    private void GeneratePropertiesWithSetter()
    {
        AddLine("#region Properties");
        AddLine();

        foreach (var field in _message.Fields)
        {
            if (field.IsMap)
            {
                var keyType = GetMapKeyType(field);
                var valueType = GetMapValueType(field);
                AddLine($"public Dictionary<{keyType}, {valueType}> {ToPascalCase(field.Name)}");
                AddLineNoIndent("{");
                Indent();
                AddLine($"get => _{GetFieldName(field)};");
                AddLine($"set => _{GetFieldName(field)} = value ?? new();");
                Outdent();
                AddLineNoIndent("}");
            }
            else if (field.Label == FieldLabel.Repeated)
            {
                var elementType = GetElementType(field);
                AddLine($"public List<{elementType}> {ToPascalCase(field.Name)}");
                AddLineNoIndent("{");
                Indent();
                AddLine($"get => _{GetFieldName(field)};");
                AddLine($"set => _{GetFieldName(field)} = value ?? new();");
                Outdent();
                AddLineNoIndent("}");
            }
            else if (field.IsOneOf)
            {
                // oneof 字段的属性在 oneof 部分生成
                continue;
            }
            else
            {
                var csharpType = GetCSharpTypeForField(field);
                AddLine($"public {csharpType} {ToPascalCase(field.Name)}");
                AddLineNoIndent("{");
                Indent();
                AddLine($"get => _{GetFieldName(field)};");
                AddLine($"set => _{GetFieldName(field)} = {GetAssignmentValue(field, "value")};");
                Outdent();
                AddLineNoIndent("}");
            }
            AddLine();
        }

        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成构造函数
    /// </summary>
    private void GenerateConstructors()
    {
        AddLine("#region Constructors");
        AddLine();

        // 空构造函数
        AddLine($"public {_message.Name}()");
        AddLineNoIndent("{");
        AddLineNoIndent("}");
        AddLine();

        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成序列化方法
    /// </summary>
    private void GenerateSerializationMethods()
    {
        AddLine("#region Serialization");
        AddLine();

        // ToByteArray 方法
        AddLine("public byte[] ToByteArray()");
        AddLineNoIndent("{");
        Indent();
        AddLine("using (var ms = new System.IO.MemoryStream())");
        AddLineNoIndent("{");
        Indent();
        AddLine("var output = new Protobuf.Core.CodedOutputStream(ms);");
        AddLine("WriteTo(output);");
        AddLine("output.Flush();");
        AddLine("return ms.ToArray();");
        Outdent();
        AddLineNoIndent("}");
        Outdent();
        AddLineNoIndent("}");
        AddLine();

        // Parser 从字节数组
        AddLine($"public static {_message.Name} FromByteArray(byte[] data)");
        AddLineNoIndent("{");
        Indent();
        AddLine($"var message = new {_message.Name}();");
        AddLine("var input = new Protobuf.Core.CodedInputStream(new System.IO.MemoryStream(data));");
        AddLine("message.MergeFrom(input);");
        AddLine("return message;");
        Outdent();
        AddLineNoIndent("}");
        AddLine();

        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成 has 方法
    /// </summary>
    private void GenerateHasMethods()
    {
        AddLine("#region Has Methods");
        AddLine();

        foreach (var field in _message.Fields)
        {
            if (field.IsMap)
            {
                AddLine($"public bool Has{ToPascalCase(field.Name)} => _{GetFieldName(field)}.Count > 0;");
            }
            else if (field.Label == FieldLabel.Repeated)
            {
                AddLine($"public bool Has{ToPascalCase(field.Name)} => _{GetFieldName(field)}.Count > 0;");
            }
            else if (field.IsOneOf)
            {
                var oneOfName = GetOneOfName(field);
                AddLine($"public bool Has{ToPascalCase(field.Name)} => _{ToCamelCase(oneOfName)}Case == {oneOfName}Case.{ToPascalCase(field.Name)};");
            }
            else
            {
                var condition = GetHasCondition(field);
                AddLine($"public bool Has{ToPascalCase(field.Name)} => {condition};");
            }
        }

        AddLine();
        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成属性访问器
    /// </summary>
    private void GenerateProperties()
    {
        AddLine("#region Properties");
        AddLine();

        foreach (var field in _message.Fields)
        {
            if (field.IsMap)
            {
                var keyType = GetMapKeyType(field);
                var valueType = GetMapValueType(field);
                AddLine($"public IReadOnlyDictionary<{keyType}, {valueType}> {ToPascalCase(field.Name)} => _{GetFieldName(field)};");
            }
            else if (field.Label == FieldLabel.Repeated)
            {
                var elementType = GetElementType(field);
                AddLine($"public IReadOnlyList<{elementType}> {ToPascalCase(field.Name)} => _{GetFieldName(field)};");
            }
            else if (field.IsOneOf)
            {
                // oneof 字段的属性在 oneof 部分生成
                continue;
            }
            else
            {
                AddLine($"public {GetCSharpTypeForField(field)} {ToPascalCase(field.Name)} => _{GetFieldName(field)};");
            }
        }

        AddLine();
        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成 Setter 方法
    /// </summary>
    private void GenerateSetters()
    {
        AddLine("#region Setters");
        AddLine();

        foreach (var field in _message.Fields)
        {
            if (field.IsMap)
            {
                var keyType = GetMapKeyType(field);
                var valueType = GetMapValueType(field);
                AddLine($"public void Add{ToPascalCase(field.Name)}({keyType} key, {valueType} value)");
                AddLineNoIndent("{");
                Indent();
                AddLine($"_{GetFieldName(field)}[key] = value;");
                Outdent();
                AddLineNoIndent("}");
                AddLine();
            }
            else if (field.Label == FieldLabel.Repeated)
            {
                var elementType = GetElementType(field);
                AddLine($"public void Add{ToPascalCase(field.Name)}({elementType} value)");
                AddLineNoIndent("{");
                Indent();
                AddLine($"_{GetFieldName(field)}.Add(value);");
                Outdent();
                AddLineNoIndent("}");
                AddLine();
            }
            else if (field.IsOneOf)
            {
                // oneof 字段的 setter 在 oneof 部分生成
                continue;
            }
            else
            {
                AddLine($"public void Set{ToPascalCase(field.Name)}({GetCSharpTypeForField(field)} value)");
                AddLineNoIndent("{");
                Indent();
                AddLine($"_{GetFieldName(field)} = {GetAssignmentValue(field, "value")};");
                Outdent();
                AddLineNoIndent("}");
                AddLine();
            }
        }

        AddLine();
        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成 Clear 方法
    /// </summary>
    private void GenerateClearMethods()
    {
        AddLine("#region Clear Methods");
        AddLine();

        foreach (var field in _message.Fields)
        {
            if (field.IsMap)
            {
                AddLine($"public void Clear{ToPascalCase(field.Name)}()");
                AddLineNoIndent("{");
                Indent();
                AddLine($"_{GetFieldName(field)}.Clear();");
                Outdent();
                AddLineNoIndent("}");
                AddLine();
            }
            else if (field.Label == FieldLabel.Repeated)
            {
                AddLine($"public void Clear{ToPascalCase(field.Name)}()");
                AddLineNoIndent("{");
                Indent();
                AddLine($"_{GetFieldName(field)}.Clear();");
                Outdent();
                AddLineNoIndent("}");
                AddLine();
            }
            else if (field.IsOneOf)
            {
                // oneof 字段的 clear 在 oneof 部分生成
                continue;
            }
            else
            {
                AddLine($"public void Clear{ToPascalCase(field.Name)}()");
                AddLineNoIndent("{");
                Indent();
                AddLine($"_{GetFieldName(field)} = {GetDefaultValueForField(field)};");
                Outdent();
                AddLineNoIndent("}");
                AddLine();
            }
        }

        AddLine();
        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成 oneof 相关代码
    /// </summary>
    private void GenerateOneOfCode()
    {
        foreach (var oneOf in _message.OneOfs)
        {
            AddLine($"#region {ToPascalCase(oneOf.Name)} OneOf");
            AddLine();

            // 生成 oneof case 枚举
            AddLine($"public enum {ToPascalCase(oneOf.Name)}Case");
            AddLineNoIndent("{");
            Indent();
            AddLine("None,");
            foreach (var field in oneOf.Fields)
            {
                AddLine($"{ToPascalCase(field.Name)},");
            }
            Outdent();
            AddLineNoIndent("}");
            AddLine();

            // 生成 oneof case 字段
            AddLine($"private {ToPascalCase(oneOf.Name)}Case _{ToCamelCase(oneOf.Name)}Case = {ToPascalCase(oneOf.Name)}Case.None;");
            AddLine();

            // 生成 oneof case 属性
            AddLine($"public {ToPascalCase(oneOf.Name)}Case {ToPascalCase(oneOf.Name)}Case => _{ToCamelCase(oneOf.Name)}Case;");
            AddLine();

            // 生成 oneof 字段的属性和 setter
            foreach (var field in oneOf.Fields)
            {
                AddLine($"public {GetCSharpTypeForField(field)}? {ToPascalCase(field.Name)} => _{ToCamelCase(oneOf.Name)}Case == {ToPascalCase(oneOf.Name)}Case.{ToPascalCase(field.Name)} ? _{GetFieldName(field)} : null;");
                AddLine();

                AddLine($"public void Set{ToPascalCase(field.Name)}({GetCSharpTypeForField(field)} value)");
                AddLineNoIndent("{");
                Indent();
                AddLine($"_{GetFieldName(field)} = {GetAssignmentValue(field, "value")};");
                AddLine($"_{ToCamelCase(oneOf.Name)}Case = {ToPascalCase(oneOf.Name)}Case.{ToPascalCase(field.Name)};");
                Outdent();
                AddLineNoIndent("}");
                AddLine();
            }

            // 生成 oneof clear 方法
            AddLine($"public void Clear{ToPascalCase(oneOf.Name)}()");
            AddLineNoIndent("{");
            Indent();
            foreach (var field in oneOf.Fields)
            {
                AddLine($"_{GetFieldName(field)} = {GetDefaultValueForField(field)};");
            }
            AddLine($"_{ToCamelCase(oneOf.Name)}Case = {ToPascalCase(oneOf.Name)}Case.None;");
            Outdent();
            AddLineNoIndent("}");
            AddLine();

            AddLine("#endregion");
            AddLine();
        }
    }

    /// <summary>
    /// 生成 IMessage 接口实现
    /// </summary>
    private void GenerateInterfaceImplementations()
    {
        AddLine("#region IMessage Implementation");
        AddLine();

        // MergeFrom
        AddLine($"public void MergeFrom({_message.Name} other)");
        AddLineNoIndent("{");
        Indent();
        AddLine("if (other == null) return;");
        AddLine();
        foreach (var field in _message.Fields)
        {
            if (field.IsMap)
            {
                AddLine($"foreach (var kvp in other._{GetFieldName(field)})");
                AddLineNoIndent("{");
                Indent();
                AddLine($"_{GetFieldName(field)}[kvp.Key] = kvp.Value;");
                Outdent();
                AddLineNoIndent("}");
            }
            else if (field.Label == FieldLabel.Repeated)
            {
                AddLine($"_{GetFieldName(field)}.AddRange(other._{GetFieldName(field)});");
            }
            else if (field.IsOneOf)
            {
                AddLine($"if (other.Has{ToPascalCase(field.Name)})");
                AddLineNoIndent("{");
                Indent();
                AddLine($"Set{ToPascalCase(field.Name)}(other.{ToPascalCase(field.Name)});");
                Outdent();
                AddLineNoIndent("}");
            }
            else
            {
                AddLine($"if (other.Has{ToPascalCase(field.Name)})");
                AddLineNoIndent("{");
                Indent();
                AddLine($"Set{ToPascalCase(field.Name)}(other.{ToPascalCase(field.Name)});");
                Outdent();
                AddLineNoIndent("}");
            }
        }
        Outdent();
        AddLineNoIndent("}");
        AddLine();

        // MergeFrom(CodedInputStream)
        AddLine($"public void MergeFrom(Protobuf.Core.CodedInputStream input)");
        AddLineNoIndent("{");
        Indent();
        AddLine("while (true)");
        AddLineNoIndent("{");
        Indent();
        AddLine("var tag = input.ReadTag();");
        AddLine("if (tag == 0) break;");
        AddLine();
        AddLine("switch (tag)");
        AddLineNoIndent("{");
        Indent();
        foreach (var field in _message.Fields)
        {
            var wireType = GetWireType(field);
            AddLine($"case {WireFormat.MakeTag(field.FieldNumber, wireType)}:");
            Indent();
            GenerateMergeFromField(field);
            Outdent();
            AddLine("break;");
        }
        AddLine("default:");
        Indent();
        AddLine("input.SkipField(tag);");
        Outdent();
        AddLine("break;");
        Outdent();
        AddLineNoIndent("}");
        Outdent();
        AddLineNoIndent("}");
        Outdent();
        AddLineNoIndent("}");
        AddLine();

        // WriteTo
        AddLine($"public void WriteTo(Protobuf.Core.CodedOutputStream output)");
        AddLineNoIndent("{");
        Indent();
        foreach (var field in _message.Fields)
        {
            AddLine($"if (Has{ToPascalCase(field.Name)})");
            AddLineNoIndent("{");
            Indent();
            GenerateWriteField(field);
            Outdent();
            AddLineNoIndent("}");
        }
        Outdent();
        AddLineNoIndent("}");
        AddLine();

        // CalculateSize
        AddLine("public int CalculateSize()");
        AddLineNoIndent("{");
        Indent();
        AddLine("int size = 0;");
        foreach (var field in _message.Fields)
        {
            AddLine($"if (Has{ToPascalCase(field.Name)})");
            AddLineNoIndent("{");
            Indent();
            GenerateCalculateSizeField(field);
            Outdent();
            AddLineNoIndent("}");
        }
        AddLine("return size;");
        Outdent();
        AddLineNoIndent("}");
        AddLine();

        // IsInitialized
        AddLine("public bool IsInitialized() => true;");
        AddLine();

        // Clone
        AddLine($"public {_message.Name} Clone()");
        AddLineNoIndent("{");
        Indent();
        AddLine($"var clone = new {_message.Name}();");
        AddLine("clone.MergeFrom(this);");
        AddLine("return clone;");
        Outdent();
        AddLineNoIndent("}");
        AddLine();

        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成 Parser
    /// </summary>
    private void GenerateParser()
    {
        AddLine("#region Parser");
        AddLine();
        AddLine($"public static readonly Protobuf.Core.MessageParser<{_message.Name}> Parser =");
        AddLine($"    new Protobuf.Core.MessageParser<{_message.Name}>(() => new {_message.Name}());");
        AddLine();
        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成嵌套消息类型的读取辅助方法
    /// </summary>
    private void GenerateNestedMessageReaders()
    {
        // 收集所有嵌套消息类型
        var nestedTypes = new List<string>();

        foreach (var field in _message.Fields)
        {
            if (!string.IsNullOrEmpty(field.TypeName) && !nestedTypes.Contains(field.TypeName))
            {
                nestedTypes.Add(field.TypeName);
            }
        }

        if (nestedTypes.Count == 0)
        {
            return;
        }

        AddLine("#region Nested Message Readers");
        AddLine();

        foreach (var typeName in nestedTypes)
        {
            var methodName = $"Read{ToValidMethodName(typeName)}Message";
            AddLine($"private static {typeName} {methodName}(Protobuf.Core.CodedInputStream input)");
            AddLineNoIndent("{");
            Indent();

            // 生成读取嵌套消息的代码
            AddLine($"var message = new {typeName}();");
            AddLine("var length = (int)input.ReadRawVarint32();");
            AddLine("var oldLimit = input.SetRecursionLimit(length);");
            AddLine("try");
            AddLineNoIndent("{");
            Indent();
            AddLine("message.MergeFrom(input);");
            Outdent();
            AddLineNoIndent("}");
            AddLine("finally");
            AddLineNoIndent("{");
            Indent();
            AddLine("input.ResetRecursionLimit(oldLimit);");
            Outdent();
            AddLineNoIndent("}");
            AddLine("return message;");

            Outdent();
            AddLineNoIndent("}");
            AddLine();
        }

        AddLine("#endregion");
        AddLine();
    }

    /// <summary>
    /// 生成 JSON 序列化支持
    /// </summary>
    private void GenerateJsonSupport()
    {
        AddLine("#region JSON Support");
        AddLine();

        // ToJson 方法
        AddLine("public string ToJson()");
        AddLineNoIndent("{");
        Indent();
        AddLine("return System.Text.Json.JsonSerializer.Serialize(this,");
        AddLine("    new System.Text.Json.JsonSerializerOptions");
        AddLine("    {");
        AddLine("        PropertyNamingPolicy = null, // PascalCase");
        AddLine("        WriteIndented = false");
        AddLine("    });");
        Outdent();
        AddLineNoIndent("}");
        AddLine();

        AddLine("#endregion");
        AddLine();
    }

    // 辅助方法

    /// <summary>
    /// 将类型名转换为有效的方法名（移除点号）
    /// 例如：Example.Common.Timestamp -> Example_CommonTimestamp
    /// </summary>
    private string ToValidMethodName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return typeName;

        // 将点号替换为下划线
        return typeName.Replace(".", "_");
    }

    private string GetFieldName(FieldDeclaration field)
    {
        return ToCamelCase(field.Name);
    }

    private string GetCSharpTypeForField(FieldDeclaration field)
    {
        if (field.TypeName != null)
        {
            return ResolveTypeName(field.TypeName);
        }
        return GetCSharpType(field.Type);
    }

    private string GetDefaultValueForField(FieldDeclaration field)
    {
        if (field.TypeName != null)
        {
            return "default";
        }
        if (field.Type == FieldType.String)
        {
            return "\"\"";
        }
        return GetDefaultValue(field.Type);
    }

    private string GetAssignmentValue(FieldDeclaration field, string value)
    {
        if (field.Type == FieldType.String)
        {
            return $"({value} ?? \"\")";
        }
        return value;
    }

    private string GetHasCondition(FieldDeclaration field)
    {
        if (field.Type == FieldType.String)
        {
            return $"_{GetFieldName(field)} != \"\"";
        }
        if (field.Type == FieldType.Bytes)
        {
            return $"_{GetFieldName(field)} != null && _{GetFieldName(field)}.Length > 0";
        }
        return $"_{GetFieldName(field)} != {GetDefaultValueForField(field)}";
    }

    private string GetMapKeyType(FieldDeclaration field)
    {
        if (field.MapKeyType?.TypeName != null)
        {
            return field.MapKeyType.TypeName;
        }
        return GetCSharpType(field.MapKeyType!.Type);
    }

    private string GetMapValueType(FieldDeclaration field)
    {
        if (field.MapValueType?.TypeName != null)
        {
            return field.MapValueType.TypeName;
        }
        return GetCSharpType(field.MapValueType!.Type);
    }

    private string GetElementType(FieldDeclaration field)
    {
        if (field.TypeName != null)
        {
            return ResolveTypeName(field.TypeName);
        }
        return GetCSharpType(field.Type);
    }

    private int GetWireType(FieldDeclaration field)
    {
        if (field.IsMap)
        {
            return 2; // LengthDelimited
        }
        if (field.Label == FieldLabel.Repeated)
        {
            // 使用 packed 编码
            return 2;
        }
        return field.Type switch
        {
            FieldType.Double or FieldType.Fixed64 => 1,
            FieldType.Float or FieldType.Fixed32 => 5,
            FieldType.String or FieldType.Bytes or FieldType.Message => 2,
            _ => 0 // Varint
        };
    }

    /// <summary>
    /// 解析类型名称，包括从导入的文件中查找
    /// </summary>
    private string ResolveTypeName(string typeName)
    {
        // 首先检查当前文件中是否定义了该类型
        if (IsTypeDefinedInCurrentFile(typeName))
        {
            return typeName;
        }

        // 检查导入的文件中是否定义了该类型
        foreach (var import in _protoFile.Imports)
        {
            var importPath = ResolveImportPath(import.Path);
            if (importPath != null && _allProtoFiles.TryGetValue(importPath, out var importedFile))
            {
                if (IsTypeDefinedInFile(typeName, importedFile))
                {
                    // 如果导入的文件有 package，需要加上命名空间前缀
                    if (!string.IsNullOrEmpty(importedFile.Package))
                    {
                        return $"{importedFile.Package}.{typeName}";
                    }
                    return typeName;
                }
            }
        }

        // 未找到类型定义，返回原始类型名
        return typeName;
    }

    /// <summary>
    /// 检查类型是否在当前文件中定义
    /// </summary>
    private bool IsTypeDefinedInCurrentFile(string typeName)
    {
        return IsTypeDefinedInFile(typeName, _protoFile);
    }

    /// <summary>
    /// 检查类型是否在指定文件中定义
    /// </summary>
    private bool IsTypeDefinedInFile(string typeName, ProtoFile file)
    {
        // 检查顶层消息类型
        if (file.Messages.Any(m => m.Name == typeName))
        {
            return true;
        }

        // 检查枚举类型
        if (file.Enums.Any(e => e.Name == typeName))
        {
            return true;
        }

        // 检查嵌套消息类型
        foreach (var message in file.Messages)
        {
            if (HasNestedType(message, typeName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 递归检查嵌套类型
    /// </summary>
    private bool HasNestedType(MessageDeclaration message, string typeName)
    {
        // 检查直接嵌套的消息
        if (message.NestedMessages.Any(nm => nm.Name == typeName))
        {
            return true;
        }

        // 递归检查更深层次的嵌套
        foreach (var nestedMessage in message.NestedMessages)
        {
            if (HasNestedType(nestedMessage, typeName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 解析导入路径
    /// </summary>
    private string? ResolveImportPath(string importPath)
    {
        var currentDirectory = System.IO.Path.GetDirectoryName(_protoFile.FilePath);
        var relativePath = System.IO.Path.Combine(currentDirectory ?? "", importPath);
        relativePath = System.IO.Path.GetFullPath(relativePath);

        if (_allProtoFiles.ContainsKey(relativePath))
        {
            return relativePath;
        }

        if (_allProtoFiles.ContainsKey(importPath))
        {
            return importPath;
        }

        // 尝试在常见位置查找
        var projectRoot = System.IO.Path.GetDirectoryName(_protoFile.FilePath);
        while (projectRoot != null)
        {
            var testPath = System.IO.Path.Combine(projectRoot, importPath);
            if (_allProtoFiles.ContainsKey(testPath))
            {
                return testPath;
            }

            testPath = System.IO.Path.Combine(projectRoot, "proto", importPath);
            if (_allProtoFiles.ContainsKey(testPath))
            {
                return testPath;
            }

            projectRoot = System.IO.Path.GetDirectoryName(projectRoot);
        }

        return null;
    }

    /// <summary>
    /// 生成字段的 MergeFrom 代码
    /// </summary>
    private void GenerateMergeFromField(FieldDeclaration field)
    {
        var fieldName = GetFieldName(field);
        var pascalName = ToPascalCase(field.Name);

        if (field.IsMap)
        {
            // Map 字段：读取嵌套的 map entry 消息
            var keyType = GetMapKeyType(field);
            var valueType = GetMapValueType(field);
            AddLine("// Deserialize map entry message");
            AddLine($"var entryLength = input.ReadRawVarint32();");
            AddLine($"var oldLimit = input.PushLimit((int)entryLength);");
            AddLine($"try");
            AddLineNoIndent("{");
            Indent();
            AddLine($"{keyType} key = default;");
            AddLine($"{valueType} value = default;");
            AddLine($"bool hasKey = false;");
            AddLine($"bool hasValue = false;");
            AddLine();
            AddLine($"while (input.ReadTag() != 0)");
            AddLineNoIndent("{");
            Indent();
            AddLine($"var entryTag = input.LastTag;");
            AddLine($"switch (entryTag)");
            AddLineNoIndent("{");
            Indent();

            // Key case (field number 1)
            var keyWireType = GetWireTypeForType(field.MapKeyType!);
            AddLine($"case {WireFormat.MakeTag(1, keyWireType)}:");
            Indent();
            AddLine($"key = {ReadValueCodeForType(field.MapKeyType!, "input")};");
            AddLine("hasKey = true;");
            AddLine("break;");
            Outdent();

            // Value case (field number 2)
            var valueWireType = GetWireTypeForType(field.MapValueType!);
            AddLine($"case {WireFormat.MakeTag(2, valueWireType)}:");
            Indent();
            AddLine($"value = {ReadValueCodeForType(field.MapValueType!, "input")};");
            AddLine("hasValue = true;");
            AddLine("break;");
            Outdent();

            // Default case
            AddLine("default:");
            Indent();
            AddLine("input.SkipField(entryTag);");
            AddLine("break;");
            Outdent();

            Outdent(); // Close switch
            AddLineNoIndent("}"); // End switch
            Outdent(); // Close while
            AddLineNoIndent("}"); // End while
            AddLine();
            AddLine("// Add to dictionary if both key and value were read");
            AddLine("if (hasKey && hasValue)");
            AddLineNoIndent("{");
            Indent();
            AddLine($"_{fieldName}[key] = value;");
            Outdent();
            AddLineNoIndent("}");

            Outdent(); // Close try
            AddLineNoIndent("}"); // End try
            AddLine("finally");
            AddLineNoIndent("{");
            Indent();
            AddLine("input.PopLimit(oldLimit);");
            Outdent();
            AddLineNoIndent("}"); // End finally
        }
        else if (field.Label == FieldLabel.Repeated)
        {
            // Repeated 字段处理
            if (field.Type == FieldType.Message || field.Type == FieldType.String || field.Type == FieldType.Bytes)
            {
                AddLine($"_{fieldName}.Add({ReadValueCode(field, "input")});");
            }
            else
            {
                // 对于基础类型，总是作为单个元素处理（简化版）
                AddLine($"_{fieldName}.Add({ReadValueCode(field, "input")});");
            }
        }
        else if (field.IsOneOf)
        {
            // Oneof 字段处理
            AddLine($"Clear{GetOneOfName(field)}();");
            AddLine($"_{fieldName} = {ReadValueCode(field, "input")};");
            AddLine($"_{GetOneOfName(field)}Case = {GetOneOfName(field)}Case.{pascalName};");
        }
        else
        {
            // 普通字段
            AddLine($"Set{pascalName}({ReadValueCode(field, "input")});");
        }
    }

    /// <summary>
    /// 生成字段的 WriteTo 代码
    /// </summary>
    private void GenerateWriteField(FieldDeclaration field)
    {
        var fieldName = GetFieldName(field);
        var fieldNumber = field.FieldNumber;

        if (field.IsMap)
        {
            // Map 字段：每个键值对序列化为一个嵌套消息
            var keyType = GetMapKeyType(field);
            var valueType = GetMapValueType(field);
            AddLine($"// Serialize map field: {field.Name}");
            AddLine($"foreach (var kvp in _{fieldName})");
            AddLineNoIndent("{");
            Indent();

            // 计算并写入 map entry 的长度
            AddLine("// Calculate map entry size");
            AddLine($"int keySize = {CalculateSizeCode(field.MapKeyType!, "kvp.Key")};");
            AddLine($"int valueSize = {CalculateSizeCode(field.MapValueType!, "kvp.Value")};");
            AddLine("int entrySize = keySize + valueSize;");
            AddLine("entrySize += Protobuf.Core.SizeCalculator.ComputeUInt32Size((uint)keySize);");
            AddLine("entrySize += Protobuf.Core.SizeCalculator.ComputeUInt32Size((uint)valueSize);");
            AddLine();

            // 写入 tag 和长度
            AddLine($"output.WriteTag({WireFormat.MakeTag(fieldNumber, 2)});");
            AddLine("output.WriteRawVarint32((uint)entrySize);");
            AddLine();

            // 写入 key (field number 1)
            AddLine("// Write key (field 1)");
            AddLine($"output.WriteTag({WireFormat.MakeTag(1, GetWireTypeForType(field.MapKeyType!))});");
            AddLine($"{WriteValueCodeForType(field.MapKeyType!, "kvp.Key", "output")};");
            AddLine();

            // 写入 value (field number 2)
            AddLine("// Write value (field 2)");
            AddLine($"output.WriteTag({WireFormat.MakeTag(2, GetWireTypeForType(field.MapValueType!))});");
            AddLine($"{WriteValueCodeForType(field.MapValueType!, "kvp.Value", "output")};");

            Outdent();
            AddLineNoIndent("}");
        }
        else if (field.Label == FieldLabel.Repeated)
        {
            var elementType = GetElementType(field);

            if (field.Type == FieldType.Message || field.Type == FieldType.String || field.Type == FieldType.Bytes)
            {
                AddLine($"foreach (var item in _{fieldName})");
                AddLineNoIndent("{");
                Indent();
                AddLine($"output.WriteTag({WireFormat.MakeTag(fieldNumber, 2)});");
                AddLine($"{WriteValueCode(field, "item", "output")};");
                Outdent();
                AddLineNoIndent("}");
            }
            else
            {
                // 简化的 packed repeated（先实现基础版本）
                AddLine($"foreach (var item in _{fieldName})");
                AddLineNoIndent("{");
                Indent();
                AddLine($"output.WriteTag({WireFormat.MakeTag(fieldNumber, GetWireType(field))});");
                AddLine($"{WriteValueCode(field, "item", "output")};");
                Outdent();
                AddLineNoIndent("}");
            }
        }
        else if (field.IsOneOf)
        {
            AddLine($"output.WriteTag({WireFormat.MakeTag(fieldNumber, GetWireType(field))});");
            AddLine($"{WriteValueCode(field, $"_{fieldName}", "output")};");
        }
        else
        {
            AddLine($"output.WriteTag({WireFormat.MakeTag(fieldNumber, GetWireType(field))});");
            AddLine($"{WriteValueCode(field, $"_{fieldName}", "output")};");
        }
    }

    /// <summary>
    /// 生成字段的 CalculateSize 代码
    /// </summary>
    private void GenerateCalculateSizeField(FieldDeclaration field)
    {
        var fieldName = GetFieldName(field);
        var fieldNumber = field.FieldNumber;

        if (field.IsMap)
        {
            // Map 字段：计算每个键值对的序列化大小
            var keyType = GetMapKeyType(field);
            var valueType = GetMapValueType(field);
            AddLine($"// Calculate size for map field: {field.Name}");
            AddLine($"foreach (var kvp in _{fieldName})");
            AddLineNoIndent("{");
            Indent();

            // 计算 map entry 大小
            AddLine("int keySize = 0;");
            AddLine("int valueSize = 0;");
            AddLine($"keySize = {CalculateSizeCodeForType(field.MapKeyType!, "kvp.Key")};");
            AddLine($"valueSize = {CalculateSizeCodeForType(field.MapValueType!, "kvp.Value")};");
            AddLine();

            // Entry 大小 = key tag + key size + value tag + value size
            AddLine("int entrySize = keySize + valueSize;");
            AddLine("entrySize += Protobuf.Core.SizeCalculator.ComputeUInt32Size(1); // key tag");
            AddLine("entrySize += Protobuf.Core.SizeCalculator.ComputeUInt32Size(2); // value tag");
            AddLine("entrySize += Protobuf.Core.SizeCalculator.ComputeUInt32Size((uint)keySize); // key length");
            AddLine("entrySize += Protobuf.Core.SizeCalculator.ComputeUInt32Size((uint)valueSize); // value length");
            AddLine();

            // 加上 map entry 的 tag 和长度
            AddLine($"size += Protobuf.Core.SizeCalculator.ComputeTagSize({fieldNumber});");
            AddLine("size += Protobuf.Core.SizeCalculator.ComputeUInt32Size((uint)entrySize);");
            AddLine("size += entrySize;");

            Outdent();
            AddLineNoIndent("}");
        }
        else if (field.Label == FieldLabel.Repeated)
        {
            if (field.Type == FieldType.Message || field.Type == FieldType.String || field.Type == FieldType.Bytes)
            {
                AddLine($"foreach (var item in _{fieldName})");
                AddLineNoIndent("{");
                Indent();
                AddLine($"size += Protobuf.Core.SizeCalculator.ComputeTagSize({fieldNumber});");
                AddLine($"size += {CalculateSizeCode(field, "item")};");
                Outdent();
                AddLineNoIndent("}");
            }
            else
            {
                // 简化版本：每个元素都有 tag
                AddLine($"foreach (var item in _{fieldName})");
                AddLineNoIndent("{");
                Indent();
                AddLine($"size += Protobuf.Core.SizeCalculator.ComputeTagSize({fieldNumber});");
                AddLine($"size += {CalculateSizeCode(field, "item")};");
                Outdent();
                AddLineNoIndent("}");
            }
        }
        else if (field.IsOneOf)
        {
            AddLine($"size += Protobuf.Core.SizeCalculator.ComputeTagSize({fieldNumber});");
            AddLine($"size += {CalculateSizeCode(field, $"_{fieldName}")};");
        }
        else
        {
            AddLine($"size += Protobuf.Core.SizeCalculator.ComputeTagSize({fieldNumber});");
            AddLine($"size += {CalculateSizeCode(field, $"_{fieldName}")};");
        }
    }

    /// <summary>
    /// 生成读取字段值的代码
    /// </summary>
    private string ReadValueCode(FieldDeclaration? field, string inputVar)
    {
        if (field == null) return "default";

        if (field.TypeName != null)
        {
            var methodName = ToValidMethodName(field.TypeName);
            return $"Read{methodName}Message({inputVar})";
        }

        return field.Type switch
        {
            FieldType.Double => $"{inputVar}.ReadDouble()",
            FieldType.Float => $"{inputVar}.ReadFloat()",
            FieldType.Int32 => $"{inputVar}.ReadInt32()",
            FieldType.Int64 => $"{inputVar}.ReadInt64()",
            FieldType.UInt32 => $"{inputVar}.ReadUInt32()",
            FieldType.UInt64 => $"{inputVar}.ReadUInt64()",
            FieldType.SInt32 => $"{inputVar}.ReadSInt32()",
            FieldType.SInt64 => $"{inputVar}.ReadSInt64()",
            FieldType.Fixed32 => $"{inputVar}.ReadFixed32()",
            FieldType.Fixed64 => $"{inputVar}.ReadFixed64()",
            FieldType.SFixed32 => $"{inputVar}.ReadSFixed32()",
            FieldType.SFixed64 => $"{inputVar}.ReadSFixed64()",
            FieldType.Bool => $"{inputVar}.ReadBool()",
            FieldType.String => $"{inputVar}.ReadString()",
            FieldType.Bytes => $"{inputVar}.ReadBytes()",
            FieldType.Enum => $"{inputVar}.ReadEnum()",
            _ => "default"
        };
    }

    /// <summary>
    /// 生成写入字段值的代码
    /// </summary>
    private string WriteValueCode(FieldDeclaration field, string valueVar, string outputVar)
    {
        if (field.TypeName != null)
        {
            return $"{outputVar}.WriteMessage({valueVar})";
        }

        return field.Type switch
        {
            FieldType.Double => $"{outputVar}.WriteDouble({valueVar})",
            FieldType.Float => $"{outputVar}.WriteFloat({valueVar})",
            FieldType.Int32 => $"{outputVar}.WriteInt32({valueVar})",
            FieldType.Int64 => $"{outputVar}.WriteInt64({valueVar})",
            FieldType.UInt32 => $"{outputVar}.WriteUInt32({valueVar})",
            FieldType.UInt64 => $"{outputVar}.WriteUInt64({valueVar})",
            FieldType.SInt32 => $"{outputVar}.WriteSInt32({valueVar})",
            FieldType.SInt64 => $"{outputVar}.WriteSInt64({valueVar})",
            FieldType.Fixed32 => $"{outputVar}.WriteFixed32({valueVar})",
            FieldType.Fixed64 => $"{outputVar}.WriteFixed64({valueVar})",
            FieldType.SFixed32 => $"{outputVar}.WriteSFixed32({valueVar})",
            FieldType.SFixed64 => $"{outputVar}.WriteSFixed64({valueVar})",
            FieldType.Bool => $"{outputVar}.WriteBool({valueVar})",
            FieldType.String => $"{outputVar}.WriteString({valueVar})",
            FieldType.Bytes => $"{outputVar}.WriteBytes({valueVar})",
            FieldType.Enum => $"{outputVar}.WriteEnum({valueVar})",
            _ => $"// Unknown type: {field.Type}"
        };
    }

    /// <summary>
    /// 生成计算字段大小的代码
    /// </summary>
    private string CalculateSizeCode(FieldDeclaration field, string valueVar)
    {
        if (field.TypeName != null)
        {
            return $"Protobuf.Core.SizeCalculator.ComputeMessageSize({valueVar})";
        }

        return field.Type switch
        {
            FieldType.Double => "Protobuf.Core.SizeCalculator.ComputeDoubleSize(0.0)",
            FieldType.Float => "Protobuf.Core.SizeCalculator.ComputeFloatSize(0.0f)",
            FieldType.Int32 => $"Protobuf.Core.SizeCalculator.ComputeInt32Size({valueVar})",
            FieldType.Int64 => $"Protobuf.Core.SizeCalculator.ComputeInt64Size({valueVar})",
            FieldType.UInt32 => $"Protobuf.Core.SizeCalculator.ComputeUInt32Size({valueVar})",
            FieldType.UInt64 => $"Protobuf.Core.SizeCalculator.ComputeUInt64Size({valueVar})",
            FieldType.SInt32 => $"Protobuf.Core.SizeCalculator.ComputeSInt32Size({valueVar})",
            FieldType.SInt64 => $"Protobuf.Core.SizeCalculator.ComputeSInt64Size({valueVar})",
            FieldType.Fixed32 => "4",
            FieldType.Fixed64 => "8",
            FieldType.SFixed32 => "4",
            FieldType.SFixed64 => "8",
            FieldType.Bool => "Protobuf.Core.SizeCalculator.ComputeBoolSize(true)",
            FieldType.String => $"Protobuf.Core.SizeCalculator.ComputeStringSize({valueVar})",
            FieldType.Bytes => $"Protobuf.Core.SizeCalculator.ComputeBytesSize({valueVar})",
            FieldType.Enum => $"Protobuf.Core.SizeCalculator.ComputeEnumSize({valueVar})",
            _ => $"0 // Unknown type: {field.Type}"
        };
    }

    /// <summary>
    /// 获取字段所属的 oneof 名称
    /// </summary>
    private string GetOneOfName(FieldDeclaration field)
    {
        foreach (var oneOf in _message.OneOfs)
        {
            if (oneOf.Fields.Any(f => f.Name == field.Name))
            {
                return ToPascalCase(oneOf.Name);
            }
        }
        return "";
    }

    /// <summary>
    /// 获取指定类型的 WireType
    /// </summary>
    private int GetWireTypeForType(FieldDeclaration? fieldDecl)
    {
        if (fieldDecl == null) return 0;

        if (fieldDecl.TypeName != null)
        {
            return 2; // Message types are length-delimited
        }

        return fieldDecl.Type switch
        {
            FieldType.Double or FieldType.Fixed64 => 1,
            FieldType.Float or FieldType.Fixed32 => 5,
            FieldType.String or FieldType.Bytes or FieldType.Message => 2,
            _ => 0 // Varint
        };
    }

    /// <summary>
    /// 生成写入指定类型值的代码
    /// </summary>
    private string WriteValueCodeForType(FieldDeclaration fieldDecl, string valueVar, string outputVar)
    {
        if (fieldDecl.TypeName != null)
        {
            return $"{outputVar}.WriteMessage({valueVar})";
        }

        return fieldDecl.Type switch
        {
            FieldType.Double => $"{outputVar}.WriteDouble({valueVar})",
            FieldType.Float => $"{outputVar}.WriteFloat({valueVar})",
            FieldType.Int32 => $"{outputVar}.WriteInt32({valueVar})",
            FieldType.Int64 => $"{outputVar}.WriteInt64({valueVar})",
            FieldType.UInt32 => $"{outputVar}.WriteUInt32({valueVar})",
            FieldType.UInt64 => $"{outputVar}.WriteUInt64({valueVar})",
            FieldType.SInt32 => $"{outputVar}.WriteSInt32({valueVar})",
            FieldType.SInt64 => $"{outputVar}.WriteSInt64({valueVar})",
            FieldType.Fixed32 => $"{outputVar}.WriteFixed32({valueVar})",
            FieldType.Fixed64 => $"{outputVar}.WriteFixed64({valueVar})",
            FieldType.SFixed32 => $"{outputVar}.WriteSFixed32({valueVar})",
            FieldType.SFixed64 => $"{outputVar}.WriteSFixed64({valueVar})",
            FieldType.Bool => $"{outputVar}.WriteBool({valueVar})",
            FieldType.String => $"{outputVar}.WriteString({valueVar})",
            FieldType.Bytes => $"{outputVar}.WriteBytes({valueVar})",
            FieldType.Enum => $"{outputVar}.WriteEnum({valueVar})",
            _ => $"// Unknown type: {fieldDecl.Type}"
        };
    }

    /// <summary>
    /// 生成计算指定类型大小的代码
    /// </summary>
    private string CalculateSizeCodeForType(FieldDeclaration fieldDecl, string valueVar)
    {
        if (fieldDecl.TypeName != null)
        {
            return $"Protobuf.Core.SizeCalculator.ComputeMessageSize({valueVar})";
        }

        return fieldDecl.Type switch
        {
            FieldType.Double => "Protobuf.Core.SizeCalculator.ComputeDoubleSize(0.0)",
            FieldType.Float => "Protobuf.Core.SizeCalculator.ComputeFloatSize(0.0f)",
            FieldType.Int32 => $"Protobuf.Core.SizeCalculator.ComputeInt32Size({valueVar})",
            FieldType.Int64 => $"Protobuf.Core.SizeCalculator.ComputeInt64Size({valueVar})",
            FieldType.UInt32 => $"Protobuf.Core.SizeCalculator.ComputeUInt32Size({valueVar})",
            FieldType.UInt64 => $"Protobuf.Core.SizeCalculator.ComputeUInt64Size({valueVar})",
            FieldType.SInt32 => $"Protobuf.Core.SizeCalculator.ComputeSInt32Size({valueVar})",
            FieldType.SInt64 => $"Protobuf.Core.SizeCalculator.ComputeSInt64Size({valueVar})",
            FieldType.Fixed32 => "4",
            FieldType.Fixed64 => "8",
            FieldType.SFixed32 => "4",
            FieldType.SFixed64 => "8",
            FieldType.Bool => "Protobuf.Core.SizeCalculator.ComputeBoolSize(true)",
            FieldType.String => $"Protobuf.Core.SizeCalculator.ComputeStringSize({valueVar})",
            FieldType.Bytes => $"Protobuf.Core.SizeCalculator.ComputeBytesSize({valueVar})",
            FieldType.Enum => $"Protobuf.Core.SizeCalculator.ComputeEnumSize({valueVar})",
            _ => $"0 // Unknown type: {fieldDecl.Type}"
        };
    }

    /// <summary>
    /// 生成读取指定类型值的代码
    /// </summary>
    private string ReadValueCodeForType(FieldDeclaration fieldDecl, string inputVar)
    {
        if (fieldDecl.TypeName != null)
        {
            var methodName = ToValidMethodName(fieldDecl.TypeName);
            return $"Read{methodName}Message({inputVar})";
        }

        return fieldDecl.Type switch
        {
            FieldType.Double => $"{inputVar}.ReadDouble()",
            FieldType.Float => $"{inputVar}.ReadFloat()",
            FieldType.Int32 => $"{inputVar}.ReadInt32()",
            FieldType.Int64 => $"{inputVar}.ReadInt64()",
            FieldType.UInt32 => $"{inputVar}.ReadUInt32()",
            FieldType.UInt64 => $"{inputVar}.ReadUInt64()",
            FieldType.SInt32 => $"{inputVar}.ReadSInt32()",
            FieldType.SInt64 => $"{inputVar}.ReadSInt64()",
            FieldType.Fixed32 => $"{inputVar}.ReadFixed32()",
            FieldType.Fixed64 => $"{inputVar}.ReadFixed64()",
            FieldType.SFixed32 => $"{inputVar}.ReadSFixed32()",
            FieldType.SFixed64 => $"{inputVar}.ReadSFixed64()",
            FieldType.Bool => $"{inputVar}.ReadBool()",
            FieldType.String => $"{inputVar}.ReadString()",
            FieldType.Bytes => $"{inputVar}.ReadBytes()",
            FieldType.Enum => $"{inputVar}.ReadEnum()",
            _ => "default"
        };
    }
}
