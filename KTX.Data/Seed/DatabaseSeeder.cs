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

        // 1.1 Đảm bảo tồn tại vai trò SinhVien & tài khoản sinh viên mẫu (tương thích cả CSDL đã tạo trước đó)
        var studentRole = await db.VaiTros.FirstOrDefaultAsync(v => v.TenVaiTro == "SinhVien");
        if (studentRole == null)
        {
            studentRole = new VaiTro { TenVaiTro = "SinhVien" };
            db.VaiTros.Add(studentRole);
            await db.SaveChangesAsync();
        }

        if (!await db.NguoiDungs.AnyAsync(u => u.TenDangNhap == "sinhvien"))
        {
            db.NguoiDungs.Add(new NguoiDung
            {
                TenDangNhap = "sinhvien",
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword("sinhvien123"),
                HoTen = "Sinh viên Nội trú KTX",
                MaVaiTro = studentRole.Id,
                HoatDong = true
            });
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
        }

        var curMonthDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        // 9. Seed chỉ số điện nước cho 3 tháng gần nhất (toàn bộ phòng hoạt động, tự động bù tháng mới)
        {
            var activeRooms = await db.Phongs
                .Include(p => p.LoaiPhong)
                .Where(p => p.TrangThai == TrangThaiPhong.HoatDong)
                .OrderBy(p => p.Id)
                .ToListAsync();

            var meterMonths = new[]
            {
                curMonthDate.AddMonths(-2),
                curMonthDate.AddMonths(-1),
                curMonthDate
            };

            var abnormalRoomCodes = new HashSet<string> { "A102", "A203", "B104", "C103", "C202", "D105" };
            var replacedRoomCodes = new HashSet<string> { "A101", "B201", "C101" };

            for (int mi = 0; mi < meterMonths.Length; mi++)
            {
                int m = meterMonths[mi].Month;
                int y = meterMonths[mi].Year;

                if (await db.ChiSoDienNuocs.AnyAsync(c => c.Thang == m && c.Nam == y))
                    continue;

                var prevDt = meterMonths[mi].AddMonths(-1);
                var prevMap = await db.ChiSoDienNuocs.AsNoTracking()
                    .Where(c => c.Thang == prevDt.Month && c.Nam == prevDt.Year)
                    .ToDictionaryAsync(c => c.PhongId);

                var batchReadings = new List<ChiSoDienNuoc>();
                foreach (var r in activeRooms)
                {
                    int cap = r.LoaiPhong?.SucChua ?? 6;
                    decimal baseElec = cap == 4 ? 145m : (cap == 6 ? 195m : 245m);
                    decimal deltaElec = baseElec + ((r.Id * 13 + m * 17) % 45);
                    decimal deltaWater = (cap * 2.6m) + ((r.Id * 3 + m) % 6);

                    decimal prevE = prevMap.TryGetValue(r.Id, out var pr) ? pr.DienMoi : (120m + (r.Id % 50) * 10m);
                    decimal prevW = prevMap.TryGetValue(r.Id, out var prw) ? prw.NuocMoi : (40m + (r.Id % 30) * 3m);

                    // Tháng giữa: mô phỏng thay đồng hồ điện cho 3 phòng mẫu
                    bool isReplaced = (mi == 1 && replacedRoomCodes.Contains(r.MaPhong));
                    // Tháng hiện tại: mô phỏng tiêu thụ điện đột biến (>1.7x trung bình) cho 6 phòng để kích hoạt cảnh báo
                    bool isSpike = (mi == 2 && abnormalRoomCodes.Contains(r.MaPhong));
                    if (isSpike) deltaElec = Math.Round(deltaElec * 1.85m, 0);

                    if (isReplaced)
                    {
                        decimal chotCu = prevE + 85m;
                        decimal moiAfter = 95m;
                        batchReadings.Add(new ChiSoDienNuoc
                        {
                            PhongId = r.Id,
                            Thang = m,
                            Nam = y,
                            DienCu = prevE,
                            DienMoi = moiAfter,
                            CoThayDongHoDien = true,
                            DienChotDongHoCu = chotCu,
                            DienDauDongHoMoi = 0,
                            SanLuongDien = (chotCu - prevE) + moiAfter,
                            NuocCu = prevW,
                            NuocMoi = prevW + deltaWater,
                            SanLuongNuoc = deltaWater,
                            GhiChu = $"Thay công tơ điện tử mới ngày 15/{m:D2}/{y}"
                        });
                    }
                    else
                    {
                        batchReadings.Add(new ChiSoDienNuoc
                        {
                            PhongId = r.Id,
                            Thang = m,
                            Nam = y,
                            DienCu = prevE,
                            DienMoi = prevE + deltaElec,
                            SanLuongDien = deltaElec,
                            NuocCu = prevW,
                            NuocMoi = prevW + deltaWater,
                            SanLuongNuoc = deltaWater,
                            GhiChu = isSpike ? "Kiểm tra điều hòa/thiết bị tiêu thụ cao bất thường" : null
                        });
                    }
                }

                db.ChiSoDienNuocs.AddRange(batchReadings);
                await db.SaveChangesAsync();
            }
        }

        // 10. Seed Hợp đồng thực tế cho sinh viên đang lưu trú (~480 sinh viên / 720 chỗ = ~67% lấp đầy)
        //     Đa dạng trạng thái phòng: Đầy 100% (Đỏ), Còn chỗ (Cam), Trống 100% (Xanh) + Lịch sử đã trả phòng
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
            var startDate = curMonthDate.AddMonths(-2);
            var normalEndDate = curMonthDate.AddMonths(8).AddDays(-1);

            int maleIdx = 0;
            for (int idx = 0; idx < maleRooms.Count; idx++)
            {
                var r = maleRooms[idx];
                // Tạo bức tranh đa dạng trên mỗi tầng: 2 phòng đầy 100%, 3 phòng còn chỗ, 1 phòng trống 100%
                int mod = idx % 6;
                int bedsToFill = mod switch
                {
                    0 or 1 => r.LoaiPhong.SucChua, // Đầy 100% (Đỏ)
                    5 => 0,                         // Trống 100% (Xanh)
                    _ => Math.Max(1, (int)Math.Round(r.LoaiPhong.SucChua * 0.65)) // Còn chỗ (Cam)
                };

                for (int b = 0; b < bedsToFill && maleIdx < maleStudents.Count - 55; b++)
                {
                    var sv = maleStudents[maleIdx++];
                    allocatedMssvs.Add(sv.MSSV);

                    // 14 hợp đồng sắp hết hạn trong 30 ngày tới để kích hoạt cảnh báo Dashboard
                    bool expiringSoon = contractList.Count < 14;
                    var endDate = expiringSoon ? DateTime.Today.AddDays(5 + (contractList.Count % 20)) : normalEndDate;

                    contractList.Add(new HopDong
                    {
                        MSSV = sv.MSSV,
                        PhongId = r.Id,
                        NgayBatDau = startDate,
                        NgayKetThuc = endDate,
                        GiaThang = r.LoaiPhong.GiaThang,
                        MienGiamPhanTram = (sv.DienUuTienId != null && sv.DienUuTienId > 1 && sv.DienUuTienId < 5) ? 15 : 0,
                        TienCoc = r.LoaiPhong.GiaThang,
                        TrangThai = TrangThaiHopDong.HieuLuc
                    });
                }
            }

            int femaleIdx = 0;
            for (int idx = 0; idx < femaleRooms.Count; idx++)
            {
                var r = femaleRooms[idx];
                int mod = idx % 6;
                int bedsToFill = mod switch
                {
                    0 or 1 => r.LoaiPhong.SucChua,
                    5 => 0,
                    _ => Math.Max(1, (int)Math.Round(r.LoaiPhong.SucChua * 0.68))
                };

                for (int b = 0; b < bedsToFill && femaleIdx < femaleStudents.Count - 35; b++)
                {
                    var sv = femaleStudents[femaleIdx++];
                    allocatedMssvs.Add(sv.MSSV);

                    contractList.Add(new HopDong
                    {
                        MSSV = sv.MSSV,
                        PhongId = r.Id,
                        NgayBatDau = startDate,
                        NgayKetThuc = normalEndDate,
                        GiaThang = r.LoaiPhong.GiaThang,
                        MienGiamPhanTram = (sv.DienUuTienId != null && sv.DienUuTienId > 1 && sv.DienUuTienId < 5) ? 15 : 0,
                        TienCoc = r.LoaiPhong.GiaThang,
                        TrangThai = TrangThaiHopDong.HieuLuc
                    });
                }
            }

            // Thêm 16 hợp đồng lịch sử đã trả phòng (DaTra) để tab Lọc Hợp đồng phong phú
            var unallocated = allStudents.Where(s => !allocatedMssvs.Contains(s.MSSV)).Take(16).ToList();
            foreach (var sv in unallocated)
            {
                var pool = sv.GioiTinh == GioiTinh.Nam ? maleRooms : femaleRooms;
                var r = pool[Math.Abs(sv.MSSV.GetHashCode()) % pool.Count];
                contractList.Add(new HopDong
                {
                    MSSV = sv.MSSV,
                    PhongId = r.Id,
                    NgayBatDau = curMonthDate.AddMonths(-8),
                    NgayKetThuc = curMonthDate.AddMonths(-2).AddDays(-1),
                    GiaThang = r.LoaiPhong.GiaThang,
                    MienGiamPhanTram = 0,
                    TienCoc = r.LoaiPhong.GiaThang,
                    TrangThai = TrangThaiHopDong.DaTra
                });
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

        // 11. Seed 18 Yêu cầu sửa chữa & Bảo trì phong phú trải đều 4 tòa
        if (!await db.YeuCauSuaChuas.AnyAsync())
        {
            var allRoomsList = await db.Phongs.OrderBy(p => p.ToaId).ThenBy(p => p.MaPhong).ToListAsync();
            int PickRoom(int idx) => allRoomsList[idx % allRoomsList.Count].Id;

            db.YeuCauSuaChuas.AddRange(
                new YeuCauSuaChua { PhongId = PickRoom(0), MoTa = "Rò rỉ vòi nước chậu rửa mặt phòng tắm", TrangThai = "DangXuLy", NgayTao = DateTime.Now.AddDays(-2), ChiPhi = 0, GhiChu = "Kỹ thuật đang kiểm tra gioăng cao su" },
                new YeuCauSuaChua { PhongId = PickRoom(2), MoTa = "Điều hòa Daikin rung lắc và phát tiếng ồn lớn khi chạy chế độ Cool", TrangThai = "ChuaXuLy", NgayTao = DateTime.Now.AddDays(-1), ChiPhi = 0 },
                new YeuCauSuaChua { PhongId = PickRoom(31), MoTa = "Chập cháy 2 bóng đèn tuýp LED chiếu sáng khu vực bàn học", TrangThai = "ChuaXuLy", NgayTao = DateTime.Now.AddDays(-1), ChiPhi = 0 },
                new YeuCauSuaChua { PhongId = PickRoom(62), MoTa = "Bình nóng lạnh Ariston không sáng đèn báo nguồn", TrangThai = "DangXuLy", NgayTao = DateTime.Now.AddDays(-3), ChiPhi = 0, GhiChu = "Đã kiểm tra aptomat chống giật" },
                new YeuCauSuaChua { PhongId = PickRoom(93), MoTa = "Cửa ban công bị xệ bản lề không đóng kín được", TrangThai = "ChuaXuLy", NgayTao = DateTime.Now.AddHours(-12), ChiPhi = 0 },
                new YeuCauSuaChua { PhongId = PickRoom(35), MoTa = "Quạt trần quay chậm, kêu lạch cạch ở tốc độ cao", TrangThai = "DangXuLy", NgayTao = DateTime.Now.AddDays(-2), ChiPhi = 0, GhiChu = "Chờ thay tụ điện quạt" },
                new YeuCauSuaChua { PhongId = PickRoom(65), MoTa = "Vỡ nắp bồn cầu khu vệ sinh chung của phòng", TrangThai = "ChuaXuLy", NgayTao = DateTime.Now.AddHours(-6), ChiPhi = 0 },

                // Đã hoàn thành trong tháng hiện tại (đóng góp chi phí vào Báo cáo Tài chính tháng)
                new YeuCauSuaChua { PhongId = PickRoom(4), MoTa = "Khóa cửa phòng bị kẹt ổ bi khó cắm chìa", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-6), NgayHoanThanh = DateTime.Now.AddDays(-4), ChiPhi = 185000, GhiChu = "Đã thay ổ khóa Việt Tiệp chính hãng" },
                new YeuCauSuaChua { PhongId = PickRoom(33), MoTa = "Thay vòi hoa sen tăng áp bị nứt vỡ dây dẫn", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-9), NgayHoanThanh = DateTime.Now.AddDays(-7), ChiPhi = 240000, GhiChu = "Đã thay bộ sen vòi Inox 304" },
                new YeuCauSuaChua { PhongId = PickRoom(61), MoTa = "Thông tắc đường ống thoát sàn ban công tầng 2", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-11), NgayHoanThanh = DateTime.Now.AddDays(-10), ChiPhi = 200000, GhiChu = "Đã xử lý triệt để bằng máy lò xo" },
                new YeuCauSuaChua { PhongId = PickRoom(91), MoTa = "Thay thế 3 ổ cắm điện âm tường bị lỏng chân tiếp xúc", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-12), NgayHoanThanh = DateTime.Now.AddDays(-10), ChiPhi = 145000, GhiChu = "Thay ổ cắm Sino mới đảm bảo PCCC" },
                new YeuCauSuaChua { PhongId = PickRoom(10), MoTa = "Gia cố bản lề tủ cá nhân sinh viên giường số 3 và số 4", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-14), NgayHoanThanh = DateTime.Now.AddDays(-12), ChiPhi = 160000, GhiChu = "Đã bắt vít gia cố chắc chắn" },
                new YeuCauSuaChua { PhongId = PickRoom(40), MoTa = "Nạp gas bổ sung và vệ sinh lưới lọc điều hòa định kỳ", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-15), NgayHoanThanh = DateTime.Now.AddDays(-13), ChiPhi = 450000, GhiChu = "Gas R32 bổ sung, hoạt động mát sâu" },
                new YeuCauSuaChua { PhongId = PickRoom(70), MoTa = "Thay thế lưới chống muỗi cửa sổ bị rách", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-16), NgayHoanThanh = DateTime.Now.AddDays(-14), ChiPhi = 175000 },
                new YeuCauSuaChua { PhongId = PickRoom(100), MoTa = "Sửa chữa giát giường tầng 2 bị gãy nan gỗ", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-18), NgayHoanThanh = DateTime.Now.AddDays(-16), ChiPhi = 210000, GhiChu = "Đã thay mới 4 thanh giát gỗ keo" },
                new YeuCauSuaChua { PhongId = PickRoom(15), MoTa = "Thay bóng đèn hành lang cảm biến chuyển động", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-22), NgayHoanThanh = DateTime.Now.AddDays(-20), ChiPhi = 130000 },
                new YeuCauSuaChua { PhongId = PickRoom(48), MoTa = "Thay van khóa tổng đường nước lạnh phòng vệ sinh", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-28), NgayHoanThanh = DateTime.Now.AddDays(-26), ChiPhi = 190000 },
                new YeuCauSuaChua { PhongId = PickRoom(80), MoTa = "Sơn dặm lại mảng tường bong tróc gần cửa sổ", TrangThai = "HoanThanh", NgayTao = DateTime.Now.AddDays(-35), NgayHoanThanh = DateTime.Now.AddDays(-32), ChiPhi = 320000 }
            );
            await db.SaveChangesAsync();
        }

        // 12. Seed Doanh thu 6 tháng gần nhất (động theo DateTime.Today: Tiền phòng & Điện nước & ThanhToan)
        var activeContracts = await db.HopDongs.Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc).OrderBy(h => h.Id).ToListAsync();
        var billingRooms = await db.Phongs
            .Include(p => p.LoaiPhong)
            .Where(p => p.TrangThai == TrangThaiPhong.HoatDong)
            .OrderBy(p => p.ToaId).ThenBy(p => p.MaPhong)
            .Take(60)
            .ToListAsync();

        for (int offset = 5; offset >= 0; offset--)
        {
            var dt = curMonthDate.AddMonths(-offset);
            int m = dt.Month;
            int y = dt.Year;
            bool isCurrentMonth = (offset == 0);

            // 12.1 Hóa đơn điện nước theo từng tháng (tự động bù nếu sang tháng mới)
            if (!await db.HoaDonDienNuocs.AnyAsync(h => h.Thang == m && h.Nam == y))
            {
                var newInvoices = new List<HoaDonDienNuoc>();
                foreach (var r in billingRooms)
                {
                    int cap = r.LoaiPhong?.SucChua ?? 6;
                    decimal baseBill = cap == 4 ? 520000m : (cap == 6 ? 760000m : 980000m);
                    decimal billAmount = baseBill + ((r.Id * 37000 + m * 43000) % 320000);
                    string payCode = $"KTXDN_{r.MaPhong}_{m:D2}{y % 100:D2}";

                    TrangThaiHoaDon status;
                    decimal paid;
                    if (!isCurrentMonth)
                    {
                        status = TrangThaiHoaDon.DaThanhToan;
                        paid = billAmount;
                    }
                    else if (r.Id % 7 == 0)
                    {
                        status = TrangThaiHoaDon.ChuaThanhToan;
                        paid = 0;
                    }
                    else if (r.Id % 9 == 0)
                    {
                        status = TrangThaiHoaDon.ThanhToanMotPhan;
                        paid = Math.Round(billAmount * 0.5m / 1000m, 0) * 1000m;
                    }
                    else
                    {
                        status = TrangThaiHoaDon.DaThanhToan;
                        paid = billAmount;
                    }

                    var invoice = new HoaDonDienNuoc
                    {
                        MaThanhToan = payCode,
                        PhongId = r.Id,
                        Thang = m,
                        Nam = y,
                        SoNguoiTinhDinhMuc = cap,
                        TongTien = billAmount,
                        DaThu = paid,
                        TrangThai = status
                    };

                    decimal elecPart = Math.Round(billAmount * 0.76m, 0);
                    decimal waterPart = billAmount - elecPart;
                    invoice.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        NoiDung = $"Tiền điện sinh hoạt bậc thang T{m}/{y}",
                        SoLuong = Math.Round(elecPart / 2300m, 0),
                        DonGia = 2300,
                        ThanhTien = elecPart
                    });
                    invoice.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        NoiDung = $"Tiền nước sinh hoạt định mức T{m}/{y}",
                        SoLuong = Math.Round(waterPart / 10500m, 0),
                        DonGia = 10500,
                        ThanhTien = waterPart
                    });

                    newInvoices.Add(invoice);
                }

                db.HoaDonDienNuocs.AddRange(newInvoices);
                await db.SaveChangesAsync();
            }

            // 12.2 Khoản thu tiền phòng theo từng tháng (160 hợp đồng / tháng)
            if (activeContracts.Count > 0 && !await db.KhoanThuTienPhongs.AnyAsync(k => k.TuNgay.Month == m && k.TuNgay.Year == y))
            {
                var feeList = new List<KhoanThuTienPhong>();
                foreach (var c in activeContracts.Take(160))
                {
                    decimal fee = c.GiaThang * (100 - c.MienGiamPhanTram) / 100m;
                    TrangThaiHoaDon status;
                    decimal paid;
                    if (!isCurrentMonth)
                    {
                        status = TrangThaiHoaDon.DaThanhToan;
                        paid = fee;
                    }
                    else if (c.Id % 8 == 0)
                    {
                        status = TrangThaiHoaDon.ChuaThanhToan;
                        paid = 0;
                    }
                    else if (c.Id % 13 == 0)
                    {
                        status = TrangThaiHoaDon.ThanhToanMotPhan;
                        paid = Math.Round(fee * 0.5m / 1000m, 0) * 1000m;
                    }
                    else
                    {
                        status = TrangThaiHoaDon.DaThanhToan;
                        paid = fee;
                    }

                    feeList.Add(new KhoanThuTienPhong
                    {
                        HopDongId = c.Id,
                        MaThanhToan = $"KTXTP_{c.Id:D4}_{m:D2}{y % 100:D2}",
                        TuNgay = new DateTime(y, m, 1),
                        DenNgay = new DateTime(y, m, 1).AddMonths(1).AddDays(-1),
                        SoTienGoc = c.GiaThang,
                        MienGiam = c.GiaThang * c.MienGiamPhanTram / 100m,
                        PhaiThu = fee,
                        DaThu = paid,
                        TrangThai = status
                    });
                }

                db.KhoanThuTienPhongs.AddRange(feeList);
                await db.SaveChangesAsync();
            }
        }

        // 13. Seed Giao dịch sao kê ngân hàng mẫu phong phú (30 giao dịch: Đã khớp / Một phần / Cần xử lý)
        if (!await db.GiaoDichNganHangs.AnyAsync())
        {
            var txList = new List<GiaoDichNganHang>();
            decimal balance = 245000000m;
            int txIndex = 1;
            string monthTag = $"{curMonthDate.Month:D2}_{curMonthDate.Year}";

            // 13.1 Giao dịch khớp chuẩn hóa đơn điện nước (12 giao dịch)
            var paidInvoices = await db.HoaDonDienNuocs
                .Include(h => h.Phong)
                .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan)
                .OrderByDescending(h => h.Nam).ThenByDescending(h => h.Thang)
                .Take(12)
                .ToListAsync();

            foreach (var inv in paidInvoices)
            {
                balance += inv.TongTien;
                txList.Add(new GiaoDichNganHang
                {
                    NgayGD = DateTime.Now.AddDays(-(txIndex % 10)).AddHours(-2),
                    SoTien = inv.TongTien,
                    NoiDungGoc = $"PHONG {inv.Phong.MaPhong} CK TIEN DIEN NUOC {inv.MaThanhToan}",
                    MaGiaoDich = $"FT{curMonthDate:yyMM}{txIndex:D4}",
                    SoDuSau = balance,
                    HashSaoKe = $"SHA256_DN_{inv.Id}_{txIndex:D4}_A9F2C1",
                    TrangThaiKhop = TrangThaiGiaoDich.DaKhop,
                    FileNguon = $"SaoKe_MBBank_T{monthTag}.xlsx",
                    NgayImport = DateTime.Now.AddDays(-(txIndex % 4))
                });
                txIndex++;
            }

            // 13.2 Giao dịch khớp chuẩn tiền phòng (10 giao dịch)
            var paidFees = await db.KhoanThuTienPhongs
                .Include(k => k.HopDong).ThenInclude(h => h.SinhVien)
                .Where(k => k.TrangThai == TrangThaiHoaDon.DaThanhToan)
                .OrderByDescending(k => k.TuNgay)
                .Take(10)
                .ToListAsync();

            foreach (var fee in paidFees)
            {
                balance += fee.PhaiThu;
                txList.Add(new GiaoDichNganHang
                {
                    NgayGD = DateTime.Now.AddDays(-(txIndex % 8)).AddHours(-5),
                    SoTien = fee.PhaiThu,
                    NoiDungGoc = $"SV {fee.HopDong.SinhVien.MSSV} NOP TIEN PHONG {fee.MaThanhToan}",
                    MaGiaoDich = $"FT{curMonthDate:yyMM}{txIndex:D4}",
                    SoDuSau = balance,
                    HashSaoKe = $"SHA256_TP_{fee.Id}_{txIndex:D4}_B7E4D0",
                    TrangThaiKhop = TrangThaiGiaoDich.DaKhop,
                    FileNguon = $"SaoKe_MBBank_T{monthTag}.xlsx",
                    NgayImport = DateTime.Now.AddDays(-1)
                });
                txIndex++;
            }

            // 13.3 Giao dịch khớp một phần (5 giao dịch)
            var partialInvs = await db.HoaDonDienNuocs
                .Include(h => h.Phong)
                .Where(h => h.TrangThai != TrangThaiHoaDon.DaThanhToan)
                .OrderByDescending(h => h.Nam).ThenByDescending(h => h.Thang)
                .Take(5)
                .ToListAsync();

            foreach (var inv in partialInvs)
            {
                decimal partial = Math.Round(inv.TongTien * 0.5m / 1000m, 0) * 1000m;
                balance += partial;
                txList.Add(new GiaoDichNganHang
                {
                    NgayGD = DateTime.Now.AddDays(-(txIndex % 5)),
                    SoTien = partial,
                    NoiDungGoc = $"PHONG {inv.Phong.MaPhong} DONG TRUOC MOT PHAN {inv.MaThanhToan}",
                    MaGiaoDich = $"FT{curMonthDate:yyMM}{txIndex:D4}",
                    SoDuSau = balance,
                    HashSaoKe = $"SHA256_PART_{inv.Id}_{txIndex:D4}_C3A811",
                    TrangThaiKhop = TrangThaiGiaoDich.MotPhan,
                    FileNguon = $"SaoKe_MBBank_T{monthTag}.xlsx",
                    NgayImport = DateTime.Now
                });
                txIndex++;
            }

            // 13.4 Giao dịch cần kế toán xử lý (3 giao dịch thiếu mã / sai cú pháp)
            var unmatchedSamples = new (decimal Amount, string Memo)[]
            {
                (350000m, "NGUYEN VAN HOANG CHUYEN TIEN PHONG KTX NHUNG QUEN MA"),
                (620000m, "TRAN MINH DUC CK DIEN NUOC THANG NAY"),
                (450000m, "LE THI LAN ANH NOP TIEN KTX KY 1")
            };

            foreach (var u in unmatchedSamples)
            {
                balance += u.Amount;
                txList.Add(new GiaoDichNganHang
                {
                    NgayGD = DateTime.Now.AddHours(-txIndex),
                    SoTien = u.Amount,
                    NoiDungGoc = u.Memo,
                    MaGiaoDich = $"FT{curMonthDate:yyMM}{txIndex:D4}",
                    SoDuSau = balance,
                    HashSaoKe = $"SHA256_UNM_{txIndex:D4}_F912BC",
                    TrangThaiKhop = TrangThaiGiaoDich.CanXuLy,
                    FileNguon = $"SaoKe_MBBank_T{monthTag}.xlsx",
                    NgayImport = DateTime.Now
                });
                txIndex++;
            }

            db.GiaoDichNganHangs.AddRange(txList);
            await db.SaveChangesAsync();
        }
    }
}
