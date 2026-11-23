using GjejVend.Api.Authentication;
using GjejVend.Api.Middleware;
using GjejVend.Application;

DotNetEnv.Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is missing. Set ConnectionStrings__Default in backend/.env (see backend/.env.example).");

var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException(
        "Supabase URL is missing. Set Supabase__Url in backend/.env (see backend/.env.example).");

var webhookSecret = builder.Configuration["Payments:WebhookSecret"]
    ?? (builder.Environment.IsDevelopment()
        ? "dev-webhook-secret-change-me"
        : throw new InvalidOperationException(
            "Payments webhook secret is missing. Set Payments__WebhookSecret (see backend/.env.example)."));

builder.Services.AddApplication(connectionString, webhookSecret);
builder.Services.AddSupabaseAuthentication(supabaseUrl);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
