using DocToolkit.ifx.Events;
using Microsoft.Extensions.Logging;

namespace DocToolkit.ifx.Infrastructure;

/// <summary>
/// Configures event subscriptions for cross-manager communication.
/// This enables decoupled communication between managers without direct dependencies.
/// </summary>
public static class EventSubscriptions
{
    /// <summary>
    /// Sets up all event subscriptions for the application.
    /// </summary>
    /// <param name="eventBus">Event bus instance</param>
    /// <param name="serviceProvider">Service provider for resolving managers</param>
    public static void ConfigureSubscriptions(IEventBus eventBus, IServiceProvider serviceProvider)
    {
        // Get logger factory and create logger (EventSubscriptions is static, so we use a string category)
        var loggerFactory = serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
        var logger = loggerFactory?.CreateLogger("DocToolkit.Events");
        // Example: KnowledgeGraphManager could subscribe to IndexBuiltEvent
        // to automatically rebuild graph when index changes
        // This is commented out as it's optional - uncomment if needed
        
        /*
        eventBus.Subscribe<IndexBuiltEvent>(async (evt) =>
        {
            // Optional: Auto-rebuild graph when index is built
            // var graphManager = serviceProvider.GetService<IKnowledgeGraphManager>();
            // if (graphManager != null)
            // {
            //     await graphManager.BuildGraphAsync(evt.SourcePath, "./knowledge-graph");
            // }
        });
        */

        // Log all events for observability (internal logging, not user-facing)
        eventBus.Subscribe<IndexBuiltEvent>(evt =>
        {
            logger?.LogInformation("Index built: {IndexPath} ({EntryCount} entries, {VectorCount} vectors)", 
                evt.IndexPath, evt.EntryCount, evt.VectorCount);
        });

        eventBus.Subscribe<GraphBuiltEvent>(evt =>
        {
            logger?.LogInformation("Graph built: {GraphPath} ({FileCount} files, {EntityCount} entities, {TopicCount} topics)", 
                evt.GraphPath, evt.FileCount, evt.EntityCount, evt.TopicCount);
        });

        eventBus.Subscribe<SummaryCreatedEvent>(evt =>
        {
            logger?.LogInformation("Summary created: {SummaryPath} ({FileCount} files)", 
                evt.SummaryPath, evt.FileCount);
        });

        eventBus.Subscribe<DocumentProcessedEvent>(evt =>
        {
            // Log document processing (can be verbose, so only log important ones)
            if (evt.CharacterCount > 10000)
            {
                logger?.LogDebug("Large document processed: {FilePath} ({CharacterCount} chars)", 
                    evt.FilePath, evt.CharacterCount);
            }
        });
    }
}
