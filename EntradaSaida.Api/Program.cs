using EntradaSaida.Core.Interfaces;
using EntradaSaida.Core.Services;
using EntradaSaida.ML.Processing;
using EntradaSaida.Api.Hubs;
using Microsoft.EntityFrameworkCore;
using EntradaSaida.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=entradasaida.db"));

// Registrar serviços
builder.Services.AddSingleton<ICounterService,CounterService>();//builder.Services.AddScoped<ICounterService, CounterService>();
builder.Services.AddSingleton<IConfigService,ConfigService>();//builder.Services.AddScoped<IConfigService, ConfigService>();
builder.Services.AddSingleton<VideoProcessor>();

// Controllers
builder.Services.AddControllers();

// SignalR para real-time updates
builder.Services.AddSignalR();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "EntradaSaida API", 
        Version = "v1",
        Description = "API para sistema de contagem de pessoas com visão computacional"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>(); //https://khalidabuhakmeh.com/health-checks-for-aspnet-core-and-entity-framework-core

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Servir arquivos estáticos (dashboard)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<CounterHub>("/counterhub");
app.MapHealthChecks("/health");

// Página principal
app.MapGet("/", () => Results.Redirect("/swagger/"));

// Criar banco de dados se não existir
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
