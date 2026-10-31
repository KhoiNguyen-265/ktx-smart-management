using System.Drawing;
using System.Windows.Forms;
using KTX.Domain.DTOs;
using KTX.Services.Dashboard;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;
using ScottPlot.WinForms;

namespace KTX.WinForms.Views;

public class DashboardView : UserControl, IThemable
{
    private readonly IDashboardService _dashboardService;
    private readonly Action<string>? _onNavigate;

    private readonly FlowLayoutPanel _pnlKpi;
    private readonly KStatCard _cardRooms;
    private readonly KStatCard _cardStudents;
    private readonly KStatCard _cardRevenue;
    private readonly KStatCard _cardAlerts;

    private readonly TableLayoutPanel _pnlCharts;
    private readonly KCard _cardChartOccupancy;
    private readonly KCard _cardChartRevenue;
    private readonly FormsPlot _plotOccupancy;
    private readonly FormsPlot _plotRevenue;

    private readonly KCard _cardQuickActions;
    private readonly FlowLayoutPanel _pnlActionButtons;

    public DashboardView(IDashboardService dashboardService, Action<string>? onNavigate = null)
    {
        _dashboardService = dashboardService;
        _onNavigate = onNavigate;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        AutoScroll = true;
        Padding = new Padding(8);

        // 1. KPI Cards Row
        _pnlKpi = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 125,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(0, 0, 0, 10)
        };

        _cardRooms = new KStatCard
        {
            Title = "Tổng số phòng KTX",
            Value = "120 phòng",
            TrendText = "▲ Đang tải...",
            IsPositiveTrend = true,
            Width = 260
        };

        _cardStudents = new KStatCard
        {
            Title = "Sinh viên đang lưu trú",
            Value = "600 SV",
            TrendText = "● Đang tải...",
            IsPositiveTrend = true,
            Width = 260
        };

        _cardRevenue = new KStatCard
        {
            Title = "Doanh thu tháng này",
            Value = "0 đ",
            TrendText = "▲ Đang tải...",
            IsPositiveTrend = true,
            Width = 260
        };

        _cardAlerts = new KStatCard
        {
            Title = "Cần theo dõi & xử lý",
            Value = "0 mục",
            TrendText = "● Đang tải...",
            IsPositiveTrend = false,
            Width = 260
        };

        _pnlKpi.Controls.AddRange(new Control[] { _cardRooms, _cardStudents, _cardRevenue, _cardAlerts });

