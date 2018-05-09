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
    [Authorize(Roles = "manager")]
    public class ContractsController : Controller
    {
        private WholesaleContext db = new WholesaleContext();

        public ActionResult Index()
        {
            var contracts = db.Contracts.Include(c => c.Contractor).Include(c => c.Goods).Include(c => c.Warehouse).Include(c => c.TurnoverType).Include(c => c.Goods.Supplier);
            if (!Request.IsAjaxRequest())
                return View(contracts.ToList());
            else
                return PartialView("ContractsList", contracts.ToList());
        }

        public ActionResult Create()
        {
            List<TurnoverType> turnoverTypes = db.TurnoverTypes.Where(tt => tt.Name == "Приход" || tt.Name == "Расход").ToList();
            ViewBag.TurnoverTypeId = new SelectList(turnoverTypes, "Id", "Name");
            List<Warehouse> warehouses = db.Warehouses.Where(g => g.Status == true).ToList();
            ViewBag.WarehouseId = new SelectList(warehouses, "Id", "Name");
            List<Goods> goods = db.Goods.Where(tm => tm.Status == true).ToList();
            ViewBag.GoodsId = new SelectList(goods, "Id", "Name");
            List<TurnoverMember> turnoverMembers = db.TurnoverMembers.Where(g => g.Status == true).ToList();
            ViewBag.ContractorId = new SelectList(turnoverMembers, "Id", "Name");
            Contract model = new Contract();
            return View(model);
        }
        public ActionResult TurnoverMembersList(int id, int goodsId, int WarehouseId, int ContractorId, Contract model)
        {
            var turnoverTypeName = db.TurnoverTypes.Find(id).Name;           
            switch (turnoverTypeName)
            {
                case "Приход":
                    Goods goods = db.Goods.Find(goodsId);
                    List<Supplier> suppliers = db.Suppliers.Where(g => g.Status == true).ToList();
                    ViewBag.ContractorId = new SelectList(suppliers.Where(s => s.Id == goods.SupplierId), "Id", "Name");
                    break;
                case "Расход":
                    List<Customer> customers = db.Customers.Where(g => g.Status == true).ToList();
                    ViewBag.ContractorId = new SelectList(customers, "Id", "Name");
                    break;
                default: return new HttpStatusCodeResult(HttpStatusCode.BadRequest); 
            }
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ConclusionDate,GoodsId,GoodsQuantity,TurnoverPeriodicity,TurnoverTypeId,StartDate,ContractorId,WarehouseId")] Contract contract)
        {
            TurnoverType turnoverType = db.TurnoverTypes.Find(contract.TurnoverTypeId);
            if (turnoverType.Name == "Расход")
            {
                var goodsMovements = db.GoodsMovements.ToList();
                GoodsMovement goodsMovement = goodsMovements.FirstOrDefault(gm => gm.GoodsId == contract.GoodsId && gm.WarehouseId == contract.WarehouseId && gm.Date.Date == DateTime.Now.Date);
                var turnoverNotices = db.TurnoverNotices.Include(tn => tn.TurnoverNoticeStatus).Include(tn => tn.TurnoverType).ToList();
                int goodsQuantityInTurnoverNotices = turnoverNotices.Where(tn => tn.GoodsId == contract.GoodsId && tn.WarehouseId == contract.WarehouseId && tn.TurnoverNoticeStatus.Name == "В ожидании" && tn.TurnoverType.Name == "Расход").Select(tn => tn.GoodsQuantity).Sum();
                var balance = goodsMovement.GoodsBalance - goodsQuantityInTurnoverNotices; 
                if (contract.GoodsQuantity > balance)
                {
                    ModelState.AddModelError("GoodsQuantity", "На складе недостаточно товара, в наличии только " + balance);
                }
            }
            if (ModelState.IsValid)
            {
                contract.Status = true;
                db.Contracts.Add(contract);
                db.SaveChanges();
                if (contract.CheckToday())
                {                    
                    TurnoverNotice turnoverNotice = new TurnoverNotice() { Date = DateTime.Now, GoodsId = contract.GoodsId, GoodsQuantity = contract.GoodsQuantity, TurnoverMemberId = contract.ContractorId, TurnoverTypeId = contract.TurnoverTypeId, WarehouseId = contract.WarehouseId};
                    Goods goods = db.Goods.Find(turnoverNotice.GoodsId);
                    turnoverNotice.Price = turnoverType.Name == "Приход" ? goods.PurchasePrice : goods.SellingPrice;
                    TurnoverNoticeStatus turnoverNoticeStatus = db.TurnoverNoticeStatus.FirstOrDefault(tns => tns.Name == "В ожидании");
                    turnoverNotice.TurnoverNoticeStatusId = turnoverNoticeStatus.Id;
                    db.TurnoverNotices.Add(turnoverNotice);
                    db.SaveChanges();
                }                
                return RedirectToAction("Index");
            }
            List<TurnoverType> turnoverTypes = db.TurnoverTypes.Where(tt => tt.Name == "Приход" || tt.Name == "Расход").ToList();
            ViewBag.TurnoverTypeId = new SelectList(turnoverTypes, "Id", "Name");
            List<Warehouse> warehouses = db.Warehouses.Where(g => g.Status == true).ToList();
            ViewBag.WarehouseId = new SelectList(warehouses, "Id", "Name");
            List<Goods> goodsList = db.Goods.Where(tm => tm.Status == true).ToList();
            ViewBag.GoodsId = new SelectList(goodsList, "Id", "Name");
            return View(contract);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void CreateTurnoverNotice()
        {
            Contract contract = TempData["contract"] as Contract;
            if (contract.CheckToday())
            {
                TurnoverType turnoverType = null;
                decimal price = 0;
                if (contract.Contractor is Supplier)
                {
                    turnoverType = db.TurnoverTypes.FirstOrDefault(tt => tt.Name == "Приход");
                    price = contract.Goods.PurchasePrice;
                }
                else if (contract.Contractor is Customer)
                {
                    turnoverType = db.TurnoverTypes.FirstOrDefault(tt => tt.Name == "Расход");
                    price = contract.Goods.SellingPrice;
                }
                TurnoverNoticeStatus turnoverNoticeStatus = db.TurnoverNoticeStatus.FirstOrDefault(tns => tns.Name == "В ожидании");
                TurnoverNotice turnoverNotice = new TurnoverNotice { Date = DateTime.Now, Goods = contract.Goods, GoodsQuantity = contract.GoodsQuantity, TurnoverType = turnoverType, Price = price, TurnoverMember = contract.Contractor, TurnoverNoticeStatus = turnoverNoticeStatus, Warehouse = contract.Warehouse };
                TempData["turnoverNotice"] = turnoverNotice;
                RedirectToAction("CreateForContract", "TurnoverNotice");
            }
        }

        public ActionResult Delete(int id)
        {
            Contract contract = db.Contracts.Find(id);
            contract.Status = false;
            db.Entry(contract).State = EntityState.Modified;
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
