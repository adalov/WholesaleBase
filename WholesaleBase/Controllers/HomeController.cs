using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WholesaleBase.Models;

namespace WholesaleBase.Controllers
{
    public class HomeController : Controller
    {
        WholesaleContext db = new WholesaleContext();
        [Authorize]
        public ActionResult Index()
        {
            if (User.IsInRole("storekeeper"))
            {
                ApplicationUserManager UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);
                var WarehouseId = user.Claims.SingleOrDefault(c => c.ClaimType == "WarehouseId").ClaimValue;
                var warehouse = db.Warehouses.Find(Int32.Parse(WarehouseId));
                ViewBag.Warehouse = warehouse;
            }            
            return View();
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