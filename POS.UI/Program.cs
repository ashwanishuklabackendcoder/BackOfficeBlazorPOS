using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using POS.UI;
using POS.UI.Services;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMudServices();
builder.Services.AddSingleton<AppSnackbarService>();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5101/";

//builder.Services.AddScoped(sp =>
//    new HttpClient
//    {
//        BaseAddress = new Uri("https://pos-api.runasp.net/")
//    });
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl)
    });


// ===== REGISTER UI SERVICES =====
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IImageApiService, ImageApiService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStockTransferUiService, StockTransferUiService>();
builder.Services.AddScoped<IBranchLocationService, BranchLocationService>();
builder.Services.AddScoped<IProductEnquiryService, ProductEnquiryService>();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductApiService, ProductApiService>();
builder.Services.AddScoped<IComboApiService, ComboApiService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReturnService, ReturnService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ILayawayService, LayawayService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStaffUserService, StaffUserService>();
builder.Services.AddScoped<IPrinterApiService, PrinterApiService>();
builder.Services.AddScoped<ISysOptionsService, SysOptionsService>();
builder.Services.AddScoped<IQuickShortcutService, QuickShortcutService>();
builder.Services.AddScoped<GridStateStorageService>();

// 🔥 THIS WAS MISSING
builder.Services.AddScoped<ICategoryService, CategoryService>();

var host = builder.Build();
await host.Services.GetRequiredService<IAuthService>().InitializeAsync();
await host.RunAsync();

