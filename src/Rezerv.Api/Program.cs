using Hangfire;
using Hangfire.MySql;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Rezerv.Api.Contracts.Common;
using Rezerv.Api.Services;
using Rezerv.Application;
using Rezerv.Infrastructure;
using Rezerv.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var databaseConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(state => state.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The request is invalid." : error.ErrorMessage)
            .ToArray();

        return new BadRequestObjectResult(ApiResponse<object>.Failed(ApiResponseMessages.ValidationFailed, errors));
    };
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(databaseConnection, new MySqlStorageOptions())));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<StartedWaitlistCleanupJob>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RezervDbContext>("mysql", tags: ["ready"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/hangfire");
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Failed(ApiResponseMessages.UnexpectedError, ApiResponseMessages.UnexpectedError));
    });
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
RecurringJob.AddOrUpdate<StartedWaitlistCleanupJob>(
    "delete-started-waitlists",
    job => job.ExecuteAsync(),
    Cron.Minutely);
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

public partial class Program;
