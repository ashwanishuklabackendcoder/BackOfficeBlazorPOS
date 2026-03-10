using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Implementations;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Implementations;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Admin.Services.Security;
using BackOfficeBlazor.Shared.DTOs;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using POSAPI.Config;
using POSAPI.Hubs;
using POSAPI.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));
builder.Services.Configure<UploadSettings>(
    builder.Configuration.GetSection("UploadSettings"));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));

builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;

    var account = new CloudinaryDotNet.Account(
        cfg.CloudName,
        cfg.ApiKey,
        cfg.ApiSecret);

    return new Cloudinary(account);
});
builder.Services.AddDataProtection();
builder.Services.AddSignalR();

//builder.Services.AddScoped<IImageService, CloudinaryImageService>();

// DB Connection
builder.Services.AddDbContext<BackOfficeAdminContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// Repository Layer DI

builder.Services.AddScoped<IProductGroupRepository, ProductGroupRepository>();
builder.Services.AddScoped<IProductGroupItemRepository, ProductGroupItemRepository>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IlocationRepositry, locationRepositry>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ISysOptionRepository, SysOptionRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductStockRepository, ProductStockRepository>();
builder.Services.AddScoped<IStockLevelRepository, StockLevelRepository>();

builder.Services.AddScoped<IStockInputService, StockInputService>();
builder.Services.AddScoped<ISalesRepository, SalesRepositroy>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IStaffUserRepository, StaffUserRepository>();
builder.Services.AddScoped<IStaffUserPermissionRepository, StaffUserPermissionRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStaffUserService, StaffUserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductLevelRepository, ProductLevelRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IReturnsRepository, ReturnsRepository>();
builder.Services.AddScoped<IReturnsService, ReturnsService>();
builder.Services.AddScoped<IReportsRepository, ReportsRepository>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<ILayawayRepository, LayawayRepository>();
builder.Services.AddScoped<ILayawayService, LayawayService>();
builder.Services.AddScoped<IPrinterConfigRepository, PrinterConfigRepository>();
builder.Services.AddScoped<ITerminalOptionRepository, TerminalOptionRepository>();
builder.Services.AddScoped<IPrintJobRepository, PrintJobRepository>();
builder.Services.AddScoped<IPrintJobService, PrintJobService>();
builder.Services.AddScoped<IPrinterConfigService, PrinterConfigService>();
builder.Services.AddScoped<ITerminalPrinterService, TerminalPrinterService>();
builder.Services.AddScoped<ISysOptionService, SysOptionService>();
builder.Services.AddScoped<ISecretEncryptionService, DataProtectionSecretEncryptionService>();
builder.Services.AddScoped<IEscPosBuilder, EscPosBuilder>();
builder.Services.AddScoped<IZplBuilder, ZplBuilder>();
builder.Services.AddSingleton<PrintAgentPresenceService>();
builder.Services.AddSingleton<IPrintAgentPresenceService>(sp => sp.GetRequiredService<PrintAgentPresenceService>());
builder.Services.AddScoped<IPrintJobDispatcher, SignalRPrintJobDispatcher>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IQuickShortcutService, QuickShortcutService>();



