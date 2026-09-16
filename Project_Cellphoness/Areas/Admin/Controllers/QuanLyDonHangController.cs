using System.Linq;
using System.Web.Mvc;
using Project_Cellphoness.Models;

namespace Project_Cellphoness.Areas.Admin.Controllers
{
    public class QuanLyDonHangController : BaseAdminController
    {
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        public ActionResult Index()
        {
            var lstDonHang = db.DonHang.OrderByDescending(n => n.NgayDat).ToList();
            return View(lstDonHang);
        }

        public ActionResult ChiTiet(int id)
        {
            var donHang = db.DonHang.SingleOrDefault(n => n.MaDH == id);
            if (donHang == null)
            {
                return HttpNotFound();
            }

            var lstChiTiet = db.ChiTietDonHang.Where(n => n.MaDH == id).ToList();
            ViewBag.DanhSachChiTiet = lstChiTiet;

            
            //  Truy vấn lấy tên nhân viên đã duyệt đơn hàng này
            
            if (donHang.MaNVDuyet != null)
            {
                var nhanVien = db.TaiKhoan.SingleOrDefault(t => t.MaTK == donHang.MaNVDuyet);
                if (nhanVien != null)
                {
                    ViewBag.TenNhanVien = nhanVien.HoTen;
                }
                else
                {
                    ViewBag.TenNhanVien = "Không xác định";
                }
            }
            else
            {
                ViewBag.TenNhanVien = "Chưa xử lý (Đơn mới)";
            }

            return View(donHang);
        }

        
        //  TRẠNG THÁI ĐƠN HÀNG và QĐ18 (LƯU LỊCH SỬ DUYỆT ĐƠN)
       
        [HttpPost]
        public ActionResult CapNhatTrangThai(int MaDH, string TrangThaiMoi)
        {
            var dh = db.DonHang.SingleOrDefault(n => n.MaDH == MaDH);
            if (dh != null)
            {
                // 1. Cập nhật trạng thái mới do Admin/Nhân viên chọn
                dh.TrangThaiDH = TrangThaiMoi;

                // 2. QĐ18: Lấy thông tin người đang thao tác và lưu vào cột MaNVDuyet
                var admin = Session["TaiKhoan"] as TaiKhoan;
                if (admin != null)
                {
                    dh.MaNVDuyet = admin.MaTK;
                }

                db.SaveChanges();
                TempData["ThongBao"] = "Cập nhật trạng thái và ghi nhận lịch sử thao tác thành công!";
            }

            return RedirectToAction("ChiTiet", new { id = MaDH });
        }

        // UC26: IN HÓA ĐƠN CHI TIẾT (XUẤT PDF / MÁY IN)
        public ActionResult InHoaDon(int id)
        {
            var dh = db.DonHang.SingleOrDefault(n => n.MaDH == id);
            if (dh == null)
            {
                return HttpNotFound();
            }
            var chiTiet = db.ChiTietDonHang.Where(c => c.MaDH == id).ToList();
            ViewBag.ChiTiet = chiTiet;

            return View(dh);
        }
    }
}