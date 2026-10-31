IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [BacGias] (
        [Id] int NOT NULL IDENTITY,
        [LoaiDichVu] int NOT NULL,
        [Bac] int NOT NULL,
        [TuMuc] decimal(18,2) NOT NULL,
        [DenMuc] decimal(18,2) NULL,
        [DonGia] decimal(18,2) NOT NULL,
        [ApDungTu] datetime2 NOT NULL,
        CONSTRAINT [PK_BacGias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [CauHinhs] (
        [Id] int NOT NULL IDENTITY,
        [Khoa] nvarchar(100) NOT NULL,
        [GiaTri] nvarchar(max) NOT NULL,
        [MoTa] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_CauHinhs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [DienUuTiens] (
        [Id] int NOT NULL IDENTITY,
        [Ten] nvarchar(max) NOT NULL,
        [DiemCong] int NOT NULL,
        CONSTRAINT [PK_DienUuTiens] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [DotDangKys] (
        [Id] int NOT NULL IDENTITY,
        [Ten] nvarchar(max) NOT NULL,
        [NgayMo] datetime2 NOT NULL,
        [NgayDong] datetime2 NOT NULL,
        [DangMo] bit NOT NULL,
        CONSTRAINT [PK_DotDangKys] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [GiaoDichNganHangs] (
        [Id] int NOT NULL IDENTITY,
        [NgayGD] datetime2 NOT NULL,
        [SoTien] decimal(18,2) NOT NULL,
        [NoiDungGoc] nvarchar(max) NOT NULL,
        [MaGiaoDich] nvarchar(100) NULL,
        [SoDuSau] decimal(18,2) NULL,
        [HashSaoKe] nvarchar(64) NOT NULL,
        [TrangThaiKhop] int NOT NULL,
        [FileNguon] nvarchar(max) NOT NULL,
        [NgayImport] datetime2 NOT NULL,
        CONSTRAINT [PK_GiaoDichNganHangs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [LoaiPhongs] (
        [Id] int NOT NULL IDENTITY,
        [Ten] nvarchar(100) NOT NULL,
        [SucChua] int NOT NULL,
        [GiaThang] decimal(18,2) NOT NULL,
        [CoDieuHoa] bit NOT NULL,
        CONSTRAINT [PK_LoaiPhongs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [NhomDangKys] (
        [Id] int NOT NULL IDENTITY,
        [MaNhom] nvarchar(50) NOT NULL,
        [MSSVTruongNhom] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_NhomDangKys] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [TinhThanhs] (
        [Id] int NOT NULL IDENTITY,
        [Ten] nvarchar(100) NOT NULL,
        [KhoangCachKm] int NOT NULL,
        [NhomKhoangCach] int NOT NULL,
        CONSTRAINT [PK_TinhThanhs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [ToaNhas] (
        [Id] int NOT NULL IDENTITY,
        [Ten] nvarchar(100) NOT NULL,
        [GioiTinh] int NOT NULL,
        [SoTang] int NOT NULL,
        CONSTRAINT [PK_ToaNhas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [VaiTros] (
        [Id] int NOT NULL IDENTITY,
        [TenVaiTro] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_VaiTros] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [PhienXepPhongs] (
        [Id] int NOT NULL IDENTITY,
        [DotId] int NOT NULL,
        [DotDangKyId] int NOT NULL,
        [ThuatToan] nvarchar(max) NOT NULL,
        [ThoiGian] datetime2 NOT NULL,
        [ThongKeJson] nvarchar(max) NOT NULL,
        [DaChot] bit NOT NULL,
        CONSTRAINT [PK_PhienXepPhongs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PhienXepPhongs_DotDangKys_DotDangKyId] FOREIGN KEY ([DotDangKyId]) REFERENCES [DotDangKys] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [SinhViens] (
        [MSSV] nvarchar(20) NOT NULL,
        [HoTen] nvarchar(150) NOT NULL,
        [NgaySinh] datetime2 NOT NULL,
        [GioiTinh] int NOT NULL,
        [CCCD] nvarchar(20) NOT NULL,
        [Khoa] nvarchar(100) NOT NULL,
        [KhoaHoc] int NOT NULL,
        [SoDienThoai] nvarchar(20) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [TinhThanhId] int NOT NULL,
        [DienUuTienId] int NULL,
        CONSTRAINT [PK_SinhViens] PRIMARY KEY ([MSSV]),
        CONSTRAINT [FK_SinhViens_DienUuTiens_DienUuTienId] FOREIGN KEY ([DienUuTienId]) REFERENCES [DienUuTiens] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_SinhViens_TinhThanhs_TinhThanhId] FOREIGN KEY ([TinhThanhId]) REFERENCES [TinhThanhs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [TinhThanhCus] (
        [Id] int NOT NULL IDENTITY,
        [TenCu] nvarchar(100) NOT NULL,
        [TinhThanhId] int NOT NULL,
        CONSTRAINT [PK_TinhThanhCus] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TinhThanhCus_TinhThanhs_TinhThanhId] FOREIGN KEY ([TinhThanhId]) REFERENCES [TinhThanhs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [Phongs] (
        [Id] int NOT NULL IDENTITY,
        [MaPhong] nvarchar(50) NOT NULL,
        [ToaId] int NOT NULL,
        [Tang] int NOT NULL,
        [LoaiPhongId] int NOT NULL,
        [TrangThai] int NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Phongs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Phongs_LoaiPhongs_LoaiPhongId] FOREIGN KEY ([LoaiPhongId]) REFERENCES [LoaiPhongs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Phongs_ToaNhas_ToaId] FOREIGN KEY ([ToaId]) REFERENCES [ToaNhas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [NguoiDungs] (
        [Id] int NOT NULL IDENTITY,
        [TenDangNhap] nvarchar(50) NOT NULL,
        [MatKhauHash] nvarchar(max) NOT NULL,
        [HoTen] nvarchar(max) NOT NULL,
        [MaVaiTro] int NOT NULL,
        [HoatDong] bit NOT NULL,
        [SoLanSai] int NOT NULL,
        [KhoaDen] datetime2 NULL,
        CONSTRAINT [PK_NguoiDungs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NguoiDungs_VaiTros_MaVaiTro] FOREIGN KEY ([MaVaiTro]) REFERENCES [VaiTros] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [DonDangKys] (
        [Id] int NOT NULL IDENTITY,
        [MSSV] nvarchar(20) NOT NULL,
        [DotId] int NOT NULL,
        [NhomId] int NULL,
        [LoaiPhongMongMuonId] int NULL,
        [BatBuocLoaiPhong] bit NOT NULL,
        [ToaMongMuonId] int NULL,
        [TangMongMuon] int NULL,
        [DiemUuTien] decimal(18,2) NOT NULL,
        [ThoiGianNop] datetime2 NOT NULL,
        [TrangThai] int NOT NULL,
        CONSTRAINT [PK_DonDangKys] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DonDangKys_DotDangKys_DotId] FOREIGN KEY ([DotId]) REFERENCES [DotDangKys] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DonDangKys_LoaiPhongs_LoaiPhongMongMuonId] FOREIGN KEY ([LoaiPhongMongMuonId]) REFERENCES [LoaiPhongs] ([Id]),
        CONSTRAINT [FK_DonDangKys_NhomDangKys_NhomId] FOREIGN KEY ([NhomId]) REFERENCES [NhomDangKys] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_DonDangKys_SinhViens_MSSV] FOREIGN KEY ([MSSV]) REFERENCES [SinhViens] ([MSSV]) ON DELETE CASCADE,
        CONSTRAINT [FK_DonDangKys_ToaNhas_ToaMongMuonId] FOREIGN KEY ([ToaMongMuonId]) REFERENCES [ToaNhas] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [ChiSoDienNuocs] (
        [Id] int NOT NULL IDENTITY,
        [PhongId] int NOT NULL,
        [Thang] int NOT NULL,
        [Nam] int NOT NULL,
        [DienCu] decimal(18,2) NOT NULL,
        [DienMoi] decimal(18,2) NOT NULL,
        [CoThayDongHoDien] bit NOT NULL,
        [DienChotDongHoCu] decimal(18,2) NULL,
        [DienDauDongHoMoi] decimal(18,2) NULL,
        [SanLuongDien] decimal(18,2) NOT NULL,
        [NuocCu] decimal(18,2) NOT NULL,
        [NuocMoi] decimal(18,2) NOT NULL,
        [CoThayDongHoNuoc] bit NOT NULL,
        [NuocChotDongHoCu] decimal(18,2) NULL,
        [NuocDauDongHoMoi] decimal(18,2) NULL,
        [SanLuongNuoc] decimal(18,2) NOT NULL,
        [GhiChu] nvarchar(max) NULL,
        CONSTRAINT [PK_ChiSoDienNuocs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChiSoDienNuocs_Phongs_PhongId] FOREIGN KEY ([PhongId]) REFERENCES [Phongs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [HoaDonDienNuocs] (
        [Id] int NOT NULL IDENTITY,
        [MaThanhToan] nvarchar(50) NOT NULL,
        [PhongId] int NOT NULL,
        [Thang] int NOT NULL,
        [Nam] int NOT NULL,
        [SoNguoiTinhDinhMuc] int NOT NULL,
        [TongTien] decimal(18,2) NOT NULL,
        [DaThu] decimal(18,2) NOT NULL,
        [TrangThai] int NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_HoaDonDienNuocs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HoaDonDienNuocs_Phongs_PhongId] FOREIGN KEY ([PhongId]) REFERENCES [Phongs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [HopDongs] (
        [Id] int NOT NULL IDENTITY,
        [MSSV] nvarchar(20) NOT NULL,
        [PhongId] int NOT NULL,
        [NgayBatDau] datetime2 NOT NULL,
        [NgayKetThuc] datetime2 NOT NULL,
        [GiaThang] decimal(18,2) NOT NULL,
        [MienGiamPhanTram] decimal(18,2) NOT NULL,
        [TienCoc] decimal(18,2) NOT NULL,
        [TrangThai] int NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_HopDongs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HopDongs_Phongs_PhongId] FOREIGN KEY ([PhongId]) REFERENCES [Phongs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HopDongs_SinhViens_MSSV] FOREIGN KEY ([MSSV]) REFERENCES [SinhViens] ([MSSV]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [TaiSanPhongs] (
        [Id] int NOT NULL IDENTITY,
        [PhongId] int NOT NULL,
        [TenTaiSan] nvarchar(max) NOT NULL,
        [SoLuong] int NOT NULL,
        [TinhTrang] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TaiSanPhongs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TaiSanPhongs_Phongs_PhongId] FOREIGN KEY ([PhongId]) REFERENCES [Phongs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [YeuCauSuaChuas] (
        [Id] int NOT NULL IDENTITY,
        [PhongId] int NOT NULL,
        [MoTa] nvarchar(max) NOT NULL,
        [TrangThai] nvarchar(max) NOT NULL,
        [NgayTao] datetime2 NOT NULL,
        [NgayHoanThanh] datetime2 NULL,
        [ChiPhi] decimal(18,2) NOT NULL,
        [GhiChu] nvarchar(max) NULL,
        CONSTRAINT [PK_YeuCauSuaChuas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_YeuCauSuaChuas_Phongs_PhongId] FOREIGN KEY ([PhongId]) REFERENCES [Phongs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [NhatKys] (
        [Id] bigint NOT NULL IDENTITY,
        [NguoiDungId] int NULL,
        [HanhDong] nvarchar(max) NOT NULL,
        [ChiTiet] nvarchar(max) NOT NULL,
        [BangLienQuan] nvarchar(max) NULL,
        [KhoaChinhLienQuan] nvarchar(max) NULL,
        [ThoiGian] datetime2 NOT NULL,
        CONSTRAINT [PK_NhatKys] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NhatKys_NguoiDungs_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDungs] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [ChiTietXepPhongs] (
        [Id] int NOT NULL IDENTITY,
        [PhienId] int NOT NULL,
        [DonId] int NOT NULL,
        [PhongId] int NOT NULL,
        [DiemPhuHop] decimal(18,2) NOT NULL,
        [ChinhTay] bit NOT NULL,
        CONSTRAINT [PK_ChiTietXepPhongs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChiTietXepPhongs_DonDangKys_DonId] FOREIGN KEY ([DonId]) REFERENCES [DonDangKys] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChiTietXepPhongs_PhienXepPhongs_PhienId] FOREIGN KEY ([PhienId]) REFERENCES [PhienXepPhongs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChiTietXepPhongs_Phongs_PhongId] FOREIGN KEY ([PhongId]) REFERENCES [Phongs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [ChiTietHoaDons] (
        [Id] int NOT NULL IDENTITY,
        [HoaDonId] int NOT NULL,
        [HoaDonDienNuocId] int NOT NULL,
        [NoiDung] nvarchar(max) NOT NULL,
        [SoLuong] decimal(18,2) NOT NULL,
        [DonGia] decimal(18,2) NOT NULL,
        [ThanhTien] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ChiTietHoaDons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChiTietHoaDons_HoaDonDienNuocs_HoaDonDienNuocId] FOREIGN KEY ([HoaDonDienNuocId]) REFERENCES [HoaDonDienNuocs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [KhoanThuTienPhongs] (
        [Id] int NOT NULL IDENTITY,
        [MaThanhToan] nvarchar(50) NOT NULL,
        [HopDongId] int NOT NULL,
        [TuNgay] datetime2 NOT NULL,
        [DenNgay] datetime2 NOT NULL,
        [SoTienGoc] decimal(18,2) NOT NULL,
        [MienGiam] decimal(18,2) NOT NULL,
        [PhaiThu] decimal(18,2) NOT NULL,
        [DaThu] decimal(18,2) NOT NULL,
        [TrangThai] int NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_KhoanThuTienPhongs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KhoanThuTienPhongs_HopDongs_HopDongId] FOREIGN KEY ([HopDongId]) REFERENCES [HopDongs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [LichSuChuyenPhongs] (
        [Id] int NOT NULL IDENTITY,
        [HopDongId] int NOT NULL,
        [TuPhongId] int NOT NULL,
        [DenPhongId] int NOT NULL,
        [NgayChuyen] datetime2 NOT NULL,
        [LyDo] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_LichSuChuyenPhongs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LichSuChuyenPhongs_HopDongs_HopDongId] FOREIGN KEY ([HopDongId]) REFERENCES [HopDongs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LichSuChuyenPhongs_Phongs_DenPhongId] FOREIGN KEY ([DenPhongId]) REFERENCES [Phongs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LichSuChuyenPhongs_Phongs_TuPhongId] FOREIGN KEY ([TuPhongId]) REFERENCES [Phongs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE TABLE [ThanhToans] (
        [Id] int NOT NULL IDENTITY,
        [HoaDonDienNuocId] int NULL,
        [KhoanThuTienPhongId] int NULL,
        [GiaoDichNganHangId] int NULL,
        [SoTien] decimal(18,2) NOT NULL,
        [HinhThuc] int NOT NULL,
        [ThoiGian] datetime2 NOT NULL,
        [NguoiThuId] int NOT NULL,
        CONSTRAINT [PK_ThanhToans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ThanhToans_GiaoDichNganHangs_GiaoDichNganHangId] FOREIGN KEY ([GiaoDichNganHangId]) REFERENCES [GiaoDichNganHangs] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_ThanhToans_HoaDonDienNuocs_HoaDonDienNuocId] FOREIGN KEY ([HoaDonDienNuocId]) REFERENCES [HoaDonDienNuocs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThanhToans_KhoanThuTienPhongs_KhoanThuTienPhongId] FOREIGN KEY ([KhoanThuTienPhongId]) REFERENCES [KhoanThuTienPhongs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThanhToans_NguoiDungs_NguoiThuId] FOREIGN KEY ([NguoiThuId]) REFERENCES [NguoiDungs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CauHinhs_Khoa] ON [CauHinhs] ([Khoa]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ChiSoDienNuocs_PhongId_Thang_Nam] ON [ChiSoDienNuocs] ([PhongId], [Thang], [Nam]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChiTietHoaDons_HoaDonDienNuocId] ON [ChiTietHoaDons] ([HoaDonDienNuocId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChiTietXepPhongs_DonId] ON [ChiTietXepPhongs] ([DonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChiTietXepPhongs_PhienId] ON [ChiTietXepPhongs] ([PhienId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChiTietXepPhongs_PhongId] ON [ChiTietXepPhongs] ([PhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DonDangKys_DotId_MSSV] ON [DonDangKys] ([DotId], [MSSV]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonDangKys_LoaiPhongMongMuonId] ON [DonDangKys] ([LoaiPhongMongMuonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonDangKys_MSSV] ON [DonDangKys] ([MSSV]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonDangKys_NhomId] ON [DonDangKys] ([NhomId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonDangKys_ToaMongMuonId] ON [DonDangKys] ([ToaMongMuonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GiaoDichNganHangs_HashSaoKe] ON [GiaoDichNganHangs] ([HashSaoKe]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_GiaoDichNganHangs_MaGiaoDich] ON [GiaoDichNganHangs] ([MaGiaoDich]) WHERE [MaGiaoDich] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HoaDonDienNuocs_MaThanhToan] ON [HoaDonDienNuocs] ([MaThanhToan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HoaDonDienNuocs_PhongId] ON [HoaDonDienNuocs] ([PhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_HopDongs_MSSV] ON [HopDongs] ([MSSV]) WHERE [TrangThai] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HopDongs_PhongId] ON [HopDongs] ([PhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KhoanThuTienPhongs_HopDongId] ON [KhoanThuTienPhongs] ([HopDongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KhoanThuTienPhongs_MaThanhToan] ON [KhoanThuTienPhongs] ([MaThanhToan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LichSuChuyenPhongs_DenPhongId] ON [LichSuChuyenPhongs] ([DenPhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LichSuChuyenPhongs_HopDongId] ON [LichSuChuyenPhongs] ([HopDongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LichSuChuyenPhongs_TuPhongId] ON [LichSuChuyenPhongs] ([TuPhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NguoiDungs_MaVaiTro] ON [NguoiDungs] ([MaVaiTro]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NguoiDungs_TenDangNhap] ON [NguoiDungs] ([TenDangNhap]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NhatKys_NguoiDungId] ON [NhatKys] ([NguoiDungId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NhomDangKys_MaNhom] ON [NhomDangKys] ([MaNhom]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PhienXepPhongs_DotDangKyId] ON [PhienXepPhongs] ([DotDangKyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Phongs_LoaiPhongId] ON [Phongs] ([LoaiPhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Phongs_ToaId_MaPhong] ON [Phongs] ([ToaId], [MaPhong]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SinhViens_CCCD] ON [SinhViens] ([CCCD]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SinhViens_DienUuTienId] ON [SinhViens] ([DienUuTienId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SinhViens_TinhThanhId] ON [SinhViens] ([TinhThanhId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TaiSanPhongs_PhongId] ON [TaiSanPhongs] ([PhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ThanhToans_GiaoDichNganHangId] ON [ThanhToans] ([GiaoDichNganHangId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ThanhToans_HoaDonDienNuocId] ON [ThanhToans] ([HoaDonDienNuocId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ThanhToans_KhoanThuTienPhongId] ON [ThanhToans] ([KhoanThuTienPhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ThanhToans_NguoiThuId] ON [ThanhToans] ([NguoiThuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TinhThanhCus_TinhThanhId] ON [TinhThanhCus] ([TinhThanhId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_YeuCauSuaChuas_PhongId] ON [YeuCauSuaChuas] ([PhongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031014127_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261031014127_InitialCreate', N'10.0.14');
END;

COMMIT;
GO

