using KTX.Domain.Enums;

namespace KTX.Domain.DTOs;

public class StudentDto
{
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateTime NgaySinh { get; set; }
    public GioiTinh GioiTinh { get; set; }
    public string GioiTinhText => GioiTinh == GioiTinh.Nam ? "Nam" : "Nữ";
    public string CCCD { get; set; } = string.Empty;
    public string Khoa { get; set; } = string.Empty;
    public int KhoaHoc { get; set; }
    public string SoDienThoai { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int TinhThanhId { get; set; }
    public string TenTinhThanh { get; set; } = string.Empty;
    public int KhoangCachKm { get; set; }

    public int? DienUuTienId { get; set; }
    public string TenDienUuTien { get; set; } = "Không";
    public int DiemCong { get; set; }

    public string? PhongHienTai { get; set; }
    public string? TrangThaiLuuTru { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class StudentCreateUpdateDto
{
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateTime NgaySinh { get; set; } = DateTime.Now.AddYears(-20);
    public GioiTinh GioiTinh { get; set; } = GioiTinh.Nam;
    public string CCCD { get; set; } = string.Empty;
    public string Khoa { get; set; } = string.Empty;
    public int KhoaHoc { get; set; } = 68;
    public string SoDienThoai { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TinhThanhId { get; set; }
    public int? DienUuTienId { get; set; }
}
