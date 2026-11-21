using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KTX.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesToVietnameseSingular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiSoDienNuocs_Phongs_PhongId",
                table: "ChiSoDienNuocs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietHoaDons_HoaDonDienNuocs_HoaDonDienNuocId",
                table: "ChiTietHoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietXepPhongs_DonDangKys_DonId",
                table: "ChiTietXepPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietXepPhongs_PhienXepPhongs_PhienId",
                table: "ChiTietXepPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietXepPhongs_Phongs_PhongId",
                table: "ChiTietXepPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKys_DotDangKys_DotId",
                table: "DonDangKys");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKys_LoaiPhongs_LoaiPhongMongMuonId",
                table: "DonDangKys");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKys_NhomDangKys_NhomId",
                table: "DonDangKys");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKys_SinhViens_MSSV",
                table: "DonDangKys");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKys_ToaNhas_ToaMongMuonId",
                table: "DonDangKys");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonDienNuocs_Phongs_PhongId",
                table: "HoaDonDienNuocs");

            migrationBuilder.DropForeignKey(
                name: "FK_HopDongs_Phongs_PhongId",
                table: "HopDongs");

            migrationBuilder.DropForeignKey(
                name: "FK_HopDongs_SinhViens_MSSV",
                table: "HopDongs");

            migrationBuilder.DropForeignKey(
                name: "FK_KhoanThuTienPhongs_HopDongs_HopDongId",
                table: "KhoanThuTienPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuChuyenPhongs_HopDongs_HopDongId",
                table: "LichSuChuyenPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuChuyenPhongs_Phongs_DenPhongId",
                table: "LichSuChuyenPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuChuyenPhongs_Phongs_TuPhongId",
                table: "LichSuChuyenPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDungs_VaiTros_MaVaiTro",
                table: "NguoiDungs");

            migrationBuilder.DropForeignKey(
                name: "FK_NhatKys_NguoiDungs_NguoiDungId",
                table: "NhatKys");

            migrationBuilder.DropForeignKey(
                name: "FK_PhienXepPhongs_DotDangKys_DotDangKyId",
                table: "PhienXepPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_Phongs_LoaiPhongs_LoaiPhongId",
                table: "Phongs");

            migrationBuilder.DropForeignKey(
                name: "FK_Phongs_ToaNhas_ToaId",
                table: "Phongs");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhViens_DienUuTiens_DienUuTienId",
                table: "SinhViens");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhViens_TinhThanhs_TinhThanhId",
                table: "SinhViens");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiSanPhongs_Phongs_PhongId",
                table: "TaiSanPhongs");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_GiaoDichNganHangs_GiaoDichNganHangId",
                table: "ThanhToans");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_HoaDonDienNuocs_HoaDonDienNuocId",
                table: "ThanhToans");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_KhoanThuTienPhongs_KhoanThuTienPhongId",
                table: "ThanhToans");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_NguoiDungs_NguoiThuId",
                table: "ThanhToans");

            migrationBuilder.DropForeignKey(
                name: "FK_TinhThanhCus_TinhThanhs_TinhThanhId",
                table: "TinhThanhCus");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauSuaChuas_Phongs_PhongId",
                table: "YeuCauSuaChuas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YeuCauSuaChuas",
                table: "YeuCauSuaChuas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VaiTros",
                table: "VaiTros");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ToaNhas",
                table: "ToaNhas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TinhThanhs",
                table: "TinhThanhs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TinhThanhCus",
                table: "TinhThanhCus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThanhToans",
                table: "ThanhToans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaiSanPhongs",
                table: "TaiSanPhongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SinhViens",
                table: "SinhViens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Phongs",
                table: "Phongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhienXepPhongs",
                table: "PhienXepPhongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NhomDangKys",
                table: "NhomDangKys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NhatKys",
                table: "NhatKys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiDungs",
                table: "NguoiDungs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoaiPhongs",
                table: "LoaiPhongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LichSuChuyenPhongs",
                table: "LichSuChuyenPhongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KhoanThuTienPhongs",
                table: "KhoanThuTienPhongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HopDongs",
                table: "HopDongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HoaDonDienNuocs",
                table: "HoaDonDienNuocs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GiaoDichNganHangs",
                table: "GiaoDichNganHangs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DotDangKys",
                table: "DotDangKys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonDangKys",
                table: "DonDangKys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DienUuTiens",
                table: "DienUuTiens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChiTietXepPhongs",
                table: "ChiTietXepPhongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChiTietHoaDons",
                table: "ChiTietHoaDons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChiSoDienNuocs",
                table: "ChiSoDienNuocs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CauHinhs",
                table: "CauHinhs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BacGias",
                table: "BacGias");

            migrationBuilder.RenameTable(
                name: "YeuCauSuaChuas",
                newName: "YeuCauSuaChua");

            migrationBuilder.RenameTable(
                name: "VaiTros",
                newName: "VaiTro");

            migrationBuilder.RenameTable(
                name: "ToaNhas",
                newName: "ToaNha");

            migrationBuilder.RenameTable(
                name: "TinhThanhs",
                newName: "TinhThanh");

            migrationBuilder.RenameTable(
                name: "TinhThanhCus",
                newName: "TinhThanhCu");

            migrationBuilder.RenameTable(
                name: "ThanhToans",
                newName: "ThanhToan");

            migrationBuilder.RenameTable(
                name: "TaiSanPhongs",
                newName: "TaiSanPhong");

            migrationBuilder.RenameTable(
                name: "SinhViens",
                newName: "SinhVien");

            migrationBuilder.RenameTable(
                name: "Phongs",
                newName: "Phong");

            migrationBuilder.RenameTable(
                name: "PhienXepPhongs",
                newName: "PhienXepPhong");

            migrationBuilder.RenameTable(
                name: "NhomDangKys",
                newName: "NhomDangKy");

            migrationBuilder.RenameTable(
                name: "NhatKys",
                newName: "NhatKy");

            migrationBuilder.RenameTable(
                name: "NguoiDungs",
                newName: "NguoiDung");

            migrationBuilder.RenameTable(
                name: "LoaiPhongs",
                newName: "LoaiPhong");

            migrationBuilder.RenameTable(
                name: "LichSuChuyenPhongs",
                newName: "LichSuChuyenPhong");

            migrationBuilder.RenameTable(
                name: "KhoanThuTienPhongs",
                newName: "KhoanThuTienPhong");

            migrationBuilder.RenameTable(
                name: "HopDongs",
                newName: "HopDong");

            migrationBuilder.RenameTable(
                name: "HoaDonDienNuocs",
                newName: "HoaDonDienNuoc");

            migrationBuilder.RenameTable(
                name: "GiaoDichNganHangs",
                newName: "GiaoDichNganHang");

            migrationBuilder.RenameTable(
                name: "DotDangKys",
                newName: "DotDangKy");

            migrationBuilder.RenameTable(
                name: "DonDangKys",
                newName: "DonDangKy");

            migrationBuilder.RenameTable(
                name: "DienUuTiens",
                newName: "DienUuTien");

            migrationBuilder.RenameTable(
                name: "ChiTietXepPhongs",
                newName: "ChiTietXepPhong");

            migrationBuilder.RenameTable(
                name: "ChiTietHoaDons",
                newName: "ChiTietHoaDon");

            migrationBuilder.RenameTable(
                name: "ChiSoDienNuocs",
                newName: "ChiSoDienNuoc");

            migrationBuilder.RenameTable(
                name: "CauHinhs",
                newName: "CauHinh");

            migrationBuilder.RenameTable(
                name: "BacGias",
                newName: "BacGia");

            migrationBuilder.RenameIndex(
                name: "IX_YeuCauSuaChuas_PhongId",
                table: "YeuCauSuaChua",
                newName: "IX_YeuCauSuaChua_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_TinhThanhCus_TinhThanhId",
                table: "TinhThanhCu",
                newName: "IX_TinhThanhCu_TinhThanhId");

            migrationBuilder.RenameIndex(
                name: "IX_ThanhToans_NguoiThuId",
                table: "ThanhToan",
                newName: "IX_ThanhToan_NguoiThuId");

            migrationBuilder.RenameIndex(
                name: "IX_ThanhToans_KhoanThuTienPhongId",
                table: "ThanhToan",
                newName: "IX_ThanhToan_KhoanThuTienPhongId");

            migrationBuilder.RenameIndex(
                name: "IX_ThanhToans_HoaDonDienNuocId",
                table: "ThanhToan",
                newName: "IX_ThanhToan_HoaDonDienNuocId");

            migrationBuilder.RenameIndex(
                name: "IX_ThanhToans_GiaoDichNganHangId",
                table: "ThanhToan",
                newName: "IX_ThanhToan_GiaoDichNganHangId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiSanPhongs_PhongId",
                table: "TaiSanPhong",
                newName: "IX_TaiSanPhong_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_SinhViens_TinhThanhId",
                table: "SinhVien",
                newName: "IX_SinhVien_TinhThanhId");

            migrationBuilder.RenameIndex(
                name: "IX_SinhViens_DienUuTienId",
                table: "SinhVien",
                newName: "IX_SinhVien_DienUuTienId");

            migrationBuilder.RenameIndex(
                name: "IX_SinhViens_CCCD",
                table: "SinhVien",
                newName: "IX_SinhVien_CCCD");

            migrationBuilder.RenameIndex(
                name: "IX_Phongs_ToaId_MaPhong",
                table: "Phong",
                newName: "IX_Phong_ToaId_MaPhong");

            migrationBuilder.RenameIndex(
                name: "IX_Phongs_LoaiPhongId",
                table: "Phong",
                newName: "IX_Phong_LoaiPhongId");

            migrationBuilder.RenameIndex(
                name: "IX_PhienXepPhongs_DotDangKyId",
                table: "PhienXepPhong",
                newName: "IX_PhienXepPhong_DotDangKyId");

            migrationBuilder.RenameIndex(
                name: "IX_NhomDangKys_MaNhom",
                table: "NhomDangKy",
                newName: "IX_NhomDangKy_MaNhom");

            migrationBuilder.RenameIndex(
                name: "IX_NhatKys_NguoiDungId",
                table: "NhatKy",
                newName: "IX_NhatKy_NguoiDungId");

            migrationBuilder.RenameIndex(
                name: "IX_NguoiDungs_TenDangNhap",
                table: "NguoiDung",
                newName: "IX_NguoiDung_TenDangNhap");

            migrationBuilder.RenameIndex(
                name: "IX_NguoiDungs_MaVaiTro",
                table: "NguoiDung",
                newName: "IX_NguoiDung_MaVaiTro");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuChuyenPhongs_TuPhongId",
                table: "LichSuChuyenPhong",
                newName: "IX_LichSuChuyenPhong_TuPhongId");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuChuyenPhongs_HopDongId",
                table: "LichSuChuyenPhong",
                newName: "IX_LichSuChuyenPhong_HopDongId");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuChuyenPhongs_DenPhongId",
                table: "LichSuChuyenPhong",
                newName: "IX_LichSuChuyenPhong_DenPhongId");

            migrationBuilder.RenameIndex(
                name: "IX_KhoanThuTienPhongs_MaThanhToan",
                table: "KhoanThuTienPhong",
                newName: "IX_KhoanThuTienPhong_MaThanhToan");

            migrationBuilder.RenameIndex(
                name: "IX_KhoanThuTienPhongs_HopDongId",
                table: "KhoanThuTienPhong",
                newName: "IX_KhoanThuTienPhong_HopDongId");

            migrationBuilder.RenameIndex(
                name: "IX_HopDongs_PhongId",
                table: "HopDong",
                newName: "IX_HopDong_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_HopDongs_MSSV",
                table: "HopDong",
                newName: "IX_HopDong_MSSV");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDonDienNuocs_PhongId",
                table: "HoaDonDienNuoc",
                newName: "IX_HoaDonDienNuoc_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDonDienNuocs_MaThanhToan",
                table: "HoaDonDienNuoc",
                newName: "IX_HoaDonDienNuoc_MaThanhToan");

            migrationBuilder.RenameIndex(
                name: "IX_GiaoDichNganHangs_MaGiaoDich",
                table: "GiaoDichNganHang",
                newName: "IX_GiaoDichNganHang_MaGiaoDich");

            migrationBuilder.RenameIndex(
                name: "IX_GiaoDichNganHangs_HashSaoKe",
                table: "GiaoDichNganHang",
                newName: "IX_GiaoDichNganHang_HashSaoKe");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKys_ToaMongMuonId",
                table: "DonDangKy",
                newName: "IX_DonDangKy_ToaMongMuonId");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKys_NhomId",
                table: "DonDangKy",
                newName: "IX_DonDangKy_NhomId");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKys_MSSV",
                table: "DonDangKy",
                newName: "IX_DonDangKy_MSSV");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKys_LoaiPhongMongMuonId",
                table: "DonDangKy",
                newName: "IX_DonDangKy_LoaiPhongMongMuonId");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKys_DotId_MSSV",
                table: "DonDangKy",
                newName: "IX_DonDangKy_DotId_MSSV");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietXepPhongs_PhongId",
                table: "ChiTietXepPhong",
                newName: "IX_ChiTietXepPhong_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietXepPhongs_PhienId",
                table: "ChiTietXepPhong",
                newName: "IX_ChiTietXepPhong_PhienId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietXepPhongs_DonId",
                table: "ChiTietXepPhong",
                newName: "IX_ChiTietXepPhong_DonId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietHoaDons_HoaDonDienNuocId",
                table: "ChiTietHoaDon",
                newName: "IX_ChiTietHoaDon_HoaDonDienNuocId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiSoDienNuocs_PhongId_Thang_Nam",
                table: "ChiSoDienNuoc",
                newName: "IX_ChiSoDienNuoc_PhongId_Thang_Nam");

            migrationBuilder.RenameIndex(
                name: "IX_CauHinhs_Khoa",
                table: "CauHinh",
                newName: "IX_CauHinh_Khoa");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YeuCauSuaChua",
                table: "YeuCauSuaChua",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VaiTro",
                table: "VaiTro",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ToaNha",
                table: "ToaNha",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TinhThanh",
                table: "TinhThanh",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TinhThanhCu",
                table: "TinhThanhCu",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThanhToan",
                table: "ThanhToan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaiSanPhong",
                table: "TaiSanPhong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SinhVien",
                table: "SinhVien",
                column: "MSSV");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Phong",
                table: "Phong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhienXepPhong",
                table: "PhienXepPhong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NhomDangKy",
                table: "NhomDangKy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NhatKy",
                table: "NhatKy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiDung",
                table: "NguoiDung",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoaiPhong",
                table: "LoaiPhong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LichSuChuyenPhong",
                table: "LichSuChuyenPhong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KhoanThuTienPhong",
                table: "KhoanThuTienPhong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HopDong",
                table: "HopDong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HoaDonDienNuoc",
                table: "HoaDonDienNuoc",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GiaoDichNganHang",
                table: "GiaoDichNganHang",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DotDangKy",
                table: "DotDangKy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonDangKy",
                table: "DonDangKy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DienUuTien",
                table: "DienUuTien",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChiTietXepPhong",
                table: "ChiTietXepPhong",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChiTietHoaDon",
                table: "ChiTietHoaDon",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChiSoDienNuoc",
                table: "ChiSoDienNuoc",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CauHinh",
                table: "CauHinh",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BacGia",
                table: "BacGia",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiSoDienNuoc_Phong_PhongId",
                table: "ChiSoDienNuoc",
                column: "PhongId",
                principalTable: "Phong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietHoaDon_HoaDonDienNuoc_HoaDonDienNuocId",
                table: "ChiTietHoaDon",
                column: "HoaDonDienNuocId",
                principalTable: "HoaDonDienNuoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietXepPhong_DonDangKy_DonId",
                table: "ChiTietXepPhong",
                column: "DonId",
                principalTable: "DonDangKy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietXepPhong_PhienXepPhong_PhienId",
                table: "ChiTietXepPhong",
                column: "PhienId",
                principalTable: "PhienXepPhong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietXepPhong_Phong_PhongId",
                table: "ChiTietXepPhong",
                column: "PhongId",
                principalTable: "Phong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKy_DotDangKy_DotId",
                table: "DonDangKy",
                column: "DotId",
                principalTable: "DotDangKy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKy_LoaiPhong_LoaiPhongMongMuonId",
                table: "DonDangKy",
                column: "LoaiPhongMongMuonId",
                principalTable: "LoaiPhong",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKy_NhomDangKy_NhomId",
                table: "DonDangKy",
                column: "NhomId",
                principalTable: "NhomDangKy",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKy_SinhVien_MSSV",
                table: "DonDangKy",
                column: "MSSV",
                principalTable: "SinhVien",
                principalColumn: "MSSV",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKy_ToaNha_ToaMongMuonId",
                table: "DonDangKy",
                column: "ToaMongMuonId",
                principalTable: "ToaNha",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonDienNuoc_Phong_PhongId",
                table: "HoaDonDienNuoc",
                column: "PhongId",
                principalTable: "Phong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HopDong_Phong_PhongId",
                table: "HopDong",
                column: "PhongId",
                principalTable: "Phong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HopDong_SinhVien_MSSV",
                table: "HopDong",
                column: "MSSV",
                principalTable: "SinhVien",
                principalColumn: "MSSV",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KhoanThuTienPhong_HopDong_HopDongId",
                table: "KhoanThuTienPhong",
                column: "HopDongId",
                principalTable: "HopDong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuChuyenPhong_HopDong_HopDongId",
                table: "LichSuChuyenPhong",
                column: "HopDongId",
                principalTable: "HopDong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuChuyenPhong_Phong_DenPhongId",
                table: "LichSuChuyenPhong",
                column: "DenPhongId",
                principalTable: "Phong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuChuyenPhong_Phong_TuPhongId",
                table: "LichSuChuyenPhong",
                column: "TuPhongId",
                principalTable: "Phong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_VaiTro_MaVaiTro",
                table: "NguoiDung",
                column: "MaVaiTro",
                principalTable: "VaiTro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NhatKy_NguoiDung_NguoiDungId",
                table: "NhatKy",
                column: "NguoiDungId",
                principalTable: "NguoiDung",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PhienXepPhong_DotDangKy_DotDangKyId",
                table: "PhienXepPhong",
                column: "DotDangKyId",
                principalTable: "DotDangKy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Phong_LoaiPhong_LoaiPhongId",
                table: "Phong",
                column: "LoaiPhongId",
                principalTable: "LoaiPhong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Phong_ToaNha_ToaId",
                table: "Phong",
                column: "ToaId",
                principalTable: "ToaNha",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVien_DienUuTien_DienUuTienId",
                table: "SinhVien",
                column: "DienUuTienId",
                principalTable: "DienUuTien",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVien_TinhThanh_TinhThanhId",
                table: "SinhVien",
                column: "TinhThanhId",
                principalTable: "TinhThanh",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiSanPhong_Phong_PhongId",
                table: "TaiSanPhong",
                column: "PhongId",
                principalTable: "Phong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToan_GiaoDichNganHang_GiaoDichNganHangId",
                table: "ThanhToan",
                column: "GiaoDichNganHangId",
                principalTable: "GiaoDichNganHang",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToan_HoaDonDienNuoc_HoaDonDienNuocId",
                table: "ThanhToan",
                column: "HoaDonDienNuocId",
                principalTable: "HoaDonDienNuoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToan_KhoanThuTienPhong_KhoanThuTienPhongId",
                table: "ThanhToan",
                column: "KhoanThuTienPhongId",
                principalTable: "KhoanThuTienPhong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToan_NguoiDung_NguoiThuId",
                table: "ThanhToan",
                column: "NguoiThuId",
                principalTable: "NguoiDung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TinhThanhCu_TinhThanh_TinhThanhId",
                table: "TinhThanhCu",
                column: "TinhThanhId",
                principalTable: "TinhThanh",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauSuaChua_Phong_PhongId",
                table: "YeuCauSuaChua",
                column: "PhongId",
                principalTable: "Phong",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiSoDienNuoc_Phong_PhongId",
                table: "ChiSoDienNuoc");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietHoaDon_HoaDonDienNuoc_HoaDonDienNuocId",
                table: "ChiTietHoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietXepPhong_DonDangKy_DonId",
                table: "ChiTietXepPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietXepPhong_PhienXepPhong_PhienId",
                table: "ChiTietXepPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietXepPhong_Phong_PhongId",
                table: "ChiTietXepPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKy_DotDangKy_DotId",
                table: "DonDangKy");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKy_LoaiPhong_LoaiPhongMongMuonId",
                table: "DonDangKy");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKy_NhomDangKy_NhomId",
                table: "DonDangKy");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKy_SinhVien_MSSV",
                table: "DonDangKy");

            migrationBuilder.DropForeignKey(
                name: "FK_DonDangKy_ToaNha_ToaMongMuonId",
                table: "DonDangKy");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonDienNuoc_Phong_PhongId",
                table: "HoaDonDienNuoc");

            migrationBuilder.DropForeignKey(
                name: "FK_HopDong_Phong_PhongId",
                table: "HopDong");

            migrationBuilder.DropForeignKey(
                name: "FK_HopDong_SinhVien_MSSV",
                table: "HopDong");

            migrationBuilder.DropForeignKey(
                name: "FK_KhoanThuTienPhong_HopDong_HopDongId",
                table: "KhoanThuTienPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuChuyenPhong_HopDong_HopDongId",
                table: "LichSuChuyenPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuChuyenPhong_Phong_DenPhongId",
                table: "LichSuChuyenPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuChuyenPhong_Phong_TuPhongId",
                table: "LichSuChuyenPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_VaiTro_MaVaiTro",
                table: "NguoiDung");

            migrationBuilder.DropForeignKey(
                name: "FK_NhatKy_NguoiDung_NguoiDungId",
                table: "NhatKy");

            migrationBuilder.DropForeignKey(
                name: "FK_PhienXepPhong_DotDangKy_DotDangKyId",
                table: "PhienXepPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_Phong_LoaiPhong_LoaiPhongId",
                table: "Phong");

            migrationBuilder.DropForeignKey(
                name: "FK_Phong_ToaNha_ToaId",
                table: "Phong");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhVien_DienUuTien_DienUuTienId",
                table: "SinhVien");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhVien_TinhThanh_TinhThanhId",
                table: "SinhVien");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiSanPhong_Phong_PhongId",
                table: "TaiSanPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToan_GiaoDichNganHang_GiaoDichNganHangId",
                table: "ThanhToan");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToan_HoaDonDienNuoc_HoaDonDienNuocId",
                table: "ThanhToan");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToan_KhoanThuTienPhong_KhoanThuTienPhongId",
                table: "ThanhToan");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToan_NguoiDung_NguoiThuId",
                table: "ThanhToan");

            migrationBuilder.DropForeignKey(
                name: "FK_TinhThanhCu_TinhThanh_TinhThanhId",
                table: "TinhThanhCu");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauSuaChua_Phong_PhongId",
                table: "YeuCauSuaChua");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YeuCauSuaChua",
                table: "YeuCauSuaChua");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VaiTro",
                table: "VaiTro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ToaNha",
                table: "ToaNha");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TinhThanhCu",
                table: "TinhThanhCu");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TinhThanh",
                table: "TinhThanh");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThanhToan",
                table: "ThanhToan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaiSanPhong",
                table: "TaiSanPhong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SinhVien",
                table: "SinhVien");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Phong",
                table: "Phong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhienXepPhong",
                table: "PhienXepPhong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NhomDangKy",
                table: "NhomDangKy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NhatKy",
                table: "NhatKy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiDung",
                table: "NguoiDung");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoaiPhong",
                table: "LoaiPhong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LichSuChuyenPhong",
                table: "LichSuChuyenPhong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KhoanThuTienPhong",
                table: "KhoanThuTienPhong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HopDong",
                table: "HopDong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HoaDonDienNuoc",
                table: "HoaDonDienNuoc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GiaoDichNganHang",
                table: "GiaoDichNganHang");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DotDangKy",
                table: "DotDangKy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonDangKy",
                table: "DonDangKy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DienUuTien",
                table: "DienUuTien");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChiTietXepPhong",
                table: "ChiTietXepPhong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChiTietHoaDon",
                table: "ChiTietHoaDon");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChiSoDienNuoc",
                table: "ChiSoDienNuoc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CauHinh",
                table: "CauHinh");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BacGia",
                table: "BacGia");

            migrationBuilder.RenameTable(
                name: "YeuCauSuaChua",
                newName: "YeuCauSuaChuas");

            migrationBuilder.RenameTable(
                name: "VaiTro",
                newName: "VaiTros");

            migrationBuilder.RenameTable(
                name: "ToaNha",
                newName: "ToaNhas");

            migrationBuilder.RenameTable(
                name: "TinhThanhCu",
                newName: "TinhThanhCus");

            migrationBuilder.RenameTable(
                name: "TinhThanh",
                newName: "TinhThanhs");

            migrationBuilder.RenameTable(
                name: "ThanhToan",
                newName: "ThanhToans");

            migrationBuilder.RenameTable(
                name: "TaiSanPhong",
                newName: "TaiSanPhongs");

            migrationBuilder.RenameTable(
                name: "SinhVien",
                newName: "SinhViens");

            migrationBuilder.RenameTable(
                name: "Phong",
                newName: "Phongs");

            migrationBuilder.RenameTable(
                name: "PhienXepPhong",
                newName: "PhienXepPhongs");

            migrationBuilder.RenameTable(
                name: "NhomDangKy",
                newName: "NhomDangKys");

            migrationBuilder.RenameTable(
                name: "NhatKy",
                newName: "NhatKys");

            migrationBuilder.RenameTable(
                name: "NguoiDung",
                newName: "NguoiDungs");

            migrationBuilder.RenameTable(
                name: "LoaiPhong",
                newName: "LoaiPhongs");

            migrationBuilder.RenameTable(
                name: "LichSuChuyenPhong",
                newName: "LichSuChuyenPhongs");

            migrationBuilder.RenameTable(
                name: "KhoanThuTienPhong",
                newName: "KhoanThuTienPhongs");

            migrationBuilder.RenameTable(
                name: "HopDong",
                newName: "HopDongs");

            migrationBuilder.RenameTable(
                name: "HoaDonDienNuoc",
                newName: "HoaDonDienNuocs");

            migrationBuilder.RenameTable(
                name: "GiaoDichNganHang",
                newName: "GiaoDichNganHangs");

            migrationBuilder.RenameTable(
                name: "DotDangKy",
                newName: "DotDangKys");

            migrationBuilder.RenameTable(
                name: "DonDangKy",
                newName: "DonDangKys");

            migrationBuilder.RenameTable(
                name: "DienUuTien",
                newName: "DienUuTiens");

            migrationBuilder.RenameTable(
                name: "ChiTietXepPhong",
                newName: "ChiTietXepPhongs");

            migrationBuilder.RenameTable(
                name: "ChiTietHoaDon",
                newName: "ChiTietHoaDons");

            migrationBuilder.RenameTable(
                name: "ChiSoDienNuoc",
                newName: "ChiSoDienNuocs");

            migrationBuilder.RenameTable(
                name: "CauHinh",
                newName: "CauHinhs");

            migrationBuilder.RenameTable(
                name: "BacGia",
                newName: "BacGias");

            migrationBuilder.RenameIndex(
                name: "IX_YeuCauSuaChua_PhongId",
                table: "YeuCauSuaChuas",
                newName: "IX_YeuCauSuaChuas_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_TinhThanhCu_TinhThanhId",
                table: "TinhThanhCus",
                newName: "IX_TinhThanhCus_TinhThanhId");

            migrationBuilder.RenameIndex(
                name: "IX_ThanhToan_NguoiThuId",
                table: "ThanhToans",
                newName: "IX_ThanhToans_NguoiThuId");

            migrationBuilder.RenameIndex(
                name: "IX_ThanhToan_KhoanThuTienPhongId",
                table: "ThanhToans",
                newName: "IX_ThanhToans_KhoanThuTienPhongId");

            migrationBuilder.RenameIndex(
                name: "IX_ThanhToan_HoaDonDienNuocId",
                table: "ThanhToans",
                newName: "IX_ThanhToans_HoaDonDienNuocId");

            migrationBuilder.RenameIndex(
                name: "IX_ThanhToan_GiaoDichNganHangId",
                table: "ThanhToans",
                newName: "IX_ThanhToans_GiaoDichNganHangId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiSanPhong_PhongId",
                table: "TaiSanPhongs",
                newName: "IX_TaiSanPhongs_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_SinhVien_TinhThanhId",
                table: "SinhViens",
                newName: "IX_SinhViens_TinhThanhId");

            migrationBuilder.RenameIndex(
                name: "IX_SinhVien_DienUuTienId",
                table: "SinhViens",
                newName: "IX_SinhViens_DienUuTienId");

            migrationBuilder.RenameIndex(
                name: "IX_SinhVien_CCCD",
                table: "SinhViens",
                newName: "IX_SinhViens_CCCD");

            migrationBuilder.RenameIndex(
                name: "IX_Phong_ToaId_MaPhong",
                table: "Phongs",
                newName: "IX_Phongs_ToaId_MaPhong");

            migrationBuilder.RenameIndex(
                name: "IX_Phong_LoaiPhongId",
                table: "Phongs",
                newName: "IX_Phongs_LoaiPhongId");

            migrationBuilder.RenameIndex(
                name: "IX_PhienXepPhong_DotDangKyId",
                table: "PhienXepPhongs",
                newName: "IX_PhienXepPhongs_DotDangKyId");

            migrationBuilder.RenameIndex(
                name: "IX_NhomDangKy_MaNhom",
                table: "NhomDangKys",
                newName: "IX_NhomDangKys_MaNhom");

            migrationBuilder.RenameIndex(
                name: "IX_NhatKy_NguoiDungId",
                table: "NhatKys",
                newName: "IX_NhatKys_NguoiDungId");

            migrationBuilder.RenameIndex(
                name: "IX_NguoiDung_TenDangNhap",
                table: "NguoiDungs",
                newName: "IX_NguoiDungs_TenDangNhap");

            migrationBuilder.RenameIndex(
                name: "IX_NguoiDung_MaVaiTro",
                table: "NguoiDungs",
                newName: "IX_NguoiDungs_MaVaiTro");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuChuyenPhong_TuPhongId",
                table: "LichSuChuyenPhongs",
                newName: "IX_LichSuChuyenPhongs_TuPhongId");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuChuyenPhong_HopDongId",
                table: "LichSuChuyenPhongs",
                newName: "IX_LichSuChuyenPhongs_HopDongId");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuChuyenPhong_DenPhongId",
                table: "LichSuChuyenPhongs",
                newName: "IX_LichSuChuyenPhongs_DenPhongId");

            migrationBuilder.RenameIndex(
                name: "IX_KhoanThuTienPhong_MaThanhToan",
                table: "KhoanThuTienPhongs",
                newName: "IX_KhoanThuTienPhongs_MaThanhToan");

            migrationBuilder.RenameIndex(
                name: "IX_KhoanThuTienPhong_HopDongId",
                table: "KhoanThuTienPhongs",
                newName: "IX_KhoanThuTienPhongs_HopDongId");

            migrationBuilder.RenameIndex(
                name: "IX_HopDong_PhongId",
                table: "HopDongs",
                newName: "IX_HopDongs_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_HopDong_MSSV",
                table: "HopDongs",
                newName: "IX_HopDongs_MSSV");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDonDienNuoc_PhongId",
                table: "HoaDonDienNuocs",
                newName: "IX_HoaDonDienNuocs_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDonDienNuoc_MaThanhToan",
                table: "HoaDonDienNuocs",
                newName: "IX_HoaDonDienNuocs_MaThanhToan");

            migrationBuilder.RenameIndex(
                name: "IX_GiaoDichNganHang_MaGiaoDich",
                table: "GiaoDichNganHangs",
                newName: "IX_GiaoDichNganHangs_MaGiaoDich");

            migrationBuilder.RenameIndex(
                name: "IX_GiaoDichNganHang_HashSaoKe",
                table: "GiaoDichNganHangs",
                newName: "IX_GiaoDichNganHangs_HashSaoKe");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKy_ToaMongMuonId",
                table: "DonDangKys",
                newName: "IX_DonDangKys_ToaMongMuonId");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKy_NhomId",
                table: "DonDangKys",
                newName: "IX_DonDangKys_NhomId");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKy_MSSV",
                table: "DonDangKys",
                newName: "IX_DonDangKys_MSSV");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKy_LoaiPhongMongMuonId",
                table: "DonDangKys",
                newName: "IX_DonDangKys_LoaiPhongMongMuonId");

            migrationBuilder.RenameIndex(
                name: "IX_DonDangKy_DotId_MSSV",
                table: "DonDangKys",
                newName: "IX_DonDangKys_DotId_MSSV");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietXepPhong_PhongId",
                table: "ChiTietXepPhongs",
                newName: "IX_ChiTietXepPhongs_PhongId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietXepPhong_PhienId",
                table: "ChiTietXepPhongs",
                newName: "IX_ChiTietXepPhongs_PhienId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietXepPhong_DonId",
                table: "ChiTietXepPhongs",
                newName: "IX_ChiTietXepPhongs_DonId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietHoaDon_HoaDonDienNuocId",
                table: "ChiTietHoaDons",
                newName: "IX_ChiTietHoaDons_HoaDonDienNuocId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiSoDienNuoc_PhongId_Thang_Nam",
                table: "ChiSoDienNuocs",
                newName: "IX_ChiSoDienNuocs_PhongId_Thang_Nam");

            migrationBuilder.RenameIndex(
                name: "IX_CauHinh_Khoa",
                table: "CauHinhs",
                newName: "IX_CauHinhs_Khoa");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YeuCauSuaChuas",
                table: "YeuCauSuaChuas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VaiTros",
                table: "VaiTros",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ToaNhas",
                table: "ToaNhas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TinhThanhCus",
                table: "TinhThanhCus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TinhThanhs",
                table: "TinhThanhs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThanhToans",
                table: "ThanhToans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaiSanPhongs",
                table: "TaiSanPhongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SinhViens",
                table: "SinhViens",
                column: "MSSV");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Phongs",
                table: "Phongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhienXepPhongs",
                table: "PhienXepPhongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NhomDangKys",
                table: "NhomDangKys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NhatKys",
                table: "NhatKys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiDungs",
                table: "NguoiDungs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoaiPhongs",
                table: "LoaiPhongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LichSuChuyenPhongs",
                table: "LichSuChuyenPhongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KhoanThuTienPhongs",
                table: "KhoanThuTienPhongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HopDongs",
                table: "HopDongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HoaDonDienNuocs",
                table: "HoaDonDienNuocs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GiaoDichNganHangs",
                table: "GiaoDichNganHangs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DotDangKys",
                table: "DotDangKys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonDangKys",
                table: "DonDangKys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DienUuTiens",
                table: "DienUuTiens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChiTietXepPhongs",
                table: "ChiTietXepPhongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChiTietHoaDons",
                table: "ChiTietHoaDons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChiSoDienNuocs",
                table: "ChiSoDienNuocs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CauHinhs",
                table: "CauHinhs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BacGias",
                table: "BacGias",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiSoDienNuocs_Phongs_PhongId",
                table: "ChiSoDienNuocs",
                column: "PhongId",
                principalTable: "Phongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietHoaDons_HoaDonDienNuocs_HoaDonDienNuocId",
                table: "ChiTietHoaDons",
                column: "HoaDonDienNuocId",
                principalTable: "HoaDonDienNuocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietXepPhongs_DonDangKys_DonId",
                table: "ChiTietXepPhongs",
                column: "DonId",
                principalTable: "DonDangKys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietXepPhongs_PhienXepPhongs_PhienId",
                table: "ChiTietXepPhongs",
                column: "PhienId",
                principalTable: "PhienXepPhongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietXepPhongs_Phongs_PhongId",
                table: "ChiTietXepPhongs",
                column: "PhongId",
                principalTable: "Phongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKys_DotDangKys_DotId",
                table: "DonDangKys",
                column: "DotId",
                principalTable: "DotDangKys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKys_LoaiPhongs_LoaiPhongMongMuonId",
                table: "DonDangKys",
                column: "LoaiPhongMongMuonId",
                principalTable: "LoaiPhongs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKys_NhomDangKys_NhomId",
                table: "DonDangKys",
                column: "NhomId",
                principalTable: "NhomDangKys",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKys_SinhViens_MSSV",
                table: "DonDangKys",
                column: "MSSV",
                principalTable: "SinhViens",
                principalColumn: "MSSV",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonDangKys_ToaNhas_ToaMongMuonId",
                table: "DonDangKys",
                column: "ToaMongMuonId",
                principalTable: "ToaNhas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonDienNuocs_Phongs_PhongId",
                table: "HoaDonDienNuocs",
                column: "PhongId",
                principalTable: "Phongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HopDongs_Phongs_PhongId",
                table: "HopDongs",
                column: "PhongId",
                principalTable: "Phongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HopDongs_SinhViens_MSSV",
                table: "HopDongs",
                column: "MSSV",
                principalTable: "SinhViens",
                principalColumn: "MSSV",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KhoanThuTienPhongs_HopDongs_HopDongId",
                table: "KhoanThuTienPhongs",
                column: "HopDongId",
                principalTable: "HopDongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuChuyenPhongs_HopDongs_HopDongId",
                table: "LichSuChuyenPhongs",
                column: "HopDongId",
                principalTable: "HopDongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuChuyenPhongs_Phongs_DenPhongId",
                table: "LichSuChuyenPhongs",
                column: "DenPhongId",
                principalTable: "Phongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuChuyenPhongs_Phongs_TuPhongId",
                table: "LichSuChuyenPhongs",
                column: "TuPhongId",
                principalTable: "Phongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDungs_VaiTros_MaVaiTro",
                table: "NguoiDungs",
                column: "MaVaiTro",
                principalTable: "VaiTros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NhatKys_NguoiDungs_NguoiDungId",
                table: "NhatKys",
                column: "NguoiDungId",
                principalTable: "NguoiDungs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PhienXepPhongs_DotDangKys_DotDangKyId",
                table: "PhienXepPhongs",
                column: "DotDangKyId",
                principalTable: "DotDangKys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Phongs_LoaiPhongs_LoaiPhongId",
                table: "Phongs",
                column: "LoaiPhongId",
                principalTable: "LoaiPhongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Phongs_ToaNhas_ToaId",
                table: "Phongs",
                column: "ToaId",
                principalTable: "ToaNhas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SinhViens_DienUuTiens_DienUuTienId",
                table: "SinhViens",
                column: "DienUuTienId",
                principalTable: "DienUuTiens",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SinhViens_TinhThanhs_TinhThanhId",
                table: "SinhViens",
                column: "TinhThanhId",
                principalTable: "TinhThanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiSanPhongs_Phongs_PhongId",
                table: "TaiSanPhongs",
                column: "PhongId",
                principalTable: "Phongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_GiaoDichNganHangs_GiaoDichNganHangId",
                table: "ThanhToans",
                column: "GiaoDichNganHangId",
                principalTable: "GiaoDichNganHangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_HoaDonDienNuocs_HoaDonDienNuocId",
                table: "ThanhToans",
                column: "HoaDonDienNuocId",
                principalTable: "HoaDonDienNuocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_KhoanThuTienPhongs_KhoanThuTienPhongId",
                table: "ThanhToans",
                column: "KhoanThuTienPhongId",
                principalTable: "KhoanThuTienPhongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_NguoiDungs_NguoiThuId",
                table: "ThanhToans",
                column: "NguoiThuId",
                principalTable: "NguoiDungs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TinhThanhCus_TinhThanhs_TinhThanhId",
                table: "TinhThanhCus",
                column: "TinhThanhId",
                principalTable: "TinhThanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauSuaChuas_Phongs_PhongId",
                table: "YeuCauSuaChuas",
                column: "PhongId",
                principalTable: "Phongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
