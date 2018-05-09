using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WholesaleBase.Models;

namespace WholesaleBase.Controllers
{
    public class InventoryResultsController : Controller
    {
        private WholesaleContext db = new WholesaleContext();
        [Authorize(Roles = "boss")]
        public ActionResult Index(int? SelectedWarehouseId, DateTime? MinDate, DateTime? MaxDate)
        {
            ViewBag.WarehousesList = db.Warehouses.Where(w => w.Status == true).ToList();
            ViewBag.Warehouse = SelectedWarehouseId == null ? null : db.Warehouses.Find(SelectedWarehouseId);
            ViewBag.MinDate = MinDate == null ? DateTime.Now.AddDays(-30) : MinDate;
            ViewBag.MaxDate = MaxDate == null ? DateTime.Now : MaxDate;
            var inventoryResults = db.InventoryResults.Include(i => i.Goods).Include(i => i.Warehouse).Include(i => i.Goods.MeasureUnit).Include(i => i.Goods.Supplier);
            if (!Request.IsAjaxRequest()) return View(inventoryResults.ToList());
            else return PartialView("InventoryResultsList", inventoryResults.ToList());
        }

        public PartialViewResult InventoryResultStatus(InventoryResult item)
        {
            return PartialView(item);
        }

        public ActionResult Confirm(int Id)
        {
            InventoryResult inventoryResult = db.InventoryResults.Find(Id);
            inventoryResult.Status = true;
            db.SaveChanges();
            var goodsMovements = db.GoodsMovements.ToList();
            GoodsMovement goodsMovement = goodsMovements.FirstOrDefault(gm => gm.Date.Date == DateTime.Now.Date && gm.GoodsId == inventoryResult.GoodsId && gm.WarehouseId == inventoryResult.WarehouseId);
            goodsMovement.GoodsBalance += inventoryResult.ActualGoodsBalance - inventoryResult.RegisteredGoodsBalance;
            db.Entry(goodsMovement).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("InventoryResultStatus", inventoryResult);
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
