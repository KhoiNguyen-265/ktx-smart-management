using Bogus;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTX.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(KtxDbContext db)
    {
        // 1. Seed Roles & Users
        if (!await db.VaiTros.AnyAsync())
        {
            var adminRole = new VaiTro { TenVaiTro = "Admin" };
            var managerRole = new VaiTro { TenVaiTro = "QuanLy" };
            var accountantRole = new VaiTro { TenVaiTro = "KeToan" };

            db.VaiTros.AddRange(adminRole, managerRole, accountantRole);
            await db.SaveChangesAsync();

            // Hash password with BCrypt
            var adminUser = new NguoiDung
            {
                TenDangNhap = "admin",
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                HoTen = "Quản trị viên Hệ thống",
                MaVaiTro = adminRole.Id,
                HoatDong = true
            };
            var managerUser = new NguoiDung
            {
                TenDangNhap = "quanly",
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword("quanly123"),
                HoTen = "Nguyễn Văn Quản",
                MaVaiTro = managerRole.Id,
                HoatDong = true
            };
            var accountantUser = new NguoiDung
            {
                TenDangNhap = "ketoan",
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword("ketoan123"),
                HoTen = "Trần Thị Thu",
                MaVaiTro = accountantRole.Id,
                HoatDong = true
            };

            db.NguoiDungs.AddRange(adminUser, managerUser, accountantUser);
            await db.SaveChangesAsync();
        }

        // 2. Seed System Config
        if (!await db.CauHinhs.AnyAsync())
        {
            db.CauHinhs.AddRange(
                new CauHinh { Khoa = "BankBin", GiaTri = "970422", MoTa = "Mã BIN Ngân hàng (MBBank)" },
                new CauHinh { Khoa = "BankAccount", GiaTri = "0888999888", MoTa = "Số tài khoản ngân hàng thụ hưởng" },
                new CauHinh { Khoa = "AccountName", GiaTri = "BAN QUAN LY KTX", MoTa = "Tên chủ tài khoản" },
                new CauHinh { Khoa = "WeightCorrectType", GiaTri = "30", MoTa = "Trọng số đúng loại phòng mong muốn" },
                new CauHinh { Khoa = "WeightCorrectBuilding", GiaTri = "20", MoTa = "Trọng số đúng tòa mong muốn" },
                new CauHinh { Khoa = "WeightCorrectFloor", GiaTri = "15", MoTa = "Trọng số đúng tầng mong muốn" },
                new CauHinh { Khoa = "WeightSameFaculty", GiaTri = "10", MoTa = "Trọng số cùng khoa" },
                new CauHinh { Khoa = "WeightSameYear", GiaTri = "10", MoTa = "Trọng số cùng khóa" },
                new CauHinh { Khoa = "WeightFillHalfRoom", GiaTri = "15", MoTa = "Trọng số ưu tiên lấp đầy phòng dở" },
                new CauHinh { Khoa = "VatRate", GiaTri = "0.08", MoTa = "Thuế suất VAT điện nước (8%)" }
            );
            await db.SaveChangesAsync();
        }

        // 3. Seed 34 Tỉnh thành mới & TinhThanhCu map
        if (!await db.TinhThanhs.AnyAsync())
        {
            var provincesData = new (string Ten, int KhoangCach, int Nhom, string[] TenCu)[]
            {
                ("Hà Nội", 15, 1, new[] { "Hà Nội", "Hà Tây" }),
                ("Hải Phòng", 120, 2, new[] { "Hải Phòng" }),
                ("Quảng Ninh", 160, 2, new[] { "Quảng Ninh" }),
                ("Bắc Ninh", 35, 1, new[] { "Bắc Ninh", "Hà Bắc" }),
                ("Bắc Giang", 60, 1, new[] { "Bắc Giang" }),
                ("Hải Dương", 65, 1, new[] { "Hải Dương", "Hải Hưng" }),
                ("Hưng Yên", 55, 1, new[] { "Hưng Yên" }),
                ("Thái Bình", 110, 2, new[] { "Thái Bình" }),
                ("Nam Định", 95, 1, new[] { "Nam Định", "Hà Nam Ninh" }),
                ("Hà Nam", 60, 1, new[] { "Hà Nam" }),
                ("Ninh Bình", 100, 2, new[] { "Ninh Bình" }),
                ("Thái Nguyên", 80, 1, new[] { "Thái Nguyên", "Bắc Thái" }),
                ("Phú Thọ", 90, 1, new[] { "Phú Thọ", "Vĩnh Phú" }),
                ("Vĩnh Phúc", 55, 1, new[] { "Vĩnh Phúc" }),
                ("Lạng Sơn", 160, 2, new[] { "Lạng Sơn" }),
                ("Cao Bằng", 270, 2, new[] { "Cao Bằng" }),
                ("Hà Giang", 300, 3, new[] { "Hà Giang" }),
                ("Tuyên Quang", 140, 2, new[] { "Tuyên Quang", "Tuyên Hà" }),
                ("Lào Cai", 320, 3, new[] { "Lào Cai", "Hoàng Liên Sơn" }),
                ("Yên Bái", 170, 2, new[] { "Yên Bái" }),
                ("Sơn La", 290, 2, new[] { "Sơn La" }),
                ("Điện Biên", 450, 3, new[] { "Điện Biên", "Lai Châu cũ" }),
                ("Hòa Bình", 85, 1, new[] { "Hòa Bình", "Hà Sơn Bình" }),
                ("Thanh Hóa", 160, 2, new[] { "Thanh Hóa" }),
                ("Nghệ An", 310, 3, new[] { "Nghệ An", "Nghệ Tĩnh" }),
                ("Hà Tĩnh", 360, 3, new[] { "Hà Tĩnh" }),
                ("Quảng Bình", 500, 3, new[] { "Quảng Bình", "Bình Trị Thiên" }),
                ("Đà Nẵng", 760, 3, new[] { "Đà Nẵng", "Quảng Nam - Đà Nẵng" }),
                ("Thừa Thiên Huế", 670, 3, new[] { "Thừa Thiên Huế" }),
                ("Quảng Nam", 820, 3, new[] { "Quảng Nam" }),
                ("Khánh Hòa", 1280, 3, new[] { "Khánh Hòa", "Phú Khánh" }),
                ("Lâm Đồng", 1450, 3, new[] { "Lâm Đồng" }),
                ("TP. Hồ Chí Minh", 1700, 3, new[] { "TP. Hồ Chí Minh", "Sài Gòn", "Gia Định" }),
                ("Cần Thơ", 1880, 3, new[] { "Cần Thơ", "Hậu Giang cũ" })
            };

            foreach (var p in provincesData)
            {
                var province = new TinhThanh
                {
                    Ten = p.Ten,
                    KhoangCachKm = p.KhoangCach,
                    NhomKhoangCach = p.Nhom
                };
                foreach (var oldName in p.TenCu)
                {
                    province.TinhThanhCus.Add(new TinhThanhCu { TenCu = oldName });
                }
                db.TinhThanhs.Add(province);
            }
            await db.SaveChangesAsync();
        }

        // 4. Seed Diện ưu tiên
        if (!await db.DienUuTiens.AnyAsync())
        {
            db.DienUuTiens.AddRange(
                new DienUuTien { Ten = "Con thương binh, liệt sĩ, có công với CM", DiemCong = 25 },
                new DienUuTien { Ten = "Vùng sâu, vùng xa, biên giới, hải đảo", DiemCong = 20 },
                new DienUuTien { Ten = "Hộ nghèo, cận nghèo", DiemCong = 15 },
                new DienUuTien { Ten = "Sinh viên khuyết tật", DiemCong = 30 },
                new DienUuTien { Ten = "Không thuộc diện ưu tiên", DiemCong = 0 }
            );
            await db.SaveChangesAsync();
        }

        // 5. Seed Bậc giá điện nước (chuẩn bậc thang lũy tiến)
        if (!await db.BacGias.AnyAsync())
        {
            var applyDate = new DateTime(2025, 1, 1);
            // Điện bậc thang sinh hoạt (đồng/kWh)
            db.BacGias.AddRange(
                new BacGia { LoaiDichVu = LoaiDichVu.Dien, Bac = 1, TuMuc = 0, DenMuc = 50, DonGia = 1806, ApDungTu = applyDate },
                new BacGia { LoaiDichVu = LoaiDichVu.Dien, Bac = 2, TuMuc = 50, DenMuc = 100, DonGia = 1866, ApDungTu = applyDate },
                new BacGia { LoaiDichVu = LoaiDichVu.Dien, Bac = 3, TuMuc = 100, DenMuc = 200, DonGia = 2167, ApDungTu = applyDate },
                new BacGia { LoaiDichVu = LoaiDichVu.Dien, Bac = 4, TuMuc = 200, DenMuc = 300, DonGia = 2729, ApDungTu = applyDate },
                new BacGia { LoaiDichVu = LoaiDichVu.Dien, Bac = 5, TuMuc = 300, DenMuc = 400, DonGia = 3050, ApDungTu = applyDate },
                new BacGia { LoaiDichVu = LoaiDichVu.Dien, Bac = 6, TuMuc = 400, DenMuc = null, DonGia = 3151, ApDungTu = applyDate }
            );

            // Nước sinh hoạt bậc thang (đồng/m3)
            db.BacGias.AddRange(
                new BacGia { LoaiDichVu = LoaiDichVu.Nuoc, Bac = 1, TuMuc = 0, DenMuc = 10, DonGia = 7500, ApDungTu = applyDate },
                new BacGia { LoaiDichVu = LoaiDichVu.Nuoc, Bac = 2, TuMuc = 10, DenMuc = 20, DonGia = 8800, ApDungTu = applyDate },
                new BacGia { LoaiDichVu = LoaiDichVu.Nuoc, Bac = 3, TuMuc = 20, DenMuc = 30, DonGia = 12000, ApDungTu = applyDate },
                new BacGia { LoaiDichVu = LoaiDichVu.Nuoc, Bac = 4, TuMuc = 30, DenMuc = null, DonGia = 24000, ApDungTu = applyDate }
            );
            await db.SaveChangesAsync();
        }

        // 6. Seed Loại phòng & Tòa nhà & Phòng (~120 phòng)
        if (!await db.LoaiPhongs.AnyAsync())
        {
            var roomType4 = new LoaiPhong { Ten = "Phòng 4 người (Điều hòa)", SucChua = 4, GiaThang = 800000, CoDieuHoa = true };
            var roomType6 = new LoaiPhong { Ten = "Phòng 6 người (Điều hòa)", SucChua = 6, GiaThang = 600000, CoDieuHoa = true };
            var roomType8 = new LoaiPhong { Ten = "Phòng 8 người (Quạt trần)", SucChua = 8, GiaThang = 350000, CoDieuHoa = false };

            db.LoaiPhongs.AddRange(roomType4, roomType6, roomType8);
            await db.SaveChangesAsync();

            var bldgA = new ToaNha { Ten = "Tòa A", GioiTinh = GioiTinh.Nam, SoTang = 5 };
            var bldgB = new ToaNha { Ten = "Tòa B", GioiTinh = GioiTinh.Nam, SoTang = 5 };
            var bldgC = new ToaNha { Ten = "Tòa C", GioiTinh = GioiTinh.Nu, SoTang = 5 };
            var bldgD = new ToaNha { Ten = "Tòa D", GioiTinh = GioiTinh.Nu, SoTang = 5 };

            db.ToaNhas.AddRange(bldgA, bldgB, bldgC, bldgD);
            await db.SaveChangesAsync();

            // Sinh 30 phòng cho mỗi tòa (6 phòng mỗi tầng x 5 tầng = 30 phòng/tòa => 120 phòng)
            var buildings = new[] { bldgA, bldgB, bldgC, bldgD };
            var types = new[] { roomType4, roomType6, roomType8 };
            var random = new Random(42);

            foreach (var b in buildings)
            {
                for (int floor = 1; floor <= 5; floor++)
                {
                    for (int roomNum = 1; roomNum <= 6; roomNum++)
                    {
                        var roomCode = $"{b.Ten.Replace("Tòa ", "")}{floor:D1}{roomNum:D2}";
                        var selectedType = types[(floor + roomNum) % 3];
                        var status = (floor == 5 && roomNum == 6) ? TrangThaiPhong.BaoTri : TrangThaiPhong.HoatDong;

                        var room = new Phong
                        {
                            MaPhong = roomCode,
                            ToaId = b.Id,
                            Tang = floor,
                            LoaiPhongId = selectedType.Id,
                            TrangThai = status
                        };

                        // Thêm tài sản mẫu cho phòng
                        room.TaiSanPhongs.Add(new TaiSanPhong { TenTaiSan = "Giường tầng", SoLuong = selectedType.SucChua / 2, TinhTrang = "Tốt" });
                        room.TaiSanPhongs.Add(new TaiSanPhong { TenTaiSan = "Bàn ghế học tập", SoLuong = selectedType.SucChua, TinhTrang = "Tốt" });
                        room.TaiSanPhongs.Add(new TaiSanPhong { TenTaiSan = "Tủ để đồ cá nhân", SoLuong = selectedType.SucChua, TinhTrang = "Tốt" });
                        if (selectedType.CoDieuHoa)
                        {
                            room.TaiSanPhongs.Add(new TaiSanPhong { TenTaiSan = "Điều hòa Daikin 12000BTU", SoLuong = 1, TinhTrang = "Tốt" });
                        }

                        db.Phongs.Add(room);
                    }
                }
            }
            await db.SaveChangesAsync();
        }

        // 7. Seed 600 Sinh viên (bằng Bogus tiếng Việt)
        if (!await db.SinhViens.AnyAsync())
        {
            var provinces = await db.TinhThanhs.ToListAsync();
            var priorities = await db.DienUuTiens.ToListAsync();
            var faculties = new[] { "Công nghệ thông tin", "Điện tử viễn thông", "An toàn thông tin", "Kinh tế & QTKD", "Khoa học máy tính" };

            var faker = new Faker("vi");
            var studentList = new List<SinhVien>();

            for (int i = 1; i <= 600; i++)
            {
                var isMale = faker.Random.Bool(0.55f);
                var gender = isMale ? GioiTinh.Nam : GioiTinh.Nu;
                var firstName = isMale ? faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male) : faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female);
                var lastName = faker.Name.LastName();
                var fullName = $"{lastName} {firstName}";

                var yearOfStudy = faker.Random.Int(65, 68); // K65 - K68
                var mssv = $"B{yearOfStudy % 100:D2}DCCN{i:D3}";
                var cccd = $"0{faker.Random.Int(10, 99):D2}{faker.Random.Int(2002, 2006):D4}{faker.Random.Int(100000, 999999):D6}";
                var province = faker.PickRandom(provinces);
                var priority = faker.Random.Bool(0.25f) ? faker.PickRandom(priorities.Where(p => p.DiemCong > 0).ToList()) : priorities.First(p => p.DiemCong == 0);

                var sv = new SinhVien
                {
                    MSSV = mssv,
                    HoTen = fullName,
                    GioiTinh = gender,
                    NgaySinh = faker.Date.Between(new DateTime(2002, 1, 1), new DateTime(2006, 12, 31)),
                    CCCD = cccd,
                    Khoa = faker.PickRandom(faculties),
                    KhoaHoc = yearOfStudy,
                    SoDienThoai = $"09{faker.Random.Int(10000000, 99999999)}",
                    Email = $"{mssv.ToLower()}@ptit.edu.vn",
                    TinhThanhId = province.Id,
                    DienUuTienId = priority.Id
                };
                studentList.Add(sv);
            }

            db.SinhViens.AddRange(studentList);
            await db.SaveChangesAsync();

            // 8. Seed Đợt đăng ký & 500 Đơn đăng ký (gồm cả nhóm bạn)
            var regRound = new DotDangKy
            {
                Ten = "Đợt 1 - Học kỳ I Năm học 2026 - 2027",
                NgayMo = DateTime.Now.AddDays(-15),
                NgayDong = DateTime.Now.AddDays(15),
                DangMo = true
            };
            db.DotDangKys.Add(regRound);
            await db.SaveChangesAsync();

            var roomTypes = await db.LoaiPhongs.ToListAsync();
            var bldgs = await db.ToaNhas.ToListAsync();

            // Tạo 15 nhóm bạn (mỗi nhóm 3-4 bạn cùng giới tính)
            var groupList = new List<NhomDangKy>();

            for (int g = 1; g <= 15; g++)
            {
                var groupGender = (g % 2 == 1) ? GioiTinh.Nam : GioiTinh.Nu;
                var groupMembers = studentList.Where(s => s.GioiTinh == groupGender).Skip(g * 4).Take(4).ToList();
                if (groupMembers.Count >= 3)
                {
                    var group = new NhomDangKy
                    {
                        MaNhom = $"NHOM_{g:D3}",
                        MSSVTruongNhom = groupMembers[0].MSSV
                    };
                    groupList.Add(group);
                    db.NhomDangKys.Add(group);
                }
            }
            await db.SaveChangesAsync();

            // Tạo 500 đơn đăng ký
            var applications = new List<DonDangKy>();
            var appliedStudents = new HashSet<string>();

            // Gắn đơn cho các nhóm bạn trước
            foreach (var g in groupList)
            {
                var members = studentList.Where(s => s.GioiTinh == (g.Id % 2 == 1 ? GioiTinh.Nam : GioiTinh.Nu))
                                         .Skip(g.Id * 4).Take(4).ToList();
                var preferredType = roomTypes[g.Id % 3];
                var preferredBuilding = bldgs.First(b => b.GioiTinh == members[0].GioiTinh);

                foreach (var m in members)
                {
                    appliedStudents.Add(m.MSSV);
                    var priorityScore = (m.DienUuTien?.DiemCong ?? 0) + (m.KhoaHoc == 68 ? 10 : 0) + (m.TinhThanh?.NhomKhoangCach == 3 ? 15 : 5);
                    applications.Add(new DonDangKy
                    {
                        MSSV = m.MSSV,
                        DotId = regRound.Id,
                        NhomId = g.Id,
                        LoaiPhongMongMuonId = preferredType.Id,
                        BatBuocLoaiPhong = false,
                        ToaMongMuonId = preferredBuilding.Id,
                        TangMongMuon = (g.Id % 5) + 1,
                        DiemUuTien = priorityScore,
                        ThoiGianNop = DateTime.Now.AddDays(-faker.Random.Int(1, 10)),
                        TrangThai = TrangThaiDonDangKy.ChoXep
                    });
                }
            }

            // Tạo các đơn đăng ký lẻ cho đủ 500 đơn
            foreach (var s in studentList)
            {
                if (applications.Count >= 500) break;
                if (appliedStudents.Contains(s.MSSV)) continue;

                appliedStudents.Add(s.MSSV);
                var preferredBuilding = bldgs.First(b => b.GioiTinh == s.GioiTinh);
                var priorityScore = (s.DienUuTien?.DiemCong ?? 0) + (s.KhoaHoc == 68 ? 10 : 0) + (s.TinhThanh?.NhomKhoangCach == 3 ? 15 : 5);

                applications.Add(new DonDangKy
                {
                    MSSV = s.MSSV,
                    DotId = regRound.Id,
                    NhomId = null,
                    LoaiPhongMongMuonId = faker.PickRandom(roomTypes).Id,
                    BatBuocLoaiPhong = faker.Random.Bool(0.3f),
                    ToaMongMuonId = preferredBuilding.Id,
                    TangMongMuon = faker.Random.Int(1, 5),
                    DiemUuTien = priorityScore,
                    ThoiGianNop = DateTime.Now.AddDays(-faker.Random.Int(1, 12)),
                    TrangThai = TrangThaiDonDangKy.ChoXep
                });
            }

            db.DonDangKys.AddRange(applications);
            await db.SaveChangesAsync();

            // 9. Seed 3 tháng chỉ số điện nước mẫu (Tháng 8, 9, 10)
            var activeRooms = await db.Phongs.Where(p => p.TrangThai == TrangThaiPhong.HoatDong).Take(30).ToListAsync();
            foreach (var r in activeRooms)
            {
                // Tháng 8
                db.ChiSoDienNuocs.Add(new ChiSoDienNuoc
                {
                    PhongId = r.Id,
                    Thang = 8,
                    Nam = 2026,
                    DienCu = 100,
                    DienMoi = 280,
                    SanLuongDien = 180,
                    NuocCu = 50,
                    NuocMoi = 68,
                    SanLuongNuoc = 18
                });

                // Tháng 9 (Phòng A101 và C101 có thay đồng hồ để demo tính năng đặc biệt)
                bool isReplaced = (r.MaPhong == "A101" || r.MaPhong == "C101");
                db.ChiSoDienNuocs.Add(new ChiSoDienNuoc
                {
                    PhongId = r.Id,
                    Thang = 9,
                    Nam = 2026,
                    DienCu = 280,
                    DienMoi = isReplaced ? 25 : 470,
                    CoThayDongHoDien = isReplaced,
                    DienChotDongHoCu = isReplaced ? 300 : null,
                    DienDauDongHoMoi = isReplaced ? 0 : null,
                    SanLuongDien = isReplaced ? ((300 - 280) + (25 - 0)) : (470 - 280),
                    NuocCu = 68,
                    NuocMoi = 85,
                    SanLuongNuoc = 17,
                    GhiChu = isReplaced ? "Đồng hồ điện cũ chập cháy, thay mới ngày 15/09" : null
                });

                // Tháng 10
                decimal prevElectric = isReplaced ? 25 : 470;
                db.ChiSoDienNuocs.Add(new ChiSoDienNuoc
                {
                    PhongId = r.Id,
                    Thang = 10,
                    Nam = 2026,
                    DienCu = prevElectric,
                    DienMoi = prevElectric + 195,
                    SanLuongDien = 195,
                    NuocCu = 85,
                    NuocMoi = 104,
                    SanLuongNuoc = 19
                });
            }
            await db.SaveChangesAsync();
        }

        // 10. Seed Hợp đồng thực tế cho sinh viên đang lưu trú (~480 sinh viên / 720 chỗ = ~67% lấp đầy)
        if (!await db.HopDongs.AnyAsync())
        {
            var allStudents = await db.SinhViens.ToListAsync();
            var maleStudents = allStudents.Where(s => s.GioiTinh == GioiTinh.Nam).ToList();
            var femaleStudents = allStudents.Where(s => s.GioiTinh == GioiTinh.Nu).ToList();

            var rooms = await db.Phongs
                .Include(p => p.LoaiPhong)
                .Include(p => p.ToaNha)
                .Where(p => p.TrangThai == TrangThaiPhong.HoatDong)
                .ToListAsync();

            var maleRooms = rooms.Where(r => r.ToaNha.GioiTinh == GioiTinh.Nam).OrderBy(r => r.MaPhong).ToList();
            var femaleRooms = rooms.Where(r => r.ToaNha.GioiTinh == GioiTinh.Nu).OrderBy(r => r.MaPhong).ToList();

            var contractList = new List<HopDong>();
            var allocatedMssvs = new HashSet<string>();

            int maleIdx = 0;
            // Xếp sinh viên Nam vào Tòa A và Tòa B (lấp đầy ~65-70%)
            foreach (var r in maleRooms)
            {
                int bedsToFill = (int)Math.Round(r.LoaiPhong.SucChua * 0.68);
                for (int b = 0; b < bedsToFill && maleIdx < maleStudents.Count - 80; b++)
                {
                    var sv = maleStudents[maleIdx++];
                    allocatedMssvs.Add(sv.MSSV);

                    // 14 hợp đồng sắp hết hạn trong 30 ngày tới để kích hoạt cảnh báo Dashboard
                    bool expiringSoon = contractList.Count < 14;
                    var endDate = expiringSoon ? DateTime.Today.AddDays(7 + (contractList.Count % 18)) : new DateTime(2027, 6, 30);

                    contractList.Add(new HopDong
                    {
                        MSSV = sv.MSSV,
                        PhongId = r.Id,
                        NgayBatDau = new DateTime(2026, 9, 1),
                        NgayKetThuc = endDate,
                        GiaThang = r.LoaiPhong.GiaThang,
                        MienGiamPhanTram = (sv.DienUuTienId != null && sv.DienUuTienId > 1) ? 15 : 0,
                        TienCoc = r.LoaiPhong.GiaThang,
                        TrangThai = TrangThaiHopDong.HieuLuc
                    });
                }
            }

            int femaleIdx = 0;
            // Xếp sinh viên Nữ vào Tòa C và Tòa D (lấp đầy ~68-75%)
            foreach (var r in femaleRooms)
            {
                int bedsToFill = (int)Math.Round(r.LoaiPhong.SucChua * 0.70);
                for (int b = 0; b < bedsToFill && femaleIdx < femaleStudents.Count - 40; b++)
                {
                    var sv = femaleStudents[femaleIdx++];
                    allocatedMssvs.Add(sv.MSSV);

                    contractList.Add(new HopDong
                    {
                        MSSV = sv.MSSV,
                        PhongId = r.Id,
                        NgayBatDau = new DateTime(2026, 9, 1),
                        NgayKetThuc = new DateTime(2027, 6, 30),
                        GiaThang = r.LoaiPhong.GiaThang,
                        MienGiamPhanTram = (sv.DienUuTienId != null && sv.DienUuTienId > 1) ? 15 : 0,
                        TienCoc = r.LoaiPhong.GiaThang,
                        TrangThai = TrangThaiHopDong.HieuLuc
                    });
                }
            }

            db.HopDongs.AddRange(contractList);

            // Cập nhật các đơn đăng ký của sinh viên đã có phòng sang Đã Xếp (DaXep)
            var applicationsToUpdate = await db.DonDangKys
                .Where(d => allocatedMssvs.Contains(d.MSSV))
                .ToListAsync();

            foreach (var app in applicationsToUpdate)
            {
                app.TrangThai = TrangThaiDonDangKy.DaXep;
            }

            await db.SaveChangesAsync();
        }

        // 11. Seed Yêu cầu sửa chữa & Bảo trì
        if (!await db.YeuCauSuaChuas.AnyAsync())
        {
            var roomList = await db.Phongs.Take(25).ToListAsync();
            db.YeuCauSuaChuas.AddRange(
                new YeuCauSuaChua { PhongId = roomList[0].Id, MoTa = "Rò rỉ vòi nước chậu rửa mặt phòng tắm", TrangThai = "DangXuLy", NgayTao = DateTime.Now.AddDays(-3), ChiPhi = 0 },
                new YeuCauSuaChua { PhongId = roomList[1].Id, MoTa = "Điều hòa Daikin rung lắc và phát tiếng ồn lớn", TrangThai = "ChuaXuLy", NgayTao = DateTime.Now.AddDays(-1), ChiPhi = 0 },
                new YeuCauSuaChua { PhongId = roomList[2].Id, MoTa = "Chập cháy bóng đèn tuýp LED chiếu sáng bàn học", TrangThai = "ChuaXuLy", NgayTao = DateTime.Now.AddDays(-2), ChiPhi = 0 },
                new YeuCauSuaChua { PhongId = roomList[3].Id, MoTa = "Khóa cửa phòng bị kẹt ổ bi khó cắm chìa", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-8), NgayHoanThanh = DateTime.Now.AddDays(-5), ChiPhi = 150000, GhiChu = "Đã thay ổ khóa Việt Tiệp mới" },
                new YeuCauSuaChua { PhongId = roomList[4].Id, MoTa = "Thay vòi hoa sen tăng áp bị nứt vỡ", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-14), NgayHoanThanh = DateTime.Now.AddDays(-10), ChiPhi = 220000, GhiChu = "Đã thay mới linh kiện chính hãng" },
                new YeuCauSuaChua { PhongId = roomList[5].Id, MoTa = "Thông tắc đường ống thoát sàn ban công", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-18), NgayHoanThanh = DateTime.Now.AddDays(-12), ChiPhi = 180000 },
                new YeuCauSuaChua { PhongId = roomList[6].Id, MoTa = "Thay thế ổ cắm điện âm tường bị lỏng chân", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-20), NgayHoanThanh = DateTime.Now.AddDays(-15), ChiPhi = 95000 },
                new YeuCauSuaChua { PhongId = roomList[7].Id, MoTa = "Gia cố bản lề tủ cá nhân sinh viên giường số 3", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-25), NgayHoanThanh = DateTime.Now.AddDays(-20), ChiPhi = 120000 }
            );
            await db.SaveChangesAsync();
        }

        // 12. Seed Doanh thu 6 tháng gần nhất (Tiền phòng & Tiền điện nước từ Tháng 5 đến Tháng 10)
        var contracts = await db.HopDongs.ToListAsync();
        int[] months = { 5, 6, 7, 8, 9, 10 };

        if (await db.HoaDonDienNuocs.CountAsync() < 10)
        {
            var activeRooms = await db.Phongs.Include(p => p.LoaiPhong).Where(p => p.TrangThai == TrangThaiPhong.HoatDong).Take(30).ToListAsync();

            // 12.1 Hóa đơn điện nước 6 tháng
            foreach (var m in months)
            {
                foreach (var r in activeRooms)
                {
                    decimal billAmount = 650000 + ((r.Id * 41000 + m * 55000) % 550000);
                    bool isPaid = (m < 10) || (r.Id % 6 != 0); // Tháng 10 có 1 vài hóa đơn chưa đóng để tạo cảnh báo

                    var invoice = new HoaDonDienNuoc
                    {
                        MaThanhToan = $"KTXDN_{r.MaPhong}_{m:D2}26",
                        PhongId = r.Id,
                        Thang = m,
                        Nam = 2026,
                        SoNguoiTinhDinhMuc = r.LoaiPhong.SucChua,
                        TongTien = billAmount,
                        DaThu = isPaid ? billAmount : 0,
                        TrangThai = isPaid ? TrangThaiHoaDon.DaThanhToan : TrangThaiHoaDon.ChuaThanhToan
                    };

                    invoice.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        NoiDung = $"Tiền điện sinh hoạt Tháng {m}/2026",
                        SoLuong = 180,
                        DonGia = 2200,
                        ThanhTien = billAmount * 0.75m
                    });
                    invoice.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        NoiDung = $"Tiền nước sinh hoạt Tháng {m}/2026",
                        SoLuong = 16,
                        DonGia = 11000,
                        ThanhTien = billAmount * 0.25m
                    });

                    db.HoaDonDienNuocs.Add(invoice);
                }
            }
            await db.SaveChangesAsync();
        }

        // 12.2 Khoản thu tiền phòng 6 tháng (tạo khoản thu phòng theo hợp đồng)
        if (!await db.KhoanThuTienPhongs.AnyAsync() && contracts.Count > 0)
        {
            var feeList = new List<KhoanThuTienPhong>();
            foreach (var m in months)
            {
                // Chọn 120 hợp đồng mỗi tháng để tạo khoản thu
                foreach (var c in contracts.Take(120))
                {
                    decimal fee = c.GiaThang * (100 - c.MienGiamPhanTram) / 100m;
                    bool isPaid = (m < 10) || (c.Id % 8 != 0);

                    feeList.Add(new KhoanThuTienPhong
                    {
                        HopDongId = c.Id,
                        MaThanhToan = $"KTXTP_{c.Id:D4}_{m:D2}26",
                        TuNgay = new DateTime(2026, m, 1),
                        DenNgay = new DateTime(2026, m, 1).AddMonths(1).AddDays(-1),
                        SoTienGoc = c.GiaThang,
                        MienGiam = c.GiaThang * c.MienGiamPhanTram / 100m,
                        PhaiThu = fee,
                        DaThu = isPaid ? fee : 0,
                        TrangThai = isPaid ? TrangThaiHoaDon.DaThanhToan : TrangThaiHoaDon.ChuaThanhToan
                    });
                }
            }
            db.KhoanThuTienPhongs.AddRange(feeList);
            await db.SaveChangesAsync();
        }
    }
}
