using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WholesaleBase.Models;

namespace WholesaleBase.job
{
    public class Job : IJob, IDisposable
    {
        WholesaleContext db = new WholesaleContext();

        public Task Execute(IJobExecutionContext context)
        {

            //создание двежений товаров
            DateTime lastDate = DateTime.Now.AddDays(-1);
            var goodsList = db.Goods.Where(g => g.Status == true).Include(g => g.GoodsMovements).ToList();
            var warehouseList = db.Warehouses.Where(w => w.Status == true).ToList();
            var goodsMovementsList = db.GoodsMovements.ToList();
            foreach (var goods in goodsList)
                foreach (var warehouse in warehouseList)
                {
                    int goodsBalance = goods.GoodsMovements.Where(m => m.WarehouseId == warehouse.Id && m.Date.Date == lastDate.Date).Select(m => m.GoodsBalance).SingleOrDefault();
                    GoodsMovement newGoodsMovement = new GoodsMovement { Date = DateTime.Now, GoodsId = goods.Id, WarehouseId = warehouse.Id, GoodsArrival = 0, GoodsConsumption = 0, GoodsBalance = goodsBalance };
                    db.GoodsMovements.Add(newGoodsMovement);
                }
            db.SaveChanges();
            //создание уведомлений оборотов
            var contracts = db.Contracts.Where(c => c.Status == true).ToList();
            foreach (var contract in contracts)
            {
                if (contract.CheckToday())
                {
                    TurnoverType turnoverType = db.TurnoverTypes.Find(contract.TurnoverTypeId);
                    TurnoverNotice turnoverNotice = new TurnoverNotice() { Date = DateTime.Now, GoodsId = contract.GoodsId, GoodsQuantity = contract.GoodsQuantity, TurnoverMemberId = contract.ContractorId, TurnoverTypeId = contract.TurnoverTypeId, WarehouseId = contract.WarehouseId };
                    Goods goods = db.Goods.Find(turnoverNotice.GoodsId);
                    turnoverNotice.Price = turnoverType.Name == "Приход" ? goods.PurchasePrice : goods.SellingPrice;
                    TurnoverNoticeStatus turnoverNoticeStatus = db.TurnoverNoticeStatus.FirstOrDefault(tns => tns.Name == "В ожидании");
                    turnoverNotice.TurnoverNoticeStatusId = turnoverNoticeStatus.Id;
                    db.TurnoverNotices.Add(turnoverNotice);                    
                }
            }
            db.SaveChanges();
            return Task.CompletedTask;
        }
        public void Dispose()
        {                          
            db.Dispose();            
        }
    }
}