namespace Protobuf.Parser;

/// <summary>
/// Token 类型
/// </summary>
public enum TokenType
{
    // 关键字
    Syntax,
    Proto,
    Package,
    Import,
    Message,
    Enum,
    Oneof,
    Map,
    Option,
    Reserved,
    Extend,
    Service,
    Rpc,
    Stream,
    Returns,

    // 类型关键字
    Double,
    Float,
    Int32,
    Int64,
    UInt32,
    UInt64,
    SInt32,
    SInt64,
    Fixed32,
    Fixed64,
    SFixed32,
    SFixed64,
    Bool,
    String,
    Bytes,

    // 修饰符
    Optional,
    Repeated,
    Required,

    // 标识符和字面量
    Identifier,
    StringLiteral,
    IntegerLiteral,
    FloatLiteral,
    BooleanLiteral,

    // 符号
    Equal,          // =
    SemiColon,      // ;
    Colon,          // :
    Comma,          // ,
    LeftBrace,      // {
    RightBrace,     // }
    LeftBracket,    // [
    RightBracket,   // ]
    LeftParen,      // (
    RightParen,     // )
    LessThan,       // <
    GreaterThan,    // >
    Dot,            // .
    Plus,           // +
    Minus,          // -

    // 特殊
    Comment,
    Whitespace,
    EndOfFile,
    Unknown
}

/// <summary>
/// Token
/// </summary>
public sealed class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Line { get; }
    public int Column { get; }
    public int Position { get; }

    public Token(TokenType type, string value, int line, int column, int position)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
        Position = position;
    }

    public override string ToString()
    {
        return $"Token({Type}, '{Value}', Line:{Line}, Col:{Column})";
    }

    /// <summary>
    /// 检查是否为指定类型的标识符
    /// </summary>
    public bool IsIdentifier(string identifier)
    {
        return Type == TokenType.Identifier && Value == identifier;
    }

    /// <summary>
    /// 检查是否为指定类型
    /// </summary>
    public bool Is(TokenType type)
    {
        return Type == type;
    }
}
