using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WholesaleBase.Models
{
    public class Goods
    {
        public int Id { get; set; }

        [Display(Name = "Название")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Поставщик")]
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        [Display(Name = "Ед.изм.")]
        public int MeasureUnitId { get; set; }
        public MeasureUnit MeasureUnit { get; set; }

        [Display(Name = "Цена закупки")]
        public decimal PurchasePrice { get; set; }

        [Display(Name = "Цена продажи")]
        public decimal SellingPrice { get; set; }
        public bool Status { get; set; }
        public virtual List<GoodsMovement> GoodsMovements { get; set; }

    }
}