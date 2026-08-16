namespace ImageProcessor.Domain.Operations;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ImageOperationTypeAttribute(string typeName) : Attribute
{
    public string TypeName { get; } = typeName;
}
