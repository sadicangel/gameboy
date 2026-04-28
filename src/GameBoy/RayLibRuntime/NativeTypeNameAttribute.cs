#nullable disable
namespace RayLibNet.Interop;

#pragma warning disable CS0649

[AttributeUsage(AttributeTargets.All)]
internal class NativeTypeNameAttribute(string nativeName) : Attribute
{
    public string NativeName { get; } = nativeName;
}
