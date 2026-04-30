using FishShop.Gateway.Middleware;
using FishShop.Gateway.Services;
using Serilog;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ── Serilog ──────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console()
       .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day)
       .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://seq:5341"));

// ── YARP ─────────────────────────────────────────
builder.Services.AddReverseProxy()
    .LoadFromConfig(configuration.GetSection("ReverseProxy"));

// ── Services ─────────────────────────────────────
builder.Services.AddSingleton<TokenValidator>();

// ── CORS ─────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddPolicy("AllowOrigin", p =>
        p.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()));

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────
app.UseSerilogRequestLogging();
app.UseCors("AllowOrigin");

// Gateway middleware: checks JWT + injects X-User-Id + X-Api-Key
app.UseMiddleware<PermissionValidationMiddleware>();
app.UseMiddleware<SetApiKeyMiddleware>();

app.MapReverseProxy();

app.Run();
