using System.Text;

namespace Protobuf.Parser;

/// <summary>
/// Proto 文件词法分析器
/// </summary>
public sealed class Lexer
{
    private readonly string _source;
    private readonly int _length;
    private int _position;
    private int _line;
    private int _column;
    private readonly List<Token> _tokens = new();

    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        // 语法关键字
        { "syntax", TokenType.Syntax },
        { "proto", TokenType.Proto },
        { "package", TokenType.Package },
        { "import", TokenType.Import },
        { "message", TokenType.Message },
        { "enum", TokenType.Enum },
        { "oneof", TokenType.Oneof },
        { "map", TokenType.Map },
        { "option", TokenType.Option },
        { "reserved", TokenType.Reserved },
        { "extend", TokenType.Extend },
        { "service", TokenType.Service },
        { "rpc", TokenType.Rpc },
        { "stream", TokenType.Stream },
        { "returns", TokenType.Returns },

        // 类型关键字
        { "double", TokenType.Double },
        { "float", TokenType.Float },
        { "int32", TokenType.Int32 },
        { "int64", TokenType.Int64 },
        { "uint32", TokenType.UInt32 },
        { "uint64", TokenType.UInt64 },
        { "sint32", TokenType.SInt32 },
        { "sint64", TokenType.SInt64 },
        { "fixed32", TokenType.Fixed32 },
        { "fixed64", TokenType.Fixed64 },
        { "sfixed32", TokenType.SFixed32 },
        { "sfixed64", TokenType.SFixed64 },
        { "bool", TokenType.Bool },
        { "string", TokenType.String },
        { "bytes", TokenType.Bytes },

        // 修饰符
        { "optional", TokenType.Optional },
        { "repeated", TokenType.Repeated },
        { "required", TokenType.Required },

