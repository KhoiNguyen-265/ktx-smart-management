using System.Drawing;
using System.Text;
using QRCoder;

namespace KTX.Infrastructure.VietQR;

public static class VietQRHelper
{
    /// <summary>
    /// Sinh chuỗi VietQR chuẩn EMVCo hoàn toàn offline không cần internet hay API bên thứ 3.
    /// </summary>
    public static string BuildEmvCoString(
        string bankBin,
        string accountNumber,
        decimal amount,
        string paymentReference)
    {
        // 00: Payload Format Indicator
        var sb = new StringBuilder();
        sb.Append(FormatTlv("00", "01"));

        // 01: Point of Initiation Method (12 = Dynamic QR với số tiền và nội dung cụ thể)
        sb.Append(FormatTlv("01", "12"));

        // 38: Consumer Account Information (NAPAS format)
        var napasInfo = new StringBuilder();
        napasInfo.Append(FormatTlv("00", "A000000727")); // NAPAS GUID

        var beneOrg = new StringBuilder();
        beneOrg.Append(FormatTlv("00", bankBin.Trim())); // Bank BIN (e.g. 970415)
        beneOrg.Append(FormatTlv("01", accountNumber.Trim())); // Số tài khoản
        napasInfo.Append(FormatTlv("01", beneOrg.ToString()));

        napasInfo.Append(FormatTlv("02", "QRIBFTTA")); // Service code chuyển nhanh 247
        sb.Append(FormatTlv("38", napasInfo.ToString()));

        // 53: Transaction Currency (704 = VND)
        sb.Append(FormatTlv("53", "704"));

        // 54: Transaction Amount
        if (amount > 0)
        {
            long amt = (long)Math.Round(amount, 0);
            sb.Append(FormatTlv("54", amt.ToString()));
        }

        // 58: Country Code
        sb.Append(FormatTlv("58", "VN"));

        // 62: Additional Data Field (Nội dung chuyển khoản / Mã hóa đơn)
        if (!string.IsNullOrWhiteSpace(paymentReference))
        {
            var addData = new StringBuilder();
            addData.Append(FormatTlv("08", paymentReference.Trim()));
            sb.Append(FormatTlv("62", addData.ToString()));
        }

        // 63: CRC16-CCITT (Checksum 4 ký tự hex)
        sb.Append("6304");
        var rawWithoutCrc = sb.ToString();
        var crc = CalculateCrc16Ccitt(Encoding.ASCII.GetBytes(rawWithoutCrc));
        return rawWithoutCrc + crc.ToString("X4");
    }

    private static string FormatTlv(string tag, string value)
    {
        int length = Encoding.UTF8.GetByteCount(value);
        return $"{tag}{length:D2}{value}";
    }

    private static ushort CalculateCrc16Ccitt(byte[] bytes)
    {
        ushort crc = 0xFFFF;
        const ushort polynomial = 0x1021;

        foreach (byte b in bytes)
        {
            for (int i = 0; i < 8; i++)
            {
                bool bit = ((b >> (7 - i)) & 1) == 1;
                bool c15 = ((crc >> 15) & 1) == 1;
                crc <<= 1;
                if (c15 ^ bit) crc ^= polynomial;
            }
        }

        return crc;
    }

    /// <summary>
    /// Sinh ảnh Bitmap mã QR từ chuỗi EMVCo bằng QRCoder
    /// </summary>
    public static Bitmap GenerateQrBitmap(string payload, int pixelsPerModule = 6)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrCodeData);
        return qrCode.GetGraphic(pixelsPerModule, Color.Black, Color.White, true);
    }
}
