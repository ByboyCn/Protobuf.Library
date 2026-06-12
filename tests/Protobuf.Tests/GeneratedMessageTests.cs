using Xunit;
using Xunit.Abstractions;

namespace Protobuf.Tests;

/// <summary>
/// 生成的消息类测试（使用 person.proto）
/// </summary>
public class GeneratedMessageTests
{
    private readonly ITestOutputHelper _output;

    public GeneratedMessageTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestPerson_BasicFields()
    {
        // 注意：这个测试需要在 person.proto 被成功生成代码后才能运行
        // 目前我们只是验证生成的代码结构是否正确

        // 一旦代码生成完成，测试应该像这样：
        /*
        var person = new Example.Person();

        // 测试默认值
        Assert.False(person.HasName);
        Assert.False(person.HasId);

        // 设置值
        person.SetName("Alice");
        person.SetId(12345);

        // 验证 has 方法
        Assert.True(person.HasName);
        Assert.True(person.HasId);

        // 验证值
        Assert.Equal("Alice", person.Name);
        Assert.Equal(12345, person.Id);

        // 清除值
        person.ClearName();
        person.ClearId();

        Assert.False(person.HasName);
        Assert.False(person.HasId);
        */

        _output.WriteLine("Person 基础字段测试需要先完成代码生成器");
    }

    [Fact]
    public void TestPerson_RepeatedFields()
    {
        // 注意：这个测试需要在代码生成完成后才能运行

        /*
        var person = new Example.Person();

        // 测试默认值
        Assert.False(person.HasPhoneNumbers);

        // 添加元素
        person.AddPhoneNumbers("555-1234");
        person.AddPhoneNumbers("555-5678");

        // 验证 has 方法
        Assert.True(person.HasPhoneNumbers);

        // 验证值
        Assert.Equal(2, person.PhoneNumbers.Count);
        Assert.Contains("555-1234", person.PhoneNumbers);
        Assert.Contains("555-5678", person.PhoneNumbers);

        // 清除
        person.ClearPhoneNumbers();
        Assert.False(person.HasPhoneNumbers);
        Assert.Empty(person.PhoneNumbers);
        */

        _output.WriteLine("Person Repeated 字段测试需要先完成代码生成器");
    }

    [Fact]
    public void TestPerson_OneOfFields()
    {
        // 注意：这个测试需要在代码生成完成后才能运行

        /*
        var person = new Example.Person();

        // 测试默认值
        Assert.False(person.HasHomeAddress);
        Assert.False(person.HasWorkAddress);
        Assert.Equal(Example.Person.AddressCase.None, person.AddressCase);

        // 设置 home_address
        person.SetHomeAddress("123 Home St");

        Assert.True(person.HasHomeAddress);
        Assert.False(person.HasWorkAddress);
        Assert.Equal("123 Home St", person.HomeAddress);
        Assert.Equal(Example.Person.AddressCase.HomeAddress, person.AddressCase);

        // 切换到 work_address（应该清除 home_address）
        person.SetWorkAddress("456 Work Ave");

        Assert.False(person.HasHomeAddress);
        Assert.True(person.HasWorkAddress);
        Assert.Equal("456 Work Ave", person.WorkAddress);
        Assert.Equal(Example.Person.AddressCase.WorkAddress, person.AddressCase);

        // 清除 oneof
        person.ClearAddress();

        Assert.False(person.HasHomeAddress);
        Assert.False(person.HasWorkAddress);
        Assert.Equal(Example.Person.AddressCase.None, person.AddressCase);
        */

        _output.WriteLine("Person OneOf 字段测试需要先完成代码生成器");
    }

    [Fact]
    public void TestPerson_MapFields()
    {
        // 注意：这个测试需要在代码生成完成后才能运行

        /*
        var person = new Example.Person();

        // 测试默认值
        Assert.False(person.HasMetadata);
        Assert.Empty(person.Metadata);

        // 添加元素
        person.AddMetadata("department", "Engineering");
        person.AddMetadata("title", "Developer");

        // 验证 has 方法
        Assert.True(person.HasMetadata);

        // 验证值
        Assert.Equal(2, person.Metadata.Count);
        Assert.Equal("Engineering", person.Metadata["department"]);
        Assert.Equal("Developer", person.Metadata["title"]);

        // 覆盖现有值
        person.AddMetadata("department", "Sales");
        Assert.Equal("Sales", person.Metadata["department"]);

        // 清除
        person.ClearMetadata();
        Assert.False(person.HasMetadata);
        Assert.Empty(person.Metadata);
        */

        _output.WriteLine("Person Map 字段测试需要先完成代码生成器");
    }

    [Fact]
    public void TestPerson_Clone()
    {
        // 注意：这个测试需要在代码生成完成后才能运行

        /*
        var person1 = new Example.Person();
        person1.SetName("Alice");
        person1.SetId(12345);
        person1.AddPhoneNumbers("555-1234");

        var person2 = person1.Clone();

        // 验证值相同
        Assert.Equal(person1.Name, person2.Name);
        Assert.Equal(person1.Id, person2.Id);
        Assert.Equal(person1.PhoneNumbers.Count, person2.PhoneNumbers.Count);

        // 验证是不同的实例
        Assert.NotSame(person1, person2);

        // 修改 person1 不影响 person2
        person1.SetName("Bob");
        Assert.Equal("Alice", person2.Name);
        */

        _output.WriteLine("Person Clone 测试需要先完成代码生成器");
    }
}
