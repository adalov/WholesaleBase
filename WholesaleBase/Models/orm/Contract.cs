using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WholesaleBase.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Display(Name = "Дата подписания")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}", ApplyFormatInEditMode = true)]
        public DateTime ConclusionDate { get; set; }

        [Display(Name = "Товар")]
        public int GoodsId { get; set; }
        public Goods Goods { get; set; }

        [Display(Name = "Количество")]
        [Range(1, Int32.MaxValue, ErrorMessage = "Количество должно быть положительным числом")]
        public int GoodsQuantity { get; set; }

        [Display(Name = "Периодичность в днях")]
        [Range(1, 365)]
        public int TurnoverPeriodicity { get; set; }

        [Display(Name = "Начиная с")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Тип оборота")]
        public int TurnoverTypeId { get; set; }
        public TurnoverType TurnoverType { get; set; }

        [Display(Name = "Контрагент")]
        public int ContractorId { get; set; }
        public TurnoverMember Contractor { get; set; }

        [Display(Name = "Склад")]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public bool Status { get; set; }
        public bool CheckToday()
        {
            TimeSpan difference = DateTime.Now - StartDate;
            //разница должна быть кратна периодичности
            if ((difference.Days >= 0) && (difference.Days % TurnoverPeriodicity == 0))
                return true;
            return false;            
        }
    }
}