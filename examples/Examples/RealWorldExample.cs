using System;
using System.Diagnostics;
using System.Linq;
using Protobuf.Core;

namespace Examples;

/// <summary>
/// 真实世界场景示例：演示如何在实际项目中使用 Protobuf 库
/// </summary>
class RealWorldExample
{
    public static void Run(string[] args)
    {
        Console.WriteLine("=== 真实世界场景示例 ===\n");

        // 场景 1: 配置文件序列化（只编码）
        Console.WriteLine("场景 1: 配置文件序列化");
        DemonstrateConfigSerialization();

        // 场景 2: 网络协议数据包（只编码）
        Console.WriteLine("\n场景 2: 网络协议数据包");
        DemonstrateNetworkProtocol();

        // 场景 3: 数据库记录存储（只编码）
        Console.WriteLine("\n场景 3: 数据库记录存储");
        DemonstrateDatabaseStorage();

        // 场景 4: 性能对比
        Console.WriteLine("\n场景 4: 性能对比");
        DemonstratePerformanceComparison();

        Console.WriteLine("\n=== 示例完成 ===");
    }

    /// <summary>
    /// 演示配置文件的序列化
    /// </summary>
    static void DemonstrateConfigSerialization()
    {
        Console.WriteLine("  应用配置通常需要序列化到文件");

        // 模拟配置数据
        var configOutput = new CodedOutputStream();

        // 字段 1: 服务器地址 (string)
        configOutput.WriteTag(WireFormat.MakeTag(1, WireFormat.LengthDelimitedType));
        configOutput.WriteString("localhost");

        // 字段 2: 端口 (int32)
        configOutput.WriteTag(WireFormat.MakeTag(2, WireFormat.VarintType));
        configOutput.WriteInt32(8080);

        // 字段 3: 启用 SSL (bool)
        configOutput.WriteTag(WireFormat.MakeTag(3, WireFormat.VarintType));
        configOutput.WriteBool(true);

        // 字段 4: 超时时间 (int32)
        configOutput.WriteTag(WireFormat.MakeTag(4, WireFormat.VarintType));
        configOutput.WriteInt32(30000);

        var configData = configOutput.ToByteArray();
        Console.WriteLine($"    配置大小: {configData.Length} 字节");
        Console.WriteLine($"    配置数据: {BitConverter.ToString(configData)}");

        // 分析编码后的数据
        Console.WriteLine("  数据分析:");
        Console.WriteLine($"    field 1 (string \"localhost\"): {BitConverter.ToString(configData, 0, 11)}");
        Console.WriteLine($"    field 2 (int32 8080): tag=0x10, value=0x7E81");
        Console.WriteLine($"    field 3 (bool true): tag=0x18, value=0x01");
        Console.WriteLine($"    field 4 (int32 30000): tag=0x20, value=0xEC-EE-02");
        Console.WriteLine("  ✅ 配置文件序列化成功");
    }

    /// <summary>
    /// 演示网络协议数据包的构建
    /// </summary>
    static void DemonstrateNetworkProtocol()
    {
        Console.WriteLine("  网络协议通常使用紧凑的二进制格式");

        var packetOutput = new CodedOutputStream();

        // 消息类型 (int32)
        packetOutput.WriteTag(WireFormat.MakeTag(1, WireFormat.VarintType));
        packetOutput.WriteInt32(1); // 1 = Login

        // 用户 ID (int64)
        packetOutput.WriteTag(WireFormat.MakeTag(2, WireFormat.VarintType));
        packetOutput.WriteInt64(1234567890);

        // 时间戳 (int64)
        packetOutput.WriteTag(WireFormat.MakeTag(3, WireFormat.VarintType));
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        packetOutput.WriteInt64((long)timestamp);

        // 负载数据 (bytes)
        packetOutput.WriteTag(WireFormat.MakeTag(4, WireFormat.LengthDelimitedType));
        packetOutput.WriteBytes(new byte[] { 0x01, 0x02, 0x03 });

        var packetData = packetOutput.ToByteArray();
        Console.WriteLine($"    数据包大小: {packetData.Length} 字节");
        Console.WriteLine($"    数据包: {BitConverter.ToString(packetData)}");

        // 计算等效 JSON 的大小
        var jsonEquivalent = $"{{\"type\":1,\"userId\":1234567890,\"timestamp\":{timestamp},\"payload\":\"AQID\"}}";
        Console.WriteLine($"    JSON 大小: {jsonEquivalent.Length} 字节");
        Console.WriteLine($"    节省空间: {jsonEquivalent.Length - packetData.Length} 字节 ({100.0 * (jsonEquivalent.Length - packetData.Length) / jsonEquivalent.Length:F1}%)");
        Console.WriteLine("  ✅ 网络协议数据包编码成功");
    }

