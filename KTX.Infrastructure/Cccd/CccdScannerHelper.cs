using System.Drawing;
using System.Text.RegularExpressions;
using KTX.Domain.Enums;
using ZXing;
using ZXing.Windows.Compatibility;

namespace KTX.Infrastructure.Cccd;

public class CccdInfo
{
    public string CCCD { get; set; } = string.Empty;
    public string? CMNDCu { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public DateTime? NgaySinh { get; set; }
    public GioiTinh GioiTinh { get; set; } = GioiTinh.Nam;
    public string DiaChi { get; set; } = string.Empty;
    public DateTime? NgayCap { get; set; }
    public string? TenTinhThanhNhanDien { get; set; }
    public string RawQr { get; set; } = string.Empty;
}

public static class CccdScannerHelper
{
    /// <summary>
    /// Phân tích chuỗi QR chuẩn CCCD gắn chip của Bộ Công An:
    /// Định dạng: SoCCCD|SoCMNDCu|HoVaTen|ddMMyyyy|GioiTinh|DiaChiThuongTru|NgayCap
    /// </summary>
    public static CccdInfo? ParseCccdQrString(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var parts = raw.Split('|');
        if (parts.Length < 6) return null;

        var info = new CccdInfo
        {
            RawQr = raw,
            CCCD = parts[0].Trim(),
            CMNDCu = string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1].Trim(),
            HoTen = FormatName(parts[2].Trim()),
            DiaChi = parts[5].Trim()
        };

        // 1. Phân tích ngày sinh (ddMMyyyy)
        if (parts[3].Trim().Length == 8 &&
            DateTime.TryParseExact(parts[3].Trim(), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out var ns))
        {
            info.NgaySinh = ns;
        }

        // 2. Phân tích giới tính
        var gtStr = parts[4].Trim().ToLowerInvariant();
        if (gtStr.Contains("nữ") || gtStr.Contains("nu") || gtStr == "f" || gtStr == "female")
        {
            info.GioiTinh = GioiTinh.Nu;
        }
        else
        {
            info.GioiTinh = GioiTinh.Nam;
        }

        // 3. Phân tích ngày cấp nếu có (phần tử thứ 7)
        if (parts.Length >= 7 && parts[6].Trim().Length == 8 &&
            DateTime.TryParseExact(parts[6].Trim(), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out var nc))
        {
            info.NgayCap = nc;
        }

        // 4. Nhận diện Tỉnh/Thành từ chuỗi địa chỉ thường trú
        info.TenTinhThanhNhanDien = ExtractProvinceFromAddress(info.DiaChi);

        return info;
    }

    /// <summary>
    /// Giải mã mã QR từ ảnh Bitmap sử dụng thư viện ZXing.Net
    /// </summary>
    public static CccdInfo? DecodeQrFromBitmap(Bitmap bitmap)
    {
        try
        {
            var reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                }
            };

            var result = reader.Decode(bitmap);
            if (result == null || string.IsNullOrWhiteSpace(result.Text)) return null;

            return ParseCccdQrString(result.Text);
        }
        catch
        {
            return null;
        }
    }

    private static string FormatName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName)) return string.Empty;
        var words = rawName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            var w = words[i].ToLowerInvariant();
            if (w.Length > 0)
            {
                words[i] = char.ToUpperInvariant(w[0]) + w.Substring(1);
            }
        }
        return string.Join(" ", words);
    }

    private static string? ExtractProvinceFromAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        // Chuỗi thường có định dạng: "Thôn..., Xã..., Huyện..., Tỉnh/TP..."
        var segments = address.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return null;

        var lastSegment = segments[^1].Trim();

        // Bỏ tiền tố "Tỉnh ", "TP ", "Thành phố ", "TP. "
        var cleaned = Regex.Replace(lastSegment, @"^(Tỉnh|Thành phố|TP\.|TP)\s+", "", RegexOptions.IgnoreCase).Trim();
        return cleaned;
    }
}
