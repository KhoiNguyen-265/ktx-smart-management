# BẢNG MA TRẬN KIỂM THỬ TOÀN DIỆN HỆ THỐNG (TEST MATRIX - 42 TEST CASES)
**Đồ án:** Hệ thống Quản lý Ký túc xá Thông minh & Tối ưu Xếp phòng Tự động
**Nền tảng:** C# Windows Forms (.NET 10) · Entity Framework Core 10 · SQL Server LocalDB

---

## 1. Phân hệ Xác thực & Quản trị Hệ thống (Authentication & RBAC)

| ID | Tên Test Case | Điều kiện đầu vào | Các bước thực hiện | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|---|
| **TC01** | Đăng nhập hợp lệ | Tài khoản `admin` / `admin123` | 1. Nhập username, password đúng<br>2. Nhấn "Đăng nhập" | Đăng nhập thành công, mở MainForm, hiển thị vai trò "Admin", nạp Dashboard | PASS |
| **TC02** | Khóa tài khoản sau 5 lần sai | Tài khoản `quanly1` | 1. Cố ý nhập sai mật khẩu 5 lần liên tiếp | Báo lỗi số lần sai, đến lần thứ 5 thông báo tài khoản bị khóa 15 phút, từ chối đăng nhập | PASS |
| **TC03** | Mở khóa tự động sau thời gian chờ | Tài khoản bị khóa | 1. Chờ hết thời gian `KhoaDen` (hoặc admin mở khóa)<br>2. Nhập đúng mật khẩu | Cho phép đăng nhập bình thường, reset `SoLanSai = 0` | PASS |
| **TC04** | Phân quyền truy cập theo vai trò | Tài khoản nhân viên kế toán | 1. Đăng nhập bằng tài khoản Kế toán<br>2. Kiểm tra menu thanh Sidebar | Chỉ thao tác được chức năng Hóa đơn, Thu tiền, Đối soát; ẩn/vô hiệu hóa Xếp phòng & Cài đặt hệ thống | PASS |
| **TC05** | Chuyển đổi giao diện Light/Dark | Ứng dụng đang mở | 1. Nhấn nút "🌓 Đổi giao diện" trên thanh Topbar | Toàn bộ Form, Sidebar, DataGridView, Card và ScottPlot đồng bộ chuyển sang bảng màu Dark/Light tức thì | PASS |

---

## 2. Phân hệ Danh mục Tòa, Phòng & Sơ đồ KRoomMap

| ID | Tên Test Case | Điều kiện đầu vào | Các bước thực hiện | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|---|
| **TC06** | Nạp sơ đồ phòng trực quan | Chọn Tòa A1, Tầng 1 | 1. Chọn tòa nhà A1<br>2. Chọn Tầng 1 | `KRoomMap` tự vẽ (owner-drawn) 10 phòng của tầng 1, không giật lag (nhờ DoubleBuffered) | PASS |
| **TC07** | Hiển thị màu sắc theo tỷ lệ lấp đầy | Phòng có số người ở khác nhau | 1. Quan sát màu sắc phòng trên `KRoomMap` | Phòng trống (Xanh lá), Còn chỗ (Xanh dương), Đầy (Đỏ), Bảo trì (Xám) | PASS |
| **TC08** | Xem chi tiết phòng & danh sách giường | Click vào phòng A101 | 1. Click chuột trái vào ô phòng A101 | Panel bên phải hiển thị: Danh sách sinh viên từng giường, tài sản phòng (điều hòa, nóng lạnh), công nợ điện nước | PASS |
| **TC09** | Chuyển trạng thái phòng sang bảo trì | Phòng đang trống | 1. Chọn phòng<br>2. Nhấn "Bảo trì"<br>3. Xác nhận | Trạng thái chuyển `BaoTri`, màu xám, thuật toán xếp phòng không được xếp sinh viên vào | PASS |
| **TC10** | Chặn đưa phòng đang có sinh viên vào bảo trì | Phòng có 2 sinh viên | 1. Nhấn "Bảo trì" cho phòng đang có sinh viên | Hệ thống từ chối và cảnh báo phải chuyển hết sinh viên sang phòng khác trước | PASS |

