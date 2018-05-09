using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WholesaleBase.Models;

namespace WholesaleBase.Controllers
{
    [Authorize(Roles = "manager")]
    public class CustomersController : Controller
    {
        private WholesaleContext db = new WholesaleContext();

        public ActionResult Index()
        {
            if (!Request.IsAjaxRequest())
                return View(db.Customers.ToList());
            else
                return PartialView("CustomersList", db.Customers.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Representative,PhoneNumber,Address")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.Status = true;
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Representative,PhoneNumber,Address,Status")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }
        public ActionResult RemoveOrRestore(int Id)
        {
            Customer customer = db.Customers.Find(Id);
            customer.Status = !customer.Status;
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
