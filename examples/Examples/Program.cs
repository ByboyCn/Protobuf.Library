using System;

namespace Examples;

/// <summary>
/// 示例程序的统一入口点
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }

        var example = args[0].ToLowerInvariant();

        try
        {
            switch (example)
            {
                case "parser":
                case "-p":
                    ProtoParserExample.Run(args);
                    break;

                case "codec":
                case "-c":
                    CodecExample.Run(args);
                    break;

                case "manual":
                case "-m":
                    ManualMessageExample.Run(args);
                    break;

                case "realworld":
                case "-r":
                    RealWorldExample.Run(args);
                    break;

                case "usage":
                case "-u":
                    UsageExample.Run(args);
                    break;

                default:
                    Console.WriteLine($"未知的示例: {example}");
                    PrintUsage();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"错误: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Protobuf.Library 示例程序");
        Console.WriteLine("==========================");
        Console.WriteLine();
        Console.WriteLine("使用方法:");
        Console.WriteLine("  dotnet run <示例名称>");
        Console.WriteLine();
        Console.WriteLine("可用示例:");
        Console.WriteLine("  parser | -p    - Proto 文件解析示例");
        Console.WriteLine("  codec  | -c    - 编解码器使用示例");
        Console.WriteLine("  manual | -m    - 手动消息处理示例");
        Console.WriteLine("  realworld | -r - 真实场景示例");
        Console.WriteLine("  usage  | -u    - 完整使用流程（需要代码生成器）");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  dotnet run parser");
        Console.WriteLine("  dotnet run codec");
        Console.WriteLine("  dotnet run manual");
        Console.WriteLine();
        Console.WriteLine("或者直接运行特定示例:");
        Console.WriteLine("  dotnet run --project Examples.csproj -- parser");
    }
}
