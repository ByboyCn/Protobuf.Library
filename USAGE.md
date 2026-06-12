# Protobuf Library 使用指南

## 目录

- [项目集成](#项目集成)
- [生成的代码结构](#生成的代码结构)
- [API 参考](#api-参考)
- [高级功能](#高级功能)
- [反射 API](#反射-api)
- [最佳实践](#最佳实践)

## 项目集成

### 方式一：完整集成（推荐）

使用 Source Generator 自动生成代码，获得完整功能支持。

#### 1. 添加项目引用

```xml
<ItemGroup>
  <!-- 核心库 -->
  <ProjectReference Include="Protobuf.Core\Protobuf.Core.csproj" />
  
  <!-- Source Generator -->
  <ProjectReference Include="Protobuf.Generator\Protobuf.Generator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
  
  <!-- JSON 支持（可选） -->
  <ProjectReference Include="Protobuf.Json\Protobuf.Json.csproj" />
  
  <!-- 反射 API（可选） -->
  <ProjectReference Include="Protobuf.Reflection\Protobuf.Reflection.csproj" />
</ItemGroup>
```

#### 2. 配置 Proto 文件

```xml
<ItemGroup>
  <!-- 支持通配符匹配 -->
  <ProtoFile Include="Protos\**\*.proto" />
  
  <!-- 或者指定具体文件 -->
  <ProtoFile Include="Protos\messages.proto" />
  <ProtoFile Include="Protos\services.proto" />
  
  <!-- 排除某些文件 -->
  <ProtoFile Include="Protos\**\*.proto" Exclude="Protos\**\*.test.proto" />
</ItemGroup>
```

#### 3. 构建项目

```bash
dotnet build
```

Source Generator 会自动为每个 `.proto` 文件生成对应的 C# 代码。

### 方式二：手动方式（仅用于学习）

如果你只想学习 Protobuf 格式或手动处理数据，可以只引用核心库。

```xml
<ItemGroup>
  <ProjectReference Include="Protobuf.Core\Protobuf.Core.csproj" />
  <ProjectReference Include="Protobuf.Parser\Protobuf.Parser.csproj" />
</ItemGroup>
```

## 生成的代码结构

### 消息类

为每个 `message` 生成一个 partial 类：

```csharp
public partial class Person : IMessage<Person>
{
    // 私有字段
    private string _name = "";
    private int _id = 0;
    
    // Has 方法
    public bool HasName => _name != "";
    public bool HasId => _id != 0;
    
    // 属性
    public string Name => _name;
    public int Id => _id;
    
    // Setter 方法
    public void SetName(string value);
    public void SetId(int value);
    
    // Clear 方法
    public void ClearName();
    public void ClearId();
    
    // IMessage 接口实现
    public void MergeFrom(Person other);
    public void MergeFrom(CodedInputStream input);
    public void WriteTo(CodedOutputStream output);
    public int CalculateSize();
    public bool IsInitialized();
    public Person Clone();
    
    // Parser
    public static readonly MessageParser<Person> Parser;
    
    // JSON 支持
    public string ToJson();
    
    // 反射 API
    public static MessageDescriptor Descriptor { get; }
    public static IEnumerable<IFieldAccessor> GetFieldAccessors();
}
```

### 枚举类

为每个 `enum` 生成一个枚举：

```csharp
public enum PhoneType
{
    MOBILE = 0,
    HOME = 1,
    WORK = 2
}
```

## API 参考

### 基础字段操作

#### 设置字段值

```csharp
var person = new Person();

// 使用 Setter 方法
person.SetName("Alice");
person.SetId(12345);
person.SetActive(true);
person.SetEmail("alice@example.com");
```

#### 检查字段是否有值

```csharp
// 使用 Has 方法
if (person.HasName)
{
    Console.WriteLine($"Name: {person.Name}");
}
```

#### 获取字段值

```csharp
// 直接使用属性
string name = person.Name;
int id = person.Id;
bool active = person.Active;
```

#### 清除字段值

```csharp
// 使用 Clear 方法
person.ClearName();
person.ClearId();

// 验证清除后
Console.WriteLine(person.HasName); // false
```

### Repeated 字段

#### 添加元素

```csharp
// 使用 Add 方法
person.AddPhoneNumbers("555-1234");
person.AddPhoneNumbers("555-5678");
```

#### 访问元素

```csharp
// 获取只读列表
IReadOnlyList<string> phones = person.PhoneNumbers;

// 遍历
foreach (var phone in person.PhoneNumbers)
{
    Console.WriteLine(phone);
}

// 检查数量
if (person.HasPhoneNumbers && person.PhoneNumbers.Count > 0)
{
    Console.WriteLine($"First phone: {person.PhoneNumbers[0]}");
}
```

#### 清除 Repeated 字段

```csharp
person.ClearPhoneNumbers();
Console.WriteLine(person.PhoneNumbers.Count); // 0
```

### Oneof 字段

#### 设置 Oneof 值

```csharp
// 设置其中一个选项
person.SetHomeAddress("123 Home St");

// 检查哪个选项被设置
if (person.HasHomeAddress)
{
    Console.WriteLine($"Home: {person.HomeAddress}");
}

// 获取 Oneof case
Person.AddressCase addrCase = person.AddressCase;
Console.WriteLine($"Current case: {addrCase}");
// 输出: Current case: HomeAddress
```

#### 切换 Oneof 选项

```csharp
person.SetHomeAddress("123 Home St");
Console.WriteLine(person.HasHomeAddress); // true

// 切换到另一个选项（自动清除前一个）
person.SetWorkAddress("456 Work Ave");
Console.WriteLine(person.HasHomeAddress); // false
Console.WriteLine(person.HasWorkAddress); // true
```

#### 清除 Oneof

```csharp
person.SetHomeAddress("123 Home St");
person.ClearAddress(); // 清除整个 oneof

Console.WriteLine(person.AddressCase); // AddressCase.None
```

### Map 字段

#### 添加键值对

```csharp
// 使用 Add 方法
person.AddMetadata("department", "Engineering");
person.AddMetadata("title", "Developer");

// 覆盖现有值
person.AddMetadata("department", "Sales");
```

#### 访问 Map

```csharp
// 获取只读字典
IReadOnlyDictionary<string, string> metadata = person.Metadata;

// 访问特定值
if (person.HasMetadata && person.Metadata.ContainsKey("department"))
{
    string dept = person.Metadata["department"];
    Console.WriteLine($"Department: {dept}");
}

// 遍历
foreach (var kvp in person.Metadata)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

#### 清除 Map

```csharp
person.ClearMetadata();
Console.WriteLine(person.Metadata.Count); // 0
```

### 序列化

#### 序列化到字节数组

```csharp
var person = new Person();
person.SetName("Alice");
person.SetId(12345);

// 序列化
byte[] data = person.Serialize();
Console.WriteLine($"Serialized {data.Length} bytes");
```

#### 反序列化

```csharp
// 从字节数组反序列化
Person person2 = Person.Parser.ParseFrom(data);

// 验证
Console.WriteLine(person2.Name); // "Alice"
Console.WriteLine(person2.Id);   // 12345
```

#### 从流反序列化

```csharp
using var stream = File.OpenRead("person.bin");
Person person3 = Person.Parser.ParseFrom(stream);
```

#### 合并消息

```csharp
var person1 = new Person();
person1.SetName("Alice");

var person2 = new Person();
person2.SetId(12345);

// 合并 person2 到 person1
person1.MergeFrom(person2);

// 现在 person1 包含两个字段
Console.WriteLine(person1.Name); // "Alice"
Console.WriteLine(person1.Id);   // 12345
```

### JSON 支持

#### 序列化到 JSON

```csharp
var person = new Person();
person.SetName("Alice");
person.SetId(12345);

// 序列化为 JSON（PascalCase）
string json = person.ToJson();
// {"Name":"Alice","Id":12345,"Email":"","PhoneNumbers":[],"AddressCase":"None","Metadata":{}}
```

#### 从 JSON 反序列化

```csharp
string json = @"{""Name"":""Bob"", ""Id"": 67890}";
Person person = Person.Parser.ParseJson(json);

Console.WriteLine(person.Name); // "Bob"
Console.WriteLine(person.Id);   // 67890
```

### 消息验证

#### 检查消息是否初始化

```csharp
var person = new Person();
person.SetName("Alice");
// 注意：proto3 没有必需字段，所以这个方法总是返回 true
bool initialized = person.IsInitialized();
```

#### 计算序列化大小

```csharp
var person = new Person();
person.SetName("Alice");

int size = person.CalculateSize();
Console.WriteLine($"Message will be {size} bytes when serialized");
```

### 消息克隆

```csharp
var person1 = new Person();
person1.SetName("Alice");
person1.SetId(12345);

// 深拷贝
var person2 = person1.Clone();

// 验证是不同的实例
Console.WriteLine(ReferenceEquals(person1, person2)); // false

// 修改 person1 不影响 person2
person1.SetName("Bob");
Console.WriteLine(person2.Name); // "Alice"
```

## 高级功能

### 嵌套消息

#### 定义嵌套消息

```protobuf
message Person {
  string name = 1;
  PhoneNumber phone = 2;  // 嵌套消息
}

message PhoneNumber {
  string number = 1;
  PhoneType type = 2;
}

enum PhoneType {
  MOBILE = 0;
  HOME = 1;
  WORK = 2;
}
```

#### 使用嵌套消息

```csharp
var person = new Person();
person.SetName("Alice");

// 创建嵌套消息
var phone = new PhoneNumber();
phone.SetNumber("555-1234");
phone.SetType(PhoneType.HOME);

// 设置嵌套字段
person.SetPhone(phone);

// 访问嵌套消息
if (person.HasPhone)
{
    PhoneNumber p = person.Phone;
    Console.WriteLine($"{p.Type}: {p.Number}");
}
```

### 嵌套消息类型（在 .proto 中）

```protobuf
message Person {
  string name = 1;
  
  message Address {
    string street = 1;
    string city = 2;
  }
  
  Address home_address = 2;
  Address work_address = 3;
}
```

生成的代码会包含嵌套类：

```csharp
public partial class Person : IMessage<Person>
{
    public partial class Address : IMessage<Address>
    {
        // Address 的实现
    }
    
    public Address HomeAddress { get; }
    public Address WorkAddress { get; }
    public void SetHomeAddress(Address value);
    public void SetWorkAddress(Address value);
}
```

## 反射 API

Protobuf.Library 提供了完全 AOT 友好的反射 API，允许在运行时查询消息元数据和动态访问字段。

### 获取消息描述符

```csharp
// 获取静态描述符
var descriptor = Person.Descriptor;

Console.WriteLine($"Message: {descriptor.Name}");
Console.WriteLine($"Full Name: {descriptor.FullName}");
Console.WriteLine($"Fields: {descriptor.Fields.Length}");
```

### 遍历字段

```csharp
foreach (var field in descriptor.Fields)
{
    Console.WriteLine($"Field {field.FieldNumber}: {field.Name}");
    Console.WriteLine($"  Type: {field.FieldType}");
    Console.WriteLine($"  Label: {field.Label}");
    Console.WriteLine($"  Is Map: {field.IsMap}");
    Console.WriteLine($"  Is OneOf: {field.IsOneOf}");
}
```

### 查找字段

```csharp
// 按名称查找
var nameField = descriptor.FindFieldByName("name");
if (nameField != null)
{
    Console.WriteLine($"Found field: {nameField.Name}");
}

// 按编号查找
var field1 = descriptor.FindFieldByNumber(1);
if (field1 != null)
{
    Console.WriteLine($"Field 1: {field1.Name}");
}
```

### 字段访问器

```csharp
// 获取所有字段访问器
var accessors = Person.GetFieldAccessors();

var person = new Person();
person.SetName("Alice");

foreach (var accessor in accessors)
{
    var field = accessor.Descriptor;
    
    // 检查字段是否有值
    bool hasValue = accessor.HasValue(person);
    
    if (hasValue)
    {
        // 获取字段值
        object? value = accessor.GetValue(person);
        Console.WriteLine($"{field.Name} = {value}");
        
        // 设置字段值
        accessor.SetValue(person, "Bob");
        
        // 清除字段
        accessor.Clear(person);
    }
}
```

### 类型安全的字段访问

对于生成的消息，可以使用类型安全的访问器：

```csharp
var person = new Person();
person.SetName("Alice");

// 使用生成的类型安全访问器
var nameAccessor = Person._nameAccessor; // 生成的私有访问器

// 类型安全的方法
string? name = nameAccessor.GetTypedValue(person);
nameAccessor.SetTypedValue(person, "Bob");
```

### 动态字段操作示例

```csharp
public void ProcessMessage(IMessage message)
{
    // 获取描述符
    var descriptor = message.GetType()
        .GetProperty("Descriptor")?
        .GetValue(null) as MessageDescriptor;
    
    if (descriptor == null) return;
    
    Console.WriteLine($"Processing message: {descriptor.Name}");
    
    // 遍历字段
    foreach (var field in descriptor.Fields)
    {
        if (field.FieldType == FieldType.String && 
            field.Label == FieldLabel.Optional)
        {
            // 处理字符串字段
            ProcessStringField(message, field);
        }
    }
}

private void ProcessStringField(IMessage message, FieldDescriptor field)
{
    // 获取访问器
    var accessors = message.GetType()
        .GetMethod("GetFieldAccessors")?
        .Invoke(null, null) as IEnumerable<IFieldAccessor>;
    
    if (accessors == null) return;
    
    var accessor = accessors.FirstOrDefault(a => 
        a.Descriptor.FieldNumber == field.FieldNumber);
    
    if (accessor != null)
    {
        var value = accessor.GetValue(message) as string;
        Console.WriteLine($"  {field.Name} = {value}");
    }
}
```

## 最佳实践

### 1. 消息构建

**推荐**：使用构造函数和 Setter 方法

```csharp
var person = new Person();
person.SetName("Alice");
person.SetId(12345);
person.AddPhoneNumbers("555-1234");
```

### 2. 消息验证

**推荐**：使用 Has 方法验证字段

```csharp
if (person.HasName && person.HasId)
{
    // 安全地访问字段
    Console.WriteLine($"{person.Name} (ID: {person.Id})");
}
```

### 3. 集合操作

**推荐**：使用 Add 方法而不是直接操作集合

```csharp
// 好
person.AddPhoneNumbers("555-1234");

// 避免（集合是只读的）
// person.PhoneNumbers.Add("555-1234"); // 编译错误
```

### 4. Oneof 操作

**推荐**：检查 Case 属性来处理 Oneof

```csharp
switch (person.AddressCase)
{
    case Person.AddressCase.HomeAddress:
        Console.WriteLine($"Home: {person.HomeAddress}");
        break;
    case Person.AddressCase.WorkAddress:
        Console.WriteLine($"Work: {person.WorkAddress}");
        break;
    case Person.AddressCase.None:
        Console.WriteLine("No address set");
        break;
}
```

### 5. 序列化优化

**推荐**：复用输出流以提高性能

```csharp
// 对于批量序列化
var output = new CodedOutputStream();

foreach (var person in people)
{
    person.WriteTo(output);
}

byte[] data = output.ToByteArray();
```

### 6. 错误处理

**推荐**：使用 Parser 进行反序列化并处理异常

```csharp
try
{
    var person = Person.Parser.ParseFrom(data);
    // 处理消息
}
catch (InvalidProtocolBufferException ex)
{
    // 处理格式错误
    Console.WriteLine($"Invalid protobuf data: {ex.Message}");
}
```

### 7. JSON 互操作

**推荐**：用于调试和日志，不用于高性能场景

```csharp
// 用于调试
Console.WriteLine($"Person data: {person.ToJson()}");

// 用于日志
logger.LogInformation("Person created: {PersonJson}", person.ToJson());
```

### 8. AOT 部署

**推荐**：使用反射 API 进行动态字段访问

```csharp
// AOT 友好的动态访问
var descriptor = Person.Descriptor;
var field = descriptor.FindFieldByName("name");

if (field != null)
{
    var accessor = Person.GetFieldAccessors()
        .First(a => a.Descriptor.FieldNumber == field.FieldNumber);
    
    var value = accessor.GetValue(person);
}
```

## 性能考虑

### 序列化性能

- **批量序列化**：使用复用的 `CodedOutputStream`
- **大小计算**：使用 `CalculateSize()` 预先分配缓冲区
- **避免频繁分配**：复用消息实例，使用 `MergeFrom` 和 `Clear`

### 反序列化性能

- **流式处理**：直接从流反序列化，避免中间缓冲区
- **选择性合并**：使用 `MergeFrom` 只更新需要的字段

### 内存管理

- **消息克隆**：使用 `Clone()` 创建深拷贝
- **字段清除**：使用 `Clear()` 方法重置字段
- **集合重用**：集合会自动管理，无需手动清空

## 常见问题

### Q: 如何处理可选字段？

在 proto3 中，所有字段都是可选的。使用 `Has` 方法检查字段是否有值：

```csharp
if (person.HasEmail)
{
    Console.WriteLine($"Email: {person.Email}");
}
```

### Q: 如何处理默认值？

proto3 使用类型默认值：
- 数值类型：0
- 布尔：false
- 字符串：空字符串 ""
- 字节：空字节 []

```csharp
var person = new Person();
Console.WriteLine(person.Name); // ""
Console.WriteLine(person.Id);   // 0
Console.WriteLine(person.HasName); // false（因为默认值）
```

### Q: 如何处理循环引用？

Protobuf 不支持循环引用。如果需要表示图结构，使用 ID 或其他引用机制。

### Q: 如何处理大型消息？

使用流式 API：

```csharp
using var stream = File.OpenRead("large-message.bin");
var message = Person.Parser.ParseFrom(stream);
```

### Q: AOT 友好性如何？

完全支持 NativeAOT 编译，所有反射 API 都是 AOT 安全的：

```csharp
// AOT 安全的元数据访问
var descriptor = Person.Descriptor;
var accessors = Person.GetFieldAccessors();
```

## 更多资源

- [README.md](README.md) - 项目概览
- [QUICKSTART.md](QUICKSTART.md) - 快速开始
- [reflection-api-guide.md](docs/reflection-api-guide.md) - 反射 API 详细指南
- [build-system-guide.md](docs/build-system-guide.md) - 构建系统指南
