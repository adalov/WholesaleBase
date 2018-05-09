using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WholesaleBase.Models
{
    public class GoodsMovement
    {
        public int Id { get; set; }

        [Display(Name = "Товар")]
        public int GoodsId { get; set; }
        public Goods Goods { get; set; }

        [Display(Name = "Склад")]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Приход")]
        public int GoodsArrival { get; set; }

        [Display(Name = "Расход")]
        public int GoodsConsumption { get; set; }

        [Display(Name = "Остаток")]
        public int GoodsBalance { get; set; }
        public void AddTurnover(TurnoverType turnoverType, int goodsQuantity)
        {
            if (turnoverType.Name == "Приход")
            {
                GoodsArrival += goodsQuantity;
                GoodsBalance += goodsQuantity;
            }                
            else if (turnoverType.Name == "Расход")
            {
                GoodsConsumption += goodsQuantity;
                GoodsBalance -= goodsQuantity;
            }                
        } 
    }
}