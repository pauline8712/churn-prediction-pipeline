using Churn.Core;
using Churn.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using Churn.Api;

var builder = WebApplication.CreateBuilder(args);

// Resolve model paths relative to the API project folder, so it works no matter where you start it from
string ResolvePath(string key) =>
    Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration[key]!));

var modelPath = ResolvePath("Model:Path");
var metadataPath = ResolvePath("Model:MetadataPath");

// DateTimeOffset keeps the "Z" (UTC) from appsettings; DateTime would convert it to local time
var referenceDateUtc = builder.Configuration.GetValue<DateTimeOffset>("Model:ReferenceDateUtc").UtcDateTime;

builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<ChurnDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Churn")));

// ML model: reloads automatically when model_current.zip changes
builder.Services.AddPredictionEnginePool<UserFeatures, ChurnPrediction>()
    .FromFile(
        modelName: "churn",
        filePath: modelPath,
        watchForChanges: true);

// Our own services
builder.Services.AddSingleton(new ModelSettings(modelPath, metadataPath, referenceDateUtc));
builder.Services.AddScoped<ChurnScoringService>();
builder.Services.AddScoped<MonitoringService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok("OK"));

// Smoke test: can we reach the database?
app.MapGet("/users/count", async (ChurnDbContext db) =>
    Results.Ok(await db.Users.CountAsync()));

// Smoke test: which model is loaded?
app.MapGet("/model", () =>
    Results.Text(File.ReadAllText(metadataPath), "application/json"));

// Churn risk for a single user
app.MapGet("/users/{id:int}/churn-risk", async (int id, ChurnScoringService scoring) =>
    await scoring.ScoreUserAsync(id) is { } risk
        ? Results.Ok(risk)
        : Results.NotFound(new { message = $"User {id} not found" }));

// The users most likely to churn right now
app.MapGet("/users/at-risk", async (ChurnScoringService scoring, int top = 20) =>
    Results.Ok(await scoring.GetAtRiskAsync(top)));

// Model health: prediction stats and feature drift compared with training
app.MapGet("/metrics", async (MonitoringService monitoring, int hours = 24) =>
    Results.Ok(await monitoring.GetMetricsAsync(hours)));

app.Run();