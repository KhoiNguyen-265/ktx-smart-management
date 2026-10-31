using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KTX.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BacGias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiDichVu = table.Column<int>(type: "int", nullable: false),
                    Bac = table.Column<int>(type: "int", nullable: false),
                    TuMuc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DenMuc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApDungTu = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacGias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CauHinhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Khoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiaTri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DienUuTiens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiemCong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DienUuTiens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DotDangKys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayMo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DangMo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DotDangKys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GiaoDichNganHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayGD = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NoiDungGoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaGiaoDich = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDuSau = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    HashSaoKe = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TrangThaiKhop = table.Column<int>(type: "int", nullable: false),
                    FileNguon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayImport = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDichNganHangs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoaiPhongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SucChua = table.Column<int>(type: "int", nullable: false),
                    GiaThang = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CoDieuHoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiPhongs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NhomDangKys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MSSVTruongNhom = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhomDangKys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TinhThanhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KhoangCachKm = table.Column<int>(type: "int", nullable: false),
                    NhomKhoangCach = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinhThanhs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToaNhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    SoTang = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToaNhas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaiTros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhienXepPhongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DotId = table.Column<int>(type: "int", nullable: false),
                    DotDangKyId = table.Column<int>(type: "int", nullable: false),
                    ThuatToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThongKeJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaChot = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienXepPhongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhienXepPhongs_DotDangKys_DotDangKyId",
                        column: x => x.DotDangKyId,
                        principalTable: "DotDangKys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SinhViens",
                columns: table => new
                {
                    MSSV = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    CCCD = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Khoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KhoaHoc = table.Column<int>(type: "int", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TinhThanhId = table.Column<int>(type: "int", nullable: false),
                    DienUuTienId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinhViens", x => x.MSSV);
                    table.ForeignKey(
                        name: "FK_SinhViens_DienUuTiens_DienUuTienId",
                        column: x => x.DienUuTienId,
                        principalTable: "DienUuTiens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SinhViens_TinhThanhs_TinhThanhId",
                        column: x => x.TinhThanhId,
                        principalTable: "TinhThanhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TinhThanhCus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenCu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TinhThanhId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinhThanhCus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinhThanhCus_TinhThanhs_TinhThanhId",
                        column: x => x.TinhThanhId,
                        principalTable: "TinhThanhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Phongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToaId = table.Column<int>(type: "int", nullable: false),
                    Tang = table.Column<int>(type: "int", nullable: false),
                    LoaiPhongId = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phongs_LoaiPhongs_LoaiPhongId",
                        column: x => x.LoaiPhongId,
                        principalTable: "LoaiPhongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Phongs_ToaNhas_ToaId",
                        column: x => x.ToaId,
                        principalTable: "ToaNhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false),
                    HoatDong = table.Column<bool>(type: "bit", nullable: false),
                    SoLanSai = table.Column<int>(type: "int", nullable: false),
                    KhoaDen = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguoiDungs_VaiTros_MaVaiTro",
                        column: x => x.MaVaiTro,
                        principalTable: "VaiTros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonDangKys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MSSV = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    DotId = table.Column<int>(type: "int", nullable: false),
                    NhomId = table.Column<int>(type: "int", nullable: true),
                    LoaiPhongMongMuonId = table.Column<int>(type: "int", nullable: true),
                    BatBuocLoaiPhong = table.Column<bool>(type: "bit", nullable: false),
                    ToaMongMuonId = table.Column<int>(type: "int", nullable: true),
                    TangMongMuon = table.Column<int>(type: "int", nullable: true),
                    DiemUuTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ThoiGianNop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonDangKys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonDangKys_DotDangKys_DotId",
                        column: x => x.DotId,
                        principalTable: "DotDangKys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonDangKys_LoaiPhongs_LoaiPhongMongMuonId",
                        column: x => x.LoaiPhongMongMuonId,
                        principalTable: "LoaiPhongs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DonDangKys_NhomDangKys_NhomId",
                        column: x => x.NhomId,
                        principalTable: "NhomDangKys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DonDangKys_SinhViens_MSSV",
                        column: x => x.MSSV,
                        principalTable: "SinhViens",
                        principalColumn: "MSSV",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonDangKys_ToaNhas_ToaMongMuonId",
                        column: x => x.ToaMongMuonId,
                        principalTable: "ToaNhas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChiSoDienNuocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhongId = table.Column<int>(type: "int", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    DienCu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DienMoi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CoThayDongHoDien = table.Column<bool>(type: "bit", nullable: false),
                    DienChotDongHoCu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DienDauDongHoMoi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SanLuongDien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NuocCu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NuocMoi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CoThayDongHoNuoc = table.Column<bool>(type: "bit", nullable: false),
                    NuocChotDongHoCu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NuocDauDongHoMoi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SanLuongNuoc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiSoDienNuocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiSoDienNuocs_Phongs_PhongId",
                        column: x => x.PhongId,
                        principalTable: "Phongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonDienNuocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhongId = table.Column<int>(type: "int", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    SoNguoiTinhDinhMuc = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DaThu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonDienNuocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoaDonDienNuocs_Phongs_PhongId",
                        column: x => x.PhongId,
                        principalTable: "Phongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HopDongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MSSV = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    PhongId = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GiaThang = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MienGiamPhanTram = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TienCoc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopDongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HopDongs_Phongs_PhongId",
                        column: x => x.PhongId,
                        principalTable: "Phongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HopDongs_SinhViens_MSSV",
                        column: x => x.MSSV,
                        principalTable: "SinhViens",
                        principalColumn: "MSSV",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiSanPhongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhongId = table.Column<int>(type: "int", nullable: false),
                    TenTaiSan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TinhTrang = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiSanPhongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiSanPhongs_Phongs_PhongId",
                        column: x => x.PhongId,
                        principalTable: "Phongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauSuaChuas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhongId = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayHoanThanh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChiPhi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauSuaChuas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuCauSuaChuas_Phongs_PhongId",
                        column: x => x.PhongId,
                        principalTable: "Phongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhatKys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<int>(type: "int", nullable: true),
                    HanhDong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BangLienQuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhoaChinhLienQuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhatKys_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietXepPhongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhienId = table.Column<int>(type: "int", nullable: false),
                    DonId = table.Column<int>(type: "int", nullable: false),
                    PhongId = table.Column<int>(type: "int", nullable: false),
                    DiemPhuHop = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ChinhTay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietXepPhongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietXepPhongs_DonDangKys_DonId",
                        column: x => x.DonId,
                        principalTable: "DonDangKys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietXepPhongs_PhienXepPhongs_PhienId",
                        column: x => x.PhienId,
                        principalTable: "PhienXepPhongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietXepPhongs_Phongs_PhongId",
                        column: x => x.PhongId,
                        principalTable: "Phongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoaDonId = table.Column<int>(type: "int", nullable: false),
                    HoaDonDienNuocId = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDons_HoaDonDienNuocs_HoaDonDienNuocId",
                        column: x => x.HoaDonDienNuocId,
                        principalTable: "HoaDonDienNuocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhoanThuTienPhongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HopDongId = table.Column<int>(type: "int", nullable: false),
                    TuNgay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DenNgay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTienGoc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MienGiam = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PhaiThu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DaThu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoanThuTienPhongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhoanThuTienPhongs_HopDongs_HopDongId",
                        column: x => x.HopDongId,
                        principalTable: "HopDongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuChuyenPhongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HopDongId = table.Column<int>(type: "int", nullable: false),
                    TuPhongId = table.Column<int>(type: "int", nullable: false),
                    DenPhongId = table.Column<int>(type: "int", nullable: false),
                    NgayChuyen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuChuyenPhongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuChuyenPhongs_HopDongs_HopDongId",
                        column: x => x.HopDongId,
                        principalTable: "HopDongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichSuChuyenPhongs_Phongs_DenPhongId",
                        column: x => x.DenPhongId,
                        principalTable: "Phongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuChuyenPhongs_Phongs_TuPhongId",
                        column: x => x.TuPhongId,
                        principalTable: "Phongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoaDonDienNuocId = table.Column<int>(type: "int", nullable: true),
                    KhoanThuTienPhongId = table.Column<int>(type: "int", nullable: true),
                    GiaoDichNganHangId = table.Column<int>(type: "int", nullable: true),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HinhThuc = table.Column<int>(type: "int", nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiThuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhToans_GiaoDichNganHangs_GiaoDichNganHangId",
                        column: x => x.GiaoDichNganHangId,
                        principalTable: "GiaoDichNganHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ThanhToans_HoaDonDienNuocs_HoaDonDienNuocId",
                        column: x => x.HoaDonDienNuocId,
                        principalTable: "HoaDonDienNuocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhToans_KhoanThuTienPhongs_KhoanThuTienPhongId",
                        column: x => x.KhoanThuTienPhongId,
                        principalTable: "KhoanThuTienPhongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhToans_NguoiDungs_NguoiThuId",
                        column: x => x.NguoiThuId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhs_Khoa",
                table: "CauHinhs",
                column: "Khoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiSoDienNuocs_PhongId_Thang_Nam",
                table: "ChiSoDienNuocs",
                columns: new[] { "PhongId", "Thang", "Nam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDons_HoaDonDienNuocId",
                table: "ChiTietHoaDons",
                column: "HoaDonDienNuocId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXepPhongs_DonId",
                table: "ChiTietXepPhongs",
                column: "DonId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXepPhongs_PhienId",
                table: "ChiTietXepPhongs",
                column: "PhienId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXepPhongs_PhongId",
                table: "ChiTietXepPhongs",
                column: "PhongId");

            migrationBuilder.CreateIndex(
                name: "IX_DonDangKys_DotId_MSSV",
                table: "DonDangKys",
                columns: new[] { "DotId", "MSSV" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonDangKys_LoaiPhongMongMuonId",
                table: "DonDangKys",
                column: "LoaiPhongMongMuonId");

            migrationBuilder.CreateIndex(
                name: "IX_DonDangKys_MSSV",
                table: "DonDangKys",
                column: "MSSV");

            migrationBuilder.CreateIndex(
                name: "IX_DonDangKys_NhomId",
                table: "DonDangKys",
                column: "NhomId");

            migrationBuilder.CreateIndex(
                name: "IX_DonDangKys_ToaMongMuonId",
                table: "DonDangKys",
                column: "ToaMongMuonId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichNganHangs_HashSaoKe",
                table: "GiaoDichNganHangs",
                column: "HashSaoKe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichNganHangs_MaGiaoDich",
                table: "GiaoDichNganHangs",
                column: "MaGiaoDich",
                unique: true,
                filter: "[MaGiaoDich] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonDienNuocs_MaThanhToan",
                table: "HoaDonDienNuocs",
                column: "MaThanhToan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonDienNuocs_PhongId",
                table: "HoaDonDienNuocs",
                column: "PhongId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongs_MSSV",
                table: "HopDongs",
                column: "MSSV",
                unique: true,
                filter: "[TrangThai] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongs_PhongId",
                table: "HopDongs",
                column: "PhongId");

            migrationBuilder.CreateIndex(
                name: "IX_KhoanThuTienPhongs_HopDongId",
                table: "KhoanThuTienPhongs",
                column: "HopDongId");

            migrationBuilder.CreateIndex(
                name: "IX_KhoanThuTienPhongs_MaThanhToan",
                table: "KhoanThuTienPhongs",
                column: "MaThanhToan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuChuyenPhongs_DenPhongId",
                table: "LichSuChuyenPhongs",
                column: "DenPhongId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuChuyenPhongs_HopDongId",
                table: "LichSuChuyenPhongs",
                column: "HopDongId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuChuyenPhongs_TuPhongId",
                table: "LichSuChuyenPhongs",
                column: "TuPhongId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_MaVaiTro",
                table: "NguoiDungs",
                column: "MaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_TenDangNhap",
                table: "NguoiDungs",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhatKys_NguoiDungId",
                table: "NhatKys",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_NhomDangKys_MaNhom",
                table: "NhomDangKys",
                column: "MaNhom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhienXepPhongs_DotDangKyId",
                table: "PhienXepPhongs",
                column: "DotDangKyId");

            migrationBuilder.CreateIndex(
                name: "IX_Phongs_LoaiPhongId",
                table: "Phongs",
                column: "LoaiPhongId");

            migrationBuilder.CreateIndex(
                name: "IX_Phongs_ToaId_MaPhong",
                table: "Phongs",
                columns: new[] { "ToaId", "MaPhong" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SinhViens_CCCD",
                table: "SinhViens",
                column: "CCCD",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SinhViens_DienUuTienId",
                table: "SinhViens",
                column: "DienUuTienId");

            migrationBuilder.CreateIndex(
                name: "IX_SinhViens_TinhThanhId",
                table: "SinhViens",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiSanPhongs_PhongId",
                table: "TaiSanPhongs",
                column: "PhongId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_GiaoDichNganHangId",
                table: "ThanhToans",
                column: "GiaoDichNganHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_HoaDonDienNuocId",
                table: "ThanhToans",
                column: "HoaDonDienNuocId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_KhoanThuTienPhongId",
                table: "ThanhToans",
                column: "KhoanThuTienPhongId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_NguoiThuId",
                table: "ThanhToans",
                column: "NguoiThuId");

            migrationBuilder.CreateIndex(
                name: "IX_TinhThanhCus_TinhThanhId",
                table: "TinhThanhCus",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauSuaChuas_PhongId",
                table: "YeuCauSuaChuas",
                column: "PhongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BacGias");

            migrationBuilder.DropTable(
                name: "CauHinhs");

            migrationBuilder.DropTable(
                name: "ChiSoDienNuocs");

            migrationBuilder.DropTable(
                name: "ChiTietHoaDons");

            migrationBuilder.DropTable(
                name: "ChiTietXepPhongs");

            migrationBuilder.DropTable(
                name: "LichSuChuyenPhongs");

            migrationBuilder.DropTable(
                name: "NhatKys");

            migrationBuilder.DropTable(
                name: "TaiSanPhongs");

            migrationBuilder.DropTable(
                name: "ThanhToans");

            migrationBuilder.DropTable(
                name: "TinhThanhCus");

            migrationBuilder.DropTable(
                name: "YeuCauSuaChuas");

            migrationBuilder.DropTable(
                name: "DonDangKys");

            migrationBuilder.DropTable(
                name: "PhienXepPhongs");

            migrationBuilder.DropTable(
                name: "GiaoDichNganHangs");

            migrationBuilder.DropTable(
                name: "HoaDonDienNuocs");

            migrationBuilder.DropTable(
                name: "KhoanThuTienPhongs");

            migrationBuilder.DropTable(
                name: "NguoiDungs");

            migrationBuilder.DropTable(
                name: "NhomDangKys");

            migrationBuilder.DropTable(
                name: "DotDangKys");

            migrationBuilder.DropTable(
                name: "HopDongs");

            migrationBuilder.DropTable(
                name: "VaiTros");

            migrationBuilder.DropTable(
                name: "Phongs");

            migrationBuilder.DropTable(
                name: "SinhViens");

            migrationBuilder.DropTable(
                name: "LoaiPhongs");

            migrationBuilder.DropTable(
                name: "ToaNhas");

            migrationBuilder.DropTable(
                name: "DienUuTiens");

            migrationBuilder.DropTable(
                name: "TinhThanhs");
        }
    }
}
