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

        var normalizedUsername = request.Username.Trim();

        var user = await db.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(u => u.TenDangNhap == normalizedUsername, ct);

        if (user == null)
        {
            // Hỗ trợ sinh viên đăng nhập trực tiếp bằng MSSV (mật khẩu mặc định 123456 hoặc 6 số cuối CCCD)
            var student = await db.SinhViens.AsNoTracking()
                .FirstOrDefaultAsync(s => s.MSSV.ToUpper() == normalizedUsername.ToUpper(), ct);

            if (student != null)
            {
                string cccdSuffix = student.CCCD.Length >= 6 ? student.CCCD[^6..] : student.CCCD;
                if (request.Password == "123456" || request.Password == cccdSuffix)
                {
                    var svRole = await db.VaiTros.AsNoTracking()
                        .FirstOrDefaultAsync(v => v.TenVaiTro == "SinhVien", ct);

                    return new LoginResult
                    {
                        Success = true,
                        Message = "Đăng nhập cổng sinh viên thành công.",
                        Session = new UserSession
                        {
                            UserId = 0,
                            Username = student.MSSV,
                            FullName = student.HoTen,
                            RoleId = svRole?.Id ?? 4,
                            RoleName = "SinhVien",
                            LinkedMssv = student.MSSV
                        }
                    };
                }
            }

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

        string? linkedMssv = null;
        string displayFullName = user.HoTen;

        if (user.VaiTro.TenVaiTro == "SinhVien")
        {
            // Tìm sinh viên khớp tên đăng nhập (MSSV) hoặc chọn sinh viên nội trú tiêu biểu có hợp đồng & hóa đơn mẫu
            var matchedStudent = await db.SinhViens.AsNoTracking()
                .FirstOrDefaultAsync(s => s.MSSV.ToUpper() == user.TenDangNhap.ToUpper(), ct);

            if (matchedStudent != null)
            {
                linkedMssv = matchedStudent.MSSV;
                displayFullName = matchedStudent.HoTen;
            }
            else
            {
                // Ưu tiên sinh viên có hợp đồng hiệu lực và có khoản thu tiền phòng chưa đóng để demo VietQR sinh động
                var demoContract = await db.HopDongs.AsNoTracking()
                    .Include(h => h.SinhVien)
                    .Where(h => h.TrangThai == Domain.Enums.TrangThaiHopDong.HieuLuc)
                    .OrderByDescending(h => db.KhoanThuTienPhongs.Any(k => k.HopDongId == h.Id && k.TrangThai != Domain.Enums.TrangThaiHoaDon.DaThanhToan))
                    .ThenBy(h => h.Id)
                    .FirstOrDefaultAsync(ct);

                if (demoContract != null)
                {
                    linkedMssv = demoContract.MSSV;
                    displayFullName = demoContract.SinhVien.HoTen;
                }
            }
        }

        return new LoginResult
        {
            Success = true,
            Message = "Đăng nhập thành công.",
            Session = new UserSession
            {
                UserId = user.Id,
                Username = user.TenDangNhap,
                FullName = displayFullName,
                RoleId = user.MaVaiTro,
                RoleName = user.VaiTro.TenVaiTro,
                LinkedMssv = linkedMssv
            }
        };
    }

    public async Task LogoutAsync(UserSession session, CancellationToken ct = default)
    {
        try
        {
            if (session.UserId <= 0) return;

            await using var db = await _contextFactory.CreateDbContextAsync(ct);
            db.NhatKys.Add(new NhatKy
            {
                NguoiDungId = session.UserId,
                HanhDong = "Đăng xuất",
                ChiTiet = $"Người dùng {session.Username} ({session.RoleName}) đã đăng xuất khỏi hệ thống.",
                BangLienQuan = "NguoiDung",
                KhoaChinhLienQuan = session.UserId.ToString(),
                ThoiGian = DateTime.Now
            });
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // Không chặn quá trình đăng xuất nếu ghi nhật ký gặp sự cố
        }
    }
}
