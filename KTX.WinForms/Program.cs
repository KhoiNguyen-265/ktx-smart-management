using KTX.Data;
using KTX.Data.Seed;
using KTX.Services.Auth;
using KTX.WinForms.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KTX.WinForms;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        // 1. Cấu hình High DPI chuẩn cho Windows 10/11
        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // 2. Global Exception Handling (Bắt mọi lỗi chưa được xử lý để tránh crash ứng dụng)
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (sender, e) =>
        {
            MessageBox.Show($"Đã xảy ra lỗi hệ thống:\n{e.Exception.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Lỗi nghiêm trọng:\n{ex.Message}", "Lỗi chí mạng", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        };

        // 3. Xây dựng Configuration từ appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 4. Thiết lập Dependency Injection Container
        var services = new ServiceCollection();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=KTX_SmartDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        // BẮT BUỘC: Sử dụng IDbContextFactory cho WinForms
        services.AddDbContextFactory<KtxDbContext>(options =>
        {
            options.UseSqlServer(connectionString)
                   .AddInterceptors(new KTX.Data.Interceptors.AuditSaveChangesInterceptor());
        });

        // Đăng ký Services
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IFormFactory, FormFactory>();
        services.AddScoped<IAuthService, AuthService>();

        // Đăng ký Forms
        services.AddTransient<Forms.LoginForm>();
        services.AddTransient<Forms.MainForm>();

        ServiceProvider = services.BuildServiceProvider();

        // 5. Tự động áp dụng Migration và nạp dữ liệu mẫu (Seeder) khi khởi động
        try
        {
            var contextFactory = ServiceProvider.GetRequiredService<IDbContextFactory<KtxDbContext>>();
            using var db = contextFactory.CreateDbContext();
            db.Database.Migrate();
            DatabaseSeeder.SeedAsync(db).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể khởi tạo cơ sở dữ liệu:\n{ex.Message}", "Cảnh báo CSDL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // 6. Khởi chạy Form Đăng nhập đầu tiên
        var loginForm = ServiceProvider.GetRequiredService<Forms.LoginForm>();
        Application.Run(loginForm);
    }
}
