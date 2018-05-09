using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WholesaleBase.Models
{
    public class WarehouseAccountView
    {
       public Warehouse Warehouse { get; set; }
       public ApplicationUser Account { get; set; }
    }
}