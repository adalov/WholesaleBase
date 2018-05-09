using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WholesaleBase.Models
{
    public class InventorizationViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Поставщик")]
        public Supplier Supplier { get; set; }

        [Display(Name = "Ед.изм.")]
        public MeasureUnit MeasureUnit { get; set; }

        public bool Status { get; set; }

    }
}