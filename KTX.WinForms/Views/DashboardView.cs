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

    private readonly TableLayoutPanel _pnlKpi;
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
    private readonly TableLayoutPanel _pnlActionButtons;
    private bool _isLoaded;
    private DashboardStatsDto? _lastStats;

    public DashboardView(IDashboardService dashboardService, Action<string>? onNavigate = null)
    {
        _dashboardService = dashboardService;
        _onNavigate = onNavigate;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        AutoScroll = true;
        Padding = new Padding(4);

        // 1. KPI Cards Row (Responsive 4-column grid, never clips!)
        _pnlKpi = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 116,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 0, 0, 10)
        };
        for (int c = 0; c < 4; c++)
        {
            _pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }

        _cardRooms = new KStatCard
        {
            Title = "Tổng số phòng KTX",
            Value = "...",
            TrendText = "▲ Đang tải...",
            IsPositiveTrend = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0)
        };

        _cardStudents = new KStatCard
        {
            Title = "Sinh viên đang lưu trú",
            Value = "...",
            TrendText = "● Đang tải...",
            IsPositiveTrend = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0)
        };

        _cardRevenue = new KStatCard
        {
            Title = "Doanh thu tháng này",
            Value = "...",
            TrendText = "▲ Đang tải...",
            IsPositiveTrend = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0)
        };

        _cardAlerts = new KStatCard
        {
            Title = "Cần theo dõi & xử lý",
            Value = "...",
            TrendText = "● Đang tải...",
            IsPositiveTrend = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };

        _pnlKpi.Controls.Add(_cardRooms, 0, 0);
        _pnlKpi.Controls.Add(_cardStudents, 1, 0);
        _pnlKpi.Controls.Add(_cardRevenue, 2, 0);
        _pnlKpi.Controls.Add(_cardAlerts, 3, 0);

        // 2. Charts Row
        _pnlCharts = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 390,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 6, 0, 12)
        };
        _pnlCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        _pnlCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        _cardChartOccupancy = new KCard
        {
            Title = "Tỷ lệ Lấp đầy theo Tòa nhà",
            Subtitle = "So sánh số giường đã có sinh viên ở (Xanh dương) và còn trống (Xanh lá)",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0),
            Padding = new Padding(14, 64, 14, 14)
        };
        _plotOccupancy = new FormsPlot { Dock = DockStyle.Fill };
        _cardChartOccupancy.Controls.Add(_plotOccupancy);

        _cardChartRevenue = new KCard
        {
            Title = "Doanh thu KTX 6 tháng gần nhất",
            Subtitle = "Tổng hợp nguồn thu tiền phòng và điện nước đã ghi nhận (triệu VNĐ)",
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(14, 64, 14, 14)
        };
        _plotRevenue = new FormsPlot { Dock = DockStyle.Fill };
        _cardChartRevenue.Controls.Add(_plotRevenue);

        _pnlCharts.Controls.Add(_cardChartOccupancy, 0, 0);
        _pnlCharts.Controls.Add(_cardChartRevenue, 1, 0);

        // 3. Quick Actions Card (Responsive 6-column grid, never clips!)
        _cardQuickActions = new KCard
        {
            Title = "Lối tắt chức năng nhanh & Thao tác thường dùng",
            Subtitle = "Phím tắt toàn cục: Bấm [Ctrl + K] để mở bảng tìm kiếm thông minh nhanh",
            Dock = DockStyle.Top,
            Height = 136,
            Padding = new Padding(16, 64, 16, 16)
        };

        _pnlActionButtons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Padding = new Padding(0)
        };
        for (int c = 0; c < 6; c++)
        {
            _pnlActionButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6f));
        }

        var btnAlloc = CreateActionButton("Xếp phòng AI", () => _onNavigate?.Invoke("Allocation"));
        var btnMeters = CreateActionButton("Chỉ số Điện Nước", () => _onNavigate?.Invoke("Meters"));
        var btnInvoices = CreateActionButton("Hóa đơn & VietQR", () => _onNavigate?.Invoke("Invoices"));
        var btnReconcile = CreateActionButton("Đối soát Sao kê", () => _onNavigate?.Invoke("Reconcile"));
        var btnMaintenance = CreateActionButton("Báo hỏng / Sự cố", () => _onNavigate?.Invoke("Maintenance"));
        var btnStudents = CreateActionButton("Quản lý Sinh viên", () => _onNavigate?.Invoke("Students"), isLast: true);

        _pnlActionButtons.Controls.Add(btnAlloc, 0, 0);
        _pnlActionButtons.Controls.Add(btnMeters, 1, 0);
        _pnlActionButtons.Controls.Add(btnInvoices, 2, 0);
        _pnlActionButtons.Controls.Add(btnReconcile, 3, 0);
        _pnlActionButtons.Controls.Add(btnMaintenance, 4, 0);
        _pnlActionButtons.Controls.Add(btnStudents, 5, 0);
        _cardQuickActions.Controls.Add(_pnlActionButtons);

        // Layout
        Controls.Add(_cardQuickActions);
        Controls.Add(_pnlCharts);
        Controls.Add(_pnlKpi);

        ApplyTheme(ThemeManager.Current);

        Load += async (s, e) => await EnsureLoadedAsync();
    }

    protected override async void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        await EnsureLoadedAsync();
    }

    private async Task EnsureLoadedAsync()
    {
        if (_isLoaded) return;
        _isLoaded = true;
        await LoadDashboardDataAsync();
    }

    private KButton CreateActionButton(string text, Action onClick, bool isLast = false)
    {
        var btn = new KButton
        {
            Text = text,
            Style = KButtonStyle.Secondary,
            Dock = DockStyle.Fill,
            Font = UIHelper.GetFont(9.2f, FontStyle.Bold),
            Margin = new Padding(0, 4, isLast ? 0 : 10, 4)
        };
        btn.Click += (s, e) => onClick();
        return btn;
    }

    public async Task LoadDashboardDataAsync()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            _lastStats = stats;

            // 1. KPI
            _cardRooms.Value = $"{stats.TotalRooms} phòng";
            _cardRooms.TrendText = $"▲ Lấp đầy: {stats.OccupancyRate}% ({stats.OccupiedBeds}/{stats.TotalBeds} chỗ)";
            _cardRooms.IsPositiveTrend = true;

            _cardStudents.Value = $"{stats.ActiveContracts} SV";
            _cardStudents.TrendText = "● 100% hợp đồng hiệu lực";
            _cardStudents.IsPositiveTrend = true;

            _cardRevenue.Value = $"{stats.MonthlyRevenue:N0} đ";
            _cardRevenue.TrendText = $"▲ Đã thu trong tháng {DateTime.Now.Month}/{DateTime.Now.Year}";
            _cardRevenue.IsPositiveTrend = true;

            int alertTotal = stats.ExpiringContracts30Days + stats.PendingMaintenance + stats.UnpaidInvoices;
            _cardAlerts.Value = $"{alertTotal} mục";
            _cardAlerts.TrendText = $"● {stats.ExpiringContracts30Days} sắp hết · {stats.PendingMaintenance} sự cố · {stats.UnpaidInvoices} nợ";
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
        ApplyPlotTheme(plot, currentTheme);

        var occupiedBars = new List<ScottPlot.Bar>();
        var availableBars = new List<ScottPlot.Bar>();
        var tickList = new List<ScottPlot.Tick>();

        for (int i = 0; i < buildings.Count; i++)
        {
            var b = buildings[i];
            double x = i * 2.4;

            occupiedBars.Add(new ScottPlot.Bar
            {
                Position = x,
                Value = b.OccupiedBeds,
                FillColor = ScottPlot.Color.FromHex("#2563EB"),
                LineColor = ScottPlot.Color.FromHex("#1D4ED8"),
                LineWidth = 1,
                Label = b.OccupiedBeds.ToString()
            });

            availableBars.Add(new ScottPlot.Bar
            {
                Position = x + 0.85,
                Value = b.AvailableBeds,
                FillColor = ScottPlot.Color.FromHex("#10B981"),
                LineColor = ScottPlot.Color.FromHex("#059669"),
                LineWidth = 1,
                Label = b.AvailableBeds.ToString()
            });

            tickList.Add(new ScottPlot.Tick(x + 0.425, b.BuildingName));
        }

        var bpOcc = plot.Add.Bars(occupiedBars);
        bpOcc.LegendText = "Đã ở";
        bpOcc.ValueLabelStyle.ForeColor = ScottPlot.Color.FromHex(currentTheme.Mode == ThemeMode.Dark ? "#E5E7EB" : "#1F2937");
        bpOcc.ValueLabelStyle.FontSize = 11;

        var bpAvail = plot.Add.Bars(availableBars);
        bpAvail.LegendText = "Còn trống";
        bpAvail.ValueLabelStyle.ForeColor = ScottPlot.Color.FromHex(currentTheme.Mode == ThemeMode.Dark ? "#E5E7EB" : "#1F2937");
        bpAvail.ValueLabelStyle.FontSize = 11;

        plot.ShowLegend(ScottPlot.Alignment.UpperRight);
        plot.Axes.Left.Label.Text = "Số giường";
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickList.ToArray());
        plot.Axes.Margins(bottom: 0, top: 0.48);
        plot.Axes.AutoScale();
        _plotOccupancy.Refresh();
    }

    private void RenderRevenueChart(List<MonthlyRevenueDto> history)
    {
        var plot = _plotRevenue.Plot;
        plot.Clear();

        if (history.Count == 0) return;

        var currentTheme = ThemeManager.Current;
        ApplyPlotTheme(plot, currentTheme);

        var bars = new List<ScottPlot.Bar>();
        var tickList = new List<ScottPlot.Tick>();
        for (int i = 0; i < history.Count; i++)
        {
            var h = history[i];
            double x = i * 1.4;
            double million = Math.Round((double)(h.TotalRevenue / 1_000_000m), 1);

            bars.Add(new ScottPlot.Bar
            {
                Position = x,
                Value = million,
                FillColor = ScottPlot.Color.FromHex("#7C3AED"),
                LineColor = ScottPlot.Color.FromHex("#6D28D9"),
                LineWidth = 1,
                Label = $"{million:0.#}"
            });

            tickList.Add(new ScottPlot.Tick(x, h.MonthName));
        }

        var bp = plot.Add.Bars(bars);
        bp.ValueLabelStyle.ForeColor = ScottPlot.Color.FromHex(currentTheme.Mode == ThemeMode.Dark ? "#E5E7EB" : "#1F2937");
        bp.ValueLabelStyle.FontSize = 11;

        plot.Axes.Left.Label.Text = "Triệu VNĐ";
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickList.ToArray());
        plot.Axes.Margins(bottom: 0, top: 0.22);
        plot.Axes.AutoScale();
        _plotRevenue.Refresh();
    }

    private static void ApplyPlotTheme(ScottPlot.Plot plot, Theme theme)
    {
        bool isDark = theme.Mode == ThemeMode.Dark;
        plot.FigureBackground.Color = ScottPlot.Color.FromHex(isDark ? "#181B21" : "#FFFFFF");
        plot.DataBackground.Color = ScottPlot.Color.FromHex(isDark ? "#0F1115" : "#F8FAFC");
        var axisColor = ScottPlot.Color.FromHex(isDark ? "#9CA3AF" : "#4B5563");
        plot.Axes.Color(axisColor);
        plot.Grid.MajorLineColor = ScottPlot.Color.FromHex(isDark ? "#2A2F38" : "#E5E7EB");
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        _pnlKpi.BackColor = theme.Background;
        _pnlCharts.BackColor = theme.Background;
        _pnlActionButtons.BackColor = theme.SurfaceCard;

        if (_lastStats != null)
        {
            RenderOccupancyChart(_lastStats.Buildings);
            RenderRevenueChart(_lastStats.RevenueHistory);
        }
        else if (_plotOccupancy != null && _plotRevenue != null)
        {
            ApplyPlotTheme(_plotOccupancy.Plot, theme);
            _plotOccupancy.Refresh();
            ApplyPlotTheme(_plotRevenue.Plot, theme);
            _plotRevenue.Refresh();
        }
    }
}