---

## 3. Phân hệ Quản lý Sinh viên & Excel 34 Tỉnh thành

| ID | Tên Test Case | Điều kiện đầu vào | Các bước thực hiện | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|---|
| **TC11** | Tìm kiếm sinh viên đa tiêu chí | Từ khóa `2026` hoặc `Nguyễn` | 1. Nhập vào ô tìm kiếm trên thanh công cụ | DataGridView lọc realtime theo MSSV, Họ tên, CCCD hoặc Khoa | PASS |
| **TC12** | Tải file Excel mẫu chuẩn 34 tỉnh | Không có file mẫu | 1. Nhấn "📋 Tải mẫu Excel" | Xuất file `.xlsx` có sẵn Data Validation Dropdown danh mục 34 tỉnh/thành phố chuẩn mới | PASS |
| **TC13** | Import Excel thành công | File Excel hợp lệ 20 sinh viên | 1. Nhấn "📥 Nhập Excel", chọn file | Import thành công 20/20 sinh viên, lưu DB, nạp lại bảng dữ liệu | PASS |
| **TC14** | Ánh xạ thông minh 63 tỉnh cũ về 34 tỉnh mới | File chứa "Hà Tây", "Bình Trị Thiên" | 1. Import file Excel có cột quê quán ghi tên tỉnh cũ | Hệ thống tra bảng `TinhThanhCu`, tự động ánh xạ "Hà Tây" -> "Hà Nội", không báo lỗi | PASS |
| **TC15** | Bắt lỗi dữ liệu Excel không hợp lệ | File thiếu MSSV, sai format CCCD | 1. Import file có dòng lỗi | Hiển thị thông báo chi tiết: "Dòng X [MSSV]: Số CCCD không đủ 12 chữ số", không làm crash hệ thống | PASS |
| **TC16** | Xuất danh sách sinh viên ra Excel | Danh sách 600 sinh viên | 1. Nhấn "📤 Xuất Excel" | Xuất file Excel chuẩn ClosedXML có định dạng màu, border và ngày xuất | PASS |

---

## 4. Phân hệ Xếp phòng Tự động AI (Killer Feature)

| ID | Tên Test Case | Điều kiện đầu vào | Các bước thực hiện | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|---|
| **TC17** | Ràng buộc cứng: Giới tính tòa nhà | Sinh viên Nữ, Tòa A (Nam) | 1. Chạy thuật toán xếp phòng | 100% sinh viên Nữ chỉ được xếp vào tòa Nữ, sinh viên Nam vào tòa Nam | PASS |
| **TC18** | Ràng buộc cứng: Sức chứa phòng | Phòng 4 người đang có 3 SV | 1. Chạy thuật toán | Chỉ xếp tối đa thêm 1 sinh viên vào phòng đó, không bao giờ vượt sức chứa | PASS |
| **TC19** | Bảo toàn nhóm bạn cùng đăng ký | Nhóm 3 bạn đăng ký chung | 1. Chạy thuật toán | Nhóm 3 bạn được xếp vào chung 1 phòng còn trống ≥ 3 chỗ | PASS |
| **TC20** | Phân rã nhóm thông minh khi thiếu phòng lớn | Nhóm 6 bạn, chỉ còn phòng 4 | 1. Chạy xếp phòng với cờ `ChoPhepTachNhom = true` | Tách nhóm thành 4 + 2 và xếp vào 2 phòng cùng tầng/cùng tòa gần nhau | PASS |
| **TC21** | Ưu tiên diện chính sách & khoảng cách xa | SV diện mồ côi/vùng sâu vs SV thường | 1. Tính điểm ưu tiên $P$ | Sinh viên chính sách có $P$ cao hơn hẳn và được xếp phòng trước | PASS |
| **TC22** | Tối ưu hóa 2-Opt Local Search | Kết quả xếp phòng ban đầu | 1. Kích hoạt bước Local Search cải tiến | Hoán đổi vị trí giữa các sinh viên để tăng điểm tổng hàm mục tiêu $S(s, r)$ | PASS |
| **TC23** | Điều chỉnh thủ công bằng Popup ΔS | Kết quả xếp phòng sau chạy AI | 1. Click vào sinh viên<br>2. Nhấn "Điều chỉnh phòng"<br>3. Chọn phòng mới | Hiển thị cột $\Delta S$ (độ chênh lệch điểm phù hợp), cập nhật phòng mới an toàn | PASS |
| **TC24** | Sinh hợp đồng hàng loạt từ phiên xếp phòng | Phiên xếp phòng đã phê duyệt | 1. Nhấn "Sinh Hợp Đồng & Nhập Học" | Tự động tạo 500 bản ghi `HopDong` trong 1 Transaction, phòng cập nhật số người ở | PASS |

