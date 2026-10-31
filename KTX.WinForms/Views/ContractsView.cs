using KTX.Domain.DTOs;
using KTX.Domain.Enums;
using KTX.Services.Contracts;
using KTX.Services.Facility;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class ContractsView : UserControl, IThemable
{
    private readonly IContractService _contractService;
    private readonly IRoomService _roomService;

    private readonly KTextBox _txtSearch;
    private readonly ComboBox _cboStatus;
    private readonly KButton _btnTransfer;
    private readonly KButton _btnExtend;
    private readonly KButton _btnCheckOut;
    private readonly KButton _btnPrintPdf;
    private readonly DataGridView _grid;

    private readonly KButton _btnPrev;
    private readonly KButton _btnNext;
    private readonly Label _lblPageInfo;

    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalPages = 1;
    private List<ContractDto> _contracts = new();

    public ContractsView(IContractService contractService, IRoomService roomService)
    {
        _contractService = contractService;
        _roomService = roomService;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        Padding = new Padding(0);

        // Header Action Bar
        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(0, 0, 0, 14)
        };

        _txtSearch = new KTextBox
        {
            Placeholder = "Tìm theo MSSV, Họ tên, Phòng hoặc CCCD...",
            Location = new Point(0, 8),
            Size = new Size(320, 40)
        };
        _txtSearch.InnerTextBox.TextChanged += async (s, e) =>
        {
            _currentPage = 1;
            await LoadContractsAsync();
        };

        _cboStatus = new ComboBox
        {
            Location = new Point(335, 12),
            Size = new Size(160, 36),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F)
        };
        _cboStatus.Items.AddRange(new object[] { "Tất cả trạng thái", "Đang hiệu lực", "Sắp hết hạn (<30 ngày)", "Đã trả phòng" });
        _cboStatus.SelectedIndex = 0;
        _cboStatus.SelectedIndexChanged += async (s, e) =>
        {
            _currentPage = 1;
            await LoadContractsAsync();
        };

        _btnTransfer = new KButton
        {
            Text = "Chuyển phòng",
            Style = KButtonStyle.Secondary,
            Location = new Point(510, 8),
            Size = new Size(120, 40)
        };
        _btnTransfer.Click += async (s, e) => await HandleTransferRoomAsync();

        _btnExtend = new KButton
        {
            Text = "Gia hạn",
            Style = KButtonStyle.Secondary,
            Location = new Point(640, 8),
            Size = new Size(90, 40)
        };
        _btnExtend.Click += async (s, e) => await HandleExtendAsync();

        _btnCheckOut = new KButton
        {
            Text = "Trả phòng & Cọc",
            Style = KButtonStyle.Secondary,
            Location = new Point(740, 8),
            Size = new Size(130, 40)
        };
        _btnCheckOut.Click += async (s, e) => await HandleCheckOutAsync();

        _btnPrintPdf = new KButton
        {
            Text = "In Hợp đồng PDF",
            Style = KButtonStyle.Primary,
            Location = new Point(880, 8),
            Size = new Size(140, 40)
        };
        _btnPrintPdf.Click += async (s, e) => await HandlePrintPdfAsync();

        topBar.Controls.AddRange(new Control[] { _txtSearch, _cboStatus, _btnTransfer, _btnExtend, _btnCheckOut, _btnPrintPdf });

        // DataGridView
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        KDataGridHelper.ApplyModernStyle(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số HĐ", DataPropertyName = "Id", FillWeight = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MSSV", DataPropertyName = "MSSV", FillWeight = 85 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", DataPropertyName = "HoTen", FillWeight = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng", DataPropertyName = "MaPhong", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tòa", DataPropertyName = "ToaTen", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bắt đầu", DataPropertyName = "NgayBatDauText", FillWeight = 85 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kết thúc", DataPropertyName = "NgayKetThucText", FillWeight = 85 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá/tháng", DataPropertyName = "GiaThucTeText", FillWeight = 95 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tiền cọc", DataPropertyName = "TienCocText", FillWeight = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiText", FillWeight = 110 });

        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 9 && _contracts.Count > e.RowIndex)
            {
                var c = _contracts[e.RowIndex];
                if (c.IsExpiringSoon)
                {
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
                }
                else if (c.TrangThai == TrangThaiHopDong.HieuLuc)
                {
                    e.CellStyle.ForeColor = Color.ForestGreen;
                }
                else
                {
                    e.CellStyle.ForeColor = Color.Gray;
                }
            }
        };

        // Bottom Pagination Bar
        var bottomBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            Padding = new Padding(0, 10, 0, 0)
        };

        _btnPrev = new KButton
        {
            Text = "Trang trước",
            Style = KButtonStyle.Secondary,
            Size = new Size(100, 34),
            Location = new Point(0, 8)
        };
        _btnPrev.Click += async (s, e) =>
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadContractsAsync();
            }
        };

        _lblPageInfo = new Label
        {
            Text = "Trang 1 / 1 (Tổng 0 hợp đồng)",
            Font = UIHelper.GetFont(9.5f, FontStyle.Regular),
            Location = new Point(115, 14),
            AutoSize = true
        };

        _btnNext = new KButton
        {
            Text = "Trang sau",
            Style = KButtonStyle.Secondary,
            Size = new Size(100, 34),
            Location = new Point(340, 8)
        };
        _btnNext.Click += async (s, e) =>
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadContractsAsync();
            }
        };

        bottomBar.Controls.AddRange(new Control[] { _btnPrev, _lblPageInfo, _btnNext });

        Controls.Add(_grid);
        Controls.Add(bottomBar);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);
        Load += async (s, e) => await LoadContractsAsync();
    }

    private async Task LoadContractsAsync()
    {
        TrangThaiHopDong? status = null;
        bool? expiringSoon = null;

        if (_cboStatus.SelectedIndex == 1) status = TrangThaiHopDong.HieuLuc;
        else if (_cboStatus.SelectedIndex == 2) expiringSoon = true;
        else if (_cboStatus.SelectedIndex == 3) status = TrangThaiHopDong.DaTra;

        var result = await _contractService.GetContractsPagedAsync(
            search: _txtSearch.Text,
            status: status,
            expiringSoon: expiringSoon,
            pageIndex: _currentPage,
            pageSize: _pageSize);

        _contracts = result.Items;
        _totalPages = Math.Max(1, result.TotalPages);
        _lblPageInfo.Text = $"Trang {_currentPage} / {_totalPages} (Tổng {result.TotalCount:N0} hợp đồng)";
        _btnPrev.Enabled = _currentPage > 1;
        _btnNext.Enabled = _currentPage < _totalPages;

        _grid.AutoGenerateColumns = false;
        _grid.DataSource = _contracts.Select(c => new
        {
            c.Id,
            c.MSSV,
            c.HoTen,
            c.MaPhong,
            c.ToaTen,
            NgayBatDauText = c.NgayBatDau.ToString("dd/MM/yyyy"),
            NgayKetThucText = c.NgayKetThuc.ToString("dd/MM/yyyy"),
            GiaThucTeText = $"{c.GiaThucTe:N0} đ",
            TienCocText = $"{c.TienCoc:N0} đ",
            TrangThaiText = c.IsExpiringSoon ? $"⚠️ Sắp hết ({c.SoNgayConLai} ngày)" :
                           c.TrangThai == TrangThaiHopDong.HieuLuc ? "🟢 Có hiệu lực" :
                           c.TrangThai == TrangThaiHopDong.DaTra ? "⚪ Đã trả phòng" : "🔴 Đã hủy"
        }).ToList();
    }

    private ContractDto? GetSelectedContract()
    {
        if (_grid.CurrentRow?.Index >= 0 && _grid.CurrentRow.Index < _contracts.Count)
        {
            return _contracts[_grid.CurrentRow.Index];
        }
        return null;
    }

    private async Task HandleTransferRoomAsync()
    {
        var contract = GetSelectedContract();
        if (contract == null)
        {
            MessageBox.Show("Vui lòng chọn một hợp đồng trên danh sách để chuyển phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (contract.TrangThai != TrangThaiHopDong.HieuLuc)
        {
            MessageBox.Show("Chỉ có thể chuyển phòng cho hợp đồng đang có hiệu lực.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Show prompt for target room code
        string prompt = Microsoft.VisualBasic.Interaction.InputBox(
            $"Sinh viên: {contract.HoTen}\nPhòng hiện tại: {contract.MaPhong} ({contract.ToaTen})\n\nNhập mã phòng muốn chuyển đến (ví dụ: A102):",
            "Chuyển phòng KTX",
            "");

        if (string.IsNullOrWhiteSpace(prompt)) return;

        // Lookup target room
        var buildings = await _roomService.GetBuildingsAsync();
        int? targetRoomId = null;
        foreach (var b in buildings)
        {
            var rooms = await _roomService.GetRoomsByBuildingAndFloorAsync(b.Id, 1);
            var found = rooms.FirstOrDefault(r => r.MaPhong.Equals(prompt.Trim(), StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                targetRoomId = found.Id;
                break;
            }
        }

        if (!targetRoomId.HasValue)
        {
            MessageBox.Show($"Không tìm thấy phòng có mã '{prompt.Trim()}'.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var result = await _contractService.TransferRoomAsync(new RoomTransferRequestDto
        {
            HopDongId = contract.Id,
            DenPhongId = targetRoomId.Value,
            LyDo = "Theo nguyện vọng của sinh viên"
        });

        if (result.Success)
        {
            KToast.Show(FindForm() ?? new Form(), result.Message, ToastType.Success);
            await LoadContractsAsync();
        }
        else
        {
            MessageBox.Show(result.Message, "Không thể chuyển phòng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task HandleExtendAsync()
    {
        var contract = GetSelectedContract();
        if (contract == null || contract.TrangThai != TrangThaiHopDong.HieuLuc)
        {
            MessageBox.Show("Vui lòng chọn một hợp đồng đang có hiệu lực để gia hạn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var newEndDate = contract.NgayKetThuc.AddMonths(5); // Gia hạn thêm 1 học kỳ
        var confirm = MessageBox.Show(
            $"Xác nhận gia hạn hợp đồng cho sinh viên {contract.HoTen} thêm 5 tháng?\n" +
            $"• Ngày kết thúc cũ: {contract.NgayKetThuc:dd/MM/yyyy}\n" +
            $"• Ngày kết thúc mới: {newEndDate:dd/MM/yyyy}",
            "Xác nhận gia hạn",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            var ok = await _contractService.ExtendContractAsync(contract.Id, newEndDate);
            if (ok)
            {
                KToast.Show(FindForm() ?? new Form(), "Đã gia hạn hợp đồng thành công!", ToastType.Success);
                await LoadContractsAsync();
            }
        }
    }

    private async Task HandleCheckOutAsync()
    {
        var contract = GetSelectedContract();
        if (contract == null || contract.TrangThai != TrangThaiHopDong.HieuLuc)
        {
            MessageBox.Show("Vui lòng chọn một hợp đồng đang có hiệu lực để làm thủ tục trả phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string deductionStr = Microsoft.VisualBasic.Interaction.InputBox(
            $"THỦ TỤC TRẢ PHÒNG & QUYẾT TOÁN CỌC:\n" +
            $"• Sinh viên: {contract.HoTen} (Phòng {contract.MaPhong})\n" +
            $"• Tiền cọc ban đầu: {contract.TienCoc:N0} đ\n\n" +
            $"Nhập tổng số tiền khấu trừ hỏng hóc tài sản & nợ điện nước (nếu có):",
            "Trả phòng KTX",
            "0");

        if (string.IsNullOrWhiteSpace(deductionStr)) return;

        if (!decimal.TryParse(deductionStr.Trim(), out decimal deduction) || deduction < 0)
        {
            MessageBox.Show("Số tiền khấu trừ không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var result = await _contractService.CheckOutContractAsync(new ContractCheckOutDto
        {
            HopDongId = contract.Id,
            TienHongHocTaiSan = deduction,
            TienNoDienNuocKhauTru = 0m
        });

        if (result.Success)
        {
            MessageBox.Show(
                $"{result.Message}\n\n" +
                $"• Tiền cọc ban đầu: {result.TienCocBanDau:N0} VNĐ\n" +
                $"• Khấu trừ bồi thường: {result.TongKhauTru:N0} VNĐ\n" +
                $"• SỐ TIỀN HOÀN TRẢ SINH VIÊN: {result.TienHoanTra:N0} VNĐ",
                "Biên bản thanh lý hợp đồng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadContractsAsync();
        }
        else
        {
            MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task HandlePrintPdfAsync()
    {
        var contract = GetSelectedContract();
        if (contract == null)
        {
            MessageBox.Show("Vui lòng chọn một hợp đồng để in file PDF.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"HopDong_KTX_{contract.MSSV}_{contract.MaPhong}_{DateTime.Now:yyyyMMdd}.pdf",
                Title = "Lưu file Hợp đồng lưu trú PDF"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var pdfBytes = await _contractService.ExportContractPdfAsync(contract.Id);
                await File.WriteAllBytesAsync(sfd.FileName, pdfBytes);
                KToast.Show(FindForm() ?? new Form(), "Đã xuất file Hợp đồng PDF chuẩn pháp lý thành công!", ToastType.Success);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất file PDF:\n{ex.Message}", "Lỗi xuất file", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        KDataGridHelper.ApplyModernStyle(_grid);
        _lblPageInfo.ForeColor = theme.TextPrimary;
    }
}
