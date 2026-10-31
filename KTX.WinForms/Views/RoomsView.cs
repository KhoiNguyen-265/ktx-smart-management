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

    public RoomsView(IRoomService roomService)
    {
        _roomService = roomService;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        // Top Filter Bar
        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(0, 0, 0, 12)
        };

        var lblBuilding = new Label
        {
            Text = "Chọn Tòa nhà:",
            Font = UIHelper.GetFont(10f, FontStyle.Bold),
            Location = new Point(0, 16),
            AutoSize = true
        };

        _cboBuilding = new KComboBox
        {
            Location = new Point(110, 10),
            Size = new Size(180, 36)
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
            Font = UIHelper.GetFont(10f, FontStyle.Bold),
            Location = new Point(310, 16),
            AutoSize = true
        };

        _panelFloors = new FlowLayoutPanel
        {
            Location = new Point(360, 8),
            Size = new Size(350, 42),
            FlowDirection = FlowDirection.LeftToRight
        };

        for (int f = 1; f <= 5; f++)
        {
            int floorNum = f;
            var btnFloor = new KButton
            {
                Text = $"Tầng {floorNum}",
                Size = new Size(62, 34),
                Style = (f == 1) ? KButtonStyle.Primary : KButtonStyle.Secondary,
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

        // Status Legend (Chú thích màu sắc)
        var legendPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36
        };

        var lblLegend = new Label
        {
            Text = "Chú thích màu sắc:   ● Trống hoàn toàn (Xanh)     ● Còn chỗ trống (Cam)     ● Đã đầy (Đỏ)     ● Đang bảo trì (Xám)",
            Font = UIHelper.GetFont(9f, FontStyle.Regular),
            Location = new Point(0, 8),
            AutoSize = true
        };
        legendPanel.Controls.Add(lblLegend);

        topBar.Controls.AddRange(new Control[] { lblBuilding, _cboBuilding, lblFloor, _panelFloors });

        // Center: Room Map & Right Detail Card
        var mainContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2,
            SplitterDistance = 800,
            SplitterWidth = 16,
            BorderStyle = BorderStyle.None
        };

        _roomMap = new KRoomMap
        {
            Dock = DockStyle.Fill
        };
        _roomMap.RoomClicked += async (room) => await LoadRoomDetailAsync(room.Id);

        mainContainer.Panel1.Controls.Add(_roomMap);

        // Detail Card on Right
        _detailCard = new KCard
        {
            Dock = DockStyle.Fill,
            Title = "Chi tiết phòng KTX",
            Subtitle = "Click vào một ô phòng để xem danh sách sinh viên"
        };

        _detailPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 60, 0, 0)
        };
        _detailCard.Controls.Add(_detailPanel);

        mainContainer.Panel2.Controls.Add(_detailCard);

        Controls.Add(mainContainer);
        Controls.Add(legendPanel);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);

        Load += async (s, e) => await InitializeDataAsync();
    }

    protected override async void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (_buildings.Count == 0)
        {
            await InitializeDataAsync();
        }
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
    }

    private async Task LoadRoomDetailAsync(int roomId)
    {
        var detail = await _roomService.GetRoomDetailAsync(roomId);
        if (detail == null) return;

        _detailCard.Title = $"Phòng {detail.Summary.MaPhong} ({detail.Summary.TenLoaiPhong})";
        _detailCard.Subtitle = $"{detail.Summary.TenToa} · Tầng {detail.Summary.Tang} · {detail.Summary.SoNguoiDangO}/{detail.Summary.SucChua} người";

        _detailPanel.Controls.Clear();
        int y = 10;

        // Trạng thái & nút bảo trì
        var btnToggleStatus = new KButton
        {
            Text = detail.Summary.TrangThai == TrangThaiPhong.BaoTri ? "Bỏ bảo trì (Kích hoạt)" : "Báo bảo trì phòng",
            Style = detail.Summary.TrangThai == TrangThaiPhong.BaoTri ? KButtonStyle.Success : KButtonStyle.Danger,
            Location = new Point(10, y),
            Size = new Size(240, 34)
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
                    Location = new Point(10, y),
                    Size = new Size(240, 50),
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(6)
                };

                var lblName = new Label
                {
                    Text = $"👤 {r.HoTen} ({r.MSSV})",
                    Font = UIHelper.GetFont(8.5f, FontStyle.Bold),
                    Dock = DockStyle.Top,
                    Height = 18
                };
                var lblSub = new Label
                {
                    Text = $"{r.Khoa} · SĐT: {r.SoDienThoai}",
                    Font = UIHelper.GetFont(8f),
                    Dock = DockStyle.Bottom,
                    Height = 16
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
