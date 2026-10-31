# 🏢 Hệ thống Quản lý Ký túc xá Thông minh & Tối ưu Xếp phòng Tự động
> **Smart Dormitory Management & Multi-Objective Auto-Allocation System**  
> Nền tảng: **C# Windows Forms (.NET 10)** · **Entity Framework Core 10** · **SQL Server LocalDB** · **QuestPDF** · **ScottPlot**

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Windows Forms](https://img.shields.io/badge/UI-Windows%20Forms%20Fluent-0078D7)](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
[![Tests](https://img.shields.io/badge/Unit%20Tests-25%20Passed-22C55E?logo=xunit)](KTX.Tests/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## 🌟 Điểm nhấn Nghiệp vụ & Công nghệ (Killer Features)

1. **Thuật toán Xếp phòng Tự động Đa mục tiêu (Auto-Allocation Engine):**
   - Kết hợp thuật toán Tham lam trọng số (**Greedy Matching**) và Cải tiến cục bộ (**2-Opt Local Search**) chạy hoàn toàn trong bộ nhớ RAM (In-Memory).
   - Đảm bảo 100% ràng buộc cứng: Giới tính tòa nhà, sức chứa phòng, phòng đang hoạt động.
   - Tối ưu hàm mục tiêu: Giữ nhóm bạn ở cùng nhau (**Roommate Group Preservation**), ưu tiên diện chính sách, khoảng cách quê quán, nguyện vọng phòng/tầng.
   - Hỗ trợ **Popup Điều chỉnh Phòng Thủ công kèm cột $\Delta S$** (độ tăng giảm điểm phù hợp toán học).
   - Xử lý 2.000 hồ sơ sinh viên chỉ trong **~0.4 giây** (xem [Báo cáo Benchmark](docs/thuat_toan_benchmark_report.md)).

2. **Tính tiền Điện Nước Bậc thang & Sinh mã VietQR Offline:**
   - Tính điện lũy tiến 6 bậc EVN và nước 4 bậc sinh hoạt; tự động nhân hệ số định mức $m = \lceil \text{người} / 4 \rceil$.
   - **Xử lý trọn vẹn sự cố thay đồng hồ:** Tự động cộng dồn sản lượng từ số chốt đồng hồ cũ và đầu đồng hồ mới mà không bị lỗi vi phạm ràng buộc số mới < số cũ.
   - **Sinh chuỗi VietQR chuẩn EMVCo hoàn toàn Offline:** Tự đóng gói NAPAS GUID, BIN ngân hàng, số tài khoản, số tiền, mã hóa đơn và mã kiểm tra CRC16-CCITT mà không phụ thuộc Internet hay API trung gian.

3. **Đối soát Sao kê Ngân hàng Tự động với Cơ chế Chống trùng 2 Lớp:**
   - **Lớp 1:** Chống trùng mã giao dịch ngân hàng (`MaGiaoDich`).
   - **Lớp 2:** Chống trùng chuỗi băm mật mã: $\text{HashSaoKe} = \text{SHA256}(\text{NgayGD} \mid \text{SoTien} \mid \text{NoiDungGoc} \mid \text{SoDuSau})$.
   - Import lại nhiều lần cùng một file sao kê sẽ phát hiện và bỏ qua 100% dòng trùng lặp, không bao giờ bị ghi nhận doanh thu hai lần.
   - Phân loại 3 tab trực quan: **Đã khớp hoàn toàn**, **Khớp một phần**, và **Cần xử lý**.

4. **Quét Mã QR Căn Cước Công Dân (CCCD) & Tự Điền Form:**
   - Hỗ trợ cả 2 chế độ: Quét trực tiếp bằng máy đọc Barcode/chuỗi QR và đọc từ tệp ảnh chụp thẻ CCCD bằng thư viện **ZXing.Net**.
   - Tự động bóc tách: Số CCCD, Họ tên, Ngày sinh, Giới tính, Nơi thường trú.
   - Tự động nhận diện Tỉnh/Thành phố quê quán và ánh xạ về danh mục chuẩn.

5. **Sơ đồ Phòng Tự vẽ (Owner-Drawn `KRoomMap`):**
   - Tự vẽ toàn bộ phòng và giường bằng GDI+ với kỹ thuật **DoubleBuffered**, khắc phục triệt để hiện tượng giật lag (flickering) của WinForms truyền thống khi hiển thị nhiều phòng.
   - Mã màu trực quan theo tỷ lệ lấp đầy: Phòng trống (Xanh lá), Còn chỗ (Xanh dương), Đã đầy (Đỏ), Đang bảo trì (Xám).

6. **Bảng Điều khiển Dashboard Hiện đại & Biểu đồ ScottPlot:**
   - 4 Card KPI thống kê theo thời gian thực (Tổng số phòng, Sinh viên lưu trú, Doanh thu tháng, Danh sách cần xử lý).
   - 2 đồ thị ScottPlot: Tỷ lệ lấp đầy theo từng Tòa nhà và Cơ cấu doanh thu 6 tháng gần nhất.
   - Tích hợp **Command Palette (Ctrl + K)**: Tìm kiếm nhanh sinh viên, phòng ký túc xá và điều hướng mọi chức năng trong 1 nốt nhạc.

7. **Thiết kế Giao diện Hiện đại Fluent Design (`KTX.UI`):**
   - Hỗ trợ 2 chế độ giao diện: **Light Mode** & **Dark Mode** chuyển đổi tức thì.
   - Chuẩn hóa High DPI (`PerMonitorV2`), không vỡ giao diện trên màn hình độ phân giải cao (100%, 125%, 150%).
   - Xuất hợp đồng và báo cáo PDF chuẩn mực văn bản hành chính Việt Nam bằng thư viện **QuestPDF** với font chữ Unicode Lato.

---

## 🏗️ Kiến trúc Giải pháp (Solution Architecture)

Dự án được cấu trúc thành 7 Project theo mô hình đa tầng chuẩn Clean Architecture:

```
KTX.sln
├── KTX.Domain           # Entities, DTOs, Enums, Value Objects
├── KTX.Data             # DbContext, Fluent API Configurations, Migrations, Seeder, Interceptors
├── KTX.Services         # Nghiệp vụ: AllocationEngine, TieredBilling, Reconcile, Contract, Student...
├── KTX.Infrastructure   # VietQR EMVCo, CCCD Scanner, QuestPDF, ClosedXML Excel
├── KTX.UI               # Design System: KRoomMap, KCard, KButton, KTextBox, KBadge, KSidebar, Themes
├── KTX.WinForms         # Windows Forms Shell, Views, Forms, Dependency Injection, Program.cs
└── KTX.Tests            # 25 Unit & Benchmark Tests (xUnit, FluentAssertions, Moq)
```

---

## 🚀 Hướng dẫn Cài đặt & Khởi chạy

### 1. Yêu cầu Hệ thống
- Hệ điều hành: Windows 10 / Windows 11 (64-bit).
- .NET SDK 10.0 (hoặc mới hơn).
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) - đã tích hợp sẵn trong Visual Studio.

### 2. Khởi chạy Ứng dụng
Mở Windows Terminal / PowerShell tại thư mục gốc của dự án và chạy:

```powershell
# 1. Khôi phục các thư viện NuGet
dotnet restore KTX.sln

# 2. Biên dịch dự án
dotnet build KTX.sln

# 3. Chạy ứng dụng WinForms
dotnet run --project KTX.WinForms\KTX.WinForms.csproj
```

> **Cơ chế Tự động hóa Khởi tạo (Database Auto-Seed):**  
> Khi ứng dụng chạy lần đầu tiên, hệ thống sẽ **tự động áp dụng Migration và nạp toàn bộ dữ liệu mẫu ban đầu**:
> - Danh mục 34 Tỉnh/Thành phố chuẩn mới kèm bảng ánh xạ 63 tỉnh cũ.
> - 4 Tòa nhà (A, B, C, D) với 120 phòng ký túc xá.
> - 600 hồ sơ sinh viên và 500 đơn đăng ký mẫu sẵn sàng chạy thử thuật toán xếp phòng.
> - Bậc giá điện 6 bậc EVN và giá nước sinh hoạt 4 bậc.

### 3. Tài khoản Đăng nhập Mặc định

| Vai trò | Tên đăng nhập | Mật khẩu | Quyền hạn |
|---|---|---|---|
| **Quản trị viên (Admin)** | `admin` | `admin123` | Toàn quyền toàn bộ hệ thống |
| **Quản lý KTX (Manager)** | `quanly1` | `admin123` | Quản lý sinh viên, phòng, chạy xếp phòng AI, duyệt hợp đồng |
| **Kế toán (Accountant)** | `ketoan1` | `admin123` | Nhập chỉ số điện nước, lập hóa đơn, đối soát sao kê ngân hàng |

---

## 🧪 Kiểm thử Tự động (Unit & Performance Tests)

Dự án có sẵn bộ 25 bài kiểm thử tự động bao phủ toàn bộ các luồng nghiệp vụ quan trọng:

```powershell
# Chạy toàn bộ 25 bài kiểm thử
dotnet test KTX.Tests\KTX.Tests.csproj
```

Kết quả: **25 / 25 Tests Passed (100%)**
- `Phase3Tests`: Thử nghiệm ánh xạ tên tỉnh cũ sang tỉnh mới, danh mục phòng.
- `Phase4Tests`: Kiểm tra ràng buộc cứng thuật toán xếp phòng, tính điểm ưu tiên, bảo toàn nhóm bạn.
- `Phase5Tests`: Vòng đời hợp đồng, chuyển phòng ghi lịch sử, khấu trừ cọc khi trả phòng.
- `Phase6Tests`: Tính điện nước bậc thang, chống ghi nhận trùng 2 lớp khi import sao kê ngân hàng, sinh mã VietQR EMVCo.
- `Phase7Tests`: Bóc tách thông tin thẻ CCCD, quản lý sự cố, xuất báo cáo tài chính PDF.
- `AlgorithmBenchmarkTests`: Đo lường thời gian chạy thuật toán với 100, 500, 1.000 và 2.000 đơn đăng ký.

---

## 📁 Tài liệu Dự án Kèm theo
- [Hồ sơ Phân tích & Thiết kế Hệ thống (SRS & SAD)](docs/ho_so_phan_tich_thiet_ke_phase1.md)
- [Bảng Ma trận Kiểm thử 42 Test Cases](docs/test_matrix_40_cases.md)
- [Báo cáo Đo lường Hiệu năng Thuật toán Xếp phòng](docs/thuat_toan_benchmark_report.md)
- [Kế hoạch Thực hiện Chi tiết](file:///C:/Users/Nguyen%20Tien%20Khoi/.gemini/antigravity/brain/4acddb18-8403-4fa2-aa4b-feb22923e291/ke_hoach_ktx.md)
