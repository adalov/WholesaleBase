using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WholesaleBase.Models;

namespace WholesaleBase.Controllers
{

    public class GoodsMovementsController : Controller
    {
        private WholesaleContext db = new WholesaleContext();

        [Authorize(Roles = "boss, manager")]
        public ActionResult Index(int? SelectedWarehouseId, DateTime? MinDate, DateTime? MaxDate)
        {
            ViewBag.WarehousesList = db.Warehouses.Where(w => w.Status == true).ToList();
            ViewBag.MinDate = MinDate == null ? DateTime.Now.AddDays(-30) : MinDate;
            ViewBag.MaxDate = MaxDate == null ? DateTime.Now : MaxDate;
            
            ViewBag.Warehouse = SelectedWarehouseId == null ? null : db.Warehouses.Find(SelectedWarehouseId);

            var goodsList = db.Goods.Include(g => g.MeasureUnit).Include(g => g.Supplier).Include(g => g.GoodsMovements).Where(g => g.Status == true).ToList();

            if (!Request.IsAjaxRequest()) return View(goodsList);
            else return PartialView("GoodsMovementsList", goodsList);
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
