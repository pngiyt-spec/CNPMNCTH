using Project_Cellphoness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_Cellphoness.Controllers
{
    public class YeuThichController : Controller
    {
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        // 1. Hàm đọc Cookie lấy danh sách Mã Sản Phẩm yêu thích
        private List<int> LayDanhSachYeuThich()
        {
            List<int> lst = new List<int>();
            var tk = Session["TaiKhoan"] as TaiKhoan;
            if (tk == null) return lst; // Phải đăng nhập mới có danh sách

            string cookieName = "YeuThichCookie_" + tk.MaTK;
            HttpCookie cookie = Request.Cookies[cookieName];

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                // Cookie lưu dạng chuỗi "1,5,12" (Các mã sản phẩm cách nhau dấu phẩy)
                string[] ids = cookie.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string id in ids)
                {
                    lst.Add(int.Parse(id));
                }
            }
            return lst;
        }

        // 2. Hàm lưu mảng ID vào Cookie
        private void LuuDanhSachYeuThich(List<int> lst)
        {
            var tk = Session["TaiKhoan"] as TaiKhoan;
            if (tk == null) return;

            string cookieName = "YeuThichCookie_" + tk.MaTK;
            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Value = string.Join(",", lst);
            cookie.Expires = DateTime.Now.AddDays(30); // Giữ danh sách yêu thích 1 tháng
            Response.Cookies.Add(cookie);
        }

        // 3. Action hiển thị trang Danh sách yêu thích
        public ActionResult Index()
        {
            var tk = Session["TaiKhoan"] as TaiKhoan;
            if (tk == null) return RedirectToAction("DangNhap", "TaiKhoan");

            List<int> lstIds = LayDanhSachYeuThich();

            // Tìm các sản phẩm trong Database có Mã nằm trong danh sách Cookie
            var dsSanPham = db.SanPham.Where(sp => lstIds.Contains(sp.MaSP) && sp.TrangThai == 1).ToList();

            return View(dsSanPham);
        }

        // 4. Action Thêm vào yêu thích
        public ActionResult ThemYeuThich(int id, string strURL)
        {
            var tk = Session["TaiKhoan"] as TaiKhoan;
            if (tk == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            List<int> lstIds = LayDanhSachYeuThich();
            if (!lstIds.Contains(id))
            {
                lstIds.Add(id); // Nếu chưa có thì thêm vào
                LuuDanhSachYeuThich(lstIds);
            }

            // Thêm xong thì ở lại đúng trang cũ
            return Redirect(strURL);
        }

        // 5. Action Xóa khỏi yêu thích
        public ActionResult XoaYeuThich(int id)
        {
            var tk = Session["TaiKhoan"] as TaiKhoan;
            if (tk == null) return RedirectToAction("DangNhap", "TaiKhoan");

            List<int> lstIds = LayDanhSachYeuThich();
            if (lstIds.Contains(id))
            {
                lstIds.Remove(id);
                LuuDanhSachYeuThich(lstIds);
            }
            return RedirectToAction("Index");
        }
    }
}