using System;
using System.Linq;
using System.Web.Mvc;
using Project_Cellphoness.Models;

namespace Project_Cellphoness.Areas.Admin.Controllers
{
    public class SanPhamController : BaseAdminController
    {
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        // 1. Hiển thị danh sách sản phẩm trong kho Admin
        public ActionResult Index()
        {
            var dsSanPham = db.SanPham.OrderByDescending(s => s.NgayTao).ToList();
            return View(dsSanPham);
        }

        // 2. GET: Hiển thị form Thêm sản phẩm
        public ActionResult Create()
        {
            // Truy vấn danh sách Hãng để đổ vào Dropdown List ngoại quan
            ViewBag.MaHang = new SelectList(db.HangSanXuat, "MaHang", "TenHang");
            return View();
        }

        // 3. POST: Nhận dữ liệu từ form (Bao gồm cả thông số kỹ thuật) và lưu vào CSDL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanPham sp, string ManHinh, string Chip, string RAM, string ROM, string Camera, string Pin)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Bước A: Lưu thông tin sản phẩm cơ bản trước
                    sp.NgayTao = DateTime.Now;
                    sp.TrangThai = 1; // 1: Đang bán

                    // Xử lý mặc định nếu form không truyền lên SoLuongTon hoặc GiaBan
                    if (sp.SoLuongTon == null) sp.SoLuongTon = 0;

                    // CỐT LÕI: TẮT BỘ KIỂM TRA GIẢ LẬP CỦA ENTITY FRAMEWORK
                    db.Configuration.ValidateOnSaveEnabled = false;

                    db.SanPham.Add(sp);
                    db.SaveChanges(); // SQL Server sẽ tự sinh ra sp.MaSP tại đây

                    // Bước B: Lưu thông số kỹ thuật với MaSP vừa sinh ra
                    ThongSoKyThuat tskt = new ThongSoKyThuat();
                    tskt.MaSP = sp.MaSP;

                    // ==============================================================
                    // TỰ ĐỘNG CẮT TỈA CHUỖI ĐỂ CHỐNG TRÀN BẢNG THÔNG SỐ KỸ THUẬT
                    // Cột ManHinh, Chip, Camera tối đa 100 ký tự
                    // Cột RAM, ROM, Pin tối đa 50 ký tự
                    // ==============================================================
                    tskt.ManHinh = !string.IsNullOrEmpty(ManHinh) && ManHinh.Length > 100 ? ManHinh.Substring(0, 97) + "..." : ManHinh;
                    tskt.Chip = !string.IsNullOrEmpty(Chip) && Chip.Length > 100 ? Chip.Substring(0, 97) + "..." : Chip;
                    tskt.Camera = !string.IsNullOrEmpty(Camera) && Camera.Length > 100 ? Camera.Substring(0, 97) + "..." : Camera;

                    tskt.RAM = !string.IsNullOrEmpty(RAM) && RAM.Length > 50 ? RAM.Substring(0, 47) + "..." : RAM;
                    tskt.ROM = !string.IsNullOrEmpty(ROM) && ROM.Length > 50 ? ROM.Substring(0, 47) + "..." : ROM;
                    tskt.Pin = !string.IsNullOrEmpty(Pin) && Pin.Length > 50 ? Pin.Substring(0, 47) + "..." : Pin;

                    db.ThongSoKyThuat.Add(tskt);
                    db.SaveChanges();

