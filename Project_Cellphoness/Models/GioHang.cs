using System.Linq;

namespace Project_Cellphoness.Models
{
    public class GioHang
    {
        // Gọi CSDL để lấy thông tin sản phẩm
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        public int iMaSP { get; set; }
        public string sTenSP { get; set; }
        public string sHinhAnh { get; set; }
        public decimal dDonGia { get; set; }
        public int iSoLuong { get; set; }
        public decimal dThanhTien
        {
            get { return iSoLuong * dDonGia; }
        }

        // Hàm khởi tạo giỏ hàng: Khi truyền Mã SP vào, nó sẽ tự móc dữ liệu từ DB ra
        public GioHang(int MaSP)
        {
            iMaSP = MaSP;
            SanPham sp = db.SanPham.Single(n => n.MaSP == iMaSP);
            sTenSP = sp.TenSP;
            sHinhAnh = sp.HinhAnh;
            dDonGia = sp.GiaBan;
            iSoLuong = 1; // Quy định QD06: Mặc định thêm vào là 1
        }
    }
}