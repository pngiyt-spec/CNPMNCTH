using Project_Cellphoness.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Project_Cellphoness.Areas.Admin.Controllers
{
    public class QuanLyTaiKhoanController : BaseAdminController
    {
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        
        // 1. QUẢN LÝ KHÁCH HÀNG (MaChucVu = 2)
        
        public ActionResult Index()
        {
            var lstKhachHang = db.TaiKhoan.Where(t => t.MaChucVu == 2).OrderByDescending(t => t.MaTK).ToList();
            ViewBag.Title = "QUẢN LÝ TÀI KHOẢN KHÁCH HÀNG";
            return View(lstKhachHang);
        }

        
        // 2. QUẢN LÝ NHÂN VIÊN (MaChucVu = 3)
        
        public ActionResult DanhSachNhanVien()
        {
            var lstNhanVien = db.TaiKhoan.Where(t => t.MaChucVu == 3).OrderByDescending(t => t.MaTK).ToList();
            ViewBag.Title = "QUẢN LÝ TÀI KHOẢN NHÂN VIÊN";

            // CẬP NHẬT: Trả về View riêng tên là "DanhSachNhanVien" thay vì dùng chung "Index"
            return View(lstNhanVien);
        }

        
        // 3. QUẢN LÝ ADMIN (MaChucVu = 1)
        
       
        public ActionResult DanhSachAdmin()
        {
            var lstAdmin = db.TaiKhoan.Where(t => t.MaChucVu == 1).OrderByDescending(t => t.MaTK).ToList();
            ViewBag.Title = "QUẢN LÝ QUẢN TRỊ VIÊN (ADMIN CẤP CAO)";

            // Xóa chữ "Index" đi, đổi thành:
            return View(lstAdmin);
        }

        
        // KHÓA / MỞ KHÓA TÀI KHOẢN (DÙNG CHUNG)
        
        public ActionResult ThayDoiTrangThai(int id)
        {
            var tk = db.TaiKhoan.SingleOrDefault(t => t.MaTK == id);
            if (tk != null)
            {
                tk.TrangThai = !tk.TrangThai;
                db.Entry(tk).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("Index");
        }

        
        // UC30 & QĐ21: ADMIN TẠO & PHÂN QUYỀN TÀI KHOẢN
       
        [HttpGet]
        public ActionResult TaoTaiKhoanNhanVien()
        {
            var admin = Session["TaiKhoan"] as TaiKhoan;
            if (admin == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (admin.MaChucVu != 1)
            {
                TempData["LoiQuyen"] = "Truy cập bị từ chối! Chỉ Admin cấp cao mới được phép tạo và phân quyền tài khoản hệ thống.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.MaChucVu = new SelectList(db.ChucVu.Where(c => c.MaChucVu == 1 || c.MaChucVu == 3).ToList(), "MaChucVu", "TenChucVu");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoTaiKhoanNhanVien(TaiKhoan tk)
        {
            var admin = Session["TaiKhoan"] as TaiKhoan;
            if (admin == null || admin.MaChucVu != 1)
            {
                TempData["LoiQuyen"] = "Truy cập bị từ chối! Bạn không có quyền thực hiện chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                var checkEmail = db.TaiKhoan.FirstOrDefault(t => t.Email == tk.Email);
                if (checkEmail != null)
                {
                    ViewBag.ThongBaoLoi = "Email này đã được sử dụng. Vui lòng chọn Email khác!";
                    ViewBag.MaChucVu = new SelectList(db.ChucVu.Where(c => c.MaChucVu == 1 || c.MaChucVu == 3).ToList(), "MaChucVu", "TenChucVu", tk.MaChucVu);
                    return View(tk);
                }

                tk.TrangThai = true;
                tk.NgayTao = DateTime.Now;

                var maxMaTK = db.TaiKhoan.Any() ? db.TaiKhoan.Max(t => t.MaTK) : 0;
                tk.MaTK = maxMaTK + 1;

                if (string.IsNullOrEmpty(tk.GoogleID))
                {
                    tk.GoogleID = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(tk.SoDienThoai))
                {
                    tk.SoDienThoai = "0000" + DateTime.Now.Ticks.ToString().Substring(0, 6);
                }

                db.TaiKhoan.Add(tk);
                db.SaveChanges();

                TempData["ThongBao"] = "Tạo tài khoản và phân quyền thành công!";

                if (tk.MaChucVu == 1) return RedirectToAction("DanhSachAdmin");
                return RedirectToAction("DanhSachNhanVien");
            }

            ViewBag.MaChucVu = new SelectList(db.ChucVu.Where(c => c.MaChucVu == 1 || c.MaChucVu == 3).ToList(), "MaChucVu", "TenChucVu", tk.MaChucVu);
            return View(tk);
        }

        
        // TÍNH NĂNG PHÂN QUYỀN (CHỈ ADMIN = 1 ĐƯỢC PHÂN QUYỀN CHO NHÂN VIÊN = 3)
        
        [HttpGet]
        public ActionResult PhanQuyen(int id)
        {
            var adminLog = Session["TaiKhoan"] as TaiKhoan;
            if (adminLog == null || adminLog.MaChucVu != 1)
            {
                TempData["LoiQuyen"] = "Truy cập bị từ chối! Chỉ Admin cấp cao mới được vào trang này.";
                return RedirectToAction("Index", "Home");
            }

            var tk = db.TaiKhoan.SingleOrDefault(t => t.MaTK == id);
            if (tk == null) return HttpNotFound();

            if (tk.MaChucVu != 3)
            {
                TempData["LoiQuyen"] = "Cảnh báo: Bạn chỉ được phép phân quyền cho tài khoản Nhân viên!";
                return RedirectToAction("DanhSachNhanVien");
            }

            return View(tk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PhanQuyen(int MaTK, string[] QuyenHan)
        {
            var adminLog = Session["TaiKhoan"] as TaiKhoan;
            if (adminLog == null || adminLog.MaChucVu != 1)
            {
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });
            }

            var tk = db.TaiKhoan.SingleOrDefault(t => t.MaTK == MaTK);
            if (tk != null)
            {
                if (tk.MaChucVu != 3)
                {
                    TempData["LoiQuyen"] = "Lỗi bảo mật: Thao tác thất bại do tài khoản đích không phải Nhân viên!";
                    return RedirectToAction("DanhSachNhanVien");
                }

                if (QuyenHan != null && QuyenHan.Length > 0)
                {
                    string chuoiQuyenMoi = string.Join(",", QuyenHan);
                    tk.DanhSachQuyen = chuoiQuyenMoi;
                }
                else
                {
                    
                    tk.DanhSachQuyen = "";
                }

                db.SaveChanges();
                TempData["ThongBao"] = $"Phân quyền thành công cho nhân viên {tk.HoTen}.";
            }

            return RedirectToAction("DanhSachNhanVien");
        }
    }
}