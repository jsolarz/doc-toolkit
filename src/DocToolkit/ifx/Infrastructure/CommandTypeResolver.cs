using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Infrastructure;

/// <summary>
/// Type registrar for Spectre.Console.Cli that uses dependency injection.
/// </summary>
public class CommandTypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the CommandTypeRegistrar.
    /// </summary>
    /// <param name="services">Service collection</param>
    public CommandTypeRegistrar(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Registers a service with the DI container.
    /// </summary>
    /// <param name="service">Service type</param>
    /// <param name="implementation">Implementation type</param>
    public void Register(Type service, Type implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    /// <summary>
    /// Registers an instance with the DI container.
    /// </summary>
    /// <param name="service">Service type</param>
    /// <param name="implementation">Implementation instance</param>
    public void RegisterInstance(Type service, object implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    /// <summary>
    /// Registers a lazy factory with the DI container.
    /// </summary>
    /// <param name="service">Service type</param>
    /// <param name="factory">Factory function</param>
    public void RegisterLazy(Type service, Func<object> factory)
    {
        _services.AddSingleton(service, _ => factory());
    }

    /// <summary>
    /// Builds the service provider and returns a type resolver.
    /// </summary>
    /// <returns>Type resolver</returns>
    public ITypeResolver Build()
    {
        return new CommandTypeResolver(_services.BuildServiceProvider());
    }
}

/// <summary>
/// Type resolver for Spectre.Console.Cli that uses dependency injection.
/// </summary>
public class CommandTypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the CommandTypeResolver.
    /// </summary>
    /// <param name="serviceProvider">Service provider from DI container</param>
    public CommandTypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Resolves a type from the DI container.
    /// </summary>
    /// <param name="type">Type to resolve</param>
    /// <returns>Resolved instance or null</returns>
    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        return _serviceProvider.GetService(type);
    }

    /// <summary>
    /// Disposes the service provider if it's disposable.
    /// </summary>
    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
