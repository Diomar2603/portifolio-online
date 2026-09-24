using System.Text.Json.Serialization;
using FluentValidation;
using Portfolio.Api.Auth;
using Portfolio.Api.Endpoints;
using Portfolio.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCloudflareAccess(builder.Configuration, builder.Environment);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
    o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapGroup("/api/admin")
    .RequireAuthorization(CloudflareAccessAuthentication.AdminPolicy)
    .MapProfile()
    .MapTimeline()
    .MapCategories()
    .MapProjects()
    .MapMedia()
    .MapPublishing();

app.Run();

public partial class Program;
