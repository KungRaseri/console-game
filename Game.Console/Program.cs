using Game.Core.Services;
using Game.Core.Features.Combat;
using Game.Core.Features.Inventory;
using Game.Core.Features.CharacterCreation;
using Game.Core.Features.SaveLoad;
using Game.Core.Features.Exploration;
using Game.Core.Features.Death;
using Game.Core.Abstractions;
using Game.Shared.Services;
using Game.Shared.Behaviors;
using Game.Console;
using Game.Console.UI;
using Game.Console.Orchestrators;
using Game.Data.Repositories;
using Game.Core.Settings;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection;
using DotNetEnv;
using FluentValidation;
using System.Text;
using Spectre.Console;

// Set console encoding to UTF-8 to support emojis and special characters
System.Console.OutputEncoding = Encoding.UTF8;
System.Console.InputEncoding = Encoding.UTF8;

// Load .env file for secrets (not versioned)
Env.Load();

// Build configuration from appsettings.json + environment variables
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables() // Override with environment variables if present
    .Build();

// Initialize logging first
LoggingService.Initialize();

try
{
    Log.Information("Game starting - Version 1.0");
    Log.Debug("Configuration loaded from appsettings.json");

    // Setup dependency injection container
    var services = new ServiceCollection();

    // Register configuration sections as strongly-typed options
    // TODO: Fix configuration binding - requires Microsoft.Extensions.Configuration.Binder package
    // services.Configure<GameSettings>(opts => configuration.GetSection("Game").Bind(opts));
    // services.Configure<AudioSettings>(opts => configuration.GetSection("Audio").Bind(opts));
    // services.Configure<UISettings>(opts => configuration.GetSection("UI").Bind(opts));
    // services.Configure<LoggingSettings>(opts => configuration.GetSection("Logging").Bind(opts));
    // services.Configure<GameplaySettings>(opts => configuration.GetSection("Gameplay").Bind(opts));

    // Register validators
    services.AddSingleton<IValidator<GameSettings>, GameSettingsValidator>();
    services.AddSingleton<IValidator<AudioSettings>, AudioSettingsValidator>();
    services.AddSingleton<IValidator<UISettings>, UISettingsValidator>();
    services.AddSingleton<IValidator<LoggingSettings>, LoggingSettingsValidator>();
    services.AddSingleton<IValidator<GameplaySettings>, GameplaySettingsValidator>();

    // Validate settings on startup
    var tempProvider = services.BuildServiceProvider();
    ValidateSettings<GameSettings>(tempProvider);
    ValidateSettings<AudioSettings>(tempProvider);
    ValidateSettings<UISettings>(tempProvider);
    ValidateSettings<LoggingSettings>(tempProvider);
    ValidateSettings<GameplaySettings>(tempProvider);

    // Register MediatR for event-driven architecture (v12.4.1 - no license required)
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add pipeline behaviors (order matters!)
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    });

    // Register core game services
    services.AddSingleton<SaveGameService>();
    services.AddSingleton<GameStateService>();
    services.AddSingleton<CombatService>();
    services.AddSingleton<LevelUpService>();
    
    // Register UI services
    services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
    services.AddSingleton<IConsoleUI, ConsoleUI>();
    
    // Apocalypse timer (shared service)
    services.AddSingleton<ApocalypseTimer>();
    
    // Death system services (Phase 2)
    services.AddSingleton<DeathService>();
    services.AddSingleton<HallOfFameRepository>();
    
    // Register repositories
    services.AddSingleton<ISaveGameRepository>(sp => new SaveGameRepository("savegames.db"));
    services.AddSingleton<IHallOfFameRepository>(sp => new HallOfFameRepository("halloffame.db"));
    services.AddTransient<ICharacterClassRepository, CharacterClassRepository>();
    services.AddTransient<IEquipmentSetRepository, EquipmentSetRepository>();
    
    // Quest system services (Phase 4) - COMMENTED OUT - moved to Game.Core
    // services.AddScoped<Game.Core.Features.Quest.Services.QuestService>();
    // services.AddScoped<Game.Core.Features.Quest.Services.MainQuestService>();
    // services.AddScoped<Game.Core.Features.Quest.Services.QuestProgressService>();
    
    // Achievement system services (Phase 4) - COMMENTED OUT - moved to Game.Core
    // services.AddScoped<Game.Core.Features.Achievement.Services.AchievementService>();
    
    // Victory system services (Phase 4) - COMMENTED OUT - moved to Game.Core
    // services.AddScoped<Game.Core.Features.Victory.Services.VictoryService>();
    // services.AddScoped<Game.Core.Features.Victory.Services.NewGamePlusService>();
    // services.AddScoped<Game.Core.Features.Victory.Orchestrators.VictoryOrchestrator>();
    
    // Register UI and interaction services
    services.AddTransient<MenuService>();
    services.AddTransient<CharacterViewService>();
    services.AddTransient<ExplorationService>();
    
    // Register orchestrator services
    services.AddTransient<CharacterCreationOrchestrator>();
    services.AddTransient<LoadGameService>();
    services.AddTransient<GameplayService>();
    services.AddTransient<CombatOrchestrator>();
    services.AddTransient<InventoryOrchestrator>();
    
    // Register GameEngine service aggregator (reduces constructor complexity)
    services.AddScoped<GameEngineServices>();
    
    // Register the game engine
    services.AddSingleton<GameEngine>();

    var serviceProvider = services.BuildServiceProvider();

    // Get the game engine from DI and run the game loop
    var gameEngine = serviceProvider.GetRequiredService<GameEngine>();
    await gameEngine.RunAsync();

    Log.Information("Console Game shutting down gracefully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    System.Console.WriteLine($"\nFatal error: {ex.Message}");
    System.Console.WriteLine("Check the log file for details.");
    System.Console.WriteLine("\nPress any key to exit...");
    System.Console.ReadKey();
}
finally
{
    LoggingService.Shutdown();
}

// Helper method to validate settings on startup
static void ValidateSettings<T>(ServiceProvider provider) where T : class
{
    var settings = provider.GetRequiredService<IOptions<T>>().Value;
    var validator = provider.GetRequiredService<IValidator<T>>();
    
    var validationResult = validator.Validate(settings);
    
    if (!validationResult.IsValid)
    {
        var errors = string.Join(Environment.NewLine, validationResult.Errors.Select(e => $"  - {e.PropertyName}: {e.ErrorMessage}"));
        throw new InvalidOperationException($"Configuration validation failed for {typeof(T).Name}:{Environment.NewLine}{errors}");
    }
    
    Log.Debug("{SettingsType} validation passed", typeof(T).Name);
}
