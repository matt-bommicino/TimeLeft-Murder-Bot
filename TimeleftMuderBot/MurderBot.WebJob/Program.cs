﻿// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurderBot.Infrastructure.Routines;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.WassengerClient;
using MurderBot.Data.Context;
using MurderBot.Infrastructure.Utility;
using MurderBot.WebJob;

var host = Host.CreateDefaultBuilder(args)
    // Configure the app's configuration
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddUserSecrets<Program>();
        config.AddEnvironmentVariables();
    })
    // Register services with the DI container
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<CommonMurderSettings>()
            .Bind(context.Configuration.GetSection("CommonMurderSettings"));
        services.AddHttpClient();
        services.AddTransient<WassengerClient>();
        services.AddScoped<MurderUtil>();
        services.AddScoped<UpdateGroupsRoutine>();
        services.AddScoped<AlwaysBanRoutine>();
        services.AddScoped<GroupMurderRoutine>();
        services.AddTransient<MurderBotService>();
        
        
        var connectionString = context.Configuration.GetSection("CommonMurderSettings")["MurderContextConnectionString"];
        
        services.AddDbContextFactory<MurderContext>(ob =>
        {
            ob.UseSqlServer(connectionString);
        });

    })
    // Optional: Configure Logging level/format
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        
        logging.AddAzureWebAppDiagnostics();
        
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    })
    .Build();
    
    var service = host.Services.GetService<MurderBotService>()!;
    await service.RunAsync();