        // 2. Charts Row
        _pnlCharts = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 360,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 10, 0, 10)
        };
        _pnlCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        _pnlCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        _cardChartOccupancy = new KCard
        {
            Title = "Tỷ lệ Lấp đầy theo Tòa nhà",
            Subtitle = "So sánh số chỗ đã có sinh viên ở vs số chỗ còn trống",
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 40, 12, 12)
        };
        _plotOccupancy = new FormsPlot { Dock = DockStyle.Fill };
        _cardChartOccupancy.Controls.Add(_plotOccupancy);

        _cardChartRevenue = new KCard
        {
            Title = "Doanh thu KTX 6 tháng gần nhất",
            Subtitle = "Tổng hợp nguồn thu tiền phòng và điện nước đã ghi nhận (triệu VNĐ)",
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 40, 12, 12)
        };
        _plotRevenue = new FormsPlot { Dock = DockStyle.Fill };
        _cardChartRevenue.Controls.Add(_plotRevenue);

        _pnlCharts.Controls.Add(_cardChartOccupancy, 0, 0);
        _pnlCharts.Controls.Add(_cardChartRevenue, 1, 0);

        // 3. Quick Actions Card
        _cardQuickActions = new KCard
        {
            Title = "Lối tắt chức năng nhanh & Thao tác thường dùng",
            Subtitle = "Phím tắt toàn cục: Bấm [Ctrl + K] để mở bảng tìm kiếm thông minh nhanh",
            Dock = DockStyle.Top,
            Height = 120,
            Padding = new Padding(16, 40, 16, 16)
        };

        _pnlActionButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(4)
        };

        var btnAlloc = CreateActionButton("Xếp phòng AI", () => _onNavigate?.Invoke("Allocation"));
        var btnMeters = CreateActionButton("Chỉ số Điện Nước", () => _onNavigate?.Invoke("Meters"));
        var btnInvoices = CreateActionButton("Hóa đơn & VietQR", () => _onNavigate?.Invoke("Invoices"));
        var btnReconcile = CreateActionButton("Đối soát Sao kê", () => _onNavigate?.Invoke("Reconcile"));
        var btnMaintenance = CreateActionButton("Báo hỏng / Sự cố", () => _onNavigate?.Invoke("Maintenance"));
        var btnStudents = CreateActionButton("Quản lý Sinh viên", () => _onNavigate?.Invoke("Students"));

        _pnlActionButtons.Controls.AddRange(new Control[] { btnAlloc, btnMeters, btnInvoices, btnReconcile, btnMaintenance, btnStudents });
        _cardQuickActions.Controls.Add(_pnlActionButtons);

        // Layout
        Controls.Add(_cardQuickActions);
        Controls.Add(_pnlCharts);
        Controls.Add(_pnlKpi);

        ApplyTheme(ThemeManager.Current);

        Load += async (s, e) => await LoadDashboardDataAsync();
    }

    protected override async void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        await LoadDashboardDataAsync();
    }

    private KButton CreateActionButton(string text, Action onClick)
    {
        var btn = new KButton
        {
            Text = text,
            Style = KButtonStyle.Secondary,
            Size = new Size(160, 38),
            Margin = new Padding(0, 0, 12, 0)
        };
        btn.Click += (s, e) => onClick();
        return btn;
    }

    public async Task LoadDashboardDataAsync()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();

            // 1. KPI
            _cardRooms.Value = $"{stats.TotalRooms} phòng";
            _cardRooms.TrendText = $"▲ Lấp đầy: {stats.OccupancyRate}% ({stats.OccupiedBeds}/{stats.TotalBeds} chỗ)";
            _cardRooms.IsPositiveTrend = true;

            _cardStudents.Value = $"{stats.ActiveContracts} SV";
            _cardStudents.TrendText = "● 100% hợp đồng hiệu lực";
            _cardStudents.IsPositiveTrend = true;

            _cardRevenue.Value = $"{stats.MonthlyRevenue:N0} đ";
            _cardRevenue.TrendText = $"▲ Đã ghi nhận trong tháng {DateTime.Now.Month}";
            _cardRevenue.IsPositiveTrend = true;

            int alertTotal = stats.ExpiringContracts30Days + stats.PendingMaintenance + stats.UnpaidInvoices;
            _cardAlerts.Value = $"{alertTotal} mục";
            _cardAlerts.TrendText = $"● {stats.ExpiringContracts30Days} HĐ hết hạn | {stats.PendingMaintenance} sự cố | {stats.UnpaidInvoices} chưa đóng";
            _cardAlerts.IsPositiveTrend = alertTotal == 0;

            // 2. Chart 1: Building Occupancy
            RenderOccupancyChart(stats.Buildings);

            // 3. Chart 2: Revenue History
            RenderRevenueChart(stats.RevenueHistory);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể nạp dữ liệu Dashboard: {ex.Message}", "Lỗi Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RenderOccupancyChart(List<BuildingOccupancyDto> buildings)
    {
        var plot = _plotOccupancy.Plot;
        plot.Clear();

        if (buildings.Count == 0) return;

        var currentTheme = ThemeManager.Current;
        plot.FigureBackground.Color = ScottPlot.Color.FromHex(currentTheme.Mode == ThemeMode.Dark ? "#1E293B" : "#FFFFFF");
        plot.DataBackground.Color = ScottPlot.Color.FromHex(currentTheme.Mode == ThemeMode.Dark ? "#0F172A" : "#F8FAFC");

        var tickList = new List<ScottPlot.Tick>();
        for (int i = 0; i < buildings.Count; i++)
        {
            var b = buildings[i];
            double x = i * 2.0;

            // Bar đã ở (Blue)
            var barOccupied = new ScottPlot.Bar
            {
                Position = x,
                Value = b.OccupiedBeds,
                FillColor = ScottPlot.Color.FromHex("#2563EB"),
                LineColor = ScottPlot.Color.FromHex("#1D4ED8"),
                LineWidth = 1
            };
            plot.Add.Bar(barOccupied);

            // Bar còn trống (Teal / Gray)
            var barAvailable = new ScottPlot.Bar
            {
                Position = x + 0.8,
                Value = b.AvailableBeds,
                FillColor = ScottPlot.Color.FromHex("#10B981"),
                LineColor = ScottPlot.Color.FromHex("#059669"),
                LineWidth = 1
            };
            plot.Add.Bar(barAvailable);

            tickList.Add(new ScottPlot.Tick(x + 0.4, b.BuildingName));
        }

        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickList.ToArray());
        plot.Axes.AutoScale();
        _plotOccupancy.Refresh();
    }

    private void RenderRevenueChart(List<MonthlyRevenueDto> history)
    {
        var plot = _plotRevenue.Plot;
        plot.Clear();

        if (history.Count == 0) return;

        var currentTheme = ThemeManager.Current;
        plot.FigureBackground.Color = ScottPlot.Color.FromHex(currentTheme.Mode == ThemeMode.Dark ? "#1E293B" : "#FFFFFF");
        plot.DataBackground.Color = ScottPlot.Color.FromHex(currentTheme.Mode == ThemeMode.Dark ? "#0F172A" : "#F8FAFC");

        var tickList = new List<ScottPlot.Tick>();
        for (int i = 0; i < history.Count; i++)
        {
            var h = history[i];
            double x = i * 1.5;

            var bar = new ScottPlot.Bar
            {
                Position = x,
                Value = (double)(h.TotalRevenue / 1_000_000m),
                FillColor = ScottPlot.Color.FromHex("#8B5CF6"),
                LineColor = ScottPlot.Color.FromHex("#7C3AED"),
                LineWidth = 1
            };
            plot.Add.Bar(bar);

            tickList.Add(new ScottPlot.Tick(x, h.MonthName));
        }

        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickList.ToArray());
        plot.Axes.AutoScale();
        _plotRevenue.Refresh();
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        _pnlKpi.BackColor = theme.Background;
        _pnlCharts.BackColor = theme.Background;
        _pnlActionButtons.BackColor = theme.Surface;

        if (_plotOccupancy != null && _plotRevenue != null)
        {
            _plotOccupancy.Plot.FigureBackground.Color = ScottPlot.Color.FromHex(theme.Mode == ThemeMode.Dark ? "#1E293B" : "#FFFFFF");
            _plotOccupancy.Plot.DataBackground.Color = ScottPlot.Color.FromHex(theme.Mode == ThemeMode.Dark ? "#0F172A" : "#F8FAFC");
            _plotOccupancy.Refresh();

            _plotRevenue.Plot.FigureBackground.Color = ScottPlot.Color.FromHex(theme.Mode == ThemeMode.Dark ? "#1E293B" : "#FFFFFF");
            _plotRevenue.Plot.DataBackground.Color = ScottPlot.Color.FromHex(theme.Mode == ThemeMode.Dark ? "#0F172A" : "#F8FAFC");
            _plotRevenue.Refresh();
        }
    }
}
