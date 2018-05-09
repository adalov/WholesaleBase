using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WholesaleBase.Models
{
    public class TurnoverNotice
    {
        public int Id { get; set; }

        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Товар")]
        public int GoodsId { get; set; }
        public Goods Goods { get; set; }

        [Display(Name = "Количество")]
        [Range(1,Int32.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int GoodsQuantity { get; set; }

        [Display(Name = "Вид оборота")]
        public int TurnoverTypeId { get; set; }
        public TurnoverType TurnoverType { get; set; }

        [Display(Name = "Участник оборота")]
        public int TurnoverMemberId { get; set; }
        public TurnoverMember TurnoverMember { get; set; }

        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Display(Name = "Склад")]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        [Display(Name = "Статус")]
        public int TurnoverNoticeStatusId { get; set; }
        public TurnoverNoticeStatus TurnoverNoticeStatus { get; set; }
    }
}