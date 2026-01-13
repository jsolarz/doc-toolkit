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

    // Semantic Intelligence Features - Removed for now, see Future Enhancements in README
    // config.AddCommand<IndexCommand>("index")
    //     .WithDescription("Build semantic index from source files")
    //     .WithExample(["index"]);
    //
    // config.AddCommand<SearchCommand>("search")
    //     .WithDescription("Search the semantic index")
    //     .WithExample(["search", "customer requirements"]);
    //
    // config.AddCommand<GraphCommand>("graph")
    //     .WithDescription("Build knowledge graph from source files")
    //     .WithExample(["graph"]);
    //
    // config.AddCommand<SummarizeCommand>("summarize")
    //     .WithAlias("sum")
    //     .WithDescription("Summarize source files into context document")
    //     .WithExample(["summarize"]);

    config.AddCommand<BuildCommand>("build")
        .WithDescription("Build static site from markdown files")
        .WithExample(["build"])
        .WithExample(["build", "--source", "./docs", "--output", "./publish/web"]);

    config.AddCommand<LintCommand>("lint")
        .WithAlias("check")
        .WithDescription("Validate document quality and compliance")
        .WithExample(["lint"])
        .WithExample(["lint", "./docs"])
        .WithExample(["lint", "./docs", "--template", "prd"])
        .WithExample(["lint", "./docs", "--strict"]);

    config.AddCommand<ListCommand>("list")
        .WithDescription("List documents with metadata")
        .WithExample(["list"])
        .WithExample(["list", "--dir", "./docs"])
        .WithExample(["list", "--filter", "prd"])
        .WithExample(["list", "--format", "tree"]);

    config.AddCommand<InfoCommand>("info")
        .WithDescription("Show detailed information about a document")
        .WithExample(["info", "prd-feature.md"])
        .WithExample(["info", "docs/prd-feature.md", "--dir", "./"]);

    config.AddCommand<ValidateCommand>("validate")
        .WithDescription("Validate setup and dependencies")
        .WithExample(["validate"]);

    config.AddCommand<WebCommand>("web")
        .WithDescription("Start web server to view and share documents")
        .WithExample(["web"])
        .WithExample(["web", "--port", "8080"])
        .WithExample(["web", "--host", "0.0.0.0", "--port", "5000"]);

    config.AddCommand<PublishCommand>("publish")
        .WithDescription("Publish documentation in various formats for deployment")
        .WithExample(["publish"])
        .WithExample(["publish", "--format", "web"])
        .WithExample(["publish", "--format", "all", "--target", "azure"])
        .WithExample(["publish", "--format", "pdf"]);
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
