using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using MurderBot.Data.Context;
using MurderBot.Infrastructure.Routines.Web;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.Utility;
using MurderBot.Infrastructure.WassengerClient;

var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
        
builder.Logging.AddAzureWebAppDiagnostics();
        
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddEnvironmentVariables();


builder.Services.AddOptions<CommonMurderSettings>()
    .Bind(builder.Configuration.GetSection("CommonMurderSettings"));
builder.Services.AddHttpClient();
builder.Services.AddTransient<WassengerClient>();
builder.Services.AddScoped<MurderUtil>();
builder.Services.AddScoped<AutoReplyRoutine>();
builder.Services.AddScoped<ProcessIncomingMessageRoutine>();


var connectionString = builder.Configuration.GetSection("CommonMurderSettings")["MurderContextConnectionString"];
        
builder.Services.AddDbContextFactory<MurderContext>(ob =>
{
    ob.UseSqlServer(connectionString);
    ob.ConfigureWarnings(warnings =>
    {
        // Ignores the log event for command execution
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting);
    });
});


builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();



var provider = new FileExtensionContentTypeProvider();

provider.Mappings[".vcf"] = "text/vcard";


app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();



app.Run();
