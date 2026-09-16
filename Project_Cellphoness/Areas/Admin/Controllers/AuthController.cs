using Project_Cellphoness.Models;
using System.Linq;
using System.Web.Mvc;

namespace Project_Cellphoness.Areas.Admin.Controllers
{
    public class AuthController : Controller
    {
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        // 1. Giao diện Đăng nhập Admin
        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        // 2. Xử lý Đăng nhập Admin
        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
        {
            string email = collection["Email"];
            string matKhau = collection["MatKhau"];

            TaiKhoan tk = db.TaiKhoan.SingleOrDefault(n => n.Email == email && n.MatKhau == matKhau);

            if (tk != null)
            {
                // Nếu bị khóa
                if (tk.TrangThai != true)
                {
                    ViewBag.ThongBao = "Tài khoản quản trị của bạn đã bị khóa!";
                    return View();
                }

                // Kiểm tra ĐÚNG CHUẨN LÀ ADMIN (MaChucVu = 1) mới cho vào
                if (tk.MaChucVu == 1 || tk.MaChucVu == 3)
                {
                    Session["TaiKhoan"] = tk;
                    return RedirectToAction("Index", "Home", new { area = "Admin" }); 
                }
                else
                {
                    ViewBag.ThongBao = "Cảnh báo: Bạn không có quyền truy cập khu vực Quản trị!";
                    return View();
                }
            }
            else
            {
                ViewBag.ThongBao = "Email hoặc mật khẩu không chính xác!";
                return View();
            }
        }

        // 3. Đăng xuất Admin
        public ActionResult DangXuat()
        {
            Session["TaiKhoan"] = null;
            return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });
        }
    }
}