using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WholesaleBase.Models;

namespace WholesaleBase.Controllers
{
    [Authorize(Roles = "manager")]
    public class SuppliersController : Controller
    {
        private WholesaleContext db = new WholesaleContext();

        public ActionResult Index()
        {
            if (!Request.IsAjaxRequest())
                return View(db.Suppliers.ToList());
            else
                return PartialView("SuppliersList", db.Suppliers.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Representative,PhoneNumber,Address")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.Status = true;
                db.Suppliers.Add(supplier);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(supplier);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Representative,PhoneNumber,Address,Status")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                db.Entry(supplier).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(supplier);
        }
        public ActionResult RemoveOrRestore(int Id)
        {
            Supplier supplier = db.Suppliers.Find(Id);
            supplier.Status = !supplier.Status;
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
