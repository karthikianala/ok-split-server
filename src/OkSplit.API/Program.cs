using OkSplit.API.Middleware;
using OkSplit.Application;
using OkSplit.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// CORS
var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins") ?? "http://localhost:3000";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins.Split(','))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Application (Services, AutoMapper, FluentValidation)
builder.Services.AddApplication();

// Infrastructure (DbContext, Identity, JWT)
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck");

app.Run();
