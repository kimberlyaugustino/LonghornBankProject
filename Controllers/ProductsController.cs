using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using LonghornBankProject.Models;

namespace LonghornBankProject.Controllers
{
    public enum AccountType
    {
        Savings,
        Checking
    }
    public class ProductsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Products
        [Authorize]
        public ActionResult Index()
        {
            string customerid = User.Identity.GetUserId();

            var query = from p in db.Accounts
                        select p;
            query = query.Where(p => p.Customer.Id == customerid);
            query = query.OrderBy(p => p.ProductID);

            List<Product> UserProducts = query.ToList();
            return View(UserProducts);
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Accounts.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        //Authorize
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //Authorize
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AccountType SelectedProduct, decimal balance)
        {
            if (ModelState.IsValid)
            {
                AppUser customer = db.Users.Find(User.Identity.GetUserId());
                Product product = new Product(SelectedProduct, customer, balance);
                db.Accounts.Add(product);
                if (balance < 5000)
                {
                    Transaction transaction = new Transaction();
                    transaction.Amount = balance;
                    transaction.Date = DateTime.Now;
                    transaction.TransactionType = "Deposit";
                    transaction.Description = "Initial Deposit";
                    transaction.Account = product;
                    db.Transactions.Add(transaction);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Accounts.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AccountName")] Product product, Int32 ProductID)
        {
            if (ModelState.IsValid)
            {
                Product productToChange = db.Accounts.Find(ProductID);
                productToChange.AccountName = product.AccountName;
                db.Entry(productToChange).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Accounts.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Accounts.Find(id);
            db.Accounts.Remove(product);
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

        public ActionResult CreateIRA()
        {
            Product product = db.Accounts.FirstOrDefault(x => x.AccountType == "IRA");
            if (product == null)
            {
                return View("Error", new string[] { "Each account can only have one IRA account." });
            }
            return View();
        }
    }
}
