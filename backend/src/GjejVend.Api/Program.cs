using System.Text.Json;
using System.Text.Json.Serialization;
using GjejVend.Api.Authentication;
using GjejVend.Api.ErrorHandling;
using GjejVend.Api.OpenApi;
using GjejVend.Api.Workers;
using GjejVend.Application;

DotNetEnv.Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is missing. Set ConnectionStrings__Default in backend/.env (see backend/.env.example).");

var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException(
        "Supabase URL is missing. Set Supabase__Url in backend/.env (see backend/.env.example).");

builder.Services.AddApplication(connectionString);
builder.Services.AddSupabaseAuthentication(supabaseUrl);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.SchemaFilter<RequiredPropertiesSchemaFilter>();
});
builder.Services.AddHealthChecks();

if (builder.Configuration.GetValue("Bookings:CloseOverdue", true))
{
    builder.Services.AddHostedService<BookingLifecycleWorker>();
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

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
