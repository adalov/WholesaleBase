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
    public class TurnoverNoticesController : Controller
    {
        private WholesaleContext db = new WholesaleContext();

        public ActionResult Index(int? SelectedWarehouseId, int? SelectedTurnoverTypeId, DateTime? MinDate, DateTime? MaxDate)
        {
            var turnoverNotices = db.TurnoverNotices.Include(t => t.Goods).Include(t => t.TurnoverMember).Include(t => t.TurnoverNoticeStatus).Include(t => t.Goods.MeasureUnit).Include(t => t.Goods.Supplier).Include(t => t.Warehouse).Include(t => t.TurnoverType);
            ViewBag.TurnoverTypesList = db.TurnoverTypes.Where(tt => tt.Name != "Внутреннее перемещение").ToList();
            ViewBag.TurnoverType = SelectedTurnoverTypeId == null ? null : db.TurnoverTypes.Find(SelectedTurnoverTypeId);
            if (User.IsInRole("storekeeper"))
            {
                ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);
                var WarehouseId = user.Claims.SingleOrDefault(c => c.ClaimType == "WarehouseId").ClaimValue;
                ViewBag.Warehouse = db.Warehouses.Find(Int32.Parse(WarehouseId));
                if (ViewBag.Warehouse.Status == false) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                ViewBag.MinDate = DateTime.Now;
                ViewBag.MaxDate = DateTime.Now;
            }
            else
            {
                ViewBag.WarehousesList = db.Warehouses.Where(w => w.Status == true).ToList();
                ViewBag.Warehouse = SelectedWarehouseId == null ? db.Warehouses.First(w => w.Status == true) : db.Warehouses.Find(SelectedWarehouseId);
                ViewBag.MinDate = MinDate == null ? DateTime.Now : MinDate;
                ViewBag.MaxDate = MaxDate == null ? DateTime.Now : MaxDate;
            }
                
            if (!Request.IsAjaxRequest()) return View(turnoverNotices.ToList());
            else return PartialView("TurnoverNoticesList", turnoverNotices.ToList());
        }
        [Authorize(Roles = "manager")]
        public ActionResult Create()
        {
            List<Goods> goods = db.Goods.Where(g => g.Status == true).ToList();
            ViewBag.GoodsId = new SelectList(goods, "Id", "Name");
            List<Supplier> suppliers = db.Suppliers.Where(g => g.Status == true).ToList();
            ViewBag.TurnoverMemberId = new SelectList(suppliers, "Id", "Name");
            ViewBag.TurnoverTypeId = new SelectList(db.TurnoverTypes.ToList(), "Id", "Name");
            List<Warehouse> warehouses = db.Warehouses.Where(g => g.Status == true).ToList();
            ViewBag.WarehouseId = new SelectList(warehouses, "Id", "Name");
            TurnoverNotice model = new TurnoverNotice();
            return View(model);
        }
        
        public ActionResult TurnoverMembersList(int id, int goodsId, int WarehouseId, int TurnoverMemberId, TurnoverNotice model)
        {
            var turnoverTypeName = db.TurnoverTypes.Find(id).Name;
            List<Warehouse> warehouses = db.Warehouses.Where(g => g.Status == true).ToList();
            ViewBag.WarehouseId = new SelectList(warehouses, "Id", "Name");
            switch (turnoverTypeName)
            {
                case "Приход":
                    Goods goods = db.Goods.Find(goodsId);
                    List<Supplier> suppliers = db.Suppliers.Where(g => g.Status == true).ToList();
                    ViewBag.TurnoverMemberId = new SelectList(suppliers.Where(s => s.Id == goods.SupplierId), "Id", "Name");                    
                    return PartialView(model);
                case "Расход":
                    List<Customer> customers = db.Customers.Where(g => g.Status == true).ToList();
                    ViewBag.TurnoverMemberId = new SelectList(customers, "Id", "Name");
                    return PartialView(model);
                case "Внутреннее перемещение":
                    ViewBag.TurnoverMemberId = new SelectList(warehouses, "Id", "Name");
                    return PartialView("WarehousesList",model);
                default: return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }
        public ActionResult Warehouse(int id, TurnoverNotice model)
        {
            ViewBag.WarehouseId = new SelectList(db.Warehouses.Where(w => w.Id != id).ToList(), "Id", "Name");
            return PartialView(model);
        }
        [Authorize(Roles = "manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date,GoodsId,GoodsQuantity,TurnoverTypeId,TurnoverMemberId,WarehouseId")] TurnoverNotice turnoverNotice)
        {
            TurnoverType turnoverType = db.TurnoverTypes.Find(turnoverNotice.TurnoverTypeId);
            if (turnoverType.Name == "Расход")
            {
                var goodsMovements = db.GoodsMovements.ToList();
                GoodsMovement goodsMovement = goodsMovements.FirstOrDefault(gm => gm.GoodsId == turnoverNotice.GoodsId && gm.WarehouseId == turnoverNotice.WarehouseId && gm.Date.Date == DateTime.Now.Date);
                if (turnoverNotice.GoodsQuantity > goodsMovement.GoodsBalance)
                {
                    ModelState.AddModelError("GoodsQuantity", "На складе недостаточно товара, в наличии только " + goodsMovement.GoodsBalance);
                }
            }
            
            if (ModelState.IsValid)
            {
                TurnoverNoticeStatus turnoverNoticeStatus = db.TurnoverNoticeStatus.FirstOrDefault(tns => tns.Name == "В ожидании");
                turnoverNotice.TurnoverNoticeStatusId = turnoverNoticeStatus.Id;
                if (turnoverType.Name == "Внутреннее перемещение")
                {
                    TurnoverType turnoverType1 = db.TurnoverTypes.FirstOrDefault(tt => tt.Name == "Приход");
                    turnoverNotice.TurnoverTypeId = turnoverType1.Id;
                    turnoverNotice.Price = 0;
                    TurnoverType turnoverType2 = db.TurnoverTypes.FirstOrDefault(tt => tt.Name == "Расход");
                    TurnoverNotice turnoverNotice2 = new TurnoverNotice() { Date = turnoverNotice.Date, TurnoverTypeId = turnoverType2.Id, GoodsId = turnoverNotice.GoodsId, GoodsQuantity = turnoverNotice.GoodsQuantity, Price = 0, TurnoverMemberId = turnoverNotice.WarehouseId, WarehouseId = turnoverNotice.TurnoverMemberId, TurnoverNoticeStatusId = turnoverNotice.TurnoverNoticeStatusId };
                    db.TurnoverNotices.Add(turnoverNotice);
                    db.TurnoverNotices.Add(turnoverNotice2);
                    db.SaveChanges();
                }
                else
                {
                    Goods goods = db.Goods.Find(turnoverNotice.GoodsId);
                    turnoverNotice.Price = turnoverType.Name == "Приход" ? goods.PurchasePrice : goods.SellingPrice;
                    db.TurnoverNotices.Add(turnoverNotice);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
                              
            }
            List<Goods> goodsList = db.Goods.Where(g => g.Status == true).ToList();
            ViewBag.GoodsId = new SelectList(goodsList, "Id", "Name");
            List<Supplier> suppliers = db.Suppliers.Where(g => g.Status == true).ToList();
            ViewBag.TurnoverMemberId = new SelectList(suppliers, "Id", "Name");
            ViewBag.TurnoverTypeId = new SelectList(db.TurnoverTypes.ToList(), "Id", "Name", 1);
            List<Warehouse> warehouses = db.Warehouses.Where(g => g.Status == true).ToList();
            ViewBag.WarehouseId = new SelectList(warehouses, "Id", "Name");
            return View(turnoverNotice);
        }
        public PartialViewResult TurnoverNoticeStatus(TurnoverNotice item) 
        {
            item.TurnoverNoticeStatus = db.TurnoverNoticeStatus.Find(item.TurnoverNoticeStatusId);
            return PartialView(item);
        }

        public ActionResult Confirm(int Id)
        {
            TurnoverNotice turnoverNotice = db.TurnoverNotices.Find(Id);
            TurnoverNoticeStatus turnoverNoticeStatus = db.TurnoverNoticeStatus.FirstOrDefault(tns => tns.Name == "Подтверждено");
            turnoverNotice.TurnoverNoticeStatus = turnoverNoticeStatus;
            db.SaveChanges();

            List<GoodsMovement> goodsMovements = db.GoodsMovements.ToList();
            GoodsMovement goodsMovement = goodsMovements.FirstOrDefault(gm => gm.GoodsId == turnoverNotice.GoodsId && gm.WarehouseId == turnoverNotice.WarehouseId && gm.Date.Date == turnoverNotice.Date.Date);
            TurnoverType turnoverType = db.TurnoverTypes.Find(turnoverNotice.TurnoverTypeId);
            goodsMovement.AddTurnover(turnoverType, turnoverNotice.GoodsQuantity);
            db.Entry(goodsMovement).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("TurnoverNoticeStatus", turnoverNotice);
        }

        public ActionResult Cancel(int Id)
        {
            TurnoverNotice turnoverNotice = db.TurnoverNotices.Find(Id);
            TurnoverNoticeStatus turnoverNoticeStatus = db.TurnoverNoticeStatus.FirstOrDefault(tns => tns.Name == "Отменено");
            turnoverNotice.TurnoverNoticeStatus = turnoverNoticeStatus;
            db.SaveChanges();
            return RedirectToAction("TurnoverNoticeStatus", turnoverNotice);
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
