using Xunit;
using Protobuf.Core;
using Protobuf.Parser;

namespace Protobuf.Tests;

/// <summary>
/// Proto 文件解析器测试
/// </summary>
public class ParserTests
{
    [Fact]
    public void Parse_SimpleMessage()
    {
        // Arrange
        var protoContent = @"
syntax = ""proto3"";

message Person {
  string name = 1;
  int32 id = 2;
}
";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        var parser = new Protobuf.Parser.Parser(tokens);
        var protoFile = parser.Parse();

        // Assert
        Assert.NotNull(protoFile);
        Assert.Equal("proto3", protoFile.Syntax);
        Assert.Single(protoFile.Messages);

        var message = protoFile.Messages[0];
        Assert.Equal("Person", message.Name);
        Assert.Equal(2, message.Fields.Count);

        var nameField = message.Fields[0];
        Assert.Equal("name", nameField.Name);
        Assert.Equal(FieldType.String, nameField.Type);
        Assert.Equal(1, nameField.FieldNumber);

        var idField = message.Fields[1];
        Assert.Equal("id", idField.Name);
        Assert.Equal(FieldType.Int32, idField.Type);
        Assert.Equal(2, idField.FieldNumber);
    }

    [Fact]
    public void Parse_MessageWithRepeated()
    {
        // Arrange
        var protoContent = @"
syntax = ""proto3"";

message Person {
  repeated string phone_numbers = 1;
}
";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        var parser = new Protobuf.Parser.Parser(tokens);
        var protoFile = parser.Parse();

        // Assert
        Assert.NotNull(protoFile);
        var message = protoFile.Messages[0];
        var field = message.Fields[0];

        Assert.Equal("phone_numbers", field.Name);
        Assert.Equal(FieldType.String, field.Type);
        Assert.Equal(FieldLabel.Repeated, field.Label);
    }

    [Fact]
    public void Parse_MessageWithOneOf()
    {
        // Arrange
        var protoContent = @"
syntax = ""proto3"";

message Person {
  oneof address {
    string home = 1;
    string work = 2;
  }
}
";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        var parser = new Protobuf.Parser.Parser(tokens);
        var protoFile = parser.Parse();

        // Assert
        Assert.NotNull(protoFile);
        var message = protoFile.Messages[0];

        Assert.Single(message.OneOfs);

        var oneOf = message.OneOfs[0];
        Assert.Equal("address", oneOf.Name);
        Assert.Equal(2, oneOf.Fields.Count);

        var homeField = oneOf.Fields[0];
        Assert.Equal("home", homeField.Name);
        Assert.True(homeField.IsOneOf);

        var workField = oneOf.Fields[1];
        Assert.Equal("work", workField.Name);
        Assert.True(workField.IsOneOf);
    }

    [Fact]
    public void Parse_MessageWithMap()
    {
        // Arrange
        var protoContent = @"
syntax = ""proto3"";

message Person {
  map<string, string> metadata = 1;
}
";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        var parser = new Protobuf.Parser.Parser(tokens);
        var protoFile = parser.Parse();

        // Assert
        Assert.NotNull(protoFile);
        var message = protoFile.Messages[0];

        Assert.Single(message.Fields);

        var mapField = message.Fields[0];
        Assert.Equal("metadata", mapField.Name);
        Assert.True(mapField.IsMap);
        Assert.NotNull(mapField.MapKeyType);
        Assert.NotNull(mapField.MapValueType);
        Assert.Equal(FieldType.String, mapField.MapKeyType.Type);
        Assert.Equal(FieldType.String, mapField.MapValueType.Type);
    }

    [Fact]
    public void Parse_Enum()
    {
        // Arrange
        var protoContent = @"
syntax = ""proto3"";

enum PhoneType {
  MOBILE = 0;
  HOME = 1;
  WORK = 2;
}
";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        var parser = new Protobuf.Parser.Parser(tokens);
        var protoFile = parser.Parse();

        // Assert
        Assert.NotNull(protoFile);
        Assert.Single(protoFile.Enums);

        var enumDecl = protoFile.Enums[0];
        Assert.Equal("PhoneType", enumDecl.Name);
        Assert.Equal(3, enumDecl.Values.Count);

        Assert.Equal("MOBILE", enumDecl.Values[0].Name);
        Assert.Equal(0, enumDecl.Values[0].Value);

        Assert.Equal("HOME", enumDecl.Values[1].Name);
        Assert.Equal(1, enumDecl.Values[1].Value);

        Assert.Equal("WORK", enumDecl.Values[2].Name);
        Assert.Equal(2, enumDecl.Values[2].Value);
    }

