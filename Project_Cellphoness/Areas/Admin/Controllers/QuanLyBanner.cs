using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project_Cellphoness.Models; 

namespace Project_Cellphoness.Areas.Admin.Controllers
{
    
    public class QuanLyBannerController : BaseAdminController
    {
        CellphonesStoreEntities1 db = new CellphonesStoreEntities1();

        // 1. TRANG DANH SÁCH BANNER
        public ActionResult Index()
        {
            var dsBanner = db.Banner.OrderBy(b => b.ViTri).ToList();
            return View(dsBanner);
        }

        // 2. TRANG SỬA BANNER (GET)
        public ActionResult Edit(int id)
        {
            var banner = db.Banner.SingleOrDefault(b => b.MaBanner == id);
            if (banner == null)
            {
                return HttpNotFound();
            }
            return View(banner);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Banner b)
        {
            if (ModelState.IsValid)
            {
                var item = db.Banner.SingleOrDefault(x => x.MaBanner == b.MaBanner);
                if (item != null)
                {
                    item.HinhAnh = b.HinhAnh;
                    item.Link = b.Link;
                    item.ViTri = b.ViTri;
                    item.TrangThai = b.TrangThai;

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(b);
        }
        // 4. TRANG TẠO MỚI BANNER (GET)
        public ActionResult Create()
        {
            return View();
        }

        // 5. XỬ LÝ LƯU BANNER MỚI VÀO CSDL (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Banner b)
        {
            if (ModelState.IsValid)
            {
                db.Banner.Add(b);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(b);
        }
    }
}