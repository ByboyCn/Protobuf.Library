using System;
using Protobuf.Core;

class Program
{
    static void Main()
    {
        var value = 1234567890123456789L;
        Console.WriteLine($"Value: {value}");
        Console.WriteLine($"Value (hex): 0x{value:X}");

        var output = new CodedOutputStream();
        output.WriteInt64(value);
        var data = output.ToByteArray();

        Console.WriteLine($"Data: {BitConverter.ToString(data)}");
        Console.WriteLine($"Data length: {data.Length}");
    }
}
