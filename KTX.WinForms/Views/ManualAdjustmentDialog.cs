using KTX.Domain.DTOs;
using KTX.Services.Allocation;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class ManualAdjustmentDialog : Form
{
    private readonly int _donId;
    private readonly AllocationResultDto _currentResult;
    private readonly SystemConfigDto _config;
    private readonly IAllocationEngine _engine;

    private DataGridView _grid = null!;
    private KButton _btnConfirm = null!;
    private KButton _btnCancel = null!;
    private Label _lblStudentInfo = null!;
    private List<ManualAdjustmentOptionDto> _options = new();

    public int? SelectedRoomId { get; private set; }

    public ManualAdjustmentDialog(
        int donId,
        AllocationResultDto currentResult,
        SystemConfigDto config,
        IAllocationEngine engine)
    {
        _donId = donId;
        _currentResult = currentResult;
        _config = config;
        _engine = engine;

        InitializeComponents();
        LoadOptions();
    }

    private void InitializeComponents()
    {
        Text = "Điều chỉnh phòng thủ công (Phân tích biến thiên điểm ΔS)";
        Size = new Size(720, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = ThemeManager.Current.Background;

        var target = _currentResult.Assignments.FirstOrDefault(a => a.DonId == _donId);

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(16, 12, 16, 0) };
        _lblStudentInfo = new Label
        {
            Text = $"Sinh viên: {target?.HoTen} (MSSV: {target?.MSSV}) | Khoa: {target?.Khoa} | Giới tính: {(target?.GioiTinh == Domain.Enums.GioiTinh.Nam ? "Nam" : "Nữ")}\n" +
                   $"Phòng hiện tại: {target?.MaPhong} ({target?.ToaTen}) | Điểm phù hợp hiện tại: {target?.DiemPhuHop:F1}",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = ThemeManager.Current.TextPrimary,
            Dock = DockStyle.Fill
        };
        topPanel.Controls.Add(_lblStudentInfo);

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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng đích", DataPropertyName = "MaPhong", FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tòa", DataPropertyName = "ToaTen", FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tầng", DataPropertyName = "Tang", FillWeight = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Còn trống", DataPropertyName = "ChoTrongText", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "S cũ", DataPropertyName = "CurrentScoreText", FillWeight = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "S mới", DataPropertyName = "NewScoreText", FillWeight = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Biến thiên ΔS", DataPropertyName = "DeltaScoreText", FillWeight = 90 });

        // Cell painting or format for DeltaScore coloring
        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 6 && _options.Count > e.RowIndex)
            {
                var delta = _options[e.RowIndex].DeltaScore;
                if (delta > 0)
                {
                    e.CellStyle.ForeColor = Color.ForestGreen;
                    e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
                }
                else if (delta < 0)
                {
                    e.CellStyle.ForeColor = Color.Crimson;
                    e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
                }
            }
        };

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(16, 10, 16, 10) };
        _btnConfirm = new KButton
        {
            Text = "Xác nhận chuyển phòng",
            Style = KButtonStyle.Primary,
            Size = new Size(190, 36),
            Location = new Point(ClientSize.Width - 330, 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        _btnConfirm.Click += (s, e) =>
        {
            if (_grid.CurrentRow?.DataBoundItem is { } selectedItem && _grid.SelectedRows.Count > 0)
            {
                int index = _grid.SelectedRows[0].Index;
                if (index >= 0 && index < _options.Count)
                {
                    SelectedRoomId = _options[index].PhongId;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        };

        _btnCancel = new KButton
        {
            Text = "Hủy bỏ",
            Style = KButtonStyle.Secondary,
            Size = new Size(110, 36),
            Location = new Point(ClientSize.Width - 125, 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        bottomPanel.Controls.Add(_btnConfirm);
        bottomPanel.Controls.Add(_btnCancel);

        Controls.Add(_grid);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);
    }

    private void LoadOptions()
    {
        _options = _engine.EvaluateManualMoves(_donId, _currentResult, _config);

        _grid.AutoGenerateColumns = false;
        _grid.DataSource = _options.Select(o => new
        {
            o.PhongId,
            o.MaPhong,
            o.ToaTen,
            o.Tang,
            ChoTrongText = $"{o.ChoTrong}/{o.SucChua} chỗ",
            CurrentScoreText = $"{o.CurrentScore:F1}",
            NewScoreText = $"{o.NewScore:F1}",
            DeltaScoreText = o.DeltaScore > 0 ? $"+{o.DeltaScore:F1} ▲" : $"{o.DeltaScore:F1} ▼"
        }).ToList();
    }
}
