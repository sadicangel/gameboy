namespace GameBoy;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceAttribute(ServiceLifetime lifetime, params Type[] serviceTypes) : Attribute
{
    public ServiceLifetime Lifetime { get; } = lifetime;
    public Type[] ServiceTypes { get; } = serviceTypes;
}