// Service Layer DI
builder.Services.AddScoped<ICategoryService, CategoryService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwt.SecretKey) || jwt.SecretKey.Length < 32)
{
    throw new Exception("Jwt:SecretKey must be configured with at least 32 characters.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// CORS 
builder.Services.Configure<UploadSettings>(options =>
{
    options.UiUploadRoot = Path.Combine(
        builder.Environment.ContentRootPath,
        "uploads");
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("UiPolicy", policy =>
    {
        policy.WithOrigins(
                "https://pos-ui.runasp.net",
                "https://localhost:7209",
                "http://localhost:7209"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BackOfficeAdminContext>();
    await EnsureStaffAuthSchemaAsync(db);
    await EnsurePrinterSchemaAsync(db);
    await EnsureQuickShortcutSchemaAsync(db);
    var bootstrapUsername = builder.Configuration["AuthBootstrap:Username"] ?? "admin";
    var bootstrapPassword = builder.Configuration["AuthBootstrap:Password"] ?? "Admin@123";
    var bootstrapName = builder.Configuration["AuthBootstrap:FullName"] ?? "System Administrator";
    var bootstrapCode = builder.Configuration["AuthBootstrap:StaffCode"] ?? "ADMIN";

    if (!await db.StaffUsers.AnyAsync())
    {
        var (hash, salt) = PasswordHasher.HashPassword(bootstrapPassword);

        var admin = new StaffUser
        {
            Username = bootstrapUsername,
            FullName = bootstrapName,
            StaffCode = bootstrapCode,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsAdmin = true,
            IsActive = true,
            DateCreated = DateTime.UtcNow,
            Permission = new StaffUserPermission
            {
                Till = true,
                Workshop = true,
                ProductAdd = true,
                StockInput = true,
                Layaway = true,
                CustomerSalesReturn = true,
                Category = true,
                Brand = true,
                Supplier = true,
                Customer = true
            }
        };

        db.StaffUsers.Add(admin);
        await db.SaveChangesAsync();

        var allKeys = PermissionCatalog.AllKeys.Select(k => new StaffUserPermissionEntry
        {
            StaffUserId = admin.Id,
            PermissionKey = k
        });
        await db.StaffUserPermissionEntries.AddRangeAsync(allKeys);
        await db.SaveChangesAsync();
    }
}

app.UseCors("UiPolicy");
app.UseAuthentication();
app.UseAuthorization();



// ⭐ FIXED SWAGGER CONFIG ⭐
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BackOfficeBlazor API v1");
});
app.UseStaticFiles();   // keep this for wwwroot

var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot/uploads");

// Ensure folder exists
Directory.CreateDirectory(uploadPath);

// Serve files from /uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/wwwroot/uploads"
});


app.MapControllers();
app.MapHub<PrintHub>("/hubs/print");
app.Run();

static async Task EnsureStaffAuthSchemaAsync(BackOfficeAdminContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'dbo.StaffUsers', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[StaffUsers](
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Username] NVARCHAR(50) NOT NULL,
                [FullName] NVARCHAR(100) NOT NULL,
                [StaffCode] NVARCHAR(10) NOT NULL,
                [PasswordHash] VARBINARY(MAX) NOT NULL,
                [PasswordSalt] VARBINARY(MAX) NOT NULL,
                [IsAdmin] BIT NOT NULL,
                [IsActive] BIT NOT NULL,
                [DateCreated] DATETIME2 NOT NULL,
                [DateUpdated] DATETIME2 NULL
            );

            CREATE UNIQUE INDEX [IX_StaffUsers_Username]
                ON [dbo].[StaffUsers]([Username]);
        END
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'dbo.StaffUserPermissions', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[StaffUserPermissions](
                [StaffUserId] INT NOT NULL PRIMARY KEY,
                [Till] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_Till] DEFAULT(0),
                [Workshop] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_Workshop] DEFAULT(0),
                [ProductAdd] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_ProductAdd] DEFAULT(0),
                [StockInput] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_StockInput] DEFAULT(0),
                [Layaway] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_Layaway] DEFAULT(0),
                [CustomerSalesReturn] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_CustomerSalesReturn] DEFAULT(0),
                [Category] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_Category] DEFAULT(0),
                [Brand] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_Brand] DEFAULT(0),
                [Supplier] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_Supplier] DEFAULT(0),
                [Customer] BIT NOT NULL CONSTRAINT [DF_StaffUserPermissions_Customer] DEFAULT(0),
                CONSTRAINT [FK_StaffUserPermissions_StaffUsers_StaffUserId]
                    FOREIGN KEY([StaffUserId]) REFERENCES [dbo].[StaffUsers]([Id])
                    ON DELETE CASCADE
            );
        END
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'dbo.StaffUserPermissionEntries', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[StaffUserPermissionEntries](
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [StaffUserId] INT NOT NULL,
                [PermissionKey] NVARCHAR(120) NOT NULL,
                CONSTRAINT [FK_StaffUserPermissionEntries_StaffUsers_StaffUserId]
                    FOREIGN KEY([StaffUserId]) REFERENCES [dbo].[StaffUsers]([Id])
                    ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX [IX_StaffUserPermissionEntries_StaffUserId_PermissionKey]
                ON [dbo].[StaffUserPermissionEntries]([StaffUserId], [PermissionKey]);
        END
        """);
}

static async Task EnsurePrinterSchemaAsync(BackOfficeAdminContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'dbo.PrintJobs', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[PrintJobs](
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [TerminalCode] NVARCHAR(5) NULL,
                [PrinterConfigId] INT NOT NULL,
                [JobType] NVARCHAR(20) NULL,
                [Payload] NVARCHAR(MAX) NULL,
                [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_PrintJobs_Status] DEFAULT('Pending'),
                [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PrintJobs_CreatedAt] DEFAULT(SYSUTCDATETIME()),
                [ProcessedAt] DATETIME2 NULL
            );
        END
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'dbo.TerminalOptions', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[TerminalOptions](
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [TerminalCode] NVARCHAR(5) NOT NULL,
                [LocationCode] NVARCHAR(2) NOT NULL,
                [ReceiptPrinterId] INT NOT NULL,
                [A4PrinterId] INT NULL
            );
            CREATE UNIQUE INDEX [IX_TerminalOptions_TerminalCode] ON [dbo].[TerminalOptions]([TerminalCode]);
        END
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        IF COL_LENGTH('dbo.TerminalOptions', 'LabelPrinterId') IS NULL
        BEGIN
            ALTER TABLE [dbo].[TerminalOptions] ADD [LabelPrinterId] INT NULL;
        END
        """);
}

static async Task EnsureQuickShortcutSchemaAsync(BackOfficeAdminContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'dbo.QuickShortcutItems', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[QuickShortcutItems](
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [PartNumber] NVARCHAR(10) NOT NULL,
                [Title] NVARCHAR(120) NOT NULL,
                [SubTitle] NVARCHAR(120) NULL,
                [ColorHex] NVARCHAR(16) NOT NULL CONSTRAINT [DF_QuickShortcutItems_ColorHex] DEFAULT('#f8f9fa'),
                [ForeColorHex] NVARCHAR(16) NOT NULL CONSTRAINT [DF_QuickShortcutItems_ForeColorHex] DEFAULT('#212529'),
                [ShowAvailableStock] BIT NOT NULL CONSTRAINT [DF_QuickShortcutItems_ShowAvailableStock] DEFAULT(1),
                [IsActive] BIT NOT NULL CONSTRAINT [DF_QuickShortcutItems_IsActive] DEFAULT(1),
                [DisplayOrder] INT NOT NULL CONSTRAINT [DF_QuickShortcutItems_DisplayOrder] DEFAULT(0)
            );

            CREATE UNIQUE INDEX [IX_QuickShortcutItems_PartNumber]
                ON [dbo].[QuickShortcutItems]([PartNumber]);
        END
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        IF COL_LENGTH('dbo.QuickShortcutItems', 'ForeColorHex') IS NULL
        BEGIN
            ALTER TABLE [dbo].[QuickShortcutItems]
                ADD [ForeColorHex] NVARCHAR(16) NOT NULL
                    CONSTRAINT [DF_QuickShortcutItems_ForeColorHex] DEFAULT('#212529');
        END
        """);
}
