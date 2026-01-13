using DocToolkit.Accessors;
using DocToolkit.Engines;
using DocToolkit.ifx.Interfaces.IAccessors;
using DocToolkit.ifx.Interfaces.IEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocToolkit.ifx.Infrastructure;

/// <summary>
/// Service configuration for dependency injection.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configures all services in the DI container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDocToolkitServices(this IServiceCollection services)
    {
        // Configure logging (if not already configured)
        if (!services.Any(sd => sd.ServiceType == typeof(ILoggerFactory)))
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
        }
        // Infrastructure - Event Bus with persistence
        var eventDbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DocToolkit",
            "events.db");
        
        // Ensure directory exists
        var eventDbDir = Path.GetDirectoryName(eventDbPath);
        if (!string.IsNullOrEmpty(eventDbDir) && !Directory.Exists(eventDbDir))
        {
            Directory.CreateDirectory(eventDbDir);
        }

        services.AddSingleton<IEventBus>(sp => new EventBus(
            logger: sp.GetRequiredService<ILogger<EventBus>>(),
            dbPath: eventDbPath,
            maxRetries: 3,
            retryInterval: TimeSpan.FromMinutes(5)));

        // Engines (Singleton - stateless or expensive to create)
        services.AddSingleton<IDocumentExtractionEngine>(sp => 
            new DocumentExtractionEngine(sp.GetService<ILogger<DocumentExtractionEngine>>()));
        
        // Semantic Intelligence Engines - Removed for now, see Future Enhancements in README
        // services.AddSingleton<IEmbeddingEngine>(sp => 
        //     new EmbeddingEngine(null, sp.GetService<ILogger<EmbeddingEngine>>()));
        // services.AddSingleton<ITextChunkingEngine, TextChunkingEngine>();
        // services.AddSingleton<ISimilarityEngine, SimilarityEngine>();
        // services.AddSingleton<IEntityExtractionEngine, EntityExtractionEngine>();
        // services.AddSingleton<ISummarizationEngine, SummarizationEngine>();

        // Accessors (Singleton - stateless)
        // Semantic Intelligence Accessors - Removed for now, see Future Enhancements in README
        // services.AddSingleton<IVectorStorageAccessor>(sp => 
        //     new VectorStorageAccessor(sp.GetService<ILogger<VectorStorageAccessor>>()));
        services.AddSingleton<ITemplateAccessor, TemplateAccessor>();
        services.AddSingleton<IProjectAccessor, ProjectAccessor>();

        // Managers (Scoped - may have state per operation)
        // Semantic Intelligence Managers - Removed for now, see Future Enhancements in README
        // services.AddScoped<ISemanticIndexManager, SemanticIndexManager>();
        // services.AddScoped<ISemanticSearchManager, SemanticSearchManager>();
        // services.AddScoped<IKnowledgeGraphManager, KnowledgeGraphManager>();
        // services.AddScoped<ISummarizeManager, SummarizeManager>();

        return services;
    }
}
