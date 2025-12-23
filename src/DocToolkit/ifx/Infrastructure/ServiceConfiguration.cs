using Microsoft.Extensions.DependencyInjection;
using DocToolkit.Interfaces.Managers;
using DocToolkit.Interfaces.Engines;
using DocToolkit.Interfaces.Accessors;
using DocToolkit.Managers;
using DocToolkit.Engines;
using DocToolkit.Accessors;
using DocToolkit.Services.Engines;

namespace DocToolkit.Infrastructure;

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
            dbPath: eventDbPath,
            maxRetries: 3,
            retryInterval: TimeSpan.FromMinutes(5)));

        // Engines (Singleton - stateless or expensive to create)
        services.AddSingleton<IDocumentExtractionEngine, DocumentExtractionEngine>();
        services.AddSingleton<IEmbeddingEngine, EmbeddingEngine>();
        services.AddSingleton<ITextChunkingEngine, TextChunkingEngine>();
        services.AddSingleton<ISimilarityEngine, SimilarityEngine>();
        services.AddSingleton<IEntityExtractionEngine, EntityExtractionEngine>();
        services.AddSingleton<ISummarizationEngine, SummarizationEngine>();

        // Accessors (Singleton - stateless)
        services.AddSingleton<IVectorStorageAccessor, VectorStorageAccessor>();
        services.AddSingleton<ITemplateAccessor, TemplateAccessor>();
        services.AddSingleton<IProjectAccessor, ProjectAccessor>();

        // Managers (Scoped - may have state per operation)
        services.AddScoped<ISemanticIndexManager, SemanticIndexManager>();
        services.AddScoped<ISemanticSearchManager, SemanticSearchManager>();
        services.AddScoped<IKnowledgeGraphManager, KnowledgeGraphManager>();
        services.AddScoped<ISummarizeManager, SummarizeManager>();

        return services;
    }
}
