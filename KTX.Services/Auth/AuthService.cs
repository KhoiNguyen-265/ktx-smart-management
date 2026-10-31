using KTX.Data;
using KTX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public AuthService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResult
            {
                Success = false,
                Message = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu."
            };
        }

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var user = await db.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(u => u.TenDangNhap == request.Username.Trim(), ct);

        if (user == null)
        {
            return new LoginResult
            {
                Success = false,
                Message = "Tên đăng nhập hoặc mật khẩu không chính xác."
            };
        }

        // Check if locked
        if (user.KhoaDen.HasValue && user.KhoaDen.Value > DateTime.Now)
        {
            var remainingMins = (int)Math.Ceiling((user.KhoaDen.Value - DateTime.Now).TotalMinutes);
            return new LoginResult
            {
                Success = false,
                Message = $"Tài khoản đang bị tạm khóa do nhập sai quá nhiều lần. Vui lòng thử lại sau {remainingMins} phút."
            };
        }

        if (!user.HoatDong)
        {
            return new LoginResult
            {
                Success = false,
                Message = "Tài khoản này đã bị vô hiệu hóa bởi Quản trị viên."
            };
        }

        // Verify password
        bool isPasswordValid = false;
        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.MatKhauHash);
        }
        catch
        {
            isPasswordValid = false;
        }

        if (!isPasswordValid)
        {
            user.SoLanSai += 1;
            if (user.SoLanSai >= 5)
            {
                user.KhoaDen = DateTime.Now.AddMinutes(15);
                user.SoLanSai = 0;
                await db.SaveChangesAsync(ct);

                return new LoginResult
                {
                    Success = false,
                    Message = "Bạn đã nhập sai mật khẩu 5 lần. Tài khoản bị tạm khóa 15 phút."
                };
            }

            await db.SaveChangesAsync(ct);
            return new LoginResult
            {
                Success = false,
                Message = $"Mật khẩu không đúng. Bạn còn {5 - user.SoLanSai} lần thử."
            };
        }

        // Reset failed count upon successful login
        user.SoLanSai = 0;
        user.KhoaDen = null;

        // Log audit
        db.NhatKys.Add(new NhatKy
        {
            NguoiDungId = user.Id,
            HanhDong = "Đăng nhập",
            ChiTiet = $"Người dùng {user.TenDangNhap} đăng nhập thành công vào hệ thống.",
            BangLienQuan = "NguoiDung",
            KhoaChinhLienQuan = user.Id.ToString(),
            ThoiGian = DateTime.Now
        });

        await db.SaveChangesAsync(ct);

        return new LoginResult
        {
            Success = true,
            Message = "Đăng nhập thành công.",
            Session = new UserSession
            {
                UserId = user.Id,
                Username = user.TenDangNhap,
                FullName = user.HoTen,
                RoleId = user.MaVaiTro,
                RoleName = user.VaiTro.TenVaiTro
            }
        };
    }
}