---

## 5. Phân hệ Hợp đồng & Vòng đời Lưu trú (Stay Lifecycle)

| ID | Tên Test Case | Điều kiện đầu vào | Các bước thực hiện | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|---|
| **TC25** | Cảnh báo hợp đồng sắp hết hạn | Hợp đồng còn hạn 20 ngày | 1. Mở màn hình Hợp đồng | Hệ thống gắn badge vàng "Sắp hết hạn", hiển thị trong danh sách cảnh báo | PASS |
| **TC26** | Gia hạn hợp đồng | Hợp đồng đang hiệu lực | 1. Chọn hợp đồng<br>2. Nhấn "Gia hạn thêm 6 tháng" | Ngày kết thúc tăng thêm 6 tháng, tạo khoản thu tiền phòng cho kỳ mới | PASS |
| **TC27** | Chuyển phòng sinh viên | Sinh viên phòng A101 sang A102 | 1. Nhấn "Chuyển phòng"<br>2. Chọn phòng đích còn chỗ<br>3. Nhập lý do | Sinh viên sang phòng A102, phòng cũ giảm 1 người, phòng mới tăng 1 người, ghi log `LichSuChuyenPhong` | PASS |
| **TC28** | Trả phòng & Khấu trừ cọc thanh lý | Sinh viên hết hạn hoặc xin rút | 1. Nhấn "Thủ tục trả phòng"<br>2. Nhập chi phí đền bù tài sản | Tính số tiền cọc hoàn lại = `TienCoc - NoTienPhong - NoDienNuoc - DenBuTaiSan`, đổi trạng thái `DaThanhLy` | PASS |
| **TC29** | Xuất Hợp đồng KTX chuẩn pháp lý (QuestPDF) | Hợp đồng bất kỳ | 1. Chọn hợp đồng<br>2. Nhấn "In Hợp Đồng PDF" | Xuất file PDF 2 trang chuẩn văn bản hành chính Việt Nam, font Lato Unicode nét căng, có đầy đủ điều khoản | PASS |

---

## 6. Phân hệ Tính tiền Điện Nước Bậc thang & VietQR Offline

| ID | Tên Test Case | Điều kiện đầu vào | Các bước thực hiện | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|---|
| **TC30** | Tính điện lũy tiến 6 bậc chuẩn EVN | Phòng 4 người, dùng 250 kWh | 1. Nhập chỉ số điện<br>2. Tính tiền | Bậc 1: 50kWh, Bậc 2: 50kWh, Bậc 3: 100kWh, Bậc 4: 50kWh nhân theo đơn giá EVN chuẩn | PASS |
| **TC31** | Nhân hệ số định mức số người | Phòng 8 người ($m = \lceil 8/4 \rceil = 2$) | 1. Tính điện nước phòng 8 người | Hạn mức từng bậc được nhân 2 (Bậc 1 từ 50 lên 100 kWh), đảm bảo quyền lợi sinh viên | PASS |
| **TC32** | Chế độ thay đồng hồ (Công tơ bị cháy) | Đồng hồ cũ chốt 1530, mới từ 0 lên 20 | 1. Tích chọn "Có thay đồng hồ"<br>2. Nhập số chốt cũ 1530, đầu mới 0 | Sản lượng = (1530 - 1500) + (20 - 0) = 50 kWh, không báo lỗi chỉ số mới < chỉ số cũ | PASS |
| **TC33** | Cảnh báo tiêu thụ điện bất thường | Sản lượng tháng này gấp 2 lần TB | 1. Nhập sản lượng 400 kWh (TB là 150) | Cột "Bất thường" gắn cờ đỏ cảnh báo nghi rò rỉ điện hoặc sử dụng bếp lén | PASS |
| **TC34** | Sinh chuỗi VietQR chuẩn EMVCo offline | Hóa đơn KTXDN000001, 540.000 đ | 1. Xem hóa đơn điện nước | Chuỗi QR sinh ra đúng chuẩn EMVCo (NAPAS, BIN ngân hàng, STK, 540.000 đ, KTXDN000001, CRC16) | PASS |
| **TC35** | Quét VietQR từ app ngân hàng | App MBBank/Vietcombank thật | 1. Mở app ngân hàng quét ảnh QR trên màn hình | App ngân hàng tự động điền đúng Số tiền 540.000 đ và Nội dung chuyển khoản KTXDN000001 | PASS |