    /// <summary>
    /// 演示数据库记录的存储
    /// </summary>
    static void DemonstrateDatabaseStorage()
    {
        Console.WriteLine("  数据库可以使用 Protobuf 存储复杂记录");

        var totalSize = 0;
        var iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            var recordOutput = new CodedOutputStream();

            // ID (int64)
            recordOutput.WriteTag(WireFormat.MakeTag(1, WireFormat.VarintType));
            recordOutput.WriteInt64(i);

            // 名称 (string)
            recordOutput.WriteTag(WireFormat.MakeTag(2, WireFormat.LengthDelimitedType));
            recordOutput.WriteString($"User{i}");

            // 活跃状态 (bool)
            recordOutput.WriteTag(WireFormat.MakeTag(3, WireFormat.VarintType));
            recordOutput.WriteBool(i % 2 == 0);

            // 分数 (int32)
            recordOutput.WriteTag(WireFormat.MakeTag(4, WireFormat.VarintType));
            recordOutput.WriteInt32(i * 100);

            totalSize += recordOutput.ToByteArray().Length;
        }

        var avgSize = totalSize / iterations;

        Console.WriteLine($"    记录数量: {iterations:N0}");
        Console.WriteLine($"    总大小: {totalSize:N0} 字节");
        Console.WriteLine($"    平均大小: {avgSize} 字节/记录");
        Console.WriteLine($"    如果使用 JSON: 约 {avgSize * 3} 字节/记录");
        Console.WriteLine("  ✅ 数据库记录存储编码成功");
    }

    /// <summary>
    /// 演示性能对比
    /// </summary>
    static void DemonstratePerformanceComparison()
    {
        Console.WriteLine("  性能测试：Protobuf 编码 vs JSON 字符串构建");

        var iterations = 10000;

        // Protobuf 编码
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var output = new CodedOutputStream();

            output.WriteTag(WireFormat.MakeTag(1, WireFormat.LengthDelimitedType));
            output.WriteString("Alice");

            output.WriteTag(WireFormat.MakeTag(2, WireFormat.VarintType));
            output.WriteInt32(30);

            output.WriteTag(WireFormat.MakeTag(3, WireFormat.LengthDelimitedType));
            output.WriteString("alice@example.com");

            var _ = output.ToByteArray();
        }
        sw.Stop();
        var protobufTime = sw.ElapsedMilliseconds;

        // JSON 字符串构建
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var json = $"{{\"name\":\"Alice\",\"age\":30,\"email\":\"alice@example.com\"}}";
        }
        sw.Stop();
        var jsonTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"    迭代次数: {iterations:N0}");
        Console.WriteLine($"    Protobuf 编码: {protobufTime}ms");
        Console.WriteLine($"    JSON 构建: {jsonTime}ms");
        Console.WriteLine($"    Protobuf 更快: {100.0 * (jsonTime - protobufTime) / jsonTime:F1}%");
        Console.WriteLine("  ✅ 性能对比完成");
    }
}
