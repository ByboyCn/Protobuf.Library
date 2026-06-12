using System.Text;
using Protobuf.Core;

namespace Protobuf.Parser;

/// <summary>
/// Proto 文件语法分析器
/// </summary>
public sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _position;
    private Token _currentToken = new Token(TokenType.EndOfFile, "", 0, 0, 0);
    private Token _nextToken = new Token(TokenType.EndOfFile, "", 0, 0, 0);

    public Parser(List<Token> tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        _position = 0;
        Advance();
        Advance(); // 预读取两个 token
    }

    /// <summary>
    /// 解析 Proto 文件
    /// </summary>
    public ProtoFile Parse()
    {
        var protoFile = new ProtoFile();

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            switch (_currentToken.Type)
            {
                case TokenType.Syntax:
                    ParseSyntax(protoFile);
                    break;

                case TokenType.Package:
                    ParsePackage(protoFile);
                    break;

                case TokenType.Import:
                    ParseImport(protoFile);
                    break;

                case TokenType.Option:
                    var option = ParseOption();
                    protoFile.Options.Add(option);
                    break;

                case TokenType.Message:
                    var message = ParseMessage();
                    protoFile.Messages.Add(message);
                    break;

                case TokenType.Enum:
                    var enumDecl = ParseEnum();
                    protoFile.Enums.Add(enumDecl);
                    break;

                case TokenType.Service:
                    // 跳过 service 声明（用于 gRPC，暂不实现）
                    SkipService();
                    break;

                case TokenType.Extend:
                    // 跳过 extend 声明
                    SkipExtend();
                    break;

                case TokenType.Comment:
                case TokenType.Whitespace:
                    Advance();
                    break;

                default:
                    throw NewParseException($"Unexpected token: {_currentToken.Type}");
            }
        }

        return protoFile;
    }

    /// <summary>
    /// 解析 syntax 声明
    /// </summary>
    private void ParseSyntax(ProtoFile protoFile)
    {
        Expect(TokenType.Syntax);
        Expect(TokenType.Equal);

        if (_currentToken.Type == TokenType.StringLiteral)
        {
            protoFile.Syntax = _currentToken.Value;
            Advance();
        }
        else if (_currentToken.Type == TokenType.Identifier && _currentToken.Value.Equals("proto3", StringComparison.OrdinalIgnoreCase))
        {
            protoFile.Syntax = "proto3";
            Advance();
        }
        else
        {
            throw NewParseException($"Expected syntax version, got: {_currentToken.Type}");
        }

        Expect(TokenType.SemiColon);
    }

    /// <summary>
    /// 解析 package 声明
    /// </summary>
    private void ParsePackage(ProtoFile protoFile)
    {
        Expect(TokenType.Package);

        var packageName = new System.Text.StringBuilder();

        // 支持带点的包名（如 Example.Common）
        while (_currentToken.Type == TokenType.Identifier || _currentToken.Type == TokenType.Dot)
        {
            packageName.Append(_currentToken.Value);
            Advance();
        }

        if (packageName.Length > 0)
        {
            protoFile.Package = packageName.ToString();
        }
        else
        {
            throw NewParseException($"Expected package name, got: {_currentToken.Type}");
        }

        Expect(TokenType.SemiColon);
    }

    /// <summary>
    /// 解析 import 声明
    /// </summary>
    private void ParseImport(ProtoFile protoFile)
    {
        Expect(TokenType.Import);

        bool isPublic = false;
        bool isWeak = false;

        if (_currentToken.Type == TokenType.Identifier)
        {
            if (_currentToken.Value.Equals("public", StringComparison.OrdinalIgnoreCase))
            {
                isPublic = true;
                Advance();
            }
            else if (_currentToken.Value.Equals("weak", StringComparison.OrdinalIgnoreCase))
            {
                isWeak = true;
                Advance();
            }
        }

        if (_currentToken.Type == TokenType.StringLiteral)
        {
            var import = new Import
            {
                Path = _currentToken.Value,
                IsPublic = isPublic,
                IsWeak = isWeak
            };
            protoFile.Imports.Add(import);
            Advance();
        }
        else
        {
            throw NewParseException($"Expected import path, got: {_currentToken.Type}");
        }

        Expect(TokenType.SemiColon);
    }

    /// <summary>
    /// 解析 option 声明
    /// </summary>
    private OptionDeclaration ParseOption()
    {
        Expect(TokenType.Option);

        var option = new OptionDeclaration();
        option.Name = ParseOptionName();

        if (_currentToken.Type == TokenType.Equal)
        {
            Advance();

            if (_currentToken.Type == TokenType.StringLiteral)
            {
                option.Value = _currentToken.Value;
                option.IsString = true;
                Advance();
            }
            else if (_currentToken.Type == TokenType.BooleanLiteral)
            {
                option.Value = _currentToken.Value;
                option.IsString = false;
                Advance();
            }
            else if (_currentToken.Type == TokenType.Identifier || _currentToken.Type == TokenType.IntegerLiteral)
            {
                option.Value = _currentToken.Value;
                option.IsString = false;
                Advance();
            }
            else
            {
                throw NewParseException($"Expected option value, got: {_currentToken.Type}");
            }
        }

        Expect(TokenType.SemiColon);
        return option;
    }

    /// <summary>
    /// 解析选项名称
    /// </summary>
    private string ParseOptionName()
    {
        StringBuilder sb = new();

        while (true)
        {
            if (_currentToken.Type == TokenType.Identifier)
            {
                sb.Append(_currentToken.Value);
                Advance();
            }
            else if (_currentToken.Type == TokenType.Dot)
            {
                sb.Append('.');
                Advance();
            }
            else
            {
                break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 解析 message 声明
    /// </summary>
    private MessageDeclaration ParseMessage()
    {
        Expect(TokenType.Message);

        var message = new MessageDeclaration();

        if (_currentToken.Type == TokenType.Identifier)
        {
            message.Name = _currentToken.Value;
            Advance();
        }
        else
        {
            throw NewParseException($"Expected message name, got: {_currentToken.Type}");
        }

        Expect(TokenType.LeftBrace);

        while (_currentToken.Type != TokenType.RightBrace && _currentToken.Type != TokenType.EndOfFile)
        {
            switch (_currentToken.Type)
            {
                case TokenType.Enum:
                    var enumDecl = ParseEnum();
                    message.Enums.Add(enumDecl);
                    break;

                case TokenType.Message:
                    var nestedMessage = ParseMessage();
                    message.NestedMessages.Add(nestedMessage);
                    break;

                case TokenType.Oneof:
                    var oneOf = ParseOneOf();
                    message.OneOfs.Add(oneOf);
                    break;

                case TokenType.Map:
                    var mapField = ParseMapField();
                    message.Fields.Add(mapField);
                    break;

                case TokenType.Reserved:
                    var reserved = ParseReserved();
                    message.Reserveds.Add(reserved);
                    break;

                case TokenType.Option:
                    var option = ParseOption();
                    message.Options.Add(option);
                    break;

                case TokenType.Comment:
                case TokenType.Whitespace:
                    Advance();
                    break;

                default:
                    // 尝试解析为普通字段
                    var field = ParseField();
                    if (field != null)
                    {
                        message.Fields.Add(field);
                    }
                    break;
            }
        }

        Expect(TokenType.RightBrace);
        return message;
    }

    /// <summary>
    /// 解析字段声明
    /// </summary>
    private FieldDeclaration? ParseField()
    {
        var field = new FieldDeclaration();

        // 解析标签（proto3 中通常是 optional 或 repeated）
        if (_currentToken.Type == TokenType.Repeated)
        {
            field.Label = FieldLabel.Repeated;
            Advance();
        }
        else if (_currentToken.Type == TokenType.Optional)
        {
            field.Label = FieldLabel.Optional;
            Advance();
        }

        // 解析类型
        string? customTypeName;
        field.Type = ParseFieldType(out customTypeName);
        field.TypeName = customTypeName;

        // 解析名称
        // 在 proto3 中，关键字也可以作为字段名（只要不是符号或特殊类型）
        if (IsValidFieldName(_currentToken))
        {
            field.Name = _currentToken.Value;
            Advance();
        }
        else
        {
            throw NewParseException($"Expected field name, got: {_currentToken.Type}");
        }

        // 解析字段编号
        Expect(TokenType.Equal);

        if (_currentToken.Type == TokenType.IntegerLiteral)
        {
            if (int.TryParse(_currentToken.Value, out int fieldNumber))
            {
                field.FieldNumber = fieldNumber;
            }
            else
            {
                throw NewParseException($"Invalid field number: {_currentToken.Value}");
            }
            Advance();
        }
        else
        {
            throw NewParseException($"Expected field number, got: {_currentToken.Type}");
        }

        // 解析选项
        while (_currentToken.Type == TokenType.LeftBracket)
        {
            Advance();
            ParseFieldOptions(field);
            Expect(TokenType.RightBracket);
        }

        Expect(TokenType.SemiColon);
        return field;
    }

    /// <summary>
    /// 检查 token 是否可以作为字段名
    /// 在 proto3 中，大多数关键字也可以作为字段名
    /// </summary>
    private bool IsValidFieldName(Token token)
    {
        // 标识符总是可以作为字段名
        if (token.Type == TokenType.Identifier)
            return true;

        // 排除明显不能作为字段名的符号类型
        if (token.Type == TokenType.Dot ||
            token.Type == TokenType.Comma ||
            token.Type == TokenType.SemiColon ||
            token.Type == TokenType.LeftBrace ||
            token.Type == TokenType.RightBrace ||
            token.Type == TokenType.LeftBracket ||
            token.Type == TokenType.RightBracket ||
            token.Type == TokenType.Equal ||
            token.Type == TokenType.LessThan ||
            token.Type == TokenType.GreaterThan ||
            token.Type == TokenType.EndOfFile ||
            token.Type == TokenType.Whitespace ||
            token.Type == TokenType.Comment ||
            token.Type == TokenType.Unknown)
        {
            return false;
        }

        // 大多数关键字可以作为字段名（message, enum, service 等）
        // 但类型关键字不应该作为字段名（如 string, int32 等）
        if (IsTypeKeyword(token.Type))
            return false;

        // 其他关键字可以作为字段名
        return !string.IsNullOrEmpty(token.Value);
    }

    /// <summary>
    /// 检查是否为类型关键字（不能作为字段名）
    /// </summary>
    private bool IsTypeKeyword(TokenType type)
    {
        return type == TokenType.Double ||
               type == TokenType.Float ||
               type == TokenType.Int32 ||
               type == TokenType.Int64 ||
               type == TokenType.UInt32 ||
               type == TokenType.UInt64 ||
               type == TokenType.SInt32 ||
               type == TokenType.SInt64 ||
               type == TokenType.Fixed32 ||
               type == TokenType.Fixed64 ||
               type == TokenType.SFixed32 ||
               type == TokenType.SFixed64 ||
               type == TokenType.Bool ||
               type == TokenType.String ||
               type == TokenType.Bytes;
    }

    /// <summary>
    /// 解析字段类型
    /// </summary>
    private FieldType ParseFieldType(out string? customTypeName)
    {
        customTypeName = null;

        switch (_currentToken.Type)
        {
            case TokenType.Double:
                Advance();
                return FieldType.Double;

            case TokenType.Float:
                Advance();
                return FieldType.Float;

            case TokenType.Int32:
                Advance();
                return FieldType.Int32;

            case TokenType.Int64:
                Advance();
                return FieldType.Int64;

            case TokenType.UInt32:
                Advance();
                return FieldType.UInt32;

            case TokenType.UInt64:
                Advance();
                return FieldType.UInt64;

            case TokenType.SInt32:
                Advance();
                return FieldType.SInt32;

            case TokenType.SInt64:
                Advance();
                return FieldType.SInt64;

            case TokenType.Fixed32:
                Advance();
                return FieldType.Fixed32;

            case TokenType.Fixed64:
                Advance();
                return FieldType.Fixed64;

            case TokenType.SFixed32:
                Advance();
                return FieldType.SFixed32;

            case TokenType.SFixed64:
                Advance();
                return FieldType.SFixed64;

            case TokenType.Bool:
                Advance();
                return FieldType.Bool;

            case TokenType.String:
                Advance();
                return FieldType.String;

            case TokenType.Bytes:
                Advance();
                return FieldType.Bytes;

            case TokenType.Identifier:
                // 可能是点号分隔的类型名（如 Example.Common.Timestamp）
                customTypeName = ParseDottedTypeName();
                return FieldType.Message; // 假定是消息或枚举类型

            default:
                throw NewParseException($"Expected field type, got: {_currentToken.Type}");
        }
    }

    /// <summary>
    /// 解析点号分隔的类型名（如 Example.Common.Timestamp）
    /// </summary>
    private string ParseDottedTypeName()
    {
        var typeName = new System.Text.StringBuilder();

        // 第一个部分必须是标识符
        if (_currentToken.Type == TokenType.Identifier)
        {
            typeName.Append(_currentToken.Value);
            Advance();
        }
        else
        {
            throw NewParseException($"Expected type name, got: {_currentToken.Type}");
        }

        // 继续读取点和标识符
        while (_currentToken.Type == TokenType.Dot && _nextToken.Type == TokenType.Identifier)
        {
            typeName.Append(_currentToken.Value); // 添加点
            Advance(); // 消费点

            typeName.Append(_currentToken.Value); // 添加标识符
            Advance(); // 消费标识符
        }

        return typeName.ToString();
    }

    /// <summary>
    /// 解析字段选项
    /// </summary>
    private void ParseFieldOptions(FieldDeclaration field)
    {
        while (true)
        {
            if (_currentToken.Type == TokenType.Identifier)
            {
                string optionName = _currentToken.Value;
                Advance();

                if (_currentToken.Type == TokenType.Equal)
                {
                    Advance();

                    if (optionName.Equals("default", StringComparison.OrdinalIgnoreCase))
                    {
                        field.DefaultValue = _currentToken.Value;
                        Advance();
                    }
                    else if (optionName.Equals("json_name", StringComparison.OrdinalIgnoreCase))
                    {
                        field.JsonName = _currentToken.Value;
                        Advance();
                    }
                    else if (optionName.Equals("packed", StringComparison.OrdinalIgnoreCase))
                    {
                        field.IsPacked = bool.Parse(_currentToken.Value);
                        Advance();
                    }
                }
                else
                {
                    // 布尔选项
                    if (optionName.Equals("packed", StringComparison.OrdinalIgnoreCase))
                    {
                        field.IsPacked = true;
                    }
                }

                if (_currentToken.Type == TokenType.Comma)
                {
                    Advance();
                    continue;
                }
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 解析 map 字段
    /// </summary>
    private FieldDeclaration ParseMapField()
    {
        Expect(TokenType.Map);
        Expect(TokenType.LessThan);

        var field = new FieldDeclaration
        {
            IsMap = true
        };

        // 解析键类型
        string? keyTypeName;
        var keyType = ParseFieldType(out keyTypeName);
        field.MapKeyType = new FieldDeclaration
        {
            Type = keyType,
            TypeName = keyTypeName
        };

        // 期望有逗号（可选，有些proto文件可能没有逗号）
        if (_currentToken.Type == TokenType.Comma)
        {
            Advance();
        }

        // 解析值类型
        string? valueTypeName;
        var valueType = ParseFieldType(out valueTypeName);
        field.MapValueType = new FieldDeclaration
        {
            Type = valueType,
            TypeName = valueTypeName
        };

        Expect(TokenType.GreaterThan);

        // 解析字段名
        if (_currentToken.Type == TokenType.Identifier)
        {
            field.Name = _currentToken.Value;
            Advance();
        }
        else
        {
            throw NewParseException($"Expected map field name, got: {_currentToken.Type}");
        }

        // 解析字段编号
        Expect(TokenType.Equal);

        if (_currentToken.Type == TokenType.IntegerLiteral)
        {
            if (int.TryParse(_currentToken.Value, out int fieldNumber))
            {
                field.FieldNumber = fieldNumber;
            }
            else
            {
                throw NewParseException($"Invalid field number: {_currentToken.Value}");
            }
            Advance();
        }
        else
        {
            throw NewParseException($"Expected field number, got: {_currentToken.Type}");
        }

        Expect(TokenType.SemiColon);
        return field;
    }

    /// <summary>
    /// 解析 oneof 声明
    /// </summary>
    private OneOfDeclaration ParseOneOf()
    {
        Expect(TokenType.Oneof);

        var oneOf = new OneOfDeclaration();

        if (_currentToken.Type == TokenType.Identifier)
        {
            oneOf.Name = _currentToken.Value;
            Advance();
        }
        else
        {
            throw NewParseException($"Expected oneof name, got: {_currentToken.Type}");
        }

        Expect(TokenType.LeftBrace);

        while (_currentToken.Type != TokenType.RightBrace && _currentToken.Type != TokenType.EndOfFile)
        {
            var field = ParseField();
            if (field != null)
            {
                field.IsOneOf = true;
                field.OneOfName = oneOf.Name;
                oneOf.Fields.Add(field);
            }
        }

        Expect(TokenType.RightBrace);
        return oneOf;
    }

    /// <summary>
    /// 解析 reserved 声明
    /// </summary>
    private ReservedDeclaration ParseReserved()
    {
        Expect(TokenType.Reserved);

        var reserved = new ReservedDeclaration();

        if (_currentToken.Type == TokenType.IntegerLiteral)
        {
            reserved.Ranges = new List<int>();

            while (_currentToken.Type == TokenType.IntegerLiteral)
            {
                if (int.TryParse(_currentToken.Value, out int value))
                {
                    reserved.Ranges.Add(value);
                }
                Advance();

                if (_currentToken.Type == TokenType.Comma)
                {
                    Advance();
                }
            }
        }
        else if (_currentToken.Type == TokenType.StringLiteral)
        {
            reserved.FieldNames = new List<string>();

            while (_currentToken.Type == TokenType.StringLiteral)
            {
                reserved.FieldNames.Add(_currentToken.Value);
                Advance();

                if (_currentToken.Type == TokenType.Comma)
                {
                    Advance();
                }
            }
        }
        else
        {
            throw NewParseException($"Expected reserved ranges or names, got: {_currentToken.Type}");
        }

        Expect(TokenType.SemiColon);
        return reserved;
    }

    /// <summary>
    /// 解析 enum 声明
    /// </summary>
    private EnumDeclaration ParseEnum()
    {
        Expect(TokenType.Enum);

        var enumDecl = new EnumDeclaration();

        if (_currentToken.Type == TokenType.Identifier)
        {
            enumDecl.Name = _currentToken.Value;
            Advance();
        }
        else
        {
            throw NewParseException($"Expected enum name, got: {_currentToken.Type}");
        }

        Expect(TokenType.LeftBrace);

        while (_currentToken.Type != TokenType.RightBrace && _currentToken.Type != TokenType.EndOfFile)
        {
            switch (_currentToken.Type)
            {
                case TokenType.Option:
                    var option = ParseOption();
                    enumDecl.Options.Add(option);
                    break;

                case TokenType.Identifier:
                    var enumValue = ParseEnumValue();
                    enumDecl.Values.Add(enumValue);
                    break;

                case TokenType.Comment:
                case TokenType.Whitespace:
                    Advance();
                    break;

                default:
                    throw NewParseException($"Unexpected token in enum: {_currentToken.Type}");
            }
        }

        Expect(TokenType.RightBrace);
        return enumDecl;
    }

    /// <summary>
    /// 解析 enum 值
    /// </summary>
    private EnumValueDeclaration ParseEnumValue()
    {
        var enumValue = new EnumValueDeclaration();

        if (_currentToken.Type == TokenType.Identifier)
        {
            enumValue.Name = _currentToken.Value;
            Advance();
        }
        else
        {
            throw NewParseException($"Expected enum value name, got: {_currentToken.Type}");
        }

        Expect(TokenType.Equal);

        if (_currentToken.Type == TokenType.IntegerLiteral)
        {
            if (int.TryParse(_currentToken.Value, out int value))
            {
                enumValue.Value = value;
            }
            else
            {
                throw NewParseException($"Invalid enum value: {_currentToken.Value}");
            }
            Advance();
        }
        else
        {
            throw NewParseException($"Expected enum value, got: {_currentToken.Type}");
        }

        // 可能有选项
        if (_currentToken.Type == TokenType.LeftBracket)
        {
            Advance();
            while (_currentToken.Type != TokenType.RightBracket && _currentToken.Type != TokenType.EndOfFile)
            {
                Advance();
            }
            Expect(TokenType.RightBracket);
        }

        Expect(TokenType.SemiColon);
        return enumValue;
    }

    /// <summary>
    /// 跳过 service 声明
    /// </summary>
    private void SkipService()
    {
        Expect(TokenType.Service);

        if (_currentToken.Type == TokenType.Identifier)
        {
            Advance();
        }

        int braceCount = 0;

        if (_currentToken.Type == TokenType.LeftBrace)
        {
            braceCount++;
            Advance();
        }

        while (braceCount > 0 && _currentToken.Type != TokenType.EndOfFile)
        {
            if (_currentToken.Type == TokenType.LeftBrace)
            {
                braceCount++;
            }
            else if (_currentToken.Type == TokenType.RightBrace)
            {
                braceCount--;
            }
            Advance();
        }

        if (_currentToken.Type == TokenType.SemiColon)
        {
            Advance();
        }
    }

    /// <summary>
    /// 跳过 extend 声明
    /// </summary>
    private void SkipExtend()
    {
        Expect(TokenType.Extend);

        if (_currentToken.Type == TokenType.Identifier)
        {
            Advance();
        }

        int braceCount = 0;

        if (_currentToken.Type == TokenType.LeftBrace)
        {
            braceCount++;
            Advance();
        }

        while (braceCount > 0 && _currentToken.Type != TokenType.EndOfFile)
        {
            if (_currentToken.Type == TokenType.LeftBrace)
            {
                braceCount++;
            }
            else if (_currentToken.Type == TokenType.RightBrace)
            {
                braceCount--;
            }
            Advance();
        }

        if (_currentToken.Type == TokenType.SemiColon)
        {
            Advance();
        }
    }

    /// <summary>
    /// 期望指定类型的 token
    /// </summary>
    private void Expect(TokenType type)
    {
        if (_currentToken.Type != type)
        {
            throw NewParseException($"Expected {type}, got {_currentToken.Type}");
        }
        Advance();
    }

    /// <summary>
    /// 前进到下一个 token
    /// </summary>
    private void Advance()
    {
        _currentToken = _nextToken;
        _nextToken = _position < _tokens.Count ? _tokens[_position] : new Token(TokenType.EndOfFile, "", 0, 0, 0);
        _position++;
    }

    /// <summary>
    /// 创建解析异常
    /// </summary>
    private ParseException NewParseException(string message)
    {
        return new ParseException($"Error at line {_currentToken.Line}, column {_currentToken.Column}: {message}");
    }
}

/// <summary>
/// 解析异常
/// </summary>
public sealed class ParseException : Exception
{
    public ParseException(string message) : base(message) { }
}
