using Churn.Core;
using Churn.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;

var builder = WebApplication.CreateBuilder(args);

// Resolve model paths relative to the API project folder, so it works no matter where you start it from
string ResolvePath(string key) =>
    Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration[key]!));

var modelPath = ResolvePath("Model:Path");
var metadataPath = ResolvePath("Model:MetadataPath");

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

app.Run();