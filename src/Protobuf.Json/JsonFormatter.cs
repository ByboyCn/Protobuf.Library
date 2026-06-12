using System.Text;
using System.Text.Json;
using Protobuf.Core;

namespace Protobuf.Json;

/// <summary>
/// Protobuf JSON 格式化器（支持 PascalCase）
/// </summary>
public static class JsonFormatter
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = null, // 使用原始属性名（PascalCase）
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// 将消息序列化为 JSON（PascalCase）
    /// </summary>
    public static string ToJson<T>(T message) where T : IMessage<T>
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();

        // 使用反射或生成的代码来写入字段
        WriteMessageFields(writer, message);

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// 从 JSON 反序列化消息（PascalCase）
    /// </summary>
    public static T FromJson<T>(string json) where T : IMessage<T>, new()
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("JSON string cannot be null or empty", nameof(json));
        }

        var message = new T();
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Expected JSON object");
        }

        // 使用反射或生成的代码来读取字段
        ReadMessageFields(root, message);

        return message;
    }

    /// <summary>
    /// 写入消息字段到 JSON writer
    /// </summary>
    private static void WriteMessageFields<T>(Utf8JsonWriter writer, T message) where T : IMessage<T>
    {
        // 这里需要使用反射或生成的代码
        // 作为基础实现，我们先提供接口

        var size = message.CalculateSize();
        if (size == 0)
        {
            return;
        }

        // 示例：如果消息实现了特定的 JSON 接口
        if (message is IJsonWritable jsonWritable)
        {
            jsonWritable.WriteJsonFields(writer);
        }
        else
        {
            // 默认实现：使用反射（性能较低）
            WriteFieldsUsingReflection(writer, message);
        }
    }

    /// <summary>
    /// 使用反射写入字段（基础实现）
    /// </summary>
    private static void WriteFieldsUsingReflection(Utf8JsonWriter writer, object message)
    {
        // 获取所有公共属性
        var properties = message.GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && p.Name != "Parser");

        foreach (var property in properties)
        {
            var value = property.GetValue(message);
            if (value != null)
            {
                writer.WritePropertyName(property.Name);

                if (value is IMessage msg)
                {
                    ToJsonInternal(writer, msg);
                }
                else if (value is System.Collections.IEnumerable enumerable && value is not string)
                {
                    WriteArray(writer, enumerable);
                }
                else
                {
                    JsonSerializer.Serialize(writer, value, value.GetType());
                }
            }
        }
    }

    /// <summary>
    /// 递归写入消息到 JSON writer（非泛型版本）
    /// </summary>
    private static void ToJsonInternal(Utf8JsonWriter writer, object message)
    {
        if (message == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        if (message is IJsonWritable jsonWritable)
        {
            jsonWritable.WriteJsonFields(writer);
        }
        else
        {
            WriteFieldsUsingReflection(writer, message);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// 写入数组到 JSON writer
    /// </summary>
    private static void WriteArray(Utf8JsonWriter writer, System.Collections.IEnumerable enumerable)
    {
        writer.WriteStartArray();

        foreach (var item in enumerable)
        {
            if (item is IMessage msg)
            {
                ToJsonInternal(writer, msg);
            }
            else
            {
                JsonSerializer.Serialize(writer, item, item?.GetType() ?? typeof(object));
            }
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// 从 JSON 读取消息字段
    /// </summary>
    private static void ReadMessageFields(JsonElement element, object message)
    {
        if (message is IJsonReadable jsonReadable)
        {
            jsonReadable.ReadJsonFields(element);
        }
        else
        {
            // 默认实现：使用反射
            ReadFieldsUsingReflection(element, message);
        }
    }

    /// <summary>
    /// 使用反射读取字段（基础实现）
    /// </summary>
    private static void ReadFieldsUsingReflection(JsonElement element, object message)
    {
        var properties = message.GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.Name != "Parser")
            .ToDictionary(p => p.Name, p => p);

        foreach (var property in element.EnumerateObject())
        {
            if (properties.TryGetValue(property.Name, out var propInfo))
            {
                var value = JsonSerializer.Deserialize(
                    property.Value.GetRawText(),
                    propInfo.PropertyType,
                    _options);

                if (value != null)
                {
                    // 尝试调用 Set{PropertyName} 方法
                    var setter = message.GetType().GetMethod($"Set{property.Name}");
                    if (setter != null)
                    {
                        setter.Invoke(message, new[] { value });
                    }
                    else
                    {
                        // 直接设置属性值
                        propInfo.SetValue(message, value);
                    }
                }
            }
        }
    }
}

/// <summary>
/// 可 JSON 写入的消息接口
/// </summary>
public interface IJsonWritable
{
    void WriteJsonFields(Utf8JsonWriter writer);
}

/// <summary>
/// 可 JSON 读取的消息接口
/// </summary>
public interface IJsonReadable
{
    void ReadJsonFields(JsonElement element);
}
