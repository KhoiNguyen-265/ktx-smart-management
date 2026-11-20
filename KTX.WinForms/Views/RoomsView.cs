using System.Drawing;
using System.Windows.Forms;
using KTX.Domain.DTOs;
using KTX.Domain.Enums;
using KTX.Services.Facility;
using KTX.UI.Controls;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class RoomsView : UserControl, IThemable
{
    private readonly IRoomService _roomService;
    private readonly KComboBox _cboBuilding;
    private readonly FlowLayoutPanel _panelFloors;
    private readonly KRoomMap _roomMap;
    private readonly KCard _detailCard;
    private readonly Panel _detailPanel;

    private List<BuildingDto> _buildings = new();
    private int _selectedBuildingId = 1;
    private int _selectedFloor = 1;
    private bool _isInitialized;

    public RoomsView(IRoomService roomService)
    {
        _roomService = roomService;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        // Top Filter Bar (FlowLayoutPanel ensures zero overlap at any DPI!)
        var topBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 56,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 8)
        };

        var lblBuilding = new Label
        {
            Text = "Chọn Tòa nhà:",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 9, 8, 0)
        };

        _cboBuilding = new KComboBox
        {
            Size = new Size(165, 36),
            Margin = new Padding(0, 0, 20, 0)
        };
        _cboBuilding.SelectedIndexChanged += async (s, e) =>
        {
            if (_cboBuilding.SelectedItem is BuildingDto b)
            {
                _selectedBuildingId = b.Id;
                await LoadFloorRoomsAsync();
            }
        };

        var lblFloor = new Label
        {
            Text = "Tầng:",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 9, 8, 0)
        };

        _panelFloors = new FlowLayoutPanel
        {
            AutoSize = true,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };

        for (int f = 1; f <= 5; f++)
        {
            int floorNum = f;
            var btnFloor = new KButton
            {
                Text = $"Tầng {floorNum}",
                Size = new Size(84, 36),
                Font = UIHelper.GetFont(9f, FontStyle.Bold),
                Style = (f == 1) ? KButtonStyle.Primary : KButtonStyle.Secondary,
                Margin = new Padding(0, 0, 8, 0),
                Tag = floorNum
            };
            btnFloor.Click += async (s, e) =>
            {
                _selectedFloor = floorNum;
                foreach (Control c in _panelFloors.Controls)
                {
                    if (c is KButton btn)
                    {
                        btn.Style = (((int?)btn.Tag ?? 0) == _selectedFloor) ? KButtonStyle.Primary : KButtonStyle.Secondary;
                    }
                }
                await LoadFloorRoomsAsync();
            };
            _panelFloors.Controls.Add(btnFloor);
        }

        // Status Legend (Chú thích màu sắc chuẩn màu trực quan)
        var legendPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };

        legendPanel.Controls.Add(new Label { Text = "Chú thích:", Font = UIHelper.GetFont(9f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 2, 12, 0) });
        legendPanel.Controls.Add(CreateLegendChip("● Trống hoàn toàn", ThemeManager.Current.Success));
        legendPanel.Controls.Add(CreateLegendChip("● Còn chỗ trống", ThemeManager.Current.Warning));
        legendPanel.Controls.Add(CreateLegendChip("● Đã đầy", ThemeManager.Current.Danger));
        legendPanel.Controls.Add(CreateLegendChip("● Đang bảo trì", ThemeManager.Current.TextMuted));

        topBar.Controls.AddRange(new Control[] { lblBuilding, _cboBuilding, lblFloor, _panelFloors });

        // Center: Room Map (inside AutoScroll container) & Right Detail Card (Fixed 340px right panel - never throws SplitterDistance!)
        var mainContainer = new Panel
        {
            Dock = DockStyle.Fill
        };

        var mapScrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 0, 12, 0)
        };

        _roomMap = new KRoomMap
        {
            Dock = DockStyle.Top
        };
        _roomMap.RoomClicked += async (room) => await LoadRoomDetailAsync(room.Id);
        mapScrollPanel.Controls.Add(_roomMap);

        var rightHolder = new Panel
        {
            Dock = DockStyle.Right,
            Width = 340,
            Padding = new Padding(10, 0, 0, 0)
        };

        // Detail Card on Right
        _detailCard = new KCard
        {
            Dock = DockStyle.Fill,
            Title = "Chi tiết phòng KTX",
            Subtitle = "Click vào một ô phòng để xem sinh viên & thiết bị",
            Padding = new Padding(14, 66, 14, 14)
        };

        _detailPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0)
        };
        _detailCard.Controls.Add(_detailPanel);
        rightHolder.Controls.Add(_detailCard);

        mainContainer.Controls.Add(mapScrollPanel);
        mainContainer.Controls.Add(rightHolder);

        Controls.Add(mainContainer);
        Controls.Add(legendPanel);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);

        Load += async (s, e) => await EnsureInitializedAsync();
    }

    private static Label CreateLegendChip(string text, Color color) => new()
    {
        Text = text,
        ForeColor = color,
        Font = UIHelper.GetFont(9f, FontStyle.Bold),
        AutoSize = true,
        Margin = new Padding(0, 2, 18, 0)
    };

    protected override async void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        await EnsureInitializedAsync();
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        await InitializeDataAsync();
    }

    private async Task InitializeDataAsync()
    {
        _buildings = await _roomService.GetBuildingsAsync();
        _cboBuilding.DisplayMember = "Ten";
        _cboBuilding.ValueMember = "Id";
        _cboBuilding.DataSource = _buildings;

        if (_buildings.Count > 0)
        {
            _selectedBuildingId = _buildings[0].Id;
            await LoadFloorRoomsAsync();
        }
    }

    private async Task LoadFloorRoomsAsync()
    {
        var rooms = await _roomService.GetRoomsByBuildingAndFloorAsync(_selectedBuildingId, _selectedFloor);
        _roomMap.Rooms = rooms;
        _detailPanel.Controls.Clear();
        if (rooms.Count > 0)
        {
            await LoadRoomDetailAsync(rooms[0].Id);
        }
    }

    private async Task LoadRoomDetailAsync(int roomId)
    {
        var detail = await _roomService.GetRoomDetailAsync(roomId);
        if (detail == null) return;

        _detailCard.Title = $"Phòng {detail.Summary.MaPhong} · {detail.Summary.TenToa} Tầng {detail.Summary.Tang}";
        _detailCard.Subtitle = $"{detail.Summary.TenLoaiPhong} · {detail.Summary.SoNguoiDangO}/{detail.Summary.SucChua} người";

        _detailPanel.Controls.Clear();
        int y = 10;

        // Trạng thái & nút bảo trì
        var btnToggleStatus = new KButton
        {
            Text = detail.Summary.TrangThai == TrangThaiPhong.BaoTri ? "Bỏ bảo trì (Kích hoạt)" : "Báo bảo trì phòng",
            Style = detail.Summary.TrangThai == TrangThaiPhong.BaoTri ? KButtonStyle.Success : KButtonStyle.Danger,
            Location = new Point(4, y),
            Size = new Size(276, 34)
        };
        btnToggleStatus.Click += async (s, e) =>
        {
            bool isMaintenance = (detail.Summary.TrangThai != TrangThaiPhong.BaoTri);
            await _roomService.UpdateRoomStatusAsync(roomId, isMaintenance);
            await LoadFloorRoomsAsync();
            await LoadRoomDetailAsync(roomId);
        };
        _detailPanel.Controls.Add(btnToggleStatus);
        y += 46;

        // Danh sách sinh viên
        var lblResidents = new Label
        {
            Text = $"Danh sách sinh viên đang ở ({detail.Residents.Count}):",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(10, y),
            AutoSize = true
        };
        _detailPanel.Controls.Add(lblResidents);
        y += 26;

        if (detail.Residents.Count == 0)
        {
            var lblEmpty = new Label
            {
                Text = "Phòng hiện đang trống.",
                Font = UIHelper.GetFont(9f, FontStyle.Italic),
                Location = new Point(10, y),
                AutoSize = true
            };
            _detailPanel.Controls.Add(lblEmpty);
            y += 24;
        }
        else
        {
            foreach (var r in detail.Residents)
            {
                var residentBox = new Panel
                {
                    Location = new Point(4, y),
                    Size = new Size(276, 52),
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(6)
                };

                var lblName = new Label
                {
                    Text = $"● {r.HoTen} ({r.MSSV})",
                    Font = UIHelper.GetFont(8.5f, FontStyle.Bold),
                    Dock = DockStyle.Top,
                    Height = 19,
                    UseMnemonic = false
                };
                var lblSub = new Label
                {
                    Text = $"{r.Khoa} · SĐT: {r.SoDienThoai}",
                    Font = UIHelper.GetFont(8f),
                    Dock = DockStyle.Bottom,
                    Height = 17,
                    UseMnemonic = false
                };

                residentBox.Controls.Add(lblName);
                residentBox.Controls.Add(lblSub);
                _detailPanel.Controls.Add(residentBox);
                y += 56;
            }
        }

        // Trang thiết bị
        y += 10;
        var lblAssets = new Label
        {
            Text = "Trang thiết bị bàn giao:",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(10, y),
            AutoSize = true
        };
        _detailPanel.Controls.Add(lblAssets);
        y += 24;

        foreach (var a in detail.Assets)
        {
            var lblAsset = new Label
            {
                Text = $"• {a.TenTaiSan} (SL: {a.SoLuong}) - {a.TinhTrang}",
                Font = UIHelper.GetFont(8.5f),
                Location = new Point(10, y),
                AutoSize = true
            };
            _detailPanel.Controls.Add(lblAsset);
            y += 20;
        }

        // Chỉ số điện nước gần nhất
        if (detail.LatestMeterReading != null)
        {
            y += 10;
            var lblMeter = new Label
            {
                Text = $"Điện nước T{detail.LatestMeterReading.Thang}/{detail.LatestMeterReading.Nam}: Điện {detail.LatestMeterReading.SanLuongDien} kWh, Nước {detail.LatestMeterReading.SanLuongNuoc} m³",
                Font = UIHelper.GetFont(8.5f, FontStyle.Italic),
                Location = new Point(10, y),
                AutoSize = true
            };
            _detailPanel.Controls.Add(lblMeter);
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        foreach (Control c in Controls)
        {
            if (c is Label lbl)
            {
                lbl.ForeColor = theme.TextPrimary;
            }
        }
    }
}
