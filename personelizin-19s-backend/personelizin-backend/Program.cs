using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using personelizin_backend.Data;
using personelizin_backend.Services;
using personelizin_backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. CORS Politikas?n? Tan?mla
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", b => b
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());
});

builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHttpClient<personelizin_backend.Services.PrimeApiService>();
// ?simlendirme karma?as?n? bitiren ayar
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Bu ayar, Property isimlerini (UserId, Token vb.) 
        // oldu?u gibi (C#'taki haliyle) Frontend'e g?nderir.
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "GizliKey1234567890123456"))
        };
    });

var app = builder.Build();

// 2. Veritaban? ?emas?: EnsureCreated + Permissions tablosunda Type s?tunu yoksa ekle
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Permissions"" ADD COLUMN IF NOT EXISTS ""Type"" text;");
    }
    catch { }

    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Permissions"" ADD COLUMN IF NOT EXISTS ""UnitId"" integer;");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""UnitId"" integer;");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""RemainingLeaveDays"" integer NOT NULL DEFAULT 14;");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC');");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""MustChangePassword"" boolean NOT NULL DEFAULT false;");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Permissions"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" timestamp with time zone;");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Permissions"" ADD COLUMN IF NOT EXISTS ""SignedDocumentOperationId"" text;");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Permissions"" ADD COLUMN IF NOT EXISTS ""DocumentPath"" text;");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Permissions"" ADD COLUMN IF NOT EXISTS ""DocumentOriginalName"" text;");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""Units"" (
                ""Id"" serial PRIMARY KEY,
                ""Name"" text NOT NULL,
                ""Code"" text NOT NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL,
                ""CreatedByUserId"" integer NOT NULL
            );
        ");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""UserUnits"" (
                ""UserId"" integer NOT NULL,
                ""UnitId"" integer NOT NULL,
                PRIMARY KEY (""UserId"", ""UnitId""),
                CONSTRAINT ""FK_UserUnits_Users"" FOREIGN KEY (""UserId"") REFERENCES ""Users""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_UserUnits_Units"" FOREIGN KEY (""UnitId"") REFERENCES ""Units""(""Id"") ON DELETE CASCADE
            );
        ");
    }
    catch { }
    try
    {
        context.Database.ExecuteSqlRaw(@"
            INSERT INTO ""UserUnits"" (""UserId"", ""UnitId"")
            SELECT ""Id"", ""UnitId"" FROM ""Users"" WHERE ""UnitId"" IS NOT NULL
            ON CONFLICT (""UserId"", ""UnitId"") DO NOTHING;
        ");
    }
    catch { }

    if (!context.Users.Any())
    {
        context.Users.Add(new User
        {
            FullName = "Sistem Admin",
            Email = "admin", // Senin istedi?in kullan?c? ad?
            PasswordHash = "1234", // Senin istedi?in ?ifre
            Role = "Manager",
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }
    const string adminEmail = "admin99@gmail.com";
    const string adminPassword = "9876";
    if (!context.Users.Any(u => u.Email == adminEmail))
    {
        context.Users.Add(new User
        {
            FullName = "Sistem Y�neticisi",
            Email = adminEmail,
            PasswordHash = adminPassword,
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }
}

// 3. Middleware S?ralamas? (CORS her zaman ?stte)
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();