using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PhamVanTung.SachOnline.Models;

namespace PhamVanTung.SachOnline.Areas.PhamVanTung_Admin.Controllers
{
    public class NHAXUATBANsController : Controller
    {
        private SachOnlineEntities db = new SachOnlineEntities();

        // GET: PhamVanTung_Admin/NHAXUATBANs
        // NHAXUATBANsController
        public ActionResult Index()
        {
            var nxb = db.NHAXUATBANs.ToList();
            return View(nxb);
        }


        // GET: PhamVanTung_Admin/NHAXUATBANs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NHAXUATBAN nHAXUATBAN = db.NHAXUATBANs.Find(id);
            if (nHAXUATBAN == null)
            {
                return HttpNotFound();
            }
            return View(nHAXUATBAN);
        }

        // GET: PhamVanTung_Admin/NHAXUATBANs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PhamVanTung_Admin/NHAXUATBANs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaNXB,TenNXB,DiaChi,DienThoai")] NHAXUATBAN nHAXUATBAN)
        {
            if (ModelState.IsValid)
            {
                db.NHAXUATBANs.Add(nHAXUATBAN);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(nHAXUATBAN);
        }

        // GET: PhamVanTung_Admin/NHAXUATBANs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NHAXUATBAN nHAXUATBAN = db.NHAXUATBANs.Find(id);
            if (nHAXUATBAN == null)
            {
                return HttpNotFound();
            }
            return View(nHAXUATBAN);
        }

        // POST: PhamVanTung_Admin/NHAXUATBANs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaNXB,TenNXB,DiaChi,DienThoai")] NHAXUATBAN nHAXUATBAN)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nHAXUATBAN).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(nHAXUATBAN);
        }

        // GET: PhamVanTung_Admin/NHAXUATBANs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NHAXUATBAN nHAXUATBAN = db.NHAXUATBANs.Find(id);
            if (nHAXUATBAN == null)
            {
                return HttpNotFound();
            }
            return View(nHAXUATBAN);
        }

        // POST: PhamVanTung_Admin/NHAXUATBANs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NHAXUATBAN nHAXUATBAN = db.NHAXUATBANs.Find(id);
            db.NHAXUATBANs.Remove(nHAXUATBAN);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
