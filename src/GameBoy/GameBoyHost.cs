using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace GameBoy;

public static class GameBoyHost
{
    public static GameBoyHostBuilder CreateBuilder()
    {
        var builder = new GameBoyHostBuilder();
        builder.Services.ConfigureDefaults();
        return builder;
    }

    extension(IServiceCollection services)
    {
        private void ConfigureDefaults()
        {
            services.Scan(scan => scan
                .FromAssemblyOf<GameBoyHostBuilder>()
                .AddClasses(classes => classes.WithAttribute<ServiceAttribute>())
                .As(type => type.GetCustomAttribute<ServiceAttribute>()?.ServiceTypes is { Length: > 0 } types ? types : [type])
                .WithLifetime(type => type.GetCustomAttribute<ServiceAttribute>()?.Lifetime ?? ServiceLifetime.Transient));
        }
    }
}

public class GameBoyHostBuilder : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _hostApplicationBuilder = Host.CreateApplicationBuilder();

    /// <inheritdoc />
    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull => _hostApplicationBuilder.ConfigureContainer(factory, configure);

    /// <inheritdoc />
    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_hostApplicationBuilder).Properties;

    /// <inheritdoc />
    public IConfigurationManager Configuration => ((IHostApplicationBuilder)_hostApplicationBuilder).Configuration;

    /// <inheritdoc />
    public IHostEnvironment Environment => _hostApplicationBuilder.Environment;

    /// <inheritdoc />
    public ILoggingBuilder Logging => _hostApplicationBuilder.Logging;

    /// <inheritdoc />
    public IMetricsBuilder Metrics => _hostApplicationBuilder.Metrics;

    /// <inheritdoc />
    public IServiceCollection Services => _hostApplicationBuilder.Services;

    public IHost Build() => _hostApplicationBuilder.Build();
}