                    return RedirectToAction("Index"); // Thêm xong quay về trang danh sách
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    string loiChiTiet = "";
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                        {
                            loiChiTiet += $"- Cột '{ve.PropertyName}': {ve.ErrorMessage} <br>";
                        }
                    }
                    return Content("<h2 style='color:red;'>LỖI VALIDATION TỪ ENTITY FRAMEWORK:</h2><p>" + loiChiTiet + "</p>");
                }
                catch (Exception ex)
                {
                    // MÁY QUÉT LỖI SÂU 3 LỚP ĐỂ TÌM RA NGUYÊN NHÂN GỐC RỄ
                    string msg = "<b>Lớp 1:</b> " + ex.Message;
                    if (ex.InnerException != null)
                    {
                        msg += "<br/><br/><b>Lớp 2:</b> " + ex.InnerException.Message;
                        if (ex.InnerException.InnerException != null)
                        {
                            msg += "<br/><br/><b style='color:blue;'>Lớp 3 (LỖI GỐC CỦA SQL SERVER):</b> " + ex.InnerException.InnerException.Message;
                        }
                    }
                    return Content("<h2 style='color:red;'>LỖI SQL SERVER KHI THÊM SẢN PHẨM:</h2><p>" + msg + "</p>");
                }
            }

            // Nếu dữ liệu form không hợp lệ, load lại Dropdown hãng
            ViewBag.MaHang = new SelectList(db.HangSanXuat, "MaHang", "TenHang", sp.MaHang);
            return View(sp);
        }

        // ================= CHỨC NĂNG SỬA SẢN PHẨM =================
        // 1. GET: Admin/SanPham/Edit/5 (Hiển thị form sửa)
        public ActionResult Edit(int id)
        {
            // Tìm sản phẩm theo Mã
            var sp = db.SanPham.SingleOrDefault(n => n.MaSP == id);
            if (sp == null)
            {
                return HttpNotFound();
            }

            var tskt = db.ThongSoKyThuat.SingleOrDefault(t => t.MaSP == id);
            if (tskt != null)
            {
                ViewBag.ManHinh = tskt.ManHinh;
                ViewBag.Chip = tskt.Chip;
                ViewBag.RAM = tskt.RAM;
                ViewBag.ROM = tskt.ROM;
                ViewBag.Camera = tskt.Camera;
                ViewBag.Pin = tskt.Pin;
            }

            ViewBag.MaHang = new SelectList(db.HangSanXuat, "MaHang", "TenHang", sp.MaHang);
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(SanPham sp, string ManHinh, string Chip, string RAM, string ROM, string Camera, string Pin)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // CỐT LÕI: TẮT BỘ KIỂM TRA GIẢ LẬP ĐỂ TRÁNH LỖI KHI CẬP NHẬT
                    db.Configuration.ValidateOnSaveEnabled = false;

                    db.Entry(sp).State = System.Data.Entity.EntityState.Modified;

                    var tskt = db.ThongSoKyThuat.SingleOrDefault(t => t.MaSP == sp.MaSP);
                    if (tskt != null)
                    {
                        // Cắt tỉa chuỗi chống tràn khi Cập nhật
                        tskt.ManHinh = !string.IsNullOrEmpty(ManHinh) && ManHinh.Length > 100 ? ManHinh.Substring(0, 97) + "..." : ManHinh;
                        tskt.Chip = !string.IsNullOrEmpty(Chip) && Chip.Length > 100 ? Chip.Substring(0, 97) + "..." : Chip;
                        tskt.Camera = !string.IsNullOrEmpty(Camera) && Camera.Length > 100 ? Camera.Substring(0, 97) + "..." : Camera;

                        tskt.RAM = !string.IsNullOrEmpty(RAM) && RAM.Length > 50 ? RAM.Substring(0, 47) + "..." : RAM;
                        tskt.ROM = !string.IsNullOrEmpty(ROM) && ROM.Length > 50 ? ROM.Substring(0, 47) + "..." : ROM;
                        tskt.Pin = !string.IsNullOrEmpty(Pin) && Pin.Length > 50 ? Pin.Substring(0, 47) + "..." : Pin;

                        db.Entry(tskt).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        tskt = new ThongSoKyThuat
                        {
                            MaSP = sp.MaSP,
                            // Cắt tỉa chuỗi chống tràn khi Thêm mới từ form Edit
                            ManHinh = !string.IsNullOrEmpty(ManHinh) && ManHinh.Length > 100 ? ManHinh.Substring(0, 97) + "..." : ManHinh,
                            Chip = !string.IsNullOrEmpty(Chip) && Chip.Length > 100 ? Chip.Substring(0, 97) + "..." : Chip,
                            Camera = !string.IsNullOrEmpty(Camera) && Camera.Length > 100 ? Camera.Substring(0, 97) + "..." : Camera,
                            RAM = !string.IsNullOrEmpty(RAM) && RAM.Length > 50 ? RAM.Substring(0, 47) + "..." : RAM,
                            ROM = !string.IsNullOrEmpty(ROM) && ROM.Length > 50 ? ROM.Substring(0, 47) + "..." : ROM,
                            Pin = !string.IsNullOrEmpty(Pin) && Pin.Length > 50 ? Pin.Substring(0, 47) + "..." : Pin
                        };
                        db.ThongSoKyThuat.Add(tskt);
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    if (ex.InnerException != null) msg += " | " + ex.InnerException.Message;
                    return Content("<h2 style='color:red;'>LỖI SQL SERVER KHI CẬP NHẬT SẢN PHẨM:</h2><p>" + msg + "</p>");
                }
            }

            ViewBag.MaHang = new SelectList(db.HangSanXuat, "MaHang", "TenHang", sp.MaHang);
            return View(sp);
        }

        public ActionResult Delete(int id)
        {
            var sp = db.SanPham.SingleOrDefault(n => n.MaSP == id);
            if (sp == null)
            {
                return HttpNotFound();
            }
            return View(sp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var sp = db.SanPham.SingleOrDefault(n => n.MaSP == id);
            if (sp != null)
            {
                sp.TrangThai = 0;

                db.Entry(sp).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // UC35: CẢNH BÁO ĐIỆN THOẠI SẮP HẾT HÀNG (TỒN KHO <= 5)
        public ActionResult SapHetHang()
        {
            var dsSapHet = db.SanPham
                             .Where(sp => sp.SoLuongTon <= 5 && sp.TrangThai == 1)
                             .OrderBy(sp => sp.SoLuongTon)
                             .ToList();

            ViewBag.SoLuongCanhBao = dsSapHet.Count;

            return View(dsSapHet);
        }
    }
}