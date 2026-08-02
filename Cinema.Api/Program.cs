using System.Reflection;
using System.Text;
using Cinema.Api.Data;
using Cinema.Api.Middleware;
using Cinema.Api.Repositories;
using Cinema.Api.Services;
using Cinema.Api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Porta dinamica (Render/Docker) - escuta na porta definida pelo ambiente
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

// User Secrets (dev only)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Database (PostgreSQL + EF Core)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI: Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// DI: Repositories
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// DI: Services (SOLID)
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IRoomService, RoomService>();

// Health Checks
builder.Services.AddHealthChecks();

// Authentication (JWT)
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "Jwt:Key nao configurada. Defina em appsettings.json, User Secrets ou variavel de ambiente Jwt__Key.");
    }
    throw new InvalidOperationException(
        "Jwt:Key nao configurada. Defina via variavel de ambiente Jwt__Key ou User Secrets.");
}

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    if (builder.Environment.IsDevelopment())
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection nao configurada. Defina em appsettings.json, User Secrets ou variavel de ambiente.");
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection nao configurada. Defina via variavel de ambiente ConnectionStrings__DefaultConnection.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// FluentValidation + ProblemDetails
builder.Services.AddValidatorsFromAssemblyContaining<CreateSessionValidator>();
builder.Services.AddFluentValidationAutoValidation(cfg =>
{
    // Desabilita o response automatico do FluentValidation
    // para usarmos ProblemDetails padronizado
    cfg.DisableDataAnnotationsValidation = false;
});

// Unifica erros de validacao no formato ProblemDetails (RFC 7807)
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "Erro de validacao",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Um ou mais campos sao invalidos.",
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["errors"] = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray()
            );

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cinema API",
        Version = "v1",
        Description = "API de gestao de cinema com autenticacao JWT. " +
                      "Use POST /api/auth/login para obter um token."
    });

    // JWT Bearer auth definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Cole apenas o token JWT (sem 'Bearer '). O Swagger adiciona o prefixo automaticamente.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// CORS (origens configuraveis via env var Cors__Origins)
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                  ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();  // HTTPS so em dev (Render gerencia TLS)
}

// Middleware de erro GLOBAL - captura DomainException e excecoes nao tratadas
app.UseMiddleware<GlobalExceptionHandler>();

app.UseCors("AllowAngular");

app.UseAuthentication();  // JWT auth middleware
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");  // Health check endpoint

// Auto-apply migrations + seed users on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();         // PostgreSQL (producao)
    else
        await db.Database.EnsureCreatedAsync();   // InMemory (testes)

    await DataSeeder.SeedUsersAsync(userRepo);
}

app.Run();

// Expor Program para testes de integracao (WebApplicationFactory<T>)
public partial class Program { }