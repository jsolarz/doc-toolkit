using DocToolkit.ifx.Commands;
using DocToolkit.ifx.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

// Setup dependency injection
var services = new ServiceCollection();
services.AddDocToolkitServices();
var serviceProvider = services.BuildServiceProvider();

// Configure event subscriptions for cross-manager communication
var eventBus = serviceProvider.GetRequiredService<IEventBus>();
EventSubscriptions.ConfigureSubscriptions(eventBus, serviceProvider);

// Create type registrar for Spectre.Console.Cli
var typeRegistrar = new CommandTypeRegistrar(services);

// Create command app with DI support
var app = new CommandApp(typeRegistrar);

app.Configure(config =>
{
    config.SetApplicationName("doc");
    config.ValidateExamples();

    config.AddCommand<InitCommand>("init")
        .WithDescription("Initialize a new documentation project")
        .WithExample(["init", "MyProject"]);

    config.AddCommand<GenerateCommand>("generate")
        .WithAlias("gen")
        .WithDescription("Generate a document from a template")
        .WithExample(["generate", "prd", "User Management"])
        .WithExample(["gen", "sow", "Cloud Migration"]);

    config.AddCommand<IndexCommand>("index")
        .WithDescription("Build semantic index from source files")
        .WithExample(["index"]);

    config.AddCommand<SearchCommand>("search")
        .WithDescription("Search the semantic index")
        .WithExample(["search", "customer requirements"]);

    config.AddCommand<GraphCommand>("graph")
        .WithDescription("Build knowledge graph from source files")
        .WithExample(["graph"]);

    config.AddCommand<SummarizeCommand>("summarize")
        .WithAlias("sum")
        .WithDescription("Summarize source files into context document")
        .WithExample(["summarize"]);

    config.AddCommand<ValidateCommand>("validate")
        .WithDescription("Validate setup and dependencies")
        .WithExample(["validate"]);
});

// Show banner
AnsiConsole.Write(
    new FigletText("Doc Toolkit")
        .LeftJustified()
        .Color(Color.Cyan1));

AnsiConsole.MarkupLine("[dim]Professional documentation generation made beautiful[/]");
AnsiConsole.WriteLine();

try
{
    return app.Run(args);
}
finally
{
    // Type resolver is disposed by CommandApp
}
