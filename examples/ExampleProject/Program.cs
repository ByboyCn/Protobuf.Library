using System;
using Protobuf.Core; // 引入核心库

// 注意：生成的代码会在 Example.Common 和 Example.Api 命名空间中
// 它们会自动可用，因为源代码生成器会生成这些类

namespace ExampleProject;

/// <summary>
/// 示例程序：展示如何使用源代码生成器生成的类
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Protobuf Library - 源代码生成器示例");
        Console.WriteLine("=====================================");
        Console.WriteLine();

        // 一旦源代码生成器开始工作，你可以这样使用生成的代码：

        /*
        // 示例 1：创建用户请求
        var request = new Example.Api.CreateUserRequest();
        request.SetName("张三");
        request.SetEmail("zhangsan@example.com");
        request.AddInitialRoles("user");
        request.AddInitialRoles("admin");

        // 示例 2：序列化
        byte[] data = request.Serialize();

        Console.WriteLine($"序列化结果：{BitConverter.ToString(data)}");
        Console.WriteLine($"数据长度：{data.Length} 字节");
        Console.WriteLine();

        // 示例 3：反序列化
        var request2 = Example.Api.CreateUserRequest.Parser.ParseFrom(data);
        Console.WriteLine($"反序列化结果：");
        Console.WriteLine($"  姓名：{request2.Name}");
        Console.WriteLine($"  邮箱：{request2.Email}");
        Console.WriteLine($"  角色：{string.Join(", ", request2.InitialRoles)}");
        Console.WriteLine();

        // 示例 4：创建用户响应
        var user = new Example.Api.User();
        user.SetId(12345);
        user.SetName("张三");
        user.SetEmail("zhangsan@example.com");
        user.AddRoles("user");
        user.AddRoles("admin");

        var response = new Example.Api.CreateUserResponse();
        var result = new Example.Common.Result();
        result.SetSuccess(true);
        result.SetMessage("用户创建成功");
        response.SetResult(result);
        response.SetUser(user);

        // 示例 5：检查字段是否有值
        if (response.HasResult)
        {
            Console.WriteLine("响应包含结果字段");
            if (response.Result.HasSuccess && response.Result.Success)
            {
                Console.WriteLine("操作成功！");
            }
            if (response.Result.HasMessage)
            {
                Console.WriteLine($"消息：{response.Result.Message}");
            }
        }

        if (response.HasUser)
        {
            Console.WriteLine($"创建的用户 ID：{response.User.Id}");
            Console.WriteLine($"创建的用户名：{response.User.Name}");
        }

        Console.WriteLine();
        Console.WriteLine("✅ 源代码生成器让使用 Protobuf 变得简单！");
        */

        Console.WriteLine("提示：构建此项目后，生成的代码将自动可用。");
        Console.WriteLine("取消注释上面的代码来查看生成的类如何工作。");
    }
}