        // 布尔字面量
        { "true", TokenType.BooleanLiteral },
        { "false", TokenType.BooleanLiteral }
    };

    public Lexer(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _length = source.Length;
        _position = 0;
        _line = 1;
        _column = 1;
    }

    /// <summary>
    /// 词法分析
    /// </summary>
    public List<Token> Tokenize()
    {
        _tokens.Clear();

        while (_position < _length)
        {
            char current = Peek();

            // 跳过空白
            if (char.IsWhiteSpace(current))
            {
                ReadWhitespace();
                continue;
            }

            // 注释
            if (current == '/' && Peek(1) == '/')
            {
                ReadLineComment();
                continue;
            }

            if (current == '/' && Peek(1) == '*')
            {
                ReadBlockComment();
                continue;
            }

            // 标识符或关键字
            if (char.IsLetter(current) || current == '_')
            {
                ReadIdentifierOrKeyword();
                continue;
            }

            // 字符串字面量
            if (current == '\'' || current == '"')
            {
                ReadStringLiteral();
                continue;
            }

            // 数字
            if (char.IsDigit(current))
            {
                ReadNumber();
                continue;
            }

            // 符号
            ReadSymbol();
        }

        // 添加 EOF token
        _tokens.Add(new Token(TokenType.EndOfFile, "", _line, _column, _position));

        return _tokens;
    }

    /// <summary>
    /// 读取空白
    /// </summary>
    private void ReadWhitespace()
    {
        while (_position < _length && char.IsWhiteSpace(Peek()))
        {
            if (Peek() == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            _position++;
        }
    }

    /// <summary>
    /// 读取行注释
    /// </summary>
    private void ReadLineComment()
    {
        int startLine = _line;
        int startColumn = _column;
        int startPosition = _position;

        _position += 2; // 跳过 '//'
        while (_position < _length && Peek() != '\n')
        {
            _column++;
            _position++;
        }

        string value = _source.Substring(startPosition, _position - startPosition);
        _tokens.Add(new Token(TokenType.Comment, value, startLine, startColumn, startPosition));
    }

    /// <summary>
    /// 读取块注释
    /// </summary>
    private void ReadBlockComment()
    {
        int startLine = _line;
        int startColumn = _column;
        int startPosition = _position;

        _position += 2; // 跳过 '/*'

        while (_position < _length)
        {
            if (Peek() == '*' && Peek(1) == '/')
            {
                _position += 2;
                _column += 2;
                break;
            }

            if (Peek() == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            _position++;
        }

        string value = _source.Substring(startPosition, _position - startPosition);
        _tokens.Add(new Token(TokenType.Comment, value, startLine, startColumn, startPosition));
    }

    /// <summary>
    /// 读取标识符或关键字
    /// </summary>
    private void ReadIdentifierOrKeyword()
    {
        int startColumn = _column;
        int startPosition = _position;

        StringBuilder sb = new();
        while (_position < _length && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            sb.Append(Peek());
            _column++;
            _position++;
        }

        string value = sb.ToString();

        // 检查是否为关键字
        if (Keywords.TryGetValue(value, out var tokenType))
        {
            _tokens.Add(new Token(tokenType, value, _line, startColumn, startPosition));
        }
        else
        {
            _tokens.Add(new Token(TokenType.Identifier, value, _line, startColumn, startPosition));
        }
    }

    /// <summary>
    /// 读取字符串字面量
    /// </summary>
    private void ReadStringLiteral()
    {
        int startColumn = _column;
        int startPosition = _position;
        char quote = Peek();

        _position++; // 跳过开始的引号
        _column++;

        StringBuilder sb = new();
        while (_position < _length)
        {
            char current = Peek();

            if (current == '\\')
            {
                // 转义序列
                _position++;
                _column++;

                if (_position >= _length) break;

                char escapeChar = Peek();
                sb.Append(escapeChar switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '\\' => '\\',
                    '\'' => '\'',
                    '"' => '"',
                    _ => escapeChar
                });

                _position++;
                _column++;
                continue;
            }

            if (current == quote)
            {
                _position++; // 跳过结束的引号
                _column++;
                break;
            }

            sb.Append(current);
            _position++;
            _column++;
        }

        _tokens.Add(new Token(TokenType.StringLiteral, sb.ToString(), _line, startColumn, startPosition));
    }

    /// <summary>
    /// 读取数字
    /// </summary>
    private void ReadNumber()
    {
        int startColumn = _column;
        int startPosition = _position;

        StringBuilder sb = new();
        bool isFloat = false;

        while (_position < _length)
        {
            char current = Peek();

            if (char.IsDigit(current))
            {
                sb.Append(current);
                _position++;
                _column++;
                continue;
            }

            if (current == '.' && !isFloat)
            {
                sb.Append(current);
                isFloat = true;
                _position++;
                _column++;
                continue;
            }

            if ((current == 'e' || current == 'E') && Peek(1) != 0)
            {
                sb.Append(current);
                _position++;
                _column++;

                // 可能的 + 或 -
                if (Peek() == '+' || Peek() == '-')
                {
                    sb.Append(Peek());
                    _position++;
                    _column++;
                }

                // 指数部分
                while (_position < _length && char.IsDigit(Peek()))
                {
                    sb.Append(Peek());
                    _position++;
                    _column++;
                }

                isFloat = true;
                continue;
            }

            break;
        }

        string value = sb.ToString();
        if (isFloat)
        {
            _tokens.Add(new Token(TokenType.FloatLiteral, value, _line, startColumn, startPosition));
        }
        else
        {
            _tokens.Add(new Token(TokenType.IntegerLiteral, value, _line, startColumn, startPosition));
        }
    }

    /// <summary>
    /// 读取符号
    /// </summary>
    private void ReadSymbol()
    {
        int startColumn = _column;
        int startPosition = _position;

        char current = Peek();

        // 多字符符号
        if (current == '<' && Peek(1) == '=')
        {
            _position += 2;
            _column += 2;
            _tokens.Add(new Token(TokenType.LessThan, "<=", _line, startColumn, startPosition));
            return;
        }

        if (current == '>' && Peek(1) == '=')
        {
            _position += 2;
            _column += 2;
            _tokens.Add(new Token(TokenType.GreaterThan, ">=", _line, startColumn, startPosition));
            return;
        }

        // 单字符符号
        _position++;
        _column++;

        var tokenType = current switch
        {
            '=' => TokenType.Equal,
            ';' => TokenType.SemiColon,
            ':' => TokenType.Colon,
            ',' => TokenType.Comma,
            '{' => TokenType.LeftBrace,
            '}' => TokenType.RightBrace,
            '[' => TokenType.LeftBracket,
            ']' => TokenType.RightBracket,
            '(' => TokenType.LeftParen,
            ')' => TokenType.RightParen,
            '<' => TokenType.LessThan,
            '>' => TokenType.GreaterThan,
            '.' => TokenType.Dot,
            '+' => TokenType.Plus,
            '-' => TokenType.Minus,
            _ => TokenType.Unknown
        };

        _tokens.Add(new Token(tokenType, current.ToString(), _line, startColumn, startPosition));
    }

    /// <summary>
    /// 查看当前字符
    /// </summary>
    private char Peek()
    {
        return Peek(0);
    }

    /// <summary>
    /// 查看指定偏移位置的字符
    /// </summary>
    private char Peek(int offset)
    {
        int pos = _position + offset;
        if (pos < 0 || pos >= _length)
        {
            return '\0';
        }
        return _source[pos];
    }
}
