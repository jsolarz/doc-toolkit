using DocToolkit.ifx.Events;

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

        // Example: Log all events for observability
        eventBus.Subscribe<IndexBuiltEvent>(evt =>
        {
            Console.WriteLine($"[Event] Index built: {evt.IndexPath} ({evt.EntryCount} entries, {evt.VectorCount} vectors)");
        });

        eventBus.Subscribe<GraphBuiltEvent>(evt =>
        {
            Console.WriteLine($"[Event] Graph built: {evt.GraphPath} ({evt.FileCount} files, {evt.EntityCount} entities, {evt.TopicCount} topics)");
        });

        eventBus.Subscribe<SummaryCreatedEvent>(evt =>
        {
            Console.WriteLine($"[Event] Summary created: {evt.SummaryPath} ({evt.FileCount} files)");
        });

        eventBus.Subscribe<DocumentProcessedEvent>(evt =>
        {
            // Log document processing (can be verbose, so only log important ones)
            if (evt.CharacterCount > 10000)
            {
                Console.WriteLine($"[Event] Large document processed: {evt.FilePath} ({evt.CharacterCount} chars)");
            }
        });
    }
}
