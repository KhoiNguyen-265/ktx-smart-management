using System.Drawing;
using System.Windows.Forms;
using KTX.Domain.DTOs;
using KTX.Services.Students;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class StudentsView : UserControl, IThemable
{
    private readonly IStudentService _studentService;
    private readonly KTextBox _txtSearch;
    private readonly DataGridView _grid;
    private readonly Label _lblPageInfo;
    private readonly KButton _btnPrev;
    private readonly KButton _btnNext;
    private readonly KButton _btnExportExcel;
    private readonly KButton _btnImportExcel;
    private readonly KButton _btnTemplateExcel;
    private readonly KButton _btnScanCccd;

    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalPages = 1;

    public StudentsView(IStudentService studentService)
    {
        _studentService = studentService;

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
            Placeholder = "🔍 Tìm theo MSSV, Họ tên, CCCD hoặc Khoa...",
            Location = new Point(0, 8),
            Size = new Size(340, 40)
        };
        _txtSearch.InnerTextBox.TextChanged += async (s, e) =>
        {
            _currentPage = 1;
            await LoadStudentsAsync();
        };

        _btnExportExcel = new KButton
        {
            Text = "📤 Xuất Excel",
            Style = KButtonStyle.Secondary,
            Location = new Point(360, 8),
            Size = new Size(125, 40)
        };
        _btnExportExcel.Click += async (s, e) => await HandleExportExcelAsync();

        _btnImportExcel = new KButton
        {
            Text = "📥 Nhập Excel",
            Style = KButtonStyle.Primary,
            Location = new Point(495, 8),
            Size = new Size(125, 40)
        };
        _btnImportExcel.Click += async (s, e) => await HandleImportExcelAsync();

        _btnTemplateExcel = new KButton
        {
            Text = "📋 Tải mẫu Excel",
            Style = KButtonStyle.Secondary,
            Location = new Point(630, 8),
            Size = new Size(130, 40)
        };
        _btnTemplateExcel.Click += async (s, e) => await HandleDownloadTemplateAsync();

        _btnScanCccd = new KButton
        {
            Text = "📷 Quét QR CCCD",
            Style = KButtonStyle.Primary,
            Location = new Point(770, 8),
            Size = new Size(150, 40)
        };
        _btnScanCccd.Click += (s, e) => HandleScanCccd();

        topBar.Controls.Add(_txtSearch);
        topBar.Controls.Add(_btnExportExcel);
        topBar.Controls.Add(_btnImportExcel);
        topBar.Controls.Add(_btnTemplateExcel);
        topBar.Controls.Add(_btnScanCccd);

        // DataGridView
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true
        };
        KDataGridHelper.ApplyModernStyle(_grid);

        // Define Columns
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MSSV", DataPropertyName = "MSSV", FillWeight = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", DataPropertyName = "HoTen", FillWeight = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giới tính", DataPropertyName = "GioiTinhText", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số CCCD", DataPropertyName = "CCCD", FillWeight = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khoa", DataPropertyName = "Khoa", FillWeight = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khóa", DataPropertyName = "KhoaHoc", FillWeight = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Quê quán", DataPropertyName = "TenTinhThanh", FillWeight = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Diện ưu tiên", DataPropertyName = "TenDienUuTien", FillWeight = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng ở", DataPropertyName = "PhongHienTai", FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiLuuTru", FillWeight = 100 });

        // Bottom Pagination Bar
        var bottomBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            Padding = new Padding(0, 10, 0, 0)
        };

        _btnPrev = new KButton
        {
            Text = "◀ Trang trước",
            Style = KButtonStyle.Secondary,
            Size = new Size(110, 34),
            Location = new Point(0, 8)
        };
        _btnPrev.Click += async (s, e) =>
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadStudentsAsync();
            }
        };

        _lblPageInfo = new Label
        {
            Text = "Trang 1 / 1 (Tổng 0 sinh viên)",
            Font = UIHelper.GetFont(9.5f, FontStyle.Regular),
            Location = new Point(125, 14),
            AutoSize = true
        };

        _btnNext = new KButton
        {
            Text = "Trang sau ▶",
            Style = KButtonStyle.Secondary,
            Size = new Size(110, 34),
            Location = new Point(360, 8)
        };
        _btnNext.Click += async (s, e) =>
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadStudentsAsync();
            }
        };

        bottomBar.Controls.AddRange(new Control[] { _btnPrev, _lblPageInfo, _btnNext });

        Controls.Add(_grid);
        Controls.Add(bottomBar);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);

        Load += async (s, e) => await LoadStudentsAsync();
    }

    private async Task LoadStudentsAsync()
    {
        var result = await _studentService.GetStudentsPagedAsync(
            search: _txtSearch.Text,
            pageIndex: _currentPage,
            pageSize: _pageSize
        );

        _totalPages = Math.Max(1, result.TotalPages);
        _lblPageInfo.Text = $"Trang {_currentPage} / {_totalPages} (Tổng {result.TotalCount:N0} sinh viên)";
        _btnPrev.Enabled = _currentPage > 1;
        _btnNext.Enabled = _currentPage < _totalPages;

        _grid.AutoGenerateColumns = false;
        _grid.DataSource = result.Items;
    }

    private async Task HandleExportExcelAsync()
    {
        try
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"DanhSachSinhVien_KTX_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var excelBytes = await _studentService.ExportStudentsToExcelAsync();
                await File.WriteAllBytesAsync(sfd.FileName, excelBytes);
                KToast.Show(FindForm() ?? this.ParentForm ?? new Form(), "Xuất danh sách sinh viên ra Excel thành công!", ToastType.Success);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất file Excel:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleImportExcelAsync()
    {
        try
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                Title = "Chọn file danh sách sinh viên cần nhập"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                await using var stream = File.OpenRead(ofd.FileName);
                var result = await _studentService.ImportStudentsFromExcelAsync(stream);

                if (result.FailedCount == 0 && result.SuccessCount > 0)
                {
                    KToast.Show(FindForm() ?? new Form(), $"Nhập thành công toàn bộ {result.SuccessCount} sinh viên!", ToastType.Success);
                    _currentPage = 1;
                    await LoadStudentsAsync();
                }
                else
                {
                    var msg = $"Kết quả nhập Excel:\n- Tổng số dòng: {result.TotalRows}\n- Thành công: {result.SuccessCount}\n- Thất bại: {result.FailedCount}\n\n";
                    if (result.Errors.Count > 0)
                    {
                        msg += "Chi tiết lỗi:\n" + string.Join("\n", result.Errors.Take(10).Select(err => $"• Dòng {err.RowIndex} [{err.Identifier}]: {err.ErrorMessage}"));
                        if (result.Errors.Count > 10)
                        {
                            msg += $"\n... và còn {result.Errors.Count - 10} dòng lỗi khác.";
                        }
                    }

                    MessageBox.Show(msg, "Kết quả nhập dữ liệu Excel", MessageBoxButtons.OK,
                        result.FailedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                    if (result.SuccessCount > 0)
                    {
                        _currentPage = 1;
                        await LoadStudentsAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi đọc file Excel:\n{ex.Message}", "Lỗi nhập file", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleDownloadTemplateAsync()
    {
        try
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "Mau_Nhap_SinhVien_KTX.xlsx",
                Title = "Lưu file mẫu Excel nhập sinh viên"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var templateBytes = await _studentService.GenerateStudentTemplateAsync();
                await File.WriteAllBytesAsync(sfd.FileName, templateBytes);
                KToast.Show(FindForm() ?? new Form(), "Đã tải file mẫu Excel (kèm dropdown 34 tỉnh) thành công!", ToastType.Success);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải file mẫu:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HandleScanCccd()
    {
        using var dlg = new Forms.CccdScannerDialog();
        if (dlg.ShowDialog() == DialogResult.OK && dlg.ScannedResult != null)
        {
            var scanned = dlg.ScannedResult;
            var confirm = MessageBox.Show(
                $"Đã trích xuất thông tin Căn cước công dân:\n\n" +
                $"• Họ tên: {scanned.HoTen}\n" +
                $"• Số CCCD: {scanned.CCCD}\n" +
                $"• Giới tính: {(scanned.GioiTinh == Domain.Enums.GioiTinh.Nu ? "Nữ" : "Nam")}\n" +
                $"• Ngày sinh: {(scanned.NgaySinh.HasValue ? scanned.NgaySinh.Value.ToString("dd/MM/yyyy") : "---")}\n" +
                $"• Quê quán: {scanned.TenTinhThanhNhanDien ?? scanned.DiaChi}\n\n" +
                $"Bạn có muốn tự động tìm kiếm hồ sơ của sinh viên này trong danh sách không?",
                "Kết quả quét CCCD thành công",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (confirm == DialogResult.Yes)
            {
                _txtSearch.Text = scanned.CCCD;
            }
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        KDataGridHelper.ApplyModernStyle(_grid);
        _lblPageInfo.ForeColor = theme.TextPrimary;
    }
}
