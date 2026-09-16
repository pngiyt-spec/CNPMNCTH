using Project_Cellphoness.Models;
using System.Web.Mvc;
using System.Web.Routing;

namespace Project_Cellphoness.Areas.Admin.Controllers
{
    // Đóng vai trò là Controller gốc, không có View, chỉ dùng để chứa logic bảo mật
    public class BaseAdminController : Controller
    {
        // Hàm này sẽ tự động chạy TRƯỚC KHI bất kỳ hàm nào trong Admin được gọi
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var tk = (TaiKhoan)Session["TaiKhoan"];

            // =======================================================================
            // 1. KIỂM TRA ĐĂNG NHẬP CƠ BẢN
            // Nếu chưa đăng nhập HOẶC (không phải Admin [1] VÀ CŨNG KHÔNG PHẢI Nhân viên [3])
            // =======================================================================
            if (tk == null || (tk.MaChucVu != 1 && tk.MaChucVu != 3))
            {
                // Chuyển hướng về trang Đăng nhập ADMIN khi không đủ quyền
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Auth", action = "DangNhap", area = "Admin" })
                );
                return; // Dừng thực thi ngay lập tức
            }

            // =======================================================================
            // 2. KIỂM TRA PHÂN QUYỀN CHI TIẾT DÀNH RIÊNG CHO NHÂN VIÊN (MaChucVu = 3)
            // =======================================================================
            if (tk.MaChucVu == 3)
            {
                // Lấy tên Controller đang truy cập
                string currentController = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

                // Lấy danh sách quyền từ CSDL/Session
                string danhSachQuyen = tk.DanhSachQuyen ?? "";
                bool hasPermission = false;

                // Các trang cơ bản cho phép nhân viên vào tự do
                if (currentController == "Home" || currentController == "Auth")
                {
                    hasPermission = true;
                }
                // Nhóm quyền Quản lý Sản phẩm
                else if (currentController == "SanPham" || currentController == "QuanLyHang")
                {
                    // Tách riêng chức năng Cảnh báo tồn kho (thuộc QL_Kho)
                    if (filterContext.ActionDescriptor.ActionName == "SapHetHang")
                    {
                        if (danhSachQuyen.Contains("QL_Kho")) hasPermission = true;
                    }
                    else
                    {
                        if (danhSachQuyen.Contains("QL_SanPham")) hasPermission = true;
                    }
                }
                // Nhóm quyền Quản lý Đơn hàng
                else if (currentController == "QuanLyDonHang" || currentController == "QuanLyDanhGia")
                {
                    if (danhSachQuyen.Contains("QL_DonHang")) hasPermission = true;
                }
                // Nhóm quyền Quản lý Voucher
                else if (currentController == "QuanLyMaGiamGia")
                {
                    if (danhSachQuyen.Contains("QL_Voucher")) hasPermission = true;
                }

                // CHỐT CHẶN: Nếu nhân viên không có quyền truy cập vào Controller hiện tại
                if (!hasPermission)
                {
                    TempData["LoiQuyen"] = "Truy cập bị từ chối! Bạn không có quyền sử dụng chức năng này.";
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Home", action = "Index", area = "Admin" })
                    );
                    return; // Đẩy về Bảng điều khiển và dừng thực thi
                }
            }

            // Nếu đúng là Admin (boss tổng) hoặc Nhân viên đã vượt qua bài test thì cho đi tiếp
            base.OnActionExecuting(filterContext);
        }
    }
}