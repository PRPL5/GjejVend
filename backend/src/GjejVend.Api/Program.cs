using GjejVend.Api.Authentication;
using GjejVend.Api.Middleware;
using GjejVend.Application;
using GjejVend.Infrastructure.Payments;

DotNetEnv.Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is missing. Set ConnectionStrings__Default in backend/.env (see backend/.env.example).");

var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException(
        "Supabase URL is missing. Set Supabase__Url in backend/.env (see backend/.env.example).");

var stripeKey = builder.Configuration["Stripe:SecretKey"];
var stripeWebhookSecret = builder.Configuration["Stripe:WebhookSecret"];
if (!builder.Environment.IsDevelopment() && (string.IsNullOrWhiteSpace(stripeKey) || string.IsNullOrWhiteSpace(stripeWebhookSecret)))
{
    throw new InvalidOperationException(
        "Stripe is not configured. Set Stripe__SecretKey and Stripe__WebhookSecret (see backend/.env.example).");
}

// Without a Stripe key (development only) the signed fake gateway is used.
var payments = new PaymentsSettings(
    stripeKey,
    stripeWebhookSecret,
    builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173",
    builder.Configuration["Payments:FakeWebhookSecret"] ?? "dev-webhook-secret-change-me");

builder.Services.AddApplication(connectionString, payments);
builder.Services.AddSupabaseAuthentication(supabaseUrl);
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
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
