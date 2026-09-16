using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Project_Cellphoness.Models; 

namespace Project_Cellphoness.Areas.Admin.Controllers
{
    public class HomeController : BaseAdminController
    {
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        public ActionResult Index()
        {
            ViewBag.TongDoanhThu = db.DonHang
                                     .Where(d => d.TrangThaiDH == "Hoàn thành")
                                     .Sum(d => (decimal?)d.TongTien) ?? 0;

            ViewBag.TongDonHang = db.DonHang.Count();
            ViewBag.TongKhachHang = db.TaiKhoan.Where(t => t.MaChucVu == 2).Count();
            ViewBag.TongSanPham = db.SanPham.Count();
            int namHienTai = DateTime.Now.Year;

            var doanhThuTheoThang = db.DonHang
                .Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Year == namHienTai && d.TrangThaiDH == "Hoàn thành")
                .GroupBy(d => d.NgayDat.Value.Month)
                .Select(g => new
                {
                    Thang = g.Key,
                    DoanhThu = g.Sum(d => d.TongTien)
                }).ToList();

            List<string> lstThang = new List<string>();
            List<decimal> lstDoanhThu = new List<decimal>();

            for (int i = 1; i <= 12; i++)
            {
                lstThang.Add("Tháng " + i);
                var dt = doanhThuTheoThang.FirstOrDefault(x => x.Thang == i);
                lstDoanhThu.Add(dt != null ? (decimal)dt.DoanhThu : 0);
            }

            ViewBag.ChartLabels = lstThang;
            ViewBag.ChartData = lstDoanhThu;
            ViewBag.NamHienTai = namHienTai;

            
            //  UC34: TÌM TOP 5 SẢN PHẨM BÁN CHẠY NHẤT
            // Lọc các đơn hàng đã hoàn thành, gom nhóm theo sản phẩm và tính tổng số lượng bán
            var topSanPham = (from ct in db.ChiTietDonHang
                              join dh in db.DonHang on ct.MaDH equals dh.MaDH
                              join sp in db.SanPham on ct.MaSP equals sp.MaSP
                              where dh.TrangThaiDH == "Hoàn thành"
                              group ct by new { sp.MaSP, sp.TenSP } into g
                              select new
                              {
                                  TenSP = g.Key.TenSP,
                                  SoLuongBan = g.Sum(x => x.SoLuong)
                              })
                             .OrderByDescending(x => x.SoLuongBan)
                             .Take(5)
                             .ToList();

            // Biểu đồ
            List<string> lstTenSP = topSanPham.Select(x => x.TenSP).ToList();
            List<int> lstSoLuongBan = topSanPham.Select(x => x.SoLuongBan).ToList();

            ViewBag.TopSPLabels = lstTenSP;
            ViewBag.TopSPData = lstSoLuongBan;

            return View();
        }
        [HttpGet]
        public ActionResult BaoCaoDoanhThu(int? thang, int? nam)
        {
            // Bắt đầu: Lấy mốc thời gian, nếu không truyền vào thì mặc định là tháng/năm hiện tại
            int selectedMonth = thang ?? DateTime.Now.Month;
            int selectedYear = nam ?? DateTime.Now.Year;

            ViewBag.ThangDaChon = selectedMonth;
            ViewBag.NamDaChon = selectedYear;

            // B1 & B2: Truy vấn CSDL, lọc theo tháng, năm và đúng trạng thái "Hoàn thành"
            var donHangs = db.DonHang
                .Where(d => d.NgayDat.HasValue
                         && d.NgayDat.Value.Month == selectedMonth
                         && d.NgayDat.Value.Year == selectedYear
                         && d.TrangThaiDH == "Hoàn thành")
                .ToList();

            // B3: Tính tổng doanh thu 
            decimal tongDoanhThu = donHangs.Sum(d => (decimal?)d.TongTien) ?? 0;
            ViewBag.TongDoanhThu = tongDoanhThu;

            //  Gom nhóm (GroupBy) theo ngày để vẽ biểu đồ
            var dataBieuDo = donHangs
                .GroupBy(d => d.NgayDat.Value.Day)
                .Select(g => new {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(x => x.TongTien)
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            ViewBag.BieuDo_Labels = string.Join(",", dataBieuDo.Select(x => "Ngày " + x.Ngay));
            ViewBag.BieuDo_Data = string.Join(",", dataBieuDo.Select(x => x.DoanhThu));

            
            return View(donHangs);
        }
    }
}