---

## 7. Phân hệ Đối soát Sao kê Ngân hàng & Chống trùng 2 lớp

| ID | Tên Test Case | Điều kiện đầu vào | Các bước thực hiện | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|---|
| **TC36** | Đối soát khớp mã tự động | File sao kê có dòng `CK KTXDN000001` | 1. Nhấn "Nhập File Sao Kê Excel" | Giao dịch tự động chuyển tab "Đã khớp", hóa đơn KTXDN000001 đổi trạng thái `DaThanhToan` | PASS |
| **TC37** | Thanh toán một phần | Sinh viên nợ 1.000.000 đ, chỉ chuyển 500.000 đ | 1. Nhập sao kê ngân hàng | Giao dịch vào tab "Khớp một phần", hóa đơn ghi nhận đã thu 500.000 đ, còn nợ 500.000 đ | PASS |
| **TC38** | Giao dịch nội dung lạ cần kế toán xử lý | Dòng sao kê "Nguyen Van A chuyển tiền" (không mã KTX) | 1. Nhập sao kê | Giao dịch tự động chuyển sang tab "Cần xử lý" để kế toán tra cứu thủ công | PASS |
| **TC39** | Chống trùng 2 lớp khi import lặp file | File sao kê đã import trước đó | 1. Cố ý import lại toàn bộ file sao kê của tuần trước | Hệ thống phát hiện 100% dòng trùng lặp qua Mã GD + Hash SHA256, ghi nhận 0 bản ghi mới, cảnh báo banner cam | PASS |

---

## 8. Phân hệ Tiện ích, Quét CCCD & Báo cáo Thống kê

| ID | Tên Test Case | Điều kiện đầu vào | Các bước thực hiện | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|---|
| **TC40** | Quét QR CCCD tự động điền form | Chuỗi QR hoặc ảnh chụp thẻ CCCD | 1. Nhấn "Quét QR CCCD"<br>2. Nhập mã hoặc chọn ảnh | Tự động bóc tách Họ tên, Số CCCD, Ngày sinh, Giới tính, Quê quán và điền sạch sẽ vào hệ thống | PASS |
| **TC41** | Phím tắt thông minh Command Palette | Nhấn phím `Ctrl + K` | 1. Bấm `Ctrl + K`<br>2. Gõ "Xếp phòng" hoặc "A101" | Menu Spotlight mở tức thì, nhấn Enter chuyển ngay đến chức năng/phòng tương ứng | PASS |
| **TC42** | Xuất báo cáo tài chính tháng ra PDF | Dữ liệu tháng 10/2026 | 1. Mở tab Báo cáo<br>2. Nhấn "Xuất Báo Cáo Tài Chính (.pdf)" | Tạo file PDF tổng hợp thu tiền phòng, điện nước, chi phí bảo trì và chữ ký xác nhận chuẩn mực | PASS |

---

## 9. Tổng kết kết quả kiểm thử
- **Tổng số Test Cases:** 42 / 42 cases
- **Passed:** 42 (100%)
- **Failed:** 0
- **Mức độ bao phủ (Coverage):** Toàn bộ 26 bảng CSDL, 7 tầng dịch vụ nghiệp vụ, và toàn bộ các trường hợp ngoại lệ thực tế (thay đồng hồ, trùng sao kê, chuyển phòng giữa kỳ, tách nhóm bạn).
