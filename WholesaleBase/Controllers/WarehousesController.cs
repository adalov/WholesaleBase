using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WholesaleBase.Models;

namespace WholesaleBase.Controllers
{
    [Authorize(Roles ="boss")]
    public class WarehousesController : Controller
    {
        private WholesaleContext db = new WholesaleContext();
        private ApplicationDbContext db2 = new ApplicationDbContext();

        public ActionResult Index()
        {
            List<WarehouseAccountView> warehouseAccountlist = new List<WarehouseAccountView>();
            foreach (Warehouse i in db.Warehouses)
            {
                WarehouseAccountView warehouseAccountView = new WarehouseAccountView{Warehouse = i};
                warehouseAccountView.Account = db2.Users.FirstOrDefault(u => u.Claims.FirstOrDefault(c => c.ClaimType == "WarehouseId").ClaimValue == i.Id.ToString());
                warehouseAccountlist.Add(warehouseAccountView);
            }
            if (!Request.IsAjaxRequest())
                return View(warehouseAccountlist);
            else
                return PartialView("WarehouseAccountList", warehouseAccountlist);                
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Representative,PhoneNumber,Address")] Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                warehouse.Status = true;
                db.Warehouses.Add(warehouse);
                db.SaveChanges();

                foreach(var goods in db.Goods.ToList())
                {
                    db.GoodsMovements.Add(new GoodsMovement() { Date = DateTime.Now, GoodsId = goods.Id, WarehouseId = warehouse.Id, GoodsArrival = 0, GoodsConsumption = 0, GoodsBalance = 0});
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(warehouse);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Warehouse warehouse = db.Warehouses.Find(id);
            if (warehouse == null)
            {
                return HttpNotFound();
            }
            return View(warehouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Representative,PhoneNumber,Address,Status")] Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                db.Entry(warehouse).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(warehouse);
        }

        public ActionResult RemoveOrRestore(int Id)
        {
            Warehouse warehouse = db.Warehouses.Find(Id);
            warehouse.Status = !warehouse.Status;
            db.SaveChanges();
            if (warehouse.Status == true)
            {
                var goodsMovement = db.GoodsMovements.ToList().Where(gm => gm.Date.Date == DateTime.Now.Date && gm.WarehouseId == warehouse.Id).FirstOrDefault();
                if (goodsMovement == null)
                {
                    foreach (Goods goods in db.Goods.Where(w => w.Status == true).ToList())
                    {
                        goodsMovement = new GoodsMovement { Date = DateTime.Now, GoodsId = goods.Id, GoodsArrival = 0, GoodsConsumption = 0, GoodsBalance = 0, WarehouseId = warehouse.Id };
                        db.GoodsMovements.Add(goodsMovement);
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                db2.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
