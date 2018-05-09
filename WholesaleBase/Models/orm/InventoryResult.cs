using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WholesaleBase.Models
{
    public class InventoryResult
    {
        public int Id { get; set; }

        [Display(Name = "Дата инвентаризации")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}", ApplyFormatInEditMode = true)]
        public DateTime InventoryDate { get; set; }

        [Display(Name = "Склад")]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        [Display(Name = "Товар")]
        public int GoodsId { get; set; }
        public Goods Goods { get; set; }

        [Display(Name = "Фактический остаток")]
        public int ActualGoodsBalance { get; set; }

        [Display(Name = "Учтённый остаток")]
        public int RegisteredGoodsBalance { get; set; }

        public bool Status { get; set; }
    }
}