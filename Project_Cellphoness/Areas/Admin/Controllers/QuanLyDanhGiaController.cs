using System;
using System.Linq;
using System.Web.Mvc;
using Project_Cellphoness.Models;

namespace Project_Cellphoness.Areas.Admin.Controllers
{
    public class QuanLyDanhGiaController : BaseAdminController
    {
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        // 1. LIỆT KÊ TẤT CẢ BÌNH LUẬN 
        public ActionResult Index()
        {
            var dsDanhGia = db.DanhGia.OrderByDescending(d => d.NgayDG).ToList();
            return View(dsDanhGia);
        }

        // 2. DUYỆT BÌNH LUẬN 
        public ActionResult Duyet(int id)
        {
            var cmt = db.DanhGia.SingleOrDefault(d => d.MaDG == id);
            if (cmt != null)
            {
                cmt.PheDuyet = true;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 3. ẨN BÌNH LUẬN 
        public ActionResult An(int id)
        {
            var cmt = db.DanhGia.SingleOrDefault(d => d.MaDG == id);
            if (cmt != null)
            {
                cmt.PheDuyet = false;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 4. XÓA VĨNH VIỄN
        public ActionResult Xoa(int id)
        {
            var cmt = db.DanhGia.SingleOrDefault(d => d.MaDG == id);
            if (cmt != null)
            {
                db.DanhGia.Remove(cmt);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}