    [Fact]
    public void Parse_NestedMessage()
    {
        // Arrange
        var protoContent = @"
syntax = ""proto3"";

message Person {
  message PhoneNumber {
    string number = 1;
  }

  repeated PhoneNumber phones = 2;
}
";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        var parser = new Protobuf.Parser.Parser(tokens);
        var protoFile = parser.Parse();

        // Assert
        Assert.NotNull(protoFile);
        var person = protoFile.Messages[0];

        Assert.Single(person.NestedMessages);

        var phoneNumber = person.NestedMessages[0];
        Assert.Equal("PhoneNumber", phoneNumber.Name);
        Assert.Single(phoneNumber.Fields);
        Assert.Equal("number", phoneNumber.Fields[0].Name);
    }

    [Fact]
    public void Parse_ComplexExample()
    {
        // Arrange
        var protoContent = @"
syntax = ""proto3"";

package Example;

message Person {
  string name = 1;
  int32 id = 2;
  string email = 3;

  enum PhoneType {
    MOBILE = 0;
    HOME = 1;
    WORK = 2;
  }

  message PhoneNumber {
    string number = 1;
    PhoneType type = 2;
  }

  repeated PhoneNumber phones = 4;

  oneof address {
    string home_address = 5;
    string work_address = 6;
  }

  map<string, string> metadata = 7;
}

enum Gender {
  MALE = 0;
  FEMALE = 1;
  OTHER = 2;
}
";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        var parser = new Protobuf.Parser.Parser(tokens);
        var protoFile = parser.Parse();

        // Assert
        Assert.NotNull(protoFile);
        Assert.Equal("proto3", protoFile.Syntax);
        Assert.Equal("Example", protoFile.Package);

        Assert.Single(protoFile.Enums);
        Assert.Single(protoFile.Messages);

        var person = protoFile.Messages[0];
        Assert.Equal("Person", person.Name);

        // 验证顶层枚举
        Assert.Equal("Gender", protoFile.Enums[0].Name);

        // 验证嵌套的枚举
        Assert.Single(person.Enums);
        Assert.Equal("PhoneType", person.Enums[0].Name);

        // 验证嵌套的消息
        Assert.Single(person.NestedMessages);
        Assert.Equal("PhoneNumber", person.NestedMessages[0].Name);

        // 验证 oneof
        Assert.Single(person.OneOfs);
        Assert.Equal("address", person.OneOfs[0].Name);

        // 验证 map (应该是最后一个字段)
        var metadataField = person.Fields.FirstOrDefault(f => f.Name == "metadata");
        Assert.NotNull(metadataField);
        Assert.True(metadataField.IsMap);
    }

    [Fact]
    public void Lexer_TokenizeBasicTokens()
    {
        // Arrange
        var protoContent = "syntax = \"proto3\";\n\nmessage Test {}";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        // Assert
        Assert.NotNull(tokens);
        Assert.True(tokens.Count > 0);

        // 查找关键字
        var syntaxToken = tokens.FirstOrDefault(t => t.Type == TokenType.Syntax);
        Assert.NotNull(syntaxToken);

        var protoToken = tokens.FirstOrDefault(t => t.Value.Equals("proto3", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(protoToken);

        var messageToken = tokens.FirstOrDefault(t => t.Type == TokenType.Message);
        Assert.NotNull(messageToken);
    }

    [Fact]
    public void Lexer_TokenizeAllFieldTypes()
    {
        // Arrange
        var protoContent = @"
double f1;
float f2;
int32 f3;
int64 f4;
uint32 f5;
uint64 f6;
sint32 f7;
sint64 f8;
fixed32 f9;
fixed64 f10;
sfixed32 f11;
sfixed64 f12;
bool f13;
string f14;
bytes f15;
";

        // Act
        var lexer = new Lexer(protoContent);
        var tokens = lexer.Tokenize();

        // Assert
        var fieldTypes = new List<TokenType>
        {
            TokenType.Double, TokenType.Float, TokenType.Int32, TokenType.Int64,
            TokenType.UInt32, TokenType.UInt64, TokenType.SInt32, TokenType.SInt64,
            TokenType.Fixed32, TokenType.Fixed64, TokenType.SFixed32, TokenType.SFixed64,
            TokenType.Bool, TokenType.String, TokenType.Bytes
        };

        foreach (var fieldType in fieldTypes)
        {
            Assert.Contains(tokens, t => t.Type == fieldType);
        }
    }
}
