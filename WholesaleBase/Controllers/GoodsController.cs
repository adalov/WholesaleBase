using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WholesaleBase.Models;

namespace WholesaleBase.Controllers
{
    [Authorize]
    public class GoodsController : Controller
    {
        WholesaleContext db = new WholesaleContext();
        ApplicationDbContext db2 = new ApplicationDbContext();

        public ActionResult Index()
        {
            if (User.IsInRole("storekeeper"))
            {
                ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);
                var WarehouseId = user.Claims.SingleOrDefault(c => c.ClaimType == "WarehouseId").ClaimValue;
                var warehouse = db.Warehouses.Find(Int32.Parse(WarehouseId));
                if (warehouse.Status == false) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var goods = db.Goods.Include(g => g.MeasureUnit).Include(g => g.Supplier);
            if (!Request.IsAjaxRequest())
                return View(goods.ToList());
            else
                return PartialView("GoodsList", goods.ToList());
        }
        [Authorize(Roles = "storekeeper")]
        public ActionResult Inventorization()
        {
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);
            var WarehouseId = user.Claims.SingleOrDefault(c => c.ClaimType == "WarehouseId").ClaimValue;
            var warehouse = db.Warehouses.Find(Int32.Parse(WarehouseId));
            if (warehouse.Status == false) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (HttpContext.Session["Inv"] == null)
            {
                var goods = db.Goods.Where(g => g.Status == true).Include(g => g.MeasureUnit).Include(g => g.Supplier);
                List<InventorizationViewModel> InventorizationsList = new List<InventorizationViewModel>();
                foreach (var item in goods.ToList())
                {
                    var Inventorization = new InventorizationViewModel() { Id = item.Id, MeasureUnit = item.MeasureUnit, Name = item.Name, Supplier = item.Supplier };
                    InventorizationsList.Add(Inventorization);
                }
                HttpContext.Session["Inv"] = InventorizationsList;
            }
            return View(HttpContext.Session["Inv"]);

        }

        [HttpPost]
        [Authorize(Roles = "storekeeper")]
        public ActionResult InventorizationSend(InventorizationViewModel model, int goodsQuantity)
        {
            Goods goods = db.Goods.Find(model.Id);
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);
            var WarehouseId = user.Claims.SingleOrDefault(c => c.ClaimType == "WarehouseId").ClaimValue;
            GoodsMovement goodsMovement = goods.GoodsMovements.FirstOrDefault(gm => gm.WarehouseId.ToString() == WarehouseId && gm.Date.Date == DateTime.Now.Date);
            int goodsBalance = goodsMovement.GoodsBalance;
            InventoryResult inventoryResult = new InventoryResult { GoodsId = model.Id, ActualGoodsBalance = goodsQuantity, RegisteredGoodsBalance = goodsBalance, InventoryDate = DateTime.Now, WarehouseId = Int32.Parse(WarehouseId) };
            db.InventoryResults.Add(inventoryResult);
            db.SaveChanges();
            ((List<InventorizationViewModel>)HttpContext.Session["Inv"]).Find(x => x.Id == model.Id).Status = true;
            model.Status = ((List<InventorizationViewModel>)HttpContext.Session["Inv"]).Find(x => x.Id == model.Id).Status;
            return PartialView("InventorizationPartial", model);
        }

        [Authorize(Roles = "storekeeper")]
        public ActionResult InventorizationReset()
        {
            HttpContext.Session["Inv"] = null;
            return RedirectToAction("Inventorization");
        }

        [Authorize(Roles = "storekeeper")]
        public ActionResult RemoveOrRestore(int Id)
        {
            Goods goods = db.Goods.Find(Id);
            goods.Status = !goods.Status;
            db.SaveChanges();

            if(goods.Status == true)
            {
                GoodsMovement goodsMovement = goods.GoodsMovements.ToList().Where(gm => gm.Date.Date == DateTime.Now.Date).FirstOrDefault();
                if(goodsMovement == null)
                {
                    foreach (Warehouse warehouse in db.Warehouses.Where(w => w.Status == true).ToList())
                    {
                        goodsMovement = new GoodsMovement { Date = DateTime.Now, GoodsId = goods.Id, GoodsArrival = 0, GoodsConsumption = 0, GoodsBalance = 0, WarehouseId = warehouse.Id };
                        db.GoodsMovements.Add(goodsMovement);
                    }
                    db.SaveChanges();
                }
            }            
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "storekeeper")]
        public ActionResult Create()
        {
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);
            var WarehouseId = user.Claims.SingleOrDefault(c => c.ClaimType == "WarehouseId").ClaimValue;
            var warehouse = db.Warehouses.Find(Int32.Parse(WarehouseId));
            if (warehouse.Status == false) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ViewBag.MeasureUnitId = new SelectList(db.MeasureUnits, "Id", "Name");
            List<Supplier> suppliers = db.Suppliers.Where(s => s.Status == true).ToList();
            ViewBag.SupplierId = new SelectList(suppliers, "Id", "Name");
            return View();
        }

        [Authorize(Roles = "storekeeper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,SupplierId,MeasureUnitId,PurchasePrice,SellingPrice")] Goods goods)
        {
            if (ModelState.IsValid)
            {
                goods.Status = true;
                db.Goods.Add(goods);
                db.SaveChanges();
                foreach(Warehouse warehouse in db.Warehouses.Where(w => w.Status == true).ToList())
                {
                    GoodsMovement goodsMovement = new GoodsMovement { Date = DateTime.Now, GoodsId = goods.Id, GoodsArrival = 0, GoodsConsumption = 0, GoodsBalance = 0, WarehouseId = warehouse.Id };
                    db.GoodsMovements.Add(goodsMovement);                  
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MeasureUnitId = new SelectList(db.MeasureUnits.ToList(), "Id", "Name", goods.MeasureUnitId);
            ViewBag.SupplierId = new SelectList(db.Suppliers.Where(s => s.Status == true).ToList(), "Id", "Name", goods.SupplierId);
            return View(goods);
        }

        [Authorize(Roles = "storekeeper")]
        public ActionResult Edit(int? id)
        {
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);
            var WarehouseId = user.Claims.SingleOrDefault(c => c.ClaimType == "WarehouseId").ClaimValue;
            var warehouse = db.Warehouses.Find(Int32.Parse(WarehouseId));
            if (warehouse.Status == false) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Goods goods = db.Goods.Find(id);
            if (goods == null)
            {
                return HttpNotFound();
            }
            ViewBag.MeasureUnitId = new SelectList(db.MeasureUnits.ToList(), "Id", "Name", goods.MeasureUnitId);
            ViewBag.SupplierId = new SelectList(db.Suppliers.Where(s => s.Status == true).ToList(), "Id", "Name", goods.SupplierId);
            return View(goods);
        }

        [Authorize(Roles = "storekeeper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,SupplierId,MeasureUnitId,PurchasePrice,SellingPrice,Status")] Goods goods)
        {
            if (ModelState.IsValid)
            {
                db.Entry(goods).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MeasureUnitId = new SelectList(db.MeasureUnits.ToList(), "Id", "Name", goods.MeasureUnitId);
            ViewBag.SupplierId = new SelectList(db.Suppliers.Where(s => s.Status == true).ToList(), "Id", "Name", goods.SupplierId);
            return View(goods);
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
