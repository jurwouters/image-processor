using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageProcessor.Domain.Operations;

public sealed class ImageOperationJsonConverter : JsonConverter<ImageOperation>
{
    private static readonly IReadOnlyDictionary<string, Type> TypeMap = BuildTypeMap();

    private static Dictionary<string, Type> BuildTypeMap()
        => typeof(ImageOperation).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false } && t.IsSubclassOf(typeof(ImageOperation)))
            .Select(t => (Type: t, Attr: t.GetCustomAttributes(typeof(ImageOperationTypeAttribute), false)
                .OfType<ImageOperationTypeAttribute>()
                .FirstOrDefault()))
            .Where(x => x.Attr is not null)
            .ToDictionary(x => x.Attr!.TypeName.ToLowerInvariant(), x => x.Type);

    public override ImageOperation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeProp))
        {
            throw new JsonException("Missing 'type' discriminator on ImageOperation.");
        }

        var typeName = typeProp.GetString()
            ?? throw new JsonException("'type' discriminator is null.");

        if (!TypeMap.TryGetValue(typeName.ToLowerInvariant(), out var concreteType))
        {
            throw new JsonException($"Unknown ImageOperation type: '{typeName}'.");
        }

        return (ImageOperation?)root.Deserialize(concreteType, options);
    }

    public override void Write(Utf8JsonWriter writer, ImageOperation value, JsonSerializerOptions options)
    {
        var attr = value.GetType()
            .GetCustomAttributes(typeof(ImageOperationTypeAttribute), false)
            .OfType<ImageOperationTypeAttribute>()
            .FirstOrDefault()
            ?? throw new JsonException($"Missing [ImageOperationType] on {value.GetType().Name}.");

        writer.WriteStartObject();
        writer.WriteString("type", attr.TypeName);

        using var doc = JsonSerializer.SerializeToDocument(value, value.GetType(), options);
        foreach (var prop in doc.RootElement.EnumerateObject())
            prop.WriteTo(writer);

        writer.WriteEndObject();
    }
}
